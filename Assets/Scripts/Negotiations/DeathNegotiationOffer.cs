using UnityEngine;

public enum DeathPenaltyType
{
    ReduceMaxHP,
    ReduceMaxMP,
    ReduceDefense,
    ReduceSpeed,
    WeakenAction,
    RemoveAction,
    RemoveItemUses,
    HalveDecisionTime,
    LoseCoins,
    IncreaseActionCosts,
    IncreaseSpecificActionCost,
    PermanentDebuff,
    PermanentVulnerability,
    WeakenAllOffensiveActions
}

[System.Serializable]
public class DeathNegotiationOffer
{
    public string title;
    public string description;
    public DeathPenaltyType penaltyType;
    public int penaltyValue;
    public BattleAction targetAction;
    public string contextInfo;
    
    public DeathNegotiationOffer(string title, string description, DeathPenaltyType type, int value = 0)
    {
        this.title = title;
        this.description = description;
        this.penaltyType = type;
        this.penaltyValue = value;
        this.contextInfo = "";
    }
    
    public void ApplyPenalty(BattleEntity player)
    {
        if (player == null || player.characterData == null)
        {
            return;
        }
        
        switch (penaltyType)
        {
            case DeathPenaltyType.ReduceMaxHP:
                ApplyMaxHPReduction(player);
                break;
                
            case DeathPenaltyType.ReduceMaxMP:
                ApplyMaxMPReduction(player);
                break;
                
            case DeathPenaltyType.ReduceDefense:
                ApplyDefenseReduction(player);
                break;
                
            case DeathPenaltyType.ReduceSpeed:
                ApplySpeedReduction(player);
                break;
                
            case DeathPenaltyType.WeakenAction:
                ApplyWeakenAction(player);
                break;
                
            case DeathPenaltyType.RemoveAction:
                ApplyRemoveAction(player);
                break;
                
            case DeathPenaltyType.RemoveItemUses:
                ApplyRemoveItemUses();
                break;
                
            case DeathPenaltyType.HalveDecisionTime:
                ApplyHalveDecisionTime();
                break;
                
            case DeathPenaltyType.LoseCoins:
                ApplyLoseCoins();
                break;
                
            case DeathPenaltyType.IncreaseActionCosts:
                ApplyIncreaseAllActionCosts();
                break;
                
            case DeathPenaltyType.IncreaseSpecificActionCost:
                ApplyIncreaseSpecificActionCost();
                break;
                
            case DeathPenaltyType.PermanentDebuff:
                ApplyPermanentDebuff(player);
                break;
                
            case DeathPenaltyType.PermanentVulnerability:
                ApplyPermanentVulnerability(player);
                break;
                
            case DeathPenaltyType.WeakenAllOffensiveActions:
                ApplyWeakenAllOffensive(player);
                break;
        }
    }
    
    #region Métodos de Aplicação Individual
    
    private void ApplyMaxHPReduction(BattleEntity player)
    {
        player.characterData.maxHp = Mathf.Max(10, player.characterData.maxHp - penaltyValue);
        
        var currentHPField = typeof(BattleEntity).GetField("currentHp", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (currentHPField != null)
        {
            int currentHP = (int)currentHPField.GetValue(player);
            if (currentHP > player.characterData.maxHp)
            {
                currentHPField.SetValue(player, player.characterData.maxHp);
            }
        }
    }
    
    private void ApplyMaxMPReduction(BattleEntity player)
    {
        player.characterData.maxMp = Mathf.Max(0, player.characterData.maxMp - penaltyValue);
        
        if (player.currentMp > player.characterData.maxMp)
        {
            player.currentMp = player.characterData.maxMp;
        }
    }
    
    private void ApplyDefenseReduction(BattleEntity player)
    {
        player.ApplyStatusEffect(StatusEffectType.DefenseDown, penaltyValue, 999);
    }
    
    private void ApplySpeedReduction(BattleEntity player)
    {
        player.ApplyStatusEffect(StatusEffectType.SpeedDown, penaltyValue, 999);
    }
    
    private void ApplyWeakenAction(BattleEntity player)
    {
        if (targetAction == null)
        {
            return;
        }
        
        foreach (var effect in targetAction.effects)
        {
            if (effect.power > 0)
            {
                effect.power = Mathf.Max(1, effect.power - penaltyValue);
            }
        }
    }
    
    private void ApplyRemoveAction(BattleEntity player)
    {
        if (targetAction == null)
        {
            return;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RemoveItemFromInventory(targetAction);
        }
    }
    
    private void ApplyRemoveItemUses()
    {
        if (targetAction == null || !targetAction.isConsumable)
        {
            return;
        }
        
        targetAction.currentUses = Mathf.Max(0, targetAction.currentUses - penaltyValue);
        
        if (targetAction.currentUses <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RemoveItemFromInventory(targetAction);
            }
        }
    }
    
    private void ApplyHalveDecisionTime()
    {
        BattleHUD battleHUD = Object.FindObjectOfType<BattleHUD>();
        if (battleHUD != null)
        {
            battleHUD.SetDecisionTimeMultiplier(0.5f);
        }
    }
    
