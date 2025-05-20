using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabCurrencyService
{
    public static void Load(Action onComplete)
    {
        // 1. ���� UserData���� �ҷ�����
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), userDataResult =>
        {
            if (userDataResult.Data != null && userDataResult.Data.TryGetValue("goldAmount", out var goldEntry))
            {
                PlayerCurrency.Instance.gold.amount = int.Parse(goldEntry.Value);
                Debug.Log("[Currency] ���(UserData) �ҷ����� �Ϸ�: " + goldEntry.Value);
            }
            else
            {
                PlayerCurrency.Instance.gold.amount = 0;
                Debug.LogWarning("[Currency] ��� ���� ����, 0���� �ʱ�ȭ");
            }

            // 2. ���̾ƴ� VC(DM)���� �ҷ�����
            PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), inventoryResult =>
            {
                var vc = inventoryResult.VirtualCurrency;
                if (vc != null && vc.TryGetValue("DM", out int dm))
                {
                    PlayerCurrency.Instance.diamond.amount = dm;
                    Debug.Log("[Currency] ���̾�(VC) �ҷ����� �Ϸ�: " + dm);
                }
                else
                {
                    PlayerCurrency.Instance.diamond.amount = 0;
                    Debug.LogWarning("[Currency] ���̾� ���� ����, 0���� �ʱ�ȭ");
                }

                onComplete?.Invoke();
            },
            error =>
            {
                Debug.LogError("[Currency] ���̾� �ҷ����� ����: " + error.GenerateErrorReport());
                onComplete?.Invoke();
            });
        },
        error =>
        {
            Debug.LogError("[Currency] ��� �ҷ����� ����: " + error.GenerateErrorReport());
            onComplete?.Invoke();
        });
    }
}
