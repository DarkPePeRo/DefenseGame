using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabCurrencyService
{
    public static void Load(Action onComplete)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result => {
            bool isMissing = false;
            var data = result.Data;

            if (data != null && data.ContainsKey("gold"))
                PlayerCurrency.Instance.gold.amount = int.Parse(data["gold"].Value);
            else { PlayerCurrency.Instance.gold.amount = 0; isMissing = true; }

            if (data != null && data.ContainsKey("diamond"))
                PlayerCurrency.Instance.diamond.amount = int.Parse(data["diamond"].Value);
            else { PlayerCurrency.Instance.diamond.amount = 0; isMissing = true; }

            if (isMissing) Save(PlayerCurrency.Instance.gold.amount, PlayerCurrency.Instance.diamond.amount);
            onComplete?.Invoke();
        },
        error => Debug.LogError("Currency 로드 실패: " + error.GenerateErrorReport()));
    }

    public static void Save(int gold, int diamond)
    {
        var data = new Dictionary<string, string> {
            { "gold", gold.ToString() },
            { "diamond", diamond.ToString() }
        };
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest { Data = data },
            result => Debug.Log("Currency 저장 성공"),
            error => Debug.LogError("Currency 저장 실패: " + error.GenerateErrorReport()));
    }
}
