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
            return timeDifference.TotalSeconds; // ��� �ð��� �� ������ ��ȯ
        }
        return 0;
    }
    public int rewardPerSecond = 10; // �ʴ� 10 ���� ���� ����

    public void GiveOfflineReward()
    {
        double offlineSeconds = GetElapsedOfflineTime();
        if (offlineSeconds > 0)
        {
            int reward = Mathf.FloorToInt((float)(offlineSeconds * rewardPerSecond));
            AddCurrency(reward);
            Debug.Log($"�������� ���� ����: {reward} ����");
        }
    }

    private void AddCurrency(int amount)
    {
        player.AddCurrency(player.gold, amount);
    }

}
