// Assets/Scripts/Battle/BattleActionRuntimeCopies.cs
// Sistema APENAS PARA CONSULTA - Mantém cópias das habilidades do jogador com modificadores aplicados

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Mantém cópias runtime das BattleActions do jogador com modificadores aplicados
/// APENAS PARA CONSULTA - Não afeta a lógica de batalha
/// </summary>
public class BattleActionRuntimeCopies : MonoBehaviour
{
    public static BattleActionRuntimeCopies Instance { get; private set; }
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    // Cópias das ações do jogador com modificadores aplicados
    private Dictionary<string, BattleAction> playerActionCopies = new Dictionary<string, BattleAction>();
    
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
    /// Inicializa cópias das ações do jogador
    /// Chamado quando o jogador é criado/carregado
    /// </summary>
    public void InitializePlayerActions(List<BattleAction> originalActions)
    {
        if (originalActions == null) return;
        
        playerActionCopies.Clear();
        
        foreach (var action in originalActions)
        {
            if (action == null) continue;
            
            // Cria cópia da ação
            BattleAction copy = ScriptableObject.Instantiate(action);
            copy.name = action.name; // Mantém o nome original
            
            playerActionCopies[action.name] = copy;
        }
        
        // Aplica modificadores atuais às cópias
        UpdateAllCopies();
        
        DebugLog($"Inicializadas {playerActionCopies.Count} cópias de ações do jogador");
    }
    
    /// <summary>
    /// Atualiza todas as cópias com os modificadores atuais do DifficultySystem
    /// Chamado após uma negociação
    /// </summary>
    public void UpdateAllCopies()
    {
        if (DifficultySystem.Instance == null) return;
        
        var modifiers = DifficultySystem.Instance.Modifiers;
        
        foreach (var kvp in playerActionCopies)
        {
            BattleAction copy = kvp.Value;
            
            if (copy.effects == null) continue;
            
            foreach (var effect in copy.effects)
            {
                // Reseta para valor base primeiro (importante!)
                // Nota: Assumimos que o valor atual já é o base, pois é uma cópia fresca
                
                // Aplica modificador geral de poder (ataque)
                if (modifiers.playerActionPowerModifier != 0 && effect.effectType == ActionType.Attack)
                {
                    effect.power = Mathf.Max(1, effect.power + modifiers.playerActionPowerModifier);
                }
                
                // Modificador específico de ataques ofensivos
                if (modifiers.playerOffensiveActionPowerModifier != 0 && effect.effectType == ActionType.Attack)
                {
                    effect.power = Mathf.Max(1, effect.power + modifiers.playerOffensiveActionPowerModifier);
                }
                
                // Modificador específico de habilidades defensivas (cura/buff)
                if (modifiers.playerDefensiveActionPowerModifier != 0)
                {
                    if (effect.effectType == ActionType.Heal || effect.effectType == ActionType.Buff)
                    {
                        effect.power = Mathf.Max(1, effect.power + modifiers.playerDefensiveActionPowerModifier);
                    }
                }
                
                // Modificador específico de AOE
                if (modifiers.playerAOEActionPowerModifier != 0 && IsAOEAction(copy))
                {
                    if (effect.effectType == ActionType.Attack)
                    {
                        effect.power = Mathf.Max(1, effect.power + modifiers.playerAOEActionPowerModifier);
                    }
                }
                
                // Modificador específico de single-target
                if (modifiers.playerSingleTargetActionPowerModifier != 0 && IsSingleTargetAction(copy))
                {
                    if (effect.effectType == ActionType.Attack)
                    {
                        effect.power = Mathf.Max(1, effect.power + modifiers.playerSingleTargetActionPowerModifier);
                    }
                }
            }
            
            // Modificador de custo de mana
            if (modifiers.playerActionManaCostModifier != 0)
            {
                copy.manaCost = Mathf.Max(0, copy.manaCost + modifiers.playerActionManaCostModifier);
            }
        }
        
        DebugLog("Todas as cópias de ações foram atualizadas com os modificadores atuais");
    }
    
    /// <summary>
    /// Retorna a cópia modificada de uma ação para consulta
    /// Usado pelos tooltips
    /// </summary>
    public BattleAction GetModifiedActionCopy(string actionName)
    {
        if (string.IsNullOrEmpty(actionName)) return null;
        
        if (playerActionCopies.TryGetValue(actionName, out BattleAction copy))
        {
            return copy;
        }
        
        DebugLog($"⚠️ Cópia não encontrada para: {actionName}");
        return null;
    }
    
    /// <summary>
    /// Retorna a cópia modificada de uma ação para consulta (por referência ao original)
    /// </summary>
    public BattleAction GetModifiedActionCopy(BattleAction originalAction)
    {
        if (originalAction == null) return null;
        return GetModifiedActionCopy(originalAction.name);
    }
    
    private bool IsAOEAction(BattleAction action)
    {
        return action.targetType == TargetType.AllEnemies || 
               action.targetType == TargetType.AllAllies || 
               action.targetType == TargetType.Everyone;
    }
    
    private bool IsSingleTargetAction(BattleAction action)
    {
        return action.targetType == TargetType.SingleEnemy || 
               action.targetType == TargetType.SingleAlly || 
               action.targetType == TargetType.Self;
    }
    
    private void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"<color=cyan>[BattleActionRuntimeCopies]</color> {message}");
        }
    }
    
    void OnDestroy()
    {
        // Limpa as cópias
        foreach (var copy in playerActionCopies.Values)
        {
            if (copy != null)
            {
                Destroy(copy);
            }
        }
        
        playerActionCopies.Clear();
    }
}