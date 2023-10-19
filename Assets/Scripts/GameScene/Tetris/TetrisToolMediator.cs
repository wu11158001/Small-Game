using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StaticScripts;

/// <summary>
/// �C������
/// </summary>
namespace GameScene
{
    /// <summary>
    /// �Xù�����_�u��������
    /// </summary>
    public class TetrisToolMediator : MonoBehaviour
    {
        [SerializeField] TetrisMediator tetrisMediator;
        TetrisLobbyMediator tetrisLobbyMediator;
        Language language;//�y��½Ķ

        [Header("�w�]���")]
        [Tooltip("����j�p")] readonly int squareSize = 70;

        [Header("�U�Ӥ��")]
        [Tooltip("���I��")] [SerializeField] RectTransform nextBg_Img;
        [Tooltip("�U�Ӥ������I���e��")] [SerializeField] RectTransform gridContent;
        [Tooltip("�U�Ӥ�����˥�")] [SerializeField] RectTransform gridSample;
        [Tooltip("�U�Ӥ����Ϊ�����e��")] [SerializeField] RectTransform nextShape_Obj;
        [Tooltip("�U�@�Ӥ�r����")] [SerializeField] Text next_Txt;

        [Header("���")]
        [Tooltip("�����Ыت��U�Ӥ��")] List<RectTransform> recodeNextObj = new List<RectTransform>();
        [Tooltip("�U�Ӥ����l��mX")] [SerializeField] float nextInitPosX;
        [Tooltip("�U�Ӥ����l��mY")] [SerializeField] float nextInitPosY;
        [Tooltip("�����Ыت�����")] List<RectTransform> recodeGhostObj = new List<RectTransform>();

        private void Awake()
        {
            language = Language.Instance;//�y��½Ķ

            //��l��m
            nextInitPosX = -((nextBg_Img.sizeDelta.x / 2) - (squareSize / 2));
            nextInitPosY = ((nextBg_Img.sizeDelta.y / 2) - (squareSize / 2));

            //��ܤU�Ӥ������
            ShowNextObj(false);

            //½Ķ
            next_Txt.text = language.GetLanguage("Next");//����
        }

        private void Start()
        {
            //�Ыؤ��
            CreateGride();          
        }

        /// <summary>
        /// ��ܤU�Ӥ������
        /// </summary>
        /// <param name="isShow">�O�_���</param>
        public void ShowNextObj(bool isShow)
        {
            nextBg_Img.gameObject.SetActive(isShow);
        }

        /// <summary>
        /// �Ыؤ��
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
        /// �ЫؤU�Ӥ��
        /// </summary>
        /// <param name="shape">����s��</param>
        public void CreateNextSquare(int shape)
        {
            //�M������
            foreach (var item in recodeNextObj)
            {
                item.gameObject.SetActive(false);
            }
            recodeNextObj.Clear();

            //��l���ͦ�m
            float initX = nextInitPosX;
            float initY = nextInitPosY;

            RectTransform obj = null;//�������
            switch (shape)
            {
                case 0://�����
                    //�Ыؤ���Ϊ�
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            obj = tetrisMediator.CreateSquare(nextShape_Obj.transform, shape);//�Ыؤ��
                            obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY + (j * -squareSize));
                            recodeNextObj.Add(obj);
                        }
                    }
                    break;
                case 1://������
                    //�Ыؤ���Ϊ�
                    for (int i = 0; i < 4; i++)
                    {
                        obj = tetrisMediator.CreateSquare(nextShape_Obj.transform, shape);//�Ыؤ��
                        obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY - squareSize);
                        recodeNextObj.Add(obj);
                    }
                    break;
                case 2://L����
                    obj = tetrisMediator.CreateSquare(nextShape_Obj.transform, shape);//�Ыؤ��
                    obj.anchoredPosition = new Vector2(initX, initY);
                    recodeNextObj.Add(obj);
                    for (int i = 0; i < 3; i++)
                    {
                        obj = tetrisMediator.CreateSquare(nextShape_Obj.transform, shape);//�Ыؤ��
                        obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY - squareSize);
                        recodeNextObj.Add(obj);
                    }
                    break;
                case 3://�g����
                    obj = tetrisMediator.CreateSquare(nextShape_Obj.transform, shape);//�Ыؤ��
                    obj.anchoredPosition = new Vector2(initX + squareSize, initY);
                    recodeNextObj.Add(obj);
                    for (int i = 0; i < 3; i++)
                    {
                        obj = tetrisMediator.CreateSquare(nextShape_Obj.transform, shape);//�Ыؤ��
                        obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY - squareSize);
                        recodeNextObj.Add(obj);
                    }
                    break;
                case 4://Z����
                    for (int i = 0; i < 2; i++)
                    {
                        obj = tetrisMediator.CreateSquare(nextShape_Obj.transform, shape);//�Ыؤ��
                        obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY);
                        recodeNextObj.Add(obj);
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        obj = tetrisMediator.CreateSquare(nextShape_Obj.transform, shape);//�Ыؤ��
                        obj.anchoredPosition = new Vector2((initX + squareSize) + (i * squareSize), initY - squareSize);
                        recodeNextObj.Add(obj);
                    }
                    break;
                case 5://��L����
                    obj = tetrisMediator.CreateSquare(nextShape_Obj.transform, shape);//�Ыؤ��
                    obj.anchoredPosition = new Vector2(initX + (2 * squareSize), initY);
                    recodeNextObj.Add(obj);
                    for (int i = 0; i < 3; i++)
                    {
                        obj = tetrisMediator.CreateSquare(nextShape_Obj.transform, shape);//�Ыؤ��
                        obj.anchoredPosition = new Vector2(initX + (i * squareSize), initY - squareSize);
                        recodeNextObj.Add(obj);
                    }
                    break;
            }
        }

        /// <summary>
        /// �Ыص���
        /// </summary>
        /// <param name="constrolSquares">��e������</param>
        /// <param name="shapeIndex">�Ϊ�Index</param>
        public void CreateGhostPiece(List<RectTransform> constrolSquares, int shapeIndex)
        {
            //�����}�Ҫ��A
            if (tetrisLobbyMediator == null)
            {
                tetrisLobbyMediator = FindObjectOfType<TetrisLobbyMediator>();
            }
            if (tetrisLobbyMediator.GetGhostPieceState == false) return;

            //�M������������
            for (int i = 0; i < recodeGhostObj.Count; i++)
            {
                recodeGhostObj[i].gameObject.SetActive(false);
            }
            recodeGhostObj.Clear();

            //�Ыص���
            for (int i = 0; i < constrolSquares.Count; i++)
            {
                int poolNum = tetrisMediator.objectPool.SerchObjId(tetrisMediator.objPoolName[1], tetrisMediator.objectPoolDic);//�j�������ID
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

            //���b����
            int actingNum = 0;
            //�˴��O�_�W�X���
            bool[] isBottom = new bool[2];
            //�˴��O�_�I����
            bool isTouchSquare = false;
            //�����U��
            while (!isBottom[1] && !isTouchSquare)
            {
                //���b
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