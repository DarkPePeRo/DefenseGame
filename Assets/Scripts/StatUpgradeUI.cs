using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Rendering;

public class StatUpgradeUI : MonoBehaviour
{
    public enum StatType
    {
        attackPower,
        attackSpeed,
        criticalRate,
        criticalDamage
    }

    public static StatUpgradeUI Instance;

    public GodStatManage statsManager;

    [System.Serializable]
    public class StatUIGroup
    {
        public StatType type;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI valueText;
        public TextMeshProUGUI goldText;
        public Button upgradeButton;
        public Sprite normalSprite;
        public Sprite maxedSprite;
    }

    public StatUIGroup[] statUIGroups;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        statsManager = FindObjectOfType<GodStatManage>();

        foreach (var group in statUIGroups)
        {
            var capturedGroup = group; // for closure
            group.upgradeButton.onClick.AddListener(() => OnClickUpgrade(capturedGroup.type));
        }

        UpdateUI();
    }

    private void OnClickUpgrade(StatType type)
    {
        statsManager.RequestLevelUp(type.ToString());
        UpdateUI();
    }

    public void UpdateUI()
    {
        foreach (var group in statUIGroups)
        {
            int level = statsManager.GetCurrentLevel(group.type.ToString());
            float value = statsManager.GetStatValue(group.type.ToString());
            int gold = statsManager.GetStatGold(group.type.ToString());

            group.levelText.text = $"Lv. {level}";
            group.valueText.text = $"{value}";
            group.goldText.text = $"{gold}";
            if (group.type == StatType.criticalRate) 
            {
                group.valueText.text = $"{value} %";
            }
            if (group.type == StatType.criticalDamage)
            {
                group.valueText.text = $"{value} %";
            }
            if (PlayerCurrency.Instance.gold.amount < gold)
            {
                group.upgradeButton.image.sprite = group.maxedSprite;
            }
            bool isMaxed = statsManager.IsMaxLevel(group.type.ToString());
            group.upgradeButton.interactable = !isMaxed;
            if (isMaxed)
            {
                group.goldText.text = "최대 레벨";
                group.goldText.fontSize = 40;
                group.goldText.color = Color.black;
            }
            if (group.maxedSprite != null && group.normalSprite != null)
                group.upgradeButton.image.sprite = isMaxed ? group.maxedSprite : group.normalSprite;
        }
    }
}
