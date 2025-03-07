using System.Text;
using UnityEngine;
using UnityEngine.UI;
using NativeWebSocket;
using TMPro;

public class ChatManager : MonoBehaviour
{
    public TMP_InputField chatInputField;
    public Button sendButton;
    public Transform chatContent; // ä�� �޽����� �߰��� Content
    public GameObject chatMessagePrefab; // ä�� �޽��� ������

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
            chatInputField.text = ""; // �Է� �ʵ� �ʱ�ȭ
        }
        else
        {
            Debug.LogError("WebSocket�� ������� �ʾҰų�, �޽����� ��� �ֽ��ϴ�.");
        }
    }
    public void AppendChatMessage(string message)
    {
        GameObject newMessage = Instantiate(chatMessagePrefab, chatContent); // ������ �ν��Ͻ� ����
        TMP_Text messageText = newMessage.GetComponent<TMP_Text>(); // TextMeshPro ������Ʈ ��������
        messageText.text = message;
    }
}
