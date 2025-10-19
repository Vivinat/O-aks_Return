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
    /// CORRIGIDO: Calcula e mostra valores REAIS baseados na intensidade
    /// </summary>
    public string GetFullDescription(CardAttribute? playerAttr, CardAttribute? enemyAttr, int intensityValue)
    {
        string desc = $"<b><size=110%>{cardName}</size></b>\n\n";
        desc += $"<i>{cardDescription}</i>\n\n";

        // === VANTAGEM ===
        CardAttribute advantageAttr = playerAttr ?? playerBenefit.targetAttribute;
        
        // CRÍTICO: Calcula o valor REAL escalado
        int realAdvantageValue;
        if (cardType == NegotiationCardType.Fixed)
        {
            realAdvantageValue = playerBenefit.value;
        }
        else
        {
            // Usa o intensityValue (12) como CardIntensity para calcular o valor REAL
            realAdvantageValue = IntensityHelper.GetScaledValue((CardIntensity)intensityValue, advantageAttr);
        }
        
        desc += $"<color=#90EE90><b>✓ Você Ganha:</b></color>\n";
        desc += $"+{realAdvantageValue} {AttributeHelper.GetDisplayName(advantageAttr)}\n";

        // === DESVANTAGEM ===
        desc += $"\n<color=#FF6B6B><b>✗ Custo:</b></color>\n";

        CardAttribute costAttr = enemyAttr ?? playerCost.targetAttribute;
        
        // CRÍTICO: Calcula o valor REAL escalado do custo
        int realCostValue;
        if (cardType == NegotiationCardType.Fixed)
        {
            realCostValue = playerCost.value;
        }
        else
        {
            realCostValue = IntensityHelper.GetScaledValue((CardIntensity)intensityValue, costAttr);
        }

        // Detecta se afeta jogador ou inimigos
        bool costAffectsPlayer = IsPlayerAttribute(costAttr) || playerCost.affectsPlayer;

        if (costAffectsPlayer)
        {
            // Debuff no jogador
            if (realCostValue < 0)
            {
                int displayValue = Mathf.Abs(realCostValue);
                desc += $"Você perde: <color=#FF4444>-{displayValue}</color> {AttributeHelper.GetDisplayName(costAttr)}";
            }
            else
            {
                desc += $"<color=#FF4444>+{realCostValue}</color> {AttributeHelper.GetDisplayName(costAttr)}";
            }
        }
        else
        {
            // Buff nos inimigos
            desc += $"Inimigos ganham: <color=#FF4444>+{realCostValue}</color> {AttributeHelper.GetDisplayName(costAttr)}";
        }

        return desc;
    }
    
    /// <summary>
    /// NOVO: Verifica se um atributo afeta o jogador
    /// </summary>
    private bool IsPlayerAttribute(CardAttribute attr)
    {
        switch (attr)
        {
            case CardAttribute.PlayerMaxHP:
            case CardAttribute.PlayerMaxMP:
            case CardAttribute.PlayerDefense:
            case CardAttribute.PlayerSpeed:
            case CardAttribute.PlayerActionPower:
            case CardAttribute.PlayerActionManaCost:
            case CardAttribute.PlayerOffensiveActionPower:
            case CardAttribute.PlayerDefensiveActionPower:
            case CardAttribute.PlayerAOEActionPower:
            case CardAttribute.PlayerSingleTargetActionPower:
            case CardAttribute.CoinsEarned:
            case CardAttribute.ShopPrices:
                return true;
            
            default:
                return false;
        }
    }
    
    public string GetCardName() => cardName;
}