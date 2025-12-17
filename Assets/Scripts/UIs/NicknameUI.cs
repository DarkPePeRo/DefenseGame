using UnityEngine;
using TMPro;

public class NicknameUI : MonoBehaviour
{
    public NicknameManager nicknameManager;
    public TMP_InputField nicknameInputField;

    public void OnConfirmNicknameButtonPressed()
    {
        string newNickname = nicknameInputField.text;
        nicknameManager.SetNickname(newNickname);
    }
}
