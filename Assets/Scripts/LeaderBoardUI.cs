using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderBoardUI : MonoBehaviour
{
    public PlayFabLeaderBoard playFabLeaderBoard;
    public TextMeshProUGUI[] leaderboardRank = new TextMeshProUGUI[10];
    public TextMeshProUGUI[] leaderboardName = new TextMeshProUGUI[10];
    public TextMeshProUGUI[] leaderboardScore = new TextMeshProUGUI[10];
    public void Start()
    {
        playFabLeaderBoard.GetLeaderboard();
    }
    public void ShowLeaderboard(List<PlayerLeaderboardEntry> leaderboardEntries)
    {
        // 텍스트 초기화
        for (int i = 0; i < leaderboardName.Length; i++)
        {
            if (i < leaderboardEntries.Count)
            {
                var entry = leaderboardEntries[i];
                leaderboardName[i].text = $"Player: {entry.DisplayName ?? "Guest"}";
                leaderboardScore[i].text = $"Score: {entry.StatValue}";
            }
            else
            {
                leaderboardName[i].text = ""; // 빈 텍스트로 설정하여 표시되지 않도록 처리
                leaderboardScore[i].text = ""; // 빈 텍스트로 설정하여 표시되지 않도록 처리
            }
        }
    }
}
