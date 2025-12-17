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
    private class PendingLevelUp
    {
        public string characterId;
        public int levelBefore;
        public int levelAfter;
        public int totalGoldUsed;
    }

    private Dictionary<string, PendingLevelUp> pendingLevelUps = new();
    private float validationDelay = 2f;
    private float validationTimer = 0f;
    private bool validationScheduled = false;

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

        // 서버 검증 타이머
        if (validationScheduled)
        {
            validationTimer += Time.deltaTime;
            if (validationTimer >= validationDelay)
            {
                validationScheduled = false;
                SendValidationRequest();
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
        var character = GetCharacterByName(characterId);
        if (character == null) return;

        var stat = CharacterStatLoader.GetNextLevelStat(characterId, character.CurrentLevel + 1);
        if (stat == null || PlayerCurrency.Instance.gold.amount < stat.goldRequired) return;

        // 즉시 반영
        PlayerCurrency.Instance.gold.amount -= stat.goldRequired;
        character.CurrentLevel++;
        character.CurrentAttackPower = stat.attackPower;
        character.CurrentAttackSpeed = stat.attackSpeed;
        character.GoldRequiredForNext = stat.goldRequired;
        UpdateUI(character);

        // 누적 등록
        if (!pendingLevelUps.ContainsKey(characterId))
        {
            pendingLevelUps[characterId] = new PendingLevelUp
            {
                characterId = characterId,
                levelBefore = character.CurrentLevel - 1,
                levelAfter = character.CurrentLevel,
                totalGoldUsed = stat.goldRequired
            };
        }
        else
        {
            var pending = pendingLevelUps[characterId];
            pending.levelAfter = character.CurrentLevel;
            pending.totalGoldUsed += stat.goldRequired;
        }

        validationTimer = 0f;
        validationScheduled = true;
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
    private void SendValidationRequest()
    {
        foreach (var entry in pendingLevelUps.Values)
        {
            var request = new ExecuteCloudScriptRequest
            {
                FunctionName = "MultiLevelUpCharacter",
                FunctionParameter = new
                {
                    characterId = entry.characterId,
                    count = entry.levelAfter - entry.levelBefore
                },
                GeneratePlayStreamEvent = false
            };

            PlayFabClientAPI.ExecuteCloudScript(request,
                result =>
                {
                    Debug.Log($"[서버 레벨업 성공] {entry.characterId}: {entry.levelBefore}→{entry.levelAfter}");
                    EnqueueCharacterSave(GetCharacterByName(entry.characterId));
                },
                error =>
                {
                    Debug.LogError($"[서버 레벨업 실패] 롤백 시작: {entry.characterId}");
                    var c = GetCharacterByName(entry.characterId);
                    c.CurrentLevel = entry.levelBefore;
                    c.ApplyCurrentStats();
                    PlayerCurrency.Instance.gold.amount += entry.totalGoldUsed;
                    UpdateUI(c);
                });
        }

        pendingLevelUps.Clear();
    }

}
