// Assets/Scripts/Difficulty_System/DeathNegotiationOffer.cs

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tipos de penalidades que podem ser aplicadas em troca de reviver
/// </summary>
public enum DeathPenaltyType
{
    ReduceMaxHP,           // Reduz HP máximo permanentemente
    ReduceMaxMP,           // Reduz MP máximo permanentemente
    ReduceDefense,         // Reduz defesa permanentemente
    ReduceSpeed,           // Reduz velocidade permanentemente
    WeakenAction,          // Enfraquece uma BattleAction específica
    RemoveAction,          // Remove uma BattleAction do inventário
    RemoveItemUses,        // Remove usos de um consumível
    HalveDecisionTime,     // Reduz tempo de decisão pela metade
    LoseCoins,             // Perde moedas
    IncreaseActionCosts,   // Aumenta custo de mana de todas ações
    PermanentDebuff        // Aplica um debuff que dura a batalha toda
}

/// <summary>
/// Representa uma oferta de segunda chance
/// </summary>
[System.Serializable]
public class DeathNegotiationOffer
{
    public string title;
    public string description;
    public DeathPenaltyType penaltyType;
    
    // Dados específicos da penalidade
    public int penaltyValue;
    public BattleAction targetAction;
    public string contextInfo; // Info adicional para display
    
    public DeathNegotiationOffer(string title, string description, DeathPenaltyType type, int value = 0)
    {
        this.title = title;
        this.description = description;
        this.penaltyType = type;
        this.penaltyValue = value;
        this.contextInfo = "";
    }
    
    /// <summary>
    /// Aplica a penalidade ao jogador
    /// </summary>
    public void ApplyPenalty(BattleEntity player)
    {
        if (player == null || player.characterData == null) return;
        
        Debug.Log($"Aplicando penalidade: {title}");
        
        switch (penaltyType)
        {
            case DeathPenaltyType.ReduceMaxHP:
                player.characterData.maxHp = Mathf.Max(10, player.characterData.maxHp - penaltyValue);
                Debug.Log($"HP máximo reduzido para {player.characterData.maxHp}");
                break;
                
            case DeathPenaltyType.ReduceMaxMP:
                player.characterData.maxMp = Mathf.Max(0, player.characterData.maxMp - penaltyValue);
                Debug.Log($"MP máximo reduzido para {player.characterData.maxMp}");
                break;
                
            case DeathPenaltyType.ReduceDefense:
                player.characterData.defense = Mathf.Max(0, player.characterData.defense - penaltyValue);
                Debug.Log($"Defesa reduzida para {player.characterData.defense}");
                break;
                
            case DeathPenaltyType.ReduceSpeed:
                player.characterData.speed = Mathf.Max(0.5f, player.characterData.speed - penaltyValue);
                Debug.Log($"Velocidade reduzida para {player.characterData.speed}");
                break;
                
            case DeathPenaltyType.WeakenAction:
                if (targetAction != null)
                {
                    foreach (var effect in targetAction.effects)
                    {
                        effect.power = Mathf.Max(1, effect.power - penaltyValue);
                    }
                    Debug.Log($"Ação '{targetAction.actionName}' enfraquecida");
                }
                break;
                
            case DeathPenaltyType.RemoveAction:
                if (targetAction != null && GameManager.Instance != null)
                {
                    GameManager.Instance.RemoveItemFromInventory(targetAction);
                    Debug.Log($"Ação '{targetAction.actionName}' removida");
                }
                break;
                
            case DeathPenaltyType.RemoveItemUses:
                if (targetAction != null && targetAction.isConsumable)
                {
                    targetAction.currentUses = Mathf.Max(0, targetAction.currentUses - penaltyValue);
                    Debug.Log($"'{targetAction.actionName}' perdeu {penaltyValue} usos");
                }
                break;
                
            case DeathPenaltyType.HalveDecisionTime:
                BattleHUD battleHUD = Object.FindObjectOfType<BattleHUD>();
                if (battleHUD != null)
                {
                    battleHUD.SetDecisionTimeMultiplier(0.5f);
                    Debug.Log("Tempo de decisão reduzido pela metade");
                }
                break;
                
            case DeathPenaltyType.LoseCoins:
                if (GameManager.Instance?.CurrencySystem != null)
                {
                    GameManager.Instance.CurrencySystem.RemoveCoins(penaltyValue);
                    Debug.Log($"{penaltyValue} moedas perdidas");
                }
                break;
                
            case DeathPenaltyType.IncreaseActionCosts:
                if (GameManager.Instance?.PlayerBattleActions != null)
                {
                    foreach (var action in GameManager.Instance.PlayerBattleActions)
                    {
                        if (action != null)
                        {
                            action.manaCost = Mathf.Max(0, action.manaCost + penaltyValue);
                        }
                    }
                    Debug.Log($"Custo de mana aumentado em {penaltyValue}");
                }
                break;
                
            case DeathPenaltyType.PermanentDebuff:
                player.ApplyStatusEffect(StatusEffectType.AttackDown, penaltyValue, 999);
                Debug.Log($"Debuff permanente aplicado: -{penaltyValue} ataque");
                break;
        }
    }
    
    /// <summary>
    /// Retorna descrição formatada para UI
    /// </summary>
    public string GetFormattedDescription()
    {
        string baseDesc = description;
        
        if (!string.IsNullOrEmpty(contextInfo))
        {
            baseDesc += $"\n\n<color=#888888><i>{contextInfo}</i></color>";
        }
        
        return baseDesc;
    }
}