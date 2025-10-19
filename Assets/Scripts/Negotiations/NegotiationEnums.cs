// Assets/Scripts/Negotiation/NegotiationEnums.cs (COMPLETE - SIMPLIFIED)

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
    
    // Atributos de Ações do Jogador
    PlayerActionPower,
    PlayerActionManaCost,
    PlayerOffensiveActionPower,
    PlayerDefensiveActionPower,
    PlayerAOEActionPower,
    PlayerSingleTargetActionPower,
    
    // Atributos de Inimigos
    EnemyMaxHP,
    EnemyMaxMP,
    EnemyDefense,
    EnemySpeed,
    EnemyActionPower,
    EnemyActionManaCost,
    EnemyOffensiveActionPower,
    EnemyAOEActionPower,
    
    // Atributos Gerais
    CoinsEarned,
    ShopPrices,
    
    // Modificadores específicos de skill
    SpecificSkillPower,
    SpecificSkillManaCost,
}

/// <summary>
/// Intensidades disponíveis para modificações
/// </summary>
public enum CardIntensity
{
    Low,     // 1x
    Medium,  // 2x
    High     // 3x
}

/// <summary>
/// Helper para trabalhar com intensidades de cartas
/// MULTIPLICADORES SIMPLES: 1x, 2x, 3x
/// </summary>
public static class IntensityHelper
{
    /// <summary>
    /// Retorna o multiplicador da intensidade
    /// Low = 1x, Medium = 2x, High = 3x
    /// </summary>
    public static float GetMultiplier(CardIntensity intensity)
    {
        return intensity switch
        {
            CardIntensity.Low => 1.0f,
            CardIntensity.Medium => 2.0f,
            CardIntensity.High => 3.0f,
            _ => 1.0f
        };
    }
    
    /// <summary>
    /// Aplica o multiplicador da intensidade a um valor base
    /// </summary>
    public static int GetScaledValue(CardIntensity intensity, int baseValue)
    {
        float multiplier = GetMultiplier(intensity);
        return Mathf.RoundToInt(baseValue * multiplier);
    }
    
    /// <summary>
    /// Retorna valor básico para a intensidade (compatibilidade)
    /// </summary>
    public static int GetValue(CardIntensity intensity)
    {
        return (int)GetMultiplier(intensity);
    }
    
