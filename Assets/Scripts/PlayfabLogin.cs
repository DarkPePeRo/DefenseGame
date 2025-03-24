using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using NativeWebSocket;
using PimDeWitte.UnityMainThreadDispatcher;

public class PlayFabLogin : MonoBehaviour
{
    public static PlayFabLogin Instance;

    private WebSocket websocket;
    private string playFabId;
    private string displayName;

    private const string ScoreStatisticName = "PlayerScore";
    private int cachedHighScore = 0; // 로컬 캐시된 점수
    public LoadingManager LoadingManager;
    private float saveInterval = 60f;
    private float timeSinceLastSave = 0;
    private const string ClearedStagesKey = "ClearedStages";
    private List<int> clearedStages = new List<int>();
    public int currentWave;

    public int attackPowerLevel = 1;
    public int attackSpeedLevel = 1;
    public int criticalRateLevel = 1;
    public int criticalDamageLevel = 1;

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
        PlayFabSettings.TitleId = "1FD00"; // PlayFab Title ID 설정
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
        playFabId = result.PlayFabId;
        Debug.Log("로그인 성공! PlayFab ID: " + playFabId);

        // WebSocket 연결
        ConnectToWebSocket();

        // 두 데이터를 로드했는지 확인하기 위한 플래그
        bool isCurrencyLoaded = false;
        bool isStagesLoaded = false;
        bool isCharacterLoaded = false;

        // 모든 로드가 완료되면 Scene 로드
        void CheckAllDataLoaded()
        {
            if (isCurrencyLoaded && isStagesLoaded)
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
        // 스텟 로드
        LoadStatsFromPlayFab();
        FetchDisplayName();
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


    async void OnApplicationQuit()
    {
        await websocket.Close();
    }
    //  PlayFab에서 DisplayName을 가져오는 함수 (비동기 호출)
    void FetchDisplayName()
    {
        var request = new GetAccountInfoRequest();
        PlayFabClientAPI.GetAccountInfo(request, result =>
        {
            displayName = result.AccountInfo.TitleInfo.DisplayName;
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = "User_" + playFabId.Substring(0, 5);
            }
            if (result.AccountInfo != null && result.AccountInfo.TitleInfo != null)
            {
                Debug.Log("PlayFab 유저 정보 확인: " + JsonUtility.ToJson(result.AccountInfo));
            }
            else
            {
                Debug.LogError("PlayFab 유저 정보 가져오기 실패: AccountInfo가 null임");
            }
        }, error => Debug.LogError("DisplayName 가져오기 실패: " + error.GenerateErrorReport()));
    }

    //  ChatManager에서 유저 DisplayName을 가져올 때 사용하는 함수
    public string GetUserDisplayName()
    {
        return string.IsNullOrEmpty(displayName) ? "Guest" : displayName;
    }

    async void ConnectToWebSocket()
    {
        websocket = new WebSocket($"ws://localhost:8080/?playFabId={playFabId}");

        websocket.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            Debug.Log($"WebSocket에서 받은 메시지: {message}");

            // UI 업데이트 요청
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("메인 스레드에서 AppendChatMessage 실행");
                FindObjectOfType<ChatManager>()?.AppendChatMessage(message);
            });
        };

        await websocket.Connect();
    }


    public WebSocket GetWebSocket()
    {
        return websocket;
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
    // 점수를 업데이트
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


    // 새로운 스테이지를 클리어했을 때 호출m
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
                    PlacementSaveData saveData = JsonUtility.FromJson<PlacementSaveData>(json);

                    if (saveData == null || saveData.placements == null)
                    {
                        Debug.LogError("JSON 데이터가 null이거나 잘못된 구조입니다.");
                        onComplete?.Invoke();
                        return;
                    }

                    GameManager.Instance.ClearPlacedCharacters();

                    foreach (var placement in saveData.placements)
                    {
                        // characterName에서 (Clone) 제거
                        string characterName = placement.characterName.Replace("(Clone)", "").Trim();
                        GameObject characterPrefab = FindCharacterPrefabByName(characterName);

                        if (characterPrefab != null)
                        {
                            GameObject characterInstance = Instantiate(characterPrefab, placement.position, Quaternion.identity);
                            characterInstance.name = characterPrefab.name;
                            Character character = characterInstance.GetComponent<Character>();
                            CharacterManager.Instance.characters.Add(character);
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
    public void SetCurrentLevel(string statType, int newLevel)
    {
        switch (statType)
        {
            case "attackPower": attackPowerLevel = newLevel; break;
            case "attackSpeed": attackSpeedLevel = newLevel; break;
            case "criticalRate": criticalRateLevel = newLevel; break;
            case "criticalDamage": criticalDamageLevel = newLevel; break;
        }
    }
    public void SaveStatsToPlayFab()
    {
        var data = new Dictionary<string, string>
        {
            { "attackPowerLevel", attackPowerLevel.ToString() },
            { "attackSpeedLevel", attackSpeedLevel.ToString() },
            { "criticalRateLevel", criticalRateLevel.ToString() },
            { "criticalDamageLevel", criticalDamageLevel.ToString() }
        };

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
        {
            Data = data
        },
        result => Debug.Log("스탯 레벨 저장 성공!"),
        error => Debug.LogError("스탯 저장 실패: " + error.ErrorMessage));
    }

    /// <summary>
    /// PlayFab에서 저장된 스탯 레벨 불러오기
    /// </summary>
    public void LoadStatsFromPlayFab()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
        result =>
        {
            if (result.Data != null)
            {
                if (result.Data.ContainsKey("attackPowerLevel"))
                    attackPowerLevel = int.Parse(result.Data["attackPowerLevel"].Value);
                if (result.Data.ContainsKey("attackSpeedLevel"))
                    attackSpeedLevel = int.Parse(result.Data["attackSpeedLevel"].Value);
                if (result.Data.ContainsKey("criticalRateLevel"))
                    criticalRateLevel = int.Parse(result.Data["criticalRateLevel"].Value);
                if (result.Data.ContainsKey("criticalDamageLevel"))
                    criticalDamageLevel = int.Parse(result.Data["criticalDamageLevel"].Value);

                Debug.Log("스탯 레벨 불러오기 성공!");
            }
        },
        error => Debug.LogError("스탯 불러오기 실패: " + error.ErrorMessage));
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
