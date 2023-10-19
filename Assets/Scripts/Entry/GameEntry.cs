using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StaticScripts;

/// <summary>
/// 遊戲啟動入口
/// </summary>
namespace Entry
{
    /// <summary>
    /// 遊戲啟動入口
    /// </summary>
    public class GameEntry : MonoBehaviour
    {
        static GameEntry gameEntry;
        public static GameEntry Instance => gameEntry;

        Language language;//語言翻譯

        private void Awake()
        {
            if (gameEntry != null)
            {
                Destroy(this);
                return;
            }
            gameEntry = this;

            language = Language.Instance;//語言翻譯
            language.LoadLangJson();//讀取語言Json

#if UNITY_EDITOR
            Application.targetFrameRate = 30;
#endif
        }

        private void Start()
        {
            //獲取語言
            language.langIndex = PlayerPrefs.GetInt(language.serchLanguageStr);
            //進入大廳
            GameSceneManager.Instance.OnLoadScene("Lobby");
        }
    }
}
