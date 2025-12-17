using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GodStatLevel
{
    public int level;          // 스탯 레벨
    public float value;        // 스탯 값 (공격력, 속도, 크리티컬율 등)
    public int goldRequired;   // 업그레이드에 필요한 골드
}

[Serializable]
public class GodStatsData
{
    public List<GodStatLevel> attackPowerLevels;
    public List<GodStatLevel> attackSpeedLevels;
    public List<GodStatLevel> criticalRateLevels;
    public List<GodStatLevel> criticalDamageLevels;
}
