using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public Dictionary<string, int> inventory = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void AddItem(string itemId, int count = 1)
    {
        if (!inventory.ContainsKey(itemId))
            inventory[itemId] = 0;

        inventory[itemId] += count;
        Debug.Log($"[Inventory] {itemId} È¹µæ x{count}");
    }
}