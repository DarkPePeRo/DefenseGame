using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterLevelStat
{
    public int level;
    public float attackPower;
    public float attackSpeed;
    public int goldRequired;
}

[System.Serializable]
public class CharacterStatData
{
    public List<CharacterLevelStat> levels;
}

public static class CharacterStatLoader
{
    private static Dictionary<string, CharacterStatData> cache = new();

    public static CharacterLevelStat GetNextLevelStat(string characterId, int nextLevel)
    {
        if (!cache.ContainsKey(characterId))
        {
            TextAsset jsonFile = Resources.Load<TextAsset>($"CharacterStats/{characterId}Stats");
            if (jsonFile == null)
            {
                Debug.LogError($"[StatLoader] {characterId}Stats.json ������ ã�� �� ����");
                return null;
            }

            var data = JsonUtility.FromJson<CharacterStatData>(jsonFile.text);
            cache[characterId] = data;
        }

        return cache[characterId].levels.Find(l => l.level == nextLevel);
    }
}
