// Assets/Scripts/Battle/BattleAction.cs (Enhanced Version)

using UnityEngine;
using System.Collections.Generic;

// Define o que a ação faz fundamentalmente
public enum ActionType
{
    Attack,
    Heal,
    RestoreMana,
    Buff,
    Debuff,
    Mixed // Para ações que fazem múltiplos efeitos
}

// Define quem a ação pode ter como alvo
public enum TargetType
{
    SingleEnemy,
    SingleAlly,
    Self,
    AllEnemies,
    AllAllies,
    Everyone // Afeta todos os personagens na batalha
}

// Tipos de status temporários
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
    Vulnerable, // Toma mais dano
    Protected,  // Toma menos dano
    Blessed,    // Cura a cada turno
    Cursed      // Perde vida a cada turno
}

[System.Serializable]
public class ActionEffect
{
    [Header("Primary Effect")]
    public ActionType effectType;
    public int power; // Dano, cura, ou intensidade do buff/debuff
    
    [Header("Status Effect (Optional)")]
    public StatusEffectType statusEffect = StatusEffectType.None;
    public int statusDuration = 0; // Turnos que o efeito dura
    public int statusPower = 0;    // Intensidade do status (ex: +5 defesa)
    
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
    public string description;
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

    // Helper method to determine primary action type for UI
    public ActionType GetPrimaryActionType()
    {
        if (effects.Count == 0) return ActionType.Attack;
        
        if (effects.Count == 1)
            return effects[0].effectType;
        
        return ActionType.Mixed;
    }
}