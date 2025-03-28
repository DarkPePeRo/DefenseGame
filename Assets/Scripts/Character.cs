using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Character : MonoBehaviour
{
    private CharacterStats characterStats; // JSON 데이터를 저장할 객체
    [SerializeField]
    public int currentLevel = 1;          // 현재 레벨
    public float currentAttackPower;      // 현재 공격력
    public float currentAttackSpeed;      // 현재 공격 속도
    public int needGold;                  // 다음 레벨업에 필요한 골드
    public float timer; 
    private bool isInitialized = false;
    public CharacterManager characterManager;

    void Awake()
    {
        LoadCharacterStats(); // JSON 파일에서 데이터 로드
        if (characterStats == null || characterStats.levels == null || characterStats.levels.Count == 0)
        {
            Debug.LogError("CharacterStats가 로드되지 않았습니다! JSON 파일을 확인하세요.");
            return;
        }
        InitializeCharacter();
        characterManager = GameObject.Find("CharacterManager").GetComponent<CharacterManager>();
    }


    private void LoadCharacterStats()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "CharacterStats.json");
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            Debug.Log($"JSON 데이터 로드: {jsonData}");  //  JSON 데이터 확인

            characterStats = JsonUtility.FromJson<CharacterStats>(jsonData);

            if (characterStats == null || characterStats.levels == null || characterStats.levels.Count == 0)
            {
                Debug.LogError("JSON 데이터가 올바르게 파싱되지 않았습니다!");
            }
            else
            {
                Debug.Log("캐릭터 데이터 로드 완료");
            }
        }
        else
        {
            Debug.LogError("CharacterStats.json 파일을 찾을 수 없습니다.");
        }
    }


    private void InitializeCharacter()
    {
        SetStatsForLevel(1); // 1레벨로 초기화
        isInitialized = true;
        Debug.Log("Set archer level : 1");
    }

    private void SetStatsForLevel(int level)
    {
        LevelStats stats = GetStatsForLevel(level);
        if (stats != null)
        {
            currentLevel = stats.level;
            currentAttackPower = stats.attackPower;
            currentAttackSpeed = stats.attackSpeed;
            needGold = stats.goldRequired;
        }
        Debug.Log($"SetStatsForLevel - 레벨: {currentLevel}, 공격력: {currentAttackPower}");

        // UI 데이터 강제 동기화 호출 (필요할 경우)
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (characterManager != null)
        {
            characterManager.GetCharacterInfo(gameObject.name); // name으로 캐릭터 정보 갱신
        }
    }


    private LevelStats GetStatsForLevel(int level)
    {
        if (characterStats == null || characterStats.levels == null)
        {
            Debug.LogError("characterStats가 null입니다. JSON 데이터 로드를 확인하세요.");
            return null;
        }

        LevelStats stats = characterStats.levels.Find(stats => stats.level == level);
        if (stats == null)
        {
            Debug.LogError($"레벨 {level}에 대한 데이터를 찾을 수 없습니다.");
        }
        return stats;
    }

    public void LevelUp()
    {
        if (characterStats == null )
        {
            Debug.LogError("레벨업을 시도했으나, characterStats 데이터가 없습니다.");
            return;
        }
        if (characterStats.levels == null)
        {
            Debug.LogError("레벨업을 시도했으나, characterStats.Level 데이터가 없습니다.");
            return;
        }

        int maxLevel = characterStats.levels[characterStats.levels.Count - 1].level;
        if (currentLevel >= maxLevel)
        {
            Debug.LogWarning($"이미 최대 레벨({maxLevel})에 도달했습니다.");
            return;
        }

        LevelStats nextLevelStats = GetStatsForLevel(currentLevel + 1);
        if (nextLevelStats == null)
        {
            Debug.LogError($"레벨 {currentLevel + 1} 데이터가 없습니다.");
            return;
        }

        if (PlayerCurrency.Instance.gold.amount >= nextLevelStats.goldRequired)
        {
            PlayerCurrency.Instance.SpendCurrency(PlayerCurrency.Instance.gold, nextLevelStats.goldRequired);
            SetStatsForLevel(nextLevelStats.level);
            Debug.Log($"레벨업 성공! 현재 레벨: {currentLevel}, 공격력: {currentAttackPower}, 공격 속도: {currentAttackSpeed}");
        }
        else
        {
            Debug.LogWarning($"골드 부족. 레벨업에 필요한 골드: {nextLevelStats.goldRequired}, 현재 골드: {PlayerCurrency.Instance.gold.amount}");
        }
    }


    public int GetCurrentLevel() => currentLevel;
    public float GetCurrentAttackPower() => currentAttackPower;
    public float GetCurrentAttackSpeed() => currentAttackSpeed;
    public int GetNeedGold() => needGold;
    public bool IsInitialized() => isInitialized;
}
