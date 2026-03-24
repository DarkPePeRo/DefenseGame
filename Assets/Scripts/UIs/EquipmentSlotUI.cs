using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour
{
    public Button button;
    public Image iconImage;
    public TMP_Text codeText;
    public TMP_Text progressText;
    public TMP_Text levelText;
    public GameObject redDot;
    public GameObject selectedFrame;
    public GameObject LockedFrame;

    private string itemId;
    private EquipmentPanelUI owner;

    public void Bind(EquipmentItemDefinition def, int count, int displayedLevel, EquipmentPanelUI panelOwner, bool selected)
    {
        owner = panelOwner;
        itemId = def.itemId;

        if (iconImage) iconImage.sprite = def.icon;
        if (codeText) codeText.text = def.displayCode;
        if (progressText) progressText.text = $"{count}/5";
        if (levelText) levelText.text = $"Lv.{displayedLevel}";
        if (redDot) redDot.SetActive(count >= 5);
        if (selectedFrame) selectedFrame.SetActive(selected);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => owner.SelectItem(itemId));
    }

    public void SetLocked(bool visible)
    {
        if (visible)
        {
            ColorBlock colorBlock = button.colors;
            colorBlock.normalColor = new Color(94 / 255f, 94 / 255f, 94 / 255f, 1);
            iconImage.color = new Color(34 / 255f, 34 / 255f, 34 / 255f, 1);
            LockedFrame.SetActive(true);
        }
        else
        {
            ColorBlock colorBlock = button.colors;
            colorBlock.normalColor = new Color(1, 1, 1, 1);
            iconImage.color = new Color(1, 1, 1, 1);
            LockedFrame.SetActive(false);
        }
    }
}