    /// <summary>
    /// Retorna descrição legível da intensidade (apenas o nome)
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
/// Classe auxiliar para nomes e descrições de atributos
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
    /// NOVO: Retorna explicação detalhada de quando/como o atributo afeta o jogo
    /// </summary>
    public static string GetDetailedExplanation(CardAttribute attribute, int value, bool isAdvantage)
    {
        string sign = value > 0 ? "+" : "";
        string absValue = Mathf.Abs(value).ToString();
        
        switch (attribute)
        {
            // === JOGADOR - STATS BASE ===
            case CardAttribute.PlayerMaxHP:
                if (isAdvantage)
                    return $"Sua vida máxima aumenta em {absValue} pontos";
                else
                    return $"Sua vida máxima diminui em {absValue} pontos";
            
            case CardAttribute.PlayerMaxMP:
                if (isAdvantage)
                    return $"Sua mana máxima aumenta em {absValue} pontos";
                else
                    return $"Sua mana máxima diminui em {absValue} pontos";
            
            case CardAttribute.PlayerDefense:
                if (isAdvantage)
                    return $"Sua defesa aumenta em {absValue}";
                else
                    return $"Sua defesa diminui em {absValue}";
            
            case CardAttribute.PlayerSpeed:
                if (isAdvantage)
                    return $"Sua velocidade aumenta em {absValue}";
                else
                    return $"Sua velocidade diminui em {absValue}";
            
            // === JOGADOR - AÇÕES ===
            case CardAttribute.PlayerActionPower:
                if (isAdvantage)
                    return $"Habilidades causam {sign}{absValue} de dano/cura extra";
                else
                    return $"Habilidades causam {sign}{absValue} de dano/cura a menos";
            
            case CardAttribute.PlayerActionManaCost:
                if (isAdvantage)
                    return $"Habilidades custam {absValue} MP a menos";
                else
                    return $"Habilidades custam {absValue} MP a mais";
            
            case CardAttribute.PlayerOffensiveActionPower:
                if (isAdvantage)
                    return $"Ataques ofensivos causam {sign}{absValue} de dano extra";
                else
                    return $"Ataques ofensivos causam {sign}{absValue} de dano a menos";
            
            case CardAttribute.PlayerDefensiveActionPower:
                if (isAdvantage)
                    return $"Curas e buffs são {sign}{absValue} pontos mais efetivos";
                else
                    return $"Curas e buffs são {sign}{absValue} pontos menos efetivos";
            
            case CardAttribute.PlayerAOEActionPower:
                if (isAdvantage)
                    return $"Ataques em área causam {sign}{absValue} de dano extra";
                else
                    return $"Ataques em área causam {sign}{absValue} de dano a menos";
            
            case CardAttribute.PlayerSingleTargetActionPower:
                if (isAdvantage)
                    return $"Ataques de alvo único causam {sign}{absValue} de dano extra";
                else
                    return $"Ataques de alvo único causam {sign}{absValue} de dano a menos";
            
            // === INIMIGOS - STATS ===
            case CardAttribute.EnemyMaxHP:
                if (isAdvantage)
                    return $"Inimigos têm {absValue} HP a menos";
                else
                    return $"Inimigos têm {sign}{absValue} HP a mais";
            
            case CardAttribute.EnemyMaxMP:
                if (isAdvantage)
                    return $"Inimigos têm {absValue} MP a menos";
                else
                    return $"Inimigos têm {sign}{absValue} MP a mais";
            
            case CardAttribute.EnemyDefense:
                if (isAdvantage)
                    return $"Inimigos têm {absValue} de defesa a menos";
                else
                    return $"Inimigos têm {sign}{absValue} de defesa a mais";
            
            case CardAttribute.EnemySpeed:
                if (isAdvantage)
                    return $"Inimigos têm {absValue} de velocidade a menos";
                else
                    return $"Inimigos têm {sign}{absValue} de velocidade a mais";
            
            // === INIMIGOS - AÇÕES ===
            case CardAttribute.EnemyActionPower:
                if (isAdvantage)
                    return $"Ataques inimigos causam {absValue} de dano a menos";
                else
                    return $"Ataques inimigos causam {sign}{absValue} de dano extra";
            
            case CardAttribute.EnemyActionManaCost:
                if (isAdvantage)
                    return $"Habilidades inimigas custam {sign}{absValue} MP a mais";
                else
                    return $"Habilidades inimigas custam {absValue} MP a menos";
            
            case CardAttribute.EnemyOffensiveActionPower:
                if (isAdvantage)
                    return $"Ataques ofensivos inimigos causam {absValue} de dano a menos";
                else
                    return $"Ataques ofensivos  inimigos causam {sign}{absValue} de dano extra";
            
            case CardAttribute.EnemyAOEActionPower:
                if (isAdvantage)
                    return $"Ataques em área inimigos causam {absValue} de dano a menos";
                else
                    return $"Ataques em área inimigos causam {sign}{absValue} de dano extra";
            
            // === ECONOMIA ===
            case CardAttribute.CoinsEarned:
                if (isAdvantage)
                    return $"Você ganha {sign}{absValue} moedas extras ao vencer batalhas";
                else
                    return $"Você ganha {absValue} moedas a menos ao vencer batalhas";
            
            case CardAttribute.ShopPrices:
                if (isAdvantage)
                    return $"Itens na loja custam {absValue} moedas a menos";
                else
                    return $"Itens na loja custam {sign}{absValue} moedas a mais";
            
            default:
                return GetDisplayName(attribute);
        }
    }
    
    /// <summary>
    /// Retorna descrição curta para UI compacta
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
    /// Verifica se um atributo modifica BattleActions
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
    /// Retorna atributos relacionados (para AttributeAndIntensity)
    /// </summary>
    public static CardAttribute[] GetRelatedAttributes(CardAttribute attribute)
    {
        switch (attribute)
        {
            case CardAttribute.PlayerMaxHP:
                return new[] { CardAttribute.PlayerMaxHP, CardAttribute.PlayerDefense };
            
            case CardAttribute.PlayerMaxMP:
                return new[] { CardAttribute.PlayerMaxMP, CardAttribute.PlayerActionManaCost };
            
            case CardAttribute.PlayerActionPower:
                return new[] { 
                    CardAttribute.PlayerActionPower,
                    CardAttribute.PlayerOffensiveActionPower,
                    CardAttribute.PlayerSingleTargetActionPower,
                    CardAttribute.PlayerAOEActionPower
                };
            
            case CardAttribute.EnemyMaxHP:
                return new[] { CardAttribute.EnemyMaxHP, CardAttribute.EnemyDefense };
            
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