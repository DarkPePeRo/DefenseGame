using UnityEngine;

[System.Serializable]
public class LevelStats
{
    public int level;              // ����
    public float attackPower;      // ���ݷ�
    public float attackSpeed;      // ���� �ӵ�
    public int goldRequired;       // �������� �ʿ��� ���
}

[CreateAssetMenu(fileName = "CharacterLevelStats", menuName = "Game/CharacterLevelStats", order = 1)]
public class CharacterLevelStats : ScriptableObject
{
    public string characterName;          // ĳ���� �̸�
    public LevelStats[] levelStatsTable;  // ������ �ɷ�ġ ���̺�
}
