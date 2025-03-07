using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    public List<Character> characters; // 관리할 캐릭터 리스트
    public TextMeshProUGUI archerNameText;
    public TextMeshProUGUI archerLevel;
    public TextMeshProUGUI archerDamage;
    public TextMeshProUGUI archerGold;
    public TextMeshProUGUI astroNameText;
    public TextMeshProUGUI astroLevel;
    public TextMeshProUGUI astroDamage;
    public TextMeshProUGUI astroGold;
    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 오브젝트 유지
        }
        else
        {
            Destroy(gameObject); // 중복된 오브젝트는 제거
        }
    }

    public void LevelUpCharacter(string characterName)
    {
        var character = characters.Find(c => c.name == characterName);
        if (character != null)
        {
            character.LevelUp();
            // 강제로 데이터 동기화
            Debug.Log($"레벨업 이후 데이터 - 레벨: {character.GetCurrentLevel()}, 공격력: {character.GetCurrentAttackPower()}");
            GetCharacterInfo(characterName);
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
            Debug.Log($"캐릭터: {character.name}, 레벨: {character.GetCurrentLevel()}, 공격력: {character.GetCurrentAttackPower()}, 골드: {character.GetNeedGold()}");
        }
    }

    public void GetCharacterInfo(string characterName)
    {
        var character = characters.Find(c => c.name == characterName);
        if (character != null && character.name == "archer")
        {
            Debug.Log($"GetCharacterInfo - 캐릭터: {character.name}, 레벨: {character.GetCurrentLevel()}, 공격력: {character.GetCurrentAttackPower()}");
            archerNameText.text = character.name;
            archerLevel.text = character.GetCurrentLevel().ToString();
            archerDamage.text = character.GetCurrentAttackPower().ToString("F1");
            archerGold.text = character.GetNeedGold().ToString();
        }
        else if (character != null && character.name == "astro")
        {
            Debug.Log($"GetCharacterInfo - 캐릭터: {character.name}, 레벨: {character.GetCurrentLevel()}, 공격력: {character.GetCurrentAttackPower()}");
            astroNameText.text = character.name;
            astroLevel.text = character.GetCurrentLevel().ToString();
            astroDamage.text = character.GetCurrentAttackPower().ToString("F1");
            astroGold.text = character.GetNeedGold().ToString();
        }
    }

}
