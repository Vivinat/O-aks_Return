using System.Collections.Generic;

/// <summary>
/// Estruturas de dados para serialização de BattleActions
/// </summary>

[System.Serializable]
public class BattleActionDatabase
{
    public List<BattleActionData> paladinActions = new List<BattleActionData>();
    public List<BattleActionData> rangerActions = new List<BattleActionData>();
    public List<BattleActionData> druidActions = new List<BattleActionData>();
    public List<BattleActionData> consumableItems = new List<BattleActionData>();
    public List<BattleActionData> otherActions = new List<BattleActionData>();
    public BattleActionStatistics statistics;
}

[System.Serializable]
public class BattleActionData
{
    public string actionName;
    public string assetPath;
    public string description;
    public string targetType;
    public int manaCost;
    public bool isConsumable;
    public int maxUses;
    public int shopPrice;
    public List<EffectData> effects = new List<EffectData>();
}

[System.Serializable]
public class EffectData
{
    public string effectType;
    public int power;
    public string statusEffect;
    public int statusDuration;
    public int statusPower;
    public bool hasSelfEffect;
    public string selfEffectType;
    public int selfEffectPower;
    public string selfStatusEffect;
    public int selfStatusDuration;
    public int selfStatusPower;
}

[System.Serializable]
public class BattleActionStatistics
{
    public int totalActions;
    public int consumableCount;
    public int nonConsumableCount;
    public float avgManaCost;
    public int minManaCost;
    public int maxManaCost;
    public float avgPower;
    public int minPower;
    public int maxPower;
    public Dictionary<string, int> targetTypeDistribution = new Dictionary<string, int>();
}