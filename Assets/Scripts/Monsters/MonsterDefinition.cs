// MonsterDefinition.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Monster Definition")]
public class MonsterDefinition : ScriptableObject
{
    public string monsterId;       // "Skeleton_A"
    [Header("Stats")]
    public float baseHP = 100;
    public float moveSpeed = 1.2f;
    public int goldReward = 5;

    [Header("Loot/Flags")]
    public string lootTableId;
    public bool isBoss = false;
}
