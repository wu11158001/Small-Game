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
    /// �g���D�j�U
    /// </summary>
    public class SlitherLobbyMediator : MonoBehaviour
    {
        GameSceneManager gameSceneManager;//�����޲z
        AudioManager audioManager;//���ĺ޲z
        Language language;//�y��½Ķ
        SlitherMediator slitherMediator;//�g���D

        [Header("����")]
        [Tooltip("�}�l���s")][SerializeField] Button start_Btn;
        [Tooltip("�}�l���s��r")] [SerializeField] Text start_Txt;
        [Tooltip("��^�j�U���s")] [SerializeField] Button home_Btn;

        [Tooltip("�C����������")] [SerializeField] GameObject gameOver_Obj;
        [Tooltip("�C������_��r")] [SerializeField] Text gameOver_Txt;

        [Tooltip("���𪫥�")] [SerializeField] GameObject through_Obj;
        [Tooltip("�����r")] [SerializeField] Text through_Txt;
        [Tooltip("������Tog")] [SerializeField] Toggle through_Tog;

        [Tooltip("��e����Canvas")] Canvas canvas;

        private void Awake()
        {
            gameSceneManager = GameSceneManager.Instance;//�����޲z
            audioManager = AudioManager.Instance;//���ĺ޲z
            language = Language.Instance;//�y��½Ķ

            canvas = FindObjectOfType<Canvas>();//��e����Canvas

            //½Ķ
            start_Txt.text = language.GetLanguage("Start");//�}�l
            through_Txt.text = language.GetLanguage("IsThrough");//����

            gameOver_Obj.SetActive(false);//�C����������

            EventListener();//�ƥ��ť
        }

        private void Start()
        {
            //���͹C��
            if (Utials.OpenView("SlitherView").TryGetComponent<SlitherMediator>(out SlitherMediator slither))
            {
                slitherMediator = slither;
            }
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
                slitherMediator.CleanObjectPool();//�M�z�����
                gameSceneManager.OnLoadScene("Lobby");
            });

            //�}�l���s
            start_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������

                //�g���D�C������
                if (slitherMediator != null)
                {
                    slitherMediator.OnInitGame(through_Tog.isOn);//�C����l��
                    slitherMediator.RefreshFoodPos();//��s������m

                    slitherMediator.isGameStart = true;//�C���}�l
                    slitherMediator.Control_Obj.SetActive(true);//�ާ@����
                    through_Obj.SetActive(false);//���𪫥�
                    gameOver_Obj.SetActive(false);//�C����������
                    start_Btn.gameObject.SetActive(false);//�}�l���s

                    //�]�mUI�h�Ŧ̤ܳU��
                    Utials.SetUIToFront(canvas.transform, transform);
                } 
            });

            //������Tog
            through_Tog.onValueChanged.AddListener((bool isSelect) =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������
            });
        }

        /// <summary>
        /// �C������
        /// </summary>
        /// <param name="isWin">�O�_���</param>
        public void OnGameOver(bool isWin)
        {
            //����C����������
            audioManager.OnPlayGameOverSound(isWin);

            if (slitherMediator != null)
            {
                slitherMediator.isGameStart = false;//��������
                slitherMediator.Control_Obj.SetActive(false);//�ާ@����
            }

            gameOver_Obj.SetActive(true);//�C����������
            start_Btn.gameObject.SetActive(true);//�}�l���s
            through_Obj.SetActive(true);//���𪫥�

            start_Txt.text = language.GetLanguage("ReStart");//���s�}�l
            gameOver_Txt.text = isWin == true ? "Win" : "Game Over";
        }
    }
}
