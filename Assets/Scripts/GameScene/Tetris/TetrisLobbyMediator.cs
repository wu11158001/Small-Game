using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Entry;
using StaticScripts;

/// <summary>
/// �C������
/// </summary>
namespace GameScene
{
    /// <summary>
    /// �Xù������j�U
    /// </summary>
    public class TetrisLobbyMediator : MonoBehaviour
    {
        GameSceneManager gameSceneManager;//�����޲z
        AudioManager audioManager;//���ĺ޲z
        Language language;//�y��½Ķ
        TetrisMediator tetrisMediator;//�Xù�����

        [Header("�w�]���")]
        [Tooltip("�W�[����(0=���s�u,1~4�s�X���u)")]readonly int[] addScore =
        {
            50, 100, 300, 600, 800
        };
        [Tooltip("�ʵe�W��")]
        readonly string[] animatorName =
{
            "SpeedUp",//�t�׼W�[
        };
        [Tooltip("�ʵeHashId")] int[] animatorHashId;

        [Tooltip("��e����Canvas")] Canvas canvas;

        [Header("����")]
        [Tooltip("��^�j�U���s")] [SerializeField] Button home_Btn;
        [Tooltip("�}�l���s")] [SerializeField] Button start_Btn;
        [Tooltip("�}�l���s��r")] [SerializeField] Text start_Txt;
        [Tooltip("�C���}�l��������")] [SerializeField] GameObject GameStartEnable_Obj;

        [Header("��ܪ��t�׵���")]
        [Tooltip("��ܪ��t�׵���")] int selectSpeed;
        [Tooltip("��e�t�׵���")] public int currentSpeed;
        [Tooltip("�����[�t������")] int autoSpeedScore;
        [Tooltip("�t���ܧ���s(��)")] [SerializeField] Button selectSpeedLeft_Btn;
        [Tooltip("�t���ܧ���s(�k)")] [SerializeField] Button selectSpeedRight_Btn;
        [Tooltip("�t���ܧ󵥯Ť�r")] [SerializeField] Text selectSpeed_Txt;
        [Tooltip("��ܳt�׼��D��r")] [SerializeField] Text selectSpeedTitle_Txt;
        [Tooltip("�C����ܳt�׼��D��r")] [SerializeField] Text speedTitle_Txt;
        [Tooltip("�C����ܳt�פ�r")] [SerializeField] Text speedLevel_Txt;

        [Header("�C������")]
        [Tooltip("�C����������")] [SerializeField] GameObject gameOver_Obj;
        [Tooltip("�C��������r")] [SerializeField] Text gameOver_Txt;
        [Tooltip("�C���������s�}�l���s")] [SerializeField] Button gameOverReStart_Btn;
        [Tooltip("�C���������s�}�l��r")] [SerializeField] Text gameOverRestart_Txt;

        [Header("�u��")]
        [Tooltip("�u�㪫��")] [SerializeField] GameObject tool_Obj;
        [Tooltip("���s�}�l���s")] [SerializeField] Button reStart_Btn;
        [Tooltip("�^������d���s")] [SerializeField] Button selectLevel_Btn;
        [Tooltip("�Ȱ����s")] [SerializeField] Button pause_Btn;
        [Tooltip("�Ȱ����s�Ϥ�")] [SerializeField] Image pause_Img;
        [Tooltip("�Ȱ����s�Ϥ���(0=�Ȱ�,1=�~��)")] [SerializeField] Sprite[] pauseSprite;

        [Header("�ﶵ")]
        [Tooltip("�۰ʥ[�t��r")] [SerializeField] Text autoSpeed_Txt;
        [Tooltip("�۰ʥ[�tToggle")] [SerializeField] Toggle autoSpeed_Tog;
        [Tooltip("�����}����r")] [SerializeField] Text ghostPiece_Txt;
        [Tooltip("����Toggle")] [SerializeField] Toggle ghostPiece_Tog;

        [Header("����")]
        [Tooltip("�Τ���Ƥ�r")] [SerializeField] Text userScore_Txt;
        [Tooltip("���Ƥ�r")] [SerializeField] Text ScoreTitle_Txt;
        [Tooltip("�Τ����")] int userScore;

