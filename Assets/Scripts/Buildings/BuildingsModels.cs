// =======================================
// File: BuildingsModels.cs (수정)
// Folder: Assets/Scripts/Buildings/
// =======================================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SLG.Buildings
{
    public enum BuildingProgressState
    {
        Idle = 0,
        Upgrading = 1
    }

    [Serializable]
    public class BuildingStateEntry
    {
        public string id;
        public int lv;     // 0 = 미건설, 1~ = 건설됨
        public int st;     // (int)BuildingProgressState
        public long end;   // unix seconds
    }

    [Serializable]
    public class BuildingsSave
    {
        public List<BuildingStateEntry> buildings = new List<BuildingStateEntry>();

        // 기본은 lv=0으로 생성 (요구사항 반영)
        public BuildingStateEntry GetOrCreate(string id)
        {
            for (int i = 0; i < buildings.Count; i++)
                if (buildings[i].id == id) return buildings[i];

            var e = new BuildingStateEntry
            {
                id = id,
                lv = 0,
                st = (int)BuildingProgressState.Idle,
                end = 0
            };
            buildings.Add(e);
            return e;
        }

        public bool AnyUpgrading(out string buildingId)
        {
            for (int i = 0; i < buildings.Count; i++)
            {
                if (buildings[i].st == (int)BuildingProgressState.Upgrading)
                {
                    buildingId = buildings[i].id;
                    return true;
                }
            }
            buildingId = null;
            return false;
        }
    }

    public static class BuildingsJson
    {
        public static string ToJson(BuildingsSave save) => JsonUtility.ToJson(save);

        public static BuildingsSave FromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return new BuildingsSave();
            try { return JsonUtility.FromJson<BuildingsSave>(json) ?? new BuildingsSave(); }
            catch { return new BuildingsSave(); }
        }
    }
}
