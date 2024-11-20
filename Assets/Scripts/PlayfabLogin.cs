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
    private int cachedHighScore = 0; // 로컬 캐시된 점수
    public LoadingManager LoadingManager;
    private float saveInterval = 60f;
    private float timeSinceLastSave = 0;
    private const string ClearedStagesKey = "ClearedStages";
    private List<int> clearedStages = new List<int>();
    public int currentWave;
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

        // 두 데이터를 로드했는지 확인하기 위한 플래그
        bool isCurrencyLoaded = false;
        bool isStagesLoaded = false;
        bool isCharacterLoaded = false;

        // 모든 로드가 완료되면 Scene 로드
        void CheckAllDataLoaded()
        {
            if (isCurrencyLoaded && isStagesLoaded && isCharacterLoaded)
            {
                Debug.Log("모든 데이터가 로드되었습니다. Scene 로딩 중...");
                LoadingManager.LoadScene("SampleScene");
            }
        }

        // 재화 로드
        LoadCurrencyFromServer(() =>
        {
            isCurrencyLoaded = true;
            OfflineRewardSystem.Instance.GiveOfflineReward();
            Debug.Log("재화 로드 완료.");
            CheckAllDataLoaded(); // 완료 체크
        });

        // 스테이지 로드
        LoadClearedStages(() =>
        {
            currentWave = clearedStages[clearedStages.Count - 1];
            isStagesLoaded = true;
            Debug.Log("스테이지 로드 완료.");
            CheckAllDataLoaded(); // 완료 체크
        });
        LoadPlacedCharactersFromPlayFab(() =>
        {
            isCharacterLoaded = true;
            Debug.Log("캐릭터 로드 완료.");
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
                    PlayerCurrency.Instance.gold.amount = 0; // 기본값 설정
                    isDataMissing = true;
                }

                if (result.Data.ContainsKey("diamond"))
                {
                    PlayerCurrency.Instance.diamond.amount = int.Parse(result.Data["diamond"].Value);
                }
                else
                {
                    PlayerCurrency.Instance.diamond.amount = 0; // diamond 기본값 설정
                    isDataMissing = true;
                }

                Debug.Log("Currency loaded from server.");
                }
                else
                {
                    // gold와 diamond 기본값 설정
                    PlayerCurrency.Instance.gold.amount = 0;
                    PlayerCurrency.Instance.diamond.amount = 0;
                    isDataMissing = true;
                }

                // 데이터가 없어서 기본값으로 설정된 경우 서버에 저장
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
                cachedHighScore = 0; // 실패 시 기본값 설정
                onInitialized?.Invoke();
            });
    }
    // 점수를 업데이트합니다.
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

    // 최고 점수를 반환합니다.
    public int GetCachedHighScore()
    {
        return cachedHighScore;
    }

    // 새로운 스테이지를 클리어했을 때 호출
    public void AddClearedStage(int stageNumber)
    {
        if (!clearedStages.Contains(stageNumber))
        {
            clearedStages.Add(stageNumber);
            SaveClearedStages();
        }
    }
    // 클리어한 스테이지 데이터를 서버에 저장
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
    // 서버에서 클리어한 스테이지 데이터를 불러옴
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
                    Debug.Log($"서버에서 클리어한 스테이지 로드 완료: {string.Join(", ", clearedStages)}");
                }
                else
                {
                    Debug.LogWarning("클리어한 스테이지 데이터가 서버에 없습니다.");
                    clearedStages = new List<int>();
                }
                onComplete?.Invoke();
            },
            error =>
            {
                Debug.LogError("클리어한 스테이지 데이터 로드 실패: " + error.GenerateErrorReport());
                clearedStages = new List<int>();
                onComplete?.Invoke();
            });
    }
    public void SavePlacedCharactersToPlayFab()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager 인스턴스가 없습니다.");
            return;
        }

        PlacementSaveData saveData = new PlacementSaveData();
        foreach (var kvp in GameManager.Instance.GetPlacedCharacters())
        {
            string characterName = kvp.Value.name.Replace("(Clone)", "").Trim(); // (Clone) 제거

            saveData.placements.Add(new PlacementData
            {
                characterName = characterName,
                position = kvp.Key
            });
        }

        string json = JsonUtility.ToJson(saveData);
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("직렬화된 JSON 데이터가 비어 있습니다.");
            return;
        }
        Debug.Log($"저장될 JSON 데이터: {json}");

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
        {
            { "PlacedCharacters", json }
        }
        };

        PlayFabClientAPI.UpdateUserData(request,
            result => Debug.Log("배치된 캐릭터 데이터가 PlayFab에 저장되었습니다."),
            error => Debug.LogError("배치된 캐릭터 데이터 저장 실패: " + error.GenerateErrorReport()));
    }

    public void LoadPlacedCharactersFromPlayFab(Action onComplete = null)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            result =>
            {
                if (result.Data == null || !result.Data.ContainsKey("PlacedCharacters"))
                {
                    Debug.LogWarning("PlayFab에서 'PlacedCharacters' 데이터를 찾을 수 없습니다.");
                    onComplete?.Invoke();
                    return;
                }

                string json = result.Data["PlacedCharacters"]?.Value;
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogWarning("'PlacedCharacters' 데이터가 비어 있습니다.");
                    onComplete?.Invoke();
                    return;
                }

                Debug.Log($"로드된 JSON 데이터: {json}");

                try
                {
                    Debug.Log("1");
                    PlacementSaveData saveData = JsonUtility.FromJson<PlacementSaveData>(json);

                    Debug.Log("2");
                    if (saveData == null || saveData.placements == null)
                    {
                        Debug.LogError("JSON 데이터가 null이거나 잘못된 구조입니다.");
                        onComplete?.Invoke();
                        return;
                    }

                    Debug.Log("3");
                    GameManager.Instance.ClearPlacedCharacters();

                    Debug.Log("4");
                    foreach (var placement in saveData.placements)
                    {
                        // characterName에서 (Clone) 제거
                        string characterName = placement.characterName.Replace("(Clone)", "").Trim();
                        GameObject characterPrefab = FindCharacterPrefabByName(characterName);

                        if (characterPrefab != null)
                        {
                            GameObject characterInstance = Instantiate(characterPrefab, placement.position, Quaternion.identity);
                            GameManager.Instance.PlaceCharacter(placement.position, characterInstance);
                        }
                        else
                        {
                            Debug.LogWarning($"프리팹 {characterName}를 찾을 수 없습니다.");
                        }
                    }

                    Debug.Log("PlayFab에서 배치된 캐릭터 데이터가 로드되었습니다.");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"데이터 처리 중 오류 발생: {ex.Message}");
                }

                onComplete?.Invoke();
            },
            error =>
            {
                Debug.LogError("배치된 캐릭터 데이터 로드 실패: " + error.GenerateErrorReport());
                onComplete?.Invoke();
            });
    }



    // 캐릭터 이름으로 프리팹을 찾는 메서드
    private GameObject FindCharacterPrefabByName(string characterName)
    {
        // Resources에서 프리팹 로드
        GameObject prefab = Resources.Load<GameObject>($"Characters/{characterName}");

        if (prefab == null)
        {
            Debug.LogError($"프리팹 {characterName}를 찾을 수 없습니다.");
        }

        return prefab;
    }


}
// JSON 직렬화를 위한 클래스
[Serializable]
public class StageData
{
    public int[] clearedStages; // 클리어한 스테이지 번호
}

[System.Serializable]
public class PlacementData
{
    public string characterName;    // 캐릭터 이름
    public Vector2 position;        // 캐릭터 위치
}

[System.Serializable]
public class PlacementSaveData
{
    public List<PlacementData> placements = new List<PlacementData>();
}
