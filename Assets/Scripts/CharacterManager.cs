using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    [Header("UI ����")]
    public TextMeshProUGUI archerNameText;
    public TextMeshProUGUI archerLevel;
    public TextMeshProUGUI archerDamage;
    public TextMeshProUGUI archerGold;

    public TextMeshProUGUI astroNameText;
    public TextMeshProUGUI astroLevel;
    public TextMeshProUGUI astroDamage;
    public TextMeshProUGUI astroGold;

    public List<Character> characters = new();

    private Coroutine saveCoroutine;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void RegisterCharacter(Character character)
    {
        if (!characters.Contains(character))
            characters.Add(character);
    }

    public void RemoveCharacter(Character character)
    {
        if (characters.Contains(character))
            characters.Remove(character);
    }

    public void UpdateUI(Character character)
    {
        if (character == null) return;

        switch (character.name.ToLower())
        {
            case "archer":
                UpdateCharacterUI(archerNameText, archerLevel, archerDamage, archerGold, character);
                break;
            case "astro":
                UpdateCharacterUI(astroNameText, astroLevel, astroDamage, astroGold, character);
                break;
            default:
                Debug.LogWarning($"[CharacterManager] '{character.name}'�� ���� UI �׸��� �����ϴ�.");
                break;
        }
    }

    private void UpdateCharacterUI(TextMeshProUGUI name, TextMeshProUGUI level, TextMeshProUGUI damage, TextMeshProUGUI gold, Character c)
    {
        name.text = c.name;
        level.text = c.CurrentLevel.ToString();
        damage.text = c.CurrentAttackPower.ToString("F1");
        gold.text = c.GoldRequiredForNext.ToString();
    }

    public void RefreshAllCharacterUI()
    {
        foreach (var c in characters)
            UpdateUI(c);
    }
    public Character GetCharacterByName(string characterName)
    {
        characterName = characterName.Trim().ToLower();
        return characters.Find(c => c.name.Trim().ToLower() == characterName);
    }


    public List<Character> GetAllCharacters()
    {
        return new List<Character>(characters);
    }

    public void ClearAllCharacters()
    {
        characters.Clear();
    }

    public void LevelUpCharacter(string characterName)
    {
        Character character = characters.Find(c => c.name == characterName);
        if (character != null)
        {
            character.LevelUp();
            UpdateUI(character);
            RequestSave(character.CurrentLevel);
        }
        else
        {
            Debug.LogError($"[CharacterManager] ������ ����: {characterName} ĳ���͸� ã�� �� �����ϴ�.");
        }
    }
    public void RequestSave(int currentLevel)
    {
        if (saveCoroutine != null)
        {
            StopCoroutine(saveCoroutine);
        }

        saveCoroutine = StartCoroutine(DelayedSave(currentLevel));
    }

    private IEnumerator DelayedSave(int currentLevel)
    {
        yield return new WaitForSeconds(2f); // ���� ������ (2�� �� �ߺ� ����)
        PlayFabCharacterProgressService.UpdateLevel(name, currentLevel);
        Debug.Log("[DebouncedSaver] ���� �����");
        saveCoroutine = null;
    }
}