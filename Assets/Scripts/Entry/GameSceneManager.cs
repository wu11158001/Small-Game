using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// �C���ҰʤJ�f
/// </summary>
namespace Entry
{
    /// <summary>
    /// �����޲z
    /// </summary>
    public class GameSceneManager : MonoBehaviour
    {
        static GameSceneManager sceneManager;
        public static GameSceneManager Instance => sceneManager;

        AudioManager audioManager;//���ĺ޲z
        [SerializeField]InputManager inputManager;//��J����

        [Tooltip("��e����")] public string currentSceneName;
        [Tooltip("�C��������Ƨ��W��")] public string gameFolder;
        [Tooltip("��ܪ��C��Index")]public int selectGameIndex;

        private void Awake()
        {
            if (sceneManager != null)
            {
                Destroy(this);
                return;
            }
            sceneManager = this;
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            audioManager = AudioManager.Instance;//���ĺ޲z
            inputManager = InputManager.Instance;//��J����
        }

        /// <summary>
        /// �ഫ�������s�����
        /// </summary>
        void OnInitData()
        {
            //���s�P�_UI�W��Component
            if (inputManager) inputManager.InitOnUIElementComponent();
        }

        /// <summary>
        /// ���J����
        /// </summary>
        /// <param name="sceneName"></param>
        public void OnLoadScene(string sceneName)
        {
            //���J����
            StartCoroutine(ILoadScene(sceneName));
        }

        /// <summary>
        /// ���J����
        /// </summary>
        /// <param name="sceneName">�����W��</param>
        /// <returns></returns>
        IEnumerator ILoadScene(string sceneName)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;
            while (true)
            {
                if (op.progress >= 0.9f)
                {
                    op.allowSceneActivation = true;
                    yield return new WaitForSeconds(0.1f);
                    audioManager.OnCleanObjPoolForAudio();//�M�z�����_���T
                    //�Ыس�����lUI
                    CreateInitUI(sceneName);
                    //�ഫ�������s�����                        
                    OnInitData();
                    yield break;
                }
                yield return null;
            }
        }

        /// <summary>
        /// �Ыس�����lUI
        /// </summary>
        /// <param name="sceneName">�����W��</param>
        void CreateInitUI(string sceneName)
        {
            currentSceneName = SceneManager.GetActiveScene().name;
            switch (currentSceneName)
            {
                case "Lobby"://�j�U
                    gameFolder = "";
                    StaticScripts.Utials.OpenView("LobbyView");
                    break;
                case "GameScene"://�C������
                    JudgeGameScene();//�P�_�i�J�C������
                    break;
            }
        }

        /// <summary>
        /// �P�_�i�J�C������
        /// </summary>
        void JudgeGameScene()
        {
            switch(selectGameIndex)
            {
                case 0://2048
                    gameFolder = "NumberGame/";
                    StaticScripts.Utials.OpenView("NumberGameLobbyView");
                    break;
                case 1://�g���D
                    gameFolder = "Slither/";
                    StaticScripts.Utials.OpenView("SlitherLobbyView");
                    break;
                case 2://�Xù�����
                    gameFolder = "Tetris/";
                    StaticScripts.Utials.OpenView("TetrisLobbyView");
                    break;
            }
        }
    }
}
