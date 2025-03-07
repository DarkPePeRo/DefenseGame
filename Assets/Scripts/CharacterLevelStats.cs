using System.Collections.Generic;

[System.Serializable]
public class LevelStats
{
    public int level;
    public float attackPower;
    public float attackSpeed;
    public int goldRequired;
}

[System.Serializable]
public class CharacterStats
{
    public List<LevelStats> levels;
}
