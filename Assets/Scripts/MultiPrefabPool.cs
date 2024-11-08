using System.Collections.Generic;
using UnityEngine;

public class MultiPrefabPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;          // Ǯ �±� (�������� �̸� ��)
        public GameObject prefab;   // Ǯ���� ������
        public int size;            // �ʱ� Ǯ ũ��
    }

    public List<Pool> pools;         // Ǯ ����� �ν����Ϳ��� ������ �� �ֵ��� ����
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        // �� �����տ� ���� Ǯ�� �����ϰ� ��ųʸ��� �߰�
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

    // Ư�� �±��� ������Ʈ ��������
    public GameObject GetObject(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        objectToSpawn.SetActive(true);
        poolDictionary[tag].Enqueue(objectToSpawn); // ����� ������Ʈ�� �ٽ� ť�� �ֱ�

        return objectToSpawn;
    }

    // Ư�� �±��� ������Ʈ�� ��Ȱ��ȭ�Ͽ� ��ȯ
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
    }
}

