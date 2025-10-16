// Assets/Scripts/Data/EnemyDatabaseStructures.cs

using System.Collections.Generic;

/// <summary>
/// Estruturas de dados para serialização de inimigos
/// IMPORTANTE: Este arquivo deve estar FORA da pasta Editor para ser acessível em runtime
/// </summary>

[System.Serializable]
public class EnemyDatabase
{
    public List<EnemyData> druids = new List<EnemyData>();
    public List<EnemyData> warriors = new List<EnemyData>();
    public List<EnemyData> monsters = new List<EnemyData>();
    public List<EnemyData> bosses = new List<EnemyData>();
    public StatisticsData statistics;
}

[System.Serializable]
public class EnemyData
{
    public string name;
    public string assetPath;
    public int maxHp;
    public int maxMp;
    public int defense;
    public float speed;
    public List<ActionData> actions = new List<ActionData>();
}

[System.Serializable]
public class ActionData
{
    public string name;
    public int manaCost;
    public string targetType;
    public int effectCount;
    public string primaryEffectType;
    public int power;
    public string statusEffect;
    public int statusDuration;
    public int statusPower;
}

[System.Serializable]
public class StatisticsData
{
    public CategoryStats normalEnemies;
    public CategoryStats bosses;
}

[System.Serializable]
public class CategoryStats
{
    public int count;
    public float avgHp;
    public float avgMp;
    public float avgDefense;
    public float avgSpeed;
    public int minHp;
    public int maxHp;
}