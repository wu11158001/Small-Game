using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StaticScripts;

/// <summary>
/// 遊戲場景
/// </summary>
namespace GameScene
{
    /// <summary>
    /// 俄羅斯方塊_工具類控制
    /// </summary>
    public class TetrisToolMediator : MonoBehaviour
    {
        [SerializeField] TetrisMediator tetrisMediator;
        TetrisLobbyMediator tetrisLobbyMediator;
        Language language;//語言翻譯

        [Header("預設資料")]
        [Tooltip("方塊大小")] readonly int squareSize = 70;

        [Header("下個方塊")]
        [Tooltip("方格背景")] [SerializeField] RectTransform nextBg_Img;
        [Tooltip("下個方塊方方格背景容器")] [SerializeField] RectTransform gridContent;
        [Tooltip("下個方塊方格樣本")] [SerializeField] RectTransform gridSample;
        [Tooltip("下個方塊方形狀方塊容器")] [SerializeField] RectTransform nextShape_Obj;
        [Tooltip("下一個文字物件")] [SerializeField] Text next_Txt;

        [Header("資料")]
        [Tooltip("紀錄創建的下個方塊")] List<RectTransform> recodeNextObj = new List<RectTransform>();
        [Tooltip("下個方塊初始位置X")] [SerializeField] float nextInitPosX;
        [Tooltip("下個方塊初始位置Y")] [SerializeField] float nextInitPosY;
        [Tooltip("紀錄創建的虛方塊")] List<RectTransform> recodeGhostObj = new List<RectTransform>();

        private void Awake()
        {
            language = Language.Instance;//語言翻譯

            //初始位置
            nextInitPosX = -((nextBg_Img.sizeDelta.x / 2) - (squareSize / 2));
            nextInitPosY = ((nextBg_Img.sizeDelta.y / 2) - (squareSize / 2));

            //顯示下個方塊物件
            ShowNextObj(false);

            //翻譯
            next_Txt.text = language.GetLanguage("Next");//分數
        }

        private void Start()
        {
            //創建方格
            CreateGride();          
        }

        /// <summary>
        /// 顯示下個方塊物件
        /// </summary>
        /// <param name="isShow">是否顯示</param>
        public void ShowNextObj(bool isShow)
        {
            nextBg_Img.gameObject.SetActive(isShow);
        }

