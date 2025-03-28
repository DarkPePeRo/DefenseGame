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
    public TMP_InputField chatInputField;
    public Button sendButton;
    public Transform chatContent; // ä�� �޽����� �߰��� Content
    public Transform chatContentHome; // ä�� �޽����� �߰��� Content
    public GameObject otherChatMessagePrefab; // �� ä�� �޽��� ������
    public GameObject myChatMessagePrefab; // �� ä�� �޽��� ������
    public GameObject otherChatMessagePrefabHome; // �� ä�� �޽��� ������
    public GameObject myChatMessagePrefabHome; // �� ä�� �޽��� ������
    public ScrollRect chatScrollRect; // ��ũ�Ѻ�
    public ScrollRect chatScrollRectHome; // ��ũ�Ѻ�
    public GameObject chatUI;
    private CanvasGroup chatCanvasGroup;
    public GameObject chatButton;
    public GameObject chatHome;
    private CanvasGroup chatHomeCanvasGroup;
    public GameObject topSpacer;

    private Queue<string> messageQueue = new Queue<string>();
    private Coroutine chatHomeFadeCoroutine; // ä�� Ȩ Fade �ڷ�ƾ �����

    // Ǯ ����
    private Dictionary<string, Queue<GameObject>> messagePools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, List<GameObject>> activeMessages = new Dictionary<string, List<GameObject>>();

    // ������
    [SerializeField] private int maxActiveMessages = 50; // ȭ�鿡 ������ �ִ� �޽��� ��
    [SerializeField] private int maxPoolSize = 100;       // Ǯ�� ���� ������ �ִ� ����

    private class ChatTimeData
    {
        public TMP_Text timeText;
        public DateTime sentTime;
    }
    private List<ChatTimeData> activeTimeTexts = new List<ChatTimeData>();


    void Start()
    {
        chatInputField.characterLimit = 60; // �ִ� 60�� �Է� ����
        sendButton.onClick.AddListener(SendChatMessage);
        chatCanvasGroup = chatUI.GetComponent<CanvasGroup>();
        chatHomeCanvasGroup = chatHome.GetComponent<CanvasGroup>();
        chatInputField.onSubmit.AddListener(delegate { SendChatMessage(); });
        StartCoroutine(UpdateChatTimesLoop());
        PlayFabChatService.Instance.OnMessageReceived += AppendChatMessage;
    }

    void Update()
    {

        // �޽��� ť���� UI ������Ʈ ó��
        while (messageQueue.Count > 0)
        {
            string message = messageQueue.Dequeue();

            // �޽������� ���� ��� �̸� ����
            string myName = PlayFabAuthService1.Instance.DisplayName;
            string sender = ExtractSenderName(message);
            string body = ExtractMessageBody(message); 
            DateTime sentTime = ExtractSentTime(message);

            bool isMyMessage = sender == myName;

            // Ű ����
            string key = isMyMessage ? "my" : "other";
            string homeKey = isMyMessage ? "my_home" : "other_home";

            // �޽��� ����
            //GameObject newMessage = CreateChatMessage(key, isMyMessage ? myChatMessagePrefab : otherChatMessagePrefab, chatContent, message);
            GameObject newMessage = CreateChatMessage(key, otherChatMessagePrefab, chatContent, message);
            SetMessageTexts(newMessage, sender, body, sentTime, isMyMessage);
            newMessage.transform.SetAsLastSibling(); 

            // Ȩ �޽��� ����
            //GameObject newMessageHome = CreateChatMessage(homeKey, isMyMessage ? myChatMessagePrefabHome : otherChatMessagePrefabHome, chatContentHome, message);
            GameObject newMessageHome = CreateChatMessage(homeKey, otherChatMessagePrefabHome, chatContentHome, message);
            SetMessageTexts(newMessageHome, sender, body, sentTime, isMyMessage);
            newMessageHome.transform.SetAsLastSibling(); 

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
    private void SetMessageTime(GameObject obj, DateTime sentTime)
    {
        TMP_Text timeText = obj.transform.Find("Time")?.GetComponent<TMP_Text>();
        if (timeText != null)
        {
            timeText.text = FormatElapsedTime(sentTime);
            activeTimeTexts.Add(new ChatTimeData { timeText = timeText, sentTime = sentTime });
        }
    }

    private IEnumerator UpdateChatTimesLoop()
    {
        while (true)
        {
            foreach (var chat in activeTimeTexts)
            {
                if (chat.timeText != null)
                {
                    chat.timeText.text = FormatElapsedTime(chat.sentTime);
                }
            }
            yield return new WaitForSeconds(10f);
        }
    }

    private string FormatElapsedTime(DateTime sentTime)
    {
        var elapsed = DateTime.Now - sentTime;
        if (elapsed.TotalMinutes < 1) return "���";
        else if (elapsed.TotalMinutes < 60) return $"{(int)elapsed.TotalMinutes}�� ��";
        else if (elapsed.TotalHours < 24) return $"{(int)elapsed.TotalHours}�ð� ��";
        return "���� ��";
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

    // �޽��� ���� �Լ�
    private GameObject CreateChatMessage(string key, GameObject prefab, Transform parent, string message)
    {
        GameObject obj = GetPooledMessage(key, prefab, parent);

        // Ȱ�� �޽��� ����Ʈ�� ���
        if (!activeMessages.ContainsKey(key))
            activeMessages[key] = new List<GameObject>();

        activeMessages[key].Add(obj);

        // �ִ� �� �ʰ� �� ������ �޽��� ��Ȱ��
        if (activeMessages[key].Count > maxActiveMessages)
        {
            var oldest = activeMessages[key][0];
            activeMessages[key].RemoveAt(0);
            RecycleMessage(key, oldest);
        }

        return obj;
    }

    // Ǯ���� ������
    private GameObject GetPooledMessage(string key, GameObject prefab, Transform parent)
    {
        if (!messagePools.ContainsKey(key))
            messagePools[key] = new Queue<GameObject>();

        if (messagePools[key].Count > 0)
        {
            var obj = messagePools[key].Dequeue();
            obj.transform.SetParent(parent, false);
            obj.SetActive(true);
            return obj;
        }

        return Instantiate(prefab, parent, false);
    }

    // Ǯ�� �ǵ�����
    private void RecycleMessage(string key, GameObject obj)
    {
        obj.SetActive(false);

        if (!messagePools.ContainsKey(key))
            messagePools[key] = new Queue<GameObject>();

        if (messagePools[key].Count < maxPoolSize)
        {
            messagePools[key].Enqueue(obj);
        }
        else
        {
            Destroy(obj); // Ǯ �ʰ� �� �ı�
        }
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

        // �г��� ó��
        if (nicknameText != null)
        {/*
            if (isMyMessage)
            {
                nicknameText.gameObject.SetActive(false);
            }
            else
            {
                nicknameText.text = sender;
                nicknameText.gameObject.SetActive(true);
            }*/
            nicknameText.text = sender;
            nicknameText.gameObject.SetActive(true);
        }

        // �޽��� ����
        if (messageText != null)
        {
            messageText.text = body;
        }
        // �ð� ǥ�� + �ڵ� ���� ���
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
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string message = $"[ {PlayFabAuthService1.Instance.DisplayName} ] {chatInputField.text}";
            PlayFabChatService.Instance.SendChatMessage(message);
            chatInputField.text = "";
        }

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
