using System;
using System.Collections.Generic;
using UnityEngine;

namespace SLG.Buildings
{
    [Serializable]
    public class BuildingLevelConfig
    {
        public int goldCost;
        public int durationSec;
        public int requiredTownHallLv; // 0이면 제한 없음
        public int powerAtNextLevel;
        public int requiredMaterials; 
    }

    [Serializable]
    public class BuildingConfig
    {
        public string id; // "TownHall", "LumberMill" ...
        public int maxLevel = 20;
        public List<BuildingLevelConfig> levels = new List<BuildingLevelConfig>();
        // levels[0] = Lv1->Lv2, levels[1] = Lv2->Lv3 ... 업그레이드 "단계" 기준
    }

    [CreateAssetMenu(menuName = "SLG/BuildingConfigDB", fileName = "BuildingConfigDB")]
    public class BuildingConfigDB : ScriptableObject
    {
        public List<BuildingConfig> configs = new List<BuildingConfig>();

        private Dictionary<string, BuildingConfig> _map;

        public void BuildCache()
        {
            _map = new Dictionary<string, BuildingConfig>(StringComparer.Ordinal);
            for (int i = 0; i < configs.Count; i++)
            {
                var c = configs[i];
                if (c == null || string.IsNullOrEmpty(c.id)) continue;
                _map[c.id] = c;
            }
        }

        public BuildingConfig Get(string id)
        {
            if (_map == null) BuildCache();
            _map.TryGetValue(id, out var cfg);
            return cfg;
        }

        // currentLevel -> nextLevel 비용/시간
        public bool TryGetUpgradeStep(string id, int currentLevel, out BuildingLevelConfig step, out int maxLevel)
        {
            step = null;
            maxLevel = 0;

            var cfg = Get(id);
            if (cfg == null) return false;

            maxLevel = cfg.maxLevel;
            if (currentLevel >= cfg.maxLevel) return false;

            int index = currentLevel - 1; // Lv1->2 는 index 0
            if (index < 0 || index >= cfg.levels.Count) return false;

            step = cfg.levels[index];
            return step != null;
        }
    }
}
