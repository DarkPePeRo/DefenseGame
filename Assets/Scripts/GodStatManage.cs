using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class GodStatManage : MonoBehaviour
{
    public GodStatsData godStatsData;
    private bool isInitialized = false;

    private Coroutine saveCoroutine;

    public float timer;

    public int attackPowerLevel = 1;
    public int attackSpeedLevel = 1;
    public int criticalRateLevel = 1;
    public int criticalDamageLevel = 1;

    public float attackPower = 10;
    public float attackSpeed = 10;
    public float criticalRate = 10;
    public float criticalDamage = 10;

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

            if (godStatsData == null)
                Debug.LogError("JSON 데이터가 올바르게 파싱되지 않았습니다!");
            else
                Debug.Log("GodStats 데이터 로드 완료");
        }
        else
        {
            Debug.LogError("GodStats.json 파일을 찾을 수 없습니다.");
        }
    }

    public void LevelUp(string statType)
    {
        int currentLevel = GetCurrentLevel(statType);
        List<GodStatLevel> statLevels = GetStatLevels(statType);
        if (statLevels == null || currentLevel >= statLevels.Count)
        {
            Debug.LogWarning($"[{statType}] 이미 최대 레벨입니다.");
            return;
        }

        GodStatLevel nextLevelStats = statLevels.Find(s => s.level == currentLevel + 1);
        if (nextLevelStats == null)
        {
            Debug.LogError($"[{statType}] 레벨 {currentLevel + 1} 데이터가 없습니다.");
            return;
        }

        if (PlayerCurrency.Instance.gold.amount >= nextLevelStats.goldRequired)
        {
            PlayerCurrency.Instance.SpendCurrency(PlayerCurrency.Instance.gold, nextLevelStats.goldRequired);
            SetCurrentLevel(statType, nextLevelStats.level);
            SetCurrentValue(statType, nextLevelStats.value);
            SetCurrentGold(statType, nextLevelStats.goldRequired); 
            PlayFabStatsService.SetStat(statType, nextLevelStats.level);
            //PlayFabStatsService.Save();
            RequestSave();

            Debug.Log($"[{statType}] 업그레이드 성공! 현재 레벨: {nextLevelStats.level}, 값: {nextLevelStats.value}");
        }
        else
        {
            Debug.LogWarning($"[{statType}] 골드 부족! 필요 골드: {nextLevelStats.goldRequired}, 현재 골드: {PlayerCurrency.Instance.gold.amount}");
        }
    }

    private List<GodStatLevel> GetStatLevels(string statType)
    {
        return statType switch
        {
            "attackPower" => godStatsData.attackPowerLevels,
            "attackSpeed" => godStatsData.attackSpeedLevels,
            "criticalRate" => godStatsData.criticalRateLevels,
            "criticalDamage" => godStatsData.criticalDamageLevels,
            _ => null,
        };
    }

    private int GetCurrentLevel(string statType)
    {
        return statType switch
        {
            "attackPower" => attackPowerLevel,
            "attackSpeed" => attackSpeedLevel,
            "criticalRate" => criticalRateLevel,
            "criticalDamage" => criticalDamageLevel,
            _ => 1,
        };
    }

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
        List<GodStatLevel> statLevels = GetStatLevels(statType);
        GodStatLevel currentLevelStats = statLevels.Find(s => s.level == currentLevel);

        if (currentLevelStats == null)
        {
            Debug.LogError($"[{statType}] 현재 레벨 {currentLevel}에 해당하는 데이터가 없습니다.");
            return;
        }

        SetCurrentValue(statType, currentLevelStats.value);
        SetCurrentGold(statType, currentLevelStats.goldRequired);
    }

    public bool IsInitialized() => isInitialized;

    public void RequestSave()
    {
        if (saveCoroutine != null)
        {
            StopCoroutine(saveCoroutine);
        }

        saveCoroutine = StartCoroutine(DelayedSave());
    }

    private IEnumerator DelayedSave()
    {
        yield return new WaitForSeconds(1f); // 저장 딜레이 (1초 내 중복 막음)
        PlayFabStatsService.Save();
        Debug.Log("[DebouncedSaver] 저장 실행됨");
        saveCoroutine = null;
    }
}
