using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatUpgradeUI : MonoBehaviour
{
    public GodStatManage statsManager;
    public TextMeshProUGUI attackPowerLevelText, attackSpeedLevelText, criticalRateLevelText, criticalDamageLevelText;
    public TextMeshProUGUI attackPowerText, attackSpeedText, criticalRateText, criticalDamageText;
    public TextMeshProUGUI attackPowerGold, attackSpeedGold, criticalRateGold, criticalDamageGold;
    public Button upgradeAttackPowerButton, upgradeAttackSpeedButton, upgradeCriticalRateButton, upgradeCriticalDamageButton;
    public Button saveButton, loadButton;

    private void Start()
    {
        statsManager = FindObjectOfType<GodStatManage>();

        upgradeAttackPowerButton.onClick.AddListener(() => UpgradeStat("attackPower"));
        upgradeAttackSpeedButton.onClick.AddListener(() => UpgradeStat("attackSpeed"));
        upgradeCriticalRateButton.onClick.AddListener(() => UpgradeStat("criticalRate"));
        upgradeCriticalDamageButton.onClick.AddListener(() => UpgradeStat("criticalDamage"));

        UpdateUI();
    }

    private void UpgradeStat(string statType)
    {
        statsManager.LevelUp(statType);
        UpdateUI();
    }

    private void UpdateUI()
    {
        attackPowerLevelText.text = "Lv. " + statsManager.attackPowerLevel;
        attackSpeedLevelText.text = "Lv. " + statsManager.attackSpeedLevel;
        criticalRateLevelText.text = "Lv. " + statsManager.criticalRateLevel;
        criticalDamageLevelText.text = "Lv. " + statsManager.criticalDamageLevel;

        attackPowerText.text = "" + statsManager.attackPower;
        attackSpeedText.text = "" + statsManager.attackSpeed;
        criticalRateText.text = "" + statsManager.criticalRate;
        criticalDamageText.text = "" + statsManager.criticalDamage;

        attackPowerGold.text = "" + statsManager.attackPowerGold;
        attackSpeedGold.text = "" + statsManager.attackSpeedGold;
        criticalRateGold.text = "" + statsManager.criticalRateGold;
        criticalDamageGold.text = "" + statsManager.criticalDamageGold;
    }
}
