// Assets/Scripts/Difficulty_System/SpecificSkillModifier.cs (NOVO - Sistema Simples)

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sistema SIMPLES para modificar skills específicas
/// Modifica diretamente o ScriptableObject
/// </summary>
public class SpecificSkillModifier : MonoBehaviour
{
    public static SpecificSkillModifier Instance { get; private set; }
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Modifica o PODER de uma skill específica pelo nome
    /// </summary>
    public void ModifySkillPower(string skillName, int powerChange)
    {
        BattleAction skill = FindSkillByName(skillName);
        
        if (skill == null)
        {
            DebugLog($"⚠️ Skill '{skillName}' não encontrada!");
            return;
        }
        
        // Modifica TODOS os efeitos da skill
        foreach (var effect in skill.effects)
        {
            if (effect.effectType == ActionType.Attack)
            {
                int oldPower = effect.power;
                effect.power = Mathf.Max(1, effect.power + powerChange);
                DebugLog($"✅ '{skillName}': Poder {oldPower} → {effect.power} ({powerChange:+#;-#;0})");
            }
        }
    }
    
    /// <summary>
    /// Modifica o CUSTO DE MANA de uma skill específica pelo nome
    /// </summary>
    public void ModifySkillManaCost(string skillName, int manaCostChange)
    {
        BattleAction skill = FindSkillByName(skillName);
        
        if (skill == null)
        {
            DebugLog($"⚠️ Skill '{skillName}' não encontrada!");
            return;
        }
        
        // Modifica custo de mana
        int oldCost = skill.manaCost;
        skill.manaCost = Mathf.Max(0, skill.manaCost + manaCostChange);
        DebugLog($"✅ '{skillName}': Custo {oldCost} MP → {skill.manaCost} MP ({manaCostChange:+#;-#;0})");
    }
    
    /// <summary>
    /// Modifica PODER e CUSTO de uma skill de uma vez
    /// </summary>
    public void ModifySkill(string skillName, int powerChange, int manaCostChange)
    {
        BattleAction skill = FindSkillByName(skillName);
        
        if (skill == null)
        {
            DebugLog($"⚠️ Skill '{skillName}' não encontrada!");
            return;
        }
        
        // Modifica poder
        foreach (var effect in skill.effects)
        {
            if (effect.effectType == ActionType.Attack)
            {
                effect.power = Mathf.Max(1, effect.power + powerChange);
            }
        }
        
        // Modifica mana
        skill.manaCost = Mathf.Max(0, skill.manaCost + manaCostChange);
        
        DebugLog($"✅ '{skillName}' modificada: Poder {powerChange:+#;-#;0}, Mana {manaCostChange:+#;-#;0}");
    }
    
    /// <summary>
    /// Encontra uma skill pelo nome nas ações do jogador
    /// </summary>
    private BattleAction FindSkillByName(string skillName)
    {
        if (GameManager.Instance == null || GameManager.Instance.PlayerBattleActions == null)
        {
            DebugLog("⚠️ GameManager ou PlayerBattleActions não encontrado!");
            return null;
        }
        
        foreach (var action in GameManager.Instance.PlayerBattleActions)
        {
            if (action != null && action.actionName == skillName)
            {
                return action;
            }
        }
        
        return null;
    }
    
    private void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"<color=cyan>[SkillModifier]</color> {message}");
        }
    }
}