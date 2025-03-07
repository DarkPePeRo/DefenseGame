using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class GodStatManage : MonoBehaviour
{
    public GodStatsData godStatsData;
    public float timer;
    private bool isInitialized = false;

    public int level;
    public float value;
    public int goldRequired;

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
        LoadGodStats(); // JSON 파일에서 데이터 로드
        PlayFabLogin.Instance.LoadStatsFromPlayFab();
        if (godStatsData == null)
        {
            Debug.LogError("GodStats가 로드되지 않았습니다! JSON 파일을 확인하세요.");
            return;
        }
        //InitializeCharacter();

    }
    private void LoadGodStats()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "GodStats.json");
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            Debug.Log($"JSON 데이터 로드: {jsonData}");  //  JSON 데이터 확인

            godStatsData = JsonUtility.FromJson<GodStatsData>(jsonData);

            if (godStatsData == null)
            {
                Debug.LogError("JSON 데이터가 올바르게 파싱되지 않았습니다!");
            }
            else
            {
                Debug.Log("캐릭터 데이터 로드 완료");
            }
        }
        else
        {
            Debug.LogError("GodStats.json 파일을 찾을 수 없습니다.");
        }
    }

    private float GetStatValue(List<GodStatLevel> statLevels, int level)
    {
        GodStatLevel stat = statLevels.Find(s => s.level == level);
        return stat != null ? stat.value : 0;
    }
    public void LevelUp(string statType)
    {
        Debug.LogError(godStatsData);
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
            PlayFabLogin.Instance.SaveStatsToPlayFab();
            Debug.Log($"[{statType}] 업그레이드 성공! 현재 레벨: {nextLevelStats.level}, 값: {nextLevelStats.value}");
        }
        else
        {
            Debug.LogWarning($"[{statType}] 골드 부족! 필요 골드: {nextLevelStats.goldRequired}, 현재 골드: {PlayerCurrency.Instance.gold.amount}");
        }
    }
    private List<GodStatLevel> GetStatLevels(string statType)
    {
        switch (statType)
        {
            case "attackPower": return godStatsData.attackPowerLevels;
            case "attackSpeed": return godStatsData.attackSpeedLevels;
            case "criticalRate": return godStatsData.criticalRateLevels;
            case "criticalDamage": return godStatsData.criticalDamageLevels;
            default: return null;
        }
    }
    private int GetCurrentLevel(string statType)
    {
        switch (statType)
        {
            case "attackPower": return attackPowerLevel;
            case "attackSpeed": return attackSpeedLevel;
            case "criticalRate": return criticalRateLevel;
            case "criticalDamage": return criticalDamageLevel;
            default: return 1;
        }
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
    public bool IsInitialized() => isInitialized;
}
