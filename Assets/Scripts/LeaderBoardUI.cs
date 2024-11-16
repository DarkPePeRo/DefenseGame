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
        // �ؽ�Ʈ �ʱ�ȭ
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
                leaderboardName[i].text = ""; // �� �ؽ�Ʈ�� �����Ͽ� ǥ�õ��� �ʵ��� ó��
                leaderboardScore[i].text = ""; // �� �ؽ�Ʈ�� �����Ͽ� ǥ�õ��� �ʵ��� ó��
            }
        }
    }
}
