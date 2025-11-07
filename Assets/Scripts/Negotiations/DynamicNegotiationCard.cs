using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DynamicNegotiationCard
{
    public string cardName;
    public string cardDescription;
    public Sprite cardSprite;
    
    public NegotiationCardType cardType;
    
    public NegotiationOffer playerBenefit;
    public NegotiationOffer playerCost;
    
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
        if (!advantage.isAdvantage || disadvantage.isAdvantage)
        {
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
    
    public string GetFullDescription(CardAttribute? playerAttr, CardAttribute? enemyAttr, CardIntensity intensity)
    {
        string desc = $"<b><size=110%>{cardName}</size></b>\n\n";
        desc += $"<i>{cardDescription}</i>\n\n";

        desc += $"<color=#90EE90><b>✓ Você Ganha:</b></color>\n";
        
        bool isSpecificSkillAdvantage = playerBenefit.HasData("isSpecificSkill") && 
                                         playerBenefit.GetData<bool>("isSpecificSkill");
        
        if (isSpecificSkillAdvantage)
        {
            string skillName = playerBenefit.GetData<string>("targetSkillName", "Skill");
            bool modifyPower = playerBenefit.GetData<bool>("modifyPower", false);
            bool modifyManaCost = playerBenefit.GetData<bool>("modifyManaCost", false);
            
            if (modifyPower && modifyManaCost)
            {
                int powerChange = playerBenefit.GetData<int>("powerChange", 0);
                int manaCostChange = playerBenefit.GetData<int>("manaCostChange", 0);
                
                powerChange = IntensityHelper.GetScaledValue(intensity, powerChange);
                manaCostChange = IntensityHelper.GetScaledValue(intensity, manaCostChange);
                
                desc += $"<color=#FFD700>'{skillName}'</color>:\n";
                desc += $"  Poder: <color=#90EE90>{powerChange:+#;-#;0}</color>\n";
                desc += $"  Custo: <color=#90EE90>{manaCostChange:+#;-#;0}</color> MP\n";
            }
            else if (modifyPower)
            {
                int powerChange = playerBenefit.GetData<int>("powerChange", 0);
                powerChange = IntensityHelper.GetScaledValue(intensity, powerChange);
                
                desc += $"<color=#FFD700>'{skillName}'</color>: ";
                desc += $"Poder <color=#90EE90>{powerChange:+#;-#;0}</color>\n";
            }
            else if (modifyManaCost)
            {
                int manaCostChange = playerBenefit.GetData<int>("manaCostChange", 0);
                manaCostChange = IntensityHelper.GetScaledValue(intensity, manaCostChange);
                
                desc += $"<color=#FFD700>'{skillName}'</color>: ";
                desc += $"Custo <color=#90EE90>{manaCostChange:+#;-#;0}</color> MP\n";
            }
        }
        else
        {
            CardAttribute advantageAttr = playerAttr ?? playerBenefit.targetAttribute;
            int realAdvantageValue = IntensityHelper.GetScaledValue(intensity, playerBenefit.value);
            realAdvantageValue = CorrectManaCostSignForUI(advantageAttr, realAdvantageValue, true);
            
            if (advantageAttr == CardAttribute.PlayerActionManaCost)
            {
                desc += $"Reduz custo de mana: <color=#90EE90>{Mathf.Abs(realAdvantageValue)}</color>\n";
            }
            else if (advantageAttr == CardAttribute.EnemyActionManaCost)
            {
                desc += $"Inimigos pagam <color=#90EE90>+{realAdvantageValue}</color> de mana a mais\n";
            }
            else if (advantageAttr == CardAttribute.ShopPrices)
            {
                desc += $"Itens custam: <color=#90EE90>{Mathf.Abs(realAdvantageValue)}</color> moedas a menos\n";
            }
            else
            {
                desc += $"+{realAdvantageValue} {AttributeHelper.GetDisplayName(advantageAttr)}\n";
            }
        }

        desc += $"\n<color=#FF6B6B><b>✗ Custo:</b></color>\n";

        bool isSpecificSkillCost = playerCost.HasData("isSpecificSkill") && 
                                    playerCost.GetData<bool>("isSpecificSkill");
        
        if (isSpecificSkillCost)
        {
            string skillName = playerCost.GetData<string>("targetSkillName", "Skill");
            bool modifyPower = playerCost.GetData<bool>("modifyPower", false);
            bool modifyManaCost = playerCost.GetData<bool>("modifyManaCost", false);
            
            if (modifyPower && modifyManaCost)
            {
                int powerChange = playerCost.GetData<int>("powerChange", 0);
                int manaCostChange = playerCost.GetData<int>("manaCostChange", 0);
                
                powerChange = IntensityHelper.GetScaledValue(intensity, powerChange);
                manaCostChange = IntensityHelper.GetScaledValue(intensity, manaCostChange);
                
                desc += $"<color=#FFD700>'{skillName}'</color>:\n";
                desc += $"  Poder: <color=#FF4444>{powerChange:+#;-#;0}</color>\n";
                desc += $"  Custo: <color=#FF4444>{manaCostChange:+#;-#;0}</color> MP";
            }
            else if (modifyPower)
            {
                int powerChange = playerCost.GetData<int>("powerChange", 0);
                powerChange = IntensityHelper.GetScaledValue(intensity, powerChange);
                
                desc += $"<color=#FFD700>'{skillName}'</color>: ";
                desc += $"Poder <color=#FF4444>{powerChange:+#;-#;0}</color>";
            }
            else if (modifyManaCost)
            {
                int manaCostChange = playerCost.GetData<int>("manaCostChange", 0);
                manaCostChange = IntensityHelper.GetScaledValue(intensity, manaCostChange);
                
                desc += $"<color=#FFD700>'{skillName}'</color>: ";
                desc += $"Custo <color=#FF4444>{manaCostChange:+#;-#;0}</color> MP";
            }
        }
        else
        {
            CardAttribute costAttr = enemyAttr ?? playerCost.targetAttribute;
            int realCostValue = IntensityHelper.GetScaledValue(intensity, playerCost.value);
            realCostValue = CorrectManaCostSignForUI(costAttr, realCostValue, false);

            bool costAffectsPlayer = IsPlayerAttribute(costAttr) || playerCost.affectsPlayer;

            if (costAffectsPlayer)
            {
                if (costAttr == CardAttribute.PlayerActionManaCost)
                {
                    desc += $"Aumenta custo de mana: <color=#FF4444>+{Mathf.Abs(realCostValue)}</color>";
                }
                else if (costAttr == CardAttribute.ShopPrices)
                {
                    desc += $"Itens custam: <color=#FF4444>+{Mathf.Abs(realCostValue)}</color> moedas a mais";
                }
                else
                {
                    desc += $"Você perde: <color=#FF4444>-{Mathf.Abs(realCostValue)}</color> {AttributeHelper.GetDisplayName(costAttr)}";
                }
            }
            else
            {
                if (costAttr == CardAttribute.EnemyActionManaCost)
                {
                    desc += $"Inimigos pagam <color=#FF4444>{realCostValue}</color> de mana a menos";
                }
                else
                {
                    desc += $"Inimigos ganham: <color=#FF4444>+{Mathf.Abs(realCostValue)}</color> {AttributeHelper.GetDisplayName(costAttr)}";
                }
            }
        }

        return desc;
    }
    
    private int CorrectManaCostSignForUI(CardAttribute attribute, int value, bool isAdvantage)
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
                return -Mathf.Abs(value);
            }
            else
            {
                return Mathf.Abs(value);
            }
        }
    
        if (attribute == CardAttribute.EnemyActionManaCost)
        {
            if (isAdvantage)
            {
                return Mathf.Abs(value);
            }
            else
            {
                return -Mathf.Abs(value);
            }
        }
    
        return value;
    }
    
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