// Assets/Scripts/Battle/BattleAction.cs (Com Descrição Dinâmica)

using UnityEngine;
using System.Collections.Generic;
using System.Text;

public enum ActionType
{
    Attack,
    Heal,
    RestoreMana,
    Buff,
    Debuff,
    Mixed
}

public enum TargetType
{
    SingleEnemy,
    SingleAlly,
    Self,
    AllEnemies,
    AllAllies,
    Everyone
}

public enum StatusEffectType
{
    None,
    AttackUp,
    AttackDown,
    DefenseUp,
    DefenseDown,
    SpeedUp,
    SpeedDown,
    Poison,
    Regeneration,
    Vulnerable,
    Protected,
    Blessed,
    Cursed
}

[System.Serializable]
public class ActionEffect
{
    [Header("Primary Effect")]
    public ActionType effectType;
    public int power;
    
    [Header("Status Effect (Optional)")]
    public StatusEffectType statusEffect = StatusEffectType.None;
    public int statusDuration = 0;
    public int statusPower = 0;
    
    [Header("Self Effect (Optional)")]
    public bool hasSelfEffect = false;
    public ActionType selfEffectType;
    public int selfEffectPower = 0;
    public StatusEffectType selfStatusEffect = StatusEffectType.None;
    public int selfStatusDuration = 0;
    public int selfStatusPower = 0;
}

[CreateAssetMenu(fileName = "New Action", menuName = "Battle/Battle Action")]
public class BattleAction : ScriptableObject
{
    [Header("General Information")]
    public string actionName;
    
    [TextArea]
    [Tooltip("NÃO USADO - A descrição é gerada dinamicamente")]
    public string description; // Mantido por compatibilidade, mas não será usado
    public Sprite icon;

    [Header("Action Logic")]
    public TargetType targetType;

    [Header("Effects")]
    public List<ActionEffect> effects = new List<ActionEffect>();

    [Header("Cost")]
    public int manaCost;
    
    [Header("Consumable (Optional)")]
    public bool isConsumable = false;
    public int maxUses = 1;
    public int shopPrice = 10;
    
    [System.NonSerialized]
    public int currentUses;
    
    void OnEnable()
    {
        if (isConsumable && currentUses <= 0)
        {
            currentUses = maxUses;
        }
    }
    
    void Awake()
    {
        if (isConsumable && currentUses <= 0)
        {
            currentUses = maxUses;
        }
    }
    
