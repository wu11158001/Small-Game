using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StaticScripts;
using Entry;

/// <summary>
/// 遊戲場景
/// </summary>
namespace GameScene
{
    /// <summary>
    /// 俄羅斯方塊
    /// </summary>
    public class TetrisMediator : MonoBehaviour
    {
        public ObjectPool objectPool;//物件池
        AudioManager audioManager;//音效管理
        InputManager inputManager;//輸入控制

        [SerializeField] TetrisToolMediator tetrisToolMediator;
        TetrisLobbyMediator tetrisLobbyMediator;

        [Header("預設資料")]
        [Tooltip("物件池名稱")]
        public readonly string[] objPoolName =
{
            "Square",//方塊
            "GhostPiece",//虛方塊
        };
        [Tooltip("方塊大小")] readonly int squareSize = 70;
        [Tooltip("下降加速速度")] readonly float accelerrateSpeed = 0.04f;
        [Tooltip("下降速度等級(秒)")]
        public readonly float[] downLevelSpeed =
        {
            1, 0.9f, 0.8f, 0.6f, 0.4f, 0.2f, 0.09f, 0.07f, 0.05f,
        };
        [Tooltip("可旋轉角度Z")] readonly int[] rotateZ = { 0, 90, 180, 270 };
        [Tooltip("物件池名稱_形狀方塊")]
        readonly string[] shapeSquareNames =
        {
            "shape_Square",//正方形
            "shape_Strip",//條狀形
            "shape_L",//L狀形
            "shape_T",//土狀形
            "shape_Z",//Z狀形
            "shape_ReversL",//反L狀形
        };
        [Tooltip("加速分數")] public readonly int addSpeedScore = 200;

        [Header("動畫")]
        [Tooltip("動畫名稱")]
        readonly string[] animatorName =
        {
            "Clean",//方塊消除
        };
        [Tooltip("動畫HashId")] int[] animatorHashId;
        [Tooltip("清除動畫時長")] float cleanAniTime;

        [Header("元件")]
        [Tooltip("方格背景")] [SerializeField] RectTransform GridBg_Img;
        [Tooltip("方格背景容器")] [SerializeField] RectTransform gridContent;
        [Tooltip("方格樣本")] [SerializeField] RectTransform gridSample;
        [Tooltip("方塊樣本")] [SerializeField] RectTransform squareSample;
        [Tooltip("方塊形狀容器")] [SerializeField] RectTransform shapeSquare_Obj;
        [Tooltip("虛方塊樣本")] public RectTransform ghostPieceSample;

        [Header("物件池")]
        [Tooltip("物件池(物件名稱,編號)")] public Dictionary<string, int> objectPoolDic = new Dictionary<string, int>();

        [Header("方塊形狀")]
        [Tooltip("方塊顏色")] public Color[] shapeSquareColors;

        [Header("控制")]
        [Tooltip("是否開始遊戲")] public bool isGameStart;
        [Tooltip("當前操作方塊")] public CurrentControlSquare controlSquare;
        [Tooltip("方塊下降等級")] public int downLevel;
        [Tooltip("方塊下降計時器")] float downTime;
        [Tooltip("方塊下降速度(秒)")] public float downSpeed;
        [Tooltip("當前旋轉角度Index")] [SerializeField] int rotateIndex;
        [Tooltip("邊界X座標")] [SerializeField] float outsideX;
        [Tooltip("邊界Y座標")] [SerializeField] float outsideY;
        [Tooltip("X數量")] [SerializeField] int lineXCount;
        [Tooltip("Y數量")] public int lineYCount;
        [Tooltip("當前方塊Index")][SerializeField] int currentIndex;
        [Tooltip("下個方塊Index")] [SerializeField] int nextIndex;
        [Tooltip("紀錄旋轉前方塊位置")] Vector2[] originalPos;
        [Tooltip("紀錄旋轉前旋轉角度")] int originalRotateIndex;

        [Header("觸碰")]
        [Tooltip("起始觸碰點")] Vector2 initTouchPos;
        [Tooltip("判斷是否有移動過")] bool isTouchMove;
        [Tooltip("紀錄首次觸碰時間")] float begainTouchTime;

