using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class GodStatManage : MonoBehaviour
{
    public GodStatsData godStatsData;
    private bool isInitialized = false;

    private Queue<string> statQueue = new();
    private Dictionary<string, int> statRequestCount = new();
    private float aggregationTimer = 0f;
    private const float aggregationDelay = 1f;
    private bool isProcessing = false;

    public float attackPower = 10;
    public float attackSpeed = 10;
    public float criticalRate = 10;
    public float criticalDamage = 10;

    public int attackPowerLevel = 1;
    public int attackSpeedLevel = 1;
    public int criticalRateLevel = 1;
    public int criticalDamageLevel = 1;

    public int attackPowerGold = 100;
    public int attackSpeedGold = 100;
    public int criticalRateGold = 100;
    public int criticalDamageGold = 100;

    void Awake()
    {
        LoadGodStats();
        PlayFabStatsService.Load(() =>
        {
            attackPowerLevel = PlayFabStatsService.AttackPowerLevel;
            attackSpeedLevel = PlayFabStatsService.AttackSpeedLevel;
            criticalRateLevel = PlayFabStatsService.CriticalRateLevel;
            criticalDamageLevel = PlayFabStatsService.CriticalDamageLevel;

            GetCurrentValue("attackPower");
            GetCurrentValue("attackSpeed");
            GetCurrentValue("criticalRate");
            GetCurrentValue("criticalDamage");

            isInitialized = true;
        });
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

        StartCoroutine(SaveGoldThenRequest(statType));
    }

    private IEnumerator SaveGoldThenRequest(string statType)
    {
        if (PlayerCurrency.Instance.HasGoldBuffer())
        {
            yield return PlayerCurrency.Instance.StartCoroutine("SaveBufferedGoldToServer");
        }

        int currentLevel = GetCurrentLevel(statType);
        var next = GetStatLevels(statType).Find(s => s.level == currentLevel + 1);
        if (next == null || PlayerCurrency.Instance.gold.amount < next.goldRequired) yield break;

        // 선반영
        SetCurrentLevel(statType, next.level);
        SetCurrentValue(statType, next.value);
        SetCurrentGold(statType, next.goldRequired);
        PlayerCurrency.Instance.gold.amount -= next.goldRequired;

        if (!statRequestCount.ContainsKey(statType))
            statRequestCount[statType] = 0;

        statRequestCount[statType]++;
    }

    void Update()
    {
        if (statRequestCount.Count > 0)
        {
            aggregationTimer += Time.deltaTime;
            if (aggregationTimer >= aggregationDelay)
            {
                foreach (var kvp in statRequestCount)
                    statQueue.Enqueue(kvp.Key + ":" + kvp.Value);

                statRequestCount.Clear();
                aggregationTimer = 0f;
                TryProcessNext();
            }
        }
    }

    private void TryProcessNext()
    {
        if (isProcessing || statQueue.Count == 0) return;

        isProcessing = true;
        string[] parts = statQueue.Dequeue().Split(':');
        string statType = parts[0];
        int count = int.Parse(parts[1]);

        ValidateWithServer(statType, count);
    }

    private void ValidateWithServer(string statType, int count)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "MultiLevelUpGodStat",
            FunctionParameter = new
            {
                statType = statType,
                count = count
            }
        };

        int rollbackLevel = GetCurrentLevel(statType);
        float rollbackValue = GetStatValue(statType);
        int rollbackGold = GetStatGold(statType);
        int rollbackClientGold = PlayerCurrency.Instance.gold.amount;

        PlayFabClientAPI.ExecuteCloudScript(request, result =>
        {
            if (result.FunctionResult == null)
            {
                Debug.LogWarning($"[검증 실패: 응답 없음] {statType}");
                RollbackStat(statType, rollbackLevel, rollbackValue, rollbackGold, rollbackClientGold);
                OnValidationComplete();
                return;
            }

            var data = JsonUtility.FromJson<GodStatServerResult>(result.FunctionResult.ToString());
            if (data.newLevel <= 0)
            {
                Debug.LogWarning($"[서버 응답 오류] {statType} → newLevel이 0 이하");
                RollbackStat(statType, rollbackLevel, rollbackValue, rollbackGold, rollbackClientGold);
                OnValidationComplete();
                return;
            }

            int localLevel = GetCurrentLevel(statType);
            if (localLevel != data.newLevel)
            {
                SetCurrentLevel(statType, data.newLevel);
                SetCurrentValue(statType, data.newValue);
                SetCurrentGold(statType, data.newGoldRequired);
                PlayerCurrency.Instance.gold.amount = data.remainingGold;

                Debug.LogWarning($"[서버 동기화] {statType} Lv.{data.newLevel}로 교체됨");
                StatUpgradeUI.Instance.UpdateUI();
            }

            OnValidationComplete();
        },
        error =>
        {
            Debug.LogError($"[서버 검증 실패] {statType}: {error.GenerateErrorReport()}");
            RollbackStat(statType, rollbackLevel, rollbackValue, rollbackGold, rollbackClientGold);
            OnValidationComplete();
        });
    }

    private void RollbackStat(string statType, int level, float value, int requiredGold, int clientGold)
    {
        SetCurrentLevel(statType, level);
        SetCurrentValue(statType, value);
        SetCurrentGold(statType, requiredGold);
        PlayerCurrency.Instance.gold.amount = clientGold;

        Debug.LogWarning($"[롤백] {statType} Lv.{level}로 복원됨");
        StatUpgradeUI.Instance.UpdateUI();
    }

    private void OnValidationComplete()
    {
        StartCoroutine(WaitAndNext());
    }

    private IEnumerator WaitAndNext()
    {
        yield return new WaitForSeconds(0.2f);
        isProcessing = false;
        TryProcessNext();
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
        "attackPower" => attackPowerLevel,
        "attackSpeed" => attackSpeedLevel,
        "criticalRate" => criticalRateLevel,
        "criticalDamage" => criticalDamageLevel,
        _ => 1,
    };

    private void SetCurrentLevel(string statType, int newLevel)
    {
        switch (statType)
        {
            case "attackPower": attackPowerLevel = newLevel; break;
            case "attackSpeed": attackSpeedLevel = newLevel; break;
            case "criticalRate": criticalRateLevel = newLevel; break;
            case "criticalDamage": criticalDamageLevel = newLevel; break;
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

    public bool IsMaxLevel(string statType)
    {
        int checkLevel = GetCurrentLevel(statType);
        return checkLevel == 27;
    }
}

[System.Serializable]
public class GodStatServerResult
{
    public int newLevel;
    public float newValue;
    public int newGoldRequired;
    public int remainingGold;
}
