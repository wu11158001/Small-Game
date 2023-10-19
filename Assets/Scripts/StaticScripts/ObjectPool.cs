using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �R�A�}��
/// </summary>
namespace StaticScripts
{
    /// <summary>
    /// �����
    /// </summary>
    public class ObjectPool
    {
        static ObjectPool objectPool;
        public static ObjectPool Instance
        {
            get
            {
                if (objectPool == null)
                {
                    objectPool = new ObjectPool();
                }
                return objectPool;
            }
        }

        List<List<TemporaryObject>> allObjectPool_List = new List<List<TemporaryObject>>();
        List<GameObject> objectType_List = new List<GameObject>();

        /// <summary>
        /// �ЫػP��������
        /// </summary>
        /// <param name="obj">����</param>
        /// <param name="parent">������</param>
        /// <returns></returns>
        public int OnCreateAndRecordObject(GameObject obj, Transform parent)
        {
            if (obj == null)
            {
                Debug.LogError("������S�����Ыت���");
                return default;
            }
            TemporaryObject temparrayObject = new TemporaryObject();
            temparrayObject.obj = GameObject.Instantiate(obj, parent);
            temparrayObject.obj.SetActive(false);

            objectType_List.Add(temparrayObject.obj);
            List<TemporaryObject> temporary = new List<TemporaryObject>();
            temporary.Add(temparrayObject);
            allObjectPool_List.Add(temporary);

            return allObjectPool_List.Count - 1;
        }

        /// <summary>
        /// �E������
        /// </summary>
        /// <param name="number">�E������s��</param>
        /// <param name="parent">������</param>
        /// <returns></returns>
        public GameObject OnActiveObject(int number, Transform parent)
        {
            if (number < 0 || number > allObjectPool_List.Count - 1)
            {
                Debug.LogError("�E������_�����ID���~:" + number);
                return null;
            }
            List<TemporaryObject> temparray = allObjectPool_List[number];

            for (int i = 0; i < temparray.Count; i++)
            {
                if (!temparray[i].obj.activeSelf)
                {
                    temparray[i].obj.SetActive(true);
                    return temparray[i].obj;
                }
            }

            TemporaryObject copy = new TemporaryObject();
            copy.obj = GameObject.Instantiate(objectType_List[number], parent);
            copy.obj.SetActive(true);
            allObjectPool_List[number].Add(copy);
            return copy.obj;
        }

        /// <summary>
        /// ���������R��
        /// </summary>
        /// <param name="number">�R���s��</param>
        public void OnDestroyObject(int number)
        {
            if (number < 0 || number > allObjectPool_List.Count - 1)
            {
                Debug.LogError("���������R��_�����ID���~:" + number);
                return;
            }

            List<TemporaryObject> temparray_List = allObjectPool_List[number];

            temparray_List[0].obj.SetActive(false);
            for (int i = 1; i < temparray_List.Count; i++)
            {
                GameObject.Destroy(temparray_List[i].obj);
            }

            TemporaryObject temporary = new TemporaryObject();
            temporary.obj = temparray_List[0].obj;
            List<TemporaryObject> record_Lists = new List<TemporaryObject>();
            record_Lists.Add(temporary);

            allObjectPool_List[number] = record_Lists;
        }

        /// <summary>
        /// �M�z�����
        /// </summary>
        /// <param name="cleanDic">�M�z���r��</param>
        public void OnCleanPool(Dictionary<string, int> cleanDic)
        {
            //���X�n�M�z������
            List<List<TemporaryObject>> cleanObjList = new List<List<TemporaryObject>>();
            List<GameObject> cleanType = new List<GameObject>();
            foreach (var item in cleanDic)
            {
                cleanObjList.Add(allObjectPool_List[item.Value]);
                cleanType.Add(objectType_List[item.Value]);
            }

            //����
            for (int i = 0; i < cleanObjList.Count; i++)
            {                
                allObjectPool_List.Remove(cleanObjList[i]);
                objectType_List.Remove(cleanType[i]);
            }
        }

        /// <summary>
        /// ���ê���
        /// </summary>
        /// <param name="number">���ê���s��</param>
        public void OnEnableObj(int number)
        {
            if (number < 0 || number > allObjectPool_List.Count -1)
            {
                Debug.LogError("���ê���_�����ID���~:" + number);
                return;
            }

            List<TemporaryObject> temparray_List = allObjectPool_List[number];

            temparray_List[0].obj.SetActive(false);
            for (int i = 1; i < temparray_List.Count; i++)
            {
                temparray_List[i].obj.SetActive(false);
            }
        }

        /// <summary>
        /// ���������P���B��ܪ���List
        /// </summary>
        /// <param name="number">����s��</param>
        public List<GameObject> GetObjs(int number)
        {
            if (number < 0 || number > allObjectPool_List.Count - 1)
            {
                Debug.LogError("���ê���_�����ID���~:" + number);
                return null;
            }

            List<GameObject> temparray_List = new List<GameObject>();
            for (int i = 0; i < allObjectPool_List[number].Count; i++)
            {
                if (allObjectPool_List[number][i].obj.activeSelf)
                {
                    temparray_List.Add(allObjectPool_List[number][i].obj);
                }  
            }

            return temparray_List;
        }

        /// <summary>
        /// �j�������ID
        /// </summary>
        /// <param name="serchName">�j������W��</param>
        /// <param name="dic">�d�䪺�r��</param>
        /// <returns></returns>
        public int SerchObjId(string serchName, Dictionary<string, int> dic)
        {
            int num = -1;
            foreach (var item in dic)
            {
                if (item.Key == serchName)
                {
                    num = item.Value;
                }
            }

            return num;
        }
    }

    class TemporaryObject
    {
        public GameObject obj;
    }
}