    /// <summary>
    /// Gera a descrição completa da ação baseada nos valores atuais (versão compacta)
    /// </summary>
    public string GetDynamicDescription()
    {
        System.Text.StringBuilder desc = new System.Text.StringBuilder();
        
        // === PROCESSA EFEITOS PRINCIPAIS ===
        bool hasMainEffects = false;
        List<string> statusEffectsList = new List<string>();
        
        if (effects != null && effects.Count > 0)
        {
            foreach (var effect in effects)
            {
                // Efeito principal
                switch (effect.effectType)
                {
                    case ActionType.Attack:
                        desc.AppendLine($"<color=#ff6b6b>Dano: {effect.power}</color>");
                        hasMainEffects = true;
                        break;
                        
                    case ActionType.Heal:
                        desc.AppendLine($"<color=#51cf66>Cura: {effect.power} HP</color>");
                        hasMainEffects = true;
                        break;
                        
                    case ActionType.RestoreMana:
                        desc.AppendLine($"<color=#4dabf7>Restaura: {effect.power} MP</color>");
                        hasMainEffects = true;
                        break;
                        
                    case ActionType.Buff:
                    case ActionType.Debuff:
                        // Buffs/Debuffs são tratados via status effects
                        break;
                }
                
                // Coleta status effects
                if (effect.statusEffect != StatusEffectType.None)
                {
                    string statusDesc = GetStatusEffectDescriptionCompact(
                        effect.statusEffect, 
                        effect.statusPower, 
                        effect.statusDuration
                    );
                    statusEffectsList.Add(statusDesc);
                }
                
                // ============================================
                // CORREÇÃO: Efeito em si mesmo
                // ============================================
                if (effect.hasSelfEffect)
                {
                    // Trata dano em si mesmo
                    if (effect.selfEffectType == ActionType.Attack)
                    {
                        desc.AppendLine($"<color=#ff6b6b>⚠️ Auto-Dano: {effect.selfEffectPower} HP</color>");
                        hasMainEffects = true;
                    }
                    
                    // Trata cura em si mesmo
                    if (effect.selfEffectType == ActionType.Heal)
                    {
                        desc.AppendLine($"<color=#51cf66>Auto-Cura: {effect.selfEffectPower} HP</color>");
                        hasMainEffects = true;
                    }
                    
                    // Trata restauração de mana em si mesmo
                    if (effect.selfEffectType == ActionType.RestoreMana)
                    {
                        desc.AppendLine($"<color=#4dabf7>Auto-Restaura: {effect.selfEffectPower} MP</color>");
                        hasMainEffects = true;
                    }
                    
                    // Trata buff em si mesmo
                    if (effect.selfEffectType == ActionType.Buff)
                    {
                        // Status effect associado ao self buff
                        if (effect.selfStatusEffect != StatusEffectType.None)
                        {
                            string selfStatusDesc = GetStatusEffectDescriptionCompact(
                                effect.selfStatusEffect, 
                                effect.selfStatusPower, 
                                effect.selfStatusDuration
                            );
                            statusEffectsList.Add($"{selfStatusDesc} (em você)");
                        }
                    }
                    
                    // Trata debuff em si mesmo
                    if (effect.selfEffectType == ActionType.Debuff)
                    {
                        if (effect.selfStatusEffect != StatusEffectType.None)
                        {
                            string selfStatusDesc = GetStatusEffectDescriptionCompact(
                                effect.selfStatusEffect, 
                                effect.selfStatusPower, 
                                effect.selfStatusDuration
                            );
                            statusEffectsList.Add($"{selfStatusDesc} (em você)");
                        }
                    }
                }
            }
        }
        
        if (hasMainEffects)
        {
            desc.AppendLine(); // Linha em branco após efeitos principais
        }
        
        // === EFEITOS DE STATUS (formato compacto) ===
        if (statusEffectsList.Count > 0)
        {
            desc.AppendLine("<b>Efeitos:</b>");
            foreach (var statusDesc in statusEffectsList)
            {
                desc.AppendLine($"  • {statusDesc}");
            }
            desc.AppendLine();
        }
        
        // === INFORMAÇÕES DE CUSTO/USOS ===
        List<string> metaInfo = new List<string>();
        
        if (isConsumable)
        {
            metaInfo.Add($"<color=#ffd43b>Usos: {currentUses}/{maxUses}</color>");
        }
        else if (manaCost > 0)
        {
            metaInfo.Add($"<color=#4dabf7>Custo: {manaCost} MP</color>");
        }
        
        // Alvo
        string targetText = GetTargetTypeText();
        if (!string.IsNullOrEmpty(targetText))
        {
            metaInfo.Add($"<color=#a9a9a9>{targetText}</color>");
        }
        
        // Junta informações meta em uma linha
        if (metaInfo.Count > 0)
        {
            desc.Append(string.Join(" | ", metaInfo));
        }
        
        return desc.ToString().TrimEnd();
    }
    
    //// <summary>
    /// Descrição compacta de efeitos de status
    /// </summary>
    private string GetStatusEffectDescriptionCompact(StatusEffectType type, int power, int duration)
    {
        string colorTag = "";
        string effectName = "";
        
        switch (type)
        {
            case StatusEffectType.AttackUp:
                colorTag = "#ff6b6b";
                effectName = $"+{power} ATK ({duration}t)";
                break;
            case StatusEffectType.AttackDown:
                colorTag = "#ff6b6b";
                effectName = $"-{power} ATK ({duration}t)";
                break;
            case StatusEffectType.DefenseUp:
                colorTag = "#4dabf7";
                effectName = $"+{power} DEF ({duration}t)";
                break;
            case StatusEffectType.DefenseDown:
                colorTag = "#4dabf7";
                effectName = $"-{power} DEF ({duration}t)";
                break;
            case StatusEffectType.SpeedUp:
                colorTag = "#51cf66";
                effectName = $"+{power}% VEL ({duration}t)";
                break;
            case StatusEffectType.SpeedDown:
                colorTag = "#51cf66";
                effectName = $"-{power}% VEL ({duration}t)";
                break;
            case StatusEffectType.Poison:
                colorTag = "#a855f7";
                effectName = $"Veneno {power}/turno ({duration}t)";
                break;
            case StatusEffectType.Regeneration:
                colorTag = "#51cf66";
                effectName = $"Regen {power}/turno ({duration}t)";
                break;
            case StatusEffectType.Vulnerable:
                colorTag = "#ff6b6b";
                effectName = $"+{power}% dano recebido ({duration}t)";
                break;
            case StatusEffectType.Protected:
                colorTag = "#4dabf7";
                effectName = $"-{power}% dano recebido ({duration}t)";
                break;
            case StatusEffectType.Blessed:
                colorTag = "#ffd43b";
                effectName = $"Bênção {power}/turno ({duration}t)";
                break;
            case StatusEffectType.Cursed:
                colorTag = "#a855f7";
                effectName = $"Maldição {power}/turno ({duration}t)";
                break;
            default:
                return type.ToString();
        }
        
        return $"<color={colorTag}>{effectName}</color>";
    }

