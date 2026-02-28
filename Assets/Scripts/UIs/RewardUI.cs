using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardUI : MonoBehaviour
{
    [Header("Refs")]
    public Transform content;  
    public GameObject rowPrefab; 
    public Button btnClaim;

    private Dictionary<string, (LootManager.LootResult data, TMP_Text text)> _rows
        = new Dictionary<string, (LootManager.LootResult, TMP_Text)>();

    private Action<List<LootManager.LootResult>> _onClaim;

    void Awake()
    {
        if (btnClaim) btnClaim.onClick.AddListener(OnClickClaim);
    }

    public void AddResults(List<LootManager.LootResult> results,
                           Action<List<LootManager.LootResult>> onClaim)
    {
        _onClaim = onClaim;

        foreach (var r in results)
        {
            if (_rows.TryGetValue(r.itemId, out var entry))
            {
                var newData = entry.data;
                newData.amount += r.amount;
                _rows[r.itemId] = (newData, entry.text);

                entry.text.text = $"{newData.itemId} x{newData.amount}";
            }
            else
            {
                var go = Instantiate(rowPrefab, content);
                var txt = go.GetComponentInChildren<TMP_Text>();
                txt.text = $"{r.itemId} x{r.amount}";
                _rows[r.itemId] = (r, txt);
            }
        }

    }

    void OnClickClaim()
    {
        var list = new List<LootManager.LootResult>();
        foreach (var kv in _rows)
            list.Add(kv.Value.data);

        _onClaim?.Invoke(list);

        foreach (Transform c in content) Destroy(c.gameObject);
        _rows.Clear();
        gameObject.SetActive(false);
    }
}
