using UnityEngine;
using UnityEngine.SceneManagement;
using Entry;

/// <summary>
/// �R�A�}��
/// </summary>
namespace StaticScripts
{
    /// <summary>
    /// ��Τu��
    /// </summary>
    public static class Utials
    {
        //�M��l����
        public static T FindChild<T>(this Transform searchObj, string searchName) where T : Component
        {
            for (int i = 0; i < searchObj.childCount; i++)
            {
                //�l�����٦��l����
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
        /// �}��View
        /// </summary>
        /// <param name="viewNmae">View�W��</param>
        public static Transform OpenView(string viewNmae)
        {
            Transform view = null;

            //view���s�b
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
                    //�]�mUI�h�Ŧ̤ܳU��
                    SetUIToFront(canvas, view_T);

                    view = view_T;
                }
            }


            return view;
        }

        /// <summary>
        /// �]�mUI�h�Ŧ̤ܳU��
        /// </summary>
        /// <param name="useCanvas">��e����Canvas</param>
        /// <param name="showUI">�n��ܪ�UI</param>
        public static void SetUIToFront(Transform useCanvas ,Transform showUI)
        {
            if (useCanvas != null && showUI != null)
            {
                showUI.SetSiblingIndex(useCanvas.childCount);
            }
        }

        /// <summary>
        /// UI��m��ù��y��
        /// </summary>
        /// <param name="rt">����</param>
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

