using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StageData { public int[] clearedStages; }
public static class PlayFabStageService
{
    private const string StageKey = "ClearedStages";
    private const string HighestKey = "HighestStage";

    public static int HighestClearedStage { get; private set; } = 0;

    public static void Save(List<int> clearedStages)
    {
        int highest = clearedStages.Count > 0 ? Mathf.Max(clearedStages.ToArray()) : 1;
        HighestClearedStage = highest;

        var data = new Dictionary<string, string>
        {
            { StageKey, JsonUtility.ToJson(new StageData { clearedStages = clearedStages.ToArray() }) },
            { HighestKey, highest.ToString() }
        };

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest { Data = data },
            result => Debug.Log("스테이지 저장 성공"),
            error => Debug.LogError("스테이지 저장 실패: " + error.GenerateErrorReport()));
    }

    public static void Load(Action<List<int>, int> onLoaded)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            List<int> cleared = new();
            int highest = 0;

            if (result.Data != null)
            {
                if (result.Data.ContainsKey(StageKey))
                {
                    var json = result.Data[StageKey].Value;
                    var data = JsonUtility.FromJson<StageData>(json);
                    cleared = new List<int>(data.clearedStages);
                }

                if (result.Data.ContainsKey(HighestKey))
                {
                    int.TryParse(result.Data[HighestKey].Value, out highest);
                }
            }

            HighestClearedStage = highest;
            onLoaded?.Invoke(cleared, highest);
        },
        error =>
        {
            Debug.LogError("스테이지 불러오기 실패: " + error.GenerateErrorReport());
            HighestClearedStage = 0;
            onLoaded?.Invoke(new List<int>(), 0);
        });
    }
    public static void RequestStageClear(int wave)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "OnStageClear", // CloudScript 함수 이름
            FunctionParameter = new { wave = wave },
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request,
            result => Debug.Log($"[StageClear] {wave}단계 완료"),
            error => Debug.LogError("[StageClear] 실패: " + error.GenerateErrorReport()));
    }
}


