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
    public event Action<int> OnGoldChanged;
    public Currency gold;
    public Currency diamond;

    private int goldBuffer = 0;
    private float saveInterval = 60f;
    private float saveTimer = 0f;
    private bool isSaving = false;

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (gold == null) gold = new Currency { name = "gold", amount = 0 };
        if (diamond == null) diamond = new Currency { name = "diamond", amount = 0 };
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
        gold.amount += amount; // UI 갱신용
        OnGoldChanged?.Invoke(gold.amount);
        Debug.Log($"골드 획득 누적: {amount} → 총 누적: {goldBuffer}");
    }
    private IEnumerator SaveBufferedGoldToServer()
    {
        isSaving = true;

        int amountToSave = goldBuffer;
        goldBuffer = 0;

        Debug.Log($"[골드 저장 요청] 클라 직접 저장: +{amountToSave}, 합계 {gold.amount}");

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
            Debug.Log($"[골드 저장 성공] → UserData.goldAmount = {gold.amount}");
            success = true;
        },
        error =>
        {
            Debug.LogError("[골드 저장 실패] 재시도 예정: " + error.GenerateErrorReport());
        });

        yield return new WaitForSeconds(0.5f);
        if (!success)
        {
            goldBuffer += amountToSave; // 실패 시 롤백
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
                if (result == null)
                {
                    Debug.LogError("[Diamond] result == null");
                    return;
                }
                if (result.FunctionResult == null)
                {
                    Debug.LogError("[Diamond] FunctionResult == null (CloudScript return 없음/서버 오류)");
                    if (result.Error != null)
                        Debug.LogError("[Diamond] CloudScriptError: " + result.Error.Message);
                    return;
                }

                // JSON 문자열 직접 파싱
                var jsonText = result.FunctionResult.ToString();
                var json = PlayFabSimpleJson.DeserializeObject<Dictionary<string, object>>(jsonText);

                if (json.TryGetValue("newBalance", out var balanceObj))
                {
                    int balance = Convert.ToInt32(balanceObj);
                    diamond.amount = balance;
                    Debug.Log($"[Diamond] 지급 완료: {balance}"); 
                    Debug.Log("FunctionResult JSON: " + result.FunctionResult.ToString());

                }
                else
                {
                    Debug.LogWarning("[Diamond] newBalance 키 없음");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[Diamond] 예외 발생: " + ex.Message);
            }
        }, error =>
        {
            Debug.LogError("[Diamond] 지급 실패: " + error.GenerateErrorReport());
        });
    }


    // 골드 소비 요청
    public void RequestSpendGold(int amount, Action<bool> onResult = null)
    {
        if (gold.amount < amount)
        {
            Debug.LogWarning("[Gold] 소비 실패: 잔액 부족");
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
                Debug.Log($"[Gold] 사용 성공: {amount} → 남은 골드: {gold.amount}");
                onResult?.Invoke(true);
            },
            error =>
            {
                // 저장 실패 시 롤백 처리
                gold.amount += amount;
                Debug.LogError("[Gold] 저장 실패 → 롤백됨: " + error.GenerateErrorReport());
                onResult?.Invoke(false);
            });
    }


    // 다이아 소비 요청
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
                Debug.Log($"남은 잔액: {diamond.amount}");
                onResult?.Invoke(true);
            }
            else
            {
                Debug.LogWarning("[Diamond] 사용 실패 (잔액 부족 또는 기타)");
                onResult?.Invoke(false);
            }
        }, error =>
        {
            Debug.LogError("다이아 소비 실패: " + error.GenerateErrorReport());
            onResult?.Invoke(false);
        });
    }

    // 앱 종료 시 강제 골드 저장
    private void OnApplicationQuit()
    {
        if (goldBuffer > 0)
        {
            Debug.Log("[종료 저장] 앱 종료 전에 골드 저장 시도");
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
            Debug.Log($"[종료 저장 성공] 골드: {gold.amount}");
        },
        error =>
        {
            Debug.LogError("[종료 저장 실패]: " + error.GenerateErrorReport());
            goldBuffer += amount;
        });
    }

}
