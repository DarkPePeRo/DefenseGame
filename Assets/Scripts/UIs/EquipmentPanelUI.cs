using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentPanelUI : MonoBehaviour
{
    [Header("고정 슬롯 연결")]
    public EquipmentSlotUI[] weaponSlots;

    [Header("오른쪽 상세")]
    public Image detailIcon;
    public TMP_Text detailCodeText;
    public TMP_Text detailNameText;
    public TMP_Text detailDamageText;
    public TMP_Text detailNextDamageText;
    public TMP_Text detailHoldDamageText;
    public TMP_Text detailOptionText;
    public TMP_Text detailLevelText;

    public Button selectButton;
    public TMP_Text upgradeCostText;
    public Button upgradeButton;

    private readonly string[] weaponItemIds =
    {
        "sword_d1", "sword_d2", "sword_d3", "sword_d4",
        "sword_c1", "sword_c2", "sword_c3", "sword_c4",
        "sword_b1", "sword_b2", "sword_b3", "sword_b4",
        "sword_a1", "sword_a2", "sword_a3", "sword_a4"
    };

    private string selectedItemId;

    private void OnEnable()
    {
        if (EquipmentInventoryService.Instance != null)
            EquipmentInventoryService.Instance.OnInventoryChanged += RefreshAll;

        RefreshAll();
    }

    private void OnDisable()
    {
        if (EquipmentInventoryService.Instance != null)
            EquipmentInventoryService.Instance.OnInventoryChanged -= RefreshAll;
    }

    public void RefreshAll()
    {
        if (EquipmentInventoryService.Instance == null) return;
        if (!EquipmentInventoryService.Instance.IsLoaded) return;

        RefreshSlots();

        if (string.IsNullOrEmpty(selectedItemId))
            AutoSelectFirstOwned();

        RefreshDetail();
    }

    private void RefreshSlots()
    {
        int count = Mathf.Min(weaponSlots.Length, weaponItemIds.Length);

        for (int i = 0; i < count; i++)
        {
            string itemId = weaponItemIds[i];
            var def = EquipmentItemDatabase.Instance.Get(itemId);
            if (def == null)
            {
                weaponSlots[i].gameObject.SetActive(false);
                continue;
            }

            int ownedCount = EquipmentInventoryService.Instance.GetCount(itemId);
            int displayedLevel = EquipmentInventoryService.Instance.GetDisplayedLevel(itemId);
            bool hasItem = ownedCount > 0;
            bool selected = selectedItemId == itemId;

            weaponSlots[i].Bind(def, ownedCount, displayedLevel, this, selected);

            weaponSlots[i].SetLocked(!hasItem);
        }
    }

    private void AutoSelectFirstOwned()
    {
        for (int i = 0; i < weaponItemIds.Length; i++)
        {
            string itemId = weaponItemIds[i];
            if (EquipmentInventoryService.Instance.GetCount(itemId) > 0)
            {
                selectedItemId = itemId;
                return;
            }
        }

        selectedItemId = null;
    }

    public void SelectItem(string itemId)
    {
        selectedItemId = itemId;
        RefreshSlots();
        RefreshDetail();
    }

    private void RefreshDetail()
    {
        if (string.IsNullOrEmpty(selectedItemId))
        {
            detailCodeText.text = "";
            detailNameText.text = "";
            detailDamageText.text = "";
            detailNextDamageText.text = "";
            detailHoldDamageText.text = "";
            detailOptionText.text = "";
            detailLevelText.text = "";
            upgradeCostText.text = "";
            if (detailIcon) detailIcon.enabled = false;
            return;
        }

        if (EquipmentItemDatabase.Instance == null) return;

        var def = EquipmentItemDatabase.Instance.Get(selectedItemId);
        if (def == null) return;

        int ownedCount = EquipmentInventoryService.Instance.GetCount(selectedItemId);
        bool hasItem = ownedCount > 0;

        if (def == null) return;

        if (detailIcon)
        {
            detailIcon.enabled = true;
            detailIcon.sprite = def.icon;
        }
        detailCodeText.text = def.displayCode;
        detailNameText.text = def.displayName;
        detailDamageText.text = hasItem ? $"{def.baseDamagePercent}%" : "미보유";
        detailNextDamageText.text = hasItem ? $"{def.baseDamagePercent + 30}%" : $"{def.baseDamagePercent}%";
        detailHoldDamageText.text = hasItem ? $"DMG {def.baseDamagePercent}%" : "미보유";
        detailOptionText.text = hasItem ? "NONE" : "미보유";
        selectButton.interactable = hasItem;
        upgradeButton.interactable = hasItem;
    }
}