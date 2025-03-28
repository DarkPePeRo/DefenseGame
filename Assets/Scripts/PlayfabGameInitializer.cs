using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayFabGameInitializer : MonoBehaviour
{
    private void Start()
    {
        // �α��� �Ϸ� �� �ε� ���� ����
        PlayFabAuthService1.Instance.OnLoginSuccess += StartDataLoading;
    }

    private void StartDataLoading()
    {
        Debug.Log("�α��� ���� �� ������ �ε� ����");

        // ������ ����
        PlayFabChatService.Instance.Connect(PlayFabAuthService1.Instance.PlayFabId);

        // ��ȭ �ε� �� �������� �ε� �� ���� �ε� �� �� ��ȯ
        PlayFabCurrencyService.Load(() =>
        {
            PlayFabStageService.Load(stageData =>
            {
                PlayFabStatsService.Load(() =>
                {
                    Debug.Log("��� ������ �ε� �Ϸ� �� MainScene �ε�");
                    SceneManager.LoadScene("SampleScene");
                });
            });
        });
    }
}
