using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EnemyAI
{
    private const float HEAL_THRESHOLD = 0.5f;
    private const float CRITICAL_HEAL_THRESHOLD = 0.25f;
    private const float ALLY_HEAL_THRESHOLD = 0.6f;
    
    /// <summary>
    /// Escolhe a melhor ação para o inimigo usando uma seleção ponderada aleatória
    /// </summary>
    public static BattleAction ChooseBestAction(BattleEntity caster, BattleEntity player, List<BattleEntity> enemyTeam)
    {
        List<BattleAction> availableActions = caster.characterData.battleActions
            .Where(a => caster.currentMp >= a.manaCost && (!a.isConsumable || a.CanUse()))
            .ToList();

        if (!availableActions.Any())
        {
            Debug.LogWarning($"{caster.characterData.characterName} não tem ações disponíveis!");
            return null;
        }

        Dictionary<BattleAction, float> actionScores = new Dictionary<BattleAction, float>();
        foreach (BattleAction action in availableActions)
        {
            float score = EvaluateAction(action, caster, player, enemyTeam);
            actionScores[action] = score;
        }

        var viableActions = actionScores.Where(kvp => kvp.Value > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (!viableActions.Any())
        {
            return actionScores.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;
        }
        
        float totalWeight = viableActions.Sum(kvp => kvp.Value);
        float randomValue = Random.Range(0, totalWeight);
        
        foreach (var action in viableActions)
        {
            randomValue -= action.Value;
            if (randomValue <= 0)
            {
                return action.Key;
            }
        }
        return viableActions.Keys.First();
    }

    private static float EvaluateAction(BattleAction action, BattleEntity caster, BattleEntity player, List<BattleEntity> enemyTeam)
    {
        float score = 0f;
        ActionType primaryType = action.GetPrimaryActionType();
        
        switch (primaryType)
        {
            case ActionType.Heal:
                score = EvaluateHealAction(action, caster, enemyTeam);
                break;
                
            case ActionType.Attack:
                score = EvaluateAttackAction(action);
                break;
                
            case ActionType.Buff:
                score = EvaluateBuffAction(action, caster, enemyTeam);
                break;
                
            case ActionType.Debuff:
                score = EvaluateDebuffAction(action, player);
                break;
                
            case ActionType.Mixed:
                score = 60f; 
                break;
        }
        score += Random.Range(-5f, 5f);
        
        return score;
    }

    private static float EvaluateHealAction(BattleAction action, BattleEntity caster, List<BattleEntity> enemyTeam)
    {
        if (action.targetType == TargetType.Self)
        {
            float hpPercent = (float)caster.GetCurrentHP() / caster.GetMaxHP();
            
            if (hpPercent > 0.9f)
                return -100f;
            
            if (hpPercent < CRITICAL_HEAL_THRESHOLD)
                return 100f;
            
            if (hpPercent < HEAL_THRESHOLD)
                return 70f;
            
            return 30f;
        }
        else if (action.targetType == TargetType.SingleAlly || action.targetType == TargetType.AllAllies)
        {
            var woundedAllies = enemyTeam
                .Where(e => !e.isDead && e != caster && (float)e.GetCurrentHP() / e.GetMaxHP() < ALLY_HEAL_THRESHOLD)
                .ToList();
            
            if (!woundedAllies.Any())
                return -100f;
            
            float worstAllyHpPercent = woundedAllies.Min(e => (float)e.GetCurrentHP() / e.GetMaxHP());
            
            if (worstAllyHpPercent < CRITICAL_HEAL_THRESHOLD)
                return 90f;
            
            if (worstAllyHpPercent < HEAL_THRESHOLD)
                return 60f;
            
            return 40f;
        }
        
        return 0f;
    }

    private static float EvaluateAttackAction(BattleAction action)
    {
        float score = 50f;
        
        foreach (ActionEffect effect in action.effects)
        {
            if (effect.statusEffect != StatusEffectType.None)
                score += 20f;
        }
        
        return score;
    }

    private static float EvaluateBuffAction(BattleAction action, BattleEntity caster, List<BattleEntity> enemyTeam)
    {
        float score = 40f;
        
        foreach (ActionEffect effect in action.effects)
        {
            if (effect.statusEffect == StatusEffectType.None) continue;
            
            if (action.targetType == TargetType.Self)
            {
                if (HasStatusEffect(caster, effect.statusEffect))
                    return -100f;
            }
            else if (action.targetType == TargetType.AllAllies)
            {
                int alliesWithBuff = enemyTeam.Count(e => !e.isDead && HasStatusEffect(e, effect.statusEffect));
                int totalAllies = enemyTeam.Count(e => !e.isDead);
                
                if (alliesWithBuff >= totalAllies * 0.7f)
                    return -100f;
                
                score += (totalAllies - alliesWithBuff) * 15f;
            }
        }
        
        return score;
    }

    private static float EvaluateDebuffAction(BattleAction action, BattleEntity playerTarget)
    {
        if (playerTarget.isDead)
            return -100f;

        foreach (ActionEffect effect in action.effects)
        {
            if (effect.statusEffect == StatusEffectType.None) continue;

            if (HasStatusEffect(playerTarget, effect.statusEffect))
            {
                return -100f; 
            }
        }
        
        return 45f;
    }

    private static bool HasStatusEffect(BattleEntity entity, StatusEffectType statusType)
    {
        return entity.GetActiveStatusEffects().Any(effect => effect.type == statusType);
    }

    /// <summary>
    /// Escolhe os alvos para a ação
    /// </summary>
    public static List<BattleEntity> ChooseBestTargets(BattleAction action, BattleEntity caster, BattleEntity player, List<BattleEntity> enemyTeam)
    {
        List<BattleEntity> targets = new List<BattleEntity>();
        
        switch (action.targetType)
        {
            case TargetType.Self:
                targets.Add(caster);
                break;
                
            case TargetType.SingleEnemy:
                if (!player.isDead)
                    targets.Add(player);
                break;
                
            case TargetType.SingleAlly:
                var targetAlly = enemyTeam
                    .Where(e => !e.isDead && e != caster)
                    .OrderBy(e => (float)e.GetCurrentHP() / e.GetMaxHP())
                    .FirstOrDefault();
                
                if (targetAlly != null)
                    targets.Add(targetAlly);
                else
                    targets.Add(caster); 
                break;
                
            case TargetType.AllEnemies:
                if (!player.isDead)
                    targets.Add(player);
                break;
                
            case TargetType.AllAllies:
                targets.AddRange(enemyTeam.Where(e => !e.isDead));
                break;
                
            case TargetType.Everyone:
                if (!player.isDead)
                    targets.Add(player);
                targets.AddRange(enemyTeam.Where(e => !e.isDead));
                break;
        }
        
        return targets;
    }
}