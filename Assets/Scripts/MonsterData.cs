using System.Collections.Generic;

[System.Serializable]
public class MonsterStat
{
    public string name;
    public int hp;
    public int attack;
    public int defense;
}

[System.Serializable]
public class MonsterData
{
    public List<MonsterStat> monsters;
}
