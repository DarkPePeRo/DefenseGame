using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public MultiPrefabPool objectPool;
    public float Timer;
    public float spawnDelay;
    public int maxSpawnCount = 200; // �ִ� ���� �� ����
    public int currentSpawnCount = 0;
    public WaveSystem waveSystem;
    public EndLine endLine;

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
        float nextSpawnTime = Time.time; // ���� ���� �ð�

        while (currentSpawnCount < waveSystem.enemyCountPerWave)
        {
            if (endLine.isEnd)
            {
                waveSystem.AgainWave();
                currentSpawnCount = 0;
                yield break; // �� ���̺긦 �����ϱ� ���� �ڷ�ƾ ����
            }


            if (Time.time >= nextSpawnTime) // ���� ���� Ȯ��
            {
                if (currentSpawnCount < maxSpawnCount)
                {
                    GameObject monster = objectPool.GetObject("Skeleton");

                    if (monster != null)
                    {
                        monster.transform.position = transform.position;
                        currentSpawnCount++;
                    }
                    else
                    {
                        Debug.LogWarning("No available objects in pool for 'Skeleton'.");
                    }
                }
                // ���� ���� �ð� ����
                nextSpawnTime = Time.time + spawnDelay;
            }

            yield return null; // �� ������ �ڷ�ƾ ����
        }
    }

}
