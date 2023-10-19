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
    /// �Ʀr�C��2048
    /// </summary>
    public class NumberGameMediator : MonoBehaviour
    {
        GameSceneManager gameSceneManager;//�����޲z
        ObjectPool objectPool;//�����
        AudioManager audioManager;//���ĺ޲z
        Language language;//�y��½Ķ

        [Header("�w�]���")]
        [Tooltip("��l����m")] readonly Vector2 initGridPos = new Vector2(-365, 365);
        [Tooltip("���j�p�C��")] readonly int[] squareSizeList = { 300, 220, 175, 148, 113 };
        [Tooltip("��涡�Z�C��")] readonly int[] spacingList = { 25, 23, 18, 12, 7 };
        [Tooltip("�w�]�Ʀr")] readonly int initNumber = 2;
        [Tooltip("��ӼƦr")] readonly int winNumber = 2048;
        [Tooltip("������ʳt��")] readonly float moveSpeed = 6000;
        [Tooltip("���ʧP�w�Z��")] readonly float judgeMoveDistance = 75;

        [Header("����")]
        [Tooltip("���˥�")][SerializeField] RectTransform gridSample;
        [Tooltip("���I���e��")][SerializeField] RectTransform gridContent;
        [Tooltip("���ͤ��������")][SerializeField] RectTransform CreateSquare_Obj;
        [Tooltip("����˥�")][SerializeField] GameObject squareSample;
        [Tooltip("�C����������")][SerializeField] GameObject gameOver_Obj;
        [Tooltip("�C������_���s�}�l���s")][SerializeField] Button gameOverReStart_Btn;
        [Tooltip("�C������_��r")][SerializeField] Text gameOver_Txt;
        [Tooltip("���s�}�l���s")][SerializeField] Button ReStart_Btn;
        [Tooltip("���s�}�l��r")] [SerializeField] Text gameOverReStart_Txt;
        [Tooltip("������d���s")][SerializeField] Button SelectLevel_Btn;
        [Tooltip("�^�j�U���s")][SerializeField] Button Home_Btn;

        [Header("���")]
        [Tooltip("����C��")][SerializeField] Color[] squareColors;
        [Tooltip("����C�ƶq")] int grideSize;
        [Tooltip("����`�ƶq")] int allSquareCount;
        [Tooltip("���j�p")] [SerializeField] int squareSize;
        [Tooltip("��涡�Z")] [SerializeField] int spacing;
        [Tooltip("���I����mList")] [SerializeField] List<Vector3> gridBgPosList = new List<Vector3>();
        [Tooltip("������")] [SerializeField] SquareData[] squareDatas;
        [Tooltip("��������")] [SerializeField] List<MoveSquare> MoveSquareList = new List<MoveSquare>();
       
        [Header("�����")]
        [Tooltip("�����(����W��,�s��)")] Dictionary<string, int> objectPoolDic = new Dictionary<string, int>();
        [Tooltip("��������")] objPoolData[] objPoolDatas;

        [Header("����")]
        [Tooltip("�Τ���Ƥ�r")] [SerializeField] Text userScore_Txt;
        [Tooltip("���Ƥ�r")] [SerializeField] Text ScoreTitle_Txt;
        [Tooltip("�Τ����")] int userScore;

        [Header("����")]
        [Tooltip("����Bool")] [SerializeField] bool isControl;
        [Tooltip("�_�lĲ�I�I")] Vector3 initTouchPos;
        [Tooltip("�O�_���b����")] [SerializeField] bool isMoving;

        [Header("�ʵe")]
        [Tooltip("�ʵe�W��")] readonly string[] animatorName = 
        {
            "Merge_Tr",//����X��
        };
        [Tooltip("�ʵeHashID")] int[] animatorHashId;

        private void Awake()
        {
            gameSceneManager = GameSceneManager.Instance;//�����޲z
            objectPool = ObjectPool.Instance;//�����
            audioManager = AudioManager.Instance;//���ĺ޲z
            language = Language.Instance;//�y��½Ķ

            //�ʵeHashID
            animatorHashId = new int[animatorName.Length];
            for (int i = 0; i < animatorName.Length; i++)
            {
                animatorHashId[i] = Animator.StringToHash(animatorName[i]);
            }

            //½Ķ
            gameOverReStart_Txt.text = language.GetLanguage("ReStart");//���s�}�l
            ScoreTitle_Txt.text = language.GetLanguage("Score");//����

            CreateObjectPool();//�Ыت����
            EventListener();//�ƥ��ť
        }

        /// <summary>
        /// �ƥ��ť
        /// </summary>
        void EventListener()
        {
            //�C������_���s�}�l���s
            gameOverReStart_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������
                OnReStare();//���s�C��
            });
            //�^�j�U���s
            Home_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������
                objectPool.OnCleanPool(objectPoolDic);//�M�z�����
                gameSceneManager.OnLoadScene("Lobby");
            });
            //������d���s
            SelectLevel_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������
                objectPool.OnCleanPool(objectPoolDic);//�M�z�����
                gameSceneManager.OnLoadScene("GameScene");
            });
            //���s�}�l���s
            ReStart_Btn.onClick.AddListener(() =>
            {
                audioManager.OnPlayButtonClickSound();//������s�I������
                OnReStare();//���s�C��
            });
        }

        private void Start()
        {
            CreateGridBgAndInitSquare();//�Ыؤ��I���P��l���
        }

        private void Update()
        {
            OnInputControl();//��J����
            SquareMove();//�������
        }

        /// <summary>
        /// �������
        /// </summary>
        void SquareMove()
        {
            if (isMoving)
            {
                for (int i = 0; i < MoveSquareList.Count; i++)
                {
                    Vector3 nowPos = MoveSquareList[i].obj.transform.position;//�ثe��m
                    Vector3 targetPos = gridBgPosList[MoveSquareList[i].posIndex];//�ؼЦ�m
                    MoveSquareList[i].obj.transform.position = Vector3.MoveTowards(nowPos, targetPos, moveSpeed * Time.deltaTime);
                }
            }
        }

        /// <summary>
        /// ��J����
        /// </summary>
        void OnInputControl()
        {
#if UNITY_EDITOR
            //�ƹ�����
            if (Input.GetMouseButtonDown(0)) initTouchPos = Input.mousePosition;
            if (Input.GetMouseButtonUp(0)) initTouchPos = Vector3.zero;
            if (isControl && !isMoving && initTouchPos != Vector3.zero)
            {
                if (Input.GetMouseButton(0))
                {
                    Vector3 movePos = Input.mousePosition;
                    //�P�_������ʤ�V
                    JudgeFingerDirection(movePos);
                }
            }
#elif UNITY_ANDROID
            if (Input.touchCount == 1)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began) initTouchPos = Input.mousePosition;
                if (Input.GetTouch(0).phase == TouchPhase.Canceled) initTouchPos = Vector3.zero;
                if (isControl && !isMoving && initTouchPos != Vector3.zero)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Moved)
                    {
                        Vector3 movePos = Input.GetTouch(Input.GetTouch(0).fingerId).position;
                        //�P�_������ʤ�V
                        JudgeFingerDirection(movePos);
                    }
                }
            }
