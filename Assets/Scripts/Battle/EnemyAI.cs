// Assets/Scripts/Battle/EnemyAI.cs
// Sistema de IA simplificado - Apenas 1 jogador como alvo

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EnemyAI
{
    // Thresholds para decisões
    private const float HEAL_THRESHOLD = 0.5f; // Cura quando HP < 50%
    private const float CRITICAL_HEAL_THRESHOLD = 0.25f; // Prioridade máxima quando HP < 25%
    private const float ALLY_HEAL_THRESHOLD = 0.6f; // Cura aliado quando HP < 60%
    
    /// <summary>
    /// Escolhe a melhor ação para o inimigo usando uma seleção ponderada aleatória
    /// </summary>
    public static BattleAction ChooseBestAction(BattleEntity caster, BattleEntity player, List<BattleEntity> enemyTeam)
    {
        // 1. Filtra ações que o inimigo pode usar (tem MP, etc.)
        List<BattleAction> availableActions = caster.characterData.battleActions
            .Where(a => caster.currentMp >= a.manaCost && (!a.isConsumable || a.CanUse()))
            .ToList();

        if (!availableActions.Any())
        {
            Debug.LogWarning($"{caster.characterData.characterName} não tem ações disponíveis!");
            return null;
        }

        // 2. Avalia cada ação e atribui uma pontuação
        Dictionary<BattleAction, float> actionScores = new Dictionary<BattleAction, float>();
        foreach (BattleAction action in availableActions)
        {
            float score = EvaluateAction(action, caster, player, enemyTeam);
            actionScores[action] = score;
        }

        // 3. NOVO: Lógica de Seleção Ponderada
        // Filtra apenas as ações com pontuação positiva (ações viáveis)
        var viableActions = actionScores.Where(kvp => kvp.Value > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // Se nenhuma ação for considerada "boa" (score > 0), recorre à melhor opção disponível, mesmo que seja ruim.
        if (!viableActions.Any())
        {
            Debug.Log($"IA ({caster.characterData.characterName}): Nenhuma ação positiva. Escolhendo a de maior score.");
            return actionScores.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;
        }
        
        // Calcula o peso total de todas as ações viáveis
        float totalWeight = viableActions.Sum(kvp => kvp.Value);
        
        // Gera um número aleatório entre 0 e o peso total
        float randomValue = Random.Range(0, totalWeight);
        
        // Itera sobre as ações viáveis até encontrar a escolhida
        foreach (var action in viableActions)
        {
            // Subtrai o peso da ação do valor aleatório.
            // A ação que fizer o valor ficar <= 0 é a escolhida.
            randomValue -= action.Value;
            if (randomValue <= 0)
            {
                Debug.Log($"IA: {caster.characterData.characterName} escolheu {action.Key.actionName} (Score: {action.Value:F2}) por seleção ponderada.");
                return action.Key;
            }
        }
        
        // Como segurança (fallback), caso algo dê errado, retorna a primeira ação viável.
        return viableActions.Keys.First();
    }

    /// <summary>
    /// Avalia o valor de uma ação (esta função permanece a mesma)
    /// </summary>
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
                score = 60f; // Score bom para ações mistas
                break;
        }
        
        // Adiciona variação aleatória para imprevisibilidade
        score += Random.Range(-5f, 5f);
        
        return score;
    }

    /// <summary>
    /// Avalia ações de cura
    /// </summary>
    private static float EvaluateHealAction(BattleAction action, BattleEntity caster, List<BattleEntity> enemyTeam)
    {
        if (action.targetType == TargetType.Self)
        {
            float hpPercent = (float)caster.GetCurrentHP() / caster.GetMaxHP();
            
            // NÃO cura se HP > 90%
            if (hpPercent > 0.9f)
                return -100f;
            
            // HP crítico? Prioridade MÁXIMA
            if (hpPercent < CRITICAL_HEAL_THRESHOLD)
                return 100f;
            
            // HP baixo? Prioridade alta
            if (hpPercent < HEAL_THRESHOLD)
                return 70f;
            
            // HP moderado? Prioridade baixa
            return 30f;
        }
        else if (action.targetType == TargetType.SingleAlly || action.targetType == TargetType.AllAllies)
        {
            // Verifica se há aliados precisando de cura
            var woundedAllies = enemyTeam
                .Where(e => !e.isDead && e != caster && (float)e.GetCurrentHP() / e.GetMaxHP() < ALLY_HEAL_THRESHOLD)
                .ToList();
            
            // Nenhum aliado precisa de cura
            if (!woundedAllies.Any())
                return -100f;
            
            // Quanto pior o HP do aliado, mais prioritário
            float worstAllyHpPercent = woundedAllies.Min(e => (float)e.GetCurrentHP() / e.GetMaxHP());
            
            if (worstAllyHpPercent < CRITICAL_HEAL_THRESHOLD)
                return 90f;
            
            if (worstAllyHpPercent < HEAL_THRESHOLD)
                return 60f;
            
            return 40f;
        }
        
        return 0f;
    }

    /// <summary>
    /// Avalia ações de ataque
    /// </summary>
    private static float EvaluateAttackAction(BattleAction action)
    {
        float score = 50f; // Score base
        
        // Bonus se tiver status effect
        foreach (ActionEffect effect in action.effects)
        {
            if (effect.statusEffect != StatusEffectType.None)
                score += 20f;
        }
        
        return score;
    }

    /// <summary>
    /// Avalia ações de buff
    /// </summary>
    private static float EvaluateBuffAction(BattleAction action, BattleEntity caster, List<BattleEntity> enemyTeam)
    {
        float score = 40f;
        
        foreach (ActionEffect effect in action.effects)
        {
            if (effect.statusEffect == StatusEffectType.None) continue;
            
            // Buff em si mesmo
            if (action.targetType == TargetType.Self)
            {
                // NÃO usa se já tiver o buff
                if (HasStatusEffect(caster, effect.statusEffect))
                    return -100f;
            }
            // Buff em área
            else if (action.targetType == TargetType.AllAllies)
            {
                int alliesWithBuff = enemyTeam.Count(e => !e.isDead && HasStatusEffect(e, effect.statusEffect));
                int totalAllies = enemyTeam.Count(e => !e.isDead);
                
                // Se maioria já tem o buff, não vale a pena
                if (alliesWithBuff >= totalAllies * 0.7f)
                    return -100f;
                
                // Bonus por cada aliado sem o buff
                score += (totalAllies - alliesWithBuff) * 15f;
            }
        }
        
        return score;
    }

    /// <summary>
    /// NOVO: Avalia ações de debuff
    /// </summary>
    private static float EvaluateDebuffAction(BattleAction action, BattleEntity playerTarget)
    {
        // Se o jogador está morto, não há alvo para debuff
        if (playerTarget.isDead)
            return -100f;

        foreach (ActionEffect effect in action.effects)
        {
            if (effect.statusEffect == StatusEffectType.None) continue;

            // Se o jogador JÁ TEM o debuff, reduz drasticamente a prioridade
            if (HasStatusEffect(playerTarget, effect.statusEffect))
            {
                Debug.Log($"IA: Jogador já tem {effect.statusEffect}, penalizando a ação {action.actionName}.");
                return -100f; // Pontuação muito baixa para evitar o uso
            }
        }
        
        // Se o jogador não tem o debuff, é uma boa opção
        return 45f;
    }

    /// <summary>
    /// Verifica se tem um status effect
    /// </summary>
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
                // Sempre ataca o único jogador
                if (!player.isDead)
                    targets.Add(player);
                break;
                
            case TargetType.SingleAlly:
                // Cura o aliado com MENOR HP
                var targetAlly = enemyTeam
                    .Where(e => !e.isDead && e != caster)
                    .OrderBy(e => (float)e.GetCurrentHP() / e.GetMaxHP())
                    .FirstOrDefault();
                
                if (targetAlly != null)
                    targets.Add(targetAlly);
                else
                    targets.Add(caster); // Se não tem aliado, usa em si mesmo
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