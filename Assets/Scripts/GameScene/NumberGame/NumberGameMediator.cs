using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Entry;
using StaticScripts;

/// <summary>
/// 遊戲場景
/// </summary>
namespace GameScene
{
    /// <summary>
    /// 數字遊戲2048
    /// </summary>
    public class NumberGameMediator : MonoBehaviour
    {
        GameSceneManager gameSceneManager;//場景管理
        ObjectPool objectPool;//物件池
        AudioManager audioManager;//音效管理
        Language language;//語言翻譯

        [Header("預設資料")]
        [Tooltip("初始方格位置")] readonly Vector2 initGridPos = new Vector2(-365, 365);
        [Tooltip("方格大小列表")] readonly int[] squareSizeList = { 300, 220, 175, 148, 113 };
        [Tooltip("方格間距列表")] readonly int[] spacingList = { 25, 23, 18, 12, 7 };
        [Tooltip("預設數字")] readonly int initNumber = 2;
        [Tooltip("獲勝數字")] readonly int winNumber = 2048;
        [Tooltip("方塊移動速度")] readonly float moveSpeed = 6000;
        [Tooltip("移動判定距離")] readonly float judgeMoveDistance = 75;

        [Header("元件")]
        [Tooltip("方格樣本")][SerializeField] RectTransform gridSample;
        [Tooltip("方格背景容器")][SerializeField] RectTransform gridContent;
        [Tooltip("產生方塊父物件")][SerializeField] RectTransform CreateSquare_Obj;
        [Tooltip("方塊樣本")][SerializeField] GameObject squareSample;
        [Tooltip("遊戲結束物件")][SerializeField] GameObject gameOver_Obj;
        [Tooltip("遊戲結束_重新開始按鈕")][SerializeField] Button gameOverReStart_Btn;
        [Tooltip("遊戲結束_文字")][SerializeField] Text gameOver_Txt;
        [Tooltip("重新開始按鈕")][SerializeField] Button ReStart_Btn;
        [Tooltip("重新開始文字")] [SerializeField] Text gameOverReStart_Txt;
        [Tooltip("選擇關卡按鈕")][SerializeField] Button SelectLevel_Btn;
        [Tooltip("回大廳按鈕")][SerializeField] Button Home_Btn;

        [Header("資料")]
        [Tooltip("方格顏色")][SerializeField] Color[] squareColors;
        [Tooltip("方格行列數量")] int grideSize;
        [Tooltip("方格總數量")] int allSquareCount;
        [Tooltip("方格大小")] [SerializeField] int squareSize;
        [Tooltip("方格間距")] [SerializeField] int spacing;
        [Tooltip("方格背景位置List")] [SerializeField] List<Vector3> gridBgPosList = new List<Vector3>();
        [Tooltip("方塊資料")] [SerializeField] SquareData[] squareDatas;
        [Tooltip("紀錄移動")] [SerializeField] List<MoveSquare> MoveSquareList = new List<MoveSquare>();
       
        [Header("物件池")]
        [Tooltip("物件池(物件名稱,編號)")] Dictionary<string, int> objectPoolDic = new Dictionary<string, int>();
        [Tooltip("物件池資料")] objPoolData[] objPoolDatas;

        [Header("分數")]
        [Tooltip("用戶分數文字")] [SerializeField] Text userScore_Txt;
        [Tooltip("分數文字")] [SerializeField] Text ScoreTitle_Txt;
        [Tooltip("用戶分數")] int userScore;

        [Header("控制")]
        [Tooltip("控制Bool")] [SerializeField] bool isControl;
        [Tooltip("起始觸碰點")] Vector3 initTouchPos;
        [Tooltip("是否正在移動")] [SerializeField] bool isMoving;

        [Header("動畫")]
        [Tooltip("動畫名稱")] readonly string[] animatorName = 
        {
            "Merge_Tr",//方塊合併
        };
        [Tooltip("動畫HashID")] int[] animatorHashId;

