using System.Collections.Generic;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    public static GachaManager Instance;

    [Header("기본 설정")]
    public string bannerId = "weapon_normal_banner";
    public string summonLevelGroup = "equipment";
    public string ticketItemId = "ticket_equipment";

    public SummonLevelState CurrentSummonLevelState { get; private set; }
    public Dictionary<string, int> CurrentItemStacks { get; private set; } = new();

    private bool isRequesting;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        LoadInitData();
    }

    public void LoadInitData()
    {
        if (isRequesting) return;
        isRequesting = true;

        GachaService.GetGachaInitData(OnInitDataSuccess, OnRequestFail);
    }

    public void PullOne()
    {
        ExecutePull(1);
    }

    public void PullTen()
    {
        ExecutePull(10);
    }

    private void ExecutePull(int count)
    {
        if (isRequesting) return;
        isRequesting = true;

        GachaService.ExecutePull(
            bannerId,
            count,
            summonLevelGroup,
            ticketItemId,
            OnPullSuccess,
            OnRequestFail
        );
    }

    private void OnInitDataSuccess(GetGachaInitDataResponse response)
    {
        isRequesting = false;

        CurrentSummonLevelState = response.summonLevelState ?? new SummonLevelState
        {
            equipment = new SummonGroupState { level = 1, exp = 0 }
        };

        CurrentItemStacks = response.itemStacks ?? new Dictionary<string, int>();

        Debug.Log("[Gacha] InitData Load Success");
        Debug.Log($"[Gacha] Equipment Summon Level = {CurrentSummonLevelState.equipment.level}, Exp = {CurrentSummonLevelState.equipment.exp}");

        if (CurrentItemStacks.TryGetValue("ticket_equipment", out int ticket))
            Debug.Log($"[Gacha] ticket_equipment = {ticket}");

        EquipmentInventoryService.Instance.ApplyServerSnapshot(
            response.itemStacks,
            response.summonLevelState
        );
    }

    private void OnPullSuccess(ExecuteGachaPullResponse response)
    {
        isRequesting = false;

        CurrentSummonLevelState = response.summonLevelState;
        CurrentItemStacks = response.itemStacks ?? new Dictionary<string, int>();

        Debug.Log($"[Gacha] Pull Success. Result Count = {response.results.Count}");

        for (int i = 0; i < response.results.Count; i++)
        {
            var r = response.results[i];
            Debug.Log($"[Gacha] #{i + 1} grade={r.grade}, itemId={r.itemId}");
        }

        if (CurrentSummonLevelState != null && CurrentSummonLevelState.equipment != null)
        {
            Debug.Log($"[Gacha] Summon Level = {CurrentSummonLevelState.equipment.level}, Exp = {CurrentSummonLevelState.equipment.exp}");
        }

        EquipmentInventoryService.Instance.ApplyServerSnapshot(
            response.itemStacks,
            response.summonLevelState
        );
        PlayResultSequence(response.results);
    }

    private void OnRequestFail(string error)
    {
        isRequesting = false;
        Debug.LogError("[Gacha] Request Failed: " + error);
    }

    private void PlayResultSequence(List<GachaRollResult> results)
    {
        // 지금은 로그만 남기고
        // 나중에 여기서 10연차 연출 붙이면 됨
        Debug.Log("[Gacha] Play Result Sequence Start");
    }

    public int GetItemCount(string itemId)
    {
        if (CurrentItemStacks == null) return 0;
        return CurrentItemStacks.TryGetValue(itemId, out int count) ? count : 0;
    }
}