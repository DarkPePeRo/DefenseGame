using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.Json;

[Serializable]
public class StageData { public int[] clearedStages; }

[Serializable]
public class StageClearResult
{
    public bool success;
    public string reason;
    public int newHighestStage;
}

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

    public static void RequestStageClear(int wave, Action<StageClearResult> onSuccess, Action<string> onFail)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "OnStageClear",
            FunctionParameter = new { wave = wave },
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(
            request,
            result =>
            {
                if (result.Error != null)
                {
                    string errorMsg = "[StageClear] CloudScript Error: " + result.Error.Message;
                    Debug.LogError(errorMsg);
                    onFail?.Invoke(errorMsg);
                    return;
                }

                if (result.FunctionResult == null)
                {
                    string errorMsg = "[StageClear] FunctionResult == null";
                    Debug.LogError(errorMsg);
                    onFail?.Invoke(errorMsg);
                    return;
                }
                string stageJson = PlayFabSimpleJson.SerializeObject(result.FunctionResult);
                StageClearResult data = PlayFabSimpleJson.DeserializeObject<StageClearResult>(stageJson);

                if (data == null)
                {
                    string errorMsg = "[StageClear] FunctionResult 파싱 실패";
                    Debug.LogError(errorMsg);
                    onFail?.Invoke(errorMsg);
                    return;
                }

                if (!data.success)
                {
                    string failReason = string.IsNullOrEmpty(data.reason)
                        ? "[StageClear] 서버에서 클리어 거부"
                        : "[StageClear] " + data.reason;

                    Debug.LogError(failReason);
                    onFail?.Invoke(failReason);
                    return;
                }

                Debug.Log($"[StageClear] {wave}단계 클리어 서버 저장 완료");
                onSuccess?.Invoke(data);
            },
            error =>
            {
                string errorMsg = "[StageClear] 실패: " + error.GenerateErrorReport();
                Debug.LogError(errorMsg);
                onFail?.Invoke(errorMsg);
            });
    }

}


