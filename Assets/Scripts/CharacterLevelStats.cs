using UnityEngine;

[System.Serializable]
public class LevelStats
{
    public int level;              // 레벨
    public float attackPower;      // 공격력
    public float attackSpeed;      // 공격 속도
    public int goldRequired;       // 레벨업에 필요한 골드
}

[CreateAssetMenu(fileName = "CharacterLevelStats", menuName = "Game/CharacterLevelStats", order = 1)]
public class CharacterLevelStats : ScriptableObject
{
    public string characterName;          // 캐릭터 이름
    public LevelStats[] levelStatsTable;  // 레벨별 능력치 테이블
}
