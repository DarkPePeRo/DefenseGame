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

    private Queue<string> levelUpQueue = new Queue<string>();
    private bool isProcessingLevelUp = false;
    private float levelUpAggregationTimer = 0f;
    private const float levelUpAggregationDelay = 1f;
    private Dictionary<string, int> levelUpRequestCount = new Dictionary<string, int>();

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
        // 1. 즉시 선반영
        var character = CharacterManager.Instance.GetCharacterByName(characterId);
        if (character == null) return;

        var stat = CharacterStatLoader.GetNextLevelStat(characterId, character.CurrentLevel + 1);
        if (stat == null || PlayerCurrency.Instance.gold.amount < stat.goldRequired) return;

        character.CurrentLevel++;
        character.CurrentAttackPower = stat.attackPower;
        character.CurrentAttackSpeed = stat.attackSpeed;
        character.GoldRequiredForNext = stat.goldRequired;
        PlayerCurrency.Instance.gold.amount -= stat.goldRequired;

        UpdateUI(character);

        // 2. 서버 검증 요청 예약
        if (!levelUpRequestCount.ContainsKey(characterId))
            levelUpRequestCount[characterId] = 0;
        levelUpRequestCount[characterId]++;
    }

    private void Update()
    {
        if (levelUpRequestCount.Count > 0)
        {
            levelUpAggregationTimer += Time.deltaTime;
            if (levelUpAggregationTimer >= levelUpAggregationDelay)
            {
                foreach (var kvp in levelUpRequestCount)
                    levelUpQueue.Enqueue(kvp.Key + ":" + kvp.Value);

                levelUpRequestCount.Clear();
                TryProcessNext();
                levelUpAggregationTimer = 0f;
            }
        }
    }

    private void TryProcessNext()
    {
        if (isProcessingLevelUp || levelUpQueue.Count == 0) return;

        isProcessingLevelUp = true;
        string[] parts = levelUpQueue.Dequeue().Split(':');
        string characterId = parts[0];
        int count = int.Parse(parts[1]);

        RequestServerValidation(characterId, count);
    }

    private void RequestServerValidation(string characterId, int count)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "MultiLevelUpCharacter",
            FunctionParameter = new { characterId = characterId, count = count }
        };

        PlayFabClientAPI.ExecuteCloudScript(request, result => {
            if (result.FunctionResult == null) { OnLevelUpComplete(); return; }
            var data = JsonUtility.FromJson<LevelUpResult>(result.FunctionResult.ToString());

            var character = CharacterManager.Instance.GetCharacterByName(characterId);
            if (character == null) { OnLevelUpComplete(); return; }

            // 서버 상태와 다르면 덮어쓰기
            if (character.CurrentLevel != data.newLevel)
            {
                character.CurrentLevel = data.newLevel;
                character.CurrentAttackPower = data.attackPower;
                character.CurrentAttackSpeed = data.attackSpeed;
                character.GoldRequiredForNext = data.newGoldRequired;
                PlayerCurrency.Instance.gold.amount = data.newGold;
                UpdateUI(character);
                Debug.LogWarning($"[서버 동기화] {characterId} 상태 수정됨");
            }

            OnLevelUpComplete();
        }, error => {
            Debug.LogError("[서버 검증 실패] " + error.GenerateErrorReport());
            OnLevelUpComplete();
        });
    }

    private void OnLevelUpComplete()
    {
        StartCoroutine(DelayedNext());
    }

    private IEnumerator DelayedNext()
    {
        yield return new WaitForSeconds(0.2f);
        isProcessingLevelUp = false;
        TryProcessNext();
    }


    [System.Serializable]
    public class LevelUpResult
    {
        public int newLevel;
        public float attackPower;
        public float attackSpeed;
        public int newGoldRequired;
        public int newGold;
    }

}