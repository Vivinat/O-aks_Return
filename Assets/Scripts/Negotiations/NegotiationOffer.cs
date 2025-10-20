// Assets/Scripts/Negotiation/NegotiationOffer.cs (FIXED - AUTO SIGN CORRECTION)

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Representa UMA oferta de negociação (vantagem ou desvantagem)
/// CORRIGE AUTOMATICAMENTE o sinal para custos de mana
/// </summary>
[System.Serializable]
public class NegotiationOffer
{
    public string offerName;
    public string offerDescription;
    
    public bool isAdvantage;
    public CardAttribute targetAttribute;
    public int value;
    public bool affectsPlayer;
    
    public BehaviorTriggerType sourceObservationType;
    public string contextualInfo;
    
    private Dictionary<string, object> customData = new Dictionary<string, object>();
    
    /// <summary>
    /// NOVO: Cria vantagem com CORREÇÃO AUTOMÁTICA de sinal para mana cost
    /// </summary>
    public static NegotiationOffer CreateAdvantage(
        string name,
        string description,
        CardAttribute attribute,
        int value,
        BehaviorTriggerType trigger,
        string context = "")
    {
        // ✅ CORREÇÃO AUTOMÁTICA: Força sinal correto para custos de mana
        int correctedValue = CorrectSignForManaCost(attribute, value, true);
        
        return new NegotiationOffer
        {
            offerName = name,
            offerDescription = description,
            isAdvantage = true,
            targetAttribute = attribute,
            value = correctedValue,  // ✅ Usa valor corrigido
            affectsPlayer = IsPlayerAttribute(attribute),
            sourceObservationType = trigger,
            contextualInfo = context
        };
    }
    
    /// <summary>
    /// NOVO: Cria desvantagem com CORREÇÃO AUTOMÁTICA de sinal para mana cost
    /// </summary>
    public static NegotiationOffer CreateDisadvantage(
        string name,
        string description,
        CardAttribute attribute,
        int value,
        bool affectsPlayer,
        BehaviorTriggerType trigger,
        string context = "")
    {
        // ✅ CORREÇÃO AUTOMÁTICA: Força sinal correto para custos de mana
        int correctedValue = CorrectSignForManaCost(attribute, value, false);
        
        return new NegotiationOffer
        {
            offerName = name,
            offerDescription = description,
            isAdvantage = false,
            targetAttribute = attribute,
            value = correctedValue,  // ✅ Usa valor corrigido
            affectsPlayer = affectsPlayer,
            sourceObservationType = trigger,
            contextualInfo = context
        };
    }
    
    /// <summary>
    /// ✅ CORREÇÃO AUTOMÁTICA DE SINAL PARA CUSTOS DE MANA
    /// </summary>
    private static int CorrectSignForManaCost(CardAttribute attribute, int value, bool isAdvantage)
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
    
    private static bool IsPlayerAttribute(CardAttribute attr)
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
    
    // Custom data storage
    public void SetData<T>(string key, T data)
    {
        customData[key] = data;
    }
    
    public T GetData<T>(string key, T defaultValue = default)
    {
        if (customData.TryGetValue(key, out object value))
        {
            try
            {
                return (T)value;
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }
    
    public bool HasData(string key)
    {
        return customData.ContainsKey(key);
    }
}