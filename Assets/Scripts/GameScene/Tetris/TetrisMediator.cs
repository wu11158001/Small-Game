using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StaticScripts;
using Entry;

/// <summary>
/// �C������
/// </summary>
namespace GameScene
{
    /// <summary>
    /// �Xù�����
    /// </summary>
    public class TetrisMediator : MonoBehaviour
    {
        public ObjectPool objectPool;//�����
        AudioManager audioManager;//���ĺ޲z
        InputManager inputManager;//��J����

        [SerializeField] TetrisToolMediator tetrisToolMediator;
        TetrisLobbyMediator tetrisLobbyMediator;

        [Header("�w�]���")]
        [Tooltip("������W��")]
        public readonly string[] objPoolName =
{
            "Square",//���
            "GhostPiece",//����
        };
        [Tooltip("����j�p")] readonly int squareSize = 70;
        [Tooltip("�U���[�t�t��")] readonly float accelerrateSpeed = 0.04f;
        [Tooltip("�U���t�׵���(��)")]
        public readonly float[] downLevelSpeed =
        {
            1, 0.9f, 0.8f, 0.6f, 0.4f, 0.2f, 0.09f, 0.07f, 0.05f,
        };
        [Tooltip("�i���ਤ��Z")] readonly int[] rotateZ = { 0, 90, 180, 270 };
        [Tooltip("������W��_�Ϊ����")]
        readonly string[] shapeSquareNames =
        {
            "shape_Square",//�����
            "shape_Strip",//������
            "shape_L",//L����
            "shape_T",//�g����
            "shape_Z",//Z����
            "shape_ReversL",//��L����
        };
        [Tooltip("�[�t����")] public readonly int addSpeedScore = 200;

        [Header("�ʵe")]
        [Tooltip("�ʵe�W��")]
        readonly string[] animatorName =
        {
            "Clean",//�������
        };
        [Tooltip("�ʵeHashId")] int[] animatorHashId;
        [Tooltip("�M���ʵe�ɪ�")] float cleanAniTime;

        [Header("����")]
        [Tooltip("���I��")] [SerializeField] RectTransform GridBg_Img;
        [Tooltip("���I���e��")] [SerializeField] RectTransform gridContent;
        [Tooltip("���˥�")] [SerializeField] RectTransform gridSample;
        [Tooltip("����˥�")] [SerializeField] RectTransform squareSample;
        [Tooltip("����Ϊ��e��")] [SerializeField] RectTransform shapeSquare_Obj;
        [Tooltip("�����˥�")] public RectTransform ghostPieceSample;

        [Header("�����")]
        [Tooltip("�����(����W��,�s��)")] public Dictionary<string, int> objectPoolDic = new Dictionary<string, int>();

        [Header("����Ϊ�")]
        [Tooltip("����C��")] public Color[] shapeSquareColors;

        [Header("����")]
        [Tooltip("�O�_�}�l�C��")] public bool isGameStart;
        [Tooltip("��e�ާ@���")] public CurrentControlSquare controlSquare;
        [Tooltip("����U������")] public int downLevel;
        [Tooltip("����U���p�ɾ�")] float downTime;
        [Tooltip("����U���t��(��)")] public float downSpeed;
        [Tooltip("��e���ਤ��Index")] [SerializeField] int rotateIndex;
        [Tooltip("���X�y��")] [SerializeField] float outsideX;
        [Tooltip("���Y�y��")] [SerializeField] float outsideY;
        [Tooltip("X�ƶq")] [SerializeField] int lineXCount;
        [Tooltip("Y�ƶq")] public int lineYCount;
        [Tooltip("��e���Index")][SerializeField] int currentIndex;
        [Tooltip("�U�Ӥ��Index")] [SerializeField] int nextIndex;
        [Tooltip("��������e�����m")] Vector2[] originalPos;
        [Tooltip("��������e���ਤ��")] int originalRotateIndex;

        [Header("Ĳ�I")]
        [Tooltip("�_�lĲ�I�I")] Vector2 initTouchPos;
        [Tooltip("�P�_�O�_�����ʹL")] bool isTouchMove;
        [Tooltip("��������Ĳ�I�ɶ�")] float begainTouchTime;

        [Header("�s�u�P�_")]
        [Tooltip("�O�_���b�P�_")] [SerializeField] bool isJudging;
        [Tooltip("�C��Y��m")] [SerializeField] float[] linePosYList;
        [Tooltip("����������")] public List<RectTransform> stopSquaresList = new List<RectTransform>();

