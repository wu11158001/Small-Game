using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Entry;
using LitJson;

/// <summary>
/// 靜態腳本
/// </summary>
namespace StaticScripts
{
    /// <summary>
    /// 語言翻譯
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

        //Json文本
        JsonData jsonData;
        //查找語言字典
        Dictionary<string, string[]> infoList = new Dictionary<string, string[]>();
        //查找翻譯ID
        string[] languageId =
        {
            "Chinese",//中文
            "English"//英文
        };

        public readonly string serchLanguageStr = "Language";//搜尋本地語言字串
        public int langIndex;//所選語言(0=中文,1=英文)

        private Language() { }

        /// <summary>
        /// 讀取語言Json
        /// </summary>
        public void LoadLangJson()
        {
            //載入Json文件
            string jsonFile = Resources.Load<TextAsset>("Json/Language").text;
            jsonData = JsonMapper.ToObject<JsonData>(jsonFile);

            for (int i = 0; i < jsonData.Count; i++)
            {
                //搜尋名稱
                string serchName = jsonData[i]["SerchName"].ToString();
                //將所有語言存入字典
                string[] lang = new string[languageId.Length];
                for (int j = 0; j < languageId.Length; j++)
                {
                    lang[j] = jsonData[i][languageId[j]].ToString();
                }

                infoList.Add(serchName, lang);
            }
        }

        /// <summary>
        /// 獲取翻譯
        /// </summary>
        /// <param name="serchName">搜尋名稱</param>
        /// <returns></returns>
        public string GetLanguage(string serchName)
        {
            string txt = infoList[serchName][language.langIndex];
            return txt;
        }
    }
}
