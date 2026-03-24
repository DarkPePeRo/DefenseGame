using System;
using System.Collections.Generic;

[Serializable]
public class ExecuteGachaPullRequest
{
    public string bannerId;
    public int pullCount;
    public string requestId;
    public string summonLevelGroup;
    public string ticketItemId;
}

[Serializable]
public class ExecuteGachaPullResponse
{
    public bool success;
    public List<GachaRollResult> results;
    public SummonLevelState summonLevelState;
    public Dictionary<string, int> itemStacks;
}

[Serializable]
public class GachaRollResult
{
    public string grade;
    public string itemId;
}

[Serializable]
public class GetGachaInitDataResponse
{
    public Dictionary<string, BannerPoolData> pools;
    public Dictionary<string, List<SummonLevelDef>> summonLevels;
    public SummonLevelState summonLevelState;
    public Dictionary<string, int> itemStacks;
}

[Serializable]
public class BannerPoolData
{
    public Dictionary<string, SummonLevelGradeTable> summonLevels;
    public Dictionary<string, List<WeightedItem>> gradeItems;
}

[Serializable]
public class SummonLevelGradeTable
{
    public Dictionary<string, int> gradeWeights;
}

[Serializable]
public class WeightedItem
{
    public string itemId;
    public int weight;
}

[Serializable]
public class SummonLevelState
{
    public SummonGroupState equipment;
}

[Serializable]
public class SummonGroupState
{
    public int level;
    public int exp;
}

[Serializable]
public class SummonLevelDef
{
    public int level;
    public int expToNext;
}