using UnityEngine;
using System.Collections.Generic;

// Representa uma oferta de negociação 
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
    
    // Cria vantagem 

    public static NegotiationOffer CreateAdvantage(
        string name,
        string description,
        CardAttribute attribute,
        int value,
        BehaviorTriggerType trigger,
        string context = "")
    {
        int correctedValue = CorrectSignForManaCost(attribute, value, true);
        
        return new NegotiationOffer
        {
            offerName = name,
            offerDescription = description,
            isAdvantage = true,
            targetAttribute = attribute,
            value = correctedValue,
            affectsPlayer = IsPlayerAttribute(attribute),
            sourceObservationType = trigger,
            contextualInfo = context
        };
    }
    
    // Cria desvantagem 
    public static NegotiationOffer CreateDisadvantage(
        string name,
        string description,
        CardAttribute attribute,
        int value,
        bool affectsPlayer,
        BehaviorTriggerType trigger,
        string context = "")
    {
        int correctedValue = CorrectSignForManaCost(attribute, value, false);
        
        return new NegotiationOffer
        {
            offerName = name,
            offerDescription = description,
            isAdvantage = false,
            targetAttribute = attribute,
            value = correctedValue,
            affectsPlayer = affectsPlayer,
            sourceObservationType = trigger,
            contextualInfo = context
        };
    }
    
    private static int CorrectSignForManaCost(CardAttribute attribute, int value, bool isAdvantage)
    {
        if (attribute != CardAttribute.PlayerActionManaCost && 
            attribute != CardAttribute.EnemyActionManaCost)
        {
            return value;
        }
        
        if (attribute == CardAttribute.PlayerActionManaCost)
        {
            if (isAdvantage)
            {
                return -Mathf.Abs(value); // Vantagem: Reduzir custo = negativo
            }
            else
            {
                return Mathf.Abs(value); // Desvantagem: Aumentar custo = positivo
            }
        }
        
        if (attribute == CardAttribute.EnemyActionManaCost)
        {
            if (isAdvantage)
            {
                return Mathf.Abs(value); // Vantagem: Aumentar custo inimigo = positivo
            }
            else
            {
                return -Mathf.Abs(value); // Desvantagem: Reduzir custo inimigo = negativo
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