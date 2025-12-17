// LootDropper.cs
using UnityEngine;

public class LootDropper : MonoBehaviour
{
    public MonsterDefinition def;
    public string LootTableId => def ? def.lootTableId : null;
    public int GoldReward => def ? def.goldReward : 0;
    public bool IsBoss => def && def.isBoss;
}
