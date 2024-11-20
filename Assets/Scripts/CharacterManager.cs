using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterManager : MonoBehaviour
{
    public List<Character> characters; // ������ ĳ���� ����Ʈ

    public void LevelUpCharacter(string characterName)
    {
        var character = characters.Find(c => c.levelStats.characterName == characterName);
        if (character != null)
        {
            character.LevelUp();
        }
        else
        {
            Debug.LogError($"ĳ���� {characterName}�� ã�� �� �����ϴ�.");
        }
    }

    public void ShowAllCharacterStats()
    {
        foreach (var character in characters)
        {
            Debug.Log($"ĳ����: {character.levelStats.characterName}, ����: {character.GetCurrentLevel()}, ���ݷ�: {character.GetCurrentAttackPower()}, ���: {character.GetCurrentGold()}");
        }
    }
}
