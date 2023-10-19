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
    /// 俄羅斯方塊大廳
    /// </summary>
    public class TetrisLobbyMediator : MonoBehaviour
    {
        GameSceneManager gameSceneManager;//場景管理
        AudioManager audioManager;//音效管理
        Language language;//語言翻譯
        TetrisMediator tetrisMediator;//俄羅斯方塊

        [Header("預設資料")]
        [Tooltip("增加分數(0=未連線,1~4連幾條線)")]readonly int[] addScore =
        {
            50, 100, 300, 600, 800
        };
        [Tooltip("動畫名稱")]
        readonly string[] animatorName =
{
            "SpeedUp",//速度增加
        };
        [Tooltip("動畫HashId")] int[] animatorHashId;

        [Tooltip("當前場景Canvas")] Canvas canvas;

        [Header("元件")]
        [Tooltip("返回大廳按鈕")] [SerializeField] Button home_Btn;
        [Tooltip("開始按鈕")] [SerializeField] Button start_Btn;
        [Tooltip("開始按鈕文字")] [SerializeField] Text start_Txt;
        [Tooltip("遊戲開始關閉物件")] [SerializeField] GameObject GameStartEnable_Obj;

        [Header("選擇的速度等級")]
        [Tooltip("選擇的速度等級")] int selectSpeed;
        [Tooltip("當前速度等級")] public int currentSpeed;
        [Tooltip("紀錄加速的分數")] int autoSpeedScore;
        [Tooltip("速度變更按鈕(左)")] [SerializeField] Button selectSpeedLeft_Btn;
        [Tooltip("速度變更按鈕(右)")] [SerializeField] Button selectSpeedRight_Btn;
        [Tooltip("速度變更等級文字")] [SerializeField] Text selectSpeed_Txt;
        [Tooltip("選擇速度標題文字")] [SerializeField] Text selectSpeedTitle_Txt;
        [Tooltip("遊戲顯示速度標題文字")] [SerializeField] Text speedTitle_Txt;
        [Tooltip("遊戲顯示速度文字")] [SerializeField] Text speedLevel_Txt;

        [Header("遊戲結束")]
        [Tooltip("遊戲結束物件")] [SerializeField] GameObject gameOver_Obj;
        [Tooltip("遊戲結束文字")] [SerializeField] Text gameOver_Txt;
        [Tooltip("遊戲結束重新開始按鈕")] [SerializeField] Button gameOverReStart_Btn;
        [Tooltip("遊戲結束重新開始文字")] [SerializeField] Text gameOverRestart_Txt;

        [Header("工具")]
        [Tooltip("工具物件")] [SerializeField] GameObject tool_Obj;
        [Tooltip("重新開始按鈕")] [SerializeField] Button reStart_Btn;
        [Tooltip("回選擇關卡按鈕")] [SerializeField] Button selectLevel_Btn;
        [Tooltip("暫停按鈕")] [SerializeField] Button pause_Btn;
        [Tooltip("暫停按鈕圖片")] [SerializeField] Image pause_Img;
        [Tooltip("暫停按鈕圖片集(0=暫停,1=繼續)")] [SerializeField] Sprite[] pauseSprite;

        [Header("選項")]
        [Tooltip("自動加速文字")] [SerializeField] Text autoSpeed_Txt;
        [Tooltip("自動加速Toggle")] [SerializeField] Toggle autoSpeed_Tog;
        [Tooltip("虛方塊開關文字")] [SerializeField] Text ghostPiece_Txt;
        [Tooltip("虛方塊Toggle")] [SerializeField] Toggle ghostPiece_Tog;

        [Header("分數")]
        [Tooltip("用戶分數文字")] [SerializeField] Text userScore_Txt;
        [Tooltip("分數文字")] [SerializeField] Text ScoreTitle_Txt;
        [Tooltip("用戶分數")] int userScore;

        [Tooltip("是否暫停")] public bool isPause;

        private void Awake()
        {
            gameSceneManager = GameSceneManager.Instance;//場景管理
            audioManager = AudioManager.Instance;//音效管理
            language = Language.Instance;//語言翻譯

            canvas = FindObjectOfType<Canvas>();//當前場景Canvas

            //用戶分數
            userScore = 0;
            userScore_Txt.text = userScore.ToString();

            //用戶選擇速度
            currentSpeed = 0;
            selectSpeed_Txt.text = (currentSpeed + 1).ToString();

            //翻譯
            start_Txt.text = language.GetLanguage("Start");//開始
            gameOverRestart_Txt.text = language.GetLanguage("ReStart");//重新開始
            ScoreTitle_Txt.text = language.GetLanguage("Score");//分數
            speedTitle_Txt.text = language.GetLanguage("Speed");//速度
            autoSpeed_Txt.text = language.GetLanguage("AutoSpeed");//自動加速
            ghostPiece_Txt.text = language.GetLanguage("GhostPiece");//虛方塊
            selectSpeedTitle_Txt.text = language.GetLanguage("SpeedLevelTitle");//速度等級

            pause_Img.sprite = isPause ? pauseSprite[1] : pauseSprite[0];
            tool_Obj.SetActive(false);//工具物件
            gameOver_Obj.SetActive(false);//遊戲結束物件

            EventListener();//事件聆聽
        }

        private void Start()
        {
            //產生遊戲
            if (Utials.OpenView("TetrisView").TryGetComponent<TetrisMediator>(out TetrisMediator tetris))
            {
                tetrisMediator = tetris;
            }
            
            //設置UI層級至最下方
            Utials.SetUIToFront(canvas.transform, transform);
        }

        /// <summary>
        /// 事件聆聽
        /// </summary>
        void EventListener()
        {
            //返回大廳按鈕
            home_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效

                isPause = false;
                OnPause();//暫停

                tetrisMediator.objectPool.OnCleanPool(tetrisMediator.objectPoolDic);//清理物件池
                gameSceneManager.OnLoadScene("Lobby");
            });

            //開始按鈕
            start_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
                if (tetrisMediator != null)
                {
                    GameStartEnable_Obj.gameObject.SetActive(false);//遊戲開始關閉物件
                    tool_Obj.SetActive(true);//工具物件
                    StartCoroutine(tetrisMediator.ICreateGrid());//創建方格
                    selectSpeed = currentSpeed;//紀錄所選速度等級
                }
            });

            //遊戲結束重新開始按鈕
            gameOverReStart_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
                OnReStart();//重新開始遊戲
            });

            //暫停按鈕
            pause_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效

                isPause = !isPause;
                OnPause();//暫停
            });

            //回選擇關卡按鈕
            selectLevel_Btn.onClick.AddListener(() =>
            {
                //停止所有協程
                tetrisMediator.StopThisAllCoroutines();

                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效

                isPause = false;
                OnPause();//暫停

                tetrisMediator.objectPool.OnCleanPool(tetrisMediator.objectPoolDic);//清理物件池
                gameSceneManager.OnLoadScene("GameScene");
            });

            //重新開始按鈕
            reStart_Btn.onClick.AddListener(() =>
            {
                //停止所有協程
                tetrisMediator.StopThisAllCoroutines();

                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
                OnReStart();//重新開始遊戲
            });

            //速度變更按鈕(左)
            selectSpeedLeft_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
                //更換速度等級
                ChangeSpeedIndex(-1);
            });

            //速度變更按鈕(右)
            selectSpeedRight_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
                //更換速度等級
                ChangeSpeedIndex(1);
            });

            //自動加速選擇Tog
            autoSpeed_Tog.onValueChanged.AddListener((bool isSelect) =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
            });

            //虛方塊Tog
            ghostPiece_Tog.onValueChanged.AddListener((bool isSelect) =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
            });
        }

        /// <summary>
        /// 設定分數
        /// </summary>
        /// <param name="line">連線數量</param>
        public void SetScore(int line)
        {
            userScore += addScore[line];//用戶分數
            userScore_Txt.text = userScore.ToString();

            //自動加速開啟
            if (autoSpeed_Tog.isOn == true)
            {
                autoSpeedScore += addScore[line];
                if (autoSpeedScore / tetrisMediator.addSpeedScore >= 1 && currentSpeed < tetrisMediator.downLevelSpeed.Length - 1)
                {
                    autoSpeedScore -= tetrisMediator.addSpeedScore;//重製紀錄分數

                    //播放音效
                    audioManager.SetPlaySound("LevelUp");

                    //播放動畫
                    if (speedLevel_Txt.TryGetComponent<Animator>(out Animator animator))
                    {
                        animator.SetTrigger(Animator.StringToHash(animatorName[0]));
                    }

                    //更換速度等級
                    ChangeSpeedIndex(1);
                }
            }
        }

        /// <summary>
        /// 更換速度等級
        /// </summary>
        /// <param name="select">加/減</param>
        void ChangeSpeedIndex(int select)
        {
            //防呆
            if (select > 1) select = 1;
            else if (select < -1) select = -1;

            currentSpeed += select;
            if (currentSpeed >= tetrisMediator.downLevelSpeed.Length) currentSpeed = tetrisMediator.downLevelSpeed.Length - 1;
            else if (currentSpeed < 0) currentSpeed = 0;

            string speedStr = (currentSpeed + 1).ToString(); ;
            selectSpeed_Txt.text = speedStr;//選擇速度文字
            speedLevel_Txt.text = speedStr;//遊戲顯示速度文字

            //更換速度
            tetrisMediator.downSpeed = tetrisMediator.downLevelSpeed[currentSpeed];
        }

        /// <summary>
        /// 暫停
        /// </summary>
        void OnPause()
        {
            Time.timeScale = isPause ? 0 : 1;
            pause_Img.sprite = isPause ? pauseSprite[1] : pauseSprite[0];
        }

        /// <summary>
        /// 遊戲結束
        /// </summary>
        /// <param name="isWin">是否獲勝</param>
        public void OnGameOver(bool isWin)
        {
            audioManager.OnPlayGameOverSound(isWin);//播放音效
            tetrisMediator.isGameStart = false;
            gameOver_Obj.SetActive(true);//遊戲結束物件
            gameOver_Txt.text = isWin == true ? "Win" : "Game Over";//結束文字
        }

        //重新開始遊戲
        void OnReStart()
        {
            tetrisMediator.isGameStart = true;
            gameOver_Obj.SetActive(false);//遊戲結束物件

            isPause = false;
            OnPause();//暫停

            //用戶分數重製
            userScore = 0;
            userScore_Txt.text = userScore.ToString();

            //速度等級重製
            currentSpeed = selectSpeed;
            speedLevel_Txt.text = (currentSpeed + 1).ToString();//遊戲顯示速度文字

            //更換速度
            tetrisMediator.downSpeed = tetrisMediator.downLevelSpeed[currentSpeed];

            //重新產生方塊
            int poolNum = tetrisMediator.objectPool.SerchObjId(tetrisMediator.objPoolName[0], tetrisMediator.objectPoolDic);//搜索物件池ID
            tetrisMediator.objectPool.OnDestroyObject(poolNum);//移除方塊物件
            tetrisMediator.stopSquaresList.Clear();//清除紀錄停止方塊
            tetrisMediator.CreateNewSquare();//創建新方塊
        }

        /// <summary>
        /// 獲取虛方塊開啟狀態
        /// </summary>
        public bool GetGhostPieceState
        {
            get
            {
                return ghostPiece_Tog.isOn;
            }
        }
    }
}