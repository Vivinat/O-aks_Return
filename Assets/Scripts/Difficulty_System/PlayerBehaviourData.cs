// Assets/Scripts/Analytics/PlayerBehaviorData.cs

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tipos de observações comportamentais do jogador
/// </summary>
public enum BehaviorTriggerType
{
    // EVENTOS ORIGINAIS
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
    
    // EVENTOS MELHORADOS
    SingleSkillCarry,        // 12
    FrequentLowHP,           // 13
    WeakSkillIgnored,        // 14
    
    // NOVOS EVENTOS - VELOCIDADE/ATB
    AlwaysOutsped,           // 15
    AlwaysFirstTurn,         // 16
    
    // NOVOS EVENTOS - TIPO DE INIMIGO
    StrugglesAgainstTanks,   // 17
    StrugglesAgainstFast,    // 18
    StrugglesAgainstSwarms,  // 19
    
    // NOVOS EVENTOS - PADRÕES DE MORTE
    AlwaysDiesEarly,         // 20
    AlwaysDiesLate,          // 21
    DeathByChipDamage,       // 22
    
    // NOVOS EVENTOS - ONE-SHOT
    OneHitKOVulnerable,      // 23
    
    // NOVOS EVENTOS - BUILD
    ExpensiveSkillsOnly,     // 24
    NoAOEDamage,             // 25
    
    // NOVOS EVENTOS - RECURSOS
    BrokeAfterShopping,      // 26
    RanOutOfConsumables,     // 27
    ConsumableDependency     // 28
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
    
        
    // NOVOS: Rastreamento de dano por skill
    public Dictionary<string, int> skillDamageDealt = new Dictionary<string, int>();
    
    // NOVOS: Rastreamento de ordem de turnos
    public List<string> turnOrder = new List<string>(); // "Player" ou nome do inimigo
    
    // NOVOS: Rastreamento de hits individuais
    public List<int> hitsReceived = new List<int>();
    public int turnOfDeath = -1; // -1 = não morreu
    
    // NOVOS: Contadores de tipo de inimigo
    public int tankEnemiesCount = 0;  // HP > 100
    public int fastEnemiesCount = 0;  // Speed > 5
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
    /// NOVO: Registra dano causado por skill específica
    /// </summary>
    public void RecordSkillDamage(string skillName, int damage)
    {
        if (skillDamageDealt.ContainsKey(skillName))
            skillDamageDealt[skillName] += damage;
        else
            skillDamageDealt[skillName] = damage;
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
    /// NOVO: Registra ordem de ação
    /// </summary>
    public void RecordTurnOrder(string actor)
    {
        turnOrder.Add(actor);
    }
    
    /// <summary>
    /// NOVO: Registra hit recebido
    /// </summary>
    public void RecordHitReceived(int damage)
    {
        hitsReceived.Add(damage);
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
    /// NOVO: Retorna a skill que causou mais dano
    /// </summary>
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
    
    /// <summary>
    /// NOVO: Retorna percentual de dano de uma skill
    /// </summary>
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
    
    /// <summary>
    /// NOVO: Calcula percentual de turnos que o jogador foi primeiro
    /// </summary>
    public float GetPlayerFirstTurnPercentage()
    {
        if (turnOrder.Count == 0) return 0f;
        
        int playerFirstCount = 0;
        for (int i = 0; i < turnOrder.Count; i++)
        {
            if (turnOrder[i] == "Player")
            {
                // Verifica se é o primeiro do turno (não tem nenhum antes dele no mesmo ciclo)
                bool isFirst = true;
                for (int j = i - 1; j >= 0 && j >= i - 4; j--) // Olha até 4 ações atrás
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
        
        // Estimativa grosseira: divide pelo número de "ciclos"
        int estimatedTurns = Mathf.Max(1, turnOrder.Count / 4);
        return (float)playerFirstCount / estimatedTurns;
    }
    
    /// <summary>
    /// NOVO: Calcula dano médio dos hits recebidos
    /// </summary>
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
    
    // NOVOS: Histórico de HP ao fim das batalhas
    public List<float> recentBattleEndHPPercentages = new List<float>();
    
    // NOVOS: Histórico de uso de skills
    public Dictionary<string, int> skillNeverUsedCount = new Dictionary<string, int>(); // Quantas batalhas cada skill não foi usada
    
    // NOVOS: Histórico de vitórias com consumíveis
    public int victoriesWithConsumables = 0;
    public int totalVictories = 0;
    
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
    
    /// <summary>
    /// NOVO: Registra HP ao final da batalha
    /// </summary>
    public void RecordBattleEndHP(float hpPercentage)
    {
        recentBattleEndHPPercentages.Add(hpPercentage);
        
        // Mantém apenas as últimas 5 batalhas
        if (recentBattleEndHPPercentages.Count > 5)
        {
            recentBattleEndHPPercentages.RemoveAt(0);
        }
    }
    
    /// <summary>
    /// NOVO: Verifica se há padrão de HP baixo
    /// </summary>
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
    
    /// <summary>
    /// NOVO: Registra skill não usada
    /// </summary>
    public void RecordSkillNotUsed(string skillName)
    {
        if (skillNeverUsedCount.ContainsKey(skillName))
            skillNeverUsedCount[skillName]++;
        else
            skillNeverUsedCount[skillName] = 1;
    }
    
    /// <summary>
    /// NOVO: Reseta contador de skill (quando ela é usada)
    /// </summary>
    public void ResetSkillUsageCounter(string skillName)
    {
        if (skillNeverUsedCount.ContainsKey(skillName))
            skillNeverUsedCount[skillName] = 0;
    }
    
    /// <summary>
    /// NOVO: Verifica se skill é cronicamente ignorada
    /// </summary>
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