using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tipos de observações comportamentais do jogador
/// </summary>
public enum BehaviorTriggerType
{

    PlayerDeath,              // 0
    LowHealthNoCure,         // 1
    NoDamageReceived,        // 2
    ItemExhausted,           // 3
    LowCoinsUnvisitedShops,  // 4
    NoDefensiveSkills,       // 5
    RepeatedBossDeath,       // 6
    ShopIgnored,             // 7
    BattleEasyVictory,       // 8
    AllSkillsUseMana,        // 9
    LowManaStreak,           // 10
    ZeroManaStreak,          // 11
    SingleSkillCarry,        // 12
    FrequentLowHP,           // 13
    WeakSkillIgnored,        // 14
    AlwaysOutsped,           // 15
    AlwaysFirstTurn,         // 16
    StrugglesAgainstTanks,   // 17
    StrugglesAgainstFast,    // 18
    StrugglesAgainstSwarms,  // 19
    AlwaysDiesEarly,         // 20
    AlwaysDiesLate,          // 21
    DeathByChipDamage,       // 22
    OneHitKOVulnerable,      // 23
    ExpensiveSkillsOnly,     // 24
    NoAOEDamage,             // 25
    BrokeAfterShopping,      // 26
    RanOutOfConsumables,     // 27
    ConsumableDependency,     // 28
    
    DefaultSessionOffer     //100
}

/// <summary>
/// Dados específicos para cada tipo de observação
/// </summary>
[System.Serializable]
public class BehaviorObservation
{
    public BehaviorTriggerType triggerType;
    public DateTime timestamp;
    public string mapName;
    public int sessionCount;
    
    // Dados específicos
    public Dictionary<string, object> data = new Dictionary<string, object>();
    
    public BehaviorObservation(BehaviorTriggerType type, string map)
    {
        triggerType = type;
        timestamp = DateTime.Now;
        mapName = map;
        sessionCount = 1;
    }
    
    public void IncrementSession()
    {
        sessionCount++;
        timestamp = DateTime.Now;
    }
    
    public void SetData(string key, object value)
    {
        data[key] = value;
    }
    
    public T GetData<T>(string key, T defaultValue = default(T))
    {
        if (data.TryGetValue(key, out object value) && value is T)
        {
            return (T)value;
        }
        return defaultValue;
    }
    
    public bool HasData(string key)
    {
        return data.ContainsKey(key);
    }
}

/// <summary>
/// Dados de comportamento específicos para batalha
/// </summary>
[System.Serializable]
public class BattleBehaviorData
{
    public Dictionary<string, int> skillUsageCount = new Dictionary<string, int>();
    public Dictionary<string, int> enemyDamageDealt = new Dictionary<string, int>();
    
        
    public Dictionary<string, int> skillDamageDealt = new Dictionary<string, int>();
    
    public List<string> turnOrder = new List<string>(); 
    
    public List<int> hitsReceived = new List<int>();
    public int turnOfDeath = -1; 
    
    public int tankEnemiesCount = 0;  
    public int fastEnemiesCount = 0;  
    public int totalEnemiesInBattle = 0;
    
    public int totalActionsUsed = 0;
    public int startingHP;
    public int startingMP;
    public int endingHP;
    public int endingMP;
    public bool playerDied = false;
    public List<string> enemiesInBattle = new List<string>();
    public List<string> unusedSkills = new List<string>();
    
    public void Reset()
    {
        skillUsageCount.Clear();
        enemyDamageDealt.Clear();
        skillDamageDealt.Clear();
        turnOrder.Clear();
        hitsReceived.Clear();
        
        turnOfDeath = -1;
        tankEnemiesCount = 0;
        fastEnemiesCount = 0;
        totalEnemiesInBattle = 0;
        totalActionsUsed = 0;
        playerDied = false;
        enemiesInBattle.Clear();
        unusedSkills.Clear();
    }
    
    public void RecordSkillUsage(string skillName)
    {
        if (skillUsageCount.ContainsKey(skillName))
            skillUsageCount[skillName]++;
        else
            skillUsageCount[skillName] = 1;
            
        totalActionsUsed++;
    }
    
