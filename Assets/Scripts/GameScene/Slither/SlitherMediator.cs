using Entry;
using StaticScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 遊戲場景
/// </summary>
namespace GameScene
{
    /// <summary>
    /// 貪食蛇
    /// </summary>
    public class SlitherMediator : MonoBehaviour
    {
        ObjectPool objectPool;//物件池
        AudioManager audioManager;//音效管理
        Language language;//語言翻譯
        SlitherLobbyMediator slitherLobby;//貪食蛇大廳

        [Header("預設資料")]
        [Tooltip("初始身體數量")] readonly int initBodyCount = 7;
        [Tooltip("身體大小")] readonly int bodySize = 20;
        [Tooltip("物件池名稱")]
        readonly string[] objPoolName =
{
            "SlitherBody",//貪食蛇身體
        };

        [Header("操作元件")]
        [Tooltip("操作物件")] public GameObject Control_Obj;
        [Tooltip("控制_上")] [SerializeField] Button up_Btn;
        [Tooltip("控制_下")] [SerializeField] Button down_Btn;
        [Tooltip("控制_左")] [SerializeField] Button left_Btn;
        [Tooltip("控制_右")] [SerializeField] Button right_Btn;

        [Header("元件")]
        [Tooltip("方格背景物件/身體物件父物件")] [SerializeField] RectTransform gridBg_Img;
        [Tooltip("身體物件樣本")] [SerializeField] RectTransform bodySample;
        [Tooltip("食物物件樣本")] [SerializeField] RectTransform foodSample;

        [Header("資料")]
        [Tooltip("方格X數量")] [SerializeField] int gridSizeX;
        [Tooltip("方格Y數量")] [SerializeField] int gridSizeY;
        [Tooltip("紀錄身體物件")] [SerializeField] List<RectTransform> bodyList = new List<RectTransform>();
        [Tooltip("記錄身體前個位置")] [SerializeField] List<Vector2> previousPosList = new List<Vector2>();
        [Tooltip("移動速度表(秒)")][SerializeField] float[] speedList;
        [Tooltip("速度增加的食物數量")][SerializeField] int[] addSpeedFood;

        [Header("物件池")]
        [Tooltip("物件池(物件名稱,編號)")] Dictionary<string, int> objectPoolDic = new Dictionary<string, int>();

        [Header("控制")]
        [Tooltip("是否開始遊戲")] public bool isGameStart;
        [Tooltip("是否穿牆")] [SerializeField] bool isThrough;
        [Tooltip("移動方向")] Vector2 moveDirection;
        [Tooltip("領頭身體物件")] RectTransform leaderBody;
        [Tooltip("移動更新計時器")][SerializeField] float moveRefreshTime;
        [Tooltip("目前移動速度")][SerializeField] float nowMoveSpeed;

        [Header("分數")]
        [Tooltip("用戶分數文字")] [SerializeField] Text userScore_Txt;
        [Tooltip("分數文字")] [SerializeField] Text ScoreTitle_Txt;
        [Tooltip("用戶分數")] int userScore;

        private void Awake()
        {
            objectPool = ObjectPool.Instance;//物件池
            audioManager = AudioManager.Instance;//音效管理
            language = Language.Instance;//語言翻譯
            slitherLobby = FindObjectOfType<SlitherLobbyMediator>();

            //資料
            gridSizeX = (int)(gridBg_Img.sizeDelta.x / bodySize) - 1;//方格X數量
            gridSizeY = (int)(gridBg_Img.sizeDelta.y / bodySize) - 1;//方格Y數量
            
            int levelCount = 6;//等級階段數量
            speedList = new float[levelCount];
            addSpeedFood = new int[levelCount];
            for (int i = 0; i < speedList.Length; i++)
            {
                speedList[i] = 0.09f - (i * 0.01f);//移動速度表(秒)
                addSpeedFood[i] = (gridSizeX * gridSizeY) / ((speedList.Length - i) + 1);
            }

            Control_Obj.SetActive(false);//控制物件

            //翻譯
            ScoreTitle_Txt.text = language.GetLanguage("Score");//分數

            CreateObjectPool();//創建物件池
            EventListener();//事件聆聽
        }

        /// <summary>
        /// 事件聆聽
        /// </summary>
        void EventListener()
        {
            //上按鈕
            up_Btn.onClick.AddListener(() =>
            {
                OnControlBtnClick(new Vector2(0, 1));//方向按鈕按下
            });
            //下按鈕
            down_Btn.onClick.AddListener(() =>
            {
                OnControlBtnClick(new Vector2(0, -1));//方向按鈕按下
            });
            //左按鈕
            left_Btn.onClick.AddListener(() =>
            {
                OnControlBtnClick(new Vector2(-1, 0));//方向按鈕按下
            });
            //右按鈕
            right_Btn.onClick.AddListener(() =>
            {
                OnControlBtnClick(new Vector2(1, 0));//方向按鈕按下
            });
        }

