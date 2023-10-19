using Entry;
using StaticScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �C������
/// </summary>
namespace GameScene
{
    /// <summary>
    /// �g���D
    /// </summary>
    public class SlitherMediator : MonoBehaviour
    {
        ObjectPool objectPool;//�����
        AudioManager audioManager;//���ĺ޲z
        Language language;//�y��½Ķ
        SlitherLobbyMediator slitherLobby;//�g���D�j�U

        [Header("�w�]���")]
        [Tooltip("��l����ƶq")] readonly int initBodyCount = 7;
        [Tooltip("����j�p")] readonly int bodySize = 20;
        [Tooltip("������W��")]
        readonly string[] objPoolName =
{
            "SlitherBody",//�g���D����
        };

        [Header("�ާ@����")]
        [Tooltip("�ާ@����")] public GameObject Control_Obj;
        [Tooltip("����_�W")] [SerializeField] Button up_Btn;
        [Tooltip("����_�U")] [SerializeField] Button down_Btn;
        [Tooltip("����_��")] [SerializeField] Button left_Btn;
        [Tooltip("����_�k")] [SerializeField] Button right_Btn;

        [Header("����")]
        [Tooltip("���I������/���骫�������")] [SerializeField] RectTransform gridBg_Img;
        [Tooltip("���骫��˥�")] [SerializeField] RectTransform bodySample;
        [Tooltip("��������˥�")] [SerializeField] RectTransform foodSample;

        [Header("���")]
        [Tooltip("���X�ƶq")] [SerializeField] int gridSizeX;
        [Tooltip("���Y�ƶq")] [SerializeField] int gridSizeY;
        [Tooltip("�������骫��")] [SerializeField] List<RectTransform> bodyList = new List<RectTransform>();
        [Tooltip("�O������e�Ӧ�m")] [SerializeField] List<Vector2> previousPosList = new List<Vector2>();
        [Tooltip("���ʳt�ת�(��)")][SerializeField] float[] speedList;
        [Tooltip("�t�׼W�[�������ƶq")][SerializeField] int[] addSpeedFood;

        [Header("�����")]
        [Tooltip("�����(����W��,�s��)")] Dictionary<string, int> objectPoolDic = new Dictionary<string, int>();

        [Header("����")]
        [Tooltip("�O�_�}�l�C��")] public bool isGameStart;
        [Tooltip("�O�_����")] [SerializeField] bool isThrough;
        [Tooltip("���ʤ�V")] Vector2 moveDirection;
        [Tooltip("���Y���骫��")] RectTransform leaderBody;
        [Tooltip("���ʧ�s�p�ɾ�")][SerializeField] float moveRefreshTime;
        [Tooltip("�ثe���ʳt��")][SerializeField] float nowMoveSpeed;

        [Header("����")]
        [Tooltip("�Τ���Ƥ�r")] [SerializeField] Text userScore_Txt;
        [Tooltip("���Ƥ�r")] [SerializeField] Text ScoreTitle_Txt;
        [Tooltip("�Τ����")] int userScore;

        private void Awake()
        {
            objectPool = ObjectPool.Instance;//�����
            audioManager = AudioManager.Instance;//���ĺ޲z
            language = Language.Instance;//�y��½Ķ
            slitherLobby = FindObjectOfType<SlitherLobbyMediator>();

            //���
            gridSizeX = (int)(gridBg_Img.sizeDelta.x / bodySize) - 1;//���X�ƶq
            gridSizeY = (int)(gridBg_Img.sizeDelta.y / bodySize) - 1;//���Y�ƶq
            
            int levelCount = 6;//���Ŷ��q�ƶq
            speedList = new float[levelCount];
            addSpeedFood = new int[levelCount];
            for (int i = 0; i < speedList.Length; i++)
            {
                speedList[i] = 0.09f - (i * 0.01f);//���ʳt�ת�(��)
                addSpeedFood[i] = (gridSizeX * gridSizeY) / ((speedList.Length - i) + 1);
            }

            Control_Obj.SetActive(false);//�����

            //½Ķ
            ScoreTitle_Txt.text = language.GetLanguage("Score");//����

            CreateObjectPool();//�Ыت����
            EventListener();//�ƥ��ť
        }

        /// <summary>
        /// �ƥ��ť
        /// </summary>
        void EventListener()
        {
            //�W���s
            up_Btn.onClick.AddListener(() =>
            {
                OnControlBtnClick(new Vector2(0, 1));//��V���s���U
            });
            //�U���s
            down_Btn.onClick.AddListener(() =>
            {
                OnControlBtnClick(new Vector2(0, -1));//��V���s���U
            });
            //�����s
            left_Btn.onClick.AddListener(() =>
            {
                OnControlBtnClick(new Vector2(-1, 0));//��V���s���U
            });
            //�k���s
            right_Btn.onClick.AddListener(() =>
            {
                OnControlBtnClick(new Vector2(1, 0));//��V���s���U
            });
        }

