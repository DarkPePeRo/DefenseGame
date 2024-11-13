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
        // �������� �ؽ�Ʈ �ʱ�ȭ
        leaderboardText.text = "Leaderboard\n\n";

        // �������� ������ ��ȸ�Ͽ� �ؽ�Ʈ�� �߰�
        foreach (var entry in leaderboardEntries)
        {
            leaderboardText.text += $"Rank: {entry.Position + 1} | Player: {entry.DisplayName ?? "Guest"} | Score: {entry.StatValue}\n";
        }
    }
}
