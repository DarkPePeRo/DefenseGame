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
                Debug.Log("[PlayFabGodStatService] 스탯 불러오기 완료");
            }
            else
            {
                Debug.Log("[PlayFabGodStatService] 기존 스탯 없음, 초기화");
                CurrentStats = new GodStatSaveData();
            }

            onComplete?.Invoke();
        },
        error =>
        {
            Debug.LogError("[PlayFabGodStatService] 스탯 로드 실패: " + error.GenerateErrorReport());
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
        result => Debug.Log("[PlayFabGodStatService] 스탯 저장 완료"),
        error => Debug.LogError("[PlayFabGodStatService] 스탯 저장 실패: " + error.GenerateErrorReport()));
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
