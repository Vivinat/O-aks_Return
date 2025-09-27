// Assets/Scripts/Battle/StatusEffect.cs

using UnityEngine;

[System.Serializable]
public class StatusEffect
{
    public StatusEffectType type;
    public int power;
    public int remainingTurns;
    public string effectName;
    public string description;
    
    public StatusEffect(StatusEffectType type, int power, int duration)
    {
        this.type = type;
        this.power = power;
        this.remainingTurns = duration;
        
        SetEffectInfo();
    }
    
    private void SetEffectInfo()
    {
        switch (type)
        {
            case StatusEffectType.AttackUp:
                effectName = "Attack Boost";
                description = $"Attack increased by {power}";
                break;
            case StatusEffectType.AttackDown:
                effectName = "Attack Reduction";
                description = $"Attack decreased by {power}";
                break;
            case StatusEffectType.DefenseUp:
                effectName = "Defense Boost";
                description = $"Defense increased by {power}";
                break;
            case StatusEffectType.DefenseDown:
                effectName = "Defense Reduction";
                description = $"Defense decreased by {power}";
                break;
            case StatusEffectType.SpeedUp:
                effectName = "Speed Boost";
                description = $"Speed increased by {power}%";
                break;
            case StatusEffectType.SpeedDown:
                effectName = "Speed Reduction";
                description = $"Speed decreased by {power}%";
                break;
            case StatusEffectType.Poison:
                effectName = "Poison";
                description = $"Takes {power} damage per turn";
                break;
            case StatusEffectType.Regeneration:
                effectName = "Regeneration";
                description = $"Heals {power} HP per turn";
                break;
            case StatusEffectType.Vulnerable:
                effectName = "Vulnerable";
                description = $"Takes {power}% more damage";
                break;
            case StatusEffectType.Protected:
                effectName = "Protected";
                description = $"Takes {power}% less damage";
                break;
            case StatusEffectType.Blessed:
                effectName = "Blessed";
                description = $"Divine protection heals {power} HP per turn";
                break;
            case StatusEffectType.Cursed:
                effectName = "Cursed";
                description = $"Dark curse deals {power} damage per turn";
                break;
        }
    }
    
    public bool ProcessTurnEffect(BattleEntity entity)
    {
        remainingTurns--;
        
        switch (type)
        {
            case StatusEffectType.Poison:
            case StatusEffectType.Cursed:
                entity.TakeDamage(power);
                Debug.Log($"{entity.characterData.characterName} takes {power} damage from {effectName}");
                break;
                
            case StatusEffectType.Regeneration:
            case StatusEffectType.Blessed:
                entity.Heal(power);
                Debug.Log($"{entity.characterData.characterName} heals {power} HP from {effectName}");
                break;
        }
        
        return remainingTurns <= 0; // Returns true if effect should be removed
    }
}