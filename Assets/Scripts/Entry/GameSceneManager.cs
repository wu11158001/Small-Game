using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 遊戲啟動入口
/// </summary>
namespace Entry
{
    /// <summary>
    /// 場景管理
    /// </summary>
    public class GameSceneManager : MonoBehaviour
    {
        static GameSceneManager sceneManager;
        public static GameSceneManager Instance => sceneManager;

        AudioManager audioManager;//音效管理
        [SerializeField]InputManager inputManager;//輸入控制

        [Tooltip("當前場景")] public string currentSceneName;
        [Tooltip("遊戲場景資料夾名稱")] public string gameFolder;
        [Tooltip("選擇的遊戲Index")]public int selectGameIndex;

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
            audioManager = AudioManager.Instance;//音效管理
            inputManager = InputManager.Instance;//輸入控制
        }

        /// <summary>
        /// 轉換場景重製的資料
        /// </summary>
        void OnInitData()
        {
            //重製判斷UI上的Component
            if (inputManager) inputManager.InitOnUIElementComponent();
        }

        /// <summary>
        /// 載入場景
        /// </summary>
        /// <param name="sceneName"></param>
        public void OnLoadScene(string sceneName)
        {
            //載入場景
            StartCoroutine(ILoadScene(sceneName));
        }

        /// <summary>
        /// 載入場景
        /// </summary>
        /// <param name="sceneName">場景名稱</param>
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
                    audioManager.OnCleanObjPoolForAudio();//清理物件池_音訊
                    //創建場景初始UI
                    CreateInitUI(sceneName);
                    //轉換場景重製的資料                        
                    OnInitData();
                    yield break;
                }
                yield return null;
            }
        }

        /// <summary>
        /// 創建場景初始UI
        /// </summary>
        /// <param name="sceneName">場景名稱</param>
        void CreateInitUI(string sceneName)
        {
            currentSceneName = SceneManager.GetActiveScene().name;
            switch (currentSceneName)
            {
                case "Lobby"://大廳
                    gameFolder = "";
                    StaticScripts.Utials.OpenView("LobbyView");
                    break;
                case "GameScene"://遊戲場景
                    JudgeGameScene();//判斷進入遊戲場景
                    break;
            }
        }

        /// <summary>
        /// 判斷進入遊戲場景
        /// </summary>
        void JudgeGameScene()
        {
            switch(selectGameIndex)
            {
                case 0://2048
                    gameFolder = "NumberGame/";
                    StaticScripts.Utials.OpenView("NumberGameLobbyView");
                    break;
                case 1://貪食蛇
                    gameFolder = "Slither/";
                    StaticScripts.Utials.OpenView("SlitherLobbyView");
                    break;
                case 2://俄羅斯方塊
                    gameFolder = "Tetris/";
                    StaticScripts.Utials.OpenView("TetrisLobbyView");
                    break;
            }
        }
    }
}