        [Tooltip("�O�_�Ȱ�")] public bool isPause;

        private void Awake()
        {
            gameSceneManager = GameSceneManager.Instance;//�����޲z
            audioManager = AudioManager.Instance;//���ĺ޲z
            language = Language.Instance;//�y��½Ķ

            canvas = FindObjectOfType<Canvas>();//��e����Canvas

            //�Τ����
            userScore = 0;
            userScore_Txt.text = userScore.ToString();

            //�Τ��ܳt��
            currentSpeed = 0;
            selectSpeed_Txt.text = (currentSpeed + 1).ToString();

            //½Ķ
            start_Txt.text = language.GetLanguage("Start");//�}�l
            gameOverRestart_Txt.text = language.GetLanguage("ReStart");//���s�}�l
            ScoreTitle_Txt.text = language.GetLanguage("Score");//����
            speedTitle_Txt.text = language.GetLanguage("Speed");//�t��
            autoSpeed_Txt.text = language.GetLanguage("AutoSpeed");//�۰ʥ[�t
            ghostPiece_Txt.text = language.GetLanguage("GhostPiece");//����
            selectSpeedTitle_Txt.text = language.GetLanguage("SpeedLevelTitle");//�t�׵���

            pause_Img.sprite = isPause ? pauseSprite[1] : pauseSprite[0];
            tool_Obj.SetActive(false);//�u�㪫��
            gameOver_Obj.SetActive(false);//�C����������

            EventListener();//�ƥ��ť
        }

        private void Start()
        {
            //���͹C��
            if (Utials.OpenView("TetrisView").TryGetComponent<TetrisMediator>(out TetrisMediator tetris))
            {
                tetrisMediator = tetris;
            }
            
            //�]�mUI�h�Ŧ̤ܳU��
            Utials.SetUIToFront(canvas.transform, transform);
        }

        /// <summary>
        /// �ƥ��ť
        /// </summary>
        void EventListener()
        {
            //��^�j�U���s
            home_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������

                isPause = false;
                OnPause();//�Ȱ�

                tetrisMediator.objectPool.OnCleanPool(tetrisMediator.objectPoolDic);//�M�z�����
                gameSceneManager.OnLoadScene("Lobby");
            });