        private void Awake()
        {
            objectPool = ObjectPool.Instance;//�����
            tetrisLobbyMediator = FindObjectOfType<TetrisLobbyMediator>();
            audioManager = AudioManager.Instance;//���ĺ޲z
            inputManager = InputManager.Instance;//��J����

            controlSquare = CurrentControlSquare.Instance;//��e�ާ@���

            //�ʵeHashId
            animatorHashId = new int[animatorName.Length];
            for (int i = 0; i < animatorName.Length; i++)
            {
                animatorHashId[i] = Animator.StringToHash(animatorName[i]);
            }

            //���
            outsideX = (GridBg_Img.sizeDelta.x / 2) - (squareSize / 2);
            outsideY = (GridBg_Img.sizeDelta.y / 2) - (squareSize / 2);

            isGameStart = false;//�O�_�}�l�C��

            //��C�ƶq
            lineXCount = (int)(((GridBg_Img.sizeDelta.x / 2) / squareSize) * 2);
            lineYCount = (int)(((GridBg_Img.sizeDelta.y / 2) / squareSize) * 2);

            //�C��Y��m
            linePosYList = new float[lineYCount];
            for (int i = 0; i < linePosYList.Length; i++)
            {
                linePosYList[i] = outsideY - (squareSize * i);
            }

            //�U�Ӥ��Index
            nextIndex = Random.Range(0, shapeSquareNames.Length);

            GridBg_Img.gameObject.SetActive(false);//���I��

            CreateObjPool();//�Ыت����
        }

        private void Update()
        {
            //��J����
            OnInputControl();
            //��e�ާ@�������
            OnCurrentSquareMove();
        }

        /// <summary>
        /// ��J����
        /// </summary>
        void OnInputControl()
        {
#if UNITY_EDITOR
            //�C���}�l && ����� && �D�Ȱ� 
            if (isGameStart == true && controlSquare.squares.Count > 0 && !tetrisLobbyMediator.isPause)
            {
                //�I����s��Return
                if (inputManager.IsOnUIElement() == true) return;

                //���s�t��
                downSpeed = downLevelSpeed[tetrisLobbyMediator.currentSpeed];

                //�D�b�P�_�s�u
                if (!isJudging)
                {
                    //����
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        //������s�I������
                        audioManager.OnPlayButtonClickSound();
                        //�󴫨���
                        ChangeRotate();
                        //�ʴ��O�_�W�X���
                        MonitorOutside();
                        //�ʴ��O�_�P��L�����m����
                        MonitorDuplicates();
                        //�Ыص���
                        tetrisToolMediator.CreateGhostPiece(controlSquare.squares, currentIndex);
                    }
                    //��
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        //������s�I������
                        audioManager.OnPlayButtonClickSound();
                        //�������
                        SquareMove(-1, 0);
                        //�Ыص���
                        tetrisToolMediator.CreateGhostPiece(controlSquare.squares, currentIndex);
                    }
                    //�k
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        //������s�I������
                        audioManager.OnPlayButtonClickSound();
                        //�������
                        SquareMove(1, 0);
                        //�Ыص���
                        tetrisToolMediator.CreateGhostPiece(controlSquare.squares, currentIndex);
                    }

                    //�����U��
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        audioManager.SetPlaySound("Rapid");
                        downSpeed = 0;
                    }
                }
                
                //�U
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    downSpeed = accelerrateSpeed;
                }
                else if (Input.GetKeyUp(KeyCode.DownArrow))
                {
                    downSpeed = downLevelSpeed[tetrisLobbyMediator.currentSpeed];
                }
            }

#elif UNITY_ANDROID
        //Ĳ�I����
        OnTouchControl();
