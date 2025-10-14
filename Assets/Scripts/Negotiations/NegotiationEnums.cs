// Assets/Scripts/Negotiation/NegotiationEnums.cs (REBALANCED)

using UnityEngine;

/// <summary>
/// Tipos de cartas de negociação
/// </summary>
public enum NegotiationCardType
{
    Fixed,                  // Carta fixa (apenas texto)
    AttributeAndIntensity,  // Permite escolher atributo e intensidade
    IntensityOnly          // Permite apenas escolher intensidade
}

/// <summary>
/// Atributos que podem ser modificados nas cartas
/// </summary>
public enum CardAttribute
{
    // Atributos do Jogador/Personagem
    PlayerMaxHP,
    PlayerMaxMP,
    PlayerDefense,
    PlayerSpeed,
    
    // Atributos de Ações do Jogador (NOVOS E ESPECÍFICOS)
    PlayerActionPower,
    PlayerActionManaCost,
    PlayerOffensiveActionPower,    // NOVO: Apenas skills ofensivas
    PlayerDefensiveActionPower,    // NOVO: Apenas skills defensivas/cura
    PlayerAOEActionPower,          // NOVO: Apenas skills de área
    PlayerSingleTargetActionPower, // NOVO: Apenas skills single-target
    
    // Atributos de Inimigos (Específicos)
    EnemyMaxHP,
    EnemyMaxMP,
    EnemyDefense,
    EnemySpeed,
    EnemyActionPower,
    EnemyActionManaCost,
    EnemyOffensiveActionPower,     // NOVO
    EnemyAOEActionPower,           // NOVO
    
    // Atributos Gerais
    CoinsEarned,
    ShopPrices
}

/// <summary>
/// Intensidades disponíveis para modificações (REBALANCEADAS - Máximo 30%)
/// </summary>
public enum CardIntensity
{
    VeryLow,    // ±3-5 (~3-5% de 100 HP base)
    Low,        // ±8-10 (~8-10%)
    Medium,     // ±15-18 (~15-18%)
    High,       // ±22-25 (~22-25%)
    VeryHigh    // ±28-30 (~28-30% - MÁXIMO)
}

/// <summary>
/// Classe auxiliar para conversão de intensidade em valores (REBALANCEADA)
/// </summary>
public static class IntensityHelper
{
    /// <summary>
    /// Retorna valor base para a intensidade
    /// </summary>
    public static int GetValue(CardIntensity intensity)
    {
        switch (intensity)
        {
            case CardIntensity.VeryLow: return 5;
            case CardIntensity.Low: return 10;
            case CardIntensity.Medium: return 18;
            case CardIntensity.High: return 25;
            case CardIntensity.VeryHigh: return 30;
            default: return 10;
        }
    }
    
