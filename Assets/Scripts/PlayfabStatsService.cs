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
        PlayFabClientAPI.GetUserReadOnlyData(new GetUserDataRequest(), result =>
        {
            if (result.Data != null)
            {
                AttackPowerLevel = TryParseOrDefault(result.Data, "attackPower_Level", 1);
                AttackSpeedLevel = TryParseOrDefault(result.Data, "attackSpeed_Level", 1);
                CriticalRateLevel = TryParseOrDefault(result.Data, "criticalRate_Level", 1);
                CriticalDamageLevel = TryParseOrDefault(result.Data, "criticalDamage_Level", 1);

                Debug.Log("[PlayFabStatsService] ReadOnly ���� �ҷ����� �Ϸ�");
            }

            onComplete?.Invoke();
        },
        error =>
        {
            Debug.LogError("[PlayFabStatsService] ReadOnly ���� �ҷ����� ����: " + error.GenerateErrorReport());
            onComplete?.Invoke();
        });
    }

    private static int TryParseOrDefault(Dictionary<string, UserDataRecord> data, string key, int defaultValue)
    {
        return data.TryGetValue(key, out var val) && int.TryParse(val.Value, out var result)
            ? result
            : defaultValue;
    }
}