#endif
        }

        /// <summary>
        /// Ĳ�I����
        /// </summary>
        void OnTouchControl()
        {
            //�C���}�l && ����� && �D�Ȱ�
            if (isGameStart == true && controlSquare.squares.Count > 0 && !tetrisLobbyMediator.isPause)
            {
                //���s�t��
                downSpeed = downLevelSpeed[tetrisLobbyMediator.currentSpeed];

                //�I����s��Return
                if (inputManager.IsOnUIElement() == true) return;

                float touchSensitivity = 70;

                if (Input.touchCount == 1)
                {
                    switch (Input.GetTouch(0).phase)
                    {
                        //����Ĳ�I
                        case TouchPhase.Began:
                            isTouchMove = false;
                            begainTouchTime = Time.realtimeSinceStartup;
                            initTouchPos = Input.GetTouch(0).position;
                            break;

                        //����
                        case TouchPhase.Moved:
                            //�D�b�P�_�s�u
                            if (!isJudging)
                            {
                                //���k����
                                if (Input.GetTouch(0).position.x > initTouchPos.x + touchSensitivity)
                                {
                                    isTouchMove = true;
                                    audioManager.OnPlayButtonClickSound();
                                    initTouchPos = Input.GetTouch(0).position;
                                    SquareMove(1, 0);//�������
                                    //�Ыص���
                                    tetrisToolMediator.CreateGhostPiece(controlSquare.squares, currentIndex);
                                }
                                else if (Input.GetTouch(0).position.x < initTouchPos.x - touchSensitivity)
                                {
                                    isTouchMove = true;
                                    audioManager.OnPlayButtonClickSound();
                                    initTouchPos = Input.GetTouch(0).position;
                                    SquareMove(-1, 0);//�������
                                    //�Ыص���
                                    tetrisToolMediator.CreateGhostPiece(controlSquare.squares, currentIndex);
                                }
                            }

                            //�V�U�[�t����
                            if (Input.GetTouch(0).position.y < initTouchPos.y - (touchSensitivity * 2))
                            {
                                isTouchMove = true;
                                downSpeed = accelerrateSpeed;
                            }

                            //����[�t
                            if (Input.GetTouch(0).position.y > initTouchPos.y + touchSensitivity)
                            {
                                isTouchMove = true;
                                //�t���٭�
                                downSpeed = downLevelSpeed[tetrisLobbyMediator.currentSpeed];
                            }
                            break;

                        //Ĳ�I����
                        case TouchPhase.Ended:
                            //�t���٭�
                            downSpeed = downLevelSpeed[tetrisLobbyMediator.currentSpeed];
                            //Ĳ�I�����ɶ�
                            float endedTime = Time.realtimeSinceStartup;

                            //�D�b�P�_�s�u
                            if (!isJudging)
                            {
                                //�����U��
                                if (endedTime - begainTouchTime < 0.2f && Input.GetTouch(0).position.y < initTouchPos.y - (touchSensitivity / 2))
                                {
                                    isTouchMove = true;
                                    audioManager.SetPlaySound("Rapid");
                                    downSpeed = 0;
                                }
                            }

                            //�����ʹL
                            if (!isTouchMove)
                            {
                                audioManager.OnPlayButtonClickSound();//������s�I������
                                //�󴫨���
                                ChangeRotate();
                                //�ʴ��O�_�W�X���
                                MonitorOutside();
                                //�ʴ��O�_�P��L�����m����
                                MonitorDuplicates();
                                //�Ыص���
                                tetrisToolMediator.CreateGhostPiece(controlSquare.squares, currentIndex);
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// �󴫨���
        /// </summary>
        void ChangeRotate()
        {
            //��������e�����m
            originalPos = new Vector2[controlSquare.squares.Count];
            for (int i = 0; i < originalPos.Length; i++)
            {
                originalPos[i] = controlSquare.squares[i].anchoredPosition;
            }
            //��������e���ਤ��
            originalRotateIndex = rotateIndex;

            //���פ���
            rotateIndex = rotateIndex++ >= rotateZ.Length - 1 ? 0 : rotateIndex++;

            switch (controlSquare.shapeIndex)
            {
                case 0://�����
                    break;
                case 1://������
                    if (rotateZ[rotateIndex] == 0 || rotateZ[rotateIndex] == 180)
                    {
                        float initX = controlSquare.squares[0].anchoredPosition.x - squareSize;
                        float initY = controlSquare.squares[0].anchoredPosition.y;
                        for (int i = 0; i < controlSquare.squares.Count; i++)
                        {
                            controlSquare.squares[i].anchoredPosition = new Vector2(initX + (i * squareSize), initY);
                        }
                    }
                    else if (rotateZ[rotateIndex] == 90 || rotateZ[rotateIndex] == 270)
                    {
                        float initX = controlSquare.squares[1].anchoredPosition.x;
                        float initY = controlSquare.squares[1].anchoredPosition.y;
                        for (int i = 0; i < controlSquare.squares.Count; i++)
                        {
                            controlSquare.squares[i].anchoredPosition = new Vector2(initX, initY - (i * squareSize));
                        }
                        RectTransform temp = controlSquare.squares[0];
                        controlSquare.squares[0] = controlSquare.squares[1];
                        controlSquare.squares[1] = temp;
                    }
                    break;
                case 2://L����
                    if (rotateZ[rotateIndex] == 0)
                    {
                        float initX = controlSquare.squares[2].anchoredPosition.x;
                        float initY = controlSquare.squares[2].anchoredPosition.y;
                        controlSquare.squares[0].anchoredPosition = new Vector2(initX, initY + squareSize);
                        for (int i = 1; i < controlSquare.squares.Count; i++)
                        {
                            controlSquare.squares[i].anchoredPosition = new Vector2(initX + ((i-1) * squareSize), initY);
                        }
                    }
                    else if (rotateZ[rotateIndex] == 90)
                    {
                        float initX = controlSquare.squares[2].anchoredPosition.x;
                        float initY = controlSquare.squares[2].anchoredPosition.y;
                        controlSquare.squares[0].anchoredPosition = new Vector2(initX - squareSize, initY);
                        for (int i = 1; i < controlSquare.squares.Count; i++)
                        {
                            controlSquare.squares[i].anchoredPosition = new Vector2(initX, initY + ((i-1) * squareSize));
                        }
                    }
                    else if (rotateZ[rotateIndex] == 180)
                    {
                        float initX = controlSquare.squares[2].anchoredPosition.x;
                        float initY = controlSquare.squares[2].anchoredPosition.y;
                        controlSquare.squares[0].anchoredPosition = new Vector2(initX, initY - squareSize);
                        for (int i = 1; i < controlSquare.squares.Count; i++)
                        {
                            controlSquare.squares[i].anchoredPosition = new Vector2(initX - ((i-1) * squareSize), initY);
                        }
                    }
                    else if (rotateZ[rotateIndex] == 270)
                    {
                        float initX = controlSquare.squares[2].anchoredPosition.x;
                        float initY = controlSquare.squares[2].anchoredPosition.y;
                        controlSquare.squares[0].anchoredPosition = new Vector2(initX + squareSize, initY);
                        for (int i = 1; i < controlSquare.squares.Count; i++)
                        {
                            controlSquare.squares[i].anchoredPosition = new Vector2(initX, initY - ((i-1) * squareSize));
                        }
                    }
                    break;
                case 3://�g����
                    if (rotateZ[rotateIndex] == 0)
                    {
                        float initX = controlSquare.squares[0].anchoredPosition.x;
                        float initY = controlSquare.squares[0].anchoredPosition.y;
                        controlSquare.squares[0].anchoredPosition = new Vector2(initX - squareSize, initY + squareSize);
                        for (int i = 1; i < controlSquare.squares.Count; i++)
                        {
                            controlSquare.squares[i].anchoredPosition = new Vector2((initX - (squareSize * 2)) + ((i-1) * squareSize), initY);
                        }
                    }
                    else if (rotateZ[rotateIndex] == 90)
                    {
                        float initX = controlSquare.squares[0].anchoredPosition.x;
                        float initY = controlSquare.squares[0].anchoredPosition.y;
                        controlSquare.squares[0].anchoredPosition = new Vector2(initX - squareSize, initY - squareSize);
                        for (int i = 1; i < controlSquare.squares.Count; i++)
                        {
                            controlSquare.squares[i].anchoredPosition = new Vector2(initX, (initY - (squareSize * 2)) + ((i - 1) * squareSize));
                        }
                    }
                    else if (rotateZ[rotateIndex] == 180)
                    {
                        float initX = controlSquare.squares[0].anchoredPosition.x;
                        float initY = controlSquare.squares[0].anchoredPosition.y;
                        controlSquare.squares[0].anchoredPosition = new Vector2(initX + squareSize, initY - squareSize);
                        for (int i = 1; i < controlSquare.squares.Count; i++)
                        {
                            controlSquare.squares[i].anchoredPosition = new Vector2((initX + (squareSize * 2)) - ((i-1) * squareSize), initY);
                        }
                    }
                    else if (rotateZ[rotateIndex] == 270)
                    {
                        float initX = controlSquare.squares[0].anchoredPosition.x;
                        float initY = controlSquare.squares[0].anchoredPosition.y;
                        controlSquare.squares[0].anchoredPosition = new Vector2(initX + squareSize, initY + squareSize);
                        for (int i = 1; i < controlSquare.squares.Count; i++)
                        {
                            controlSquare.squares[i].anchoredPosition = new Vector2(initX, (initY + (squareSize * 2)) - ((i-1) * squareSize));
                        }
                    }
                    break;
                case 4://Z����
                    if (rotateZ[rotateIndex] == 0 || rotateZ[rotateIndex] == 180)
                    {
                        float initX = controlSquare.squares[2].anchoredPosition.x;
                        float initY = controlSquare.squares[2].anchoredPosition.y;
                        for (int i = 0; i < 2; i++)
                        {
                            controlSquare.squares[i].anchoredPosition = new Vector2(initX + (i * squareSize), initY + squareSize);
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            controlSquare.squares[i + 2].anchoredPosition = new Vector2((initX + squareSize) + ( i * squareSize), initY);
                        }
                    }
                    else if (rotateZ[rotateIndex] == 90 || rotateZ[rotateIndex] == 270)
                    {
                        float initX = controlSquare.squares[2].anchoredPosition.x;
                        float initY = controlSquare.squares[2].anchoredPosition.y;
                        for (int i = 0; i < 2; i++)
                        {
                            controlSquare.squares[i].anchoredPosition = new Vector2(initX, (initY + squareSize) - (i * squareSize));
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            controlSquare.squares[i+2].anchoredPosition = new Vector2(initX - squareSize, initY - (i * squareSize));
                        }
                    }
                    break;
                case 5://��L����
                    if (rotateZ[rotateIndex] == 0)
                    {
                        float initX = controlSquare.squares[2].anchoredPosition.x;
                        float initY = controlSquare.squares[2].anchoredPosition.y;
                        controlSquare.squares[0].anchoredPosition = new Vector2(initX + squareSize, initY);
                        for (int i = 1; i < controlSquare.squares.Count; i++)
                        {
                            controlSquare.squares[i].anchoredPosition = new Vector2((initX + squareSize) - ((i-1) * squareSize), initY - squareSize);
                        }
                    }
                    else if (rotateZ[rotateIndex] == 90)
                    {
                        float initX = controlSquare.squares[2].anchoredPosition.x;
                        float initY = controlSquare.squares[2].anchoredPosition.y;
                        controlSquare.squares[0].anchoredPosition = new Vector2(initX, initY + squareSize);
                        for (int i = 1; i < controlSquare.squares.Count; i++)
                        {
                            controlSquare.squares[i].anchoredPosition = new Vector2(initX + squareSize, (initY + squareSize) - ((i - 1) * squareSize));
                        }
                    }
                    else if (rotateZ[rotateIndex] == 180)
                    {
                        float initX = controlSquare.squares[2].anchoredPosition.x;
                        float initY = controlSquare.squares[2].anchoredPosition.y;
                        controlSquare.squares[0].anchoredPosition = new Vector2(initX - squareSize, initY);
                        for (int i = 1; i < controlSquare.squares.Count; i++)
                        {
                            controlSquare.squares[i].anchoredPosition = new Vector2((initX - squareSize) + ((i-1) * squareSize), initY + squareSize);
                        }
                    }
                    else if (rotateZ[rotateIndex] == 270)
                    {
                        float initX = controlSquare.squares[2].anchoredPosition.x;
                        float initY = controlSquare.squares[2].anchoredPosition.y;
                        controlSquare.squares[0].anchoredPosition = new Vector2(initX, initY - squareSize);
                        for (int i = 1; i < controlSquare.squares.Count; i++)
                        {
                            controlSquare.squares[i].anchoredPosition = new Vector2(initX - squareSize, (initY - squareSize) + ((i-1) * squareSize));
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// �ʴ��O�_�W�X���
        /// </summary>
        void MonitorOutside()
        {
            int positiveX = 0;//�j��X�ƶq
            int negativeX = 0;//�p��X�ƶq
            int negativeY = 0;//�p��Y�ƶq
            for (int i = 0; i < controlSquare.squares.Count; i++)
            {
                float x = controlSquare.squares[i].anchoredPosition.x;
                float y = controlSquare.squares[i].anchoredPosition.y;

                if (x > outsideX) positiveX++;//�W�L�k�����
                else if (x < -outsideX) negativeX++;//�W�L�������
                else if (y < -outsideY) negativeY++;//�W�L�������
            }

            //�ץ���m
            for (int i = 0; i < controlSquare.squares.Count; i++)
            {
                float initX = controlSquare.squares[i].anchoredPosition.x;
                float initY = controlSquare.squares[i].anchoredPosition.y;

                if (positiveX > 0) controlSquare.squares[i].anchoredPosition = new Vector2(initX - (squareSize * positiveX), initY);
                else if (negativeX > 0) controlSquare.squares[i].anchoredPosition = new Vector2(initX + (squareSize * negativeX), initY);
                else if (negativeY > 0) controlSquare.squares[i].anchoredPosition = new Vector2(initX, initY + (squareSize * negativeY));
            }
        }

        /// <summary>
        /// �ʴ��O�_�P��L�����m����
        /// </summary>
        void MonitorDuplicates()
        {
            //�˴��O�_�I����
            bool isDuplicates = false;
            for (int i = 0; i < stopSquaresList.Count; i++)
            {
                if (isDuplicates) break;

                for (int j = 0; j < controlSquare.squares.Count; j++)
                {
                    if (isDuplicates) break;

                    if (controlSquare.squares[j].anchoredPosition == stopSquaresList[i].anchoredPosition)
                    {
                        isDuplicates = true;
                        break;
                    }
                }
            }

            //���I����
            if (isDuplicates)
            {
                //�^����m�P����
                rotateIndex = originalRotateIndex;

                for (int i = 0; i < controlSquare.squares.Count; i++)
                {
                    controlSquare.squares[i].anchoredPosition = originalPos[i];
                }
            }
        }

        /// <summary>
        /// ��e�ާ@�������
        /// </summary>
        void OnCurrentSquareMove()
        {
            if (isGameStart)
            {
                //�D�P�_�s�u && �����
                if (!isJudging && controlSquare.squares.Count != 0)
                {
                    //����U��
                    downTime += Time.deltaTime;
                    if (downTime >= downSpeed)
                    {
                        //���s�U���ɶ�
                        downTime = 0;

                        //�������
                        SquareMove(0, -1);
                    }
                }
            }
        }

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="valueX">�����ܶqX</param>
        /// <param name="valueY">�����ܶqY</param>
        void SquareMove(int valueX, int valueY)
        {
            //���b
            if (valueX >= 1) valueX = 1;
            else if (valueX <= -1) valueX = -1;
            if (controlSquare.squares.Count == 0) return;

            //�˴����ʬO�_�W�X���
            bool[] outside = IsOutside(controlSquare.squares, valueX, valueY);
            bool isOutside = outside[0];
            bool isBotton = outside[1];

            //�˴��O�_�I����
            if (IsTouchOtherSquare(controlSquare.squares, valueX, valueY) == true)
            {
                //�V�U����
                if (valueY == -1) isBotton = true;
                else if (valueX != 0) isOutside = true;
            }

            //�w�F�̩��� || �I�쩳�����
            if (isBotton)
            {
                for (int i = 0; i < controlSquare.squares.Count; i++)
                {
                    //���������m
                    stopSquaresList.Add(controlSquare.squares[i]);
                }

                //�P�_�s�u����
                StartCoroutine(IJudgeLine());
                return;
            }

            //�i����
            if (!isBotton && !isOutside)
            {
                for (int i = 0; i < controlSquare.squares.Count; i++)
                {
                    float x = controlSquare.squares[i].anchoredPosition.x + (squareSize * valueX);
                    float y = controlSquare.squares[i].anchoredPosition.y + (squareSize * valueY);
                    controlSquare.squares[i].anchoredPosition = new Vector2(x, y);
                }
            }

            //��t�U��
            if (downSpeed == 0) SquareMove(0, -1);
        }

        /// <summary>
        /// �˴��O�_�I����
        /// </summary>
        /// <param name="judgeSquareList">�n�P�_�����List</param>
        /// <param name="dirX">�˴���VX</param>
        /// <param name="dirY">�˴���VY</param>
        /// <returns></returns>
        public bool IsTouchOtherSquare(List<RectTransform> judgeSquareList, int dirX, int dirY)
        {
            for (int i = 0; i < stopSquaresList.Count; i++)
            {
                for (int j = 0; j < judgeSquareList.Count; j++)
                {
                    float judgeX = judgeSquareList[j].anchoredPosition.x + (squareSize * dirX);
                    float judgeY = judgeSquareList[j].anchoredPosition.y + (squareSize * dirY);
                    if (new Vector2(judgeX, judgeY) == stopSquaresList[i].anchoredPosition)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// �˴��O�_�W�X���(0=�W�X���k���, 1=�W�X�̩���)
        /// </summary>
        /// <param name="judgeSquareList">�n�P�_�����List</param>
        /// <param name="dirX">�˴���VX</param>
        /// <param name="dirY">�˴���VY</param>
        /// <returns></returns>
        public bool[] IsOutside(List<RectTransform> judgeSquareList, int dirX, int dirY)
        {
            //(0=�W�X���k���, 1=�W�X�̩���)
            bool[] result = new bool[2];

            for (int i = 0; i < judgeSquareList.Count; i++)
            {
                float judgeX = judgeSquareList[i].anchoredPosition.x + (squareSize * dirX);
                float judgeY = judgeSquareList[i].anchoredPosition.y + (squareSize * dirY);

                //�w�b�ⰼ�����
                if (judgeX < -outsideX || judgeX > outsideX)
                {
                    //�W�X���k���
                    result[0] = true;
                    break;
                }
                else if (judgeY < -outsideY)//�F�̩���
                {
                    //�W�X�̩���
                    result[1] = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// �P�_�s�u����
        /// </summary>
        IEnumerator IJudgeLine()
        {
            isJudging = true;//���b�P�_�s�u
            int bottomNum = 0;//�s�u�̩������s��
            int lineCount = 0;//�s�u����

            //�����ۦP��m
            List<RectTransform>[] recodeList = new List<RectTransform>[lineYCount];
            for (int i = 0; i < recodeList.Length; i++)
            {
                recodeList[i] = new List<RectTransform>();
            }
            for (int i = 0; i < stopSquaresList.Count; i++)
            {
                for (int j = 0; j < linePosYList.Length; j++)
                {
                    if (stopSquaresList[i].anchoredPosition.y == linePosYList[j])
                    {
                        recodeList[j].Add(stopSquaresList[i]);
                    }
                }
            }

            //�����ʵe
            List<RectTransform> cleanList = new List<RectTransform>();//�����n���������
            for (int i = 0; i < recodeList.Length; i++)
            {
                if (recodeList[i].Count == lineXCount)
                {
                    lineCount++;//�s�u����
                    bottomNum = bottomNum < i ? i : bottomNum;//�s�u�̩������s��
                    foreach (var item in recodeList[i])
                    {
                        if(item.TryGetComponent<Animator>(out Animator animator))
                        {
                            animator.SetTrigger(animatorHashId[0]);
                        }
                        cleanList.Add(item);             
                    }
                }
            }

            //����
            float yieldTime = cleanList.Count > 0 ? cleanAniTime + 0.1f : 0;
            yield return new WaitForSeconds(yieldTime);

            //����o������
            if (lineCount > 0) audioManager.OnPlayScoreSound();

            //�]�w����
            tetrisLobbyMediator.SetScore(lineCount);

            //�������
            foreach (var item in cleanList)
            {
                if (item != null)
                {
                    item.gameObject.SetActive(false);
                    stopSquaresList.Remove(item);
                }
            }

            //�������W������U��
            if (cleanList.Count > 0)
            {                
                foreach (var item in stopSquaresList)
                {
                    if (item.anchoredPosition.y > linePosYList[bottomNum])
                    {
                        item.anchoredPosition = new Vector2(item.anchoredPosition.x, item.anchoredPosition.y - (squareSize * (cleanList.Count / lineXCount)));
                    }
                }
            }

            //�P�_����O�_�W�X�̤W��
            for (int i = 0; i < stopSquaresList.Count; i++)
            {
                if (stopSquaresList[i].anchoredPosition.y > outsideY)
                {
                    //�C������
                    tetrisLobbyMediator.OnGameOver(false);
                }
            }

            //��t�U���٭�
            if (downSpeed == 0) downSpeed = downLevelSpeed[tetrisLobbyMediator.currentSpeed]; ;

            //�Ыطs���
            if (isGameStart) CreateNewSquare();
        }

        /// <summary>
        /// �Ыؤ��
        /// </summary>
        public IEnumerator ICreateGrid()
        {
            GridBg_Img.gameObject.SetActive(true);//���I��

            gridSample.sizeDelta = new Vector2(squareSize, squareSize);//���Size
            gridSample.gameObject.SetActive(false);

            int row = (int)(GridBg_Img.sizeDelta.x / squareSize);//��Ƽƶq
            int column = (int)(GridBg_Img.sizeDelta.y / squareSize);//�a�V�ƶq
            for (int i = 0; i < column; i++)
            {
                for (int j = 0; j < row; j++)
                {
                    RectTransform obj = Instantiate(gridSample, gridContent);
                    float posX = -outsideX + (j * gridSample.sizeDelta.x);
                    float posY = outsideY - (i * gridSample.sizeDelta.y);
                    obj.anchoredPosition = new Vector2(posX, posY);
                    obj.gameObject.SetActive(true);
                }
            }

            //�U���t��
            downSpeed = downLevelSpeed[tetrisLobbyMediator.currentSpeed];
            //��ܤU�Ӥ������
            tetrisToolMediator.ShowNextObj(true);
            //�Ыطs���
            CreateNewSquare();

            yield return new WaitForSeconds(0.2f);

            //�C���}�l
            isGameStart = true;
        }

        /// <summary>
        /// �]�w�U�Ӥ��
        /// </summary>
        void SetNextSquare()
        {
            nextIndex = Random.Range(0, shapeSquareNames.Length);//�U�Ӥ��Index
            tetrisToolMediator.CreateNextSquare(nextIndex);
        }

        /// <summary>
        /// �Ыطs���
        /// </summary>
        public void CreateNewSquare()
        {
            isJudging = false;
            //�M�z�ثe������
            controlSquare.squares.Clear();

            //�Ϊ��s��
            currentIndex = nextIndex;
            controlSquare.shapeIndex = currentIndex;//�Ϊ��s��
            rotateIndex = 0;//���s����Index

            //��l���ͦ�m
            float initX = -outsideX + ((int)(lineXCount / 2) * squareSize);
            float initY = outsideY + squareSize;

            RectTransform obj = null;//�������
            switch (currentIndex)
            {
                case 0://�����
                    //�Ыؤ���Ϊ�
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            obj = CreateSquare(shapeSquare_Obj.transform, currentIndex);//�Ыؤ��
                            obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY + (j * -squareSize));
                            controlSquare.squares.Add(obj);
                        }
                    }
                    break;
                case 1://������
                    //�Ыؤ���Ϊ�
                    for (int i = 0; i < 4; i++)
                    {
                        obj = CreateSquare(shapeSquare_Obj.transform, currentIndex);//�Ыؤ��
                        obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY);
                        controlSquare.squares.Add(obj);
                    }
                    break;
                case 2://L����
                    obj = CreateSquare(shapeSquare_Obj.transform, currentIndex);//�Ыؤ��
                    obj.anchoredPosition = new Vector2(initX, initY);
                    controlSquare.squares.Add(obj);
                    for (int i = 0; i < 3; i++)
                    {
                        obj = CreateSquare(shapeSquare_Obj.transform, currentIndex);//�Ыؤ��
                        obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY - squareSize);
                        controlSquare.squares.Add(obj);
                    }
                    break;
                case 3://�g����
                    obj = CreateSquare(shapeSquare_Obj.transform, currentIndex);//�Ыؤ��
                    obj.anchoredPosition = new Vector2(initX, initY);
                    controlSquare.squares.Add(obj);
                    for (int i = 0; i < 3; i++)
                    {
                        obj = CreateSquare(shapeSquare_Obj.transform, currentIndex);//�Ыؤ��
                        obj.anchoredPosition = new Vector2((initX - squareSize) + (i * squareSize), initY - squareSize);
                        controlSquare.squares.Add(obj);
                    }
                    break;
                case 4://Z����
                    for (int i = 0; i < 2; i++)
                    {
                        obj = CreateSquare(shapeSquare_Obj.transform, currentIndex);//�Ыؤ��
                        obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY);
                        controlSquare.squares.Add(obj);
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        obj = CreateSquare(shapeSquare_Obj.transform, currentIndex);//�Ыؤ��
                        obj.anchoredPosition = new Vector2((initX + squareSize) + (i * squareSize), initY - squareSize);
                        controlSquare.squares.Add(obj);
                    }
                    break;
                case 5://��L����
                    obj = CreateSquare(shapeSquare_Obj.transform, currentIndex);//�Ыؤ��
                    obj.anchoredPosition = new Vector2(initX + (2 * squareSize), initY);
                    controlSquare.squares.Add(obj);
                    for (int i = 0; i < 3; i++)
                    {
                        obj = CreateSquare(shapeSquare_Obj.transform, currentIndex);//�Ыؤ��
                        obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY - squareSize);
                        controlSquare.squares.Add(obj);
                    }
                    break;
            }

            //����ʵe�ɪ�
            if (cleanAniTime == 0)
            {
                AnimationClip[] clips = obj.GetComponent<Animator>().runtimeAnimatorController.animationClips;
                foreach (var item in clips)
                {
                    if (item.name.Equals("Tetris_SquareClean"))
                    {
                        cleanAniTime = item.length;
                    }
                }
            }

            //�]�w�U�Ӥ��
            SetNextSquare();
            //�Ыص���
            tetrisToolMediator.CreateGhostPiece(controlSquare.squares, currentIndex);
            //���ʤ@�ӧP�_�O�_���I��
            SquareMove(0, -1);
        }

        /// <summary>
        /// �Ыؤ��
        /// </summary>
        /// <param name="container">�e������</param>
        /// <param name="shapeIndex">�Ϊ��s��</param>
        /// <returns></returns>
        public RectTransform CreateSquare(Transform container, int shapeIndex)
        {
            int poolNum = objectPool.SerchObjId(objPoolName[0], objectPoolDic);//�j�������ID
            RectTransform obj = objectPool.OnActiveObject(poolNum, container).GetComponent<RectTransform>();
            obj.SetParent(container);
            obj.rotation = Quaternion.Euler(Vector3.zero);
            obj.sizeDelta = new Vector2(squareSize, squareSize);
            obj.gameObject.SetActive(true);
            if (obj.TryGetComponent<Image>(out Image img))
            {
                img.color = shapeSquareColors[shapeIndex];
            }

            return obj;
        }

        /// <summary>
        /// �Ыت����
        /// </summary>
        void CreateObjPool()
        {
            //���
            squareSample.gameObject.SetActive(false);
            int poolNum = objectPool.OnCreateAndRecordObject(squareSample.gameObject, shapeSquare_Obj);
            objectPoolDic.Add(objPoolName[0], poolNum);

            //����
            ghostPieceSample.gameObject.SetActive(false);
            poolNum = objectPool.OnCreateAndRecordObject(ghostPieceSample.gameObject, shapeSquare_Obj);
            objectPoolDic.Add(objPoolName[1], poolNum);
        }

        /// <summary>
        /// ����Ҧ���{
        /// </summary>
        public void StopThisAllCoroutines()
        {
            StopAllCoroutines();
        }
    }

    /// <summary>
    /// ��e�ާ@���
    /// </summary>
    public class CurrentControlSquare
    {
        static CurrentControlSquare currentControlSquare = null;
        public static CurrentControlSquare Instance
        {
            get
            {
                if (currentControlSquare == null)
                {
                    currentControlSquare = new CurrentControlSquare();
                }
                return currentControlSquare;
            }
        }

        public int shapeIndex;//�Ϊ��s��
        public List<RectTransform> squares = new List<RectTransform>();//�C�Ӥp���

        private CurrentControlSquare() { }
    }
}