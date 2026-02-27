using System;
using System.Collections;
using UnityEngine;
using PlayFab;

namespace SLG.Buildings
{
    public class BuildingSystem : MonoBehaviour
    {
        [Header("Refs")]
        public BuildingConfigDB configDB;
        public PlayFabBuildingsService playFab;
        public SLG.Services.TimeSyncService timeSync;

        [Header("Options")]
        public bool singleBuilder = true;

        public BuildingsSave SaveData { get; private set; } = new BuildingsSave();
        public bool IsLoaded { get; private set; }

        private Coroutine _tickCo;

        public event Action<string> OnUpgradeStarted;
        public event Action<string, int> OnUpgradeCompleted;
        public event Action OnDataChanged;

        [Serializable]
        public class BuildingUpgradePreview
        {
            public string buildingId;

            // 현재 상태
            public int currentLevel;
            public int currentPower;
            public bool isUpgrading;
            public long remainingSec;

            // 다음 단계(업그레이드 가능할 때만 의미 있음)
            public bool hasNext;
            public int nextLevel;
            public int requiredGold;
            public int requiredTownHallLevel;
            public int durationSec;
            public int requiredMaterials;

            public int nextPower;
            public int powerIncrease;

            // 업그레이드 가능 여부
            public bool canUpgrade;
            public string reason; // CanStartUpgrade reason
        }

        private void Start()
        {
            LoadAll();
        }

        public void LoadAll(Action onSuccess = null, Action<PlayFabError> onError = null)
        {
            if (configDB != null) configDB.BuildCache();

            playFab.Load(
                (save, isNew) =>
                {
                    if (isNew)
                    {
                        SaveData = CreateSeedAllLv0(configDB);

                        playFab.Save(SaveData,
                            () =>
                            {
                                FinishLoadCommon(onSuccess);
                            },
                            err => onError?.Invoke(err));

                        return;
                    }

                    // 기존 유저
                    SaveData = save ?? new BuildingsSave();

                    bool changed = EnsureAllBuildingsExist(SaveData, configDB);

                    IsLoaded = true;
                    ProcessCompletionsIfAny(forceSaveIfCompleted: false);

                    if (changed)
                    {
                        playFab.Save(SaveData, () =>
                        {
                            FinishLoadCommon(onSuccess);
                        }, err => onError?.Invoke(err));
                    }
                    else
                    {
                        FinishLoadCommon(onSuccess);
                    }
                },
                err => onError?.Invoke(err));
        }

        private void FinishLoadCommon(Action onSuccess)
        {
            IsLoaded = true;
            StartTick();
            OnDataChanged?.Invoke();
            onSuccess?.Invoke();
        }

        private BuildingsSave CreateSeedAllLv0(BuildingConfigDB db)
        {
            var seed = new BuildingsSave();

            if (db != null && db.configs != null)
            {
                for (int i = 0; i < db.configs.Count; i++)
                {
                    var cfg = db.configs[i];
                    if (cfg == null || string.IsNullOrEmpty(cfg.id)) continue;

                    var e = seed.GetOrCreate(cfg.id);
                    e.lv = 0;
                    e.st = (int)BuildingProgressState.Idle;
                    e.end = 0;
                }
            }

            seed.GetOrCreate("TownHall").lv = 1;

            return seed;
        }
        private bool EnsureAllBuildingsExist(BuildingsSave save, BuildingConfigDB db)
        {
            if (save == null || db == null || db.configs == null) return false;

            bool changed = false;
            for (int i = 0; i < db.configs.Count; i++)
            {
                var cfg = db.configs[i];
                if (cfg == null || string.IsNullOrEmpty(cfg.id)) continue;

                // 없으면 생성(lv0)
                bool exists = false;
                for (int j = 0; j < save.buildings.Count; j++)
                {
                    if (save.buildings[j].id == cfg.id) { exists = true; break; }
                }

                if (!exists)
                {
                    save.GetOrCreate(cfg.id); // 기본 lv0
                    changed = true;
                }
            }
            return changed;
        }

        private void StartTick()
        {
            if (_tickCo != null) StopCoroutine(_tickCo);
            _tickCo = StartCoroutine(TickRoutine());
        }

        private IEnumerator TickRoutine()
        {
            var wait = new WaitForSeconds(1f);
            while (true)
            {
                yield return wait;
                if (!IsLoaded) continue;

                bool completed = ProcessCompletionsIfAny(forceSaveIfCompleted: true);
                if (completed) OnDataChanged?.Invoke();
            }
        }

        private bool ProcessCompletionsIfAny(bool forceSaveIfCompleted)
        {
            long now = timeSync != null ? timeSync.ServerNowUnixSec : DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            bool anyCompleted = false;

            for (int i = 0; i < SaveData.buildings.Count; i++)
            {
                var b = SaveData.buildings[i];
                if (b.st != (int)BuildingProgressState.Upgrading) continue;
                if (b.end <= 0) continue;

                if (now >= b.end)
                {
                    b.lv = Mathf.Max(1, b.lv + 1);
                    b.st = (int)BuildingProgressState.Idle;
                    b.end = 0;

                    anyCompleted = true;
                    OnUpgradeCompleted?.Invoke(b.id, b.lv);
                }
            }

            if (anyCompleted && forceSaveIfCompleted)
                playFab.Save(SaveData, () => { }, _ => { });

            return anyCompleted;
        }
        public int GetCurrentPower(string buildingId)
        {
            var b = GetBuilding(buildingId);

            // lv0(미건설)은 전투력 0
            if (b.lv <= 0) return 0;

            // Lv1의 전투력은 "기본값"으로 처리
            // (원하면 config에 별도 basePower를 넣는 구조로 확장 가능)
            if (b.lv == 1) return 100;

            // 현재 레벨(LvN)의 전투력은 "Lv(N-1)->LvN" 업그레이드 단계의 powerAtNextLevel로 본다
            // 즉 index = (currentLevel - 1) - 1 = currentLevel - 2
            int index = b.lv - 2;

            var cfg = configDB.Get(buildingId);
            if (cfg == null) return 0;
            if (cfg.levels == null) return 0;
            if (index < 0 || index >= cfg.levels.Count) return 0;

            return cfg.levels[index].powerAtNextLevel;
        }

