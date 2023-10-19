using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// �C���ҰʤJ�f
/// </summary>
namespace Entry
{
    /// <summary>
    /// ��J����
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        static InputManager inputManager;
        public static InputManager Instance => inputManager;

        [Header("�P�_�O�_�bUI�W")]
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
        /// ���s�P�_UI�W��Component
        /// </summary>
        public void InitOnUIElementComponent()
        {
            init = false;
            graphicRaycaster = null;
            eventSystem = null;
            eventData = null;
        }

        //�M���e����eventSystem
        void FindEventSystem()
        {
            if (!init)
            {
                //�ھڶ��ا�canvas�W��GraphicRaycaster
                graphicRaycaster = GameObject.Find("Canvas").GetComponent<GraphicRaycaster>();
                //��e����eventSystem 
                eventSystem = EventSystem.current;
                eventData = new PointerEventData(eventSystem);
                init = true;
            }
        }
        /// <summary>
        /// �O�_�bUI�W
        /// </summary>
        /// <returns></returns>
        public bool IsOnUIElement()
        {
            //�M���e����eventSystem
            FindEventSystem();
            if (graphicRaycaster == null)
            {
                Debug.Log("����GraphicRaycaster");
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