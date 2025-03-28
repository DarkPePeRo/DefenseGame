// PlayFabStatsService.cs
using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public static class PlayFabStatsService
{
    public static int AttackPowerLevel { get; private set; } = 1;
    public static int AttackSpeedLevel { get; private set; } = 1;
    public static int CriticalRateLevel { get; private set; } = 1;
    public static int CriticalDamageLevel { get; private set; } = 1;

    public static void Load(Action onComplete = null)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            if (result.Data != null)
            {
                if (result.Data.TryGetValue("attackPowerLevel", out var val))
                    AttackPowerLevel = int.Parse(val.Value);
                if (result.Data.TryGetValue("attackSpeedLevel", out val))
                    AttackSpeedLevel = int.Parse(val.Value);
                if (result.Data.TryGetValue("criticalRateLevel", out val))
                    CriticalRateLevel = int.Parse(val.Value);
                if (result.Data.TryGetValue("criticalDamageLevel", out val))
                    CriticalDamageLevel = int.Parse(val.Value);

                Debug.Log("[PlayFabStatsService] 스탯 불러오기 완료");
            }

            onComplete?.Invoke();

        }, error =>
        {
            Debug.LogError("[PlayFabStatsService] 스탯 불러오기 실패: " + error.ErrorMessage);
            onComplete?.Invoke();
        });
    }

    public static void Save()
    {
        var data = new Dictionary<string, string>
        {
            { "attackPowerLevel", AttackPowerLevel.ToString() },
            { "attackSpeedLevel", AttackSpeedLevel.ToString() },
            { "criticalRateLevel", CriticalRateLevel.ToString() },
            { "criticalDamageLevel", CriticalDamageLevel.ToString() }
        };

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest { Data = data },
            result => Debug.Log("[PlayFabStatsService] 스탯 저장 완료"),
            error => Debug.LogError("[PlayFabStatsService] 스탯 저장 실패: " + error.ErrorMessage));
    }

    public static void SetStat(string type, int level)
    {
        switch (type)
        {
            case "attackPower": AttackPowerLevel = level; break;
            case "attackSpeed": AttackSpeedLevel = level; break;
            case "criticalRate": CriticalRateLevel = level; break;
            case "criticalDamage": CriticalDamageLevel = level; break;
        }
    }
}
