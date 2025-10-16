// Assets/Scripts/Negotiations/DeathNegotiationOffer.cs (VERSÃO EXPANDIDA)

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tipos de penalidades que podem ser aplicadas em troca de reviver
/// EXPANDIDO: Agora com 13 tipos diferentes de penalidades
/// </summary>
public enum DeathPenaltyType
{
    ReduceMaxHP,                    // Reduz HP máximo permanentemente
    ReduceMaxMP,                    // Reduz MP máximo permanentemente
    ReduceDefense,                  // Reduz defesa permanentemente (afeta TODOS os tipos de dano)
    ReduceSpeed,                    // Reduz velocidade permanentemente
    WeakenAction,                   // Enfraquece uma BattleAction específica
    RemoveAction,                   // Remove uma BattleAction do inventário
    RemoveItemUses,                 // Remove usos de um consumível
    HalveDecisionTime,              // Reduz tempo de decisão pela metade
    LoseCoins,                      // Perde moedas
    IncreaseActionCosts,            // Aumenta custo de mana de TODAS as ações
    IncreaseSpecificActionCost,     // NOVO: Aumenta custo de mana de UMA ação específica
    PermanentDebuff,                // Aplica debuff de ataque que dura a batalha toda
    PermanentVulnerability,         // NOVO: Aumenta dano recebido permanentemente
    WeakenAllOffensiveActions       // NOVO: Enfraquece todas as skills ofensivas
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
    /// Aplica a penalidade ao jogador - VERSÃO EXPANDIDA
    /// </summary>
    public void ApplyPenalty(BattleEntity player)
    {
        if (player == null || player.characterData == null)
        {
            Debug.LogError("[DeathPenalty] Jogador inválido!");
            return;
        }
        
        Debug.Log($"[DeathPenalty] Aplicando: {title} (Tipo: {penaltyType}, Valor: {penaltyValue})");
        
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
                
            default:
                Debug.LogWarning($"[DeathPenalty] Tipo desconhecido: {penaltyType}");
                break;
        }
        