        [Header("連線判斷")]
        [Tooltip("是否正在判斷")] [SerializeField] bool isJudging;
        [Tooltip("每行Y位置")] [SerializeField] float[] linePosYList;
        [Tooltip("紀錄停止方塊")] public List<RectTransform> stopSquaresList = new List<RectTransform>();

        private void Awake()
        {
            objectPool = ObjectPool.Instance;//物件池
            tetrisLobbyMediator = FindObjectOfType<TetrisLobbyMediator>();
            audioManager = AudioManager.Instance;//音效管理
            inputManager = InputManager.Instance;//輸入控制

            controlSquare = CurrentControlSquare.Instance;//當前操作方塊

            //動畫HashId
            animatorHashId = new int[animatorName.Length];
            for (int i = 0; i < animatorName.Length; i++)
            {
                animatorHashId[i] = Animator.StringToHash(animatorName[i]);
            }

            //邊界
            outsideX = (GridBg_Img.sizeDelta.x / 2) - (squareSize / 2);
            outsideY = (GridBg_Img.sizeDelta.y / 2) - (squareSize / 2);

            isGameStart = false;//是否開始遊戲

            //行列數量
            lineXCount = (int)(((GridBg_Img.sizeDelta.x / 2) / squareSize) * 2);
            lineYCount = (int)(((GridBg_Img.sizeDelta.y / 2) / squareSize) * 2);

            //每行Y位置
            linePosYList = new float[lineYCount];
            for (int i = 0; i < linePosYList.Length; i++)
            {
                linePosYList[i] = outsideY - (squareSize * i);
            }

            //下個方塊Index
            nextIndex = Random.Range(0, shapeSquareNames.Length);

            GridBg_Img.gameObject.SetActive(false);//方格背景

            CreateObjPool();//創建物件池
        }

        private void Update()
        {
            //輸入控制
            OnInputControl();
            //當前操作方塊移動
            OnCurrentSquareMove();
        }

        /// <summary>
        /// 輸入控制
        /// </summary>
        void OnInputControl()
        {
#if UNITY_EDITOR
            //遊戲開始 && 有方塊 && 非暫停 
            if (isGameStart == true && controlSquare.squares.Count > 0 && !tetrisLobbyMediator.isPause)
            {
                //碰到按鈕類Return
                if (inputManager.IsOnUIElement() == true) return;

                //重製速度
                downSpeed = downLevelSpeed[tetrisLobbyMediator.currentSpeed];

                //非在判斷連線
                if (!isJudging)
                {
                    //旋轉
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        //播放按鈕點擊音效
                        audioManager.OnPlayButtonClickSound();
                        //更換角度
                        ChangeRotate();
                        //監測是否超出邊界
                        MonitorOutside();
                        //監測是否與其他方塊位置重複
                        MonitorDuplicates();
                        //創建虛方塊
                        tetrisToolMediator.CreateGhostPiece(controlSquare.squares, currentIndex);
                    }
                    //左
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        //播放按鈕點擊音效
                        audioManager.OnPlayButtonClickSound();
                        //方塊移動
                        SquareMove(-1, 0);
                        //創建虛方塊
                        tetrisToolMediator.CreateGhostPiece(controlSquare.squares, currentIndex);
                    }
                    //右
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        //播放按鈕點擊音效
                        audioManager.OnPlayButtonClickSound();
                        //方塊移動
                        SquareMove(1, 0);
                        //創建虛方塊
                        tetrisToolMediator.CreateGhostPiece(controlSquare.squares, currentIndex);
                    }

                    //瞬間下降
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        audioManager.SetPlaySound("Rapid");
                        downSpeed = 0;
                    }
                }
                
                //下
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
        //觸碰控制
        OnTouchControl();
