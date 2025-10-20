// Assets/Scripts/Negotiation/DynamicNegotiationCard.cs (COMPLETE - FIXED)

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
    /// ✅ CORRIGIDO: Calcula e mostra valores REAIS com sinal correto para mana cost
    /// </summary>
    public string GetFullDescription(CardAttribute? playerAttr, CardAttribute? enemyAttr, CardIntensity intensity)
    {
        string desc = $"<b><size=110%>{cardName}</size></b>\n\n";
        desc += $"<i>{cardDescription}</i>\n\n";

        // === VANTAGEM ===
        CardAttribute advantageAttr = playerAttr ?? playerBenefit.targetAttribute;
        
        // Calcula o valor REAL aplicando o multiplicador ao valor base
        int realAdvantageValue = IntensityHelper.GetScaledValue(intensity, playerBenefit.value);
        
        // ✅ CORREÇÃO: Força sinal correto para custos de mana
        realAdvantageValue = CorrectManaCostSignForUI(advantageAttr, realAdvantageValue, true);
        
        desc += $"<color=#90EE90><b>✓ Você Ganha:</b></color>\n";
        
        // Formato especial para custos de mana (sempre mostrar redução como positivo na UI)
        if (advantageAttr == CardAttribute.PlayerActionManaCost)
        {
            desc += $"Reduz custo de mana: <color=#90EE90>{Mathf.Abs(realAdvantageValue)}</color>\n";
        }
        else if (advantageAttr == CardAttribute.EnemyActionManaCost)
        {
            desc += $"Inimigos pagam <color=#90EE90>+{realAdvantageValue}</color> de mana a mais\n";
        }
        else
        {
            desc += $"+{realAdvantageValue} {AttributeHelper.GetDisplayName(advantageAttr)}\n";
        }

        // === DESVANTAGEM ===
        desc += $"\n<color=#FF6B6B><b>✗ Custo:</b></color>\n";

        CardAttribute costAttr = enemyAttr ?? playerCost.targetAttribute;
        
        // Calcula o valor REAL aplicando o multiplicador ao valor base
        int realCostValue = IntensityHelper.GetScaledValue(intensity, playerCost.value);
        
        // ✅ CORREÇÃO: Força sinal correto para custos de mana
        realCostValue = CorrectManaCostSignForUI(costAttr, realCostValue, false);

        // Detecta se afeta jogador ou inimigos
        bool costAffectsPlayer = IsPlayerAttribute(costAttr) || playerCost.affectsPlayer;

        if (costAffectsPlayer)
        {
            // Debuff no jogador
            if (costAttr == CardAttribute.PlayerActionManaCost)
            {
                desc += $"Aumenta custo de mana: <color=#FF4444>+{Mathf.Abs(realCostValue)}</color>";
            }
            else
            {
                desc += $"Você perde: <color=#FF4444>-{Mathf.Abs(realCostValue)}</color> {AttributeHelper.GetDisplayName(costAttr)}";
            }
        }
        else
        {
            // Buff nos inimigos
            if (costAttr == CardAttribute.EnemyActionManaCost)
            {
                desc += $"Inimigos pagam <color=#FF4444>{realCostValue}</color> de mana a menos";
            }
            else
            {
                desc += $"Inimigos ganham: <color=#FF4444>+{Mathf.Abs(realCostValue)}</color> {AttributeHelper.GetDisplayName(costAttr)}";
            }
        }

        return desc;
    }
    
    /// <summary>
    /// ✅ NOVO: Força sinal correto para custos de mana na UI
    /// </summary>
    private int CorrectManaCostSignForUI(CardAttribute attribute, int value, bool isAdvantage)
    {
        // Se não for custo de mana, retorna valor original
        if (attribute != CardAttribute.PlayerActionManaCost && 
            attribute != CardAttribute.EnemyActionManaCost)
        {
            return value;
        }
    
        // === CUSTO DE MANA DO JOGADOR ===
        if (attribute == CardAttribute.PlayerActionManaCost)
        {
            if (isAdvantage)
            {
                // ✅ VANTAGEM: Reduzir custo = NEGATIVO
                return -Mathf.Abs(value);
            }
            else
            {
                // ❌ DESVANTAGEM: Aumentar custo = POSITIVO
                return Mathf.Abs(value);
            }
        }
    
        // === CUSTO DE MANA DOS INIMIGOS ===
        if (attribute == CardAttribute.EnemyActionManaCost)
        {
            if (isAdvantage)
            {
                // ✅ VANTAGEM (para jogador): Aumentar custo inimigo = POSITIVO
                return Mathf.Abs(value);
            }
            else
            {
                // ❌ DESVANTAGEM: Reduzir custo inimigo = NEGATIVO
                return -Mathf.Abs(value);
            }
        }
    
        return value;
    }
    
    /// <summary>
    /// Verifica se um atributo afeta o jogador
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