using PlayFab.ClientModels;
using PlayFab;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using System.Collections;

public class PlayFabLeaderBoard : MonoBehaviour
{
    public LeaderBoardUI leaderBoardUI;

    private void Start()
    {
        StartCoroutine(UpdateRanking());
    }
    public void SendScore(int score)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate { StatisticName = "PlayerScore", Value = score }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnScoreUpdateSuccess, OnScoreUpdateFailure);
    }

    private void OnScoreUpdateSuccess(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("점수 전송 성공!");
    }

    private void OnScoreUpdateFailure(PlayFabError error)
    {
        Debug.LogError("점수 전송 실패: " + error.GenerateErrorReport());
    }
    public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = "PlayerScore",
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnLeaderboardGetError);
    }
    private void OnLeaderboardGet(GetLeaderboardResult result)
    {
        // 리더보드 데이터를 UI에 전달
        leaderBoardUI.ShowLeaderboard(result.Leaderboard);
    }

    private void OnLeaderboardGetError(PlayFabError error)
    {
        Debug.LogError("리더보드 가져오기 실패: " + error.GenerateErrorReport());
    }
    private IEnumerator UpdateRanking()
    {
        while (true)
        {
            GetLeaderboard(); // 리더보드 갱신
            yield return new WaitForSeconds(600); // 10분 (600초) 대기
        }

    }
}