        private void Start()
        {
            CreateInitBody();//產生初始身體
        }

        private void Update()
        {
            OnInputControl();//輸入控制
            SnakeMove();//貪食蛇移動
        }

        /// <summary>
        /// 輸入控制
        /// </summary>
        void OnInputControl()
        {
            //上
            if (Input.GetKeyDown(KeyCode.W))
            {
                OnControlBtnClick(new Vector2(0, 1));//方向按鈕按下
            }
            //下
            if (Input.GetKeyDown(KeyCode.S))
            {
                OnControlBtnClick(new Vector2(0, -1));//方向按鈕按下
            }
            //左
            if (Input.GetKeyDown(KeyCode.A))
            {
                OnControlBtnClick(new Vector2(-1, 0));//方向按鈕按下
            }
            //右
            if (Input.GetKeyDown(KeyCode.D))
            {
                OnControlBtnClick(new Vector2(1, 0));//方向按鈕按下
            }
        }

        /// <summary>
        /// 方向按鈕按下
        /// </summary>
        /// <param name="dir">邊更方向</param>
        void OnControlBtnClick(Vector2 dir)
        {
            audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效

            //不能反方向/相同方向移動
            if ((dir.x != 0 && moveDirection.x != 0) || (dir.y != 0 && moveDirection.y != 0))
            {
                return;
            }

            moveRefreshTime = 0;//重製計時器
            moveDirection = dir;//移動方向
            SnakeMove();//貪食蛇移動
        }

        /// <summary>
        /// 貪食蛇移動
        /// </summary>
        void SnakeMove()
        {
            //遊戲開始 && 有身體物件
            if (isGameStart && bodyList.Count > 0)
            {
                moveRefreshTime -= Time.deltaTime;
                if (moveRefreshTime <= 0)
                {
                    //重製計時器
                    moveRefreshTime = nowMoveSpeed;

                    //領頭準備移動位置
                    float posX = leaderBody.anchoredPosition.x + (bodySize * moveDirection.x);
                    float posY = leaderBody.anchoredPosition.y + (bodySize * moveDirection.y);
                    Vector2 preparePos = new Vector2(posX, posY);

                    //穿牆關閉
                    if (isThrough == false)
                    {
                        if (IsMoveOutside(preparePos) != 0)
                        {
                            slitherLobby.OnGameOver(false);//顯示遊戲結束畫面
                            return;
                        }
                        else leaderBody.anchoredPosition = preparePos;
                    }
                    else
                    {
                        int sideX = (gridSizeX / 2) * bodySize;//方格最外邊X
                        int sideY = (gridSizeY / 2) * bodySize;//方格最外邊Y
                        switch (IsMoveOutside(preparePos))
                        {
                            case 0://沒有移動到外側
                                leaderBody.anchoredPosition = preparePos;
                                break;
                            case 1://超出上側
                                leaderBody.anchoredPosition = new Vector2(leaderBody.anchoredPosition.x, -sideY);
                                return;
                            case 2://超出下側
                                leaderBody.anchoredPosition = new Vector2(leaderBody.anchoredPosition.x, sideY);
                                return;
                            case 3://超出左側
                                leaderBody.anchoredPosition = new Vector2(sideX, leaderBody.anchoredPosition.y);
                                return;
                            case 4://超出右側
                                leaderBody.anchoredPosition = new Vector2(-sideX, leaderBody.anchoredPosition.y);
                                return;
                        }
                    }

                    //身體移動
                    for (int i = 1; i < bodyList.Count; i++)
                    {
                        //位置等於前一個身體位置
                        bodyList[i].anchoredPosition = previousPosList[i - 1];
                    }

                    //更新位置
                    for (int i = 0; i < bodyList.Count; i++)
                    {
                        previousPosList[i] = bodyList[i].anchoredPosition;
                    }

                    //檢測是否碰到身體
                    for (int i = 1; i < previousPosList.Count; i++)
                    {
                        if (leaderBody.anchoredPosition == previousPosList[i])
                        {
                            slitherLobby.OnGameOver(false);//顯示遊戲結束畫面
                            return;
                        }
                    }    

                    //吃到食物
                    if (leaderBody.anchoredPosition == foodSample.anchoredPosition)
                    {
                        EatFood();
                    }
                }
            }
        }

        /// <summary>
        /// 是否移動到外側
        /// </summary>
        /// <param name="pos">判斷的Vector2</param>
        /// <returns></returns>
        int IsMoveOutside(Vector2 pos)
        {
            int sideX = (gridSizeX / 2) * bodySize;//方格最外邊X
            int sideY = (gridSizeY / 2) * bodySize;//方格最外邊Y

            if (pos.y > sideY) return 1;//超出上側
            else if (pos.y < -sideY) return 2;//超出下側
            else if (pos.x < -sideX) return 3;//超出左側
            else if (pos.x > sideX) return 4; //超出右側

            return 0;//沒有移動到外側
        }    

