using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

#region JSON Data Classes

[Serializable]
public class GodStatsParamData
{
    public List<GodStatParam> stats;
}

[Serializable]
public class GodStatParam
{
    public string statType;
    public float baseValue;
    public float stepValue;
    public int maxLevel;
    public List<CostSegment> costSegments;
}

[Serializable]
public class CostSegment
{
    public int from;
    public int to;
    public int baseCost;
    public float rate;
}

#endregion

public class GodStatManage : MonoBehaviour
{
    [Header("Config (Loaded from JSON)")]
    public GodStatsParamData godStatsData;

    [Header("Runtime Flags")]
    private bool isInitialized = false;

    [Header("Save")]
    public GodStatSaveData statLevels = new();
    private float saveTimer = 0f;
    private const float saveDelay = 2f;
    private bool saveRequested = false;

    [Header("Current Values")]
    public float attackPower = 100;
    public float attackSpeed = 10;
    public float criticalRate = 10;
    public float criticalDamage = 10;

    [Header("Current Gold Required (Next Level Cost)")]
    public int attackPowerGold = 100;
    public int attackSpeedGold = 100;
    public int criticalRateGold = 100;
    public int criticalDamageGold = 100;

    [Header("Loop")]
    private Coroutine levelUpLoopCoroutine;
    private string currentLoopStatType;
    private bool cancelLoopRequested;

    private bool isLeveling;

    private readonly Dictionary<string, GodStatParam> paramCache = new Dictionary<string, GodStatParam>(8);

    private void Awake()
    {
        StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        yield return LoadGodStats_Co();

        if (godStatsData == null || godStatsData.stats == null || godStatsData.stats.Count == 0)
        {
            Debug.LogError("[GodStat] Init failed: godStatsData is null/empty. GodStats.json 로딩 실패 또는 포맷 오류.");
            yield break;
        }

        BuildParamCache();

        bool playfabLoaded = false;

        PlayFabGodStatService.Load((loaded, createdNew) =>
        {
            statLevels = loaded;

            ApplyCurrentFromLevel("attackPower");
            ApplyCurrentFromLevel("attackSpeed");
            ApplyCurrentFromLevel("criticalRate");
            ApplyCurrentFromLevel("criticalDamage");

            if (createdNew)
            {
                PlayFabGodStatService.Save(statLevels);
                Debug.Log("[GodStat] No existing stats -> created defaults and saved once.");
            }

            isInitialized = true;
            playfabLoaded = true;

            Debug.Log("[GodStat] Initialized OK (param json + PlayFab stats loaded)");
        });

        float timeout = 10f;
        while (!playfabLoaded && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (!playfabLoaded)
        {
            Debug.LogError("[GodStat] PlayFabGodStatService.Load timeout. isInitialized=false");
        }
    }

    private void Update()
    {
        if (!isInitialized) return;

        if (saveRequested)
        {
            saveTimer += Time.deltaTime;
            if (saveTimer >= saveDelay)
            {
                saveTimer = 0f;
                saveRequested = false;

                PlayFabGodStatService.Save(statLevels);
                Debug.Log("[GodStat] Save requested -> Save(statLevels)");
            }
        }
    }

    private IEnumerator LoadGodStats_Co()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "GodStats.json");

        Debug.Log($"[GodStat] StreamingAssetsPath={Application.streamingAssetsPath}");
        Debug.Log($"[GodStat] GodStats.json path={path}");

#if UNITY_ANDROID && !UNITY_EDITOR
        using (var req = UnityWebRequest.Get(path))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[GodStat] Failed to load GodStats.json (Android). err={req.error}, path={path}");
                godStatsData = null;
                yield break;
            }

            string jsonData = req.downloadHandler.text;
            godStatsData = JsonUtility.FromJson<GodStatsParamData>(jsonData);

            Debug.Log($"[GodStat] Loaded GodStats.json (Android). bytes={jsonData?.Length ?? 0}, null={godStatsData == null}");
        }
#else
        if (!File.Exists(path))
        {
            Debug.LogError($"[GodStat] GodStats.json NOT FOUND. path={path}");
            godStatsData = null;
            yield break;
        }

        string jsonData = File.ReadAllText(path);
        godStatsData = JsonUtility.FromJson<GodStatsParamData>(jsonData);

        Debug.Log($"[GodStat] Loaded GodStats.json (Editor/PC). bytes={jsonData?.Length ?? 0}, null={godStatsData == null}");
        yield return null;
