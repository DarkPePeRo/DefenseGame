using UnityEngine;
using System.Collections.Generic;

public class MonsterStatsLoader : MonoBehaviour
{
    public MonsterData monsterData;

    private void Awake()
    {
        LoadMonsterStats(); // ���� ���� �� �� ���� JSON ���� �ε�
    }

    private void LoadMonsterStats()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/Stats");
        if (jsonFile != null)
        {
            monsterData = JsonUtility.FromJson<MonsterData>(jsonFile.text);
            Debug.Log("Monster stats loaded and cached successfully!");
        }
        else
        {
            Debug.LogError("MonsterStats.json file not found in Resources folder.");
        }
    }

    public MonsterStat GetMonsterStatByName(string name)
    {
        return monsterData.monsters.Find(monster => monster.name == name);
    }
}
