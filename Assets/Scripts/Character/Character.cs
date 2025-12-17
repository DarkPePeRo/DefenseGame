
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
        name = gameObject.name; // Character.name과 GameObject.name 동기화
    }
    public void ApplyCurrentStats()
    {
        if (levelStats == null) return;

        var current = levelStats.Find(l => l.level == CurrentLevel);
        if (current == null)
        {
            Debug.LogError($"[{name}] 현재 레벨 {CurrentLevel} 데이터가 존재하지 않습니다.");
            return;
        }

        CurrentAttackPower = current.attackPower;
        CurrentAttackSpeed = current.attackSpeed;
        GoldRequiredForNext = current.goldRequired;
    }

    public int CurrentLevel { get; set; } = 1;
    public float CurrentAttackPower { get; set; }
    public float CurrentAttackSpeed { get; set; }
    public int GoldRequiredForNext { get; set; }


}