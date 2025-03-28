using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StageData { public int[] clearedStages; }

public class PlayFabStageService
{
    private const string StageKey = "ClearedStages";

    public static void Save(List<int> clearedStages)
    {
        var data = new Dictionary<string, string>
        {
            { StageKey, JsonUtility.ToJson(new StageData { clearedStages = clearedStages.ToArray() }) }
        };

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest { Data = data },
            result => Debug.Log("�������� ���� ����"),
            error => Debug.LogError("�������� ���� ����: " + error.GenerateErrorReport()));
    }

    public static void Load(Action<List<int>> onLoaded)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result => {
            if (result.Data != null && result.Data.ContainsKey(StageKey))
            {
                var json = result.Data[StageKey].Value;
                var data = JsonUtility.FromJson<StageData>(json);
                onLoaded?.Invoke(new List<int>(data.clearedStages));
            }
            else
            {
                onLoaded?.Invoke(new List<int>());
            }
        },
        error => {
            Debug.LogError("�������� �ҷ����� ����: " + error.GenerateErrorReport());
            onLoaded?.Invoke(new List<int>());
        });
    }
}