        /// <summary>
        /// 創建物件池
        /// </summary>
        void CreateObjectPool()
        {
            //物件池編號_身體
            int num = objectPool.OnCreateAndRecordObject(bodySample.gameObject, gridBg_Img);
            objectPoolDic.Add(objPoolName[0], num);
        }

        /// <summary>
        /// 清理物件池
        /// </summary>
        public void CleanObjectPool()
        {
            objectPool.OnCleanPool(objectPoolDic);
        }

        /// <summary>
        /// 遊戲初始化
        /// </summary>
        /// <param name="isThrough">是否穿牆</param>
        public void OnInitGame(bool isThrough)
        {
            this.isThrough = isThrough;

            //方格顏色
            if (gridBg_Img.TryGetComponent<Image>(out Image img))
            {
                img.color = isThrough == false ? Color.red : Color.green;
            }

            moveDirection = new Vector2(1, 0);//預設移動方向
            nowMoveSpeed = speedList[0];//預設速度

            //物件池物件刪除_身體
            objectPool.OnDestroyObject(objectPool.SerchObjId(objPoolName[0] ,objectPoolDic));
            bodyList = new List<RectTransform>();
            previousPosList = new List<Vector2>();

            //分數
            userScore = 0;
            userScore_Txt.text = userScore.ToString();

            CreateInitBody();//產生初始身體
        }

        /// <summary>
        /// 產生初始身體
        /// </summary>
        void CreateInitBody()
        {
            bodySample.gameObject.SetActive(false);//身體物件樣本

            //產生身體
            for (int i = 0; i < initBodyCount; i++)
            {
                RectTransform obj = CreateBody();//產生身體
                obj.anchoredPosition = new Vector2(0 - (bodySize * i), 0);//位置
                previousPosList.Add(obj.anchoredPosition);//紀錄位置
                //領頭身體物件
                if (i == 0) leaderBody = obj;
            }
        }

        /// <summary>
        /// 產生身體
        /// </summary>
        RectTransform CreateBody()
        {
            int poolNum = objectPool.SerchObjId(objPoolName[0], objectPoolDic);//搜索物件池ID
            RectTransform obj = objectPool.OnActiveObject(poolNum, gridBg_Img).GetComponent<RectTransform>();
            obj.sizeDelta = new Vector2(bodySize, bodySize);//大小
            obj.SetParent(gridBg_Img);
            bodyList.Add(obj);//紀錄物件
            
            return obj;
        }

        /// <summary>
        /// 吃到食物
        /// </summary>
        void EatFood()
        {
            //撥放得分音效
            audioManager.OnPlayScoreSound();

            //分數
            userScore++;
            userScore_Txt.text = userScore.ToString();

            //獲勝(分數 >= (網格邊長X * 網格邊長Y) - 初始身體數量-1)
            if (userScore >= (gridSizeX * gridSizeY) - (initBodyCount - 1))
            {
                //顯示遊戲結束畫面
                foodSample.transform.position = new Vector2(0 - foodSample.sizeDelta.x, 0 - foodSample.sizeDelta.y);//食物移出畫面
                slitherLobby.OnGameOver(true);//顯示遊戲結束畫面
                return;
            }

            //加速
            for (int i = 0; i < addSpeedFood.Length; i++)
            {
                //分數達增加速度標準 && 防呆(不超過速度列表)
                if (userScore >= addSpeedFood[i] && i < speedList.Length)
                {
                    nowMoveSpeed = speedList[i];
                }
            }
            
            //產生身體
            RectTransform obj = CreateBody();//產生身體
            obj.anchoredPosition = bodyList[bodyList.Count - 1].anchoredPosition;//在最後身體位置
            previousPosList.Add(obj.anchoredPosition);//紀錄位置

            //刷新食物位置
            RefreshFoodPos();
        }


        /// <summary>
        /// 刷新食物位置
        /// </summary>
        public void RefreshFoodPos()
        {
            //網格位置列表
            List<Vector2> gridPosList = new List<Vector2>();
            for (int i = 0; i < gridSizeX; i++)
            {
                for (int j = 0; j < gridSizeY; j++)
                {
                    int side = (int)(-gridBg_Img.sizeDelta.x / 2) + bodySize;//邊邊位置
                    Vector2 pos = new Vector2(side + (i * bodySize), side + (j * bodySize));
                    gridPosList.Add(pos);
                }
            }

            for (int i = 0; i < bodyList.Count; i++)
            {
                //移除目前蛇的所有位置
                gridPosList.Remove(bodyList[i].anchoredPosition);
            }

            foodSample.anchoredPosition = gridPosList[Random.Range(0, gridPosList.Count)];
        }
    }
}