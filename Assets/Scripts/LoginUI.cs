using System;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LoginUI : MonoBehaviour
{
    public TextMeshProUGUI tmp;
    private void Start()
    {
#if UNITY_ANDROID
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
            .AddOauthScope("profile")
            .RequestServerAuthCode(false)
            .RequestIdToken()
            .Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;

        PlayGamesPlatform.Activate();
#endif
    }

    // 구글 페더레이션 로그인/회원가입
    public void GoogleAuthorizeFederation(Action<bool, string> func)
    {
#if UNITY_ANDROID

        Social.localUser.Authenticate((bool success) =>
        {
            if (success)
            {
                PlayFabAuthService.Instance.AuthTicket = PlayGamesPlatform.Instance.GetServerAuthCode();
                PlayFabAuthService.Instance.Authenticate(Authtypes.Google);
            }
            else
            {
                //GoogleStatusText.text = "Google Failed to Authorize your login";
            }
        });
    }
#endif
    public void HelloWorld()
    {
        Social.localUser.Authenticate((bool success) =>
        {
            if (success)
            {
                PlayFabAuthService.Instance.AuthTicket = PlayGamesPlatform.Instance.GetServerAuthCode();
                PlayFabAuthService.Instance.Authenticate(Authtypes.Google);
            }
            else
            {
                //GoogleStatusText.text = "Google Failed to Authorize your login";
            }
        });

    }
    public void OnClickLogin()
    {

        Social.localUser.Authenticate((bool success) =>
        {

            if (success)
            {
                var serverAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();

                PlayFabClientAPI.LoginWithGoogleAccount(new LoginWithGoogleAccountRequest()
                {
                    TitleId = PlayFabSettings.TitleId,
                    ServerAuthCode = serverAuthCode,
                    CreateAccount = true
                }, (result) =>
                {
                    tmp.text = serverAuthCode;

                }, (error) =>
                {
                    tmp.text = error.ToString();
                    return;
                }
                );
            }
            else
            {
            }

        });
    }
    public void OnSignInButtonClicked()
    {
        Social.localUser.Authenticate((bool success) => {

            if (success)
            {
                tmp.text = "Google Signed In";
                var serverAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
                tmp.text = "Server Auth Code: " + serverAuthCode;

                PlayFabClientAPI.LoginWithGoogleAccount(new LoginWithGoogleAccountRequest()
                {
                    TitleId = PlayFabSettings.TitleId,
                    ServerAuthCode = serverAuthCode,
                    CreateAccount = true
                }, (result) =>
                {
                    tmp.text = "Signed In as " + result.PlayFabId;
                }, OnPlayFabError);
            }
            else
            {
                tmp.text = "Failed Login"; 
            }

        });

    }
    private void OnPlayFabError(PlayFabError obj)
    {
        tmp.text = "Playfab error";
    }

    void OnEnable()
        {
            PlayFabAuthService.OnLoginSuccess += PlayFabLogin_OnLoginSuccess;
            PlayFabAuthService.OnPlayFabError += PlayFabLogin_OnPlayFabError;
        }

        private void PlayFabLogin_OnLoginSuccess(LoginResult result)
        {
        }

        private void PlayFabLogin_OnPlayFabError(PlayFabError error)
        {
        }
    public void OnClickGoogleLogin() //구글 로그인 버튼
    {
#if UNITY_ANDROID
        LoginGoogleAuthenticate();
#else
        SetEditorOnlyMessage("Only Android Platform");
#endif
    }

    private void LoginGoogleAuthenticate() //로그인 시도
    {
        Debug.Log("구글 로그인 시도중");

        if (Social.localUser.authenticated)
        {
            return;
        }

        Social.localUser.Authenticate((bool success) =>
        {
            if (!success)
            {
                return;
            }

            var serverAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
            PlayFabClientAPI.LoginWithGoogleAccount(new LoginWithGoogleAccountRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                ServerAuthCode = serverAuthCode,
                CreateAccount = true,
            },
            result =>
            {
                OnLoginSuccess(result);
            },
            error =>
            {
            });
        });
    }
    public void OnLoginSuccess(LoginResult result) //로그인 결과
    {
    }
}
