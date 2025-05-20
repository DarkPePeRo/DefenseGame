using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    [Header("UI 연결")]
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
    private Dictionary<string, CharacterSaveData> saveQueue = new();
    private float saveTimer = 0f;
    private const float saveDelay = 2f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Update()
    {
        if (saveQueue.Count > 0)
        {
            saveTimer += Time.deltaTime;
            if (saveTimer >= saveDelay)
            {
                saveTimer = 0f;
                FlushSaveQueue();
            }
        }
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
                Debug.LogWarning($"[CharacterManager] '{character.name}'에 대한 UI 항목이 없습니다.");
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

    public void RequestLevelUp(string characterId)
    {
        var character = CharacterManager.Instance.GetCharacterByName(characterId);
        if (character == null) return;

        var stat = CharacterStatLoader.GetNextLevelStat(characterId, character.CurrentLevel + 1);
        if (stat == null || PlayerCurrency.Instance.gold.amount < stat.goldRequired) return;

        character.CurrentLevel++;
        character.CurrentAttackPower = stat.attackPower;
        character.CurrentAttackSpeed = stat.attackSpeed;
        character.GoldRequiredForNext = stat.goldRequired;
        PlayerCurrency.Instance.gold.amount -= stat.goldRequired;

        EnqueueCharacterSave(character);
        UpdateUI(character);
        Debug.Log($"[레벨업] {characterId} → Lv.{character.CurrentLevel}");
    }

    private void EnqueueCharacterSave(Character character)
    {
        var data = new CharacterSaveData
        {
            characterId = character.name,
            level = character.CurrentLevel
        };

        saveQueue[character.name] = data;
    }

    private void FlushSaveQueue()
    {
        var dataToSave = new List<CharacterSaveData>(saveQueue.Values);
        saveQueue.Clear();
        PlayFabCharacterProgressService.Save(dataToSave);
    }

    public void StartCharacterLevelUpLoop(string characterId)
    {
        if (levelUpLoopCoroutine != null) return;
        currentLoopCharacterId = characterId;
        levelUpLoopCoroutine = StartCoroutine(LevelUpLoop());
    }

    public void StopCharacterLevelUpLoop()
    {
        if (levelUpLoopCoroutine != null)
        {
            StopCoroutine(levelUpLoopCoroutine);
            levelUpLoopCoroutine = null;
        }
    }

    private Coroutine levelUpLoopCoroutine;
    private string currentLoopCharacterId;

    private IEnumerator LevelUpLoop()
    {
        const float interval = 0.1f;

        while (true)
        {
            RequestLevelUp(currentLoopCharacterId);

            var character = GetCharacterByName(currentLoopCharacterId);
            var stat = CharacterStatLoader.GetNextLevelStat(currentLoopCharacterId, character.CurrentLevel + 1);
            if (character == null || stat == null || PlayerCurrency.Instance.gold.amount < stat.goldRequired)
                break;

            yield return new WaitForSeconds(interval);
        }

        levelUpLoopCoroutine = null;
    }
}