        private void Awake()
        {
            gameSceneManager = GameSceneManager.Instance;//場景管理
            objectPool = ObjectPool.Instance;//物件池
            audioManager = AudioManager.Instance;//音效管理
            language = Language.Instance;//語言翻譯

            //動畫HashID
            animatorHashId = new int[animatorName.Length];
            for (int i = 0; i < animatorName.Length; i++)
            {
                animatorHashId[i] = Animator.StringToHash(animatorName[i]);
            }

            //翻譯
            gameOverReStart_Txt.text = language.GetLanguage("ReStart");//重新開始
            ScoreTitle_Txt.text = language.GetLanguage("Score");//分數

            CreateObjectPool();//創建物件池
            EventListener();//事件聆聽
        }

        /// <summary>
        /// 事件聆聽
        /// </summary>
        void EventListener()
        {
            //遊戲結束_重新開始按鈕
            gameOverReStart_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
                OnReStare();//重新遊戲
            });
            //回大廳按鈕
            Home_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
                objectPool.OnCleanPool(objectPoolDic);//清理物件池
                gameSceneManager.OnLoadScene("Lobby");
            });
            //選擇關卡按鈕
            SelectLevel_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
                objectPool.OnCleanPool(objectPoolDic);//清理物件池
                gameSceneManager.OnLoadScene("GameScene");
            });
            //重新開始按鈕
            ReStart_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
                OnReStare();//重新遊戲
            });
        }

        private void Start()
        {
            CreateGridBgAndInitSquare();//創建方格背景與初始方塊
        }

        private void Update()
        {
            OnInputControl();//輸入控制
            SquareMove();//方塊移動
        }

        /// <summary>
        /// 方塊移動
        /// </summary>
        void SquareMove()
        {
            if (isMoving)
            {
                for (int i = 0; i < MoveSquareList.Count; i++)
                {
                    Vector3 nowPos = MoveSquareList[i].obj.transform.position;//目前位置
                    Vector3 targetPos = gridBgPosList[MoveSquareList[i].posIndex];//目標位置
                    MoveSquareList[i].obj.transform.position = Vector3.MoveTowards(nowPos, targetPos, moveSpeed * Time.deltaTime);
                }
            }
        }

        /// <summary>
        /// 輸入控制
        /// </summary>
        void OnInputControl()
        {
#if UNITY_EDITOR
            //滑鼠控制
            if (Input.GetMouseButtonDown(0)) initTouchPos = Input.mousePosition;
            if (Input.GetMouseButtonUp(0)) initTouchPos = Vector3.zero;
            if (isControl && !isMoving && initTouchPos != Vector3.zero)
            {
                if (Input.GetMouseButton(0))
                {
                    Vector3 movePos = Input.mousePosition;
                    //判斷手指移動方向
                    JudgeFingerDirection(movePos);
                }
            }
#elif UNITY_ANDROID
            if (Input.touchCount == 1)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began) initTouchPos = Input.mousePosition;
                if (Input.GetTouch(0).phase == TouchPhase.Canceled) initTouchPos = Vector3.zero;
                if (isControl && !isMoving && initTouchPos != Vector3.zero)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Moved)
                    {
                        Vector3 movePos = Input.GetTouch(Input.GetTouch(0).fingerId).position;
                        //判斷手指移動方向
                        JudgeFingerDirection(movePos);
                    }
                }
            }
