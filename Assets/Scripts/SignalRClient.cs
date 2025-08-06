using UnityEngine;
using Best.SignalR;
using Best.SignalR.Encoders;
using System;
using System.Threading.Tasks;
using System.Data;

public class SignalRClient : MonoBehaviour
{
    public static SignalRClient Instance;
    private HubConnection hub;
    private string entityToken;
    public event Action<string> OnMessageReceived;

    private const string HUB_URL = "http://localhost:5236/chathub";
    private string AUTH_HUB_URL;
    private bool isConnecting = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async void ConnectAfterLogin(string playFabId, string displayName)
    {
        if (hub != null && hub.State == ConnectionStates.Connected)
            return;

        await CreateAndConnectHub(playFabId);
    }

    private async Task CreateAndConnectHub(string playFabId)
    {
        var encoder = new LitJsonEncoder();
        var protocol = new JsonProtocol(encoder);

        var uri = new Uri($"{HUB_URL}?playFabId={playFabId}");
        AUTH_HUB_URL = uri.ToString();

        hub = new HubConnection(new Uri(AUTH_HUB_URL), protocol);

        // 핵심 인증 토큰 설정
        string token = PlayFabAuthService1.Instance.EntityToken;
        hub.AuthenticationProvider = new PlayFabAuthenticator(token);

        hub.On<string>("ReceiveMessage", (message) =>
        {
            OnMessageReceived?.Invoke(message);
            ChatManager.Instance?.AppendChatMessage(message);
        });

        try
        {
            await hub.ConnectAsync();
            Debug.Log("SignalR 연결 성공");
        }
        catch (Exception ex)
        {
            Debug.LogError("SignalR 연결 실패: " + ex.Message);
        }

    }

    public async void SendChatMessage(string rawMessage)
    {
        if (hub == null || hub.State == ConnectionStates.Closed)
        {
            if (isConnecting) return;
            isConnecting = true;
            await ReconnectHub();
            isConnecting = false;
        }

        if (hub == null || hub.State != ConnectionStates.Connected)
        {
            Debug.LogWarning("SignalR 연결이 아직 안 됨");
            return;
        }

        string displayName = PlayFabAuthService1.Instance.DisplayName;
        string wrappedMessage = $"[ {displayName} ] {rawMessage} <time:{DateTime.Now}>";

        try
        {
            await hub.SendAsync("SendMessage", wrappedMessage);
        }
        catch (Exception e)
        {
            Debug.LogError("SignalR 메시지 전송 실패: " + e.Message);
        }
    }

    private async Task ReconnectHub()
    {
        Debug.Log("[SignalR] 허브 재생성 및 재연결 시도 중...");

        var encoder = new LitJsonEncoder();
        var protocol = new JsonProtocol(encoder);

        hub = new HubConnection(new Uri(AUTH_HUB_URL), protocol);
        // 핵심 인증 토큰 설정
        string token = PlayFabAuthService1.Instance.EntityToken;
        hub.AuthenticationProvider = new PlayFabAuthenticator(token);

        hub.On<string>("ReceiveMessage", message =>
        {
            OnMessageReceived?.Invoke(message);
            if (ChatManager.Instance != null)
                ChatManager.Instance.AppendChatMessage(message);
        });

        try
        {
            await hub.ConnectAsync();
            Debug.Log("[SignalR] 재연결 성공");
        }
        catch (Exception ex)
        {
            Debug.LogError("[SignalR] 재연결 실패: " + ex.Message);
        }
    }

    private async void OnDestroy()
    {
        if (hub != null)
        {
            await hub.CloseAsync();
            Debug.Log("SignalR 연결 종료");
        }
    }
}
