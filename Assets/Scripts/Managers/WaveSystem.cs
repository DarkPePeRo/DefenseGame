using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveSystem : MonoBehaviour
{
    public int currentWave = 1;
    public int requiredEnemies = 10;
    public int enemyCountPerWave;
    public int enemyCount;
    public bool isBossSpawned = false;

    public Spawn spawn;
    public TextMeshProUGUI stage;
    public MultiPrefabPool pool;
    public GameObject BossPrefab;

    public GameObject defeatedUI;
    private CanvasGroup defeatedCanvasGroup;
    public GameObject winUI;
    private CanvasGroup winCanvasGroup;
    public GameObject bossUI;
    private CanvasGroup bossCanvasGroup;
    public GameObject bossHPUI;
    public Slider bossHPSlider;
    public TextMeshProUGUI bossHPText;

    public float[] healthMultipliers;
    public int[] goldRewards;
    public float[] goldMultiplier;

    public EndLine endLine;

    void Start()
    {
        defeatedCanvasGroup = defeatedUI.GetComponent<CanvasGroup>();
        winCanvasGroup = winUI.GetComponent<CanvasGroup>();
        bossCanvasGroup = bossUI.GetComponent<CanvasGroup>();

        PlayFabStageService.Load((loadedStages, highestStage) => {
            if (loadedStages.Count > 0)
                currentWave = loadedStages[loadedStages.Count - 1];
            StartWave();
        });
    }

    public void StartWave()
    {
        pool.ReturnAllObjects();
        enemyCountPerWave = requiredEnemies;
        enemyCount = enemyCountPerWave;
        isBossSpawned = false;
        stage.text = "Stage : " + currentWave.ToString();

        if (enemyCount > 0)
        {
            Debug.Log("EnemySpawn");
            spawn.StartCoroutine(spawn.SpawnEnemy("WolfA"));
            spawn.StartCoroutine(spawn.SpawnEnemy("SkeletonTest"));
        }
    }

    public void AgainWave()
    {
        Debug.Log("Again");
        pool.ReturnAllObjects();
        StartCoroutine(FadeInDefeatedUI());
        spawn.StopCoroutine(spawn.SpawnEnemy("WolfA"));
        spawn.StopCoroutine(spawn.SpawnEnemy("SkeletonTest"));
        Invoke("StartWave", 2f);
        spawn.currentSpawnCount = 0;
        currentWave--;
        endLine.isEnd = false;
    }

    public void AgainWaveCharacterChanged()
    {
        spawn.StopCoroutine(spawn.SpawnEnemy("WolfA"));
        spawn.StopCoroutine(spawn.SpawnEnemy("SkeletonTest"));
        pool.ReturnAllObjects();
        Invoke("StartWave", 2f);
        spawn.currentSpawnCount = 0;
    }

    public void OnEnemyDefeated()
    {
        enemyCount--;
        if (enemyCount <= 0 && !isBossSpawned)
        {
            spawn.StartCoroutine(spawn.SpawnEnemy("Boss"));
            isBossSpawned = true;
            //StartCoroutine(FadeInBossUI());
        }
    }
    public void OnBossDefeated()
    {
        currentWave++;

        PlayFabStageService.RequestStageClear(currentWave); // 서버 검증 요청
        var reward = new Dictionary<ResourceType, int>{
            { ResourceType.Gold, 800 },
            { ResourceType.Diamond, 500 }
        };
        ResourceRewardEffectManager.Instance.PlayMultiResourceReward(reward);
        spawn.currentSpawnCount = 0;
        //StartCoroutine(FadeInWinUI());
        StartWave();
        isBossSpawned = false;
    }

    public float GetHealthMultiplier()
    {
        if (currentWave - 1 < healthMultipliers.Length)
            return healthMultipliers[currentWave/100];
        return 1.0f;
    }

    public int GetGoldReward()
    {
        if (currentWave - 1 < goldRewards.Length)
            return goldRewards[currentWave/100];
        return 100;
    }

    public float GetGoldMultiplier()
    {
        if (currentWave - 1 < goldRewards.Length)
            return goldMultiplier[currentWave / 100];
        return 0;
    }
    private IEnumerator FadeInDefeatedUI()
    {
        if (defeatedCanvasGroup != null)
        {
            defeatedUI.SetActive(true);
            float duration = 1f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                defeatedCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            yield return new WaitForSeconds(1f);
            StartCoroutine(FadeOutDefeatedUI());
        }
    }

    private IEnumerator FadeOutDefeatedUI()
    {
        if (defeatedCanvasGroup != null)
        {
            float duration = 1f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                defeatedCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            defeatedUI.SetActive(false);
        }
    }

    private IEnumerator FadeInBossUI()
    {
        if (bossCanvasGroup != null)
        {
            bossUI.SetActive(true);
            float duration = 1f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bossCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            yield return new WaitForSeconds(1f);
            StartCoroutine(FadeOutBossUI());
        }
    }

    private IEnumerator FadeOutBossUI()
    {
        if (bossCanvasGroup != null)
        {
            float duration = 1f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bossCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            bossUI.SetActive(false);
        }
    }

    private IEnumerator FadeInWinUI()
    {
        if (winCanvasGroup != null)
        {
            winUI.SetActive(true);
            float duration = 1f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                winCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            yield return new WaitForSeconds(1f);
            StartCoroutine(FadeOutWinUI());
        }
    }

    private IEnumerator FadeOutWinUI()
    {
        if (winCanvasGroup != null)
        {
            float duration = 1f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                winCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            winUI.SetActive(false);
        }
    }
}
