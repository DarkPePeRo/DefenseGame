using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public MultiPrefabPool objectPool;
    public float Timer;
    public float spawnDelay;
    public int maxSpawnCount = 100; // 최대 스폰 수 제한
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
        // 오브젝트가 다시 풀로 반환될 때 호출되어 스폰 카운트를 줄임
        currentSpawnCount--;
        if (currentSpawnCount < 0) currentSpawnCount = 0; // 음수 방지
    }

    public IEnumerator SpawnEnemy()
    {
        while(currentSpawnCount < waveSystem.enemyCountPerWave)
        {
            Timer += Time.deltaTime;

            // 일정 간격으로 스폰 및 최대 수 제한 확인
            if (Timer > spawnDelay && currentSpawnCount < maxSpawnCount)
            {
                GameObject monster = objectPool.GetObject("Skeleton");

                if (monster != null) // 오브젝트가 풀에서 정상적으로 반환된 경우만 진행
                {
                    monster.transform.position = transform.position;
                    Timer = 0;
                    currentSpawnCount++; // 현재 스폰된 수 증가
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