        Debug.Log($"[DeathPenalty] Penalidade '{title}' aplicada com sucesso!");
    }
    
    #region Métodos de Aplicação Individual
    
    private void ApplyMaxHPReduction(BattleEntity player)
    {
        int oldMaxHP = player.characterData.maxHp;
        player.characterData.maxHp = Mathf.Max(10, oldMaxHP - penaltyValue);
        
        // Garante que HP atual não exceda o novo máximo
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
        
        Debug.Log($"[Penalty] HP máximo: {oldMaxHP} → {player.characterData.maxHp} (-{penaltyValue})");
    }
    
    private void ApplyMaxMPReduction(BattleEntity player)
    {
        int oldMaxMP = player.characterData.maxMp;
        player.characterData.maxMp = Mathf.Max(0, oldMaxMP - penaltyValue);
        
        // Garante que MP atual não exceda o novo máximo
        if (player.currentMp > player.characterData.maxMp)
        {
            player.currentMp = player.characterData.maxMp;
        }
        
        Debug.Log($"[Penalty] MP máximo: {oldMaxMP} → {player.characterData.maxMp} (-{penaltyValue})");
    }
    
    private void ApplyDefenseReduction(BattleEntity player)
    {
        // NOVO: Usa status effect ao invés de modificação permanente
        player.ApplyStatusEffect(StatusEffectType.DefenseDown, penaltyValue, 999);
        Debug.Log($"[Penalty] Defesa reduzida em {penaltyValue} por 999 turnos (até fim da batalha)");
        Debug.Log($"[Penalty] AVISO: Defesa protege contra TODOS os ataques (exceto veneno)!");
    }
    
    private void ApplySpeedReduction(BattleEntity player)
    {
        // NOVO: Usa status effect ao invés de modificação permanente
        player.ApplyStatusEffect(StatusEffectType.SpeedDown, penaltyValue, 999);
        Debug.Log($"[Penalty] Velocidade reduzida em {penaltyValue} por 999 turnos (até fim da batalha)");
    }
    
    private void ApplyWeakenAction(BattleEntity player)
    {
        if (targetAction == null)
        {
            Debug.LogWarning("[Penalty] Nenhuma ação alvo especificada para enfraquecer!");
            return;
        }
        
        bool wasWeakened = false;
        foreach (var effect in targetAction.effects)
        {
            if (effect.power > 0)
            {
                int oldPower = effect.power;
                effect.power = Mathf.Max(1, effect.power - penaltyValue);
                Debug.Log($"[Penalty] '{targetAction.actionName}' - Poder: {oldPower} → {effect.power}");
                wasWeakened = true;
            }
        }
        
        if (!wasWeakened)
        {
            Debug.LogWarning($"[Penalty] Ação '{targetAction.actionName}' não pôde ser enfraquecida!");
        }
    }
    
    private void ApplyRemoveAction(BattleEntity player)
    {
        if (targetAction == null)
        {
            Debug.LogWarning("[Penalty] Nenhuma ação alvo especificada para remover!");
            return;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RemoveItemFromInventory(targetAction);
            Debug.Log($"[Penalty] Ação '{targetAction.actionName}' removida permanentemente!");
        }
        else
        {
            Debug.LogError("[Penalty] GameManager não encontrado!");
        }
    }
    
    private void ApplyRemoveItemUses()
    {
        if (targetAction == null || !targetAction.isConsumable)
        {
            Debug.LogWarning("[Penalty] Item consumível inválido!");
            return;
        }
        
        int oldUses = targetAction.currentUses;
        targetAction.currentUses = Mathf.Max(0, oldUses - penaltyValue);
        Debug.Log($"[Penalty] '{targetAction.actionName}' - Usos: {oldUses} → {targetAction.currentUses}");
        
        // Remove do inventário se acabaram os usos
        if (targetAction.currentUses <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RemoveItemFromInventory(targetAction);
                Debug.Log($"[Penalty] '{targetAction.actionName}' removido - sem usos!");
            }
        }
    }
    
    private void ApplyHalveDecisionTime()
    {
        BattleHUD battleHUD = Object.FindObjectOfType<BattleHUD>();
        if (battleHUD != null)
        {
            battleHUD.SetDecisionTimeMultiplier(0.5f);
            Debug.Log("[Penalty] Tempo de decisão reduzido para 50%!");
        }
        else
        {
            Debug.LogWarning("[Penalty] BattleHUD não encontrado!");
        }
    }
    
    private void ApplyLoseCoins()
    {
        if (GameManager.Instance?.CurrencySystem != null)
        {
            int oldCoins = GameManager.Instance.CurrencySystem.CurrentCoins;
            GameManager.Instance.CurrencySystem.RemoveCoins(penaltyValue);
            int newCoins = GameManager.Instance.CurrencySystem.CurrentCoins;
            Debug.Log($"[Penalty] Moedas: {oldCoins} → {newCoins} (-{penaltyValue})");
        }
        else
        {
            Debug.LogWarning("[Penalty] Sistema de moedas não encontrado!");
        }
    }
    
    private void ApplyIncreaseAllActionCosts()
    {
        if (GameManager.Instance?.PlayerBattleActions == null)
        {
            Debug.LogWarning("[Penalty] PlayerBattleActions não encontrado!");
            return;
        }
        
        int actionsAffected = 0;
        foreach (var action in GameManager.Instance.PlayerBattleActions)
        {
            if (action != null)
            {
                int oldCost = action.manaCost;
                action.manaCost = Mathf.Max(0, oldCost + penaltyValue);
                Debug.Log($"[Penalty] '{action.actionName}' - Custo MP: {oldCost} → {action.manaCost}");
                actionsAffected++;
            }
        }
        
        Debug.Log($"[Penalty] {actionsAffected} ações tiveram custo aumentado em +{penaltyValue} MP");
    }
    
    private void ApplyIncreaseSpecificActionCost()
    {
        if (targetAction == null)
        {
            Debug.LogWarning("[Penalty] Nenhuma ação alvo especificada!");
            return;
        }
        
        int oldCost = targetAction.manaCost;
        targetAction.manaCost = Mathf.Max(0, oldCost + penaltyValue);
        Debug.Log($"[Penalty] '{targetAction.actionName}' - Custo MP: {oldCost} → {targetAction.manaCost} (+{penaltyValue})");
    }
    
    private void ApplyPermanentDebuff(BattleEntity player)
    {
        // Aplica debuff de ataque com duração muito alta (999 turnos = resto da batalha)
        player.ApplyStatusEffect(StatusEffectType.AttackDown, penaltyValue, 999);
        Debug.Log($"[Penalty] Debuff permanente aplicado: -{penaltyValue} ataque por 999 turnos");
    }
    
    private void ApplyPermanentVulnerability(BattleEntity player)
    {
        // Aplica vulnerabilidade permanente (aumenta dano recebido)
        player.ApplyStatusEffect(StatusEffectType.Vulnerable, penaltyValue, 999);
        Debug.Log($"[Penalty] Vulnerabilidade permanente: +{penaltyValue}% dano recebido por 999 turnos");
    }
    
    private void ApplyWeakenAllOffensive(BattleEntity player)
    {
        // NOVO: Usa status effect ao invés de modificar ações permanentemente
        // AttackDown já reduz o ataque base do personagem, que afeta todas skills ofensivas
        player.ApplyStatusEffect(StatusEffectType.AttackDown, penaltyValue, 999);
        
        Debug.Log($"[Penalty] Todas ações ofensivas enfraquecidas: -{penaltyValue} poder via AttackDown (999 turnos)");
        Debug.Log($"[Penalty] Este efeito reduz o modificador de ataque usado em GetModifiedAttackPower()");
    }
    
    #endregion
    
    /// <summary>
    /// Retorna descrição formatada para UI
    /// </summary>
    public string GetFormattedDescription()
    {
        string baseDesc = description;
        
        // Adiciona detalhes sobre o impacto
        string impactInfo = GetImpactDescription();
        if (!string.IsNullOrEmpty(impactInfo))
        {
            baseDesc += $"\n\n<color=#FF6B6B><b>Impacto:</b></color> {impactInfo}";
        }
        
        // Adiciona contexto se disponível
        if (!string.IsNullOrEmpty(contextInfo))
        {
            baseDesc += $"\n\n<color=#888888><i>{contextInfo}</i></color>";
        }
        
        return baseDesc;
    }
    
    /// <summary>
    /// Gera descrição do impacto da penalidade
    /// </summary>
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
    
    /// <summary>
    /// Retorna a severidade da penalidade (1-5)
    /// </summary>
    public int GetSeverityLevel()
    {
        switch (penaltyType)
        {
            case DeathPenaltyType.RemoveAction:
            case DeathPenaltyType.WeakenAllOffensiveActions:
                return 5; // Muito severo
                
            case DeathPenaltyType.ReduceMaxHP:
            case DeathPenaltyType.HalveDecisionTime:
            case DeathPenaltyType.PermanentVulnerability:
                return 4; // Severo
                
            case DeathPenaltyType.ReduceDefense:
            case DeathPenaltyType.IncreaseActionCosts:
            case DeathPenaltyType.PermanentDebuff:
                return 3; // Moderado-Alto
                
            case DeathPenaltyType.ReduceMaxMP:
            case DeathPenaltyType.WeakenAction:
            case DeathPenaltyType.LoseCoins:
                return 2; // Moderado
                
            case DeathPenaltyType.ReduceSpeed:
            case DeathPenaltyType.RemoveItemUses:
            case DeathPenaltyType.IncreaseSpecificActionCost:
                return 1; // Leve
                
            default:
                return 2;
        }
    }
}