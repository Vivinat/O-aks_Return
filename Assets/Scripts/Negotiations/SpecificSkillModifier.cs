using UnityEngine;

// Sistema simples para modificar skills específicas
public class SpecificSkillModifier : MonoBehaviour
{
    public static SpecificSkillModifier Instance { get; private set; }
    
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
    
    // Modifica o poder de uma skill específica pelo nome
    public void ModifySkillPower(string skillName, int powerChange)
    {
        BattleAction skill = FindSkillByName(skillName);
        
        foreach (var effect in skill.effects)
        {
            if (effect.effectType == ActionType.Attack)
            {
                int oldPower = effect.power;
                effect.power = Mathf.Max(1, effect.power + powerChange);
                DebugLog($"'{skillName}': Poder {oldPower} → {effect.power} ({powerChange:+#;-#;0})");
            }
        }
    }
    
    // Modifica o custo de mana de uma skill específica pelo nome
    public void ModifySkillManaCost(string skillName, int manaCostChange)
    {
        BattleAction skill = FindSkillByName(skillName);
        
        int oldCost = skill.manaCost;
        skill.manaCost = Mathf.Max(0, skill.manaCost + manaCostChange);
        DebugLog($"'{skillName}': Custo {oldCost} MP → {skill.manaCost} MP ({manaCostChange:+#;-#;0})");
    }
    public void ModifySkill(string skillName, int powerChange, int manaCostChange)
    {
        BattleAction skill = FindSkillByName(skillName);
        
        if (skill == null)
        {
            DebugLog($"Skill '{skillName}' não encontrada!");
            return;
        }
        
        foreach (var effect in skill.effects)
        {
            if (effect.effectType == ActionType.Attack)
            {
                effect.power = Mathf.Max(1, effect.power + powerChange);
            }
        }
        
        skill.manaCost = Mathf.Max(0, skill.manaCost + manaCostChange);
        
        DebugLog($"'{skillName}' modificada: Poder {powerChange:+#;-#;0}, Mana {manaCostChange:+#;-#;0}");
    }
    
    // Encontra uma skill pelo nome nas ações do jogador
    private BattleAction FindSkillByName(string skillName)
    {
        
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