        private void Start()
        {
            CreateInitBody();//���ͪ�l����
        }

        private void Update()
        {
            OnInputControl();//��J����
            SnakeMove();//�g���D����
        }

        /// <summary>
        /// ��J����
        /// </summary>
        void OnInputControl()
        {
            //�W
            if (Input.GetKeyDown(KeyCode.W))
            {
                OnControlBtnClick(new Vector2(0, 1));//��V���s���U
            }
            //�U
            if (Input.GetKeyDown(KeyCode.S))
            {
                OnControlBtnClick(new Vector2(0, -1));//��V���s���U
            }
            //��
            if (Input.GetKeyDown(KeyCode.A))
            {
                OnControlBtnClick(new Vector2(-1, 0));//��V���s���U
            }
            //�k
            if (Input.GetKeyDown(KeyCode.D))
            {
                OnControlBtnClick(new Vector2(1, 0));//��V���s���U
            }
        }

        /// <summary>
        /// ��V���s���U
        /// </summary>
        /// <param name="dir">����V</param>
        void OnControlBtnClick(Vector2 dir)
        {
            audioManager.OnPlayButtonClickSound();//������s�I������

            //����Ϥ�V/�ۦP��V����
            if ((dir.x != 0 && moveDirection.x != 0) || (dir.y != 0 && moveDirection.y != 0))
            {
                return;
            }

            moveRefreshTime = 0;//���s�p�ɾ�
            moveDirection = dir;//���ʤ�V
            SnakeMove();//�g���D����
        }

        /// <summary>
        /// �g���D����
        /// </summary>
        void SnakeMove()
        {
            //�C���}�l && �����骫��
            if (isGameStart && bodyList.Count > 0)
            {
                moveRefreshTime -= Time.deltaTime;
                if (moveRefreshTime <= 0)
                {
                    //���s�p�ɾ�
                    moveRefreshTime = nowMoveSpeed;

                    //���Y�ǳƲ��ʦ�m
                    float posX = leaderBody.anchoredPosition.x + (bodySize * moveDirection.x);
                    float posY = leaderBody.anchoredPosition.y + (bodySize * moveDirection.y);
                    Vector2 preparePos = new Vector2(posX, posY);

                    //��������
                    if (isThrough == false)
                    {
                        if (IsMoveOutside(preparePos) != 0)
                        {
                            slitherLobby.OnGameOver(false);//��ܹC�������e��
                            return;
                        }
                        else leaderBody.anchoredPosition = preparePos;
                    }
                    else
                    {
                        int sideX = (gridSizeX / 2) * bodySize;//���̥~��X
                        int sideY = (gridSizeY / 2) * bodySize;//���̥~��Y
                        switch (IsMoveOutside(preparePos))
                        {
                            case 0://�S�����ʨ�~��
                                leaderBody.anchoredPosition = preparePos;
                                break;
                            case 1://�W�X�W��
                                leaderBody.anchoredPosition = new Vector2(leaderBody.anchoredPosition.x, -sideY);
                                return;
                            case 2://�W�X�U��
                                leaderBody.anchoredPosition = new Vector2(leaderBody.anchoredPosition.x, sideY);
                                return;
                            case 3://�W�X����
                                leaderBody.anchoredPosition = new Vector2(sideX, leaderBody.anchoredPosition.y);
                                return;
                            case 4://�W�X�k��
                                leaderBody.anchoredPosition = new Vector2(-sideX, leaderBody.anchoredPosition.y);
                                return;
                        }
                    }

                    //���鲾��
                    for (int i = 1; i < bodyList.Count; i++)
                    {
                        //��m����e�@�Ө����m
                        bodyList[i].anchoredPosition = previousPosList[i - 1];
                    }

                    //��s��m
                    for (int i = 0; i < bodyList.Count; i++)
                    {
                        previousPosList[i] = bodyList[i].anchoredPosition;
                    }

                    //�˴��O�_�I�쨭��
                    for (int i = 1; i < previousPosList.Count; i++)
                    {
                        if (leaderBody.anchoredPosition == previousPosList[i])
                        {
                            slitherLobby.OnGameOver(false);//��ܹC�������e��
                            return;
                        }
                    }    

                    //�Y�쭹��
                    if (leaderBody.anchoredPosition == foodSample.anchoredPosition)
                    {
                        EatFood();
                    }
                }
            }
        }

        /// <summary>
        /// �O�_���ʨ�~��
        /// </summary>
        /// <param name="pos">�P�_��Vector2</param>
        /// <returns></returns>
        int IsMoveOutside(Vector2 pos)
        {
            int sideX = (gridSizeX / 2) * bodySize;//���̥~��X
            int sideY = (gridSizeY / 2) * bodySize;//���̥~��Y

            if (pos.y > sideY) return 1;//�W�X�W��
            else if (pos.y < -sideY) return 2;//�W�X�U��
            else if (pos.x < -sideX) return 3;//�W�X����
            else if (pos.x > sideX) return 4; //�W�X�k��

            return 0;//�S�����ʨ�~��
        }    

