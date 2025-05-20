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

    public MonsterStatsLoader statsLoader;
    public MonsterData monsterData;
    public List<Pool> pools;         // Ǯ ����� �ν����Ϳ��� ������ �� �ֵ��� ����
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private List<GameObject> activeObjects = new List<GameObject>(); // Ȱ��ȭ�� ������Ʈ ����Ʈ

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
        activeObjects.Add(objectToSpawn); // Ȱ��ȭ�� ������Ʈ ����Ʈ�� �߰�
        poolDictionary[tag].Enqueue(objectToSpawn); // ����� ������Ʈ�� �ٽ� ť�� �ֱ�
        
        DemoPlayer demoPlayer = objectToSpawn.GetComponent<DemoPlayer>();
        if (demoPlayer != null)
        {
            MonsterStat stat = statsLoader.GetMonsterStatByName(tag);
            if (stat != null)
            {
                demoPlayer.SetStats(stat); // DemoPlayer���� ���� ���� �޼��� ȣ��
            }
            else
            {
                Debug.LogWarning($"Monster stat not found for {tag}");
            }
        }
        Wolf wolf = objectToSpawn.GetComponent<Wolf>();
        if (wolf != null)
        {
            MonsterStat stat = statsLoader.GetMonsterStatByName(tag);
            if (stat != null)
            {
                wolf.SetStats(stat); // DemoPlayer���� ���� ���� �޼��� ȣ��
            }
            else
            {
                Debug.LogWarning($"Monster stat not found for {tag}");
            }
        }
        return objectToSpawn;
    }

    // Ư�� �±��� ������Ʈ�� ��Ȱ��ȭ�Ͽ� ��ȯ
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        activeObjects.Remove(obj); // Ȱ��ȭ�� ������Ʈ ����Ʈ���� ����
    }
    public void ReturnAllObjects()
    {
        foreach (GameObject obj in activeObjects.ToArray()) // �迭 ���� �� ��ȸ�Ͽ� ����Ʈ ���� ������ Ȯ��
        {
            ReturnObject(obj);
        }
        activeObjects.Clear(); // ����Ʈ �ʱ�ȭ
    }

}

