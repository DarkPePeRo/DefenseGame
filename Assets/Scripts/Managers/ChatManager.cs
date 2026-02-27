using NativeWebSocket;
using PimDeWitte.UnityMainThreadDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SignalRClient;

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance;

    public TMP_InputField chatInputField;
    public Button sendButton;
    public Transform chatContent;
    public Transform chatContentHome;
    public GameObject otherChatMessagePrefab;
    public GameObject myChatMessagePrefab;
    public GameObject otherChatMessagePrefabHome;
    public GameObject myChatMessagePrefabHome;
    public ScrollRect chatScrollRect;
    public ScrollRect chatScrollRectHome;
    public GameObject chatUI;
    private CanvasGroup chatCanvasGroup;
    public GameObject chatButton;
    public GameObject chatHome;
    private CanvasGroup chatHomeCanvasGroup;
    public GameObject topSpacerPrefab;
    private Transform topSpacerInstance;

    private Queue<ChatMessageDto> messageQueue = new Queue<ChatMessageDto>();
    private Coroutine chatHomeFadeCoroutine;

    private Dictionary<string, Queue<GameObject>> messagePools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, List<GameObject>> activeMessages = new Dictionary<string, List<GameObject>>();

    [SerializeField] private int maxActiveMessages = 20;
    [SerializeField] private int maxPoolSize = 50;

    private class ChatTimeData
    {
        public TMP_Text timeText;
        public DateTime sentTime;
    }
    private List<ChatTimeData> activeTimeTexts = new List<ChatTimeData>();
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {
        chatInputField.characterLimit = 60;
        chatCanvasGroup = chatUI.GetComponent<CanvasGroup>();
        chatHomeCanvasGroup = chatHome.GetComponent<CanvasGroup>();
        chatInputField.onSubmit.AddListener(delegate { SendChatMessage(); });
        StartCoroutine(UpdateChatTimesLoop());
        SignalRClient.Instance.OnMessageReceivedDto += AppendChatMessageDto;
        sendButton.onClick.AddListener(SendChatMessage);
        EnsureTopSpacer();
    }

    void Update()
    {
        while (messageQueue.Count > 0)
        {
            ChatMessageDto dto = messageQueue.Dequeue();

            string myName = PlayFabAuthService1.Instance.DisplayName;
            string sender = dto.Sender ?? "";
            string body = dto.Body ?? "";

            DateTime sentUtc;
            if (!DateTime.TryParse(dto.SentUtc, null, DateTimeStyles.RoundtripKind, out sentUtc))
                sentUtc = DateTime.UtcNow;

            // elapsed 계산은 로컬이든 UTC든 상관 없지만, 기준을 통일하자
            // FormatElapsedTime이 DateTime.Now 기반이라 localTime으로 변환해서 넣음
            DateTime sentLocal = sentUtc.ToLocalTime();

            bool isMyMessage = sender == myName;

            string key = isMyMessage ? "my" : "other";
            string homeKey = isMyMessage ? "my_home" : "other_home";

            GameObject newMessage = CreateChatMessage(key, isMyMessage ? myChatMessagePrefab : otherChatMessagePrefab, chatContent);
            SetMessageTexts(newMessage, sender, body, sentLocal);
            newMessage.transform.SetAsLastSibling();

            GameObject newMessageHome = CreateChatMessage(homeKey, isMyMessage ? myChatMessagePrefabHome : otherChatMessagePrefabHome, chatContentHome);
            SetMessageTexts(newMessageHome, sender, body, sentLocal);
            newMessageHome.transform.SetAsLastSibling();

            if (chatHomeFadeCoroutine != null) StopCoroutine(chatHomeFadeCoroutine);
            chatHomeFadeCoroutine = StartCoroutine(FadeOutChatHomeUI());

            Canvas.ForceUpdateCanvases();
            chatScrollRect.verticalNormalizedPosition = 0f;
            chatScrollRectHome.verticalNormalizedPosition = 0f;
        }
    }

    private void ResetMessage(GameObject obj)
    {
        if (obj == null) return;

        TMP_Text[] texts = obj.GetComponentsInChildren<TMP_Text>(true);
        foreach (var t in texts) t.text = "";
        activeTimeTexts.RemoveAll(c => c.timeText == null || c.timeText.gameObject == obj);
    }

    private IEnumerator UpdateChatTimesLoop()
    {
        while (true)
        {
            foreach (var chat in activeTimeTexts)
                if (chat.timeText != null)
                    chat.timeText.text = FormatElapsedTime(chat.sentTime);

            yield return new WaitForSeconds(10f);
        }
    }

    private string FormatElapsedTime(DateTime sentTime)
    {
        var elapsed = DateTime.Now - sentTime;
        if (elapsed.TotalMinutes < 1) return "방금";
        else if (elapsed.TotalMinutes < 60) return $"{(int)elapsed.TotalMinutes}분 전";
        else if (elapsed.TotalHours < 24) return $"{(int)elapsed.TotalHours}시간 전";
        return "오래 전";
    }


    private GameObject CreateChatMessage(string key, GameObject prefab, Transform parent)
    {
        GameObject obj = GetPooledMessage(key, prefab, parent);
        if (obj == null) return null;

        if (!activeMessages.ContainsKey(key))
            activeMessages[key] = new List<GameObject>();

        activeMessages[key].Add(obj);

        if (activeMessages[key].Count > maxActiveMessages)
        {
            var oldest = activeMessages[key][0];
            activeMessages[key].RemoveAt(0);
            RecycleMessage(key, oldest);
        }

        return obj;
    }

    private GameObject GetPooledMessage(string key, GameObject prefab, Transform parent)
    {
        if (!messagePools.ContainsKey(key))
            messagePools[key] = new Queue<GameObject>();

        while (messagePools[key].Count > 0)
        {
            var obj = messagePools[key].Dequeue();
            if (obj == null) continue; // Destroy된 객체 무시

            obj.transform.SetParent(parent, false);
            obj.SetActive(true);
            ResetMessage(obj);
            return obj;
        }

        return Instantiate(prefab, parent, false);
    }

    private void RecycleMessage(string key, GameObject obj)
    {
        if (obj == null) return;

        ResetMessage(obj);
        obj.SetActive(false);

        if (!messagePools.ContainsKey(key))
            messagePools[key] = new Queue<GameObject>();

        if (messagePools[key].Count < maxPoolSize)
            messagePools[key].Enqueue(obj);
        else
            Destroy(obj);
    }

    private void SetMessageTexts(GameObject messageObj, string sender, string body, DateTime sentTime)
    {
        TMP_Text[] texts = messageObj.GetComponentsInChildren<TMP_Text>(true);

        TMP_Text nicknameText = null;
        TMP_Text messageText = null;
        TMP_Text timeText = null;

        foreach (var t in texts)
        {
            if (t.name == "Nickname") nicknameText = t;
            else if (t.name == "Message") messageText = t;
            else if (t.name == "Time") timeText = t;
        }

        if (nicknameText != null)
        {
            nicknameText.text = sender;
            nicknameText.gameObject.SetActive(true);
        }
        if (messageText != null)
        {
            messageText.text = body;
        }
        if (timeText != null)
        {
            timeText.text = FormatElapsedTime(sentTime);

            bool already = activeTimeTexts.Exists(x => x.timeText == timeText);
            if (!already)
                activeTimeTexts.Add(new ChatTimeData { timeText = timeText, sentTime = sentTime });
        }
    }

    public void SendChatMessage()
    {
        if (!string.IsNullOrWhiteSpace(chatInputField.text))
        {
            SignalRClient.Instance.SendChatMessage(chatInputField.text);
            chatInputField.text = "";
        }

        chatInputField.ActivateInputField();
    }


    public void AppendChatMessageDto(ChatMessageDto dto)
    {
        messageQueue.Enqueue(dto);
    }
    public void OnChat()
    {
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
            chatUI.SetActive(true);
            float duration = 0.2f;
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
            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                chatCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            chatButton.SetActive(true);
            chatUI.SetActive(false);
        }
    }

    private IEnumerator FadeOutChatHomeUI()
    {
        if (chatHomeCanvasGroup != null)
        {
            chatHomeCanvasGroup.alpha = 1;
            yield return new WaitForSeconds(3f);
            chatHomeCanvasGroup.alpha = 0;
            ClearChat();
        }
    }
    private void EnsureTopSpacer()
    {
        if (topSpacerInstance != null) return;
        var go = Instantiate(topSpacerPrefab, chatContentHome, false);
        topSpacerInstance = go.transform;
        topSpacerInstance.SetAsFirstSibling();
    }
    public void ClearChat()
    {
        EnsureTopSpacer();

        for (int i = chatContentHome.childCount - 1; i >= 0; i--)
        {
            var child = chatContentHome.GetChild(i);
            if (child == topSpacerInstance) continue;

            bool isMy = child.gameObject.name.Contains("My");
            RecycleMessage(isMy ? "my_home" : "other_home", child.gameObject);
        }

        topSpacerInstance.SetAsFirstSibling();
        chatScrollRectHome.verticalNormalizedPosition = 0f;
    }

}

