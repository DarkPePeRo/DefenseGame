using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor.U2D.Aseprite;

public class PlayFabLogin : MonoBehaviour
{
    public static PlayFabLogin Instance;

    private const string ScoreStatisticName = "PlayerScore";
    private int cachedHighScore = 0; // ���� ĳ�õ� ����
    public LoadingManager LoadingManager;
    private float saveInterval = 60f;
    private float timeSinceLastSave = 0;
    private const string ClearedStagesKey = "ClearedStages";
    private List<int> clearedStages = new List<int>();
    public int currentWave;
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

    private void Start()
    {
        var request = new LoginWithCustomIDRequest { CustomId = SystemInfo.deviceUniqueIdentifier, CreateAccount = true };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }
    private void Update()
    {
        timeSinceLastSave += Time.deltaTime;
        if (timeSinceLastSave > saveInterval) {
            SavePlayerData(PlayerCurrency.Instance.gold.amount, PlayerCurrency.Instance.diamond.amount);
           timeSinceLastSave = 0;
        }
    }
    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login successful!");

        // �� �����͸� �ε��ߴ��� Ȯ���ϱ� ���� �÷���
        bool isCurrencyLoaded = false;
        bool isStagesLoaded = false;
        bool isCharacterLoaded = false;

        // ��� �ε尡 �Ϸ�Ǹ� Scene �ε�
        void CheckAllDataLoaded()
        {
            if (isCurrencyLoaded && isStagesLoaded && isCharacterLoaded)
            {
                Debug.Log("��� �����Ͱ� �ε�Ǿ����ϴ�. Scene �ε� ��...");
                LoadingManager.LoadScene("SampleScene");
            }
        }

        // ��ȭ �ε�
        LoadCurrencyFromServer(() =>
        {
            isCurrencyLoaded = true;
            OfflineRewardSystem.Instance.GiveOfflineReward();
            Debug.Log("��ȭ �ε� �Ϸ�.");
            CheckAllDataLoaded(); // �Ϸ� üũ
        });

