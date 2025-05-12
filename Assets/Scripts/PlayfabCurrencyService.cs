using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabCurrencyService
{
    public static void Load(Action onComplete)
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), result =>
        {
            var vc = result.VirtualCurrency;

            // 골드 VC: "GD"
            if (vc != null && vc.TryGetValue("GD", out int gd))
            {
                PlayerCurrency.Instance.gold.amount = gd;
                Debug.Log("[Currency] 골드 불러오기 완료: " + gd);
            }
            else
            {
                PlayerCurrency.Instance.gold.amount = 0;
                Debug.LogWarning("[Currency] 골드 정보 없음, 0으로 초기화");
            }

            // 다이아 VC: "DM"
            if (vc != null && vc.TryGetValue("DM", out int dm))
            {
                PlayerCurrency.Instance.diamond.amount = dm;
                Debug.Log("[Currency] 다이아 불러오기 완료: " + dm);
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
            Debug.LogError("[Currency] 불러오기 실패: " + error.GenerateErrorReport());
            onComplete?.Invoke();
        });
    }

    // VC 기반에서는 직접 저장 호출은 거의 없음 → CloudScript 또는 자동 저장 사용
    // 예: GrantGold / SpendGold 등
}
