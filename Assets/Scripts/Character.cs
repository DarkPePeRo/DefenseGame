
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterLevelStats
{
    public int level;
    public float attackPower;
    public float attackSpeed;
    public int goldRequired;
}

[System.Serializable]
public class CharacterLevelData
{
    public List<CharacterLevelStats> levels;
}

public partial class Character : MonoBehaviour
{
    public List<CharacterLevelStats> levelStats;
    public void SetLevelData(CharacterLevelData data)
    {
        levelStats = data.levels;
        ApplyCurrentStats();
    }
    private void Awake()
    {
        name = gameObject.name; // Character.name�� GameObject.name ����ȭ
    }
    public void LevelUp()
    {
        if (levelStats == null || CurrentLevel >= levelStats.Count)
        {
            Debug.LogWarning($"[{name}] �̹� �ִ� �����Դϴ�.");
            return;
        }

        var next = levelStats.Find(l => l.level == CurrentLevel + 1);
        if (next == null)
        {
            Debug.LogError($"[{name}] ���� ���� �����͸� ã�� �� �����ϴ�.");
            return;
        }

        if (PlayerCurrency.Instance.gold.amount < next.goldRequired)
        {
            Debug.LogWarning($"[{name}] ��� ����: �ʿ� {next.goldRequired}, ���� {PlayerCurrency.Instance.gold.amount}");
            return;
        }

        PlayerCurrency.Instance.SpendCurrency(PlayerCurrency.Instance.gold, next.goldRequired);

        CurrentLevel = next.level;
        ApplyCurrentStats();
        //PlayFabCharacterProgressService.UpdateLevel(name, CurrentLevel);

        Debug.Log($"[{name}] ������ �Ϸ� �� Lv.{CurrentLevel}");
    }

    public void ApplyCurrentStats()
    {
        if (levelStats == null) return;

        var current = levelStats.Find(l => l.level == CurrentLevel);
        if (current == null)
        {
            Debug.LogError($"[{name}] ���� ���� {CurrentLevel} �����Ͱ� �������� �ʽ��ϴ�.");
            return;
        }

        CurrentAttackPower = current.attackPower;
        CurrentAttackSpeed = current.attackSpeed;
        GoldRequiredForNext = current.goldRequired;
    }

    public int CurrentLevel { get; set; } = 1;
    public float CurrentAttackPower { get; private set; }
    public float CurrentAttackSpeed { get; private set; }
    public int GoldRequiredForNext { get; private set; }


}