// ? 1. 로그인 및 초기화 담당: PlayFabAuthService.cs
using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine;

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
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        PlayFabSettings.TitleId = "1FD00";
        var request = new LoginWithCustomIDRequest { CustomId = SystemInfo.deviceUniqueIdentifier, CreateAccount = true };
        PlayFabClientAPI.LoginWithCustomID(request, OnLogin, OnError);
    }

    private void OnLogin(LoginResult result)
    {
        PlayFabId = result.PlayFabId;
        FetchDisplayName();
        OnLoginSuccess?.Invoke();
    }

    private void FetchDisplayName()
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), result => {
            DisplayName = result.AccountInfo.TitleInfo.DisplayName;
            if (string.IsNullOrEmpty(DisplayName))
                DisplayName = "User_" + PlayFabId.Substring(0, 5);
        }, error => Debug.LogError("DisplayName 불러오기 실패: " + error.GenerateErrorReport()));
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("PlayFab 로그인 실패: " + error.GenerateErrorReport());
    }
}
