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
            result => Debug.Log("스테이지 저장 성공"),
            error => Debug.LogError("스테이지 저장 실패: " + error.GenerateErrorReport()));
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
            Debug.LogError("스테이지 불러오기 실패: " + error.GenerateErrorReport());
            onLoaded?.Invoke(new List<int>());
        });
    }
}
