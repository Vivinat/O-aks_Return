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
    
    // ... (métodos anteriores permanecem iguais)
    
    /// <summary>
    /// Gera a descrição completa da ação baseada nos valores atuais (versão compacta)
    /// </summary>
    public string GetDynamicDescription()
    {
        StringBuilder sb = new StringBuilder();
        
        // Linha 1: Alvo + MP + Usos/Preço
        sb.Append(GetTargetTypeText(targetType));
        sb.Append(" | MP:");
        sb.Append(manaCost);
        
        if (isConsumable)
        {
            sb.Append(" | Usos:");
            sb.Append(currentUses);
            sb.Append("/");
            sb.Append(maxUses);
        }
        
        if (shopPrice > 0)
        {
            sb.Append(" | $");
            sb.Append(shopPrice);
        }
        
        // Efeitos
        if (effects != null && effects.Count > 0)
        {
            sb.Append(" || ");
            
            for (int i = 0; i < effects.Count; i++)
            {
                ActionEffect effect = effects[i];
                
                if (i > 0) sb.Append(" + ");
                
                // Efeito Principal
                sb.Append(GetEffectText(effect));
                
                // Status Effect
                if (effect.statusEffect != StatusEffectType.None)
                {
                    sb.Append(" [");
                    sb.Append(GetStatusEffectText(effect.statusEffect));
                    sb.Append(":");
                    sb.Append(effect.statusPower);
                    sb.Append("/");
                    sb.Append(effect.statusDuration);
                    sb.Append("t]");
                }
                
                // Self Effect
                if (effect.hasSelfEffect)
                {
                    sb.Append(" (Si:");
                    sb.Append(GetEffectTypeText(effect.selfEffectType));
                    sb.Append(":");
                    sb.Append(effect.selfEffectPower);
                    
                    if (effect.selfStatusEffect != StatusEffectType.None)
                    {
                        sb.Append("+");
                        sb.Append(GetStatusEffectText(effect.selfStatusEffect));
                        sb.Append(":");
                        sb.Append(effect.selfStatusPower);
                        sb.Append("/");
                        sb.Append(effect.selfStatusDuration);
                        sb.Append("t");
                    }
                    
                    sb.Append(")");
                }
            }
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Converte TargetType para texto em português
    /// </summary>
    private string GetTargetTypeText(TargetType type)
    {
        switch (type)
        {
            case TargetType.SingleEnemy: return "Inimigo";
            case TargetType.SingleAlly: return "Aliado";
            case TargetType.Self: return "Si Mesmo";
            case TargetType.AllEnemies: return "Todos Inimigos";
            case TargetType.AllAllies: return "Todos Aliados";
            case TargetType.Everyone: return "Todos";
            default: return type.ToString();
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