#endif
        }

        /// <summary>
        /// �P�_������ʤ�V
        /// </summary>
        /// <param name="movePos"></param>
        void JudgeFingerDirection(Vector3 movePos)
        {
            float distanceX = movePos.x - initTouchPos.x;//���ʶZ��X
            float distanceY = movePos.y - initTouchPos.y;//���ʶZ��Y
            if (movePos.x > initTouchPos.x && Mathf.Abs(distanceX) > judgeMoveDistance && Mathf.Abs(distanceY) < judgeMoveDistance)//�V�k
            {
                SquareAction(0);//�����
            }
            else if (movePos.x < initTouchPos.x && Mathf.Abs(distanceX) > judgeMoveDistance && Mathf.Abs(distanceY) < judgeMoveDistance)//�V��
            {
                SquareAction(1);//�����
            }
            else if (movePos.y > initTouchPos.y && Mathf.Abs(distanceY) > judgeMoveDistance && Mathf.Abs(distanceX) < judgeMoveDistance)//�V�W
            {
                SquareAction(2);//�����
            }
            else if (movePos.y < initTouchPos.y && Mathf.Abs(distanceY) > judgeMoveDistance && Mathf.Abs(distanceX) < judgeMoveDistance)//�V�U
            {
                SquareAction(3);//�����
            }
        }

        /// <summary>
        /// �Ыت����
        /// </summary>
        void CreateObjectPool()
        {
            //�]�w������
            objPoolDatas = new objPoolData[]
            {
                new objPoolData(){ objName = "girdBg", obj = gridSample.gameObject, parentObj = gridContent },//���
                new objPoolData(){ objName = "square", obj = squareSample, parentObj = CreateSquare_Obj },//���
            };

            //���ͪ�l����
            int num;
            for (int i = 0; i < objPoolDatas.Length; i++)
            {
                num = objectPool.OnCreateAndRecordObject(objPoolDatas[i].obj, objPoolDatas[i].parentObj);
                objectPoolDic.Add(objPoolDatas[i].objName, num);
            }
        }

        /// <summary>
        /// �]�w���
        /// </summary>
        /// <param name="size">����C�ƶq</param>
        /// <param name="level">��ܪ����d�j�p</param>
        public void SetGride(int size, int level)
        {
            grideSize = size;//����C�ƶq
            allSquareCount = grideSize * grideSize;//����`�ƶq
            squareSize = squareSizeList[level];//���j�p
            spacing = spacingList[level];//��涡�Z

            //������
            squareDatas = new SquareData[allSquareCount];
            for (int i = 0; i < squareDatas.Length; i++)
            {
                squareDatas[i] = new SquareData();
            }
        }

        /// <summary>
        /// ���s�C��
        /// </summary>
        void OnReStare()
        {
            //�M�z���
            for (int i = 0; i < squareDatas.Length; i++)
            {
                CleanSquareData(i);//�M�z������
            }
            //�M�z�����
            for (int i = 0; i < objPoolDatas.Length; i++)
            {
                int serchId = objectPool.SerchObjId(objPoolDatas[i].objName, objectPoolDic);//�j�������ID
                objectPool.OnDestroyObject(serchId);//���������R��
            }

            CreateGridBgAndInitSquare();//�Ыؤ��I���P��l���
        }

        /// <summary>
        /// �Ыؤ��I���P��l���
        /// </summary>
        void CreateGridBgAndInitSquare()
        {
            gridSample.gameObject.SetActive(false);//���I���˥�
            squareSample.SetActive(false);//����˥�
            gameOver_Obj.SetActive(false);//�C����������

            //���b
            if (gridSample == null || gridContent == null) return;

            //�Ыؤ��I��
            gridBgPosList = new List<Vector3>();
            for (int i = 0; i < grideSize; i++)
            {
                for (int j = 0; j < grideSize; j++)
                {
                    //������E������
                    int serchId = objectPool.SerchObjId(objPoolDatas[0].objName, objectPoolDic);//�j�������ID
                    RectTransform obj = objectPool.OnActiveObject(serchId, objPoolDatas[0].parentObj).GetComponent<RectTransform>();
                    
                    //�w�]����j�p����m����
                    float initSize = squareSizeList[1];//�w�]����j�p
                    float size = (initSize - squareSize) / 2;
                    obj.sizeDelta = new Vector2(squareSize, squareSize);
                    obj.anchoredPosition = new Vector2((initGridPos.x - size) + ((squareSize + spacing) * j),
                                                        (initGridPos.y + size) - ((squareSize + spacing) * i));
                    gridBgPosList.Add(obj.position);
                }
            }

            //�Ыت�l���
            for (int i = 0; i < 2; i++)
            {
                CreateSquare();//�Ыؤ��
            }

            //�]�w����
            userScore = 0;
            SetScore(userScore);

            //�i�ާ@
            isControl = true;
        }

        /// <summary>
        /// �Ыؤ��
        /// </summary>
        void CreateSquare()
        {
            int count = 0;//���b
            bool isCreate = false;
            while (!isCreate)
            {
                //�üƦ�m
                List<int> emptyList = new List<int>();
                for (int i = 0; i < squareDatas.Length; i++)
                {
                    if (squareDatas[i].number == 0)
                    {
                        emptyList.Add(i);
                    }
                }
                
                //���b
                if (emptyList.Count == 0)
                {
                    SetGameResult(false);//�]�w�C�����G
                    return;
                }

                int posIndex = Random.Range(0, emptyList.Count);
                //�Ӧ�m����
                if (squareDatas[emptyList[posIndex]].number == 0)
                {
                    isCreate = true;
                    //������E������
                    int serchId = objectPool.SerchObjId(objPoolDatas[1].objName, objectPoolDic);//�j�������ID_���ID
                    RectTransform square = objectPool.OnActiveObject(serchId, objPoolDatas[1].parentObj).GetComponent<RectTransform>();
                    square.sizeDelta = new Vector2(squareSize, squareSize);
                    square.position = gridBgPosList[emptyList[posIndex]];

                    //�]�w�Ʀr
                    squareDatas[emptyList[posIndex]].imageObj = square.transform.GetComponentInChildren<Image>();
                    squareDatas[emptyList[posIndex]].imageObj.color = squareColors[0];
                    squareDatas[emptyList[posIndex]].textObj = square.transform.GetComponentInChildren<Text>();
                    squareDatas[emptyList[posIndex]].textObj.text = initNumber.ToString();
                    squareDatas[emptyList[posIndex]].animator = square.transform.GetComponentInChildren<Animator>();
                    squareDatas[emptyList[posIndex]].number = initNumber;//�w�]��
                    squareDatas[emptyList[posIndex]].obj = square;
                }

                if (emptyList.Count == 1) JudgeIsFial();//�P�_�O�_����

                //���b
                count++;
                if (count > 2)
                {
                    Debug.LogError("�X���F");
                    isCreate = true;
                }
            }
        }

        /// <summary>
        /// �]�w����
        /// </summary>
        /// <param name="addScore">�W�[����</param>
        void SetScore(int addScore)
        {
            userScore += addScore;
            userScore_Txt.text = userScore.ToString();
        }

        /// <summary>
        /// �P�_�O�_����
        /// </summary>
        void JudgeIsFial()
        {
            for (int i = 0; i < squareDatas.Length; i++)
            {
                //���Ʀr
                if (squareDatas[i].number > 0)
                {
                    //�P�_�k��(i < �`�ƶq && �D�̥k�� && �Ʀr�ۦP)
                    if (i + 1 < allSquareCount - 1 && (i + 1) % grideSize != 0 && squareDatas[i + 1].number == squareDatas[i].number) return;
                    //�P�_����(i > 0 && �D�̥��� && �Ʀr�ۦP)
                    if (i - 1 > 0 && (i + grideSize) % grideSize != 0 && squareDatas[i - 1].number == squareDatas[i].number) return;
                    //�P�_�W��(�D�̤W�� && �Ʀr�ۦP)
                    if (i >= grideSize && squareDatas[i - grideSize].number == squareDatas[i].number) return;
                    //�P�_�U��(�D�̤U�� && �Ʀr�ۦP)
                    if (i < allSquareCount - grideSize && squareDatas[i + grideSize].number == squareDatas[i].number) return;
                }
            }

            SetGameResult(false);//�]�w�C�����G����
        }

        /// <summary>
        /// �]�w�C�����G
        /// </summary>
        /// <param name="isWin">�O�_���</param>
        void SetGameResult(bool isWin)
        {
            //����C����������
            audioManager.OnPlayGameOverSound(isWin);

            isControl = false;//���i�ާ@
            gameOver_Obj.SetActive(true);
            gameOver_Txt.text = isWin == true ? "Win" : "Game Over";
        }

        /// <summary>
        /// �����
        /// </summary>
        /// <param name="dir">�ưʤ�V(0=�k,1=��,2=�W,3=�U)</param>
        void SquareAction(int dir)
        {
            isMoving = true;
            switch (dir)
            {
                case 0://�V�k
                    for (int i = allSquareCount - 1; i >= 0; i--)
                    {
                        //���Ʀr && �D�̥k��
                        if (squareDatas[i].number != 0 && (i + 1) % grideSize != 0)
                        {
                            int zorePos = -1;//���洫�Ʀr=0
                            int posIndex;//�洫��mindex
                            for (posIndex = 1; posIndex < grideSize; posIndex++)
                            {
                                int ComparisonIndex = i + posIndex;//������s��

                                //�D�̫�@�� && ��磌��D�̥���
                                if (ComparisonIndex > allSquareCount - 1 || ComparisonIndex % grideSize == 0) break;

                                if (squareDatas[i].number != squareDatas[ComparisonIndex].number && squareDatas[ComparisonIndex].number != 0)//�Ʀr���ۦP
                                {
                                    zorePos = -1;//���s
                                    posIndex = ComparisonIndex - 1;
                                    OnNumberNotSame(i, posIndex);//�Ʀr���ۦP
                                    break;
                                }
                                else if (squareDatas[i].number == squareDatas[ComparisonIndex].number && !squareDatas[ComparisonIndex].isMerge)//�Ʀr�ۦP
                                {
                                    zorePos = -1;//���s
                                    posIndex = ComparisonIndex;
                                    OnNumberSame(i, posIndex);//�Ʀr�ۦP
                                    break;
                                }
                                else if (squareDatas[ComparisonIndex].number == 0 && (ComparisonIndex) % grideSize != 0)//���X�ּƦr=0
                                {
                                    //�����Ů��m
                                    zorePos = ComparisonIndex;
                                }

                                //���s���w�b�̥k��
                                if ((ComparisonIndex) % grideSize == 0) break;
                            }

                            //�V�Ů�B����
                            if (zorePos != -1)
                            {
                                //�[�J����List
                                MoveSquareList.Add(new MoveSquare() { obj = squareDatas[i].obj, posIndex = zorePos });
                                //�洫������
                                ChangeSquareDate(i, zorePos, squareDatas[i].number);
                            }
                        }
                    }
                    break;
                case 1://�V��
                    for (int i = 1; i < allSquareCount; i++)
                    {
                        //���Ʀr && �D�̥���
                        if (squareDatas[i].number != 0 && (i + grideSize) % grideSize != 0)
                        {
                            int zorePos = 0;//���洫�Ʀr=0
                            int posIndex;//�洫��mindex
                            for (posIndex = 1; posIndex < grideSize; posIndex++)
                            {
                                int ComparisonIndex = i - posIndex;//������s��

                                //�D�Ĥ@�� && ��磌��D�̥k��
                                if (ComparisonIndex < 0 && (ComparisonIndex + 1) % grideSize == 0) break;

                                if (squareDatas[i].number != squareDatas[ComparisonIndex].number && squareDatas[ComparisonIndex].number != 0)//�Ʀr���ۦP
                                {
                                    zorePos = -1;//���s
                                    posIndex = ComparisonIndex + 1;
                                    OnNumberNotSame(i, posIndex);//�Ʀr���ۦP
                                    break;
                                }
                                else if (squareDatas[i].number == squareDatas[ComparisonIndex].number && !squareDatas[ComparisonIndex].isMerge)//�Ʀr�ۦP
                                {
                                    zorePos = -1;//���s
                                    posIndex = ComparisonIndex;
                                    OnNumberSame(i, posIndex);//�Ʀr�ۦP
                                    break;
                                }
                                else if (squareDatas[ComparisonIndex].number == 0)//���X�ּƦr=0
                                {
                                    //�����Ů��m
                                    zorePos = ComparisonIndex;
                                }

                                //���s���w�b�̥���
                                if (ComparisonIndex % grideSize == 0) break;
                            }

                            //�V�Ů�B����
                            if (zorePos != -1)
                            {
                                //�[�J����List
                                MoveSquareList.Add(new MoveSquare() { obj = squareDatas[i].obj, posIndex = zorePos });
                                //�洫������
                                ChangeSquareDate(i, zorePos, squareDatas[i].number);
                            }
                        }
                    }
                    break;
                case 2://�V�W
                    for (int i = grideSize; i < allSquareCount; i++)
                    {
                        //���Ʀr
                        if (squareDatas[i].number != 0)
                        {
                            int zorePos = -1;//���洫�Ʀr=0
                            int posIndex;//�洫��mindex
                            for (posIndex = 1; posIndex < grideSize; posIndex++)
                            {
                                int ComparisonIndex = i - (grideSize * posIndex);//������s��
                                if (squareDatas[i].number != squareDatas[ComparisonIndex].number && squareDatas[ComparisonIndex].number != 0)//�Ʀr���ۦP
                                {
                                    zorePos = -1;//���s
                                    posIndex = ComparisonIndex + grideSize;
                                    OnNumberNotSame(i, posIndex);//�Ʀr���ۦP
                                    break;
                                }
                                else if (squareDatas[i].number == squareDatas[ComparisonIndex].number && !squareDatas[ComparisonIndex].isMerge)//�Ʀr�ۦP
                                {
                                    zorePos = -1;//���s
                                    posIndex = ComparisonIndex;
                                    OnNumberSame(i, posIndex);//�Ʀr�ۦP
                                    break;
                                }
                                else if (squareDatas[ComparisonIndex].number == 0)//���X�ּƦr=0
                                {
                                    //�����Ů��m
                                    zorePos = ComparisonIndex;
                                }

                                //���s���w�b�̤W��
                                if (ComparisonIndex < grideSize) break;
                            }

                            //�V�Ů�B����
                            if (zorePos != -1)
                            {
                                //�[�J����List
                                MoveSquareList.Add(new MoveSquare() { obj = squareDatas[i].obj, posIndex = zorePos });
                                //�洫������
                                ChangeSquareDate(i, zorePos, squareDatas[i].number);
                            }
                        }
                    }
                    break;
                case 3://�V�U
                    for (int i = allSquareCount - 1 - grideSize; i >= 0; i--)
                    {
                        //���Ʀr
                        if (squareDatas[i].number != 0)
                        {
                            int zorePos = -1;//���洫�Ʀr=0
                            int posIndex;//�洫��mindex
                            for (posIndex = 1; posIndex < grideSize; posIndex++)
                            {
                                int ComparisonIndex = i + (grideSize * posIndex);//������s��
                                if (squareDatas[i].number != squareDatas[ComparisonIndex].number && squareDatas[ComparisonIndex].number != 0)//�Ʀr���ۦP
                                {
                                    zorePos = -1;//���s
                                    posIndex = ComparisonIndex - grideSize;
                                    OnNumberNotSame(i, posIndex);//�Ʀr���ۦP
                                    break;
                                }
                                else if (squareDatas[i].number == squareDatas[ComparisonIndex].number && !squareDatas[ComparisonIndex].isMerge)//�Ʀr�ۦP
                                {
                                    zorePos = -1;//���s
                                    posIndex = ComparisonIndex;
                                    OnNumberSame(i, posIndex);//�Ʀr�ۦP
                                    break;
                                }
                                else if (squareDatas[ComparisonIndex].number == 0)//���X�ּƦr=0
                                {
                                    //�����Ů��m
                                    zorePos = ComparisonIndex;
                                }

                                //���s���w�b�̤U��
                                if (ComparisonIndex > allSquareCount - 1 - grideSize) break;
                            }

                            //�V�Ů�B����
                            if (zorePos != -1)
                            {
                                //�[�J����List
                                MoveSquareList.Add(new MoveSquare() { obj = squareDatas[i].obj, posIndex = zorePos });
                                //�洫������
                                ChangeSquareDate(i, zorePos, squareDatas[i].number);
                            }
                        }
                    }
                    break;
            }

            //�������
            if (MoveSquareList.Count > 0)
            {
                isMoving = true;
                StartCoroutine(ISquareMoveResult());//�������
            }
            else isMoving = false;

            //���s������
            for (int i = 0; i < squareDatas.Length; i++)
            {
                squareDatas[i].isMerge = false;
            }

            initTouchPos = Vector3.zero;//���sĲ�I��m
        }

        /// <summary>
        /// �Ʀr���ۦP
        /// </summary>
        /// <param name="index">����index</param>
        /// <param name="posIndex">�s���index</param>
        void OnNumberNotSame(int index, int posIndex)
        {
            //��m���P
            if (index != posIndex)
            {
                //�[�J����List
                MoveSquareList.Add(new MoveSquare() { obj = squareDatas[index].obj, posIndex = posIndex });
                //�洫������
                ChangeSquareDate(index, posIndex, squareDatas[index].number);
            }
        }

        /// <summary>
        /// �Ʀr�ۦP
        /// </summary>
        /// <param name="index">����index</param>
        /// <param name="posIndex">�s���index</param>
        void OnNumberSame(int index, int posIndex)
        {
            squareDatas[posIndex].isMerge = true;
            //�]�w����
            SetScore(squareDatas[posIndex].number * 2);
            //�[�J����List
            MoveSquareList.Add(new MoveSquare() { obj = squareDatas[index].obj, posIndex = posIndex, destroyObj = squareDatas[posIndex].obj });
            //�洫������
            ChangeSquareDate(index, posIndex, squareDatas[posIndex].number * 2);
        }

        /// <summary>
        /// �洫������
        /// </summary>
        /// <param name="oldIndex">����index</param>
        /// <param name="newIndex">�s���index</param>
        /// <param name="number">��ܼƦr</param>
        void ChangeSquareDate(int oldIndex, int newIndex, int number)
        {
            //����������
            squareDatas[newIndex].number = number;
            squareDatas[newIndex].obj = squareDatas[oldIndex].obj;
            squareDatas[newIndex].textObj = squareDatas[oldIndex].textObj;
            squareDatas[newIndex].imageObj = squareDatas[oldIndex].imageObj;
            squareDatas[newIndex].animator = squareDatas[oldIndex].animator;
            //���
            if (number == winNumber) SetGameResult(true);
            //�M�z������
            CleanSquareData(oldIndex);
        }

        /// <summary>
        /// �M�z������
        /// </summary>
        /// <param name="index">�ĴX�����</param>
        void CleanSquareData(int index)
        {
            squareDatas[index].number = 0;
            squareDatas[index].obj = null;
            squareDatas[index].textObj = null;
            squareDatas[index].imageObj = null;
            squareDatas[index].animator = null;
        }

        /// <summary>
        /// ������ʵ��G
        /// </summary>
        /// <returns></returns>
        IEnumerator ISquareMoveResult()
        {
            bool isMerge = false;//�O�_������X��

            //�������
            yield return new WaitUntil(() =>
            {
                bool isMoved = true;//�P�_�O�_���ʵ���
                for (int i = 0; i < MoveSquareList.Count; i++)
                {
                    Vector3 nowPos = MoveSquareList[i].obj.transform.position;//�ثe��m
                    Vector3 targetPos = gridBgPosList[MoveSquareList[i].posIndex];//�ؼЦ�m

                    //�w����
                    if (nowPos != targetPos) isMoved = false;
                }
                return isMoved;
            });

            //�Ʀr���
            for (int i = 0; i < MoveSquareList.Count; i++)
            {
                //����C��
                float powNumber = 2;
                for (int pow = 1; pow < 12; pow++)
                {
                    powNumber = Mathf.Pow(2, pow);
                    if (powNumber == squareDatas[MoveSquareList[i].posIndex].number)
                    {
                        squareDatas[MoveSquareList[i].posIndex].imageObj.color = squareColors[pow-1];
                        break;
                    }
                }
                //��ܼƦr
                squareDatas[MoveSquareList[i].posIndex].textObj.text = squareDatas[MoveSquareList[i].posIndex].number.ToString();

                //�����ۦP�Ʀr
                if (MoveSquareList[i].destroyObj != null)
                {
                    isMerge = true;
                    
                    //����X�ְʵe
                    squareDatas[MoveSquareList[i].posIndex].animator.SetTrigger(animatorHashId[0]);
                    //�����Q�X�֤��
                    MoveSquareList[i].destroyObj.gameObject.SetActive(false);
                }
            }

            if (isMerge)
            {
                //����o������
                audioManager.OnPlayScoreSound();
            }

            //���sList
            MoveSquareList = new List<MoveSquare>();
            //�Ыؤ��
            CreateSquare();
            //���ʵ���
            isMoving = false;
        }
    }

    /// <summary>
    /// ��������
    /// </summary>
    class objPoolData
    {
        [Tooltip("������s��")] public int id;
        [Tooltip("����W��")] public string objName;
        [Tooltip("����")] public GameObject obj;
        [Tooltip("������")] public Transform parentObj;
    }

    /// <summary>
    /// ������
    /// </summary>
    [System.Serializable]
    class SquareData
    {
        [Tooltip("������Ʀr")] public int number;
        [Tooltip("����C��")] public Image imageObj;
        [Tooltip("Animator")] public Animator animator;
        [Tooltip("�������")] public RectTransform obj;
        [Tooltip("��r����")] public Text textObj;
        [Tooltip("�O�_�w�X�ֹL")] public bool isMerge;
    }

    /// <summary>
    /// ���ʤ�����
    /// </summary>
    [System.Serializable]
    class MoveSquare
    {
        [Tooltip("�������")] public RectTransform obj;
        [Tooltip("���ʦ�mindex")] public int posIndex;
        [Tooltip("��������")] public RectTransform destroyObj;
    }
}
