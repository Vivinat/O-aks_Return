using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Negotiation/Negotiation Card")]
public class NegotiationCardSO : ScriptableObject
{
    public string cardName;
    [TextArea(3, 6)]
    public string cardDescription;
    public Sprite cardSprite;
    
    public NegotiationCardType cardType;
    
    public CardAttribute fixedPlayerAttribute;
    public int fixedPlayerValue;
    public CardAttribute fixedEnemyAttribute;
    public int fixedEnemyValue;
    
    public CardAttribute intensityOnlyPlayerAttribute;
    public CardAttribute intensityOnlyEnemyAttribute;
    public List<CardIntensity> availableIntensities = new List<CardIntensity>
    {
        CardIntensity.Low,
        CardIntensity.Medium,
        CardIntensity.High
    };
    
    public List<CardAttribute> availablePlayerAttributes = new List<CardAttribute>();
    public List<CardAttribute> availableEnemyAttributes = new List<CardAttribute>();
    
    public string GetFullDescription(CardAttribute? playerAttr, CardAttribute? enemyAttr, CardIntensity intensity)
    {
        string desc = $"<b><size=110%>{cardName}</size></b>\n\n";
        desc += $"<i>{cardDescription}</i>\n\n";
        
        switch (cardType)
        {
            case NegotiationCardType.Fixed:
                desc += GetFixedDescription();
                break;
                
            case NegotiationCardType.IntensityOnly:
                desc += GetIntensityOnlyDescription(intensity);
                break;
                
            case NegotiationCardType.AttributeAndIntensity:
                desc += GetAttributeAndIntensityDescription(
                    playerAttr ?? intensityOnlyPlayerAttribute, 
                    enemyAttr ?? intensityOnlyEnemyAttribute, 
                    intensity
                );
                break;
        }
        
        return desc;
    }
    
    private string GetFixedDescription()
    {
        string desc = $"<color=#90EE90><b>✓ Você Ganha:</b></color>\n";
        desc += $"+{fixedPlayerValue} {AttributeHelper.GetDisplayName(fixedPlayerAttribute)}\n\n";
        
        desc += $"<color=#FF6B6B><b>✗ Custo:</b></color>\n";
        desc += $"Inimigos ganham: +{fixedEnemyValue} {AttributeHelper.GetDisplayName(fixedEnemyAttribute)}";
        
        return desc;
    }
    
    private string GetIntensityOnlyDescription(CardIntensity intensity)
    {
        int playerValue = IntensityHelper.GetScaledValue(intensity, fixedPlayerValue);
        int enemyValue = IntensityHelper.GetScaledValue(intensity, fixedEnemyValue);
        
        string desc = $"<color=#90EE90><b>✓ Você Ganha:</b></color>\n";
        desc += $"+{playerValue} {AttributeHelper.GetDisplayName(intensityOnlyPlayerAttribute)}\n\n";
        
        desc += $"<color=#FF6B6B><b>✗ Custo:</b></color>\n";
        desc += $"Inimigos ganham: +{enemyValue} {AttributeHelper.GetDisplayName(intensityOnlyEnemyAttribute)}";
        
        return desc;
    }
    
    private string GetAttributeAndIntensityDescription(CardAttribute playerAttr, CardAttribute enemyAttr, CardIntensity intensity)
    {
        int playerValue = IntensityHelper.GetScaledValue(intensity, fixedPlayerValue);
        int enemyValue = IntensityHelper.GetScaledValue(intensity, fixedEnemyValue);
        
        string desc = $"<color=#90EE90><b>✓ Você Ganha:</b></color>\n";
        desc += $"+{playerValue} {AttributeHelper.GetDisplayName(playerAttr)}\n\n";
        
        desc += $"<color=#FF6B6B><b>✗ Custo:</b></color>\n";
        desc += $"Inimigos ganham: +{enemyValue} {AttributeHelper.GetDisplayName(enemyAttr)}";
        
        return desc;
    }
    
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(cardName))
            return false;
        
        switch (cardType)
        {
            case NegotiationCardType.Fixed:
                return fixedPlayerValue != 0 || fixedEnemyValue != 0;
            
            case NegotiationCardType.IntensityOnly:
                return availableIntensities.Count > 0;
            
            case NegotiationCardType.AttributeAndIntensity:
                return availablePlayerAttributes.Count > 0 && 
                       availableEnemyAttributes.Count > 0 && 
                       availableIntensities.Count > 0;
            
            default:
                return false;
        }
    }
    
    void OnValidate()
    {
        if (availableIntensities.Count == 0)
        {
            availableIntensities.Add(CardIntensity.Low);
            availableIntensities.Add(CardIntensity.Medium);
            availableIntensities.Add(CardIntensity.High);
        }
        
        if (cardType == NegotiationCardType.AttributeAndIntensity)
        {
            if (availablePlayerAttributes.Count == 0)
            {
                availablePlayerAttributes.Add(CardAttribute.PlayerMaxHP);
                availablePlayerAttributes.Add(CardAttribute.PlayerDefense);
            }
            
            if (availableEnemyAttributes.Count == 0)
            {
                availableEnemyAttributes.Add(CardAttribute.EnemyMaxHP);
                availableEnemyAttributes.Add(CardAttribute.EnemyDefense);
            }
        }
    }
}