    private void ApplyLoseCoins()
    {
        if (GameManager.Instance?.CurrencySystem != null)
        {
            GameManager.Instance.CurrencySystem.RemoveCoins(penaltyValue);
        }
    }
    
    private void ApplyIncreaseAllActionCosts()
    {
        if (GameManager.Instance?.PlayerBattleActions == null)
        {
            return;
        }
        
        foreach (var action in GameManager.Instance.PlayerBattleActions)
        {
            if (action != null)
            {
                action.manaCost = Mathf.Max(0, action.manaCost + penaltyValue);
            }
        }
    }
    
    private void ApplyIncreaseSpecificActionCost()
    {
        if (targetAction == null)
        {
            return;
        }
        
        targetAction.manaCost = Mathf.Max(0, targetAction.manaCost + penaltyValue);
    }
    
    private void ApplyPermanentDebuff(BattleEntity player)
    {
        player.ApplyStatusEffect(StatusEffectType.AttackDown, penaltyValue, 999);
    }
    
    private void ApplyPermanentVulnerability(BattleEntity player)
    {
        player.ApplyStatusEffect(StatusEffectType.Vulnerable, penaltyValue, 999);
    }
    
    private void ApplyWeakenAllOffensive(BattleEntity player)
    {
        player.ApplyStatusEffect(StatusEffectType.AttackDown, penaltyValue, 999);
    }
    
    #endregion
    
    public string GetFormattedDescription()
    {
        string baseDesc = description;
        
        string impactInfo = GetImpactDescription();
        if (!string.IsNullOrEmpty(impactInfo))
        {
            baseDesc += $"\n\n<color=#FF6B6B><b>Impacto:</b></color> {impactInfo}";
        }
        
        if (!string.IsNullOrEmpty(contextInfo))
        {
            baseDesc += $"\n\n<color=#888888><i>{contextInfo}</i></color>";
        }
        
        return baseDesc;
    }
    
    private string GetImpactDescription()
    {
        switch (penaltyType)
        {
            case DeathPenaltyType.ReduceMaxHP:
                return $"Você terá {penaltyValue} HP a menos em todas as batalhas futuras.";
                
            case DeathPenaltyType.ReduceMaxMP:
                return $"Suas reservas de mana serão {penaltyValue} pontos menores.";
                
            case DeathPenaltyType.ReduceDefense:
                return $"Você receberá {penaltyValue} de dano a mais de TODOS os ataques.";
                
            case DeathPenaltyType.ReduceSpeed:
                return "Inimigos agirão com mais frequência antes de você.";
                
            case DeathPenaltyType.WeakenAction:
                return targetAction != null ? 
                    $"'{targetAction.actionName}' causará menos dano/efeito." : 
                    "Uma habilidade será permanentemente enfraquecida.";
                
            case DeathPenaltyType.RemoveAction:
                return targetAction != null ? 
                    $"Você NÃO poderá mais usar '{targetAction.actionName}'." : 
                    "Uma habilidade será completamente perdida.";
                
            case DeathPenaltyType.RemoveItemUses:
                return "Você terá menos usos disponíveis deste item.";
                
            case DeathPenaltyType.HalveDecisionTime:
                return "Você terá que pensar MUITO mais rápido em combate.";
                
            case DeathPenaltyType.LoseCoins:
                return "Moedas perdidas não retornam. Progresso econômico perdido.";
                
            case DeathPenaltyType.IncreaseActionCosts:
                return $"Você gastará {penaltyValue} MP a mais em CADA habilidade.";
                
            case DeathPenaltyType.IncreaseSpecificActionCost:
                return targetAction != null ?
                    $"'{targetAction.actionName}' consumirá mais mana por uso." :
                    "Uma habilidade custará mais mana.";
                
            case DeathPenaltyType.PermanentDebuff:
                return $"Seu ataque será {penaltyValue} pontos menor pelo resto da batalha.";
                
            case DeathPenaltyType.PermanentVulnerability:
                return $"Você receberá {penaltyValue}% mais dano de TODAS as fontes.";
                
            case DeathPenaltyType.WeakenAllOffensiveActions:
                return "TODAS as suas skills de ataque serão enfraquecidas.";
                
            default:
                return "";
        }
    }
    
    public int GetSeverityLevel()
    {
        switch (penaltyType)
        {
            case DeathPenaltyType.RemoveAction:
            case DeathPenaltyType.WeakenAllOffensiveActions:
                return 5;
                
            case DeathPenaltyType.ReduceMaxHP:
            case DeathPenaltyType.HalveDecisionTime:
            case DeathPenaltyType.PermanentVulnerability:
                return 4;
                
            case DeathPenaltyType.ReduceDefense:
            case DeathPenaltyType.IncreaseActionCosts:
            case DeathPenaltyType.PermanentDebuff:
                return 3;
                
            case DeathPenaltyType.ReduceMaxMP:
            case DeathPenaltyType.WeakenAction:
            case DeathPenaltyType.LoseCoins:
                return 2;
                
            case DeathPenaltyType.ReduceSpeed:
            case DeathPenaltyType.RemoveItemUses:
            case DeathPenaltyType.IncreaseSpecificActionCost:
                return 1;
                
            default:
                return 2;
        }
    }
}