using UnityEngine;

public class Character : MonoBehaviour
{
    public CharacterLevelStats levelStats; // ������ �ɷ�ġ ������
    private int currentLevel = 1;          // ���� ����
    private float currentAttackPower;      // ���� ���ݷ�
    private float currentAttackSpeed;      // ���� ���� �ӵ�
    private int currentGold;               // ���� ���

    void Start()
    {
        InitializeCharacter();
    }

    private void InitializeCharacter()
    {
        SetStatsForLevel(1); // 1������ �ʱ�ȭ
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
        Debug.LogError($"���� {level}�� ���� �����Ͱ� �����ϴ�.");
        return null;
    }

    public void LevelUp()
    {
        if (currentLevel >= levelStats.levelStatsTable.Length)
        {
            Debug.LogWarning($"{levelStats.characterName}�� �̹� �ִ� ������ �����߽��ϴ�.");
            return;
        }

        LevelStats nextLevelStats = GetStatsForLevel(currentLevel + 1);
        if (nextLevelStats != null && PlayerCurrency.Instance.gold.amount >= nextLevelStats.goldRequired)
        {
            PlayerCurrency.Instance.SpendCurrency(PlayerCurrency.Instance.gold, nextLevelStats.goldRequired);
            SetStatsForLevel(nextLevelStats.level);     // ���� ������ ����
            Debug.Log($"{levelStats.characterName} ������! ���ο� ����: {currentLevel}, ���ݷ�: {currentAttackPower}, ���� �ӵ�: {currentAttackSpeed}");
        }
        else
        {
            Debug.LogWarning($"�������� �ʿ��� ��尡 �����մϴ�. �ʿ��� ���: {nextLevelStats.goldRequired}, ���� ���: {PlayerCurrency.Instance.gold.amount}");
        }
    }

    public void AddGold(int amount)
    {
        PlayerCurrency.Instance.gold.amount += amount;
        Debug.Log($"{levelStats.characterName} ��� ȹ��: {amount}. ���� ���: {PlayerCurrency.Instance.gold.amount}");
    }

    public int GetCurrentLevel() => currentLevel;
    public float GetCurrentAttackPower() => currentAttackPower;
    public float GetCurrentAttackSpeed() => currentAttackSpeed;
    public int GetCurrentGold() => PlayerCurrency.Instance.gold.amount;
}
