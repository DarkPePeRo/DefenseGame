// ? Google �α��� + PlayFab ���� (Android ����)
using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class PlayFabAuthService1 : MonoBehaviour
{
    public static PlayFabAuthService1 Instance;
    public Action OnLoginSuccess;
    public string PlayFabId { get; private set; }
    public string DisplayName { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
                .RequestServerAuthCode(true) // �ݵ�� true���� PlayFab ���� ����
                .Build();

            PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.Activate();
        }
        else Destroy(gameObject);
    }
    public void LoginWithCustomID()
    {
        PlayFabSettings.TitleId = "1FD00";

        // 1. PlayerPrefs�� ����� UUID ����ϰų�, ������ ���� ����
        string customId;
        if (PlayerPrefs.HasKey("custom_id"))
        {
            customId = PlayerPrefs.GetString("custom_id");
        }
        else
        {
            customId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString("custom_id", customId);
            PlayerPrefs.Save();
        }

        var request = new LoginWithCustomIDRequest
        {
            CustomId = customId,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLogin, OnError);
    }


    public void LoginWithGoogle()
    {
        Debug.Log("[Auth] Google �α��� �õ�...");

        PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptAlways, (result) =>
        {
            if (result == SignInStatus.Success)
            {
                string serverAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
                Debug.Log("[Auth] GPGS �α��� ����, AuthCode: " + serverAuthCode);

                if (string.IsNullOrEmpty(serverAuthCode))
                {
                    Debug.LogError("[Auth] serverAuthCode�� ����ֽ��ϴ�. ���� Ȯ�� �ʿ�.");
                    return;
                }

                PlayFabClientAPI.LoginWithGooglePlayGamesServices(new LoginWithGooglePlayGamesServicesRequest
                {
                    TitleId = PlayFabSettings.TitleId,
                    ServerAuthCode = serverAuthCode,
                    CreateAccount = true
                }, OnLogin, OnError);
            }
            else
            {
                Debug.LogError("[Auth] GPGS �α��� ����: " + result);
            }
        });
    }

    private void OnLogin(LoginResult result)
    {
        PlayFabId = result.PlayFabId;
        Debug.Log("[Auth] PlayFab �α��� ����: " + PlayFabId);
        FetchDisplayName();
        OnLoginSuccess?.Invoke();
    }

    private void FetchDisplayName()
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), result =>
        {
            DisplayName = result.AccountInfo.TitleInfo.DisplayName;
            if (string.IsNullOrEmpty(DisplayName))
                DisplayName = "User_" + PlayFabId.Substring(0, 5);
        }, error => Debug.LogError("[Auth] DisplayName �ҷ����� ����: " + error.GenerateErrorReport()));
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("[Auth] PlayFab �α��� ����: " + error.GenerateErrorReport());
    }
}
