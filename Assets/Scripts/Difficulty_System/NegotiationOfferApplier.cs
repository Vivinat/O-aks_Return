// Assets/Scripts/Difficulty_System/NegotiationOfferApplier.cs (NOVO)

using UnityEngine;

/// <summary>
/// Aplica ofertas de negociação (tanto gerais quanto específicas de skill)
/// </summary>
public static class NegotiationOfferApplier
{
    /// <summary>
    /// Aplica UMA oferta (vantagem ou desvantagem)
    /// </summary>
    public static void ApplyOffer(NegotiationOffer offer, int finalValue)
    {
        // Verifica se é uma oferta de skill específica
        bool isSpecificSkill = offer.HasData("isSpecificSkill") && offer.GetData<bool>("isSpecificSkill");
        
        if (isSpecificSkill)
        {
            ApplySpecificSkillOffer(offer, finalValue);
        }
        else
        {
            ApplyGeneralOffer(offer, finalValue);
        }
    }
    
    /// <summary>
    /// Aplica oferta de skill específica
    /// </summary>
    private static void ApplySpecificSkillOffer(NegotiationOffer offer, int finalValue)
    {
        if (SpecificSkillModifier.Instance == null)
        {
            Debug.LogError("SpecificSkillModifier não encontrado! Adicione ao GameManager.");
            return;
        }
        
        string skillName = offer.GetData<string>("targetSkillName", "");
        
        if (string.IsNullOrEmpty(skillName))
        {
            Debug.LogError("Nome da skill não encontrado na oferta!");
            return;
        }
        
        bool modifyPower = offer.GetData<bool>("modifyPower", false);
        bool modifyManaCost = offer.GetData<bool>("modifyManaCost", false);
        
        if (modifyPower && modifyManaCost)
        {
            // Modifica ambos
            int powerChange = offer.GetData<int>("powerChange", 0);
            int manaCostChange = offer.GetData<int>("manaCostChange", 0);
            
            // Escala pelo valor final (se não for Fixed)
            if (finalValue != offer.value)
            {
                float scale = (float)finalValue / offer.value;
                powerChange = Mathf.RoundToInt(powerChange * scale);
                manaCostChange = Mathf.RoundToInt(manaCostChange * scale);
            }
            
            SpecificSkillModifier.Instance.ModifySkill(skillName, powerChange, manaCostChange);
            Debug.Log($"✅ Skill '{skillName}' modificada: Poder {powerChange:+#;-#;0}, Mana {manaCostChange:+#;-#;0}");
        }
        else if (modifyPower)
        {
            // Modifica apenas poder
            int powerChange = offer.GetData<int>("powerChange", 0);
            
            if (finalValue != offer.value)
            {
                float scale = (float)finalValue / offer.value;
                powerChange = Mathf.RoundToInt(powerChange * scale);
            }
            
            SpecificSkillModifier.Instance.ModifySkillPower(skillName, powerChange);
            Debug.Log($"✅ Skill '{skillName}': Poder {powerChange:+#;-#;0}");
        }
        else if (modifyManaCost)
        {
            // Modifica apenas custo de mana
            int manaCostChange = offer.GetData<int>("manaCostChange", 0);
            
            if (finalValue != offer.value)
            {
                float scale = (float)finalValue / offer.value;
                manaCostChange = Mathf.RoundToInt(manaCostChange * scale);
            }
            
            SpecificSkillModifier.Instance.ModifySkillManaCost(skillName, manaCostChange);
            Debug.Log($"✅ Skill '{skillName}': Custo {manaCostChange:+#;-#;0} MP");
        }
    }
    
    /// <summary>
    /// Aplica oferta geral (sistema existente)
    /// </summary>
    private static void ApplyGeneralOffer(NegotiationOffer offer, int finalValue)
    {
        if (DifficultySystem.Instance == null)
        {
            Debug.LogError("DifficultySystem não encontrado!");
            return;
        }
        
        // Aplica pelo sistema de dificuldade existente
        DifficultySystem.Instance.Modifiers.ApplyModifier(offer.targetAttribute, finalValue);
        
        Debug.Log($"✅ Modificador geral aplicado: {offer.targetAttribute} {finalValue:+#;-#;0}");
    }
}