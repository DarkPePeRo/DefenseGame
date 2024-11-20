using UnityEngine;

public class Character : MonoBehaviour
{
    public CharacterLevelStats levelStats; // 레벨별 능력치 데이터
    private int currentLevel = 1;          // 현재 레벨
    private float currentAttackPower;      // 현재 공격력
    private float currentAttackSpeed;      // 현재 공격 속도
    private int currentGold;               // 보유 골드

    void Start()
    {
        InitializeCharacter();
    }

    private void InitializeCharacter()
    {
        SetStatsForLevel(1); // 1레벨로 초기화
    }

    private void SetStatsForLevel(int level)
    {
        LevelStats stats = GetStatsForLevel(level);
        if (stats != null)
        {
            currentLevel = stats.level;
            currentAttackPower = stats.attackPower;
            currentAttackSpeed = stats.attackSpeed;
        }
    }

    private LevelStats GetStatsForLevel(int level)
    {
        foreach (var stats in levelStats.levelStatsTable)
        {
            if (stats.level == level)
                return stats;
        }
        Debug.LogError($"레벨 {level}에 대한 데이터가 없습니다.");
        return null;
    }

    public void LevelUp()
    {
        if (currentLevel >= levelStats.levelStatsTable.Length)
        {
            Debug.LogWarning($"{levelStats.characterName}는 이미 최대 레벨에 도달했습니다.");
            return;
        }

        LevelStats nextLevelStats = GetStatsForLevel(currentLevel + 1);
        if (nextLevelStats != null && PlayerCurrency.Instance.gold.amount >= nextLevelStats.goldRequired)
        {
            PlayerCurrency.Instance.SpendCurrency(PlayerCurrency.Instance.gold, nextLevelStats.goldRequired);
            SetStatsForLevel(nextLevelStats.level);     // 다음 레벨로 설정
            Debug.Log($"{levelStats.characterName} 레벨업! 새로운 레벨: {currentLevel}, 공격력: {currentAttackPower}, 공격 속도: {currentAttackSpeed}");
        }
        else
        {
            Debug.LogWarning($"레벨업에 필요한 골드가 부족합니다. 필요한 골드: {nextLevelStats.goldRequired}, 현재 골드: {PlayerCurrency.Instance.gold.amount}");
        }
    }

    public void AddGold(int amount)
    {
        PlayerCurrency.Instance.gold.amount += amount;
        Debug.Log($"{levelStats.characterName} 골드 획득: {amount}. 현재 골드: {PlayerCurrency.Instance.gold.amount}");
    }

    public int GetCurrentLevel() => currentLevel;
    public float GetCurrentAttackPower() => currentAttackPower;
    public float GetCurrentAttackSpeed() => currentAttackSpeed;
    public int GetCurrentGold() => PlayerCurrency.Instance.gold.amount;
}
