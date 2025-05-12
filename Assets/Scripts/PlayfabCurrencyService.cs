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

            // ��� VC: "GD"
            if (vc != null && vc.TryGetValue("GD", out int gd))
            {
                PlayerCurrency.Instance.gold.amount = gd;
                Debug.Log("[Currency] ��� �ҷ����� �Ϸ�: " + gd);
            }
            else
            {
                PlayerCurrency.Instance.gold.amount = 0;
                Debug.LogWarning("[Currency] ��� ���� ����, 0���� �ʱ�ȭ");
            }

            // ���̾� VC: "DM"
            if (vc != null && vc.TryGetValue("DM", out int dm))
            {
                PlayerCurrency.Instance.diamond.amount = dm;
                Debug.Log("[Currency] ���̾� �ҷ����� �Ϸ�: " + dm);
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
            Debug.LogError("[Currency] �ҷ����� ����: " + error.GenerateErrorReport());
            onComplete?.Invoke();
        });
    }

    // VC ��ݿ����� ���� ���� ȣ���� ���� ���� �� CloudScript �Ǵ� �ڵ� ���� ���
    // ��: GrantGold / SpendGold ��
}