        /// <summary>
        /// �Ыت����
        /// </summary>
        void CreateObjectPool()
        {
            //������s��_����
            int num = objectPool.OnCreateAndRecordObject(bodySample.gameObject, gridBg_Img);
            objectPoolDic.Add(objPoolName[0], num);
        }

        /// <summary>
        /// �M�z�����
        /// </summary>
        public void CleanObjectPool()
        {
            objectPool.OnCleanPool(objectPoolDic);
        }

        /// <summary>
        /// �C����l��
        /// </summary>
        /// <param name="isThrough">�O�_����</param>
        public void OnInitGame(bool isThrough)
        {
            this.isThrough = isThrough;

            //����C��
            if (gridBg_Img.TryGetComponent<Image>(out Image img))
            {
                img.color = isThrough == false ? Color.red : Color.green;
            }

            moveDirection = new Vector2(1, 0);//�w�]���ʤ�V
            nowMoveSpeed = speedList[0];//�w�]�t��

            //���������R��_����
            objectPool.OnDestroyObject(objectPool.SerchObjId(objPoolName[0] ,objectPoolDic));
            bodyList = new List<RectTransform>();
            previousPosList = new List<Vector2>();

            //����
            userScore = 0;
            userScore_Txt.text = userScore.ToString();

            CreateInitBody();//���ͪ�l����
        }

        /// <summary>
        /// ���ͪ�l����
        /// </summary>
        void CreateInitBody()
        {
            bodySample.gameObject.SetActive(false);//���骫��˥�

            //���ͨ���
            for (int i = 0; i < initBodyCount; i++)
            {
                RectTransform obj = CreateBody();//���ͨ���
                obj.anchoredPosition = new Vector2(0 - (bodySize * i), 0);//��m
                previousPosList.Add(obj.anchoredPosition);//������m
                //���Y���骫��
                if (i == 0) leaderBody = obj;
            }
        }

        /// <summary>
        /// ���ͨ���
        /// </summary>
        RectTransform CreateBody()
        {
            int poolNum = objectPool.SerchObjId(objPoolName[0], objectPoolDic);//�j�������ID
            RectTransform obj = objectPool.OnActiveObject(poolNum, gridBg_Img).GetComponent<RectTransform>();
            obj.sizeDelta = new Vector2(bodySize, bodySize);//�j�p
            obj.SetParent(gridBg_Img);
            bodyList.Add(obj);//��������
            
            return obj;
        }

        /// <summary>
        /// �Y�쭹��
        /// </summary>
        void EatFood()
        {
            //����o������
            audioManager.OnPlayScoreSound();

            //����
            userScore++;
            userScore_Txt.text = userScore.ToString();

            //���(���� >= (�������X * �������Y) - ��l����ƶq-1)
            if (userScore >= (gridSizeX * gridSizeY) - (initBodyCount - 1))
            {
                //��ܹC�������e��
                foodSample.transform.position = new Vector2(0 - foodSample.sizeDelta.x, 0 - foodSample.sizeDelta.y);//�������X�e��
                slitherLobby.OnGameOver(true);//��ܹC�������e��
                return;
            }

            //�[�t
            for (int i = 0; i < addSpeedFood.Length; i++)
            {
                //���ƹF�W�[�t�׼з� && ���b(���W�L�t�צC��)
                if (userScore >= addSpeedFood[i] && i < speedList.Length)
                {
                    nowMoveSpeed = speedList[i];
                }
            }
            
            //���ͨ���
            RectTransform obj = CreateBody();//���ͨ���
            obj.anchoredPosition = bodyList[bodyList.Count - 1].anchoredPosition;//�b�̫ᨭ���m
            previousPosList.Add(obj.anchoredPosition);//������m

            //��s������m
            RefreshFoodPos();
        }


        /// <summary>
        /// ��s������m
        /// </summary>
        public void RefreshFoodPos()
        {
            //�����m�C��
            List<Vector2> gridPosList = new List<Vector2>();
            for (int i = 0; i < gridSizeX; i++)
            {
                for (int j = 0; j < gridSizeY; j++)
                {
                    int side = (int)(-gridBg_Img.sizeDelta.x / 2) + bodySize;//�����m
                    Vector2 pos = new Vector2(side + (i * bodySize), side + (j * bodySize));
                    gridPosList.Add(pos);
                }
            }

            for (int i = 0; i < bodyList.Count; i++)
            {
                //�����ثe�D���Ҧ���m
                gridPosList.Remove(bodyList[i].anchoredPosition);
            }

            foodSample.anchoredPosition = gridPosList[Random.Range(0, gridPosList.Count)];
        }
    }
}