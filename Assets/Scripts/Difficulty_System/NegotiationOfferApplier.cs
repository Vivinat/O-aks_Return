// Assets/Scripts/Difficulty_System/NegotiationOfferApplier.cs (IMMEDIATE APPLICATION)

using UnityEngine;

/// <summary>
/// Aplica ofertas de negociação IMEDIATAMENTE
/// (tanto gerais quanto específicas de skill)
/// </summary>
public static class NegotiationOfferApplier
{
    /// <summary>
    /// Aplica UMA oferta (vantagem ou desvantagem) IMEDIATAMENTE
    /// </summary>
    public static void ApplyOffer(NegotiationOffer offer, int finalValue)
    {
        // Verifica se é uma oferta de skill específica
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
    
    /// <summary>
    /// ATUALIZADO: Aplica oferta de skill específica IMEDIATAMENTE no ScriptableObject
    /// </summary>
    private static void ApplySpecificSkillOfferImmediate(NegotiationOffer offer, int finalValue)
    {
        if (SpecificSkillModifier.Instance == null)
        {
            Debug.LogError("⚠️ SpecificSkillModifier não encontrado! Adicione ao GameManager.");
            return;
        }
        
        string skillName = offer.GetData<string>("targetSkillName", "");
        
        if (string.IsNullOrEmpty(skillName))
        {
            Debug.LogError("⚠️ Nome da skill não encontrado na oferta!");
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
            
            // APLICA IMEDIATAMENTE no SO
            SpecificSkillModifier.Instance.ModifySkill(skillName, powerChange, manaCostChange);
            Debug.Log($"✅ Skill '{skillName}' modificada IMEDIATAMENTE: Poder {powerChange:+#;-#;0}, Mana {manaCostChange:+#;-#;0}");
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
            
            // APLICA IMEDIATAMENTE no SO
            SpecificSkillModifier.Instance.ModifySkillPower(skillName, powerChange);
            Debug.Log($"✅ Skill '{skillName}': Poder {powerChange:+#;-#;0} (IMEDIATO)");
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
            
            // APLICA IMEDIATAMENTE no SO
            SpecificSkillModifier.Instance.ModifySkillManaCost(skillName, manaCostChange);
            Debug.Log($"✅ Skill '{skillName}': Custo {manaCostChange:+#;-#;0} MP (IMEDIATO)");
        }
    }
    
    /// <summary>
    /// ATUALIZADO: Aplica oferta geral IMEDIATAMENTE via DifficultySystem
    /// </summary>
    private static void ApplyGeneralOfferImmediate(NegotiationOffer offer, int finalValue)
    {
        if (DifficultySystem.Instance == null)
        {
            Debug.LogError("⚠️ DifficultySystem não encontrado!");
            return;
        }
        
        // IMPORTANTE: Usa ApplyModifierImmediate via DifficultySystem
        // NÃO apenas registra - APLICA no SO
        
        // DifficultySystem.ApplyNegotiation() já faz isso corretamente
        // Mas aqui estamos lidando com ofertas individuais
        
        // Chama método privado ApplyModifierImmediate via reflexão
        var method = typeof(DifficultySystem).GetMethod("ApplyModifierImmediate", 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        if (method != null)
        {
            method.Invoke(DifficultySystem.Instance, new object[] { offer.targetAttribute, finalValue, offer.affectsPlayer });
            
            // Registra no histórico
            DifficultySystem.Instance.Modifiers.RecordModifier(offer.targetAttribute, finalValue);
            
            Debug.Log($"✅ Modificador geral aplicado IMEDIATAMENTE: {offer.targetAttribute} {finalValue:+#;-#;0}");
        }
        else
        {
            Debug.LogError("⚠️ Método ApplyModifierImmediate não encontrado!");
        }
    }
}