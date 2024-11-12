using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaveSystem : MonoBehaviour
{
    public int currentWave = 1;
    public int requiredEnemies = 10; // 잡아야 하는 DemoPlayer 수
    public int enemyCountPerWave;
    public int enemyCount;
    public bool isBossSpawned = false;
    public Spawn spawn;
    public TextMeshProUGUI stage;

    void Start()
    {
        StartWave();

    }

    void StartWave()
    {
        enemyCountPerWave = requiredEnemies + (currentWave - 1) * 5; // 웨이브마다 적 수 증가
        enemyCount = enemyCountPerWave;
        isBossSpawned = false;
        stage.text = "Stage : "+currentWave.ToString();
        if(enemyCount > 0)
        {
            Debug.Log("EnemySpawn");
            spawn.StartCoroutine("SpawnEnemy");
        }
    }

    public void OnEnemyDefeated()
    {
        enemyCount--;
        if (enemyCount <= 0 && !isBossSpawned)
        {
            SpawnBoss();
            isBossSpawned = true;
        }
    }

    public void OnBossDefeated()
    {
        currentWave++;
        spawn.currentSpawnCount = 0;
        isBossSpawned = false;
        StartWave();
    }

    void SpawnEnemies(int count)
    {
        // DemoPlayer를 스폰하는 로직을 여기에 구현
    }

    void SpawnBoss()
    {
        Debug.Log("BossSpawn");
        // 보스 스폰 로직이 없으므로, 임시로 3초 후에 OnBossDefeated를 호출
        StartCoroutine(BossDefeatSimulation());

    }
    public IEnumerator BossDefeatSimulation()
    {
        yield return new WaitForSeconds(3); // 임시 대기 시간
        OnBossDefeated(); // 3초 후 다음 웨이브로 진행
    }
}
