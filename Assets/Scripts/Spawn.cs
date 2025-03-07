using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public MultiPrefabPool objectPool;
    public float Timer;
    public float spawnDelay;
    public int maxSpawnCount = 200; // 최대 스폰 수 제한
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
        // 오브젝트가 다시 풀로 반환될 때 호출되어 스폰 카운트를 줄임
        currentSpawnCount--;
        if (currentSpawnCount < 0) currentSpawnCount = 0; // 음수 방지
    }
    public IEnumerator SpawnEnemy()
    {
        float nextSpawnTime = Time.time; // 다음 스폰 시간

        while (currentSpawnCount < waveSystem.enemyCountPerWave)
        {
            if (endLine.isEnd)
            {
                waveSystem.AgainWave();
                currentSpawnCount = 0;
                yield break; // 새 웨이브를 시작하기 위해 코루틴 종료
            }


            if (Time.time >= nextSpawnTime) // 스폰 간격 확인
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
                // 다음 스폰 시간 갱신
                nextSpawnTime = Time.time + spawnDelay;
            }

            yield return null; // 매 프레임 코루틴 지속
        }
    }

}
