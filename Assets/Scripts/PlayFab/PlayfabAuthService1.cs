using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using PlayFab.AuthenticationModels;

public class PlayFabAuthService1 : MonoBehaviour
{
    public static PlayFabAuthService1 Instance;
    [SerializeField] private LoadingManager loadingManager;
    public Action OnLoginSuccess;
    public string PlayFabId { get; private set; }
    public string DisplayName { get; private set; }
    public string EntityToken { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
                .RequestServerAuthCode(true) // 반드시 true여야 PlayFab 연동 가능
                .Build();

            PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.Activate();
            QualitySettings.vSyncCount = 0;   // vSync 끔
            Application.targetFrameRate = 60; // 60 (원하면 120)
        }
        else Destroy(gameObject);
    }
    public void LoginWithCustomID()
    {
        PlayFabSettings.TitleId = "1FD00";

        // 1. PlayerPrefs에 저장된 UUID 사용하거나, 없으면 새로 생성
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

        PlayFabClientAPI.LoginWithCustomID(request, result =>
        {
            PlayFabAuthenticationAPI.GetEntityToken(new GetEntityTokenRequest
            {
                AuthenticationContext = result.AuthenticationContext
            }, tokenResult =>
            {
                EntityToken = tokenResult.EntityToken;
                Debug.Log("EntityToken 성공: " + EntityToken);
                OnLoginSuccess?.Invoke();
                loadingManager.StartLoading();
                EquipmentInventoryService.Instance.LoadFromServerAfterLogin();
                SignalRClient.Instance.ConnectAfterLogin(result.PlayFabId);
            }, error =>
            {
                Debug.LogError("EntityToken 실패: " + error.GenerateErrorReport());
            });

            PlayFabId = result.PlayFabId;
        }, OnError);
    }


    public void LoginWithGoogle()
    {
        Debug.Log("[Auth] Google 로그인 시도...");

        PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptAlways, (result) =>
        {
            if (result == SignInStatus.Success)
            {
                string serverAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
                Debug.Log("[Auth] GPGS 로그인 성공, AuthCode: " + serverAuthCode);

                if (string.IsNullOrEmpty(serverAuthCode))
                {
                    Debug.LogError("[Auth] serverAuthCode가 비어있습니다. 설정 확인 필요.");
                    return;
                }

                PlayFabClientAPI.LoginWithGooglePlayGamesServices(new LoginWithGooglePlayGamesServicesRequest
                {
                    TitleId = PlayFabSettings.TitleId,
                    ServerAuthCode = serverAuthCode,
                    CreateAccount = true
                }, result => {
                    OnLogin(result);
                    RequestEntityToken();
                }, OnError);
            }
            else
            {
                Debug.LogError("[Auth] GPGS 로그인 실패: " + result);
            }
        });
    }

    private void OnLogin(LoginResult result)
    {
        PlayFabId = result.PlayFabId;
        Debug.Log("[Auth] PlayFab 로그인 성공: " + PlayFabId);
        FetchDisplayName();
        OnLoginSuccess?.Invoke();
    }
    private void RequestEntityToken()
    {
        PlayFabAuthenticationAPI.GetEntityToken(new GetEntityTokenRequest
        {
            AuthenticationContext = PlayFabSettings.staticPlayer
        }, result =>
        {
            EntityToken = result.EntityToken;
            Debug.Log("EntityToken: " + EntityToken);
        }, error =>
        {
            Debug.LogError("EntityToken 요청 실패: " + error.GenerateErrorReport());
        });
    }

    public void FetchDisplayName()
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), result =>
        {
            DisplayName = result.AccountInfo.TitleInfo.DisplayName;
            if (string.IsNullOrEmpty(DisplayName))
                DisplayName = "User_" + PlayFabId.Substring(0, 5);
        }, error => Debug.LogError("[Auth] DisplayName 불러오기 실패: " + error.GenerateErrorReport()));
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("[Auth] PlayFab 로그인 실패: " + error.GenerateErrorReport());
    }
}
