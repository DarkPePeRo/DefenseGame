using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStats", menuName = "Game/CharacterStats", order = 1)]
public class CharacterStats : ScriptableObject
{
    public string characterName;       // ĳ���� �̸�
    public int maxLevel = 10;          // �ִ� ����
    public float baseAttackPower;      // �⺻ ���ݷ�
    public float baseAttackSpeed;      // �⺻ ���� �ӵ�
    public float baseHealth;           // �⺻ ü��

    [Header("������ ������")]
    public float attackPowerGrowth;    // ������ �� ���ݷ� ������
    public float attackSpeedGrowth;    // ������ �� ���� �ӵ� ������
    public float healthGrowth;         // ������ �� ü�� ������
}
