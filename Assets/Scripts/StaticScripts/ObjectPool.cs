using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 靜態腳本
/// </summary>
namespace StaticScripts
{
    /// <summary>
    /// 物件池
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
        /// 創建與紀錄物件
        /// </summary>
        /// <param name="obj">物件</param>
        /// <param name="parent">父物件</param>
        /// <returns></returns>
        public int OnCreateAndRecordObject(GameObject obj, Transform parent)
        {
            if (obj == null)
            {
                Debug.LogError("物件池沒有找到創建物件");
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
        /// 激活物件
        /// </summary>
        /// <param name="number">激活物件編號</param>
        /// <param name="parent">父物件</param>
        /// <returns></returns>
        public GameObject OnActiveObject(int number, Transform parent)
        {
            if (number < 0 || number > allObjectPool_List.Count - 1)
            {
                Debug.LogError("激活物件_物件池ID錯誤:" + number);
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
        /// 物件池物件刪除
        /// </summary>
        /// <param name="number">刪除編號</param>
        public void OnDestroyObject(int number)
        {
            if (number < 0 || number > allObjectPool_List.Count - 1)
            {
                Debug.LogError("物件池物件刪除_物件池ID錯誤:" + number);
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
        /// 清理物件池
        /// </summary>
        /// <param name="cleanDic">清理的字典</param>
        public void OnCleanPool(Dictionary<string, int> cleanDic)
        {
            //取出要清理的物件
            List<List<TemporaryObject>> cleanObjList = new List<List<TemporaryObject>>();
            List<GameObject> cleanType = new List<GameObject>();
            foreach (var item in cleanDic)
            {
                cleanObjList.Add(allObjectPool_List[item.Value]);
                cleanType.Add(objectType_List[item.Value]);
            }

            //移除
            for (int i = 0; i < cleanObjList.Count; i++)
            {                
                allObjectPool_List.Remove(cleanObjList[i]);
                objectType_List.Remove(cleanType[i]);
            }
        }

        /// <summary>
        /// 隱藏物件
        /// </summary>
        /// <param name="number">隱藏物件編號</param>
        public void OnEnableObj(int number)
        {
            if (number < 0 || number > allObjectPool_List.Count -1)
            {
                Debug.LogError("隱藏物件_物件池ID錯誤:" + number);
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
        /// 獲取物件池同類且顯示物件List
        /// </summary>
        /// <param name="number">物件編號</param>
        public List<GameObject> GetObjs(int number)
        {
            if (number < 0 || number > allObjectPool_List.Count - 1)
            {
                Debug.LogError("隱藏物件_物件池ID錯誤:" + number);
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
        /// 搜索物件池ID
        /// </summary>
        /// <param name="serchName">搜索物件名稱</param>
        /// <param name="dic">查找的字典</param>
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