    /// <summary>
    /// Texto descritivo do tipo de alvo
    /// </summary>
    private string GetTargetTypeText()
    {
        switch (targetType)
        {
            case TargetType.Self:
                return "Você";
            case TargetType.SingleEnemy:
                return "1 Inimigo";
            case TargetType.SingleAlly:
                return "1 Aliado";
            case TargetType.AllEnemies:
                return "Todos Inimigos";
            case TargetType.AllAllies:
                return "Todos Aliados";
            default:
                return "";
        }
    }
    
    /// <summary>
    /// Converte ActionType para texto em português
    /// </summary>
    private string GetEffectTypeText(ActionType type)
    {
        switch (type)
        {
            case ActionType.Attack: return "Dano";
            case ActionType.Heal: return "Cura";
            case ActionType.RestoreMana: return "Restaura MP";
            case ActionType.Buff: return "Buff";
            case ActionType.Debuff: return "Debuff";
            case ActionType.Mixed: return "Misto";
            default: return type.ToString();
        }
    }
    
    /// <summary>
    /// Gera texto do efeito com poder
    /// </summary>
    private string GetEffectText(ActionEffect effect)
    {
        StringBuilder sb = new StringBuilder();
        
        sb.Append(GetEffectTypeText(effect.effectType));
        sb.Append(" ");
        sb.Append(effect.power);
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Converte StatusEffectType para texto em português
    /// </summary>
    private string GetStatusEffectText(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.AttackUp: return "Atq+";
            case StatusEffectType.AttackDown: return "Atq-";
            case StatusEffectType.DefenseUp: return "Def+";
            case StatusEffectType.DefenseDown: return "Def-";
            case StatusEffectType.SpeedUp: return "Vel+";
            case StatusEffectType.SpeedDown: return "Vel-";
            case StatusEffectType.Poison: return "Veneno";
            case StatusEffectType.Regeneration: return "Regen";
            case StatusEffectType.Vulnerable: return "Vulnerável";
            case StatusEffectType.Protected: return "Protegido";
            case StatusEffectType.Blessed: return "Abençoado";
            case StatusEffectType.Cursed: return "Amaldiçoado";
            default: return type.ToString();
        }
    }
    
    public bool UseAction()
    {
        if (isConsumable)
        {
            if (currentUses > 0)
            {
                currentUses--;
                Debug.Log($"{actionName} used. Remaining uses: {currentUses}");
                return currentUses > 0;
            }
            
            Debug.Log($"{actionName} has no more uses!");
            return false;
        }
        
        return true;
    }
    
    public bool CanUse()
    {
        if (isConsumable)
        {
            return currentUses > 0;
        }
        return true;
    }
    
    public void RefillUses()
    {
        if (isConsumable)
        {
            currentUses = maxUses;
        }
    }
    
    public BattleAction CreateInstance()
    {
        BattleAction instance = Instantiate(this);
        
        if (instance.isConsumable)
        {
            instance.currentUses = instance.maxUses;
        }
        
        return instance;
    }

    public ActionType GetPrimaryActionType()
    {
        if (effects.Count == 0) return ActionType.Attack;
        
        if (effects.Count == 1)
            return effects[0].effectType;
        
        return ActionType.Mixed;
    }
}