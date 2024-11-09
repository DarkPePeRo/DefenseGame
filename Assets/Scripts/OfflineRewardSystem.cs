using UnityEngine;

public class OfflineRewardSystem : MonoBehaviour
{
    public PlayerCurrency player; 
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveLastLogoutTime();
        }
    }

    private void OnApplicationQuit()
    {
        SaveLastLogoutTime();
    }

    private void SaveLastLogoutTime()
    {
        PlayerPrefs.SetString("LastLogoutTime", System.DateTime.Now.ToString());
        PlayerPrefs.Save();
    }
    private double GetElapsedOfflineTime()
    {
        if (PlayerPrefs.HasKey("LastLogoutTime"))
        {
            string lastLogoutTime = PlayerPrefs.GetString("LastLogoutTime");
            System.DateTime lastTime = System.DateTime.Parse(lastLogoutTime);
            System.TimeSpan timeDifference = System.DateTime.Now - lastTime;
            return timeDifference.TotalSeconds; // 경과 시간을 초 단위로 반환
        }
        return 0;
    }
    public int rewardPerSecond = 10; // 초당 10 코인 지급 예시

    public void GiveOfflineReward()
    {
        double offlineSeconds = GetElapsedOfflineTime();
        if (offlineSeconds > 0)
        {
            int reward = Mathf.FloorToInt((float)(offlineSeconds * rewardPerSecond));
            AddCurrency(reward);
            Debug.Log($"오프라인 보상 지급: {reward} 코인");
        }
    }

    private void AddCurrency(int amount)
    {
        player.AddCurrency(player.gold, amount);
    }

}
