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
                effectName = "Ataque Aumentado";
                description = $"Attack increased by {power}";
                break;
            case StatusEffectType.AttackDown:
                effectName = "Ataque Reduzido";
                description = $"Attack decreased by {power}";
                break;
            case StatusEffectType.DefenseUp:
                effectName = "Defesa Aumentada";
                description = $"Defense increased by {power}";
                break;
            case StatusEffectType.DefenseDown:
                effectName = "Defesa Reduzida";
                description = $"Defense decreased by {power}";
                break;
            case StatusEffectType.SpeedUp:
                effectName = "Velocidade Aumentada";
                description = $"Speed increased by {power}%";
                break;
            case StatusEffectType.SpeedDown:
                effectName = "Velocidade Reduzida";
                description = $"Speed decreased by {power}%";
                break;
            case StatusEffectType.Poison:
                effectName = "Veneno";
                description = $"Takes {power} damage per turn";
                break;
            case StatusEffectType.Regeneration:
                effectName = "Regeneração";
                description = $"Heals {power} HP per turn";
                break;
            case StatusEffectType.Vulnerable:
                effectName = "Vulnerável";
                description = $"Takes {power}% more damage";
                break;
            case StatusEffectType.Protected:
                effectName = "Protegido";
                description = $"Takes {power}% less damage";
                break;
            case StatusEffectType.Blessed:
                effectName = "Abençoado";
                description = $"Divine protection heals {power} HP per turn";
                break;
            case StatusEffectType.Cursed:
                effectName = "Amaldiçoado";
                description = $"Dark curse deals {power} damage per turn";
                break;
        }
    }
    
    // Retorna true se o efeito deve ser removido
    public bool ProcessTurnEffect(BattleEntity entity)
    {
        remainingTurns--;
        
        switch (type)
        {
            case StatusEffectType.Poison:
            case StatusEffectType.Cursed:
                entity.TakeDamage(power, null, true);
                Debug.Log($"{entity.characterData.characterName} takes {power} damage from {effectName} (ignores defense)");
                break;
                
            case StatusEffectType.Regeneration:
            case StatusEffectType.Blessed:
                entity.Heal(power);
                Debug.Log($"{entity.characterData.characterName} heals {power} HP from {effectName}");
                break;
        }
        
        return remainingTurns <= 0;
    }
}