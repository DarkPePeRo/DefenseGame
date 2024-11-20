using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStats", menuName = "Game/CharacterStats", order = 1)]
public class CharacterStats : ScriptableObject
{
    public string characterName;       // 캐릭터 이름
    public int maxLevel = 10;          // 최대 레벨
    public float baseAttackPower;      // 기본 공격력
    public float baseAttackSpeed;      // 기본 공격 속도
    public float baseHealth;           // 기본 체력

    [Header("레벨업 증가량")]
    public float attackPowerGrowth;    // 레벨업 시 공격력 증가량
    public float attackSpeedGrowth;    // 레벨업 시 공격 속도 증가량
    public float healthGrowth;         // 레벨업 시 체력 증가량
}
