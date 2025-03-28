using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Character : MonoBehaviour
{
    private CharacterStats characterStats; // JSON �����͸� ������ ��ü
    [SerializeField]
    public int currentLevel = 1;          // ���� ����
    public float currentAttackPower;      // ���� ���ݷ�
    public float currentAttackSpeed;      // ���� ���� �ӵ�
    public int needGold;                  // ���� �������� �ʿ��� ���
    public float timer; 
    private bool isInitialized = false;
    public CharacterManager characterManager;

    void Awake()
    {
        LoadCharacterStats(); // JSON ���Ͽ��� ������ �ε�
        if (characterStats == null || characterStats.levels == null || characterStats.levels.Count == 0)
        {
            Debug.LogError("CharacterStats�� �ε���� �ʾҽ��ϴ�! JSON ������ Ȯ���ϼ���.");
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
            Debug.Log($"JSON ������ �ε�: {jsonData}");  //  JSON ������ Ȯ��

            characterStats = JsonUtility.FromJson<CharacterStats>(jsonData);

            if (characterStats == null || characterStats.levels == null || characterStats.levels.Count == 0)
            {
                Debug.LogError("JSON �����Ͱ� �ùٸ��� �Ľ̵��� �ʾҽ��ϴ�!");
            }
            else
            {
                Debug.Log("ĳ���� ������ �ε� �Ϸ�");
            }
        }
        else
        {
            Debug.LogError("CharacterStats.json ������ ã�� �� �����ϴ�.");
        }
    }


    private void InitializeCharacter()
    {
        SetStatsForLevel(1); // 1������ �ʱ�ȭ
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
        Debug.Log($"SetStatsForLevel - ����: {currentLevel}, ���ݷ�: {currentAttackPower}");

        // UI ������ ���� ����ȭ ȣ�� (�ʿ��� ���)
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (characterManager != null)
        {
            characterManager.GetCharacterInfo(gameObject.name); // name���� ĳ���� ���� ����
        }
    }


    private LevelStats GetStatsForLevel(int level)
    {
        if (characterStats == null || characterStats.levels == null)
        {
            Debug.LogError("characterStats�� null�Դϴ�. JSON ������ �ε带 Ȯ���ϼ���.");
            return null;
        }

        LevelStats stats = characterStats.levels.Find(stats => stats.level == level);
        if (stats == null)
        {
            Debug.LogError($"���� {level}�� ���� �����͸� ã�� �� �����ϴ�.");
        }
        return stats;
    }

    public void LevelUp()
    {
        if (characterStats == null )
        {
            Debug.LogError("�������� �õ�������, characterStats �����Ͱ� �����ϴ�.");
            return;
        }
        if (characterStats.levels == null)
        {
            Debug.LogError("�������� �õ�������, characterStats.Level �����Ͱ� �����ϴ�.");
            return;
        }

        int maxLevel = characterStats.levels[characterStats.levels.Count - 1].level;
        if (currentLevel >= maxLevel)
        {
            Debug.LogWarning($"�̹� �ִ� ����({maxLevel})�� �����߽��ϴ�.");
            return;
        }

        LevelStats nextLevelStats = GetStatsForLevel(currentLevel + 1);
        if (nextLevelStats == null)
        {
            Debug.LogError($"���� {currentLevel + 1} �����Ͱ� �����ϴ�.");
            return;
        }

        if (PlayerCurrency.Instance.gold.amount >= nextLevelStats.goldRequired)
        {
            PlayerCurrency.Instance.SpendCurrency(PlayerCurrency.Instance.gold, nextLevelStats.goldRequired);
            SetStatsForLevel(nextLevelStats.level);
            Debug.Log($"������ ����! ���� ����: {currentLevel}, ���ݷ�: {currentAttackPower}, ���� �ӵ�: {currentAttackSpeed}");
        }
        else
        {
            Debug.LogWarning($"��� ����. �������� �ʿ��� ���: {nextLevelStats.goldRequired}, ���� ���: {PlayerCurrency.Instance.gold.amount}");
        }
    }


    public int GetCurrentLevel() => currentLevel;
    public float GetCurrentAttackPower() => currentAttackPower;
    public float GetCurrentAttackSpeed() => currentAttackSpeed;
    public int GetNeedGold() => needGold;
    public bool IsInitialized() => isInitialized;
}
