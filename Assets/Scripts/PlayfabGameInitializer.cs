using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayFabGameInitializer : MonoBehaviour
{
    private void Start()
    {
        // 로그인 완료 시 로딩 시작 연결
        PlayFabAuthService1.Instance.OnLoginSuccess += StartDataLoading;
    }

    private void StartDataLoading()
    {
        Debug.Log("로그인 성공 → 데이터 로딩 시작");

        // 웹소켓 연결
        PlayFabChatService.Instance.Connect(PlayFabAuthService1.Instance.PlayFabId);

        // 재화 로드 → 스테이지 로드 → 스탯 로드 → 씬 전환
        PlayFabCurrencyService.Load(() =>
        {
            PlayFabStageService.Load(stageData =>
            {
                PlayFabStatsService.Load(() =>
                {
                    Debug.Log("모든 데이터 로드 완료 → MainScene 로드");
                    SceneManager.LoadScene("SampleScene");
                });
            });
        });
    }
}
