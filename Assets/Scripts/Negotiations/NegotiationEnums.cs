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
    ShopPrices,
    
    // NOVO: Modificadores específicos de skill
    SpecificSkillPower,      // Modifica apenas uma skill específica (poder)
    SpecificSkillManaCost,   // Modifica apenas uma skill específica (custo)
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
/// Helper para trabalhar com intensidades de cartas
/// VALORES BALANCEADOS: 4, 8, 12 (teto máximo)
/// </summary>
public static class IntensityHelper
{
    /// <summary>
    /// Retorna o valor escalado baseado na intensidade e tipo de atributo
    /// TETO: 4 (Low), 8 (Medium), 12 (High)
    /// </summary>
    public static int GetScaledValue(CardIntensity intensity, CardAttribute attribute)
    {
        // Define multiplicador base por intensidade
        float baseMultiplier = intensity switch
        {
            CardIntensity.Low => 1.0f,      // 4
            CardIntensity.Medium => 2.0f,   // 8
            CardIntensity.High => 3.0f,     // 12
            _ => 1.0f
        };
        
        // Define valor base por categoria de atributo
        int baseValue = GetBaseValueForAttribute(attribute);
        
        // Calcula valor final
        int finalValue = Mathf.RoundToInt(baseValue * baseMultiplier);
        
        return finalValue;
    }
    
    /// <summary>
    /// Retorna valor básico para a intensidade
    /// </summary>
    public static int GetValue(CardIntensity intensity)
    {
        return intensity switch
        {
            CardIntensity.Low => 4,
            CardIntensity.Medium => 8,
            CardIntensity.High => 12,
            _ => 4
        };
    }
    
    /// <summary>
    /// Retorna valor base por categoria de atributo
    /// Base = 4 (para escalar 4, 8, 12)
    /// </summary>
    private static int GetBaseValueForAttribute(CardAttribute attribute)
    {
        switch (attribute)
        {
            // === STATS BASE (valores maiores) ===
            case CardAttribute.PlayerMaxHP:
            case CardAttribute.EnemyMaxHP:
                return 8;  // 8, 16, 24
            
            case CardAttribute.PlayerMaxMP:
            case CardAttribute.EnemyMaxMP:
                return 6;  // 6, 12, 18
            
            // === STATS DE COMBATE (valores médios) ===
            case CardAttribute.PlayerDefense:
            case CardAttribute.EnemyDefense:
            case CardAttribute.PlayerActionPower:
            case CardAttribute.EnemyActionPower:
                return 4;  // 4, 8, 12
            
            // === TIPOS ESPECÍFICOS DE AÇÃO ===
            case CardAttribute.PlayerOffensiveActionPower:
            case CardAttribute.PlayerDefensiveActionPower:
            case CardAttribute.PlayerAOEActionPower:
            case CardAttribute.PlayerSingleTargetActionPower:
            case CardAttribute.EnemyOffensiveActionPower:
            case CardAttribute.EnemyAOEActionPower:
                return 5;  // 5, 10, 15
            
            // === VELOCIDADE (valores menores) ===
            case CardAttribute.PlayerSpeed:
            case CardAttribute.EnemySpeed:
                return 1;  // 1, 2, 3
            
            // === MANA COST (valores menores) ===
            case CardAttribute.PlayerActionManaCost:
            case CardAttribute.EnemyActionManaCost:
                return 2;  // 2, 4, 6
            
            // === ECONOMIA ===
            case CardAttribute.CoinsEarned:
                return 5;  // 5, 10, 15
            
            case CardAttribute.ShopPrices:
                return 4;  // 4, 8, 12
            
            default:
                return 4;  // Default: 4, 8, 12
        }
    }
    
    /// <summary>
    /// Retorna descrição legível da intensidade
    /// </summary>
    public static string GetIntensityDisplayName(CardIntensity intensity)
    {
        return intensity switch
        {
            CardIntensity.Low => "Baixo",
            CardIntensity.Medium => "Médio",
            CardIntensity.High => "Alto",
            _ => "Desconhecido"
        };
    }
    
    /// <summary>
    /// Retorna cor para a intensidade
    /// </summary>
    public static Color GetIntensityColor(CardIntensity intensity)
    {
        return intensity switch
        {
            CardIntensity.Low => new Color(0.6f, 0.8f, 1f),      // Azul claro
            CardIntensity.Medium => new Color(1f, 0.8f, 0.4f),   // Amarelo
            CardIntensity.High => new Color(1f, 0.4f, 0.4f),     // Vermelho
            _ => Color.white
        };
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