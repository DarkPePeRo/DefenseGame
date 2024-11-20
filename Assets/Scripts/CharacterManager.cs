using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterManager : MonoBehaviour
{
    public List<Character> characters; // 관리할 캐릭터 리스트

    public void LevelUpCharacter(string characterName)
    {
        var character = characters.Find(c => c.levelStats.characterName == characterName);
        if (character != null)
        {
            character.LevelUp();
        }
        else
        {
            Debug.LogError($"캐릭터 {characterName}를 찾을 수 없습니다.");
        }
    }

    public void ShowAllCharacterStats()
    {
        foreach (var character in characters)
        {
            Debug.Log($"캐릭터: {character.levelStats.characterName}, 레벨: {character.GetCurrentLevel()}, 공격력: {character.GetCurrentAttackPower()}, 골드: {character.GetCurrentGold()}");
        }
    }
}
