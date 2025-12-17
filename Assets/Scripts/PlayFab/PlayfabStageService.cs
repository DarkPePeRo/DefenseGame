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
            bool shouldSaveDefault = false;

            if (result.Data != null)
            {
                if (result.Data.ContainsKey(StageKey))
                {
                    var json = result.Data[StageKey].Value;
                    var data = JsonUtility.FromJson<StageData>(json);
                    cleared = new List<int>(data.clearedStages);
                }
                else
                {
                    shouldSaveDefault = true;
                }

                if (result.Data.ContainsKey(HighestKey))
                {
                    int.TryParse(result.Data[HighestKey].Value, out highest);
                }
                else
                {
                    shouldSaveDefault = true;
                }
            }
            else
            {
                shouldSaveDefault = true;
            }

            if (shouldSaveDefault)
            {
                cleared = new List<int>(); // 아무 것도 안 깼다면 빈 리스트
                highest = 0;
                Save(cleared); // 서버에 기본값 저장
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
            FunctionName = "OnStageClear",
            FunctionParameter = new { wave = wave },
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request,
            result =>
            {
                Debug.Log($"[StageClear] {wave}단계 클리어 서버 저장 완료");
            },
            error =>
            {
                Debug.LogError("[StageClear] 실패: " + error.GenerateErrorReport());
            });
    }

}


