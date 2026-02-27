using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SLG.Buildings;

namespace SLG.UI
{
    public class BuildingPanelUI : MonoBehaviour
    {
        [Header("Refs")]
        public BuildingSystem buildingSystem;

        [Header("Building Id")]
        public string buildingId;

        [Header("UI")]
        public TMP_Text txtName;
        public TMP_Text txtCurrentLevel1;
        public TMP_Text txtCurrentLevel2;
        public TMP_Text txtNextLevel;
        public TMP_Text txtTimer;
        public TMP_Text txtTime; 
        public TMP_Text txtCost;
        public TMP_Text txtReqHQ;
        public TMP_Text txtCurrentPower;
        public TMP_Text txtNextPower;
        public TMP_Text txtMaterials;

        public Button btnUpgrade;

        private void Awake()
        {
            if (btnUpgrade) btnUpgrade.onClick.AddListener(OnClickUpgrade);
        }

        private void OnEnable()
        {
            if (buildingSystem != null)
            {
                buildingSystem.OnDataChanged += Refresh;
                buildingSystem.OnUpgradeCompleted += OnUpgradeCompleted;
            }
            Refresh();
        }

        private void OnDisable()
        {
            if (buildingSystem != null)
            {
                buildingSystem.OnDataChanged -= Refresh;
                buildingSystem.OnUpgradeCompleted -= OnUpgradeCompleted;
            }
        }

        private void Update()
        {
            if (buildingSystem == null || !buildingSystem.IsLoaded) return;

            long remain = buildingSystem.GetRemainingSec(buildingId);
            if (txtTimer)
            {
                txtTimer.text = remain > 0 ? $"Remained Time: {remain}s" : "";
            }

            if (btnUpgrade)
            {
                btnUpgrade.interactable = buildingSystem.CanStartUpgrade(buildingId, out _);
            }
        }

        private void Refresh()
        {
            if (buildingSystem == null || !buildingSystem.IsLoaded) return;

            var preview = buildingSystem.GetUpgradePreview(buildingId);

            if (txtName) txtName.text = buildingId;
            if (txtCurrentLevel1) txtCurrentLevel1.text = $"Lv {preview.currentLevel}";
            if (txtCurrentLevel2) txtCurrentLevel2.text = $"Lv {preview.currentLevel}";
            if (txtNextLevel) txtNextLevel.text = $"Lv {preview.nextLevel}";
            if (preview.hasNext)
            {
                txtCost.text = $"{preview.requiredGold}";
                txtTime.text = $"{preview.durationSec}s";
                txtReqHQ.text = preview.requiredTownHallLevel > 0
                    ? $"{preview.requiredTownHallLevel}"
                    : "-";
                txtMaterials.text = preview.requiredMaterials.ToString();
                txtCurrentPower.text = $"{preview.currentPower}";
                txtNextPower.text = $"{preview.nextPower}";
            }
            else
            {
                txtCost.text = "Max";
                txtTime.text = "";
                txtReqHQ.text = "";
                txtCurrentPower.text = $"{preview.currentPower}";
            }

        }

        private void OnClickUpgrade()
        {
            if (buildingSystem == null) return;

            buildingSystem.StartUpgrade(buildingId,
                onSuccess: () => { },
                onFail: r =>
                {
                    Debug.LogWarning($"Upgrade failed: {r}");
                });
        }

        private void OnUpgradeCompleted(string id, int newLv)
        {
            if (id != buildingId) return;
            Debug.Log($"Upgrade completed: {id} -> Lv{newLv}");
        }
    }
}
