using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlayFabGodStatService
{
    private const string StatKey = "GodStatLevels";

    public static GodStatSaveData CurrentStats { get; private set; } = new GodStatSaveData();

    public static void Load(Action onComplete = null)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            if (result.Data != null && result.Data.TryGetValue(StatKey, out var record))
            {
                CurrentStats = JsonUtility.FromJson<GodStatSaveData>(record.Value);
                Debug.Log("[PlayFabGodStatService] ���� �ҷ����� �Ϸ�");
            }
            else
            {
                Debug.Log("[PlayFabGodStatService] ���� ���� ����, �ʱ�ȭ");
                CurrentStats = new GodStatSaveData();
            }

            onComplete?.Invoke();
        },
        error =>
        {
            Debug.LogError("[PlayFabGodStatService] ���� �ε� ����: " + error.GenerateErrorReport());
            onComplete?.Invoke();
        });
    }

    public static void Save(GodStatSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { StatKey, json } }
        },
        result => Debug.Log("[PlayFabGodStatService] ���� ���� �Ϸ�"),
        error => Debug.LogError("[PlayFabGodStatService] ���� ���� ����: " + error.GenerateErrorReport()));
    }
}

[Serializable]
public class GodStatSaveData
{
    public int attackPower;
    public int attackSpeed;
    public int criticalRate;
    public int criticalDamage;
}
