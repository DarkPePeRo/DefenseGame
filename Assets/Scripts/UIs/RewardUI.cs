using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardUI : MonoBehaviour
{
    [Header("Refs")]
    public Transform content;          // 항목이 붙을 부모(VerticalLayoutGroup 등)
    public GameObject rowPrefab;       // 간단 Text 한 줄 프리팹
    public Button btnClaim;

    private Dictionary<string, (LootManager.LootResult data, TMP_Text text)> _rows
        = new Dictionary<string, (LootManager.LootResult, TMP_Text)>();

    private Action<List<LootManager.LootResult>> _onClaim;

    void Awake()
    {
        if (btnClaim) btnClaim.onClick.AddListener(OnClickClaim);
    }

    /// <summary>
    /// 새로운 결과가 나오면 누적 리스트에 반영
    /// </summary>
    public void AddResults(List<LootManager.LootResult> results,
                           Action<List<LootManager.LootResult>> onClaim)
    {
        _onClaim = onClaim;

        foreach (var r in results)
        {
            if (_rows.TryGetValue(r.itemId, out var entry))
            {
                // 기존 항목에 누적
                var newData = entry.data;
                newData.amount += r.amount;
                _rows[r.itemId] = (newData, entry.text);

                entry.text.text = $"{newData.itemId} x{newData.amount}";
            }
            else
            {
                // 새로운 항목 추가
                var go = Instantiate(rowPrefab, content);
                var txt = go.GetComponentInChildren<TMP_Text>();
                txt.text = $"{r.itemId} x{r.amount}";
                _rows[r.itemId] = (r, txt);
            }
        }

    }

    void OnClickClaim()
    {
        // 모든 누적 항목을 리스트로 반환
        var list = new List<LootManager.LootResult>();
        foreach (var kv in _rows)
            list.Add(kv.Value.data);

        _onClaim?.Invoke(list);

        // UI 초기화
        foreach (Transform c in content) Destroy(c.gameObject);
        _rows.Clear();
        gameObject.SetActive(false);
    }
}
