using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    public List<Character> characters; // ������ ĳ���� ����Ʈ
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
        // �̱��� ���� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ������Ʈ ����
        }
        else
        {
            Destroy(gameObject); // �ߺ��� ������Ʈ�� ����
        }
    }

    public void LevelUpCharacter(string characterName)
    {
        var character = characters.Find(c => c.name == characterName);
        if (character != null)
        {
            character.LevelUp();
            // ������ ������ ����ȭ
            Debug.Log($"������ ���� ������ - ����: {character.GetCurrentLevel()}, ���ݷ�: {character.GetCurrentAttackPower()}");
            GetCharacterInfo(characterName);
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
            Debug.Log($"ĳ����: {character.name}, ����: {character.GetCurrentLevel()}, ���ݷ�: {character.GetCurrentAttackPower()}, ���: {character.GetNeedGold()}");
        }
    }

    public void GetCharacterInfo(string characterName)
    {
        var character = characters.Find(c => c.name == characterName);
        if (character != null && character.name == "archer")
        {
            Debug.Log($"GetCharacterInfo - ĳ����: {character.name}, ����: {character.GetCurrentLevel()}, ���ݷ�: {character.GetCurrentAttackPower()}");
            archerNameText.text = character.name;
            archerLevel.text = character.GetCurrentLevel().ToString();
            archerDamage.text = character.GetCurrentAttackPower().ToString("F1");
            archerGold.text = character.GetNeedGold().ToString();
        }
        else if (character != null && character.name == "astro")
        {
            Debug.Log($"GetCharacterInfo - ĳ����: {character.name}, ����: {character.GetCurrentLevel()}, ���ݷ�: {character.GetCurrentAttackPower()}");
            astroNameText.text = character.name;
            astroLevel.text = character.GetCurrentLevel().ToString();
            astroDamage.text = character.GetCurrentAttackPower().ToString("F1");
            astroGold.text = character.GetNeedGold().ToString();
        }
    }

}
