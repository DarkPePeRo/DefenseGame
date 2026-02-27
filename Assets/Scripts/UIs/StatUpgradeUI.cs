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
        public TextMeshProUGUI valueTextNext;
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
            group.goldText.text = FormatGold(gold);
            group.valueTextNext.text = $"{value + 10}";

            bool isAffordable = PlayerCurrency.Instance.gold.amount >= gold;

            if (group.type == StatType.criticalRate) 
            {
                group.valueText.text = $"{value} %";
            }
            if (group.type == StatType.criticalDamage)
            {
                group.valueText.text = $"{value} %";
            }
            if (!isAffordable)
            {
                group.upgradeButton.interactable = false;

                if (group.maxedSprite != null)
                {
                    group.upgradeButton.image.sprite = group.maxedSprite;
                    group.goldText.color = Color.red;
                }

                Debug.Log("골드가 부족");
                continue;
            }
            else
            {
                group.upgradeButton.interactable = true;

                if (group.normalSprite != null)
                    group.upgradeButton.image.sprite = group.normalSprite;

                group.goldText.color = new Color(34f/255f,60f/255f,56f/255f,1);
            }
        }
    }
    private string FormatGold(int gold)
    {
        long value = (long)gold;

        if (value >= 1_000_000)
        {
            long manUnit = value / 10_000;      // 만 단위
            long remainder = value % 10_000;    // 나머지

            if (remainder == 0)
                return $"{manUnit}만";
            else
                return $"{manUnit}만{remainder}";
        }
        else if (value >= 1_000)
        {
            return value.ToString("N0");
        }
        else
        {
            return value.ToString();
        }
    }
}
