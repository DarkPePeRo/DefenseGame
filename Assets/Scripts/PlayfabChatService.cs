using NativeWebSocket;
using PimDeWitte.UnityMainThreadDispatcher;
using System;
using System.Text;
using UnityEngine;

public class PlayFabChatService : MonoBehaviour
{
    public static PlayFabChatService Instance;
    private WebSocket websocket;

    public event Action<string> OnMessageReceived;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public async void Connect(string playFabId)
    {
        websocket = new WebSocket($"ws://localhost:8080/?playFabId={playFabId}");

        websocket.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            Debug.Log($"WebSocket���� ���� �޽���: {message}");

            // UI ������Ʈ ��û
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("���� �����忡�� AppendChatMessage ����");
                FindObjectOfType<ChatManager>()?.AppendChatMessage(message);
            });
        };

        await websocket.Connect();
    }

    private void Update()
    {
        websocket?.DispatchMessageQueue();
    }

    public async void SendChatMessage(string message)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
            await websocket.SendText(message);
    }

    public WebSocket GetWebSocket() => websocket;
}
