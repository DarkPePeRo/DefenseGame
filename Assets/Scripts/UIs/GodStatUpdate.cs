using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodStatUpdate : MonoBehaviour
{
    private int _lastGold = -1; 
    private void Update()
    {
        if (PlayerCurrency.Instance == null) return;

        int currentGold = PlayerCurrency.Instance.gold.amount;
        if (currentGold != _lastGold)
        {
            _lastGold = currentGold;
            StatUpgradeUI.Instance.UpdateUI();
        }
    }
    private void OnEnable()
    {
        if (PlayerCurrency.Instance != null)
            PlayerCurrency.Instance.OnGoldChanged += HandleGoldChanged;
        
        StatUpgradeUI.Instance.UpdateUI();
    }

    private void OnDisable()
    {
        if (PlayerCurrency.Instance != null)
            PlayerCurrency.Instance.OnGoldChanged -= HandleGoldChanged;
    }

    private void HandleGoldChanged(int currentGold)
    {
        StatUpgradeUI.Instance.UpdateUI();
    }
}
