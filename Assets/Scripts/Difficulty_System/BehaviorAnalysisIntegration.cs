// Assets/Scripts/Analytics/BehaviorAnalysisIntegration.cs

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Classe estática que serve como ponte entre o sistema de batalha e o PlayerBehaviorAnalyzer
/// </summary>
public static class BehaviorAnalysisIntegration
{
    /// <summary>
    /// Registra quando o jogador usa uma skill
    /// </summary>
    public static void OnPlayerSkillUsed(BattleAction skill, BattleEntity user)
    {
        if (PlayerBehaviorAnalyzer.Instance != null && skill != null && user != null)
        {
            PlayerBehaviorAnalyzer.Instance.RecordPlayerSkillUsage(skill, user);
        }
    }
    
    /// <summary>
    /// NOVO: Registra quando o jogador causa dano com uma skill
    /// </summary>
    public static void OnPlayerSkillDamage(BattleAction skill, int damage)
    {
        if (PlayerBehaviorAnalyzer.Instance != null && skill != null)
        {
            PlayerBehaviorAnalyzer.Instance.RecordPlayerSkillDamage(skill, damage);
        }
    }
    
    /// <summary>
    /// Registra quando o jogador recebe dano
    /// </summary>
    public static void OnPlayerDamageReceived(BattleEntity target, BattleEntity attacker, int damage)
    {
        if (PlayerBehaviorAnalyzer.Instance != null && target != null && attacker != null)
        {
            // Só registra se o target é um jogador
            if (target.characterData.team == Team.Player)
            {
                PlayerBehaviorAnalyzer.Instance.RecordPlayerDamageReceived(attacker, damage);
            }
        }
    }
    
    /// <summary>
    /// NOVO: Registra quando alguém age (para ordem de turnos)
    /// </summary>
    public static void OnTurnAction(BattleEntity actor)
    {
        if (PlayerBehaviorAnalyzer.Instance != null && actor != null)
        {
            PlayerBehaviorAnalyzer.Instance.RecordTurnAction(actor);
        }
    }
    
    /// <summary>
    /// NOVO: Registra hit individual recebido
    /// </summary>
    public static void OnPlayerHitReceived(int damage)
    {
        if (PlayerBehaviorAnalyzer.Instance != null)
        {
            PlayerBehaviorAnalyzer.Instance.RecordPlayerHitReceived(damage);
        }
    }
    
    /// <summary>
    /// Registra quando o jogador morre
    /// </summary>
    public static void OnPlayerDeath(BattleEntity player)
    {
        if (PlayerBehaviorAnalyzer.Instance != null && player != null && player.characterData.team == Team.Player)
        {
            PlayerBehaviorAnalyzer.Instance.RecordPlayerDeath();
        }
    }
    
    /// <summary>
    /// Registra quando o jogador compra algo na loja
    /// </summary>
    public static void OnShopPurchase(BattleAction item)
    {
        if (PlayerBehaviorAnalyzer.Instance != null && item != null)
        {
            PlayerBehaviorAnalyzer.Instance.RecordShopPurchase(item);
        }
    }
    
    /// <summary>
    /// Registra quando o jogador sai da loja
    /// </summary>
    public static void OnShopExit(List<BattleAction> availableItems)
    {
        if (PlayerBehaviorAnalyzer.Instance != null)
        {
            PlayerBehaviorAnalyzer.Instance.RecordShopExit(availableItems);
        }
    }
}