            //�}�l���s
            start_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������
                if (tetrisMediator != null)
                {
                    GameStartEnable_Obj.gameObject.SetActive(false);//�C���}�l��������
                    tool_Obj.SetActive(true);//�u�㪫��
                    StartCoroutine(tetrisMediator.ICreateGrid());//�Ыؤ��
                    selectSpeed = currentSpeed;//�����ҿ�t�׵���
                }
            });

            //�C���������s�}�l���s
            gameOverReStart_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������
                OnReStart();//���s�}�l�C��
            });

            //�Ȱ����s
            pause_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������

                isPause = !isPause;
                OnPause();//�Ȱ�
            });

            //�^������d���s
            selectLevel_Btn.onClick.AddListener(() =>
            {
                //����Ҧ���{
                tetrisMediator.StopThisAllCoroutines();

                audioManager.OnPlayButtonClickSound();//������s�I������

                isPause = false;
                OnPause();//�Ȱ�

                tetrisMediator.objectPool.OnCleanPool(tetrisMediator.objectPoolDic);//�M�z�����
                gameSceneManager.OnLoadScene("GameScene");
            });

            //���s�}�l���s
            reStart_Btn.onClick.AddListener(() =>
            {
                //����Ҧ���{
                tetrisMediator.StopThisAllCoroutines();

                audioManager.OnPlayButtonClickSound();//������s�I������
                OnReStart();//���s�}�l�C��
            });

            //�t���ܧ���s(��)
            selectSpeedLeft_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������
                //�󴫳t�׵���
                ChangeSpeedIndex(-1);
            });

            //�t���ܧ���s(�k)
            selectSpeedRight_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������
                //�󴫳t�׵���
                ChangeSpeedIndex(1);
            });

            //�۰ʥ[�t���Tog
            autoSpeed_Tog.onValueChanged.AddListener((bool isSelect) =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������
            });

            //����Tog
            ghostPiece_Tog.onValueChanged.AddListener((bool isSelect) =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������
            });
        }

        /// <summary>
        /// �]�w����
        /// </summary>
        /// <param name="line">�s�u�ƶq</param>
        public void SetScore(int line)
        {
            userScore += addScore[line];//�Τ����
            userScore_Txt.text = userScore.ToString();

            //�۰ʥ[�t�}��
            if (autoSpeed_Tog.isOn == true)
            {
                autoSpeedScore += addScore[line];
                if (autoSpeedScore / tetrisMediator.addSpeedScore >= 1 && currentSpeed < tetrisMediator.downLevelSpeed.Length - 1)
                {
                    autoSpeedScore -= tetrisMediator.addSpeedScore;//���s��������

                    //���񭵮�
                    audioManager.SetPlaySound("LevelUp");

                    //����ʵe
                    if (speedLevel_Txt.TryGetComponent<Animator>(out Animator animator))
                    {
                        animator.SetTrigger(Animator.StringToHash(animatorName[0]));
                    }

                    //�󴫳t�׵���
                    ChangeSpeedIndex(1);
                }
            }
        }

        /// <summary>
        /// �󴫳t�׵���
        /// </summary>
        /// <param name="select">�[/��</param>
        void ChangeSpeedIndex(int select)
        {
            //���b
            if (select > 1) select = 1;
            else if (select < -1) select = -1;

            currentSpeed += select;
            if (currentSpeed >= tetrisMediator.downLevelSpeed.Length) currentSpeed = tetrisMediator.downLevelSpeed.Length - 1;
            else if (currentSpeed < 0) currentSpeed = 0;

            string speedStr = (currentSpeed + 1).ToString(); ;
            selectSpeed_Txt.text = speedStr;//��ܳt�פ�r
            speedLevel_Txt.text = speedStr;//�C����ܳt�פ�r

            //�󴫳t��
            tetrisMediator.downSpeed = tetrisMediator.downLevelSpeed[currentSpeed];
        }

        /// <summary>
        /// �Ȱ�
        /// </summary>
        void OnPause()
        {
            Time.timeScale = isPause ? 0 : 1;
            pause_Img.sprite = isPause ? pauseSprite[1] : pauseSprite[0];
        }

        /// <summary>
        /// �C������
        /// </summary>
        /// <param name="isWin">�O�_���</param>
        public void OnGameOver(bool isWin)
        {
            audioManager.OnPlayGameOverSound(isWin);//���񭵮�
            tetrisMediator.isGameStart = false;
            gameOver_Obj.SetActive(true);//�C����������
            gameOver_Txt.text = isWin == true ? "Win" : "Game Over";//������r
        }

        //���s�}�l�C��
        void OnReStart()
        {
            tetrisMediator.isGameStart = true;
            gameOver_Obj.SetActive(false);//�C����������

            isPause = false;
            OnPause();//�Ȱ�

            //�Τ���ƭ��s
            userScore = 0;
            userScore_Txt.text = userScore.ToString();

            //�t�׵��ŭ��s
            currentSpeed = selectSpeed;
            speedLevel_Txt.text = (currentSpeed + 1).ToString();//�C����ܳt�פ�r

            //�󴫳t��
            tetrisMediator.downSpeed = tetrisMediator.downLevelSpeed[currentSpeed];

            //���s���ͤ��
            int poolNum = tetrisMediator.objectPool.SerchObjId(tetrisMediator.objPoolName[0], tetrisMediator.objectPoolDic);//�j�������ID
            tetrisMediator.objectPool.OnDestroyObject(poolNum);//�����������
            tetrisMediator.stopSquaresList.Clear();//�M������������
            tetrisMediator.CreateNewSquare();//�Ыطs���
        }

        /// <summary>
        /// ��������}�Ҫ��A
        /// </summary>
        public bool GetGhostPieceState
        {
            get
            {
                return ghostPiece_Tog.isOn;
            }
        }
    }
}