using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StaticScripts;
using Entry;

/// <summary>
/// 大廳
/// </summary>
namespace Lobby
{
    /// <summary>
    /// 大廳
    /// </summary>
    public class LobbyMediator : MonoBehaviour
    {
        GameSceneManager gameSceneManager;//場景管理
        AudioManager audioManager;//音效管理
        Language language;//語言翻譯

        [Tooltip("項目容器")] [SerializeField] RectTransform Content;
        [Tooltip("項目樣本")] [SerializeField] RectTransform itemSample;

        [Tooltip("項目間距")] readonly int spacing = 200;
        [Tooltip("項目名稱元件List")] [SerializeField] Text[] itemNmaesObj;

        [Tooltip("中文按鈕")] [SerializeField] Button chinese_Btn;
        [Tooltip("英文按鈕")] [SerializeField] Button english_Btn;
        [Tooltip("音樂按鈕")] [SerializeField] Button music_Btn;
        [Tooltip("音效按鈕")] [SerializeField] Button sound_Btn;

        [Tooltip("音樂音效圖片集")] [SerializeField] Sprite[] musicSprites;//0.1=音樂, 1.2=音效
        [Tooltip("音樂圖片物件")] [SerializeField] Image music_Img;
        [Tooltip("音效圖片物件")] [SerializeField] Image sound_Img;

        //遊戲關卡
        readonly public string[] itemNames = { "NumberGame", "Slither", "Tetris" };//遊戲項目名稱(2048, 貪吃蛇, 俄羅斯方塊)

        private void Awake()
        {
            gameSceneManager = GameSceneManager.Instance;//場景管理
            audioManager = AudioManager.Instance;//音效管理
            language = Language.Instance;//語言翻譯

            itemNmaesObj = new Text[itemNames.Length];

            SetMusicSprite();//設置音樂圖片
            EventListener();//事件聆聽
        }

        /// <summary>
        /// 事件聆聽
        /// </summary>
        void EventListener()
        {
            //中文按鈕
            chinese_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
                language.langIndex = 0;
                OnChangeLanguage();//更換語言
            });
            //英文按鈕
            english_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
                language.langIndex = 1;
                OnChangeLanguage();//更換語言
            });
            //音樂按鈕
            music_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
                audioManager.OnMusicSwitch();//音樂開關
                SetMusicSprite();//設置音樂圖片
            });
            //音效按鈕
            sound_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
                audioManager.OnSoundSwitch();//音效開關
                SetMusicSprite();//設置音樂圖片
            });
        }

        private void Start()
        {
            CreateItems();//產生項目
        }

        /// <summary>
        /// 設置音樂圖片
        /// </summary>
        void SetMusicSprite()
        {
            //音樂
            music_Img.sprite = audioManager.musicSwitch[0] == true ? musicSprites[0] : musicSprites[1];
            //音效
            sound_Img.sprite = audioManager.musicSwitch[1] == true ? musicSprites[2] : musicSprites[3];
        }

        /// <summary>
        /// 更換語言
        /// </summary>
        void OnChangeLanguage()
        {
            PlayerPrefs.SetInt(language.serchLanguageStr, language.langIndex);
            for (int i = 0; i < itemNmaesObj.Length; i++)
            {
                itemNmaesObj[i].text = language.GetLanguage(itemNames[i]);
            }
        }

        /// <summary>
        /// 產生項目
        /// </summary>
        void CreateItems()
        {
            //防呆
            if (Content == null || itemSample == null) return;

            //獲取樣本sizeDelta
            float sizeX = itemSample.sizeDelta.x;
            float sizeY = itemSample.sizeDelta.y;

            //Layout設定
            VerticalLayoutGroup layout = Content.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 500, spacing);
            layout.spacing = spacing;

            itemSample.gameObject.SetActive(false);
            for (int i = 0; i < itemNames.Length; i++)
            {
                //項目按鈕
                Button itemButton = Instantiate(itemSample, Content).GetComponent<Button>();
                itemButton.gameObject.SetActive(true);
                int index = i;
                itemButton.onClick.AddListener(() =>
                {
                    audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
                    SetItemButtonEvent(index);//設定項目按鈕事件
                });
                //項目名稱
                Text itemNmae = Utials.FindChild<Text>(itemButton.transform, "ItemName");
                itemNmae.text = language.GetLanguage(itemNames[i]);
                itemNmaesObj[i] = itemNmae;
                Content.sizeDelta = new Vector2(sizeX, sizeY * (itemNames.Length + 1) + spacing);
            }
        }

        /// <summary>
        /// 設定項目按鈕事件
        /// </summary>
        /// <param name="index">項目index</param>
        void SetItemButtonEvent(int index)
        {
            gameSceneManager.selectGameIndex = index;//選擇的遊戲Index
            gameSceneManager.OnLoadScene("GameScene");//進入遊戲場景
        }
    }
}
