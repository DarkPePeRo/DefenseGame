using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if PLAYFAB
using PlayFab;
using PlayFab.ClientModels;
#endif

public class LootManager : MonoBehaviour
{
    [Tooltip("드랍 테이블(간단형). 프로젝트에 ScriptableObject 기반이 있다면 아래 SimpleLootTable 대신 교체")]
    public List<LootTable> lootTables = new();

    [Tooltip("드랍이 발생했을 때 화면에 띄울 VFX/아이콘 프리팹(선택)")]
    public GameObject lootPopupVfxPrefab;

    [Header("Batch Claim Settings")]
    [Tooltip("서버 정산 주기(초). 경과 시 1회 Claim 시도")]
    public float claimIntervalSeconds = 60f;

    [Tooltip("Kill 누적이 이 값에 도달하면 즉시 Claim 시도")]
    public int claimWhenKillsReach = 100;

    [Tooltip("클라 선반영 총액이 이 값 이상이면 즉시 Claim 시도")]
    public int claimWhenSoftLootReach = 5000;

    [Tooltip("오프라인 보상/과도 청구 방지용 상한(서버측도 동일 값으로 검증 권장)")]
    public int oneClaimMaxSoftLoot = 200000;

    [Header("Debug/Telemetry")]
    public bool verboseLog = false;

    // 내부 누적 상태
    private int _killsSinceClaim = 0;
    private int _softLootBuffered = 0; // 골드/재료 등 저가치 예측 드랍 총합
    private float _claimTimer = 0f;

    // 세션 시드(선택): 서버가 발급한 값을 받을 수 있으면 이용
    private string _sessionSeed = string.Empty;

    // 빠른 테이블 조회
    private readonly Dictionary<string, LootTable> _tableMap = new();

    public RewardUI rewardUI;

    void Awake()
    {
        DontDestroyOnLoad(gameObject); // 루트에 두고 유지
        BuildTableMap();
    }

    void Update()
    {
        _claimTimer += Time.deltaTime;

        // 배치 조건 충족 시 서버 정산 시도
        if (_claimTimer >= claimIntervalSeconds ||
            _killsSinceClaim >= claimWhenKillsReach ||
            _softLootBuffered >= claimWhenSoftLootReach)
        {
            TryClaimToServer();
        }
    }

    private void BuildTableMap()
    {
        _tableMap.Clear();
        foreach (var t in lootTables)
        {
            if (t == null || string.IsNullOrEmpty(t.lootTableId)) continue;
            _tableMap[t.lootTableId] = t;
        }
    }
    // LootManager 내부 어디든
    public void OnMonsterKilled(LootTable table, Vector3 worldPos)
    {
        if (table == null) return;

        var results = LootRoller.Roll(table);

        // 누적 UI에 추가(사용 중인 LootUI에 맞춰 호출)
        if (rewardUI != null)
            rewardUI.AddResults(results, claimed => ApplyLootResults(claimed, worldPos));
        else
            ApplyLootResults(results, worldPos); // UI가 없으면 기존처럼 즉시 지급

        // 배치 정산 카운터/버퍼 누적(원 코드 그대로 사용)
        _killsSinceClaim++;
        foreach (var r in results)
            if (r.itemId == "Gold") _softLootBuffered += r.amount;
    }


