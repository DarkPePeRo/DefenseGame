using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlayFabGodStatService
{
    private const string StatKey = "GodStatLevels";

    public static GodStatSaveData CurrentStats { get; private set; } = GodStatSaveData.CreateDefault();

    // 저장 경합 방지
    private static bool isSaving = false;
    private static bool saveQueued = false;
    private static GodStatSaveData queuedData = null;

    public static void Load(Action<GodStatSaveData, bool> onComplete = null)
    {
        var req = new GetUserDataRequest
        {
            Keys = new List<string> { StatKey }
        };

        PlayFabClientAPI.GetUserData(req, result =>
        {
            bool createdNew = false;

            if (result.Data != null &&
                result.Data.TryGetValue(StatKey, out var record) &&
                !string.IsNullOrEmpty(record.Value))
            {
                try
                {
                    var loaded = JsonUtility.FromJson<GodStatSaveData>(record.Value);
                    CurrentStats = GodStatSaveData.SanitizeOrDefault(loaded);
                    Debug.Log("[PlayFabGodStatService] 스탯 불러오기 완료");
                }
                catch (Exception e)
                {
                    Debug.LogError("[PlayFabGodStatService] 스탯 JSON 파싱 실패 -> 기본값. err=" + e.Message);
                    CurrentStats = GodStatSaveData.CreateDefault();
                    createdNew = true;
                }
            }
            else
            {
                Debug.Log("[PlayFabGodStatService] 기존 스탯 없음 -> 기본값 생성");
                CurrentStats = GodStatSaveData.CreateDefault();
                createdNew = true;
            }

            onComplete?.Invoke(CurrentStats, createdNew);
        },
        error =>
        {
            Debug.LogError("[PlayFabGodStatService] 스탯 로드 실패: " + error.GenerateErrorReport());

            CurrentStats = GodStatSaveData.CreateDefault();
            onComplete?.Invoke(CurrentStats, false);
        });
    }

    public static void Save(GodStatSaveData data)
    {
        if (data == null) return;

        var sanitized = GodStatSaveData.SanitizeOrDefault(data);

        if (isSaving)
        {
            saveQueued = true;
            queuedData = sanitized;
            return;
        }

        isSaving = true;

        string json = JsonUtility.ToJson(sanitized);

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { StatKey, json } }
        },
        result =>
        {
            Debug.Log("[PlayFabGodStatService] 스탯 저장 완료");
            isSaving = false;

            if (saveQueued && queuedData != null)
            {
                saveQueued = false;
                var last = queuedData;
                queuedData = null;
                Save(last);
            }
        },
        error =>
        {
            Debug.LogError("[PlayFabGodStatService] 스탯 저장 실패: " + error.GenerateErrorReport());
            isSaving = false;

            saveQueued = false;
            queuedData = null;
        });
    }
}

[Serializable]
public class GodStatSaveData
{
    public int attackPower = 1;
    public int attackSpeed = 1;
    public int criticalRate = 1;
    public int criticalDamage = 1;

    public static GodStatSaveData CreateDefault()
    {
        return new GodStatSaveData
        {
            attackPower = 1,
            attackSpeed = 1,
            criticalRate = 1,
            criticalDamage = 1
        };
    }

    public static GodStatSaveData SanitizeOrDefault(GodStatSaveData data)
    {
        if (data == null) return CreateDefault();

        if (data.attackPower < 1) data.attackPower = 1;
        if (data.attackSpeed < 1) data.attackSpeed = 1;
        if (data.criticalRate < 1) data.criticalRate = 1;
        if (data.criticalDamage < 1) data.criticalDamage = 1;

        return data;
    }
}
