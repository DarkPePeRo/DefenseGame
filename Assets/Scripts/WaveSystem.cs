using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaveSystem : MonoBehaviour
{
    public int currentWave = 1;
    public int requiredEnemies = 10; // ��ƾ� �ϴ� DemoPlayer ��
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
        enemyCountPerWave = requiredEnemies + (currentWave - 1) * 5; // ���̺긶�� �� �� ����
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
        // DemoPlayer�� �����ϴ� ������ ���⿡ ����
    }

    void SpawnBoss()
    {
        Debug.Log("BossSpawn");
        // ���� ���� ������ �����Ƿ�, �ӽ÷� 3�� �Ŀ� OnBossDefeated�� ȣ��
        StartCoroutine(BossDefeatSimulation());

    }
    public IEnumerator BossDefeatSimulation()
    {
        yield return new WaitForSeconds(3); // �ӽ� ��� �ð�
        OnBossDefeated(); // 3�� �� ���� ���̺�� ����
    }
}