#endif
        }

        /// <summary>
        /// 判斷手指移動方向
        /// </summary>
        /// <param name="movePos"></param>
        void JudgeFingerDirection(Vector3 movePos)
        {
            float distanceX = movePos.x - initTouchPos.x;//移動距離X
            float distanceY = movePos.y - initTouchPos.y;//移動距離Y
            if (movePos.x > initTouchPos.x && Mathf.Abs(distanceX) > judgeMoveDistance && Mathf.Abs(distanceY) < judgeMoveDistance)//向右
            {
                SquareAction(0);//方格行動
            }
            else if (movePos.x < initTouchPos.x && Mathf.Abs(distanceX) > judgeMoveDistance && Mathf.Abs(distanceY) < judgeMoveDistance)//向左
            {
                SquareAction(1);//方格行動
            }
            else if (movePos.y > initTouchPos.y && Mathf.Abs(distanceY) > judgeMoveDistance && Mathf.Abs(distanceX) < judgeMoveDistance)//向上
            {
                SquareAction(2);//方格行動
            }
            else if (movePos.y < initTouchPos.y && Mathf.Abs(distanceY) > judgeMoveDistance && Mathf.Abs(distanceX) < judgeMoveDistance)//向下
            {
                SquareAction(3);//方格行動
            }
        }

        /// <summary>
        /// 創建物件池
        /// </summary>
        void CreateObjectPool()
        {
            //設定物件資料
            objPoolDatas = new objPoolData[]
            {
                new objPoolData(){ objName = "girdBg", obj = gridSample.gameObject, parentObj = gridContent },//方格
                new objPoolData(){ objName = "square", obj = squareSample, parentObj = CreateSquare_Obj },//方塊
            };

            //產生初始物件
            int num;
            for (int i = 0; i < objPoolDatas.Length; i++)
            {
                num = objectPool.OnCreateAndRecordObject(objPoolDatas[i].obj, objPoolDatas[i].parentObj);
                objectPoolDic.Add(objPoolDatas[i].objName, num);
            }
        }

        /// <summary>
        /// 設定方格
        /// </summary>
        /// <param name="size">方格行列數量</param>
        /// <param name="level">選擇的關卡大小</param>
        public void SetGride(int size, int level)
        {
            grideSize = size;//方格行列數量
            allSquareCount = grideSize * grideSize;//方格總數量
            squareSize = squareSizeList[level];//方格大小
            spacing = spacingList[level];//方格間距

            //方塊資料
            squareDatas = new SquareData[allSquareCount];
            for (int i = 0; i < squareDatas.Length; i++)
            {
                squareDatas[i] = new SquareData();
            }
        }

        /// <summary>
        /// 重新遊戲
        /// </summary>
        void OnReStare()
        {
            //清理資料
            for (int i = 0; i < squareDatas.Length; i++)
            {
                CleanSquareData(i);//清理方塊資料
            }
            //清理物件池
            for (int i = 0; i < objPoolDatas.Length; i++)
            {
                int serchId = objectPool.SerchObjId(objPoolDatas[i].objName, objectPoolDic);//搜索物件池ID
                objectPool.OnDestroyObject(serchId);//物件池物件刪除
            }

            CreateGridBgAndInitSquare();//創建方格背景與初始方塊
        }

        /// <summary>
        /// 創建方格背景與初始方塊
        /// </summary>
        void CreateGridBgAndInitSquare()
        {
            gridSample.gameObject.SetActive(false);//方格背景樣本
            squareSample.SetActive(false);//方塊樣本
            gameOver_Obj.SetActive(false);//遊戲結束物件

            //防呆
            if (gridSample == null || gridContent == null) return;

            //創建方格背景
            gridBgPosList = new List<Vector3>();
            for (int i = 0; i < grideSize; i++)
            {
                for (int j = 0; j < grideSize; j++)
                {
                    //物件池激活物件
                    int serchId = objectPool.SerchObjId(objPoolDatas[0].objName, objectPoolDic);//搜索物件池ID
                    RectTransform obj = objectPool.OnActiveObject(serchId, objPoolDatas[0].parentObj).GetComponent<RectTransform>();
                    
                    //預設方塊大小做位置偏移
                    float initSize = squareSizeList[1];//預設方塊大小
                    float size = (initSize - squareSize) / 2;
                    obj.sizeDelta = new Vector2(squareSize, squareSize);
                    obj.anchoredPosition = new Vector2((initGridPos.x - size) + ((squareSize + spacing) * j),
                                                        (initGridPos.y + size) - ((squareSize + spacing) * i));
                    gridBgPosList.Add(obj.position);
                }
            }

            //創建初始方塊
            for (int i = 0; i < 2; i++)
            {
                CreateSquare();//創建方塊
            }

            //設定分數
            userScore = 0;
            SetScore(userScore);

            //可操作
            isControl = true;
        }

        /// <summary>
        /// 創建方塊
        /// </summary>
        void CreateSquare()
        {
            int count = 0;//防呆
            bool isCreate = false;
            while (!isCreate)
            {
                //亂數位置
                List<int> emptyList = new List<int>();
                for (int i = 0; i < squareDatas.Length; i++)
                {
                    if (squareDatas[i].number == 0)
                    {
                        emptyList.Add(i);
                    }
                }
                
                //防呆
                if (emptyList.Count == 0)
                {
                    SetGameResult(false);//設定遊戲結果
                    return;
                }

                int posIndex = Random.Range(0, emptyList.Count);
                //該位置為空
                if (squareDatas[emptyList[posIndex]].number == 0)
                {
                    isCreate = true;
                    //物件池激活物件
                    int serchId = objectPool.SerchObjId(objPoolDatas[1].objName, objectPoolDic);//搜索物件池ID_方塊ID
                    RectTransform square = objectPool.OnActiveObject(serchId, objPoolDatas[1].parentObj).GetComponent<RectTransform>();
                    square.sizeDelta = new Vector2(squareSize, squareSize);
                    square.position = gridBgPosList[emptyList[posIndex]];

                    //設定數字
                    squareDatas[emptyList[posIndex]].imageObj = square.transform.GetComponentInChildren<Image>();
                    squareDatas[emptyList[posIndex]].imageObj.color = squareColors[0];
                    squareDatas[emptyList[posIndex]].textObj = square.transform.GetComponentInChildren<Text>();
                    squareDatas[emptyList[posIndex]].textObj.text = initNumber.ToString();
                    squareDatas[emptyList[posIndex]].animator = square.transform.GetComponentInChildren<Animator>();
                    squareDatas[emptyList[posIndex]].number = initNumber;//預設數
                    squareDatas[emptyList[posIndex]].obj = square;
                }

                if (emptyList.Count == 1) JudgeIsFial();//判斷是否失敗

                //防呆
                count++;
                if (count > 2)
                {
                    Debug.LogError("出錯了");
                    isCreate = true;
                }
            }
        }

        /// <summary>
        /// 設定分數
        /// </summary>
        /// <param name="addScore">增加分數</param>
        void SetScore(int addScore)
        {
            userScore += addScore;
            userScore_Txt.text = userScore.ToString();
        }

        /// <summary>
        /// 判斷是否失敗
        /// </summary>
        void JudgeIsFial()
        {
            for (int i = 0; i < squareDatas.Length; i++)
            {
                //有數字
                if (squareDatas[i].number > 0)
                {
                    //判斷右側(i < 總數量 && 非最右側 && 數字相同)
                    if (i + 1 < allSquareCount - 1 && (i + 1) % grideSize != 0 && squareDatas[i + 1].number == squareDatas[i].number) return;
                    //判斷左側(i > 0 && 非最左側 && 數字相同)
                    if (i - 1 > 0 && (i + grideSize) % grideSize != 0 && squareDatas[i - 1].number == squareDatas[i].number) return;
                    //判斷上側(非最上側 && 數字相同)
                    if (i >= grideSize && squareDatas[i - grideSize].number == squareDatas[i].number) return;
                    //判斷下側(非最下側 && 數字相同)
                    if (i < allSquareCount - grideSize && squareDatas[i + grideSize].number == squareDatas[i].number) return;
                }
            }

            SetGameResult(false);//設定遊戲結果失敗
        }

        /// <summary>
        /// 設定遊戲結果
        /// </summary>
        /// <param name="isWin">是否獲勝</param>
        void SetGameResult(bool isWin)
        {
            //播放遊戲結束音效
            audioManager.OnPlayGameOverSound(isWin);

            isControl = false;//不可操作
            gameOver_Obj.SetActive(true);
            gameOver_Txt.text = isWin == true ? "Win" : "Game Over";
        }

        /// <summary>
        /// 方格行動
        /// </summary>
        /// <param name="dir">滑動方向(0=右,1=左,2=上,3=下)</param>
        void SquareAction(int dir)
        {
            isMoving = true;
            switch (dir)
            {
                case 0://向右
                    for (int i = allSquareCount - 1; i >= 0; i--)
                    {
                        //有數字 && 非最右側
                        if (squareDatas[i].number != 0 && (i + 1) % grideSize != 0)
                        {
                            int zorePos = -1;//欲交換數字=0
                            int posIndex;//交換位置index
                            for (posIndex = 1; posIndex < grideSize; posIndex++)
                            {
                                int ComparisonIndex = i + posIndex;//比對方塊編號

                                //非最後一個 && 比對物件非最左側
                                if (ComparisonIndex > allSquareCount - 1 || ComparisonIndex % grideSize == 0) break;

                                if (squareDatas[i].number != squareDatas[ComparisonIndex].number && squareDatas[ComparisonIndex].number != 0)//數字不相同
                                {
                                    zorePos = -1;//重製
                                    posIndex = ComparisonIndex - 1;
                                    OnNumberNotSame(i, posIndex);//數字不相同
                                    break;
                                }
                                else if (squareDatas[i].number == squareDatas[ComparisonIndex].number && !squareDatas[ComparisonIndex].isMerge)//數字相同
                                {
                                    zorePos = -1;//重製
                                    posIndex = ComparisonIndex;
                                    OnNumberSame(i, posIndex);//數字相同
                                    break;
                                }
                                else if (squareDatas[ComparisonIndex].number == 0 && (ComparisonIndex) % grideSize != 0)//欲合併數字=0
                                {
                                    //紀錄空格位置
                                    zorePos = ComparisonIndex;
                                }

                                //比對編號已在最右側
                                if ((ComparisonIndex) % grideSize == 0) break;
                            }

                            //向空格處移動
                            if (zorePos != -1)
                            {
                                //加入移動List
                                MoveSquareList.Add(new MoveSquare() { obj = squareDatas[i].obj, posIndex = zorePos });
                                //交換方塊資料
                                ChangeSquareDate(i, zorePos, squareDatas[i].number);
                            }
                        }
                    }
                    break;
                case 1://向左
                    for (int i = 1; i < allSquareCount; i++)
                    {
                        //有數字 && 非最左側
                        if (squareDatas[i].number != 0 && (i + grideSize) % grideSize != 0)
                        {
                            int zorePos = 0;//欲交換數字=0
                            int posIndex;//交換位置index
                            for (posIndex = 1; posIndex < grideSize; posIndex++)
                            {
                                int ComparisonIndex = i - posIndex;//比對方塊編號

                                //非第一個 && 比對物件非最右側
                                if (ComparisonIndex < 0 && (ComparisonIndex + 1) % grideSize == 0) break;

                                if (squareDatas[i].number != squareDatas[ComparisonIndex].number && squareDatas[ComparisonIndex].number != 0)//數字不相同
                                {
                                    zorePos = -1;//重製
                                    posIndex = ComparisonIndex + 1;
                                    OnNumberNotSame(i, posIndex);//數字不相同
                                    break;
                                }
                                else if (squareDatas[i].number == squareDatas[ComparisonIndex].number && !squareDatas[ComparisonIndex].isMerge)//數字相同
                                {
                                    zorePos = -1;//重製
                                    posIndex = ComparisonIndex;
                                    OnNumberSame(i, posIndex);//數字相同
                                    break;
                                }
                                else if (squareDatas[ComparisonIndex].number == 0)//欲合併數字=0
                                {
                                    //紀錄空格位置
                                    zorePos = ComparisonIndex;
                                }

                                //比對編號已在最左側
                                if (ComparisonIndex % grideSize == 0) break;
                            }

                            //向空格處移動
                            if (zorePos != -1)
                            {
                                //加入移動List
                                MoveSquareList.Add(new MoveSquare() { obj = squareDatas[i].obj, posIndex = zorePos });
                                //交換方塊資料
                                ChangeSquareDate(i, zorePos, squareDatas[i].number);
                            }
                        }
                    }
                    break;
                case 2://向上
                    for (int i = grideSize; i < allSquareCount; i++)
                    {
                        //有數字
                        if (squareDatas[i].number != 0)
                        {
                            int zorePos = -1;//欲交換數字=0
                            int posIndex;//交換位置index
                            for (posIndex = 1; posIndex < grideSize; posIndex++)
                            {
                                int ComparisonIndex = i - (grideSize * posIndex);//比對方塊編號
                                if (squareDatas[i].number != squareDatas[ComparisonIndex].number && squareDatas[ComparisonIndex].number != 0)//數字不相同
                                {
                                    zorePos = -1;//重製
                                    posIndex = ComparisonIndex + grideSize;
                                    OnNumberNotSame(i, posIndex);//數字不相同
                                    break;
                                }
                                else if (squareDatas[i].number == squareDatas[ComparisonIndex].number && !squareDatas[ComparisonIndex].isMerge)//數字相同
                                {
                                    zorePos = -1;//重製
                                    posIndex = ComparisonIndex;
                                    OnNumberSame(i, posIndex);//數字相同
                                    break;
                                }
                                else if (squareDatas[ComparisonIndex].number == 0)//欲合併數字=0
                                {
                                    //紀錄空格位置
                                    zorePos = ComparisonIndex;
                                }

                                //比對編號已在最上側
                                if (ComparisonIndex < grideSize) break;
                            }

                            //向空格處移動
                            if (zorePos != -1)
                            {
                                //加入移動List
                                MoveSquareList.Add(new MoveSquare() { obj = squareDatas[i].obj, posIndex = zorePos });
                                //交換方塊資料
                                ChangeSquareDate(i, zorePos, squareDatas[i].number);
                            }
                        }
                    }
                    break;
                case 3://向下
                    for (int i = allSquareCount - 1 - grideSize; i >= 0; i--)
                    {
                        //有數字
                        if (squareDatas[i].number != 0)
                        {
                            int zorePos = -1;//欲交換數字=0
                            int posIndex;//交換位置index
                            for (posIndex = 1; posIndex < grideSize; posIndex++)
                            {
                                int ComparisonIndex = i + (grideSize * posIndex);//比對方塊編號
                                if (squareDatas[i].number != squareDatas[ComparisonIndex].number && squareDatas[ComparisonIndex].number != 0)//數字不相同
                                {
                                    zorePos = -1;//重製
                                    posIndex = ComparisonIndex - grideSize;
                                    OnNumberNotSame(i, posIndex);//數字不相同
                                    break;
                                }
                                else if (squareDatas[i].number == squareDatas[ComparisonIndex].number && !squareDatas[ComparisonIndex].isMerge)//數字相同
                                {
                                    zorePos = -1;//重製
                                    posIndex = ComparisonIndex;
                                    OnNumberSame(i, posIndex);//數字相同
                                    break;
                                }
                                else if (squareDatas[ComparisonIndex].number == 0)//欲合併數字=0
                                {
                                    //紀錄空格位置
                                    zorePos = ComparisonIndex;
                                }

                                //比對編號已在最下側
                                if (ComparisonIndex > allSquareCount - 1 - grideSize) break;
                            }

                            //向空格處移動
                            if (zorePos != -1)
                            {
                                //加入移動List
                                MoveSquareList.Add(new MoveSquare() { obj = squareDatas[i].obj, posIndex = zorePos });
                                //交換方塊資料
                                ChangeSquareDate(i, zorePos, squareDatas[i].number);
                            }
                        }
                    }
                    break;
            }

            //方塊移動
            if (MoveSquareList.Count > 0)
            {
                isMoving = true;
                StartCoroutine(ISquareMoveResult());//方塊移動
            }
            else isMoving = false;

            //重製方塊資料
            for (int i = 0; i < squareDatas.Length; i++)
            {
                squareDatas[i].isMerge = false;
            }

            initTouchPos = Vector3.zero;//重製觸碰位置
        }

        /// <summary>
        /// 數字不相同
        /// </summary>
        /// <param name="index">原方塊index</param>
        /// <param name="posIndex">新方塊index</param>
        void OnNumberNotSame(int index, int posIndex)
        {
            //位置不同
            if (index != posIndex)
            {
                //加入移動List
                MoveSquareList.Add(new MoveSquare() { obj = squareDatas[index].obj, posIndex = posIndex });
                //交換方塊資料
                ChangeSquareDate(index, posIndex, squareDatas[index].number);
            }
        }

        /// <summary>
        /// 數字相同
        /// </summary>
        /// <param name="index">原方塊index</param>
        /// <param name="posIndex">新方塊index</param>
        void OnNumberSame(int index, int posIndex)
        {
            squareDatas[posIndex].isMerge = true;
            //設定分數
            SetScore(squareDatas[posIndex].number * 2);
            //加入移動List
            MoveSquareList.Add(new MoveSquare() { obj = squareDatas[index].obj, posIndex = posIndex, destroyObj = squareDatas[posIndex].obj });
            //交換方塊資料
            ChangeSquareDate(index, posIndex, squareDatas[posIndex].number * 2);
        }

        /// <summary>
        /// 交換方塊資料
        /// </summary>
        /// <param name="oldIndex">原方塊index</param>
        /// <param name="newIndex">新方塊index</param>
        /// <param name="number">顯示數字</param>
        void ChangeSquareDate(int oldIndex, int newIndex, int number)
        {
            //互換方塊資料
            squareDatas[newIndex].number = number;
            squareDatas[newIndex].obj = squareDatas[oldIndex].obj;
            squareDatas[newIndex].textObj = squareDatas[oldIndex].textObj;
            squareDatas[newIndex].imageObj = squareDatas[oldIndex].imageObj;
            squareDatas[newIndex].animator = squareDatas[oldIndex].animator;
            //獲勝
            if (number == winNumber) SetGameResult(true);
            //清理方塊資料
            CleanSquareData(oldIndex);
        }

        /// <summary>
        /// 清理方塊資料
        /// </summary>
        /// <param name="index">第幾筆資料</param>
        void CleanSquareData(int index)
        {
            squareDatas[index].number = 0;
            squareDatas[index].obj = null;
            squareDatas[index].textObj = null;
            squareDatas[index].imageObj = null;
            squareDatas[index].animator = null;
        }

        /// <summary>
        /// 方塊移動結果
        /// </summary>
        /// <returns></returns>
        IEnumerator ISquareMoveResult()
        {
            bool isMerge = false;//是否有方塊合併

            //方塊移動
            yield return new WaitUntil(() =>
            {
                bool isMoved = true;//判斷是否移動結束
                for (int i = 0; i < MoveSquareList.Count; i++)
                {
                    Vector3 nowPos = MoveSquareList[i].obj.transform.position;//目前位置
                    Vector3 targetPos = gridBgPosList[MoveSquareList[i].posIndex];//目標位置

                    //已完成
                    if (nowPos != targetPos) isMoved = false;
                }
                return isMoved;
            });

            //數字顯示
            for (int i = 0; i < MoveSquareList.Count; i++)
            {
                //顯示顏色
                float powNumber = 2;
                for (int pow = 1; pow < 12; pow++)
                {
                    powNumber = Mathf.Pow(2, pow);
                    if (powNumber == squareDatas[MoveSquareList[i].posIndex].number)
                    {
                        squareDatas[MoveSquareList[i].posIndex].imageObj.color = squareColors[pow-1];
                        break;
                    }
                }
                //顯示數字
                squareDatas[MoveSquareList[i].posIndex].textObj.text = squareDatas[MoveSquareList[i].posIndex].number.ToString();

                //移除相同數字
                if (MoveSquareList[i].destroyObj != null)
                {
                    isMerge = true;
                    
                    //撥放合併動畫
                    squareDatas[MoveSquareList[i].posIndex].animator.SetTrigger(animatorHashId[0]);
                    //關閉被合併方塊
                    MoveSquareList[i].destroyObj.gameObject.SetActive(false);
                }
            }

            if (isMerge)
            {
                //撥放得分音效
                audioManager.OnPlayScoreSound();
            }

            //重製List
            MoveSquareList = new List<MoveSquare>();
            //創建方塊
            CreateSquare();
            //移動結束
            isMoving = false;
        }
    }

    /// <summary>
    /// 物件池資料
    /// </summary>
    class objPoolData
    {
        [Tooltip("物件池編號")] public int id;
        [Tooltip("物件名稱")] public string objName;
        [Tooltip("物件")] public GameObject obj;
        [Tooltip("父物件")] public Transform parentObj;
    }

    /// <summary>
    /// 方塊資料
    /// </summary>
    [System.Serializable]
    class SquareData
    {
        [Tooltip("方塊內數字")] public int number;
        [Tooltip("方塊顏色")] public Image imageObj;
        [Tooltip("Animator")] public Animator animator;
        [Tooltip("方塊物件")] public RectTransform obj;
        [Tooltip("文字元件")] public Text textObj;
        [Tooltip("是否已合併過")] public bool isMerge;
    }

    /// <summary>
    /// 移動方塊資料
    /// </summary>
    [System.Serializable]
    class MoveSquare
    {
        [Tooltip("方塊物件")] public RectTransform obj;
        [Tooltip("移動位置index")] public int posIndex;
        [Tooltip("移除物件")] public RectTransform destroyObj;
    }
}
