using System.Collections.Generic;
using UnityEngine;

public class MultiPrefabPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;          // 풀 태그 (프리팹의 이름 등)
        public GameObject prefab;   // 풀링할 프리팹
        public int size;            // 초기 풀 크기
    }

    public List<Pool> pools;         // 풀 목록을 인스펙터에서 관리할 수 있도록 설정
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        // 각 프리팹에 대해 풀을 생성하고 딕셔너리에 추가
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    // 특정 태그의 오브젝트 가져오기
    public GameObject GetObject(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        objectToSpawn.SetActive(true);
        poolDictionary[tag].Enqueue(objectToSpawn); // 사용한 오브젝트를 다시 큐에 넣기

        return objectToSpawn;
    }

    // 특정 태그의 오브젝트를 비활성화하여 반환
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
    }
}

