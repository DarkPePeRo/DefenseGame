using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;       // Ǯ���� ������Ʈ�� ������
    public int poolSize = 10;       // Ǯ�� �ʱ� ũ��
    private Queue<GameObject> pool; // ��� ������ ������Ʈ���� �����ϴ� ť

    void Start()
    {
        pool = new Queue<GameObject>();

        // �ʱ� Ǯ ũ�⸸ŭ ������Ʈ ���� �� ��Ȱ��ȭ�Ͽ� ť�� �߰�
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    // Ǯ���� ������Ʈ�� ��������
    public GameObject GetObject()
    {
        // ���� Ǯ�� ������Ʈ�� ������ ���� �����Ͽ� �߰�
        if (pool.Count == 0)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        // ť���� ������Ʈ�� ���� Ȱ��ȭ�ϰ� ��ȯ
        GameObject pooledObject = pool.Dequeue();
        pooledObject.SetActive(true);
        return pooledObject;
    }

    // ������Ʈ�� Ǯ�� ��ȯ�ϱ�
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false); // ��Ȱ��ȭ�ϰ� �ٽ� ť�� �߰�
        pool.Enqueue(obj);
    }
}
