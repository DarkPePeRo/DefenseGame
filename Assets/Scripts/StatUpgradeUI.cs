using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Rendering;
using static StatUpgradeUI;

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
            var capturedGroup = group;

            // 1. StatUpgradeButton 컴포넌트 추가
            var statBtn = group.upgradeButton.gameObject.GetComponent<StatUpgradeButton>();
            if (statBtn == null)
                statBtn = group.upgradeButton.gameObject.AddComponent<StatUpgradeButton>();

            // 2. statType 지정
            statBtn.statType = capturedGroup.type.ToString();

            // 3. 클릭 이벤트 제거 (연속/단일 모두 StatUpgradeButton에서 처리하므로 불필요)
            group.upgradeButton.onClick.RemoveAllListeners();
        }

        UpdateUI();
    }


    private void OnClickUpgrade(StatType type)
    {
        statsManager.StartStatLevelUpLoop(type.ToString());
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
