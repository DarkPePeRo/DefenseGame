using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Spawn : MonoBehaviour
{
    public MultiPrefabPool objectPool;
    public float spawnDelay;
    public int maxSpawnCount = 200;
    public int currentSpawnCount = 0;
    public WaveSystem waveSystem;
    public EndLine endLine;

    public List<Transform> spawnPoints = new(); // 여러 스폰 위치
    public int currentIndex = 0;

    void Start()
    {
        objectPool = GameObject.Find("PoolManager").GetComponent<MultiPrefabPool>();
        if (objectPool == null)
            Debug.LogError("Object Pool not found!");

        // 스폰 위치 자동 수집 (원하면 수동으로 할당 가능)
        foreach (Transform child in transform)
            spawnPoints.Add(child);
    }

    public void OnMonsterDespawn()
    {
        currentSpawnCount = Mathf.Max(0, currentSpawnCount - 1);
    }

    public IEnumerator SpawnEnemy(string enemy)
    {
        float nextSpawnTime = Time.time;

        while (currentSpawnCount < waveSystem.enemyCountPerWave)
        {
            if (endLine.isEnd)
            {
                waveSystem.AgainWave();
                currentSpawnCount = 0;
                yield break;
            }

            if (Time.time >= nextSpawnTime)
            {
                if (currentSpawnCount < maxSpawnCount)
                {
                    GameObject monster = objectPool.GetObject(enemy);

                    if (monster != null && spawnPoints.Count > 0)
                    {
                        Transform spawnPoint = spawnPoints[currentIndex];
                        monster.transform.position = spawnPoint.position;

                        // 인덱스 순환
                        currentIndex = (currentIndex + 1) % spawnPoints.Count;

                        currentSpawnCount++;
                    }
                }
                nextSpawnTime = Time.time + spawnDelay;
            }

            yield return null;
        }
    }
}

