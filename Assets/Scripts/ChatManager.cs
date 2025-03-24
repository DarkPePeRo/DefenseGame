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
    public Transform chatContent; // 채팅 메시지를 추가할 Content
    public Transform chatContentHome; // 채팅 메시지를 추가할 Content
    public GameObject chatMessagePrefab; // 채팅 메시지 프리팹
    public ScrollRect chatScrollRect; // 스크롤뷰
    public ScrollRect chatScrollRectHome; // 스크롤뷰
    public GameObject chatUI;
    private CanvasGroup chatCanvasGroup;
    public GameObject chatButton;
    public GameObject chatHome;
    private CanvasGroup chatHomeCanvasGroup;
    public GameObject topSpacer;

    private WebSocket websocket;
    private Queue<string> messageQueue = new Queue<string>();
    private Coroutine chatHomeFadeCoroutine; // 채팅 홈 Fade 코루틴 제어용

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
            websocket.DispatchMessageQueue(); // WebSocket 메시지 즉시 처리
        }

        // 메시지 큐에서 UI 업데이트 처리
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
            
            // 기존 코루틴이 돌고 있으면 중지
            if (chatHomeFadeCoroutine != null)
            {
                StopCoroutine(chatHomeFadeCoroutine);
            }

            // 새로 시작
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
            chatUI.SetActive(true); // UI 활성화
            float duration = 0.2f; // 페이드 인 지속 시간
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
            float duration = 0.2f; // 페이드 아웃 지속 시간
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                chatCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            chatButton.SetActive(true);
            chatUI.SetActive(false); // UI 비활성화
        }
    }
    private IEnumerator FadeOutChatHomeUI()
    {
        if (chatHomeCanvasGroup != null)
        {
            chatHomeCanvasGroup.alpha = 1;
            yield return new WaitForSeconds(3f); // 3초 대기
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
