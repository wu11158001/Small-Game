using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StaticScripts;

/// <summary>
/// �C���ҰʤJ�f
/// </summary>
namespace Entry
{
    /// <summary>
    /// �C���ҰʤJ�f
    /// </summary>
    public class GameEntry : MonoBehaviour
    {
        static GameEntry gameEntry;
        public static GameEntry Instance => gameEntry;

        Language language;//�y��½Ķ

        private void Awake()
        {
            if (gameEntry != null)
            {
                Destroy(this);
                return;
            }
            gameEntry = this;

            language = Language.Instance;//�y��½Ķ
            language.LoadLangJson();//Ū���y��Json

#if UNITY_EDITOR
            Application.targetFrameRate = 30;
#endif
        }

        private void Start()
        {
            //����y��
            language.langIndex = PlayerPrefs.GetInt(language.serchLanguageStr);
            //�i�J�j�U
            GameSceneManager.Instance.OnLoadScene("Lobby");
        }
    }
}
