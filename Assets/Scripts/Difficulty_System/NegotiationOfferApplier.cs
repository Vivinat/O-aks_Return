using UnityEngine;

// Aplica ofertas de negociação imediatamente
public static class NegotiationOfferApplier
{
    public static void ApplyOffer(NegotiationOffer offer, int finalValue)
    {
        bool isSpecificSkill = offer.HasData("isSpecificSkill") && offer.GetData<bool>("isSpecificSkill");
        
        if (isSpecificSkill)
        {
            ApplySpecificSkillOfferImmediate(offer, finalValue);
        }
        else
        {
            ApplyGeneralOfferImmediate(offer, finalValue);
        }
    }
    
    private static void ApplySpecificSkillOfferImmediate(NegotiationOffer offer, int finalValue)
    {
        
        string skillName = offer.GetData<string>("targetSkillName", "");
        
        bool modifyPower = offer.GetData<bool>("modifyPower", false);
        bool modifyManaCost = offer.GetData<bool>("modifyManaCost", false);
        
        if (modifyPower && modifyManaCost)
        {
            int powerChange = offer.GetData<int>("powerChange", 0);
            int manaCostChange = offer.GetData<int>("manaCostChange", 0);
            
            if (finalValue != offer.value)
            {
                float scale = (float)finalValue / offer.value;
                powerChange = Mathf.RoundToInt(powerChange * scale);
                manaCostChange = Mathf.RoundToInt(manaCostChange * scale);
            }
            
            SpecificSkillModifier.Instance.ModifySkill(skillName, powerChange, manaCostChange);
        }
        else if (modifyPower)
        {
            int powerChange = offer.GetData<int>("powerChange", 0);
            
            if (finalValue != offer.value)
            {
                float scale = (float)finalValue / offer.value;
                powerChange = Mathf.RoundToInt(powerChange * scale);
            }
            
            SpecificSkillModifier.Instance.ModifySkillPower(skillName, powerChange);
        }
        else if (modifyManaCost)
        {
            int manaCostChange = offer.GetData<int>("manaCostChange", 0);
            
            if (finalValue != offer.value)
            {
                float scale = (float)finalValue / offer.value;
                manaCostChange = Mathf.RoundToInt(manaCostChange * scale);
            }
            
            SpecificSkillModifier.Instance.ModifySkillManaCost(skillName, manaCostChange);
        }
    }
    
    private static void ApplyGeneralOfferImmediate(NegotiationOffer offer, int finalValue)
    {
        
        // Aplica modificador imediatamente via reflexão
        var method = typeof(DifficultySystem).GetMethod("ApplyModifierImmediate", 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        if (method != null)
        {
            method.Invoke(DifficultySystem.Instance, new object[] { offer.targetAttribute, finalValue, offer.affectsPlayer });
            DifficultySystem.Instance.Modifiers.RecordModifier(offer.targetAttribute, finalValue);
        }
        else
        {
            Debug.LogError("Método ApplyModifierImmediate não encontrado!");
        }
    }
}