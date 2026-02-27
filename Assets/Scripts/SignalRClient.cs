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
    public event Action<string> OnMessageReceived;
    public event Action<ChatMessageDto> OnMessageReceivedDto;

    private const int HUB_PORT = 5236;
    private const string HUB_PATH = "/chathub";

    private string AUTH_HUB_URL;

    private bool isConnecting = false;
    [Serializable]
    public class ChatMessageDto
    {
        public string Sender;
        public string Body;
        public string SentUtc;   // ISO string으로 받기(파싱 안정)
        public string Channel;
    }
    private void Awake()
    {
        Debug.Log($"[SignalRClient] Awake instanceId={GetInstanceID()} scene={gameObject.scene.name}");
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
    private static string GetHubBaseUrl()
    {
#if UNITY_EDITOR
        // 에디터(PC 실행)
        return $"http://localhost:{HUB_PORT}";
#elif UNITY_ANDROID
        // Android 에뮬레이터: 호스트 PC는 10.0.2.2
        // (실기기는 여기 값이 LAN IP여야 함)
        return $"http://10.0.2.2:{HUB_PORT}";
#else
        // 다른 플랫폼(일단 안전하게)
        return $"http://localhost:{HUB_PORT}";
#endif
    }

    private static string BuildHubUrl(string playFabId, string entityToken)
    {
        var baseUrl = GetHubBaseUrl();
        var url = $"{baseUrl}{HUB_PATH}?playFabId={UnityWebRequestEscape(playFabId)}&access_token={UnityWebRequestEscape(entityToken)}";
        return url;
    }
    // Uri.EscapeDataString 써도 되는데, Unity 쪽에서 문자열 깨짐 방지용으로 분리
    private static string UnityWebRequestEscape(string s)
        => string.IsNullOrEmpty(s) ? "" : Uri.EscapeDataString(s);
    public async void ConnectAfterLogin(string playFabId)
    {
        if (hub != null && hub.State == ConnectionStates.Connected)
            return;

        await CreateAndConnectHub(playFabId);
    }

    private async Task CreateAndConnectHub(string playFabId)
    {
        var encoder = new LitJsonEncoder();
        var protocol = new JsonProtocol(encoder);


        string token = PlayFabAuthService1.Instance.EntityToken;

        AUTH_HUB_URL = BuildHubUrl(playFabId, token);

        hub = new HubConnection(new Uri(AUTH_HUB_URL), protocol);

        // AuthenticationProvider는 일단 제거(중복 전달 방지)
        hub.AuthenticationProvider = null;

        //hub.AuthenticationProvider = new PlayFabAuthenticator(token);

        hub.On<ChatMessageDto>("ReceiveMessage", (dto) =>
        {
            OnMessageReceivedDto?.Invoke(dto);
        });

        try
        {
            hub.OnError += (c, err) =>
            {
                Debug.LogError($"[SignalR] OnError: {err}");
            };

            hub.OnClosed += (c) =>
            {
                Debug.LogWarning($"[SignalR] OnClosed. State={hub.State}");
            };
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
        Debug.Log("[SendChatMessage] called: " + rawMessage);
        if (hub == null || hub.State == ConnectionStates.Closed)
        {
            if (isConnecting) return;
            isConnecting = true;
            isConnecting = false;
        }

        if (hub == null || hub.State != ConnectionStates.Connected)
        {
            Debug.LogWarning("SignalR 연결이 아직 안 됨");
            return;
        }

        string displayName = PlayFabAuthService1.Instance.DisplayName;

        try
        {
            await hub.SendAsync("SendMessage", rawMessage);
        }
        catch (Exception e)
        {
            Debug.LogError("SignalR 메시지 전송 실패: " + e.Message);
        }
    }

    //private async Task ReconnectHub()
    //{
    //    Debug.Log("[SignalR] 허브 재생성 및 재연결 시도 중...");

    //    var encoder = new LitJsonEncoder();
    //    var protocol = new JsonProtocol(encoder);

    //    hub = new HubConnection(new Uri(AUTH_HUB_URL), protocol);
    //    // 핵심 인증 토큰 설정
    //    string token = PlayFabAuthService1.Instance.EntityToken;
    //    hub.AuthenticationProvider = new PlayFabAuthenticator(token);

    //    hub.On<string>("ReceiveMessage", message =>
    //    {
    //        OnMessageReceived?.Invoke(message);
    //        if (ChatManager.Instance != null)
    //            ChatManager.Instance.AppendChatMessage(message);
    //    });

    //    try
    //    {
    //        await hub.ConnectAsync();
    //        Debug.Log("[SignalR] 재연결 성공");
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError("[SignalR] 재연결 실패: " + ex.Message);
    //    }
    //}
    private async void OnApplicationQuit()
    {
        if (hub != null)
            await hub.CloseAsync();
    }
    private void OnDestroy()
    {
        Debug.Log($"[SignalRClient] OnDestroy instanceId={GetInstanceID()}");
    }
}
