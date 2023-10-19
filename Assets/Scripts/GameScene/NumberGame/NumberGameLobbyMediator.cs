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
    /// 數字遊戲2048大廳
    /// </summary>
    public class NumberGameLobbyMediator : MonoBehaviour
    {
        GameSceneManager gameSceneManager;//場景管理
        AudioManager audioManager;//音效管理
        Language language;//語言翻譯

        [Tooltip("關卡大小")] public readonly int[] level = { 3, 4, 5, 6, 8 };

        [Tooltip("選擇的關卡")] public int numberGameLevel;

        [Header("元件")]
        [Tooltip("左按鈕")] [SerializeField] Button left_Btn;
        [Tooltip("右按鈕")] [SerializeField] Button right_Btn;
        [Tooltip("關卡文字")] [SerializeField] Text level_Txe;
        [Tooltip("開始按鈕")] [SerializeField] Button start_Btn;
        [Tooltip("開始按鈕文字")] [SerializeField] Text start_Txt;
        [Tooltip("返回大廳按鈕")] [SerializeField] Button home_Btn;

        private void Awake()
        {
            gameSceneManager = GameSceneManager.Instance;//場景管理
            audioManager = AudioManager.Instance;//音效管理
            language = Language.Instance;//語言翻譯

            numberGameLevel = 1;//預設關卡=1

            //翻譯
            start_Txt.text = language.GetLanguage("Start");//開始

            EventListener();//事件聆聽
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
                gameSceneManager.OnLoadScene("Lobby");
            });
            //左按鈕
            left_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
                ChangeLevel(-1);
            });
            //右按鈕
            right_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
                ChangeLevel(1);
            });
            //開始按鈕
            start_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
                gameObject.SetActive(false);
                if(Utials.OpenView("NumberGameView").TryGetComponent<NumberGameMediator>(out NumberGameMediator numberGameMediator))
                {
                    numberGameMediator.SetGride(level[numberGameLevel], numberGameLevel);//設定方格
                }
            });
        }

        private void Start()
        {
            ChangeLevel(0);//預設關卡
        }

        /// <summary>
        /// 更換關卡
        /// </summary>
        /// <param name="select">1=增/-1=減</param>
        void ChangeLevel(int select)
        {
            //防呆
            if (select >= 1) select = 1;
            else if (select <= -1) select = -1;

            //選擇的關卡index
            numberGameLevel += select;
            if (numberGameLevel > level.Length - 1) numberGameLevel = 0;
            else if (numberGameLevel < 0) numberGameLevel = level.Length - 1;
            //關卡文字
            level_Txe.text = level[numberGameLevel] + " X " + level[numberGameLevel];

        }
    }
}