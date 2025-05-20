using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using TMPro;
using GooglePlayGames.BasicApi;
using System;
using PlayFab.Json;
using System.Collections;

[System.Serializable]
public class Currency
{
    public string name;
    public int amount;
}

public class PlayerCurrency : MonoBehaviour
{
    public static PlayerCurrency Instance;
    public Currency gold;
    public Currency diamond;

    private int goldBuffer = 0;
    private float saveInterval = 20f;
    private float saveTimer = 0f;
    private bool isSaving = false;


    private void Awake()
    {
        // �̱��� ���� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ������Ʈ ����
            gold = new Currency { name = "gold", amount = 0 };
            diamond = new Currency { name = "diamond", amount = 0 };
        }
        else
        {
            Destroy(gameObject); // �ߺ��� ������Ʈ�� ����
        }
    }
    private void Update()
    {
        if (goldBuffer <= 0 || isSaving) return;

        saveTimer += Time.deltaTime;
        if (saveTimer >= saveInterval)
        {
            saveTimer = 0f;
            StartCoroutine(SaveBufferedGoldToServer());
        }
    }
    public void AddGoldBuffered(int amount)
    {
        goldBuffer += amount;
        gold.amount += amount; // UI ���ſ�

        Debug.Log($"��� ȹ�� ����: {amount} �� �� ����: {goldBuffer}");
    }
    private IEnumerator SaveBufferedGoldToServer()
    {
        isSaving = true;

        int amountToSave = goldBuffer;
        goldBuffer = 0;

        gold.amount += amountToSave;

        Debug.Log($"[��� ���� ��û] Ŭ�� ���� ����: +{amountToSave}, �հ� {gold.amount}");

        bool success = false;

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
        {
            { "goldAmount", gold.amount.ToString() }
        }
        };

        PlayFabClientAPI.UpdateUserData(request, result =>
        {
            Debug.Log($"[��� ���� ����] �� UserData.goldAmount = {gold.amount}");
            success = true;
        },
        error =>
        {
            Debug.LogError("[��� ���� ����] ��õ� ����: " + error.GenerateErrorReport());
        });

        yield return new WaitForSeconds(0.5f);
        if (!success)
        {
            goldBuffer += amountToSave; // ���� �� �ѹ�
        }

        isSaving = false;
    }


    public bool HasGoldBuffer()
    {
        return goldBuffer > 0;
    }

    public void RequestAddDiamond(string reason)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
        {
            FunctionName = "GrantDiamond",
            FunctionParameter = new { reason = reason }
        }, result =>
        {
            try
            {
                // JSON ���ڿ� ���� �Ľ�
                var jsonText = result.FunctionResult.ToString();
                var json = PlayFabSimpleJson.DeserializeObject<Dictionary<string, object>>(jsonText);

                if (json.TryGetValue("newBalance", out var balanceObj))
                {
                    int balance = Convert.ToInt32(balanceObj);
                    PlayerCurrency.Instance.diamond.amount = balance;
                    Debug.Log($"[Diamond] ���� �Ϸ�: {balance}"); 
                    Debug.Log("FunctionResult JSON: " + result.FunctionResult.ToString());

                }
                else
                {
                    Debug.LogWarning("[Diamond] newBalance Ű ����");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[Diamond] ���� �߻�: " + ex.Message);
            }
        }, error =>
        {
            Debug.LogError("[Diamond] ���� ����: " + error.GenerateErrorReport());
        });
    }


    // ��� �Һ� ��û
    public void RequestSpendGold(int amount, Action<bool> onResult = null)
    {
        if (gold.amount < amount)
        {
            Debug.LogWarning("[Gold] �Һ� ����: �ܾ� ����");
            onResult?.Invoke(false);
            return;
        }

        gold.amount -= amount;

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
        {
            { "goldAmount", gold.amount.ToString() }
        }
        };

        PlayFabClientAPI.UpdateUserData(request,
            result =>
            {
                Debug.Log($"[Gold] ��� ����: {amount} �� ���� ���: {gold.amount}");
                onResult?.Invoke(true);
            },
            error =>
            {
                // ���� ���� �� �ѹ� ó��
                gold.amount += amount;
                Debug.LogError("[Gold] ���� ���� �� �ѹ��: " + error.GenerateErrorReport());
                onResult?.Invoke(false);
            });
    }


    // ���̾� �Һ� ��û
    public void RequestSpendDiamond(string reason, Action<bool> onResult = null)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
        {
            FunctionName = "SpendDiamond",
            FunctionParameter = new { reason = reason }
        }, result =>
        {
            var json = result.FunctionResult as Dictionary<string, object>;
            if (json != null && (bool)json["success"])
            {
                diamond.amount = Convert.ToInt32(json["newBalance"]);
                Debug.Log($"���� �ܾ�: {diamond.amount}");
                onResult?.Invoke(true);
            }
            else
            {
                Debug.LogWarning("[Diamond] ��� ���� (�ܾ� ���� �Ǵ� ��Ÿ)");
                onResult?.Invoke(false);
            }
        }, error =>
        {
            Debug.LogError("���̾� �Һ� ����: " + error.GenerateErrorReport());
            onResult?.Invoke(false);
        });
    }

    // �� ���� �� ���� ��� ����
    private void OnApplicationQuit()
    {
        if (goldBuffer > 0)
        {
            Debug.Log("[���� ����] �� ���� ���� ��� ���� �õ�");
            SaveBufferedGoldSync();
        }
    }

    private void SaveBufferedGoldSync()
    {
        int amount = goldBuffer;
        goldBuffer = 0;

        gold.amount += amount;

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
        {
            { "goldAmount", gold.amount.ToString() }
        }
        };

        PlayFabClientAPI.UpdateUserData(request, result =>
        {
            Debug.Log($"[���� ���� ����] ���: {gold.amount}");
        },
        error =>
        {
            Debug.LogError("[���� ���� ����]: " + error.GenerateErrorReport());
            goldBuffer += amount;
        });
    }

}
