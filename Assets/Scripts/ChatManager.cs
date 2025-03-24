using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using NativeWebSocket;
using TMPro;
using PimDeWitte.UnityMainThreadDispatcher;

public class ChatManager : MonoBehaviour
{
    public TMP_InputField chatInputField;
    public Button sendButton;
    public Transform chatContent; // 채팅 메시지를 추가할 Content
    public GameObject chatMessagePrefab; // 채팅 메시지 프리팹
    public ScrollRect chatScrollRect; // 스크롤뷰

    private WebSocket websocket;
    private Queue<string> messageQueue = new Queue<string>();

    void Start()
    {
        websocket = PlayFabLogin.Instance.GetWebSocket();
        sendButton.onClick.AddListener(SendChatMessage);

        chatInputField.onSubmit.AddListener(delegate { SendChatMessage(); });
    }

    void Update()
    {
        if (websocket != null)
        {
            websocket.DispatchMessageQueue(); // WebSocket 메시지 즉시 처리
        }

        // 메시지 큐에서 UI 업데이트 처리
        while (messageQueue.Count > 0)
        {
            string message = messageQueue.Dequeue();
            GameObject newMessage = Instantiate(chatMessagePrefab, chatContent);
            TMP_Text messageText = newMessage.GetComponent<TMP_Text>();
            messageText.text = message;

            Canvas.ForceUpdateCanvases();
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    public async void SendChatMessage()
    {
        if (websocket != null && websocket.State == WebSocketState.Open && !string.IsNullOrWhiteSpace(chatInputField.text))
        {
            string message = $"{PlayFabLogin.Instance.GetUserDisplayName()}: {chatInputField.text}";
            await websocket.SendText(message);
            chatInputField.text = ""; // 입력 필드 초기화
        }
        // 입력 필드에 다시 포커스 주기 (엔터 후 다시 입력 가능하도록)
        chatInputField.ActivateInputField();
    }

    public void AppendChatMessage(string message)
    {
        // 메시지를 큐에 저장하고 Update()에서 UI 업데이트 처리
        messageQueue.Enqueue(message);
    }
}