        // �������� �ε�
        LoadClearedStages(() =>
        {
            currentWave = clearedStages[clearedStages.Count - 1];
            isStagesLoaded = true;
            Debug.Log("�������� �ε� �Ϸ�.");
            CheckAllDataLoaded(); // �Ϸ� üũ
        });
        LoadPlacedCharactersFromPlayFab(() =>
        {
            isCharacterLoaded = true;
            Debug.Log("ĳ���� �ε� �Ϸ�.");
            CheckAllDataLoaded();
        });
    }


    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("Login failed: " + error.GenerateErrorReport());
    }
    public void SavePlayerData(int goldAmount, int diamondAmount)
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
        {
            { "gold", goldAmount.ToString() },
            { "diamond", diamondAmount.ToString() }
        }
        };
        PlayFabClientAPI.UpdateUserData(request, OnDataSendSuccess, OnDataSendFailure);
    }
    public void LoadCurrencyFromServer(System.Action onComplete)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            result =>
            {
            bool isDataMissing = false;

            if (result.Data != null)
            {
                if (result.Data.ContainsKey("gold"))
                {
                    PlayerCurrency.Instance.gold.amount = int.Parse(result.Data["gold"].Value);
                }
                else
                {
                    PlayerCurrency.Instance.gold.amount = 0; // �⺻�� ����
                    isDataMissing = true;
                }

                if (result.Data.ContainsKey("diamond"))
                {
                    PlayerCurrency.Instance.diamond.amount = int.Parse(result.Data["diamond"].Value);
                }
                else
                {
                    PlayerCurrency.Instance.diamond.amount = 0; // diamond �⺻�� ����
                    isDataMissing = true;
                }

                Debug.Log("Currency loaded from server.");
                }
                else
                {
                    // gold�� diamond �⺻�� ����
                    PlayerCurrency.Instance.gold.amount = 0;
                    PlayerCurrency.Instance.diamond.amount = 0;
                    isDataMissing = true;
                }

                // �����Ͱ� ��� �⺻������ ������ ��� ������ ����
                if (isDataMissing)
                {
                    SaveDefaultCurrencyToServer();
                }
                onComplete?.Invoke();
            },
            error => Debug.LogError("Error loading currency: " + error.GenerateErrorReport()));
    }

    private void SaveDefaultCurrencyToServer()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "gold", PlayerCurrency.Instance.gold.amount.ToString() },
                { "diamond", PlayerCurrency.Instance.diamond.amount.ToString() }
            }
        };
        PlayFabClientAPI.UpdateUserData(request, OnDataSendSuccess, OnDataSendFailure);
    }
    private void OnDataSendSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Data successfully saved!");
    }

    private void OnDataSendFailure(PlayFabError error)
    {
        Debug.LogError("Failed to save data: " + error.GenerateErrorReport());
    }
    public void Initialize(Action onInitialized = null)
    {
        PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest(),
            result =>
            {
                foreach (var stat in result.Statistics)
                {
                    if (stat.StatisticName == ScoreStatisticName)
                    {
                        cachedHighScore = stat.Value;
                        break;
                    }
                }

                Debug.Log($"[PlayFab] Cached High Score: {cachedHighScore}");
                onInitialized?.Invoke();
            },
            error =>
            {
                Debug.LogError($"[PlayFab] Failed to retrieve statistics: {error.GenerateErrorReport()}");
                cachedHighScore = 0; // ���� �� �⺻�� ����
                onInitialized?.Invoke();
            });
    }
    // ������ ������Ʈ�մϴ�.
    public void UpdateScore(int newScore)
    {
        if (newScore > cachedHighScore)
        {
            Debug.Log($"[PlayFab] Updating high score to: {newScore}");
            cachedHighScore = newScore;

            var request = new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>
                {
                    new StatisticUpdate { StatisticName = ScoreStatisticName, Value = newScore }
                }
            };

            PlayFabClientAPI.UpdatePlayerStatistics(request,
                result => Debug.Log("[PlayFab] High score updated successfully."),
                error => Debug.LogError($"[PlayFab] Failed to update statistics: {error.GenerateErrorReport()}"));
        }
        else
        {
            Debug.Log($"[PlayFab] Current score {newScore} is not higher than cached high score {cachedHighScore}. No update needed.");
        }
    }

    // �ְ� ������ ��ȯ�մϴ�.
    public int GetCachedHighScore()
    {
        return cachedHighScore;
    }

    // ���ο� ���������� Ŭ�������� �� ȣ��
    public void AddClearedStage(int stageNumber)
    {
        if (!clearedStages.Contains(stageNumber))
        {
            clearedStages.Add(stageNumber);
            SaveClearedStages();
        }
    }
    // Ŭ������ �������� �����͸� ������ ����
    public void SaveClearedStages()
    {
        string json = JsonUtility.ToJson(new StageData { clearedStages = clearedStages.ToArray() });

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { ClearedStagesKey, json }
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnDataSendSuccess, OnDataSendFailure);
    }
    // �������� Ŭ������ �������� �����͸� �ҷ���
    public void LoadClearedStages(Action onComplete = null)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            result =>
            {
                if (result.Data != null && result.Data.ContainsKey(ClearedStagesKey))
                {
                    string json = result.Data[ClearedStagesKey].Value;
                    StageData data = JsonUtility.FromJson<StageData>(json);
                    clearedStages = new List<int>(data.clearedStages);
                    Debug.Log($"�������� Ŭ������ �������� �ε� �Ϸ�: {string.Join(", ", clearedStages)}");
                }
                else
                {
                    Debug.LogWarning("Ŭ������ �������� �����Ͱ� ������ �����ϴ�.");
                    clearedStages = new List<int>();
                }
                onComplete?.Invoke();
            },
            error =>
            {
                Debug.LogError("Ŭ������ �������� ������ �ε� ����: " + error.GenerateErrorReport());
                clearedStages = new List<int>();
                onComplete?.Invoke();
            });
    }
    public void SavePlacedCharactersToPlayFab()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager �ν��Ͻ��� �����ϴ�.");
            return;
        }

        PlacementSaveData saveData = new PlacementSaveData();
        foreach (var kvp in GameManager.Instance.GetPlacedCharacters())
        {
            string characterName = kvp.Value.name.Replace("(Clone)", "").Trim(); // (Clone) ����

            saveData.placements.Add(new PlacementData
            {
                characterName = characterName,
                position = kvp.Key
            });
        }

        string json = JsonUtility.ToJson(saveData);
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("����ȭ�� JSON �����Ͱ� ��� �ֽ��ϴ�.");
            return;
        }
        Debug.Log($"����� JSON ������: {json}");

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
        {
            { "PlacedCharacters", json }
        }
        };

        PlayFabClientAPI.UpdateUserData(request,
            result => Debug.Log("��ġ�� ĳ���� �����Ͱ� PlayFab�� ����Ǿ����ϴ�."),
            error => Debug.LogError("��ġ�� ĳ���� ������ ���� ����: " + error.GenerateErrorReport()));
    }

    public void LoadPlacedCharactersFromPlayFab(Action onComplete = null)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            result =>
            {
                if (result.Data == null || !result.Data.ContainsKey("PlacedCharacters"))
                {
                    Debug.LogWarning("PlayFab���� 'PlacedCharacters' �����͸� ã�� �� �����ϴ�.");
                    onComplete?.Invoke();
                    return;
                }

                string json = result.Data["PlacedCharacters"]?.Value;
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogWarning("'PlacedCharacters' �����Ͱ� ��� �ֽ��ϴ�.");
                    onComplete?.Invoke();
                    return;
                }

                Debug.Log($"�ε�� JSON ������: {json}");

                try
                {
                    Debug.Log("1");
                    PlacementSaveData saveData = JsonUtility.FromJson<PlacementSaveData>(json);

                    Debug.Log("2");
                    if (saveData == null || saveData.placements == null)
                    {
                        Debug.LogError("JSON �����Ͱ� null�̰ų� �߸��� �����Դϴ�.");
                        onComplete?.Invoke();
                        return;
                    }

                    Debug.Log("3");
                    GameManager.Instance.ClearPlacedCharacters();

                    Debug.Log("4");
                    foreach (var placement in saveData.placements)
                    {
                        // characterName���� (Clone) ����
                        string characterName = placement.characterName.Replace("(Clone)", "").Trim();
                        GameObject characterPrefab = FindCharacterPrefabByName(characterName);

                        if (characterPrefab != null)
                        {
                            GameObject characterInstance = Instantiate(characterPrefab, placement.position, Quaternion.identity);
                            GameManager.Instance.PlaceCharacter(placement.position, characterInstance);
                        }
                        else
                        {
                            Debug.LogWarning($"������ {characterName}�� ã�� �� �����ϴ�.");
                        }
                    }

                    Debug.Log("PlayFab���� ��ġ�� ĳ���� �����Ͱ� �ε�Ǿ����ϴ�.");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"������ ó�� �� ���� �߻�: {ex.Message}");
                }

                onComplete?.Invoke();
            },
            error =>
            {
                Debug.LogError("��ġ�� ĳ���� ������ �ε� ����: " + error.GenerateErrorReport());
                onComplete?.Invoke();
            });
    }



    // ĳ���� �̸����� �������� ã�� �޼���
    private GameObject FindCharacterPrefabByName(string characterName)
    {
        // Resources���� ������ �ε�
        GameObject prefab = Resources.Load<GameObject>($"Characters/{characterName}");

        if (prefab == null)
        {
            Debug.LogError($"������ {characterName}�� ã�� �� �����ϴ�.");
        }

        return prefab;
    }


}
// JSON ����ȭ�� ���� Ŭ����
[Serializable]
public class StageData
{
    public int[] clearedStages; // Ŭ������ �������� ��ȣ
}

[System.Serializable]
public class PlacementData
{
    public string characterName;    // ĳ���� �̸�
    public Vector2 position;        // ĳ���� ��ġ
}

[System.Serializable]
public class PlacementSaveData
{
    public List<PlacementData> placements = new List<PlacementData>();
}
