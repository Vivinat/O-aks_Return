// Assets/Scripts/Negotiation/DynamicNegotiationCard.cs

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Representa uma carta de negociação gerada dinamicamente
/// Suporta Fixed, IntensityOnly e AttributeAndIntensity
/// </summary>
[System.Serializable]
public class DynamicNegotiationCard
{
    public string cardName;
    public string cardDescription;
    public Sprite cardSprite;
    
    public NegotiationCardType cardType;
    
    // Dados da oferta de benefício (vantagem)
    public NegotiationOffer playerBenefit;
    
    // Dados da oferta de custo (desvantagem)
    public NegotiationOffer playerCost;
    
    // Para IntensityOnly e AttributeAndIntensity
    public List<CardIntensity> availableIntensities = new List<CardIntensity>
    {
        CardIntensity.Low,
        CardIntensity.Medium,
        CardIntensity.High
    };
    
    // Para AttributeAndIntensity
    public List<CardAttribute> availablePlayerAttributes = new List<CardAttribute>();
    public List<CardAttribute> availableEnemyAttributes = new List<CardAttribute>();
    
    public DynamicNegotiationCard(NegotiationOffer advantage, NegotiationOffer disadvantage, NegotiationCardType type = NegotiationCardType.Fixed)
    {
        playerBenefit = advantage;
        playerCost = disadvantage;
        cardType = type;
        
        cardName = GenerateCardName();
        cardDescription = GenerateDescription();
        
        // Setup baseado no tipo
        SetupByType();
    }
    
    private void SetupByType()
    {
        switch (cardType)
        {
            case NegotiationCardType.Fixed:
                // Nada a fazer, valores são fixos
                break;
                
            case NegotiationCardType.IntensityOnly:
                // Mantém os atributos fixos mas permite escolher intensidade
                break;
                
            case NegotiationCardType.AttributeAndIntensity:
                // Gera opções de atributos baseadas nos originais
                GenerateAttributeOptions();
                break;
        }
    }
    
    private void GenerateAttributeOptions()
    {
        // Jogador: oferece variações do atributo original ou relacionados
        availablePlayerAttributes.Add(playerBenefit.playerAttribute);
        
        // Adiciona atributos relacionados
        switch (playerBenefit.playerAttribute)
        {
            case CardAttribute.PlayerMaxHP:
                availablePlayerAttributes.Add(CardAttribute.PlayerDefense);
                break;
            case CardAttribute.PlayerMaxMP:
                availablePlayerAttributes.Add(CardAttribute.PlayerActionManaCost);
                break;
            case CardAttribute.PlayerSpeed:
                availablePlayerAttributes.Add(CardAttribute.PlayerDefense);
                break;
            case CardAttribute.PlayerActionPower:
                availablePlayerAttributes.Add(CardAttribute.PlayerMaxHP);
                break;
        }
        
        // Inimigos: oferece variações
        availableEnemyAttributes.Add(playerCost.enemyAttribute);
        
        switch (playerCost.enemyAttribute)
        {
            case CardAttribute.EnemyMaxHP:
                availableEnemyAttributes.Add(CardAttribute.EnemyDefense);
                break;
            case CardAttribute.EnemySpeed:
                availableEnemyAttributes.Add(CardAttribute.EnemyActionPower);
                break;
            case CardAttribute.EnemyActionPower:
                availableEnemyAttributes.Add(CardAttribute.EnemyMaxHP);
                break;
        }
    }
    
    private string GenerateCardName()
    {
        string[] benefitWords = playerBenefit.offerName.Split(' ');
        string[] costWords = playerCost.offerName.Split(' ');
        
        string benefitPart = benefitWords.Length > 0 ? benefitWords[0] : "Acordo";
        string costPart = costWords.Length > 0 ? costWords[costWords.Length - 1] : "Cósmico";
        
        return $"{benefitPart} & {costPart}";
    }
    
    private string GenerateDescription()
    {
        return $"Uma negociação que equilibra {playerBenefit.offerName.ToLower()} com {playerCost.offerName.ToLower()}.";
    }
    
    /// <summary>
    /// Retorna descrição completa formatada (para tipo Fixed)
    /// </summary>
    public string GetFullDescription()
    {
        return GetFullDescription(
            playerBenefit.playerAttribute, 
            playerCost.enemyAttribute, 
            playerBenefit.playerValue
        );
    }
    
    /// <summary>
    /// Retorna descrição completa formatada com valores customizados
    /// </summary>
    public string GetFullDescription(CardAttribute? playerAttr, CardAttribute? enemyAttr, int value)
    {
        string desc = $"<b><size=110%>{cardName}</size></b>\n\n";
        desc += $"<i>{cardDescription}</i>\n\n";
        
        // Benefício (vantagem)
        desc += $"<color=#90EE90><b>✓ Você Ganha:</b></color>\n";
        if (playerBenefit.playerValue != 0)
        {
            CardAttribute attr = playerAttr ?? playerBenefit.playerAttribute;
            int val = (cardType == NegotiationCardType.Fixed) ? playerBenefit.playerValue : value;
            string sign = val > 0 ? "+" : "";
            desc += $"{sign}{val} {AttributeHelper.GetDisplayName(attr)}\n";
        }
        
        // Custo (desvantagem)
        desc += $"\n<color=#FF6B6B><b>✗ Você Perde / Inimigos Ganham:</b></color>\n";
        
        // Debuff no jogador (se houver)
        if (playerCost.playerValue != 0)
        {
            int val = (cardType == NegotiationCardType.Fixed) ? playerCost.playerValue : value;
            string sign = val > 0 ? "+" : "";
            desc += $"{sign}{val} {AttributeHelper.GetDisplayName(playerCost.playerAttribute)}\n";
        }
        
        // Buff nos inimigos (se houver)
        if (playerCost.enemyValue != 0)
        {
            CardAttribute attr = enemyAttr ?? playerCost.enemyAttribute;
            int val = (cardType == NegotiationCardType.Fixed) ? playerCost.enemyValue : value;
            string sign = val > 0 ? "+" : "";
            desc += $"Inimigos: {sign}{val} {AttributeHelper.GetDisplayName(attr)}";
        }
        
        return desc;
    }
    
    public string GetCardName() => cardName;
}