using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabLogin : MonoBehaviour
{
    public static PlayFabLogin Instance;

    public LoadingManager LoadingManager;
    private float saveInterval = 60f;
    private float timeSinceLastSave = 0; 
    
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
        LoadCurrencyFromServer(() =>
        {
            OfflineRewardSystem.Instance.GiveOfflineReward();
            LoadingManager.LoadScene("SampleScene");
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

}
