// Assets/Scripts/Negotiation/DynamicNegotiationCard.cs (CORRIGIDO)

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Carta de negociação: EXATAMENTE 1 vantagem + 1 desvantagem
/// </summary>
[System.Serializable]
public class DynamicNegotiationCard
{
    public string cardName;
    public string cardDescription;
    public Sprite cardSprite;
    
    public NegotiationCardType cardType;
    
    // EXATAMENTE uma vantagem e uma desvantagem
    public NegotiationOffer playerBenefit;  // O que você ganha
    public NegotiationOffer playerCost;     // O que você perde OU o que inimigos ganham
    
    // Para IntensityOnly e AttributeAndIntensity
    public List<CardIntensity> availableIntensities = new List<CardIntensity>
    {
        CardIntensity.Low,
        CardIntensity.Medium,
        CardIntensity.High
    };
    
    public List<CardAttribute> availablePlayerAttributes = new List<CardAttribute>();
    public List<CardAttribute> availableEnemyAttributes = new List<CardAttribute>();
    
    public DynamicNegotiationCard(NegotiationOffer advantage, NegotiationOffer disadvantage, NegotiationCardType type = NegotiationCardType.Fixed)
    {
        // Validação: garante que temos exatamente 1 vantagem e 1 desvantagem
        if (!advantage.isAdvantage)
        {
            Debug.LogError("Primeira oferta deve ser vantagem!");
            return;
        }
        
        if (disadvantage.isAdvantage)
        {
            Debug.LogError("Segunda oferta deve ser desvantagem!");
            return;
        }
        
        playerBenefit = advantage;
        playerCost = disadvantage;
        cardType = type;
        
        cardName = GenerateCardName();
        cardDescription = GenerateDescription();
        
        SetupByType();
    }
    
    private void SetupByType()
    {
        switch (cardType)
        {
            case NegotiationCardType.Fixed:
                break;
                
            case NegotiationCardType.IntensityOnly:
                break;
                
            case NegotiationCardType.AttributeAndIntensity:
                GenerateAttributeOptions();
                break;
        }
    }
    
    private void GenerateAttributeOptions()
    {
        // Gera variações do atributo da vantagem
        availablePlayerAttributes.Add(playerBenefit.targetAttribute);
        
        switch (playerBenefit.targetAttribute)
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
        
        // Gera variações do atributo da desvantagem
        availableEnemyAttributes.Add(playerCost.targetAttribute);
        
        switch (playerCost.targetAttribute)
        {
            case CardAttribute.PlayerMaxHP:
                availableEnemyAttributes.Add(CardAttribute.PlayerDefense);
                break;
            case CardAttribute.EnemyMaxHP:
                availableEnemyAttributes.Add(CardAttribute.EnemyDefense);
                break;
            case CardAttribute.EnemySpeed:
                availableEnemyAttributes.Add(CardAttribute.EnemyActionPower);
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
        return $"Troca {playerBenefit.offerName.ToLower()} por {playerCost.offerName.ToLower()}.";
    }
    
    /// <summary>
    /// Retorna descrição completa CLARA e CORRETA
    /// </summary>
    public string GetFullDescription()
    {
        return GetFullDescription(
            playerBenefit.targetAttribute, 
            playerCost.targetAttribute, 
            playerBenefit.value
        );
    }
    
    public string GetFullDescription(CardAttribute? playerAttr, CardAttribute? enemyAttr, int value)
    {
        string desc = $"<b><size=110%>{cardName}</size></b>\n\n";
        desc += $"<i>{cardDescription}</i>\n\n";
        
        // === VANTAGEM (sempre para o jogador) ===
        desc += $"<color=#90EE90><b>✓ Você Ganha:</b></color>\n";
        CardAttribute advantageAttr = playerAttr ?? playerBenefit.targetAttribute;
        int advantageValue = (cardType == NegotiationCardType.Fixed) ? playerBenefit.value : value;
        string advantageSign = advantageValue > 0 ? "+" : "";
        desc += $"{advantageSign}{advantageValue} {AttributeHelper.GetDisplayName(advantageAttr)}\n";
        
        // === DESVANTAGEM (debuff no jogador OU buff nos inimigos) ===
        desc += $"\n<color=#FF6B6B><b>✗ Custo:</b></color>\n";
        
        CardAttribute costAttr = enemyAttr ?? playerCost.targetAttribute;
        int costValue = (cardType == NegotiationCardType.Fixed) ? playerCost.value : value;
        string costSign = costValue > 0 ? "+" : "";
        
        if (playerCost.affectsPlayer)
        {
            // Debuff no jogador
            desc += $"Você perde: {costSign}{costValue} {AttributeHelper.GetDisplayName(costAttr)}";
        }
        else
        {
            // Buff nos inimigos
            desc += $"Inimigos ganham: {costSign}{costValue} {AttributeHelper.GetDisplayName(costAttr)}";
        }
        
        return desc;
    }
    
    public string GetCardName() => cardName;
}