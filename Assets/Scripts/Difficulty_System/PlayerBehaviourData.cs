// Assets/Scripts/Analytics/PlayerBehaviorData.cs

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tipos de observações comportamentais do jogador
/// </summary>
public enum BehaviorTriggerType
{
    PlayerDeath,              // Jogador morreu em batalha
    SkillOveruse,            // Jogador usa muito uma skill específica
    LowHealthNoCure,         // Jogador com pouca vida e sem cura
    NoDamageReceived,        // Jogador não recebeu dano
    CriticalHealth,          // Jogador com vida crítica
    ItemExhausted,           // Item consumível esgotado
    LowCoinsUnvisitedShops,  // Poucas moedas com lojas disponíveis
    UnusedSkill,             // Skill não utilizada no nível
    NoDefensiveSkills,       // Falta de skills defensivas
    RepeatedBossDeath,       // Morte repetida no mesmo boss
    ShopIgnored,             // Ignorou itens na loja
    BossEasyVictory,         // Derrotou boss facilmente
    AllSkillsUseMana,        // Todas skills usam MP
    LowManaStreak,           // Sequência de batalhas com pouco MP
    ZeroManaStreak           // Sequência de batalhas com MP zerado
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
    public int sessionCount; // Quantas vezes foi observado
    
    // Dados específicos (usando Dictionary para flexibilidade)
    public Dictionary<string, object> data = new Dictionary<string, object>();
    
    public BehaviorObservation(BehaviorTriggerType type, string map)
    {
        triggerType = type;
        timestamp = DateTime.Now;
        mapName = map;
        sessionCount = 1;
    }
    
    /// <summary>
    /// Incrementa o contador e atualiza timestamp
    /// </summary>
    public void IncrementSession()
    {
        sessionCount++;
        timestamp = DateTime.Now;
    }
    
    /// <summary>
    /// Adiciona ou atualiza um dado específico
    /// </summary>
    public void SetData(string key, object value)
    {
        data[key] = value;
    }
    
    /// <summary>
    /// Recupera um dado específico
    /// </summary>
    public T GetData<T>(string key, T defaultValue = default(T))
    {
        if (data.TryGetValue(key, out object value) && value is T)
        {
            return (T)value;
        }
        return defaultValue;
    }
    
    /// <summary>
    /// Verifica se tem um dado específico
    /// </summary>
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
        totalActionsUsed = 0;
        playerDied = false;
        enemiesInBattle.Clear();
        unusedSkills.Clear();
    }
    
    /// <summary>
    /// Registra uso de uma skill
    /// </summary>
    public void RecordSkillUsage(string skillName)
    {
        if (skillUsageCount.ContainsKey(skillName))
            skillUsageCount[skillName]++;
        else
            skillUsageCount[skillName] = 1;
            
        totalActionsUsed++;
    }
    
    /// <summary>
    /// Registra dano causado por inimigo
    /// </summary>
    public void RecordEnemyDamage(string enemyName, int damage)
    {
        if (enemyDamageDealt.ContainsKey(enemyName))
            enemyDamageDealt[enemyName] += damage;
        else
            enemyDamageDealt[enemyName] = damage;
    }
    
    /// <summary>
    /// Retorna a skill mais usada
    /// </summary>
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
    
    /// <summary>
    /// Retorna o inimigo que mais causou dano
    /// </summary>
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
    
    /// <summary>
    /// Verifica se uma skill foi usada mais de X% das vezes
    /// </summary>
    public bool IsSkillOverused(string skillName, float percentage = 0.5f)
    {
        if (totalActionsUsed == 0) return false;
        
        int skillCount = skillUsageCount.GetValueOrDefault(skillName, 0);
        float usage = (float)skillCount / totalActionsUsed;
        
        return usage >= percentage;
    }
}

/// <summary>
/// Dados de sessão para rastrear comportamentos entre mapas
/// </summary>
[System.Serializable]
public class SessionBehaviorData
{
    public int consecutiveLowManaBattles = 0;
    public int consecutiveZeroManaBattles = 0;
    public List<string> bossDeathHistory = new List<string>();
    public Dictionary<string, int> mapCompletionCount = new Dictionary<string, int>();
    
    public void Reset()
    {
        consecutiveLowManaBattles = 0;
        consecutiveZeroManaBattles = 0;
    }
    
    /// <summary>
    /// Registra morte em boss
    /// </summary>
    public void RecordBossDeath(string bossName)
    {
        bossDeathHistory.Add($"{bossName}_{DateTime.Now.Ticks}");
        
        // Mantém apenas os últimos 10 registros
        if (bossDeathHistory.Count > 10)
        {
            bossDeathHistory.RemoveAt(0);
        }
    }
    
    /// <summary>
    /// Verifica se morreu no mesmo boss recentemente
    /// </summary>
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
    
    /// <summary>
    /// Adiciona uma nova observação ou incrementa uma existente
    /// </summary>
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
        // Lógica básica - pode ser expandida por tipo específico
        switch (obs1.triggerType)
        {
            case BehaviorTriggerType.SkillOveruse:
                return obs1.GetData<string>("skillName") == obs2.GetData<string>("skillName");
            case BehaviorTriggerType.PlayerDeath:
                return obs1.GetData<string>("killerEnemy") == obs2.GetData<string>("killerEnemy");
            case BehaviorTriggerType.RepeatedBossDeath:
                return obs1.GetData<string>("bossName") == obs2.GetData<string>("bossName");
            default:
                return true; // Para outros tipos, considera similar
        }
    }
    
    /// <summary>
    /// Obtém todas as observações de um tipo específico
    /// </summary>
    public List<BehaviorObservation> GetObservationsByType(BehaviorTriggerType type)
    {
        return observations.FindAll(obs => obs.triggerType == type);
    }
    
    /// <summary>
    /// Limpa observações antigas (mais de X dias)
    /// </summary>
    public void CleanOldObservations(int maxDays = 30)
    {
        DateTime cutoff = DateTime.Now.AddDays(-maxDays);
        observations.RemoveAll(obs => obs.timestamp < cutoff);
    }
}