    /// <summary>
    /// NOVO: Retorna valor escalado baseado no tipo de atributo
    /// Stats de combate usam valores menores, economia usa valores maiores
    /// </summary>
    public static int GetScaledValue(CardIntensity intensity, CardAttribute attribute)
    {
        int baseValue = GetValue(intensity);
        
        switch (attribute)
        {
            // Stats de combate diretos (HP, MP, Defense)
            case CardAttribute.PlayerMaxHP:
            case CardAttribute.EnemyMaxHP:
                return baseValue; // Valor cheio para HP
            
            case CardAttribute.PlayerMaxMP:
            case CardAttribute.EnemyMaxMP:
                return Mathf.RoundToInt(baseValue * 0.8f); // 80% para MP
            
            case CardAttribute.PlayerDefense:
            case CardAttribute.EnemyDefense:
                return Mathf.RoundToInt(baseValue * 0.6f); // 60% para Defense
            
            // Stats de velocidade (valores muito menores)
            case CardAttribute.PlayerSpeed:
            case CardAttribute.EnemySpeed:
                return Mathf.Max(1, Mathf.RoundToInt(baseValue * 0.2f)); // 20% para Speed (min 1)
            
            // Poder de ações
            case CardAttribute.PlayerActionPower:
            case CardAttribute.EnemyActionPower:
            case CardAttribute.PlayerOffensiveActionPower:
            case CardAttribute.EnemyOffensiveActionPower:
                return Mathf.RoundToInt(baseValue * 0.7f); // 70% para action power
            
            case CardAttribute.PlayerDefensiveActionPower:
                return Mathf.RoundToInt(baseValue * 0.6f); // 60% para defensive
            
            case CardAttribute.PlayerAOEActionPower:
            case CardAttribute.EnemyAOEActionPower:
                return Mathf.RoundToInt(baseValue * 0.5f); // 50% para AOE (afeta múltiplos alvos)
            
            case CardAttribute.PlayerSingleTargetActionPower:
                return Mathf.RoundToInt(baseValue * 0.8f); // 80% para single target
            
            // Custo de mana
            case CardAttribute.PlayerActionManaCost:
            case CardAttribute.EnemyActionManaCost:
                return Mathf.RoundToInt(baseValue * 0.4f); // 40% para mana cost
            
            // Economia (valores relativamente maiores)
            case CardAttribute.CoinsEarned:
                return Mathf.RoundToInt(baseValue * 1.2f); // 120% para coins
            
            case CardAttribute.ShopPrices:
                return Mathf.RoundToInt(baseValue * 0.9f); // 90% para preços
            
            default:
                return baseValue;
        }
    }
    
    public static string GetDisplayName(CardIntensity intensity)
    {
        switch (intensity)
        {
            case CardIntensity.VeryLow: return "Muito Baixa";
            case CardIntensity.Low: return "Baixa";
            case CardIntensity.Medium: return "Média";
            case CardIntensity.High: return "Alta";
            case CardIntensity.VeryHigh: return "Muito Alta";
            default: return "Média";
        }
    }
    
    /// <summary>
    /// NOVO: Retorna descrição com valor exato
    /// </summary>
    public static string GetDisplayNameWithValue(CardIntensity intensity, CardAttribute attribute)
    {
        int value = GetScaledValue(intensity, attribute);
        return $"{GetDisplayName(intensity)} (±{value})";
    }
}

/// <summary>
/// Classe auxiliar para nomes de atributos (EXPANDIDA)
/// </summary>
public static class AttributeHelper
{
    public static string GetDisplayName(CardAttribute attribute)
    {
        switch (attribute)
        {
            // Atributos base do jogador
            case CardAttribute.PlayerMaxHP: return "Vida Máxima";
            case CardAttribute.PlayerMaxMP: return "Mana Máxima";
            case CardAttribute.PlayerDefense: return "Defesa";
            case CardAttribute.PlayerSpeed: return "Velocidade";
            
            // Atributos de ações do jogador
            case CardAttribute.PlayerActionPower: return "Poder de Ações";
            case CardAttribute.PlayerActionManaCost: return "Custo de Mana";
            case CardAttribute.PlayerOffensiveActionPower: return "Poder de Ataques";
            case CardAttribute.PlayerDefensiveActionPower: return "Poder de Cura/Defesa";
            case CardAttribute.PlayerAOEActionPower: return "Poder de Ataques em Área";
            case CardAttribute.PlayerSingleTargetActionPower: return "Poder de Ataques Únicos";
            
            // Atributos dos inimigos
            case CardAttribute.EnemyMaxHP: return "Vida Máxima dos Inimigos";
            case CardAttribute.EnemyMaxMP: return "Mana Máxima dos Inimigos";
            case CardAttribute.EnemyDefense: return "Defesa dos Inimigos";
            case CardAttribute.EnemySpeed: return "Velocidade dos Inimigos";
            case CardAttribute.EnemyActionPower: return "Poder dos Inimigos";
            case CardAttribute.EnemyActionManaCost: return "Custo de Mana Inimigo";
            case CardAttribute.EnemyOffensiveActionPower: return "Poder de Ataques Inimigos";
            case CardAttribute.EnemyAOEActionPower: return "Poder de Ataques em Área Inimigos";
            
            // Economia
            case CardAttribute.CoinsEarned: return "Moedas Ganhas";
            case CardAttribute.ShopPrices: return "Preços da Loja";
            
            default: return attribute.ToString();
        }
    }
    
