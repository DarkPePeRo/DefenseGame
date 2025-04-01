using System.Collections.Generic;

[System.Serializable]
public class CharacterSaveData
{
    public string characterId;
    public int level;
}

[System.Serializable]
public class CharacterSaveWrapper
{
    public List<CharacterSaveData> characters = new List<CharacterSaveData>();
}