    public void RecordSkillDamage(string skillName, int damage)
    {
        if (skillDamageDealt.ContainsKey(skillName))
            skillDamageDealt[skillName] += damage;
        else
            skillDamageDealt[skillName] = damage;
    }
    
    public void RecordEnemyDamage(string enemyName, int damage)
    {
        if (enemyDamageDealt.ContainsKey(enemyName))
            enemyDamageDealt[enemyName] += damage;
        else
            enemyDamageDealt[enemyName] = damage;
    }
    
    public void RecordTurnOrder(string actor)
    {
        turnOrder.Add(actor);
    }
    
    public void RecordHitReceived(int damage)
    {
        hitsReceived.Add(damage);
    }
    
    public string GetMostUsedSkill()
    {
        string mostUsed = "";
        int maxCount = 0;
        
        foreach (var skill in skillUsageCount)
        {
            if (skill.Value > maxCount)
            {
                maxCount = skill.Value;
                mostUsed = skill.Key;
            }
        }
        
        return mostUsed;
    }
    
    public string GetHighestDamageSkill()
    {
        string highestDamage = "";
        int maxDamage = 0;
        
        foreach (var skill in skillDamageDealt)
        {
            if (skill.Value > maxDamage)
            {
                maxDamage = skill.Value;
                highestDamage = skill.Key;
            }
        }
        
        return highestDamage;
    }
    
    public float GetSkillDamagePercentage(string skillName)
    {
        int totalDamage = 0;
        foreach (var damage in skillDamageDealt.Values)
        {
            totalDamage += damage;
        }
        
        if (totalDamage == 0) return 0f;
        
        int skillDamage = skillDamageDealt.GetValueOrDefault(skillName, 0);
        return (float)skillDamage / totalDamage;
    }
    
    public string GetMostDamagingEnemy()
    {
        string mostDamaging = "";
        int maxDamage = 0;
        
        foreach (var enemy in enemyDamageDealt)
        {
            if (enemy.Value > maxDamage)
            {
                maxDamage = enemy.Value;
                mostDamaging = enemy.Key;
            }
        }
        
        return mostDamaging;
    }
    
    public bool IsSkillOverused(string skillName, float percentage = 0.5f)
    {
        if (totalActionsUsed == 0) return false;
        
        int skillCount = skillUsageCount.GetValueOrDefault(skillName, 0);
        float usage = (float)skillCount / totalActionsUsed;
        
        return usage >= percentage;
    }
    
    public float GetPlayerFirstTurnPercentage()
    {
        if (turnOrder.Count == 0) return 0f;
        
        int playerFirstCount = 0;
        for (int i = 0; i < turnOrder.Count; i++)
        {
            if (turnOrder[i] == "Player")
            {
                bool isFirst = true;
                for (int j = i - 1; j >= 0 && j >= i - 4; j--)
                {
                    if (turnOrder[j] == "Player")
                    {
                        isFirst = false;
                        break;
                    }
                }
                if (isFirst) playerFirstCount++;
            }
        }
        
        // divide pelo número de ciclos
        int estimatedTurns = Mathf.Max(1, turnOrder.Count / 4);
        return (float)playerFirstCount / estimatedTurns;
    }
    
    public float GetAverageHitSize()
    {
        if (hitsReceived.Count == 0) return 0f;
        
        int total = 0;
        foreach (int hit in hitsReceived)
        {
            total += hit;
        }
        
        return (float)total / hitsReceived.Count;
    }
}

[System.Serializable]
public class SessionBehaviorData
{
    public int consecutiveLowManaBattles = 0;
    public int consecutiveZeroManaBattles = 0;
    public List<string> bossDeathHistory = new List<string>();
    public Dictionary<string, int> mapCompletionCount = new Dictionary<string, int>();
    
    public List<float> recentBattleEndHPPercentages = new List<float>();
    
    public Dictionary<string, int> skillNeverUsedCount = new Dictionary<string, int>(); 
    
    public int victoriesWithConsumables = 0;
    public int totalVictories = 0;
    
    public void Reset()
    {
        consecutiveLowManaBattles = 0;
        consecutiveZeroManaBattles = 0;
    }
    
