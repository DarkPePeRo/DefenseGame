using System.Text;
using UnityEngine;
using UnityEngine.UI;
using NativeWebSocket;
using TMPro;

public class ChatManager : MonoBehaviour
{
    public TMP_InputField chatInputField;
    public Button sendButton;
    public Transform chatContent; // 채팅 메시지를 추가할 Content
    public GameObject chatMessagePrefab; // 채팅 메시지 프리팹

    private WebSocket websocket;

    void Start()
    {
        websocket = PlayFabLogin.Instance.GetWebSocket();
        sendButton.onClick.AddListener(SendChatMessage);
    }

    public async void SendChatMessage()
    {
        if (websocket != null && websocket.State == WebSocketState.Open && !string.IsNullOrWhiteSpace(chatInputField.text))
        {
            string message = $"{PlayFabLogin.Instance.GetUserDisplayName()}: {chatInputField.text}";
            await websocket.SendText(message);
            chatInputField.text = ""; // 입력 필드 초기화
        }
        else
        {
            Debug.LogError("WebSocket이 연결되지 않았거나, 메시지가 비어 있습니다.");
        }
    }
    public void AppendChatMessage(string message)
    {
        GameObject newMessage = Instantiate(chatMessagePrefab, chatContent); // 프리팹 인스턴스 생성
        TMP_Text messageText = newMessage.GetComponent<TMP_Text>(); // TextMeshPro 컴포넌트 가져오기
        messageText.text = message;
    }
}