        public int GetNextPower(string buildingId)
        {
            var b = GetBuilding(buildingId);

            // 다음 단계가 존재할 때만
            if (!configDB.TryGetUpgradeStep(buildingId, b.lv, out var step, out _))
                return GetCurrentPower(buildingId);

            return step.powerAtNextLevel;
        }
        public BuildingUpgradePreview GetUpgradePreview(string buildingId)
        {
            var p = new BuildingUpgradePreview();
            p.buildingId = buildingId;

            var b = GetBuilding(buildingId);
            p.currentLevel = b.lv;

            p.isUpgrading = (b.st == (int)BuildingProgressState.Upgrading);
            p.remainingSec = p.isUpgrading ? GetRemainingSec(buildingId) : 0;

            // 전투력
            p.currentPower = GetCurrentPower(buildingId);

            // 다음 단계 정보
            if (configDB != null && configDB.TryGetUpgradeStep(buildingId, b.lv, out var step, out var maxLv) && step != null)
            {
                p.hasNext = true;
                p.nextLevel = b.lv + 1;
                p.requiredGold = step.goldCost;
                p.requiredTownHallLevel = step.requiredTownHallLv;
                p.durationSec = step.durationSec;
                p.requiredMaterials = step.requiredMaterials;

                p.nextPower = step.powerAtNextLevel;
                p.powerIncrease = p.nextPower - p.currentPower;
            }
            else
            {
                p.hasNext = false;
                p.nextLevel = b.lv;
                p.requiredGold = 0;
                p.requiredTownHallLevel = 0;
                p.durationSec = 0;

                p.nextPower = p.currentPower;
                p.powerIncrease = 0;
            }

            // 업그레이드 가능 여부
            p.canUpgrade = CanStartUpgrade(buildingId, out var reason);
            p.reason = reason;

            return p;
        }

        public BuildingStateEntry GetBuilding(string buildingId) => SaveData.GetOrCreate(buildingId);
        public bool CanStartUpgrade(string buildingId, out string reason)
        {
            reason = null;

            if (!IsLoaded)
            {
                reason = "NotLoaded";
                return false;
            }

            if (singleBuilder && SaveData.AnyUpgrading(out var other))
            {
                // 빌더 1명 제한
                reason = $"BuilderBusy:{other}";
                return false;
            }

            var b = GetBuilding(buildingId);

            if (b.st == (int)BuildingProgressState.Upgrading)
            {
                reason = "AlreadyUpgrading";
                return false;
            }

            if (!configDB.TryGetUpgradeStep(buildingId, b.lv, out var step, out var maxLv))
            {
                reason = $"MaxLevelOrNoConfig:{maxLv}";
                return false;
            }

            // 선행 조건: TownHall 레벨
            if (step.requiredTownHallLv > 0)
            {
                var th = GetBuilding("TownHall");

                if (th.lv < step.requiredTownHallLv)
                {
                    reason = $"NeedTownHallLv:{step.requiredTownHallLv}";
                    return false;
                }
            }

            if (PlayerCurrency.Instance.gold.amount < step.goldCost)
            {
                reason = "NotEnoughGold";
                return false;
            }

            return true;
        }
        public void StartUpgrade(string buildingId, Action onSuccess = null, Action<string> onFail = null)
        {
            if (!CanStartUpgrade(buildingId, out var reason))
            {
                onFail?.Invoke(reason);
                return;
            }

            var b = GetBuilding(buildingId);

            configDB.TryGetUpgradeStep(
                buildingId,
                b.lv,
                out var step,
                out _);

            // 골드 차감 (버퍼 저장 구조 전제)
            PlayerCurrency.Instance.AddGoldBuffered(-step.goldCost);

            // 서버 기준 시간 계산
            long now = timeSync != null
                ? timeSync.ServerNowUnixSec
                : DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            b.st = (int)BuildingProgressState.Upgrading;
            b.end = now + step.durationSec;

            // 서버 저장
            playFab.Save(
                SaveData,
                () =>
                {
                    OnUpgradeStarted?.Invoke(buildingId);
                    OnDataChanged?.Invoke();
                    onSuccess?.Invoke();
                },
                err =>
                {
                    // 저장 실패 시 롤백
                    b.st = (int)BuildingProgressState.Idle;
                    b.end = 0;

                    OnDataChanged?.Invoke();

                    onFail?.Invoke($"SaveFailed:{err.ErrorMessage}");
                });
        }
        public long GetRemainingSec(string buildingId)
        {
            var b = GetBuilding(buildingId);

            if (b.st != (int)BuildingProgressState.Upgrading || b.end <= 0)
                return 0;

            long now = timeSync != null
                ? timeSync.ServerNowUnixSec
                : DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            long remain = b.end - now;

            return Math.Max(0, remain);
        }

    }
}

