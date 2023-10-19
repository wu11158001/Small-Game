using UnityEngine;
using UnityEngine.SceneManagement;
using Entry;

/// <summary>
/// 靜態腳本
/// </summary>
namespace StaticScripts
{
    /// <summary>
    /// 實用工具
    /// </summary>
    public static class Utials
    {
        //尋找子物件
        public static T FindChild<T>(this Transform searchObj, string searchName) where T : Component
        {
            for (int i = 0; i < searchObj.childCount; i++)
            {
                //子物件內還有子物件
                if (searchObj.GetChild(i).childCount > 0)
                {
                    var obj = searchObj.GetChild(i).FindChild<Transform>(searchName);
                    if (obj != null) return obj.GetComponent<T>();
                }

                if (searchObj.GetChild(i).name == searchName)
                {
                    return searchObj.GetChild(i).GetComponent<T>();
                }
            }

            return default;
        }

        /// <summary>
        /// 開啟View
        /// </summary>
        /// <param name="viewNmae">View名稱</param>
        public static Transform OpenView(string viewNmae)
        {
            Transform view = null;

            //view不存在
            if (UnityEngine.GameObject.Find(viewNmae) == null)
            {
                string currentSceneName = SceneManager.GetActiveScene().name;
                GameObject viewObj = Resources.Load<GameObject>("Prefab/" + currentSceneName + "/" + GameSceneManager.Instance.gameFolder + viewNmae);
                RectTransform view_RT = GameObject.Instantiate(viewObj).GetComponent<RectTransform>();
                view_RT.SetParent(GameObject.FindObjectOfType<Canvas>().transform);
                view_RT.offsetMax = Vector2.zero;
                view_RT.offsetMin = Vector2.zero;
                view_RT.name = viewNmae;

                view = view_RT;
            }
            else
            {
                Transform canvas = UnityEngine.GameObject.FindObjectOfType<Canvas>().transform;
                if (canvas != null)
                {
                    Transform view_T = UnityEngine.GameObject.Find(viewNmae).transform;
                    //設置UI層級至最下方
                    SetUIToFront(canvas, view_T);

                    view = view_T;
                }
            }


            return view;
        }

        /// <summary>
        /// 設置UI層級至最下方
        /// </summary>
        /// <param name="useCanvas">當前場景Canvas</param>
        /// <param name="showUI">要顯示的UI</param>
        public static void SetUIToFront(Transform useCanvas ,Transform showUI)
        {
            if (useCanvas != null && showUI != null)
            {
                showUI.SetSiblingIndex(useCanvas.childCount);
            }
        }

        /// <summary>
        /// UI位置轉螢幕座標
        /// </summary>
        /// <param name="rt">物件</param>
        /// <returns></returns>
        public static Rect RectTransToScreenPos(RectTransform rt)
        {
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            Vector2 v0 = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
            Vector2 v1 = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

            Rect rect = new Rect(v0, v1 - v0);
            return rect;
        }
    }
}