        /// <summary>
        /// 創建方格
        /// </summary>
        void CreateGride()
        {
            for (int i = 0; i < nextBg_Img.sizeDelta.y / squareSize; i++)
            {
                for (int j = 0; j < nextBg_Img.sizeDelta.x / squareSize; j++)
                {
                    RectTransform obj = Instantiate(gridSample, gridContent);
                    obj.sizeDelta = new Vector2(squareSize, squareSize);
                    obj.anchoredPosition = new Vector2(nextInitPosX + (j * squareSize), nextInitPosY - (i * squareSize));
                    obj.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// 創建下個方塊
        /// </summary>
        /// <param name="shape">方塊編號</param>
        public void CreateNextSquare(int shape)
        {
            //清除紀錄
            foreach (var item in recodeNextObj)
            {
                item.gameObject.SetActive(false);
            }
            recodeNextObj.Clear();

            //初始產生位置
            float initX = nextInitPosX;
            float initY = nextInitPosY;

            RectTransform obj = null;//方塊物件
            switch (shape)
            {
                case 0://正方形
                    //創建方塊形狀
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            obj = tetrisMediator.CreateSquare(nextShape_Obj.transform, shape);//創建方塊
                            obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY + (j * -squareSize));
                            recodeNextObj.Add(obj);
                        }
                    }
                    break;
                case 1://條狀形
                    //創建方塊形狀
                    for (int i = 0; i < 4; i++)
                    {
                        obj = tetrisMediator.CreateSquare(nextShape_Obj.transform, shape);//創建方塊
                        obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY - squareSize);
                        recodeNextObj.Add(obj);
                    }
                    break;
                case 2://L狀形
                    obj = tetrisMediator.CreateSquare(nextShape_Obj.transform, shape);//創建方塊
                    obj.anchoredPosition = new Vector2(initX, initY);
                    recodeNextObj.Add(obj);
                    for (int i = 0; i < 3; i++)
                    {
                        obj = tetrisMediator.CreateSquare(nextShape_Obj.transform, shape);//創建方塊
                        obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY - squareSize);
                        recodeNextObj.Add(obj);
                    }
                    break;
                case 3://土狀形
                    obj = tetrisMediator.CreateSquare(nextShape_Obj.transform, shape);//創建方塊
                    obj.anchoredPosition = new Vector2(initX + squareSize, initY);
                    recodeNextObj.Add(obj);
                    for (int i = 0; i < 3; i++)
                    {
                        obj = tetrisMediator.CreateSquare(nextShape_Obj.transform, shape);//創建方塊
                        obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY - squareSize);
                        recodeNextObj.Add(obj);
                    }
                    break;
                case 4://Z狀形
                    for (int i = 0; i < 2; i++)
                    {
                        obj = tetrisMediator.CreateSquare(nextShape_Obj.transform, shape);//創建方塊
                        obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY);
                        recodeNextObj.Add(obj);
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        obj = tetrisMediator.CreateSquare(nextShape_Obj.transform, shape);//創建方塊
                        obj.anchoredPosition = new Vector2((initX + squareSize) + (i * squareSize), initY - squareSize);
                        recodeNextObj.Add(obj);
                    }
                    break;
                case 5://反L狀形
                    obj = tetrisMediator.CreateSquare(nextShape_Obj.transform, shape);//創建方塊
                    obj.anchoredPosition = new Vector2(initX + (2 * squareSize), initY);
                    recodeNextObj.Add(obj);
                    for (int i = 0; i < 3; i++)
                    {
                        obj = tetrisMediator.CreateSquare(nextShape_Obj.transform, shape);//創建方塊
                        obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY - squareSize);
                        recodeNextObj.Add(obj);
                    }
                    break;
            }
        }

        /// <summary>
        /// 創建虛方塊
        /// </summary>
        /// <param name="constrolSquares">當前控制方塊</param>
        /// <param name="shapeIndex">形狀Index</param>
        public void CreateGhostPiece(List<RectTransform> constrolSquares, int shapeIndex)
        {
            //虛方塊開啟狀態
            if (tetrisLobbyMediator == null)
            {
                tetrisLobbyMediator = FindObjectOfType<TetrisLobbyMediator>();
            }
            if (tetrisLobbyMediator.GetGhostPieceState == false) return;

            //清除紀錄的虛方塊
            for (int i = 0; i < recodeGhostObj.Count; i++)
            {
                recodeGhostObj[i].gameObject.SetActive(false);
            }
            recodeGhostObj.Clear();

            //創建虛方塊
            for (int i = 0; i < constrolSquares.Count; i++)
            {
                int poolNum = tetrisMediator.objectPool.SerchObjId(tetrisMediator.objPoolName[1], tetrisMediator.objectPoolDic);//搜索物件池ID
                RectTransform obj = tetrisMediator.objectPool.OnActiveObject(poolNum, constrolSquares[i].parent).GetComponent<RectTransform>();
                obj.SetParent(constrolSquares[i].parent);
                obj.rotation = constrolSquares[i].rotation;
                obj.sizeDelta = constrolSquares[i].sizeDelta;
                obj.anchoredPosition = constrolSquares[i].anchoredPosition;
                obj.gameObject.SetActive(true);
                if (obj.TryGetComponent<Image>(out Image img))
                {
                    img.color = tetrisMediator.shapeSquareColors[shapeIndex];
                    img.color = new Color(img.color.r, img.color.g, img.color.b, 0.4f);
                }
                recodeGhostObj.Add(obj);
            }

            //防呆次數
            int actingNum = 0;
            //檢測是否超出邊界
            bool[] isBottom = new bool[2];
            //檢測是否碰到方塊
            bool isTouchSquare = false;
            //虛方塊下降
            while (!isBottom[1] && !isTouchSquare)
            {
                //防呆
                actingNum++;
                if (actingNum > tetrisMediator.lineYCount)
                {
                    return;
                }

                isBottom = tetrisMediator.IsOutside(recodeGhostObj, 0, -1);
                isTouchSquare = tetrisMediator.IsTouchOtherSquare(recodeGhostObj, 0, -1);

                if (!isBottom[1] && !isTouchSquare)
                {
                    for (int i = 0; i < recodeGhostObj.Count; i++)
                    {
                        float x = recodeGhostObj[i].anchoredPosition.x;
                        float y = recodeGhostObj[i].anchoredPosition.y + (squareSize * -1);
                        recodeGhostObj[i].anchoredPosition = new Vector2(x, y);
                    }
                }       
            }
        }
    }
}