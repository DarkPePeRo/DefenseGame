using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentInventoryService : MonoBehaviour
{
    public static EquipmentInventoryService Instance;

    public event Action OnInventoryChanged;
    public event Action OnLoaded;

    public SummonLevelState CurrentSummonLevelState { get; private set; }
    public Dictionary<string, int> ItemStacks { get; private set; } = new();
    public Dictionary<string, int> UpgradeLevels { get; private set; } = new();

    public bool IsLoaded { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void LoadFromServerAfterLogin()
    {
        GachaService.GetGachaInitData(
            onSuccess: response =>
            {
                CurrentSummonLevelState = response.summonLevelState ?? new SummonLevelState
                {
                    equipment = new SummonGroupState { level = 1, exp = 0 }
                };

                ItemStacks = response.itemStacks ?? new Dictionary<string, int>();

                PlayFabEquipmentUpgradeService.Load(levels =>
                {
                    UpgradeLevels = levels ?? new Dictionary<string, int>();
                    IsLoaded = true;

                    Debug.Log("[EquipmentInventoryService] 초기 로드 완료");
                    OnLoaded?.Invoke();
                    OnInventoryChanged?.Invoke();
                });
            },
            onFail: error =>
            {
                Debug.LogError("[EquipmentInventoryService] 초기 로드 실패: " + error);
                IsLoaded = false;
            });
    }

    public void ApplyServerSnapshot(Dictionary<string, int> itemStacks, SummonLevelState summonLevelState)
    {
        ItemStacks = itemStacks ?? new Dictionary<string, int>();
        CurrentSummonLevelState = summonLevelState ?? CurrentSummonLevelState;
        IsLoaded = true;

        OnInventoryChanged?.Invoke();
    }

    public int GetCount(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return 0;
        return ItemStacks != null && ItemStacks.TryGetValue(itemId, out int count) ? count : 0;
    }

    public int GetUpgradeLevel(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return 0;
        return UpgradeLevels != null && UpgradeLevels.TryGetValue(itemId, out int level) ? level : 0;
    }

    public int GetDisplayedLevel(string itemId)
    {
        return GetUpgradeLevel(itemId) + 1;
    }

    public int GetCurrentDamagePercent(string itemId)
    {
        var def = EquipmentItemDatabase.Instance.Get(itemId);
        if (def == null) return 0;

        int level = GetUpgradeLevel(itemId);
        return def.baseDamagePercent + (def.damagePerUpgrade * level);
    }

    public int GetNextDamagePercent(string itemId)
    {
        var def = EquipmentItemDatabase.Instance.Get(itemId);
        if (def == null) return 0;

        int level = GetUpgradeLevel(itemId);
        if (level >= def.maxUpgradeLevel)
            return GetCurrentDamagePercent(itemId);

        return def.baseDamagePercent + (def.damagePerUpgrade * (level + 1));
    }

    public int GetUpgradeCost(string itemId)
    {
        var def = EquipmentItemDatabase.Instance.Get(itemId);
        if (def == null) return 0;

        int level = GetUpgradeLevel(itemId);
        float cost = def.baseUpgradeCost * Mathf.Pow(def.upgradeCostMultiplier, level);
        return Mathf.RoundToInt(cost);
    }

    public bool IsMaxLevel(string itemId)
    {
        var def = EquipmentItemDatabase.Instance.Get(itemId);
        if (def == null) return true;

        return GetUpgradeLevel(itemId) >= def.maxUpgradeLevel;
    }

    public bool CanUpgrade(string itemId)
    {
        if (GetCount(itemId) <= 0) return false;
        if (IsMaxLevel(itemId)) return false;
        if (PlayerCurrency.Instance == null || PlayerCurrency.Instance.gold == null) return false;

        int cost = GetUpgradeCost(itemId);
        return PlayerCurrency.Instance.gold.amount >= cost;
    }

    public bool TryUpgrade(string itemId, out string reason)
    {
        reason = "";

        if (string.IsNullOrEmpty(itemId))
        {
            reason = "선택된 장비가 없습니다.";
            return false;
        }

        var def = EquipmentItemDatabase.Instance.Get(itemId);
        if (def == null)
        {
            reason = "장비 정의를 찾을 수 없습니다.";
            return false;
        }

        if (GetCount(itemId) <= 0)
        {
            reason = "장비를 보유하고 있지 않습니다.";
            return false;
        }

        int currentLevel = GetUpgradeLevel(itemId);
        if (currentLevel >= def.maxUpgradeLevel)
        {
            reason = "최대 레벨입니다.";
            return false;
        }

        if (PlayerCurrency.Instance == null || PlayerCurrency.Instance.gold == null)
        {
            reason = "골드 데이터를 찾을 수 없습니다.";
            return false;
        }

        int cost = GetUpgradeCost(itemId);
        if (PlayerCurrency.Instance.gold.amount < cost)
        {
            reason = "골드가 부족합니다.";
            return false;
        }

        PlayerCurrency.Instance.gold.amount -= cost;
        UpgradeLevels[itemId] = currentLevel + 1;

        PlayFabEquipmentUpgradeService.Save(UpgradeLevels,
            onSuccess: () =>
            {
                Debug.Log($"[EquipmentUpgrade] 저장 성공: {itemId} -> Lv.{UpgradeLevels[itemId]}");
            },
            onFail: error =>
            {
                Debug.LogError("[EquipmentUpgrade] 저장 실패: " + error);
            });

        OnInventoryChanged?.Invoke();
        return true;
    }

    public int GetEquipmentSummonLevel()
    {
        return CurrentSummonLevelState?.equipment?.level ?? 1;
    }

    public int GetEquipmentSummonExp()
    {
        return CurrentSummonLevelState?.equipment?.exp ?? 0;
    }
}