#endif
        }

        /// <summary>
        /// 觸碰控制
        /// </summary>
        void OnTouchControl()
        {
            //遊戲開始 && 有方塊 && 非暫停
            if (isGameStart == true && controlSquare.squares.Count > 0 && !tetrisLobbyMediator.isPause)
            {
                //重製速度
                downSpeed = downLevelSpeed[tetrisLobbyMediator.currentSpeed];

                //碰到按鈕類Return
                if (inputManager.IsOnUIElement() == true) return;

                float touchSensitivity = 70;

                if (Input.touchCount == 1)
                {
                    switch (Input.GetTouch(0).phase)
                    {
                        //首次觸碰
                        case TouchPhase.Began:
                            isTouchMove = false;
                            begainTouchTime = Time.realtimeSinceStartup;
                            initTouchPos = Input.GetTouch(0).position;
                            break;

                        //移動
                        case TouchPhase.Moved:
                            //非在判斷連線
                            if (!isJudging)
                            {
                                //左右移動
                                if (Input.GetTouch(0).position.x > initTouchPos.x + touchSensitivity)
                                {
                                    isTouchMove = true;
                                    audioManager.OnPlayButtonClickSound();
                                    initTouchPos = Input.GetTouch(0).position;
                                    SquareMove(1, 0);//方塊移動
                                    //創建虛方塊
                                    tetrisToolMediator.CreateGhostPiece(controlSquare.squares, currentIndex);
                                }
                                else if (Input.GetTouch(0).position.x < initTouchPos.x - touchSensitivity)
                                {
                                    isTouchMove = true;
                                    audioManager.OnPlayButtonClickSound();
                                    initTouchPos = Input.GetTouch(0).position;
                                    SquareMove(-1, 0);//方塊移動
                                    //創建虛方塊
                                    tetrisToolMediator.CreateGhostPiece(controlSquare.squares, currentIndex);
                                }
                            }

                            //向下加速移動
                            if (Input.GetTouch(0).position.y < initTouchPos.y - (touchSensitivity * 2))
                            {
                                isTouchMove = true;
                                downSpeed = accelerrateSpeed;
                            }

                            //停止加速
                            if (Input.GetTouch(0).position.y > initTouchPos.y + touchSensitivity)
                            {
                                isTouchMove = true;
                                //速度還原
                                downSpeed = downLevelSpeed[tetrisLobbyMediator.currentSpeed];
                            }
                            break;

                        //觸碰結束
                        case TouchPhase.Ended:
                            //速度還原
                            downSpeed = downLevelSpeed[tetrisLobbyMediator.currentSpeed];
                            //觸碰結束時間
                            float endedTime = Time.realtimeSinceStartup;

                            //非在判斷連線
                            if (!isJudging)
                            {
                                //瞬間下降
                                if (endedTime - begainTouchTime < 0.2f && Input.GetTouch(0).position.y < initTouchPos.y - (touchSensitivity / 2))
                                {
                                    isTouchMove = true;
                                    audioManager.SetPlaySound("Rapid");
                                    downSpeed = 0;
                                }
                            }

                            //未移動過
                            if (!isTouchMove)
                            {
                                audioManager.OnPlayButtonClickSound();//播放按鈕點擊音效
                                //更換角度
                                ChangeRotate();
                                //監測是否超出邊界
                                MonitorOutside();
                                //監測是否與其他方塊位置重複
                                MonitorDuplicates();
                                //創建虛方塊
                                tetrisToolMediator.CreateGhostPiece(controlSquare.squares, currentIndex);
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 更換角度
        /// </summary>
        void ChangeRotate()
        {
            //紀錄旋轉前方塊位置
            originalPos = new Vector2[controlSquare.squares.Count];
            for (int i = 0; i < originalPos.Length; i++)
            {
                originalPos[i] = controlSquare.squares[i].anchoredPosition;
            }
            //紀錄旋轉前旋轉角度
            originalRotateIndex = rotateIndex;

            //角度切換
            rotateIndex = rotateIndex++ >= rotateZ.Length - 1 ? 0 : rotateIndex++;

            switch (controlSquare.shapeIndex)
            {
                case 0://正方形
                    break;
                case 1://條狀形
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
                case 2://L狀形
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
                case 3://土狀形
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
                case 4://Z狀形
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
                case 5://反L狀形
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
        /// 監測是否超出邊界
        /// </summary>
        void MonitorOutside()
        {
            int positiveX = 0;//大於X數量
            int negativeX = 0;//小於X數量
            int negativeY = 0;//小於Y數量
            for (int i = 0; i < controlSquare.squares.Count; i++)
            {
                float x = controlSquare.squares[i].anchoredPosition.x;
                float y = controlSquare.squares[i].anchoredPosition.y;

                if (x > outsideX) positiveX++;//超過右側邊界
                else if (x < -outsideX) negativeX++;//超過左側邊界
                else if (y < -outsideY) negativeY++;//超過底部邊界
            }

            //修正位置
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
        /// 監測是否與其他方塊位置重複
        /// </summary>
        void MonitorDuplicates()
        {
            //檢測是否碰到方塊
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

            //有碰到方塊
            if (isDuplicates)
            {
                //回到原位置與旋轉
                rotateIndex = originalRotateIndex;

                for (int i = 0; i < controlSquare.squares.Count; i++)
                {
                    controlSquare.squares[i].anchoredPosition = originalPos[i];
                }
            }
        }

        /// <summary>
        /// 當前操作方塊移動
        /// </summary>
        void OnCurrentSquareMove()
        {
            if (isGameStart)
            {
                //非判斷連線 && 有方塊
                if (!isJudging && controlSquare.squares.Count != 0)
                {
                    //方塊下降
                    downTime += Time.deltaTime;
                    if (downTime >= downSpeed)
                    {
                        //重製下降時間
                        downTime = 0;

                        //方塊移動
                        SquareMove(0, -1);
                    }
                }
            }
        }

        /// <summary>
        /// 方塊移動
        /// </summary>
        /// <param name="valueX">移動變量X</param>
        /// <param name="valueY">移動變量Y</param>
        void SquareMove(int valueX, int valueY)
        {
            //防呆
            if (valueX >= 1) valueX = 1;
            else if (valueX <= -1) valueX = -1;
            if (controlSquare.squares.Count == 0) return;

            //檢測移動是否超出邊界
            bool[] outside = IsOutside(controlSquare.squares, valueX, valueY);
            bool isOutside = outside[0];
            bool isBotton = outside[1];

            //檢測是否碰到方塊
            if (IsTouchOtherSquare(controlSquare.squares, valueX, valueY) == true)
            {
                //向下移動
                if (valueY == -1) isBotton = true;
                else if (valueX != 0) isOutside = true;
            }

            //已達最底部 || 碰到底部方塊
            if (isBotton)
            {
                for (int i = 0; i < controlSquare.squares.Count; i++)
                {
                    //紀錄停止位置
                    stopSquaresList.Add(controlSquare.squares[i]);
                }

                //判斷連線消除
                StartCoroutine(IJudgeLine());
                return;
            }

            //可移動
            if (!isBotton && !isOutside)
            {
                for (int i = 0; i < controlSquare.squares.Count; i++)
                {
                    float x = controlSquare.squares[i].anchoredPosition.x + (squareSize * valueX);
                    float y = controlSquare.squares[i].anchoredPosition.y + (squareSize * valueY);
                    controlSquare.squares[i].anchoredPosition = new Vector2(x, y);
                }
            }

            //急速下降
            if (downSpeed == 0) SquareMove(0, -1);
        }

        /// <summary>
        /// 檢測是否碰到方塊
        /// </summary>
        /// <param name="judgeSquareList">要判斷的方塊List</param>
        /// <param name="dirX">檢測方向X</param>
        /// <param name="dirY">檢測方向Y</param>
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
        /// 檢測是否超出邊界(0=超出左右邊界, 1=超出最底部)
        /// </summary>
        /// <param name="judgeSquareList">要判斷的方塊List</param>
        /// <param name="dirX">檢測方向X</param>
        /// <param name="dirY">檢測方向Y</param>
        /// <returns></returns>
        public bool[] IsOutside(List<RectTransform> judgeSquareList, int dirX, int dirY)
        {
            //(0=超出左右邊界, 1=超出最底部)
            bool[] result = new bool[2];

            for (int i = 0; i < judgeSquareList.Count; i++)
            {
                float judgeX = judgeSquareList[i].anchoredPosition.x + (squareSize * dirX);
                float judgeY = judgeSquareList[i].anchoredPosition.y + (squareSize * dirY);

                //已在兩側最邊界
                if (judgeX < -outsideX || judgeX > outsideX)
                {
                    //超出左右邊界
                    result[0] = true;
                    break;
                }
                else if (judgeY < -outsideY)//達最底部
                {
                    //超出最底部
                    result[1] = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// 判斷連線消除
        /// </summary>
        IEnumerator IJudgeLine()
        {
            isJudging = true;//正在判斷連線
            int bottomNum = 0;//連線最底部的編號
            int lineCount = 0;//連線條數

            //紀錄相同位置
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

            //消除動畫
            List<RectTransform> cleanList = new List<RectTransform>();//紀錄要消除的方塊
            for (int i = 0; i < recodeList.Length; i++)
            {
                if (recodeList[i].Count == lineXCount)
                {
                    lineCount++;//連線條數
                    bottomNum = bottomNum < i ? i : bottomNum;//連線最底部的編號
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

            //延遲
            float yieldTime = cleanList.Count > 0 ? cleanAniTime + 0.1f : 0;
            yield return new WaitForSeconds(yieldTime);

            //撥放得分音效
            if (lineCount > 0) audioManager.OnPlayScoreSound();

            //設定分數
            tetrisLobbyMediator.SetScore(lineCount);

            //消除方塊
            foreach (var item in cleanList)
            {
                if (item != null)
                {
                    item.gameObject.SetActive(false);
                    stopSquaresList.Remove(item);
                }
            }

            //消除之上的方塊下降
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

            //判斷方塊是否超出最上側
            for (int i = 0; i < stopSquaresList.Count; i++)
            {
                if (stopSquaresList[i].anchoredPosition.y > outsideY)
                {
                    //遊戲結束
                    tetrisLobbyMediator.OnGameOver(false);
                }
            }

            //急速下降還原
            if (downSpeed == 0) downSpeed = downLevelSpeed[tetrisLobbyMediator.currentSpeed]; ;

            //創建新方塊
            if (isGameStart) CreateNewSquare();
        }

        /// <summary>
        /// 創建方格
        /// </summary>
        public IEnumerator ICreateGrid()
        {
            GridBg_Img.gameObject.SetActive(true);//方格背景

            gridSample.sizeDelta = new Vector2(squareSize, squareSize);//方格Size
            gridSample.gameObject.SetActive(false);

            int row = (int)(GridBg_Img.sizeDelta.x / squareSize);//橫排數量
            int column = (int)(GridBg_Img.sizeDelta.y / squareSize);//縱向數量
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

            //下降速度
            downSpeed = downLevelSpeed[tetrisLobbyMediator.currentSpeed];
            //顯示下個方塊物件
            tetrisToolMediator.ShowNextObj(true);
            //創建新方塊
            CreateNewSquare();

            yield return new WaitForSeconds(0.2f);

            //遊戲開始
            isGameStart = true;
        }

        /// <summary>
        /// 設定下個方塊
        /// </summary>
        void SetNextSquare()
        {
            nextIndex = Random.Range(0, shapeSquareNames.Length);//下個方塊Index
            tetrisToolMediator.CreateNextSquare(nextIndex);
        }

        /// <summary>
        /// 創建新方塊
        /// </summary>
        public void CreateNewSquare()
        {
            isJudging = false;
            //清理目前控制方塊
            controlSquare.squares.Clear();

            //形狀編號
            currentIndex = nextIndex;
            controlSquare.shapeIndex = currentIndex;//形狀編號
            rotateIndex = 0;//重製旋轉Index

            //初始產生位置
            float initX = -outsideX + ((int)(lineXCount / 2) * squareSize);
            float initY = outsideY + squareSize;

            RectTransform obj = null;//方塊物件
            switch (currentIndex)
            {
                case 0://正方形
                    //創建方塊形狀
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            obj = CreateSquare(shapeSquare_Obj.transform, currentIndex);//創建方塊
                            obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY + (j * -squareSize));
                            controlSquare.squares.Add(obj);
                        }
                    }
                    break;
                case 1://條狀形
                    //創建方塊形狀
                    for (int i = 0; i < 4; i++)
                    {
                        obj = CreateSquare(shapeSquare_Obj.transform, currentIndex);//創建方塊
                        obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY);
                        controlSquare.squares.Add(obj);
                    }
                    break;
                case 2://L狀形
                    obj = CreateSquare(shapeSquare_Obj.transform, currentIndex);//創建方塊
                    obj.anchoredPosition = new Vector2(initX, initY);
                    controlSquare.squares.Add(obj);
                    for (int i = 0; i < 3; i++)
                    {
                        obj = CreateSquare(shapeSquare_Obj.transform, currentIndex);//創建方塊
                        obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY - squareSize);
                        controlSquare.squares.Add(obj);
                    }
                    break;
                case 3://土狀形
                    obj = CreateSquare(shapeSquare_Obj.transform, currentIndex);//創建方塊
                    obj.anchoredPosition = new Vector2(initX, initY);
                    controlSquare.squares.Add(obj);
                    for (int i = 0; i < 3; i++)
                    {
                        obj = CreateSquare(shapeSquare_Obj.transform, currentIndex);//創建方塊
                        obj.anchoredPosition = new Vector2((initX - squareSize) + (i * squareSize), initY - squareSize);
                        controlSquare.squares.Add(obj);
                    }
                    break;
                case 4://Z狀形
                    for (int i = 0; i < 2; i++)
                    {
                        obj = CreateSquare(shapeSquare_Obj.transform, currentIndex);//創建方塊
                        obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY);
                        controlSquare.squares.Add(obj);
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        obj = CreateSquare(shapeSquare_Obj.transform, currentIndex);//創建方塊
                        obj.anchoredPosition = new Vector2((initX + squareSize) + (i * squareSize), initY - squareSize);
                        controlSquare.squares.Add(obj);
                    }
                    break;
                case 5://反L狀形
                    obj = CreateSquare(shapeSquare_Obj.transform, currentIndex);//創建方塊
                    obj.anchoredPosition = new Vector2(initX + (2 * squareSize), initY);
                    controlSquare.squares.Add(obj);
                    for (int i = 0; i < 3; i++)
                    {
                        obj = CreateSquare(shapeSquare_Obj.transform, currentIndex);//創建方塊
                        obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY - squareSize);
                        controlSquare.squares.Add(obj);
                    }
                    break;
            }

            //獲取動畫時長
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

            //設定下個方塊
            SetNextSquare();
            //創建虛方塊
            tetrisToolMediator.CreateGhostPiece(controlSquare.squares, currentIndex);
            //移動一個判斷是否有碰撞
            SquareMove(0, -1);
        }

        /// <summary>
        /// 創建方塊
        /// </summary>
        /// <param name="container">容器物件</param>
        /// <param name="shapeIndex">形狀編號</param>
        /// <returns></returns>
        public RectTransform CreateSquare(Transform container, int shapeIndex)
        {
            int poolNum = objectPool.SerchObjId(objPoolName[0], objectPoolDic);//搜索物件池ID
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
        /// 創建物件池
        /// </summary>
        void CreateObjPool()
        {
            //方塊
            squareSample.gameObject.SetActive(false);
            int poolNum = objectPool.OnCreateAndRecordObject(squareSample.gameObject, shapeSquare_Obj);
            objectPoolDic.Add(objPoolName[0], poolNum);

            //虛方塊
            ghostPieceSample.gameObject.SetActive(false);
            poolNum = objectPool.OnCreateAndRecordObject(ghostPieceSample.gameObject, shapeSquare_Obj);
            objectPoolDic.Add(objPoolName[1], poolNum);
        }

        /// <summary>
        /// 停止所有協程
        /// </summary>
        public void StopThisAllCoroutines()
        {
            StopAllCoroutines();
        }
    }

    /// <summary>
    /// 當前操作方塊
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

        public int shapeIndex;//形狀編號
        public List<RectTransform> squares = new List<RectTransform>();//每個小方塊

        private CurrentControlSquare() { }
    }
}