#endif
    }

    private void BuildParamCache()
    {
        paramCache.Clear();

        foreach (var p in godStatsData.stats)
        {
            if (p == null || string.IsNullOrEmpty(p.statType)) continue;
            paramCache[p.statType] = p;
        }

        Debug.Log($"[GodStat] Param cache built. count={paramCache.Count}");
    }
    public void RequestLevelUp(string statType)
    {
        if (!isInitialized) return;
        TryLevelUpOnce(statType);
    }

    public void StartStatLevelUpLoop(string statType)
    {
        if (!isInitialized) return;
        if (levelUpLoopCoroutine != null) return;

        cancelLoopRequested = false;
        currentLoopStatType = statType;
        levelUpLoopCoroutine = StartCoroutine(LevelUpLoop());
    }

    public void StopStatLevelUpLoop()
    {
        cancelLoopRequested = true;

        if (levelUpLoopCoroutine != null)
        {
            StopCoroutine(levelUpLoopCoroutine);
            levelUpLoopCoroutine = null;
        }
    }

    private IEnumerator LevelUpLoop()
    {
        float interval = 0.3f;
        const float minInterval = 0.05f;
        const float speedUpRate = 0.95f;

        yield return null;

        while (!cancelLoopRequested)
        {
            bool leveled = TryLevelUpOnce(currentLoopStatType);
            if (!leveled) break;

            if (IsMaxLevel(currentLoopStatType)) break;

            yield return new WaitForSeconds(interval);

            interval *= speedUpRate;
            if (interval < minInterval) interval = minInterval;
        }

        levelUpLoopCoroutine = null;
        Debug.Log("[GodStat] LevelUpLoop ended");
    }

    private bool TryLevelUpOnce(string statType)
    {
        if (!isInitialized) return false;
        if (isLeveling) return false;

        isLeveling = true;
        try
        {
            if (PlayerCurrency.Instance == null) return false;
            if (!paramCache.TryGetValue(statType, out var param) || param == null) return false;

            int curLevel = GetCurrentLevel(statType);
            int nextLevel = curLevel + 1;

            if (nextLevel > param.maxLevel) return false;

            int nextCost = CalcNextCost(param, nextLevel);
            int gold = PlayerCurrency.Instance.gold.amount;

            if (gold < nextCost) return false;

            PlayerCurrency.Instance.gold.amount = gold - nextCost;

            SetCurrentLevel(statType, nextLevel);

            float newValue = CalcValue(param, nextLevel);
            SetCurrentValue(statType, newValue);

            int newNextCost = (nextLevel < param.maxLevel) ? CalcNextCost(param, nextLevel + 1) : nextCost;
            SetCurrentGold(statType, newNextCost);

            saveRequested = true;

            StatUpgradeUI.Instance?.UpdateUI();
            return true;
        }
        finally
        {
            isLeveling = false;
        }
    }
    private float CalcValue(GodStatParam param, int level)
    {
        return param.baseValue + (level * param.stepValue);
    }

    private int CalcNextCost(GodStatParam param, int nextLevel)
    {
        var seg = FindSegment(param, nextLevel);
        if (seg == null)
            return int.MaxValue;

        int exponent = nextLevel - seg.from;
        if (exponent < 0) exponent = 0;

        double cost = seg.baseCost * Math.Pow(seg.rate, exponent);

        double ceiled = Math.Ceiling(cost);

        if (ceiled > int.MaxValue) return int.MaxValue;
        return (int)ceiled;
    }

    private CostSegment FindSegment(GodStatParam param, int level)
    {
        if (param.costSegments == null) return null;

        for (int i = 0; i < param.costSegments.Count; i++)
        {
            var s = param.costSegments[i];
            if (s == null) continue;
            if (level >= s.from && level <= s.to) return s;
        }
        return null;
    }

    public void ApplyCurrentFromLevel(string statType)
    {
        if (!paramCache.TryGetValue(statType, out var param) || param == null) return;

        int level = GetCurrentLevel(statType);
        level = Mathf.Clamp(level, 1, param.maxLevel);

        SetCurrentLevel(statType, level);
        SetCurrentValue(statType, CalcValue(param, level));

        int nextCost = (level < param.maxLevel) ? CalcNextCost(param, level + 1) : CalcNextCost(param, level);
        SetCurrentGold(statType, nextCost);
    }
    public int GetCurrentLevel(string statType) => statType switch
    {
        "attackPower" => statLevels.attackPower,
        "attackSpeed" => statLevels.attackSpeed,
        "criticalRate" => statLevels.criticalRate,
        "criticalDamage" => statLevels.criticalDamage,
        _ => 1,
    };

    public float GetStatValue(string statType) => statType switch
    {
        "attackPower" => attackPower,
        "attackSpeed" => attackSpeed,
        "criticalRate" => criticalRate,
        "criticalDamage" => criticalDamage,
        _ => 0f
    };

    public int GetStatGold(string statType) => statType switch
    {
        "attackPower" => attackPowerGold,
        "attackSpeed" => attackSpeedGold,
        "criticalRate" => criticalRateGold,
        "criticalDamage" => criticalDamageGold,
        _ => 0
    };

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

    public bool IsMaxLevel(string statType)
    {
        if (!paramCache.TryGetValue(statType, out var param) || param == null) return false;
        return GetCurrentLevel(statType) >= param.maxLevel;
    }
}
