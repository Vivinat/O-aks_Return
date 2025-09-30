// Assets/Scripts/Negotiation/NegotiationEnums.cs

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
    
    // Atributos de Inimigos (Específicos)
    EnemyMaxHP,
    EnemyDefense,
    EnemySpeed,
    EnemyActionPower,
    
    // Atributos Gerais
    CoinsEarned,
    ShopPrices
}

/// <summary>
/// Intensidades disponíveis para modificações
/// </summary>
public enum CardIntensity
{
    VeryLow,    // -5 ou +5
    Low,        // -10 ou +10
    Medium,     // -20 ou +20
    High,       // -30 ou +30
    VeryHigh    // -50 ou +50
}

/// <summary>
/// Classe auxiliar para conversão de intensidade em valores
/// </summary>
public static class IntensityHelper
{
    public static int GetValue(CardIntensity intensity)
    {
        switch (intensity)
        {
            case CardIntensity.VeryLow: return 5;
            case CardIntensity.Low: return 10;
            case CardIntensity.Medium: return 20;
            case CardIntensity.High: return 30;
            case CardIntensity.VeryHigh: return 50;
            default: return 10;
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
}

/// <summary>
/// Classe auxiliar para nomes de atributos
/// </summary>
public static class AttributeHelper
{
    public static string GetDisplayName(CardAttribute attribute)
    {
        switch (attribute)
        {
            case CardAttribute.PlayerMaxHP: return "Vida Máxima do Jogador";
            case CardAttribute.PlayerMaxMP: return "Mana Máxima do Jogador";
            case CardAttribute.PlayerDefense: return "Defesa do Jogador";
            case CardAttribute.PlayerSpeed: return "Velocidade do Jogador";
            case CardAttribute.PlayerActionPower: return "Poder das Ações do Jogador";
            case CardAttribute.PlayerActionManaCost: return "Custo de Mana das Ações";
            case CardAttribute.EnemyMaxHP: return "Vida Máxima dos Inimigos";
            case CardAttribute.EnemyDefense: return "Defesa dos Inimigos";
            case CardAttribute.EnemySpeed: return "Velocidade dos Inimigos";
            case CardAttribute.EnemyActionPower: return "Poder das Ações dos Inimigos";
            case CardAttribute.CoinsEarned: return "Moedas Ganhas";
            case CardAttribute.ShopPrices: return "Preços da Loja";
            default: return attribute.ToString();
        }
    }
}