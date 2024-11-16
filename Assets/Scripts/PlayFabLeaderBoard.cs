using PlayFab.ClientModels;
using PlayFab;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using System.Collections;

public class PlayFabLeaderBoard : MonoBehaviour
{
    private List<PlayerLeaderboardEntry> cachedLeaderboard = null;
    private float cacheExpiryTime = 0f;
    private float cacheDuration = 60f;

    public LeaderBoardUI leaderBoardUI;
    [System.Serializable]
    private class LeaderboardResponse
    {
        public List<PlayerLeaderboardEntry> leaderboard;
    }

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
        Debug.Log("���� ���� ����!");
    }

    private void OnScoreUpdateFailure(PlayFabError error)
    {
        Debug.LogError("���� ���� ����: " + error.GenerateErrorReport());
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
    public void GetUpdatedLeaderboard()
    {
        if (cachedLeaderboard != null && Time.time < cacheExpiryTime)
        {
            Debug.Log("Using cached leaderboard data.");
            leaderBoardUI.ShowLeaderboard(cachedLeaderboard);
        }
        else
        {
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
            {
                FunctionName = "UpdateLeaderboard",
                GeneratePlayStreamEvent = false
            },
            result =>
            {
                var leaderboardResponse = JsonUtility.FromJson<LeaderboardResponse>(result.FunctionResult.ToString());
                cachedLeaderboard = leaderboardResponse.leaderboard;
                cacheExpiryTime = Time.time + cacheDuration;
                leaderBoardUI.ShowLeaderboard(cachedLeaderboard);
            },
            error =>
            {
                Debug.LogError("Failed to execute CloudScript: " + error.GenerateErrorReport());
            });
        }
    }
    private void OnLeaderboardGet(GetLeaderboardResult result)
    {
        // �������� �����͸� UI�� ����
        leaderBoardUI.ShowLeaderboard(result.Leaderboard);
    }

    private void OnLeaderboardGetError(PlayFabError error)
    {
        Debug.LogError("�������� �������� ����: " + error.GenerateErrorReport());
    }
    private IEnumerator UpdateRanking()
    {
        while (true)
        {
            GetLeaderboard(); // �������� ����
            yield return new WaitForSeconds(600); // 10�� (600��) ���
        }

    }
}
