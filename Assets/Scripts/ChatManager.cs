using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using NativeWebSocket;
using TMPro;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections;
using System;

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
    public GameObject topSpacer;

    private Queue<string> messageQueue = new Queue<string>();
    private Coroutine chatHomeFadeCoroutine;

    private Dictionary<string, Queue<GameObject>> messagePools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, List<GameObject>> activeMessages = new Dictionary<string, List<GameObject>>();

    [SerializeField] private int maxActiveMessages = 50;
    [SerializeField] private int maxPoolSize = 100;

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
        SignalRClient.Instance.OnMessageReceived += AppendChatMessage;
        sendButton.onClick.AddListener(SendChatMessage);
    }

    void Update()
    {
        while (messageQueue.Count > 0)
        {
            string message = messageQueue.Dequeue();

            string myName = PlayFabAuthService1.Instance.DisplayName;
            string sender = ExtractSenderName(message);
            string body = ExtractMessageBody(message);
            DateTime sentTime = ExtractSentTime(message);

            bool isMyMessage = sender == myName;

            string key = isMyMessage ? "my" : "other";
            string homeKey = isMyMessage ? "my_home" : "other_home";

            GameObject newMessage = CreateChatMessage(key, isMyMessage ? myChatMessagePrefab : otherChatMessagePrefab, chatContent, message);
            SetMessageTexts(newMessage, sender, body, sentTime, isMyMessage);
            newMessage.transform.SetAsLastSibling();

            GameObject newMessageHome = CreateChatMessage(homeKey, isMyMessage ? myChatMessagePrefabHome : otherChatMessagePrefabHome, chatContentHome, message);
            SetMessageTexts(newMessageHome, sender, body, sentTime, isMyMessage);
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

    private DateTime ExtractSentTime(string message)
    {
        int start = message.LastIndexOf("<time:") + 6;
        int end = message.LastIndexOf(">");
        if (start > 5 && end > start)
        {
            string timeStr = message.Substring(start, end - start);
            if (DateTime.TryParse(timeStr, out var parsed)) return parsed;
        }
        return DateTime.Now;
    }

    private GameObject CreateChatMessage(string key, GameObject prefab, Transform parent, string message)
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

    private void SetMessageTexts(GameObject messageObj, string sender, string body, DateTime sentTime, bool isMyMessage)
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
            activeTimeTexts.Add(new ChatTimeData { timeText = timeText, sentTime = sentTime });
        }
    }

    private string ExtractSenderName(string message)
    {
        int start = message.IndexOf("[") + 1;
        int end = message.IndexOf(" ]");
        if (start >= 0 && end > start)
        {
            return message.Substring(start, end - start).Trim();
        }
        return "";
    }

    private string ExtractMessageBody(string message)
    {
        int end = message.IndexOf("] ");
        if (end >= 0 && end + 2 < message.Length)
        {
            return message.Substring(end + 2).Trim();
        }
        return message;
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


    public void AppendChatMessage(string message)
    {
        messageQueue.Enqueue(message);
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

    public void ClearChat()
    {
        foreach (Transform child in chatContentHome)
        {
            if (child == topSpacer.transform) continue;

            if (child.gameObject.name.Contains("My"))
                RecycleMessage("my_home", child.gameObject);
            else
                RecycleMessage("other_home", child.gameObject);
        }

        Instantiate(topSpacer, chatContentHome);
        chatScrollRectHome.verticalNormalizedPosition = 0f;
    }
}

