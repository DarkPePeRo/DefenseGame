using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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

    public Transform startPosition;
    public Transform battlePosition;
    public Transform endPosition;
    public ShadowController shadow;
    public float shadowEnterDuration = 1f;
    public float shadowOutDuration = 1.2f;
    private Tween shadowMoveTween;
    private Tween gridMoveTween;

    public GameObject grid1;
    public GameObject grid2;
    private GameObject currentGrid;
    private GameObject nextGrid;
    public Transform gridMovePosition1;
    public Transform gridMovePosition2;
    public Transform gridMovePosition3;

    private bool isStageClearProcessing = false;

    void Start()
    {
        defeatedCanvasGroup = defeatedUI.GetComponent<CanvasGroup>();
        winCanvasGroup = winUI.GetComponent<CanvasGroup>();
        bossCanvasGroup = bossUI.GetComponent<CanvasGroup>();
        currentGrid = grid1;
        nextGrid = grid2;
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

        StartCoroutine(StartWaveRoutine());
    }
    private IEnumerator StartWaveRoutine()
    {
        // 1. 그림자 전투 정지
        if (shadow != null)
            shadow.SetBattleEnabled(false);

        // 2. 시작 위치로 배치
        shadow.gameObject.transform.position = startPosition.position;

        // 3. 전투 위치까지 이동
        shadowMoveTween?.Kill();

        bool moveDone = false;

        shadowMoveTween = shadow.gameObject.transform
            .DOMove(battlePosition.position, shadowEnterDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                moveDone = true;
            });

        yield return new WaitUntil(() => moveDone);

        // 4. 몬스터 스폰
        if (enemyCount > 0)
        {
            spawn.StartCoroutine(spawn.SpawnEnemy("WolfA"));
            spawn.StartCoroutine(spawn.SpawnEnemy("SkeletonTest"));
        }

        // 5. 전투 시작
        if (shadow != null)
            shadow.SetBattleEnabled(true);
    }
    private IEnumerator EndWaveRoutine()
    {
        if (shadow != null)
            shadow.SetBattleEnabled(false);

        shadowMoveTween?.Kill();

        bool moveDone = false;
        Sequence seq = DOTween.Sequence();
        seq.Append(
            shadow.gameObject.transform
            .DOMove(endPosition.position, shadowOutDuration)
            .SetEase(Ease.OutCubic));

        seq.Append(
            currentGrid.gameObject.transform
            .DOMove(gridMovePosition1.position, shadowOutDuration)
            .SetEase(Ease.OutCubic));

        seq.Join(
            nextGrid.gameObject.transform
            .DOMove(gridMovePosition2.position, shadowOutDuration)
            .SetEase(Ease.InCubic));
        seq.Join(
            shadow.gameObject.transform
            .DOMove(startPosition.position, shadowEnterDuration)
            .SetEase(Ease.OutCubic));
        seq.OnComplete(() =>
        {
            moveDone = true;
            });

        yield return new WaitUntil(() => moveDone);
        currentGrid.gameObject.transform.position = gridMovePosition3.position;
        GameObject oldGrid = currentGrid;
        currentGrid = nextGrid;
        nextGrid = oldGrid;
        StartWave();
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
        if (isStageClearProcessing) return;
        isStageClearProcessing = true;

        int clearedWave = currentWave + 1;

        PlayFabStageService.RequestStageClear(
            clearedWave,
            onSuccess: (result) =>
            {
                currentWave = clearedWave;

                var reward = new Dictionary<ResourceType, int>
                {
                { ResourceType.Gold, 1000 },
                { ResourceType.Diamond, 1000 }
                };

                ResourceRewardEffectManager.Instance.PlayMultiResourceReward(reward);

                spawn.currentSpawnCount = 0;
                isBossSpawned = false;
                StartCoroutine(EndWaveRoutine());

                isStageClearProcessing = false;
            },
            onFail: (error) =>
            {
                isStageClearProcessing = false;

                // 여기서 다음 웨이브 시작 금지
                // 재시도 또는 재동기화 UI
                Debug.LogError($"[StageClear] 최종 실패: {error}");
            });
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
        return goldMultiplier[currentWave / 10];
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
