using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;       // 풀링할 오브젝트의 프리팹
    public int poolSize = 10;       // 풀의 초기 크기
    private Queue<GameObject> pool; // 사용 가능한 오브젝트들을 보관하는 큐

    void Start()
    {
        pool = new Queue<GameObject>();

        // 초기 풀 크기만큼 오브젝트 생성 및 비활성화하여 큐에 추가
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    // 풀에서 오브젝트를 가져오기
    public GameObject GetObject()
    {
        // 만약 풀에 오브젝트가 없으면 새로 생성하여 추가
        if (pool.Count == 0)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        // 큐에서 오브젝트를 꺼내 활성화하고 반환
        GameObject pooledObject = pool.Dequeue();
        pooledObject.SetActive(true);
        return pooledObject;
    }

    // 오브젝트를 풀에 반환하기
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false); // 비활성화하고 다시 큐에 추가
        pool.Enqueue(obj);
    }
}