    public void ApplyLootResults(List<LootResult> results, Vector3 pos)
    {
        foreach (var r in results)
        {
            // 예시: 아이템 ID에 따라 처리 분기
            if (r.itemId == "Gold") PlayerCurrency.Instance.AddGoldBuffered(r.amount);
            else if (r.itemId == "EnhanceStone")
            {
                Debug.Log("EnhanceStone" + r.amount + " 개 획득");
            }
            else if (r.itemId == "C4_Sword")
            {
                Debug.Log("C4_Sword" + r.amount + " 개 획득");
            }
        }
    }
    public void TryClaimToServer()
    {
        if (_killsSinceClaim <= 0 && _softLootBuffered <= 0)
        {
            _claimTimer = 0f;
            return;
        }

        int claimSoft = Mathf.Min(_softLootBuffered, oneClaimMaxSoftLoot);
        int claimKills = _killsSinceClaim;

        _claimTimer = 0f;
        _killsSinceClaim = 0;
        _softLootBuffered -= claimSoft; // 선반영값에서 차감(서버 응답 후 보정 가능)

        if (verboseLog)
            Debug.Log($"[LootManager] Claim start kills={claimKills} soft={claimSoft}");

#if PLAYFAB
        // 예시: CloudScript ClaimDrops 호출(서버는 기대값/상한 검증 후 확정 지급)
        var args = new Dictionary<string, object> {
            {"kills", claimKills},
            {"predictedSoft", claimSoft},
            {"sessionSeed", _sessionSeed}
        };

        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest {
            FunctionName = "ClaimDrops",
            FunctionParameter = args,
            GeneratePlayStreamEvent = true
        },
        result =>
        {
            // 서버 확정 결과
            // 기대값보다 낮게 확정되면 로컬 선반영을 서서히 보정
            // result.FunctionResult 예: { grantSoft:int, now:long }
            var payload = result.FunctionResult as Dictionary<string, object>;
            int granted = 0;
            if (payload != null && payload.TryGetValue("grantSoft", out var v) && v != null)
                granted = Convert.ToInt32(v);

            if (currency != null)
            {
                // 선반영과 실제가 차이나면 부드럽게 보정
                int delta = granted - claimSoft;
                if (delta != 0)
                    currency.SoftGoldSmoothAdjust(delta); // 구현: 숫자 애니로 부드럽게 증감
            }

            if (verboseLog)
                Debug.Log($"[LootManager] Claim ok, granted={granted}");
        },
        error =>
        {
            // 실패 시: 로컬 버퍼를 되돌려두고 재시도(네트워크 이슈)
            _softLootBuffered += claimSoft;
            _killsSinceClaim += claimKills;
            if (verboseLog)
                Debug.LogWarning($"[LootManager] Claim failed: {error.GenerateErrorReport()}");
        });
#else
        // 서버 연동이 아직 없을 때의 더미 처리(로컬 확정)
        if (PlayerCurrency.Instance != null)
             // 버퍼를 실제 값으로 확정시키는 메서드가 있다면 호출

        if (verboseLog)
            Debug.Log($"[LootManager] Claim simulated offline: +{claimSoft}");
#endif
    }

    /// <summary>
    /// 서버가 세션 시작 시 내려주는 시드를 세팅(선택).
    /// </summary>
    public void SetSessionSeed(string seed)
    {
        _sessionSeed = seed ?? string.Empty;
        if (verboseLog)
            Debug.Log($"[LootManager] SessionSeed set: {seed}");
    }

    #region Simple Loot Table (Sample)
    [Serializable]
    public class SimpleLootTable
    {
        public string lootTableId = "Default";
        [Range(0, 1f)] public float baseDropRate = 0.5f;
        [Range(0, 1f)] public float baseDropRateBoss = 1f;
        public int softAmountMin = 1;
        public int softAmountMax = 10;

        // 필요 시: 드랍 항목 리스트/가중치/상자/재료 등 확장
    }
    #endregion
    public struct LootResult { public string itemId; public int amount; }

    public static class LootRoller
    {
        // 항목별 '독립 확률'로 여러 개가 동시에 나올 수 있음
        public static List<LootResult> Roll(LootTable table)
        {
            var results = new List<LootResult>();
            if (table == null) return results;

            foreach (var e in table.entries)
            {
                if (e == null || string.IsNullOrEmpty(e.itemId)) continue;

                float roll = UnityEngine.Random.value * 100f; // 0~100
                if (roll <= e.chancePercent)
                {
                    int amt = UnityEngine.Random.Range(e.amountRange.x, e.amountRange.y + 1);
                    if (amt > 0) results.Add(new LootResult { itemId = e.itemId, amount = amt });
                }
            }
            return results;
        }
    }
}
