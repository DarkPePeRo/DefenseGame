using PlayFab;
using PlayFab.ClientModels;
using System.Text.RegularExpressions;
using UnityEngine;

public class NicknameManager : MonoBehaviour
{
    public GameObject nicknameInputUI; // 닉네임 입력 UI
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
        // DisplayName이 비어 있는지 확인
        if (string.IsNullOrEmpty(result.AccountInfo.TitleInfo.DisplayName))
        {
            // 닉네임이 설정되지 않았으면 입력 UI를 보여줌
            nicknameInputUI.SetActive(true);
        }
        else
        {
            Debug.Log("닉네임이 이미 설정되었습니다: " + result.AccountInfo.TitleInfo.DisplayName);
            nicknameInputUI.SetActive(false); // 닉네임이 이미 설정되어 있으면 UI를 숨김
            defaultUI.SetActive(true);
        }
    }

    private void OnGetAccountInfoFailure(PlayFabError error)
    {
        Debug.LogError("계정 정보를 가져오는 데 실패했습니다: " + error.GenerateErrorReport());
    }

    public void SetNickname(string newNickname)
    {
        if (!IsNicknameValid(newNickname))
        {
            Debug.LogError("닉네임이 유효하지 않습니다. 조건을 확인하세요.");
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
        // 길이 확인 (3~12자)
        if (nickname.Length < 2 || nickname.Length > 12)
        {
            Debug.Log("닉네임은 2자 이상 12자 이하이어야 합니다.");
            return false;
        }

        // 공백 또는 특수문자 확인
        if (!Regex.IsMatch(nickname, "^[a-zA-Z0-9]+$"))
        {
            Debug.Log("닉네임은 공백이나 특수문자를 포함할 수 없습니다.");
            return false;
        }

        return true;
    }

    private void OnNicknameSetSuccess(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("닉네임 설정 성공: " + result.DisplayName);
        nicknameInputUI.SetActive(false); // 설정 후 닉네임 입력 UI를 숨김
        defaultUI.SetActive(true);
    }

    private void OnNicknameSetFailure(PlayFabError error)
    {
        Debug.LogError("닉네임 설정 실패: " + error.GenerateErrorReport());
    }
}
