using UnityEngine;
using System.Collections.Generic;

// Ponte entre o sistema de batalha e o PlayerBehaviorAnalyzer
public static class BehaviorAnalysisIntegration
{
    public static void OnPlayerSkillUsed(BattleAction skill, BattleEntity user)
    {
        if (PlayerBehaviorAnalyzer.Instance != null && skill != null && user != null)
            PlayerBehaviorAnalyzer.Instance.RecordPlayerSkillUsage(skill, user);
    }
    
    public static void OnPlayerSkillDamage(BattleAction skill, int damage)
    {
        if (PlayerBehaviorAnalyzer.Instance != null && skill != null)
            PlayerBehaviorAnalyzer.Instance.RecordPlayerSkillDamage(skill, damage);
    }
    
    public static void OnPlayerDamageReceived(BattleEntity target, BattleEntity attacker, int damage)
    {
        if (PlayerBehaviorAnalyzer.Instance != null && target != null && attacker != null)
        {
            if (target.characterData.team == Team.Player)
                PlayerBehaviorAnalyzer.Instance.RecordPlayerDamageReceived(attacker, damage);
        }
    }
    
    public static void OnTurnAction(BattleEntity actor)
    {
        if (PlayerBehaviorAnalyzer.Instance != null && actor != null)
            PlayerBehaviorAnalyzer.Instance.RecordTurnAction(actor);
    }
    
    public static void OnPlayerHitReceived(int damage)
    {
        if (PlayerBehaviorAnalyzer.Instance != null)
            PlayerBehaviorAnalyzer.Instance.RecordPlayerHitReceived(damage);
    }
    
    public static void OnPlayerDeath(BattleEntity player)
    {
        if (PlayerBehaviorAnalyzer.Instance != null && player != null && player.characterData.team == Team.Player)
            PlayerBehaviorAnalyzer.Instance.RecordPlayerDeath();
    }
    
    public static void OnShopPurchase(BattleAction item)
    {
        if (PlayerBehaviorAnalyzer.Instance != null && item != null)
            PlayerBehaviorAnalyzer.Instance.RecordShopPurchase(item);
    }
    
    public static void OnShopExit(List<BattleAction> availableItems)
    {
        if (PlayerBehaviorAnalyzer.Instance != null)
            PlayerBehaviorAnalyzer.Instance.RecordShopExit(availableItems);
    }
}