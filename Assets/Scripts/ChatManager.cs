using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using NativeWebSocket;
using TMPro;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections;
using Photon.Chat.Demo;

public class ChatManager : MonoBehaviour
{
    public TMP_InputField chatInputField;
    public Button sendButton;
    public Transform chatContent; // ä�� �޽����� �߰��� Content
    public Transform chatContentHome; // ä�� �޽����� �߰��� Content
    public GameObject chatMessagePrefab; // ä�� �޽��� ������
    public ScrollRect chatScrollRect; // ��ũ�Ѻ�
    public ScrollRect chatScrollRectHome; // ��ũ�Ѻ�
    public GameObject chatUI;
    private CanvasGroup chatCanvasGroup;
    public GameObject chatButton;
    public GameObject chatHome;
    private CanvasGroup chatHomeCanvasGroup;
    public GameObject topSpacer;

    private WebSocket websocket;
    private Queue<string> messageQueue = new Queue<string>();
    private Coroutine chatHomeFadeCoroutine; // ä�� Ȩ Fade �ڷ�ƾ �����

    void Start()
    {
        websocket = PlayFabLogin.Instance.GetWebSocket();
        sendButton.onClick.AddListener(SendChatMessage);
        chatCanvasGroup = chatUI.GetComponent<CanvasGroup>();
        chatHomeCanvasGroup = chatHome.GetComponent<CanvasGroup>();

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
            GameObject newMessageHome = Instantiate(chatMessagePrefab);
            newMessageHome.transform.SetParent(chatContentHome, false);
            newMessageHome.transform.SetAsLastSibling();
            TMP_Text messageTextHome = newMessageHome.GetComponent<TMP_Text>();
            messageTextHome.text = message;
            
            // ���� �ڷ�ƾ�� ���� ������ ����
            if (chatHomeFadeCoroutine != null)
            {
                StopCoroutine(chatHomeFadeCoroutine);
            }

            // ���� ����
            chatHomeFadeCoroutine = StartCoroutine(FadeOutChatHomeUI());

            Canvas.ForceUpdateCanvases();
            chatScrollRect.verticalNormalizedPosition = 0f;
            chatScrollRectHome.verticalNormalizedPosition = 0f;
        }
    }

    public async void SendChatMessage()
    {
        if (websocket != null && websocket.State == WebSocketState.Open && !string.IsNullOrWhiteSpace(chatInputField.text))
        {
            string message = $"[ {PlayFabLogin.Instance.GetUserDisplayName()} ] {chatInputField.text}";
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


    public void OnChat()
    {
        Debug.Log("Shop");
        StartCoroutine(FadeInChatUI());

    }
    public void OffChat()
    {
        StartCoroutine(FadeOutChatUI());
    }
    private IEnumerator FadeInChatUI()
    {
        if (chatCanvasGroup != null)
        {
            chatButton.SetActive(false);
            chatUI.SetActive(true); // UI Ȱ��ȭ
            float duration = 0.2f; // ���̵� �� ���� �ð�
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                chatCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

        }
    }
    private IEnumerator FadeOutChatUI()
    {
        if (chatCanvasGroup != null)
        {
            float duration = 0.2f; // ���̵� �ƿ� ���� �ð�
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                chatCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            chatButton.SetActive(true);
            chatUI.SetActive(false); // UI ��Ȱ��ȭ
        }
    }
    private IEnumerator FadeOutChatHomeUI()
    {
        if (chatHomeCanvasGroup != null)
        {
            chatHomeCanvasGroup.alpha = 1;
            yield return new WaitForSeconds(3f); // 3�� ���
            chatHomeCanvasGroup.alpha = 0;
            ClearChat();
        }
    }
    public void ClearChat()
    {
        foreach (Transform child in chatContentHome)
        {
            Destroy(child.gameObject);
        }
        Instantiate(topSpacer, chatContentHome);
        chatScrollRectHome.verticalNormalizedPosition = 0f;
    }
}
