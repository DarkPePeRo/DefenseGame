using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabCurrencyService
{
    public static void Load(Action onComplete)
    {
        // 1. 골드는 UserData에서 불러오기
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), userDataResult =>
        {
            if (userDataResult.Data != null && userDataResult.Data.TryGetValue("goldAmount", out var goldEntry))
            {
                PlayerCurrency.Instance.gold.amount = int.Parse(goldEntry.Value);
                Debug.Log("[Currency] 골드(UserData) 불러오기 완료: " + goldEntry.Value);
            }
            else
            {
                PlayerCurrency.Instance.gold.amount = 0;
                Debug.LogWarning("[Currency] 골드 정보 없음, 0으로 초기화");
            }

            // 2. 다이아는 VC(DM)에서 불러오기
            PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), inventoryResult =>
            {
                var vc = inventoryResult.VirtualCurrency;
                if (vc != null && vc.TryGetValue("DM", out int dm))
                {
                    PlayerCurrency.Instance.diamond.amount = dm;
                    Debug.Log("[Currency] 다이아(VC) 불러오기 완료: " + dm);
                }
                else
                {
                    PlayerCurrency.Instance.diamond.amount = 0;
                    Debug.LogWarning("[Currency] 다이아 정보 없음, 0으로 초기화");
                }

                onComplete?.Invoke();
            },
            error =>
            {
                Debug.LogError("[Currency] 다이아 불러오기 실패: " + error.GenerateErrorReport());
                onComplete?.Invoke();
            });
        },
        error =>
        {
            Debug.LogError("[Currency] 골드 불러오기 실패: " + error.GenerateErrorReport());
            onComplete?.Invoke();
        });
    }
}
