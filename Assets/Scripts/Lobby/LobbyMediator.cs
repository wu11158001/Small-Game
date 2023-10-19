using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StaticScripts;
using Entry;

/// <summary>
/// �j�U
/// </summary>
namespace Lobby
{
    /// <summary>
    /// �j�U
    /// </summary>
    public class LobbyMediator : MonoBehaviour
    {
        GameSceneManager gameSceneManager;//�����޲z
        AudioManager audioManager;//���ĺ޲z
        Language language;//�y��½Ķ

        [Tooltip("���خe��")] [SerializeField] RectTransform Content;
        [Tooltip("���ؼ˥�")] [SerializeField] RectTransform itemSample;

        [Tooltip("���ض��Z")] readonly int spacing = 200;
        [Tooltip("���ئW�٤���List")] [SerializeField] Text[] itemNmaesObj;

        [Tooltip("������s")] [SerializeField] Button chinese_Btn;
        [Tooltip("�^����s")] [SerializeField] Button english_Btn;
        [Tooltip("���֫��s")] [SerializeField] Button music_Btn;
        [Tooltip("���ī��s")] [SerializeField] Button sound_Btn;

        [Tooltip("���֭��ĹϤ���")] [SerializeField] Sprite[] musicSprites;//0.1=����, 1.2=����
        [Tooltip("���ֹϤ�����")] [SerializeField] Image music_Img;
        [Tooltip("���ĹϤ�����")] [SerializeField] Image sound_Img;

        //�C�����d
        readonly public string[] itemNames = { "NumberGame", "Slither", "Tetris" };//�C�����ئW��(2048, �g�Y�D, �Xù�����)

        private void Awake()
        {
            gameSceneManager = GameSceneManager.Instance;//�����޲z
            audioManager = AudioManager.Instance;//���ĺ޲z
            language = Language.Instance;//�y��½Ķ

            itemNmaesObj = new Text[itemNames.Length];

            SetMusicSprite();//�]�m���ֹϤ�
            EventListener();//�ƥ��ť
        }

        /// <summary>
        /// �ƥ��ť
        /// </summary>
        void EventListener()
        {
            //������s
            chinese_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������
                language.langIndex = 0;
                OnChangeLanguage();//�󴫻y��
            });
            //�^����s
            english_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������
                language.langIndex = 1;
                OnChangeLanguage();//�󴫻y��
            });
            //���֫��s
            music_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������
                audioManager.OnMusicSwitch();//���ֶ}��
                SetMusicSprite();//�]�m���ֹϤ�
            });
            //���ī��s
            sound_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������
                audioManager.OnSoundSwitch();//���Ķ}��
                SetMusicSprite();//�]�m���ֹϤ�
            });
        }

        private void Start()
        {
            CreateItems();//���Ͷ���
        }

        /// <summary>
        /// �]�m���ֹϤ�
        /// </summary>
        void SetMusicSprite()
        {
            //����
            music_Img.sprite = audioManager.musicSwitch[0] == true ? musicSprites[0] : musicSprites[1];
            //����
            sound_Img.sprite = audioManager.musicSwitch[1] == true ? musicSprites[2] : musicSprites[3];
        }

        /// <summary>
        /// �󴫻y��
        /// </summary>
        void OnChangeLanguage()
        {
            PlayerPrefs.SetInt(language.serchLanguageStr, language.langIndex);
            for (int i = 0; i < itemNmaesObj.Length; i++)
            {
                itemNmaesObj[i].text = language.GetLanguage(itemNames[i]);
            }
        }

        /// <summary>
        /// ���Ͷ���
        /// </summary>
        void CreateItems()
        {
            //���b
            if (Content == null || itemSample == null) return;

            //����˥�sizeDelta
            float sizeX = itemSample.sizeDelta.x;
            float sizeY = itemSample.sizeDelta.y;

            //Layout�]�w
            VerticalLayoutGroup layout = Content.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 500, spacing);
            layout.spacing = spacing;

            itemSample.gameObject.SetActive(false);
            for (int i = 0; i < itemNames.Length; i++)
            {
                //���ث��s
                Button itemButton = Instantiate(itemSample, Content).GetComponent<Button>();
                itemButton.gameObject.SetActive(true);
                int index = i;
                itemButton.onClick.AddListener(() =>
                {
                    audioManager.OnPlayButtonClickSound();//������s�I������
                    SetItemButtonEvent(index);//�]�w���ث��s�ƥ�
                });
                //���ئW��
                Text itemNmae = Utials.FindChild<Text>(itemButton.transform, "ItemName");
                itemNmae.text = language.GetLanguage(itemNames[i]);
                itemNmaesObj[i] = itemNmae;
                Content.sizeDelta = new Vector2(sizeX, sizeY * (itemNames.Length + 1) + spacing);
            }
        }

        /// <summary>
        /// �]�w���ث��s�ƥ�
        /// </summary>
        /// <param name="index">����index</param>
        void SetItemButtonEvent(int index)
        {
            gameSceneManager.selectGameIndex = index;//��ܪ��C��Index
            gameSceneManager.OnLoadScene("GameScene");//�i�J�C������
        }
    }
}
