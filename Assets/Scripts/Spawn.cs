using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public MultiPrefabPool objectPool;
    public float Timer;
    public float spawnDelay;
    public int maxSpawnCount = 100; // �ִ� ���� �� ����
    public int currentSpawnCount = 0;
    public WaveSystem waveSystem;

    void Start()
    {
        objectPool = GameObject.Find("PoolManager").GetComponent<MultiPrefabPool>();
        if (objectPool == null)
        {
            Debug.LogError("Object Pool not found! Please assign a PoolManager with MultiPrefabPool component.");
        }
    }

    void Update()
    {
    }

    public void OnMonsterDespawn()
    {
        // ������Ʈ�� �ٽ� Ǯ�� ��ȯ�� �� ȣ��Ǿ� ���� ī��Ʈ�� ����
        currentSpawnCount--;
        if (currentSpawnCount < 0) currentSpawnCount = 0; // ���� ����
    }

    public IEnumerator SpawnEnemy()
    {
        while(currentSpawnCount < waveSystem.enemyCountPerWave)
        {
            Timer += Time.deltaTime;

            // ���� �������� ���� �� �ִ� �� ���� Ȯ��
            if (Timer > spawnDelay && currentSpawnCount < maxSpawnCount)
            {
                GameObject monster = objectPool.GetObject("Skeleton");

                if (monster != null) // ������Ʈ�� Ǯ���� ���������� ��ȯ�� ��츸 ����
                {
                    monster.transform.position = transform.position;
                    Timer = 0;
                    currentSpawnCount++; // ���� ������ �� ����
                }
                else
                {
                    Debug.LogWarning("No available objects in pool for 'Skeleton'.");
                }
            }
            yield return null;
        }
    }
}
