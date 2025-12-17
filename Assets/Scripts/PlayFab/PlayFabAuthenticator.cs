using System;
using Best.SignalR.Authentication;
using Best.HTTP;
using Best.SignalR;

public class PlayFabAuthenticator : IAuthenticationProvider
{
    private string token;
    public bool IsPreAuthRequired => true;

    public event OnAuthenticationSuccededDelegate OnAuthenticationSucceded;
    public event OnAuthenticationFailedDelegate OnAuthenticationFailed;

    public PlayFabAuthenticator(string entityToken)
    {
        token = entityToken;
    }

    public void StartAuthentication()
    {
        OnAuthenticationSucceded?.Invoke(this);
    }

    public void PrepareRequest(HTTPRequest request)
    {
        request.SetHeader("Authorization", $"Bearer {token}");
    }

    public Uri PrepareUri(Uri uri)
    {
        // 필요 시 쿼리스트링 등에 추가 가능
        return uri;
    }

    public void Cancel() { }
}
