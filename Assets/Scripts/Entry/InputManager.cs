using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 遊戲啟動入口
/// </summary>
namespace Entry
{
    /// <summary>
    /// 輸入控制
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        static InputManager inputManager;
        public static InputManager Instance => inputManager;

        [Header("判斷是否在UI上")]
        private GraphicRaycaster graphicRaycaster;
        private EventSystem eventSystem;
        private PointerEventData eventData;
        private bool init = false;

        private void Awake()
        {
            if (inputManager != null)
            {
                Destroy(this);
                return;
            }
            inputManager = this;
            DontDestroyOnLoad(this);
        }

        /// <summary>
        /// 重製判斷UI上的Component
        /// </summary>
        public void InitOnUIElementComponent()
        {
            init = false;
            graphicRaycaster = null;
            eventSystem = null;
            eventData = null;
        }

        //尋找當前場景eventSystem
        void FindEventSystem()
        {
            if (!init)
            {
                //根據項目找canvas上的GraphicRaycaster
                graphicRaycaster = GameObject.Find("Canvas").GetComponent<GraphicRaycaster>();
                //當前場景eventSystem 
                eventSystem = EventSystem.current;
                eventData = new PointerEventData(eventSystem);
                init = true;
            }
        }
        /// <summary>
        /// 是否在UI上
        /// </summary>
        /// <returns></returns>
        public bool IsOnUIElement()
        {
            //尋找當前場景eventSystem
            FindEventSystem();
            if (graphicRaycaster == null)
            {
                Debug.Log("未有GraphicRaycaster");
                return false;
            }
            eventData.pressPosition = Input.mousePosition;
            eventData.position = Input.mousePosition;
            List<RaycastResult> list = new List<RaycastResult>();
            graphicRaycaster.Raycast(eventData, list);
            foreach (var temp in list)
            {
                if (temp.gameObject.layer.Equals(5)) return true;
            }
            return false;
        }
    }
}