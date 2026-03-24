using System;
using System.Collections.Generic;
using UnityEngine;

public enum EquipmentGrade
{
    Common,
    Rare,
    Epic,
    Legendary
}

[Serializable]
public class EquipmentItemDefinition
{
    public string itemId;
    public string displayCode;
    public string displayName;
    public EquipmentGrade grade;
    public Sprite icon;

    [Header("±âº» ¼º´É")]
    public int baseDamagePercent;

    [Header("¾÷±×·¹ÀÌµå")]
    public int damagePerUpgrade;
    public int baseUpgradeCost;
    public float upgradeCostMultiplier = 1.2f;
    public int maxUpgradeLevel = 100;

    public string nextItemId;
}

public class EquipmentItemDatabase : MonoBehaviour
{
    public static EquipmentItemDatabase Instance;

    [Header("¾ÆÀÌÄÜ ¿¬°á")]
    public Sprite defaultSwordIcon;

    [Header("Àåºñ Á¤ÀÇ")]
    public List<EquipmentItemDefinition> items = new();

    private Dictionary<string, EquipmentItemDefinition> map = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        BuildDefaultDataIfEmpty();
        BuildMap();
    }

    private void BuildMap()
    {
        map.Clear();
        foreach (var item in items)
        {
            if (item == null || string.IsNullOrEmpty(item.itemId)) continue;
            map[item.itemId] = item;
        }
    }

    public EquipmentItemDefinition Get(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return null;
        return map.TryGetValue(itemId, out var def) ? def : null;
    }

    public List<EquipmentItemDefinition> GetAllSorted()
    {
        return new List<EquipmentItemDefinition>(items);
    }

    private void BuildDefaultDataIfEmpty()
    {
        if (items != null && items.Count > 0) return;

        items = new List<EquipmentItemDefinition>
        {
            new EquipmentItemDefinition { itemId = "sword_d1", displayCode = "D1", displayName = "³°Àº Sword", grade = EquipmentGrade.Common, icon = defaultSwordIcon, baseDamagePercent = 50, damagePerUpgrade = 5, baseUpgradeCost = 100, upgradeCostMultiplier = 1.15f, maxUpgradeLevel = 100, nextItemId = "sword_d2" },
            new EquipmentItemDefinition { itemId = "sword_d2", displayCode = "D2", displayName = "µµÁ¦ Sword", grade = EquipmentGrade.Common, icon = defaultSwordIcon, baseDamagePercent = 55, damagePerUpgrade = 5, baseUpgradeCost = 120, upgradeCostMultiplier = 1.15f, maxUpgradeLevel = 100, nextItemId = "sword_d3" },
            new EquipmentItemDefinition { itemId = "sword_d3", displayCode = "D3", displayName = "±âÃÊ Sword", grade = EquipmentGrade.Common, icon = defaultSwordIcon, baseDamagePercent = 60, damagePerUpgrade = 5, baseUpgradeCost = 140, upgradeCostMultiplier = 1.15f, maxUpgradeLevel = 100, nextItemId = "sword_d4" },
            new EquipmentItemDefinition { itemId = "sword_d4", displayCode = "D4", displayName = "´Ü°ËÇü Sword", grade = EquipmentGrade.Common, icon = defaultSwordIcon, baseDamagePercent = 65, damagePerUpgrade = 5, baseUpgradeCost = 160, upgradeCostMultiplier = 1.15f, maxUpgradeLevel = 100, nextItemId = "sword_c1" },

            new EquipmentItemDefinition { itemId = "sword_c1", displayCode = "C1", displayName = "ÈÆ·Ã¿ë Sword", grade = EquipmentGrade.Common, icon = defaultSwordIcon, baseDamagePercent = 100, damagePerUpgrade = 8, baseUpgradeCost = 300, upgradeCostMultiplier = 1.16f, maxUpgradeLevel = 100, nextItemId = "sword_c2" },
            new EquipmentItemDefinition { itemId = "sword_c2", displayCode = "C2", displayName = "Ã»µ¿ Sword", grade = EquipmentGrade.Common, icon = defaultSwordIcon, baseDamagePercent = 110, damagePerUpgrade = 8, baseUpgradeCost = 320, upgradeCostMultiplier = 1.16f, maxUpgradeLevel = 100, nextItemId = "sword_c3" },
            new EquipmentItemDefinition { itemId = "sword_c3", displayCode = "C3", displayName = "°­Ã¶ Sword", grade = EquipmentGrade.Common, icon = defaultSwordIcon, baseDamagePercent = 120, damagePerUpgrade = 8, baseUpgradeCost = 340, upgradeCostMultiplier = 1.16f, maxUpgradeLevel = 100, nextItemId = "sword_c4" },
            new EquipmentItemDefinition { itemId = "sword_c4", displayCode = "C4", displayName = "Àººû Sword", grade = EquipmentGrade.Common, icon = defaultSwordIcon, baseDamagePercent = 130, damagePerUpgrade = 8, baseUpgradeCost = 360, upgradeCostMultiplier = 1.16f, maxUpgradeLevel = 100, nextItemId = "sword_b1" },

            new EquipmentItemDefinition { itemId = "sword_b1", displayCode = "B1", displayName = "Èñ±Í ÈÆ·Ã¿ë Sword", grade = EquipmentGrade.Rare, icon = defaultSwordIcon, baseDamagePercent = 180, damagePerUpgrade = 12, baseUpgradeCost = 800, upgradeCostMultiplier = 1.18f, maxUpgradeLevel = 100, nextItemId = "sword_b2" },
            new EquipmentItemDefinition { itemId = "sword_b2", displayCode = "B2", displayName = "Èñ±Í Ã»µ¿ Sword", grade = EquipmentGrade.Rare, icon = defaultSwordIcon, baseDamagePercent = 190, damagePerUpgrade = 12, baseUpgradeCost = 850, upgradeCostMultiplier = 1.18f, maxUpgradeLevel = 100, nextItemId = "sword_b3" },
            new EquipmentItemDefinition { itemId = "sword_b3", displayCode = "B3", displayName = "Èñ±Í °­Ã¶ Sword", grade = EquipmentGrade.Rare, icon = defaultSwordIcon, baseDamagePercent = 200, damagePerUpgrade = 12, baseUpgradeCost = 900, upgradeCostMultiplier = 1.18f, maxUpgradeLevel = 100, nextItemId = "sword_b4" },
            new EquipmentItemDefinition { itemId = "sword_b4", displayCode = "B4", displayName = "Èñ±Í Àººû Sword", grade = EquipmentGrade.Rare, icon = defaultSwordIcon, baseDamagePercent = 210, damagePerUpgrade = 12, baseUpgradeCost = 950, upgradeCostMultiplier = 1.18f, maxUpgradeLevel = 100, nextItemId = "sword_a1" },

            new EquipmentItemDefinition { itemId = "sword_a1", displayCode = "A1", displayName = "Àü¼³ ÈÆ·Ã¿ë Sword", grade = EquipmentGrade.Epic, icon = defaultSwordIcon, baseDamagePercent = 300, damagePerUpgrade = 20, baseUpgradeCost = 2000, upgradeCostMultiplier = 1.2f, maxUpgradeLevel = 100, nextItemId = "sword_a2" },
            new EquipmentItemDefinition { itemId = "sword_a2", displayCode = "A2", displayName = "Àü¼³ Ã»µ¿ Sword", grade = EquipmentGrade.Epic, icon = defaultSwordIcon, baseDamagePercent = 320, damagePerUpgrade = 20, baseUpgradeCost = 2100, upgradeCostMultiplier = 1.2f, maxUpgradeLevel = 100, nextItemId = "sword_a3" },
            new EquipmentItemDefinition { itemId = "sword_a3", displayCode = "A3", displayName = "Àü¼³ °­Ã¶ Sword", grade = EquipmentGrade.Epic, icon = defaultSwordIcon, baseDamagePercent = 340, damagePerUpgrade = 20, baseUpgradeCost = 2200, upgradeCostMultiplier = 1.2f, maxUpgradeLevel = 100, nextItemId = "sword_a4" },
            new EquipmentItemDefinition { itemId = "sword_a4", displayCode = "A4", displayName = "Àü¼³ Àººû Sword", grade = EquipmentGrade.Epic, icon = defaultSwordIcon, baseDamagePercent = 360, damagePerUpgrade = 20, baseUpgradeCost = 2300, upgradeCostMultiplier = 1.2f, maxUpgradeLevel = 100, nextItemId = null }
        };
    }
}