using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EquipmentUpgradeEntry
{
    public string itemId;
    public int level;
}

[Serializable]
public class EquipmentUpgradeSaveData
{
    public List<EquipmentUpgradeEntry> entries = new();
}

public static class PlayFabEquipmentUpgradeService
{
    private const string UpgradeKey = "EquipmentUpgradeLevels";

    public static void Load(Action<Dictionary<string, int>> onLoaded)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            var dict = new Dictionary<string, int>();

            if (result.Data != null && result.Data.TryGetValue(UpgradeKey, out var value))
            {
                try
                {
                    var saveData = JsonUtility.FromJson<EquipmentUpgradeSaveData>(value.Value);
                    if (saveData != null && saveData.entries != null)
                    {
                        foreach (var entry in saveData.entries)
                        {
                            if (entry == null || string.IsNullOrEmpty(entry.itemId)) continue;
                            dict[entry.itemId] = entry.level;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("[EquipmentUpgrade] Load parse error: " + e.Message);
                }
            }

            onLoaded?.Invoke(dict);
        },
        error =>
        {
            Debug.LogError("[EquipmentUpgrade] Load failed: " + error.GenerateErrorReport());
            onLoaded?.Invoke(new Dictionary<string, int>());
        });
    }

    public static void Save(Dictionary<string, int> levels, Action onSuccess = null, Action<string> onFail = null)
    {
        var saveData = new EquipmentUpgradeSaveData();

        foreach (var kv in levels)
        {
            saveData.entries.Add(new EquipmentUpgradeEntry
            {
                itemId = kv.Key,
                level = kv.Value
            });
        }

        var data = new Dictionary<string, string>
        {
            { UpgradeKey, JsonUtility.ToJson(saveData) }
        };

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
        {
            Data = data
        },
        result =>
        {
            onSuccess?.Invoke();
        },
        error =>
        {
            var msg = error.GenerateErrorReport();
            Debug.LogError("[EquipmentUpgrade] Save failed: " + msg);
            onFail?.Invoke(msg);
        });
    }
}