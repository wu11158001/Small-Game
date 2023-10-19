using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Entry;
using LitJson;

/// <summary>
/// �R�A�}��
/// </summary>
namespace StaticScripts
{
    /// <summary>
    /// �y��½Ķ
    /// </summary>
    public class Language
    {
        static Language language = null;
        public static Language Instance
        {
            get
            {
                if (language == null)
                {
                    language = new Language();
                }
                return language;
            }
        }

        //Json�奻
        JsonData jsonData;
        //�d��y���r��
        Dictionary<string, string[]> infoList = new Dictionary<string, string[]>();
        //�d��½ĶID
        string[] languageId =
        {
            "Chinese",//����
            "English"//�^��
        };

        public readonly string serchLanguageStr = "Language";//�j�M���a�y���r��
        public int langIndex;//�ҿ�y��(0=����,1=�^��)

        private Language() { }

        /// <summary>
        /// Ū���y��Json
        /// </summary>
        public void LoadLangJson()
        {
            //���JJson���
            string jsonFile = Resources.Load<TextAsset>("Json/Language").text;
            jsonData = JsonMapper.ToObject<JsonData>(jsonFile);

            for (int i = 0; i < jsonData.Count; i++)
            {
                //�j�M�W��
                string serchName = jsonData[i]["SerchName"].ToString();
                //�N�Ҧ��y���s�J�r��
                string[] lang = new string[languageId.Length];
                for (int j = 0; j < languageId.Length; j++)
                {
                    lang[j] = jsonData[i][languageId[j]].ToString();
                }

                infoList.Add(serchName, lang);
            }
        }

        /// <summary>
        /// ���½Ķ
        /// </summary>
        /// <param name="serchName">�j�M�W��</param>
        /// <returns></returns>
        public string GetLanguage(string serchName)
        {
            string txt = infoList[serchName][language.langIndex];
            return txt;
        }
    }
}
