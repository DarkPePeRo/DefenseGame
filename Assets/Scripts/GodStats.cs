using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GodStatLevel
{
    public int level;          // ���� ����
    public float value;        // ���� �� (���ݷ�, �ӵ�, ũ��Ƽ���� ��)
    public int goldRequired;   // ���׷��̵忡 �ʿ��� ���
}

[Serializable]
public class GodStatsData
{
    public List<GodStatLevel> attackPowerLevels;
    public List<GodStatLevel> attackSpeedLevels;
    public List<GodStatLevel> criticalRateLevels;
    public List<GodStatLevel> criticalDamageLevels;
}