    public void RecordBossDeath(string bossName)
    {
        bossDeathHistory.Add($"{bossName}_{DateTime.Now.Ticks}");
        
        if (bossDeathHistory.Count > 10)
        {
            bossDeathHistory.RemoveAt(0);
        }
    }
    
    public bool HasRepeatedBossDeath(string bossName, int withinLastDeaths = 3)
    {
        int count = 0;
        for (int i = bossDeathHistory.Count - 1; i >= 0 && count < withinLastDeaths; i--)
        {
            if (bossDeathHistory[i].StartsWith(bossName + "_"))
            {
                count++;
                if (count >= 2) return true;
            }
        }
        return false;
    }
    
    public void RecordBattleEndHP(float hpPercentage)
    {
        recentBattleEndHPPercentages.Add(hpPercentage);
        
        if (recentBattleEndHPPercentages.Count > 5)
        {
            recentBattleEndHPPercentages.RemoveAt(0);
        }
    }
    
    public bool HasFrequentLowHPPattern(float threshold = 0.3f, int minBattles = 3)
    {
        if (recentBattleEndHPPercentages.Count < minBattles) return false;
        
        int lowHPCount = 0;
        foreach (float hp in recentBattleEndHPPercentages)
        {
            if (hp < threshold) lowHPCount++;
        }
        
        return lowHPCount >= minBattles;
    }
    
    public void RecordSkillNotUsed(string skillName)
    {
        if (skillNeverUsedCount.ContainsKey(skillName))
            skillNeverUsedCount[skillName]++;
        else
            skillNeverUsedCount[skillName] = 1;
    }
    
    public void ResetSkillUsageCounter(string skillName)
    {
        if (skillNeverUsedCount.ContainsKey(skillName))
            skillNeverUsedCount[skillName] = 0;
    }
    
    public bool IsSkillChronicallyIgnored(string skillName, int minBattles = 5)
    {
        return skillNeverUsedCount.GetValueOrDefault(skillName, 0) >= minBattles;
    }
}

/// <summary>
/// Container principal para todos os dados comportamentais
/// </summary>
[System.Serializable]
public class PlayerBehaviorProfile
{
    public List<BehaviorObservation> observations = new List<BehaviorObservation>();
    public BattleBehaviorData currentBattle = new BattleBehaviorData();
    public SessionBehaviorData session = new SessionBehaviorData();
    
    public void AddObservation(BehaviorObservation newObservation)
    {
        // Procura por observação similar existente
        var existing = observations.Find(obs => 
            obs.triggerType == newObservation.triggerType &&
            obs.mapName == newObservation.mapName &&
            AreObservationsSimilar(obs, newObservation));
            
        if (existing != null)
        {
            existing.IncrementSession();
            // Atualiza dados se necessário
            foreach (var kvp in newObservation.data)
            {
                existing.SetData(kvp.Key, kvp.Value);
            }
        }
        else
        {
            observations.Add(newObservation);
        }
        
        Debug.Log($"Comportamento registrado: {newObservation.triggerType} no mapa {newObservation.mapName}");
    }
    
    /// <summary>
    /// Verifica se duas observações são similares o suficiente para serem agrupadas
    /// </summary>
    private bool AreObservationsSimilar(BehaviorObservation obs1, BehaviorObservation obs2)
    {
        switch (obs1.triggerType)
        {
            // Eventos melhorados que comparam por skill
            case BehaviorTriggerType.SingleSkillCarry:
            case BehaviorTriggerType.WeakSkillIgnored:
                return obs1.GetData<string>("skillName") == obs2.GetData<string>("skillName");
        
            // Eventos que comparam por inimigo
            case BehaviorTriggerType.PlayerDeath:
                return obs1.GetData<string>("killerEnemy") == obs2.GetData<string>("killerEnemy");
        
            case BehaviorTriggerType.RepeatedBossDeath:
                return obs1.GetData<string>("bossName") == obs2.GetData<string>("bossName");
        
            default:
                return true; 
        }
    }
    
    /// <summary>
    /// Obtém todas as observações de um tipo específico
    /// </summary>
    public List<BehaviorObservation> GetObservationsByType(BehaviorTriggerType type)
    {
        return observations.FindAll(obs => obs.triggerType == type);
    }
}