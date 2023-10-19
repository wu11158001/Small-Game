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
    /// �Ʀr�C��2048�j�U
    /// </summary>
    public class NumberGameLobbyMediator : MonoBehaviour
    {
        GameSceneManager gameSceneManager;//�����޲z
        AudioManager audioManager;//���ĺ޲z
        Language language;//�y��½Ķ

        [Tooltip("���d�j�p")] public readonly int[] level = { 3, 4, 5, 6, 8 };

        [Tooltip("��ܪ����d")] public int numberGameLevel;

        [Header("����")]
        [Tooltip("�����s")] [SerializeField] Button left_Btn;
        [Tooltip("�k���s")] [SerializeField] Button right_Btn;
        [Tooltip("���d��r")] [SerializeField] Text level_Txe;
        [Tooltip("�}�l���s")] [SerializeField] Button start_Btn;
        [Tooltip("�}�l���s��r")] [SerializeField] Text start_Txt;
        [Tooltip("��^�j�U���s")] [SerializeField] Button home_Btn;

        private void Awake()
        {
            gameSceneManager = GameSceneManager.Instance;//�����޲z
            audioManager = AudioManager.Instance;//���ĺ޲z
            language = Language.Instance;//�y��½Ķ

            numberGameLevel = 1;//�w�]���d=1

            //½Ķ
            start_Txt.text = language.GetLanguage("Start");//�}�l

            EventListener();//�ƥ��ť
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
                gameSceneManager.OnLoadScene("Lobby");
            });
            //�����s
            left_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������
                ChangeLevel(-1);
            });
            //�k���s
            right_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������
                ChangeLevel(1);
            });
            //�}�l���s
            start_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������
                gameObject.SetActive(false);
                if(Utials.OpenView("NumberGameView").TryGetComponent<NumberGameMediator>(out NumberGameMediator numberGameMediator))
                {
                    numberGameMediator.SetGride(level[numberGameLevel], numberGameLevel);//�]�w���
                }
            });
        }

        private void Start()
        {
            ChangeLevel(0);//�w�]���d
        }

        /// <summary>
        /// �����d
        /// </summary>
        /// <param name="select">1=�W/-1=��</param>
        void ChangeLevel(int select)
        {
            //���b
            if (select >= 1) select = 1;
            else if (select <= -1) select = -1;

            //��ܪ����dindex
            numberGameLevel += select;
            if (numberGameLevel > level.Length - 1) numberGameLevel = 0;
            else if (numberGameLevel < 0) numberGameLevel = level.Length - 1;
            //���d��r
            level_Txe.text = level[numberGameLevel] + " X " + level[numberGameLevel];

        }
    }
}