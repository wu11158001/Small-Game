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
    /// 貪食蛇大廳
    /// </summary>
    public class SlitherLobbyMediator : MonoBehaviour
    {
        GameSceneManager gameSceneManager;//場景管理
        AudioManager audioManager;//音效管理
        Language language;//語言翻譯
        SlitherMediator slitherMediator;//貪食蛇

        [Header("元件")]
        [Tooltip("開始按鈕")][SerializeField] Button start_Btn;
        [Tooltip("開始按鈕文字")] [SerializeField] Text start_Txt;
        [Tooltip("返回大廳按鈕")] [SerializeField] Button home_Btn;

        [Tooltip("遊戲結束物件")] [SerializeField] GameObject gameOver_Obj;
        [Tooltip("遊戲結束_文字")] [SerializeField] Text gameOver_Txt;

        [Tooltip("穿牆物件")] [SerializeField] GameObject through_Obj;
        [Tooltip("穿牆文字")] [SerializeField] Text through_Txt;
        [Tooltip("穿牆選擇Tog")] [SerializeField] Toggle through_Tog;

        [Tooltip("當前場景Canvas")] Canvas canvas;

        private void Awake()
        {
            gameSceneManager = GameSceneManager.Instance;//場景管理
            audioManager = AudioManager.Instance;//音效管理
            language = Language.Instance;//語言翻譯

            canvas = FindObjectOfType<Canvas>();//當前場景Canvas

            //翻譯
            start_Txt.text = language.GetLanguage("Start");//開始
            through_Txt.text = language.GetLanguage("IsThrough");//穿牆

            gameOver_Obj.SetActive(false);//遊戲結束物件

            EventListener();//事件聆聽
        }

        private void Start()
        {
            //產生遊戲
            if (Utials.OpenView("SlitherView").TryGetComponent<SlitherMediator>(out SlitherMediator slither))
            {
                slitherMediator = slither;
            }
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
                slitherMediator.CleanObjectPool();//清理物件池
                gameSceneManager.OnLoadScene("Lobby");
            });

            //開始按鈕
            start_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效

                //貪食蛇遊戲場景
                if (slitherMediator != null)
                {
                    slitherMediator.OnInitGame(through_Tog.isOn);//遊戲初始化
                    slitherMediator.RefreshFoodPos();//刷新食物位置

                    slitherMediator.isGameStart = true;//遊戲開始
                    slitherMediator.Control_Obj.SetActive(true);//操作物件
                    through_Obj.SetActive(false);//穿牆物件
                    gameOver_Obj.SetActive(false);//遊戲結束物件
                    start_Btn.gameObject.SetActive(false);//開始按鈕

                    //設置UI層級至最下方
                    Utials.SetUIToFront(canvas.transform, transform);
                } 
            });

            //穿牆選擇Tog
            through_Tog.onValueChanged.AddListener((bool isSelect) =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
            });
        }

        /// <summary>
        /// 遊戲結束
        /// </summary>
        /// <param name="isWin">是否獲勝</param>
        public void OnGameOver(bool isWin)
        {
            //播放遊戲結束音效
            audioManager.OnPlayGameOverSound(isWin);

            if (slitherMediator != null)
            {
                slitherMediator.isGameStart = false;//關閉控制
                slitherMediator.Control_Obj.SetActive(false);//操作物件
            }

            gameOver_Obj.SetActive(true);//遊戲結束物件
            start_Btn.gameObject.SetActive(true);//開始按鈕
            through_Obj.SetActive(true);//穿牆物件

            start_Txt.text = language.GetLanguage("ReStart");//重新開始
            gameOver_Txt.text = isWin == true ? "Win" : "Game Over";
        }
    }
}