    /// <summary>
    /// NOVO: Retorna descrição curta para UI compacta
    /// </summary>
    public static string GetShortName(CardAttribute attribute)
    {
        switch (attribute)
        {
            case CardAttribute.PlayerMaxHP: return "HP Max";
            case CardAttribute.PlayerMaxMP: return "MP Max";
            case CardAttribute.PlayerDefense: return "Defesa";
            case CardAttribute.PlayerSpeed: return "Velocidade";
            case CardAttribute.PlayerActionPower: return "Dano";
            case CardAttribute.PlayerOffensiveActionPower: return "Dano Ataque";
            case CardAttribute.PlayerDefensiveActionPower: return "Cura";
            case CardAttribute.PlayerAOEActionPower: return "Dano Área";
            case CardAttribute.PlayerSingleTargetActionPower: return "Dano Single";
            case CardAttribute.PlayerActionManaCost: return "Custo MP";
            case CardAttribute.CoinsEarned: return "Moedas";
            case CardAttribute.ShopPrices: return "Preços";
            default: return GetDisplayName(attribute);
        }
    }
    
    /// <summary>
    /// NOVO: Verifica se um atributo modifica BattleActions
    /// </summary>
    public static bool IsActionModifier(CardAttribute attribute)
    {
        return attribute == CardAttribute.PlayerActionPower ||
               attribute == CardAttribute.PlayerActionManaCost ||
               attribute == CardAttribute.PlayerOffensiveActionPower ||
               attribute == CardAttribute.PlayerDefensiveActionPower ||
               attribute == CardAttribute.PlayerAOEActionPower ||
               attribute == CardAttribute.PlayerSingleTargetActionPower ||
               attribute == CardAttribute.EnemyActionPower ||
               attribute == CardAttribute.EnemyActionManaCost ||
               attribute == CardAttribute.EnemyOffensiveActionPower ||
               attribute == CardAttribute.EnemyAOEActionPower;
    }
    
    /// <summary>
    /// NOVO: Retorna atributos relacionados (para AttributeAndIntensity)
    /// </summary>
    public static CardAttribute[] GetRelatedAttributes(CardAttribute attribute)
    {
        switch (attribute)
        {
            // HP relaciona com Defense
            case CardAttribute.PlayerMaxHP:
                return new[] { CardAttribute.PlayerMaxHP, CardAttribute.PlayerDefense };
            
            // MP relaciona com custo de mana
            case CardAttribute.PlayerMaxMP:
                return new[] { CardAttribute.PlayerMaxMP, CardAttribute.PlayerActionManaCost };
            
            // Action Power pode escolher tipo específico
            case CardAttribute.PlayerActionPower:
                return new[] { 
                    CardAttribute.PlayerActionPower,
                    CardAttribute.PlayerOffensiveActionPower,
                    CardAttribute.PlayerSingleTargetActionPower,
                    CardAttribute.PlayerAOEActionPower
                };
            
            // Inimigos: HP com Defense
            case CardAttribute.EnemyMaxHP:
                return new[] { CardAttribute.EnemyMaxHP, CardAttribute.EnemyDefense };
            
            // Inimigos: Action Power com tipos
            case CardAttribute.EnemyActionPower:
                return new[] {
                    CardAttribute.EnemyActionPower,
                    CardAttribute.EnemyOffensiveActionPower,
                    CardAttribute.EnemyAOEActionPower
                };
            
            default:
                return new[] { attribute };
        }
    }
}