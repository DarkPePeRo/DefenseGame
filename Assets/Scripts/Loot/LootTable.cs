// LootTable.cs
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LootEntry
{
    public string itemId;                 // "C4_Weapon", "EnhanceStone"
    [Range(0f, 100f)] public float chancePercent;  // 드랍 확률(%) - 항목별 독립 롤
    public Vector2Int amountRange = new Vector2Int(1, 1); // [min,max]
    public bool highValue = false;        // 고가치(서버 롤 대상) 표시용(선택)
}

[CreateAssetMenu(menuName = "Game/Loot Table")]
public class LootTable : ScriptableObject
{
    public string lootTableId = "Default";      // "Skeleton", "GolemBoss" 등
    public List<LootEntry> entries = new();
}
