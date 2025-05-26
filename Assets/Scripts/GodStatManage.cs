using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GodStatManage : MonoBehaviour
{
    public GodStatsData godStatsData;
    private bool isInitialized = false;

    public GodStatSaveData statLevels = new();
    private float saveTimer = 0f;
    private const float saveDelay = 2f;
    private bool saveRequested = false;

    // 복구된 필드
    public float attackPower = 10;
    public float attackSpeed = 10;
    public float criticalRate = 10;
    public float criticalDamage = 10;

    public int attackPowerGold = 100;
    public int attackSpeedGold = 100;
    public int criticalRateGold = 100;
    public int criticalDamageGold = 100;

    private Coroutine levelUpLoopCoroutine;
    private string currentLoopStatType;

    void Awake()
    {
        LoadGodStats();
        PlayFabGodStatService.Load(() =>
        {
            statLevels = PlayFabGodStatService.CurrentStats;

            GetCurrentValue("attackPower");
            GetCurrentValue("attackSpeed");
            GetCurrentValue("criticalRate");
            GetCurrentValue("criticalDamage");

            isInitialized = true;
        });
    }

    void Update()
    {
        if (saveRequested)
        {
            saveTimer += Time.deltaTime;
            if (saveTimer >= saveDelay)
            {
                saveTimer = 0f;
                saveRequested = false;
                PlayFabGodStatService.Save(statLevels);
            }
        }
    }

    private void LoadGodStats()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "GodStats.json");
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            godStatsData = JsonUtility.FromJson<GodStatsData>(jsonData);
        }
    }

    public void RequestLevelUp(string statType)
    {
        if (!isInitialized) return;
        StartCoroutine(PerformLevelUp(statType));
    }

    public void StartStatLevelUpLoop(string statType)
    {
        if (levelUpLoopCoroutine != null) return;
        currentLoopStatType = statType;
        levelUpLoopCoroutine = StartCoroutine(LevelUpLoop());
    }

    public void StopStatLevelUpLoop()
    {
        if (levelUpLoopCoroutine != null)
        {
            StopCoroutine(levelUpLoopCoroutine);
            levelUpLoopCoroutine = null;
        }
    }


    private IEnumerator LevelUpLoop()
    {
        float interval = 0.3f;            // 시작 간격
        const float minInterval = 0.05f;  // 최소 간격
        const float speedUpRate = 0.95f;  // 반복할수록 줄어드는 비율
        while (true)
        {
            yield return PerformLevelUp(currentLoopStatType);

            if (IsMaxLevel(currentLoopStatType)) break;

            int currentLevel = GetCurrentLevel(currentLoopStatType);
            var next = GetStatLevels(currentLoopStatType).Find(s => s.level == currentLevel + 1);
            if (next == null || PlayerCurrency.Instance.gold.amount < next.goldRequired)
                break;

            yield return new WaitForSeconds(interval);

            // 속도 점점 빨라짐
            interval *= speedUpRate;
            if (interval < minInterval) interval = minInterval;
        }

        levelUpLoopCoroutine = null;
    }
    private IEnumerator PerformLevelUp(string statType)
{/*
    if (PlayerCurrency.Instance.HasGoldBuffer())
        yield return PlayerCurrency.Instance.StartCoroutine("SaveBufferedGoldToServer");*/

    int currentLevel = GetCurrentLevel(statType);
    var next = GetStatLevels(statType).Find(s => s.level == currentLevel + 1);

    if (next == null || PlayerCurrency.Instance.gold.amount < next.goldRequired)
        yield break;

    SetCurrentLevel(statType, next.level);
    SetCurrentValue(statType, next.value);
    SetCurrentGold(statType, next.goldRequired);
    PlayerCurrency.Instance.gold.amount -= next.goldRequired;

    saveRequested = true;
    StatUpgradeUI.Instance.UpdateUI();
    Debug.Log($"[연속 레벨업] {statType} → Lv.{next.level}");
}

private List<GodStatLevel> GetStatLevels(string statType) => statType switch
{
    "attackPower" => godStatsData.attackPowerLevels,
    "attackSpeed" => godStatsData.attackSpeedLevels,
    "criticalRate" => godStatsData.criticalRateLevels,
    "criticalDamage" => godStatsData.criticalDamageLevels,
    _ => null,
};

public int GetCurrentLevel(string statType) => statType switch
{
    "attackPower" => statLevels.attackPower,
    "attackSpeed" => statLevels.attackSpeed,
    "criticalRate" => statLevels.criticalRate,
    "criticalDamage" => statLevels.criticalDamage,
    _ => 1,
};

public float GetStatValue(string statType)
{
    return statType switch
    {
        "attackPower" => attackPower,
        "attackSpeed" => attackSpeed,
        "criticalRate" => criticalRate,
        "criticalDamage" => criticalDamage,
        _ => 0f
    };
}

public int GetStatGold(string statType)
{
    return statType switch
    {
        "attackPower" => attackPowerGold,
        "attackSpeed" => attackSpeedGold,
        "criticalRate" => criticalRateGold,
        "criticalDamage" => criticalDamageGold,
        _ => 0
    };
}

private void SetCurrentLevel(string statType, int newLevel)
{
    switch (statType)
    {
        case "attackPower": statLevels.attackPower = newLevel; break;
        case "attackSpeed": statLevels.attackSpeed = newLevel; break;
        case "criticalRate": statLevels.criticalRate = newLevel; break;
        case "criticalDamage": statLevels.criticalDamage = newLevel; break;
    }
}

private void SetCurrentValue(string statType, float newValue)
{
    switch (statType)
    {
        case "attackPower": attackPower = newValue; break;
        case "attackSpeed": attackSpeed = newValue; break;
        case "criticalRate": criticalRate = newValue; break;
        case "criticalDamage": criticalDamage = newValue; break;
    }
}

private void SetCurrentGold(string statType, int newGold)
{
    switch (statType)
    {
        case "attackPower": attackPowerGold = newGold; break;
        case "attackSpeed": attackSpeedGold = newGold; break;
        case "criticalRate": criticalRateGold = newGold; break;
        case "criticalDamage": criticalDamageGold = newGold; break;
    }
}

public void GetCurrentValue(string statType)
{
    int currentLevel = GetCurrentLevel(statType);
    var data = GetStatLevels(statType).Find(s => s.level == currentLevel);
    if (data == null) return;

    SetCurrentValue(statType, data.value);
    SetCurrentGold(statType, data.goldRequired);
}

public bool IsMaxLevel(string statType)
{
    int checkLevel = GetCurrentLevel(statType);
    return checkLevel == 27;
}
}
