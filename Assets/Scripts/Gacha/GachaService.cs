using Best.HTTP.JSON.LitJson;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using System;

public static class GachaService
{
    public static void GetGachaInitData(
        Action<GetGachaInitDataResponse> onSuccess,
        Action<string> onFail)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "GetGachaInitData",
            FunctionParameter = new { },
            GeneratePlayStreamEvent = false
        };

        PlayFabClientAPI.ExecuteCloudScript(request, result =>
        {
            if (result.Error != null)
            {
                onFail?.Invoke(result.Error.Message);
                return;
            }

            if (result.FunctionResult == null)
            {
                onFail?.Invoke("GetGachaInitData: FunctionResult is null");
                return;
            }

            try
            {
                string gachaJson = PlayFabSimpleJson.SerializeObject(result.FunctionResult);
                GetGachaInitDataResponse response = PlayFabSimpleJson.DeserializeObject<GetGachaInitDataResponse>(gachaJson);
                onSuccess?.Invoke(response);
            }
            catch (Exception e)
            {
                onFail?.Invoke("GetGachaInitData parse error: " + e.Message);
            }

        }, error =>
        {
            onFail?.Invoke(error.GenerateErrorReport());
        });
    }

    public static void ExecutePull(
        string bannerId,
        int pullCount,
        string summonLevelGroup,
        string ticketItemId,
        Action<ExecuteGachaPullResponse> onSuccess,
        Action<string> onFail)
    {
        var args = new ExecuteGachaPullRequest
        {
            bannerId = bannerId,
            pullCount = pullCount,
            requestId = Guid.NewGuid().ToString("N"),
            summonLevelGroup = summonLevelGroup,
            ticketItemId = ticketItemId
        };

        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "ExecuteGachaPull",
            FunctionParameter = args,
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request, result =>
        {
            if (result.Error != null)
            {
                onFail?.Invoke(result.Error.Message);
                return;
            }

            if (result.FunctionResult == null)
            {
                onFail?.Invoke("ExecuteGachaPull: FunctionResult is null");
                return;
            }

            try
            {
                string json = PlayFabSimpleJson.SerializeObject(result.FunctionResult);
                ExecuteGachaPullResponse response =
                    PlayFabSimpleJson.DeserializeObject<ExecuteGachaPullResponse>(json);

                if (response == null)
                {
                    onFail?.Invoke("ExecuteGachaPull: response is null");
                    return;
                }

                onSuccess?.Invoke(response);
            }
            catch (Exception e)
            {
                onFail?.Invoke("ExecuteGachaPull parse error: " + e.Message);
            }

        }, error =>
        {
            onFail?.Invoke(error.GenerateErrorReport());
        });
    }
}