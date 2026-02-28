// MonsterDeathHandler.cs
using System.Collections.Generic;
using UnityEngine;
using static LootManager;

[RequireComponent(typeof(MonsterHealth))]
[RequireComponent(typeof(LootDropper))]
public class MonsterDeathHandler : MonoBehaviour
{
    public LootTable lootTable;   // 프리팹에 몬스터별로 연결
    public MultiPrefabPool pool;          // 인스펙터 배치
    public WaveSystem wave;
    public PlayerCurrency currency;       // 골드 선반영 버퍼
    public LootManager lootManager;       // 중앙 매니저

    MonsterHealth hp; LootDropper drop;

    void Awake() {
        pool = GameObject.Find("PoolManager")?.GetComponent<MultiPrefabPool>();
        wave = GameObject.Find("WaveSystem")?.GetComponent<WaveSystem>();
        currency = GameObject.Find("PlayerCurrency")?.GetComponent<PlayerCurrency>();
        lootManager = GameObject.Find("LootManager")?.GetComponent<LootManager>();
        hp = GetComponent<MonsterHealth>(); 
        drop = GetComponent<LootDropper>(); 
    }
    void OnEnable() { hp.OnDied += HandleDeath; }
    void OnDisable() { hp.OnDied -= HandleDeath; }

    void HandleDeath(MonsterHealth _)
    {
        // 2) 드랍/연출 통지
        if (lootTable != null && lootManager != null)
            lootManager.OnMonsterKilled(lootTable, transform.position);

        // 3) 웨이브 통지
        if (wave != null && hp.def.monsterId != "BossA") wave.OnEnemyDefeated();
        else wave.OnBossDefeated();

        // 4) 풀 반환
        if (pool != null) pool.ReturnObject(gameObject);
        else gameObject.SetActive(false);
    }
}
