using PlayFab;
using PlayFab.ClientModels;
using System.Text.RegularExpressions;
using UnityEngine;

public class NicknameManager : MonoBehaviour
{
    public GameObject nicknameInputUI; // �г��� �Է� UI
    public GameObject defaultUI;

    private void Start()
    {
        CheckNickname();
    }

    private void CheckNickname()
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), OnGetAccountInfoSuccess, OnGetAccountInfoFailure);
    }

    private void OnGetAccountInfoSuccess(GetAccountInfoResult result)
    {
        // DisplayName�� ��� �ִ��� Ȯ��
        if (string.IsNullOrEmpty(result.AccountInfo.TitleInfo.DisplayName))
        {
            // �г����� �������� �ʾ����� �Է� UI�� ������
            nicknameInputUI.SetActive(true);
        }
        else
        {
            Debug.Log("�г����� �̹� �����Ǿ����ϴ�: " + result.AccountInfo.TitleInfo.DisplayName);
            nicknameInputUI.SetActive(false); // �г����� �̹� �����Ǿ� ������ UI�� ����
            defaultUI.SetActive(true);
        }
    }

    private void OnGetAccountInfoFailure(PlayFabError error)
    {
        Debug.LogError("���� ������ �������� �� �����߽��ϴ�: " + error.GenerateErrorReport());
    }

    public void SetNickname(string newNickname)
    {
        if (!IsNicknameValid(newNickname))
        {
            Debug.LogError("�г����� ��ȿ���� �ʽ��ϴ�. ������ Ȯ���ϼ���.");
            return;
        }

        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = newNickname
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnNicknameSetSuccess, OnNicknameSetFailure);
    }
    private bool IsNicknameValid(string nickname)
    {
        // ���� Ȯ�� (3~12��)
        if (nickname.Length < 2 || nickname.Length > 12)
        {
            Debug.Log("�г����� 2�� �̻� 12�� �����̾�� �մϴ�.");
            return false;
        }

        // ���� �Ǵ� Ư������ Ȯ��
        if (!Regex.IsMatch(nickname, "^[a-zA-Z0-9]+$"))
        {
            Debug.Log("�г����� �����̳� Ư�����ڸ� ������ �� �����ϴ�.");
            return false;
        }

        return true;
    }

    private void OnNicknameSetSuccess(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("�г��� ���� ����: " + result.DisplayName);
        nicknameInputUI.SetActive(false); // ���� �� �г��� �Է� UI�� ����
        defaultUI.SetActive(true);
    }

    private void OnNicknameSetFailure(PlayFabError error)
    {
        Debug.LogError("�г��� ���� ����: " + error.GenerateErrorReport());
    }
}
