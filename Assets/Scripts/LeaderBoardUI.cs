using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderBoardUI : MonoBehaviour
{
    public PlayFabLeaderBoard playFabLeaderBoard;
    public TextMeshProUGUI leaderboardText;
    public void Start()
    {
        playFabLeaderBoard.GetLeaderboard();
    }

    public void ShowLeaderboard(List<PlayerLeaderboardEntry> leaderboardEntries)
    {
        // 리더보드 텍스트 초기화
        leaderboardText.text = "Leaderboard\n\n";

        // 리더보드 데이터 순회하여 텍스트에 추가
        foreach (var entry in leaderboardEntries)
        {
            leaderboardText.text += $"Rank: {entry.Position + 1} | Player: {entry.DisplayName ?? "Guest"} | Score: {entry.StatValue}\n";
        }
    }
}
