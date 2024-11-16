using Photon.Chat;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class ChatManager : MonoBehaviour, IChatClientListener
{
    public TMP_InputField chatInputField;
    public TextMeshProUGUI chatDisplayText;
    private ChatClient chatClient;
    private string userName;

    private void Start()
    {
        // userName�� Start �޼��忡�� �ʱ�ȭ
        userName = "User_" + Random.Range(1000, 9999);

        // ChatClient �ν��Ͻ� ���� �� ����
        chatClient = new ChatClient(this);
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.AppVersion, new Photon.Chat.AuthenticationValues(userName));
    }

    private void Update()
    {
        chatClient?.Service();
    }

    public void SendMessageToChat()
    {
        string message = chatInputField.text;
        if (!string.IsNullOrEmpty(message))
        {
            chatClient.PublishMessage("GlobalChannel", message);
            chatInputField.text = "";
        }
    }

    public void OnConnected()
    {
        Debug.Log("Chat ������ ����Ǿ����ϴ�.");
        chatClient.Subscribe(new string[] { "GlobalChannel" });
    }

    public void OnDisconnected()
    {
        Debug.Log("Chat �������� ������ ������ϴ�.");
    }

    public void OnChatStateChange(ChatState state) { }
    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < messages.Length; i++)
        {
            chatDisplayText.text += $"\n{senders[i]}: {messages[i]}";
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName) { }
    public void OnSubscribed(string[] channels, bool[] results)
    {
        Debug.Log("ä�ο� �����Ǿ����ϴ�.");
    }

    public void OnUnsubscribed(string[] channels) { }
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message) { }
    public void OnUserSubscribed(string channel, string user) { }
    public void OnUserUnsubscribed(string channel, string user) { }

    public void DebugReturn(ExitGames.Client.Photon.DebugLevel level, string message)
    {
        Debug.Log($"[Photon Chat Debug] Level: {level}, Message: {message}");
    }
}
