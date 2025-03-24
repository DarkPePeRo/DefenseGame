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
    public Transform chatContent; // ä�� �޽����� �߰��� Content
    public GameObject chatMessagePrefab; // ä�� �޽��� ������
    public ScrollRect chatScrollRect; // ��ũ�Ѻ�

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
            websocket.DispatchMessageQueue(); // WebSocket �޽��� ��� ó��
        }

        // �޽��� ť���� UI ������Ʈ ó��
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
            chatInputField.text = ""; // �Է� �ʵ� �ʱ�ȭ
        }
        // �Է� �ʵ忡 �ٽ� ��Ŀ�� �ֱ� (���� �� �ٽ� �Է� �����ϵ���)
        chatInputField.ActivateInputField();
    }

    public void AppendChatMessage(string message)
    {
        // �޽����� ť�� �����ϰ� Update()���� UI ������Ʈ ó��
        messageQueue.Enqueue(message);
    }
}
