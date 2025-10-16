// Assets/Scripts/Negotiations/DeathOfferGenerator.cs (VERSÃO CORRIGIDA - SÓ TRIGGERS EXISTENTES)

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gera ofertas de segunda chance baseadas no comportamento do jogador
/// CORRIGIDO: Usa apenas triggers que existem no PlayerBehaviorData.cs
/// </summary>
public static class DeathOfferGenerator
{
    private const float CONTEXTUAL_OFFER_CHANCE = 0.75f;
    private const int MIN_COINS_FOR_COIN_PENALTY = 30;
    
    /// <summary>
    /// Gera uma oferta, priorizando ofertas contextuais quando possível
    /// GARANTIDO: Sempre retorna uma oferta válida
    /// </summary>
    public static DeathNegotiationOffer GenerateOffer(BattleEntity player)
    {
        // 75% chance de oferta contextual, 25% de oferta fixa
        if (Random.value < CONTEXTUAL_OFFER_CHANCE)
        {
            var contextualOffer = GenerateContextualOffer(player);
            if (contextualOffer != null)
            {
                Debug.Log($"[DeathOffer] Oferta contextual gerada: {contextualOffer.title}");
                return contextualOffer;
            }
        }
        
        // GARANTIA: Fallback SEMPRE gera uma oferta
        var fixedOffer = GenerateFixedOffer(player);
        Debug.Log($"[DeathOffer] Oferta fixa gerada: {fixedOffer.title}");
        return fixedOffer;
    }
    
    /// <summary>
    /// Gera ofertas baseadas em padrões comportamentais EXISTENTES
    /// </summary>
    private static DeathNegotiationOffer GenerateContextualOffer(BattleEntity player)
    {
        if (PlayerBehaviorAnalyzer.Instance == null)
        {
            Debug.LogWarning("[DeathOffer] PlayerBehaviorAnalyzer não disponível");
            return null;
        }
        
        var battleData = PlayerBehaviorAnalyzer.Instance.GetAllObservations();
        if (battleData.Count == 0)
        {
            Debug.Log("[DeathOffer] Sem dados comportamentais suficientes");
            return null;
        }
        
        List<DeathNegotiationOffer> contextualOffers = new List<DeathNegotiationOffer>();
        
        // ============================================
        // APENAS TRIGGERS QUE EXISTEM NO ENUM
        // ============================================
        
        // === TRIGGER: SingleSkillCarry ===
        var singleSkillCarry = battleData.FirstOrDefault(obs => 
            obs.triggerType == BehaviorTriggerType.SingleSkillCarry);
        
        if (singleSkillCarry != null)
        {
            string skillName = singleSkillCarry.GetData<string>("skillName", "");
            float damagePercent = singleSkillCarry.GetData<float>("damagePercentage", 0f) * 100f;
            
            var targetSkill = GameManager.Instance?.PlayerBattleActions?.FirstOrDefault(a => 
                a != null && a.actionName == skillName);
            
            if (targetSkill != null)
            {
                // Opção 1: Enfraquecer a skill dominante
                int weakenAmount = Mathf.RoundToInt(15 + (damagePercent * 0.1f));
                contextualOffers.Add(new DeathNegotiationOffer(
                    "Sacrifício da Técnica Dominante",
                    $"Sua dependência de '{skillName}' é patética. Ela perderá {weakenAmount} de poder.",
                    DeathPenaltyType.WeakenAction,
                    weakenAmount
                )
                {
                    targetAction = targetSkill,
                    contextInfo = $"Esta skill causou {damagePercent:F0}% do seu dano total. Hora de diversificar."
                });
                
                // Opção 2: Aumentar custo de mana
                contextualOffers.Add(new DeathNegotiationOffer(
                    "Custo da Repetição",
                    $"Sua skill '{skillName}' custará +5 MP. Aprenda a variar suas táticas.",
                    DeathPenaltyType.IncreaseSpecificActionCost,
                    5
                )
                {
                    targetAction = targetSkill,
                    contextInfo = "Usar a mesma técnica repetidamente tem consequências."
                });
            }
        }
        
        // === TRIGGER: FrequentLowHP ===
        var lowHPPattern = battleData.FirstOrDefault(obs => 
            obs.triggerType == BehaviorTriggerType.FrequentLowHP);
        
        if (lowHPPattern != null)
        {
            float avgHP = lowHPPattern.GetData<float>("averageEndingHP", 0.3f) * 100f;
            int hpReduction = Mathf.RoundToInt(25 + (1f - avgHP/100f) * 15); // 25-40 HP
            
            contextualOffers.Add(new DeathNegotiationOffer(
                "Fragilidade Aceita",
                $"Você vive no limite da morte. Que tal tornar isso permanente? -{hpReduction} HP máximo.",
                DeathPenaltyType.ReduceMaxHP,
                hpReduction
            )
            {
                contextInfo = $"Você termina batalhas com média de {avgHP:F0}% HP. Pura sorte."
            });
            
            // Opção alternativa: Defesa
            int defenseReduction = Mathf.Clamp(8 + Mathf.RoundToInt((1f - avgHP/100f) * 7), 8, 15);
            contextualOffers.Add(new DeathNegotiationOffer(
                "Armadura Fraturada",
                $"Já que toma tanto dano, sua defesa será reduzida em {defenseReduction} pontos.",
                DeathPenaltyType.ReduceDefense,
                defenseReduction
            )
            {
                contextInfo = "Defesa reduzida significa AINDA MAIS dano recebido."
            });
        }
        
        // === TRIGGER: LowManaStreak ou AllSkillsUseMana ===
        var manaIssues = battleData.FirstOrDefault(obs => 
            obs.triggerType == BehaviorTriggerType.LowManaStreak ||
            obs.triggerType == BehaviorTriggerType.AllSkillsUseMana ||
            obs.triggerType == BehaviorTriggerType.ZeroManaStreak);
        
        if (manaIssues != null)
        {
            contextualOffers.Add(new DeathNegotiationOffer(
                "Reservas Esgotadas",
                "Suas reservas mágicas sempre foram insuficientes. -30 MP máximo.",
                DeathPenaltyType.ReduceMaxMP,
                30
            )
            {
                contextInfo = "Você luta constantemente com falta de mana. Isso vai piorar."
            });
            
            contextualOffers.Add(new DeathNegotiationOffer(
                "Sobrecarga Arcana",
                "Cada habilidade custará +4 MP adicional. Gerencie melhor.",
                DeathPenaltyType.IncreaseActionCosts,
                4
            )
            {
                contextInfo = "Talvez assim você aprenda a economizar mana."
            });
        }
        
        // === TRIGGER: ConsumableDependency ===
        var consumableDep = battleData.FirstOrDefault(obs => 
            obs.triggerType == BehaviorTriggerType.ConsumableDependency);
        
        if (consumableDep != null)
        {
            var consumables = GameManager.Instance?.PlayerBattleActions?
                .Where(a => a != null && a.isConsumable && a.currentUses > 0)
                .ToList();
            
            if (consumables != null && consumables.Count > 0)
            {
                var targetItem = consumables[Random.Range(0, consumables.Count)];
                int usesToRemove = Mathf.Min(targetItem.currentUses, Random.Range(2, 4));
                
                contextualOffers.Add(new DeathNegotiationOffer(
                    "Escassez Forçada",
                    $"Seu item '{targetItem.actionName}' perderá {usesToRemove} usos imediatamente.",
                    DeathPenaltyType.RemoveItemUses,
                    usesToRemove
                )
                {
                    targetAction = targetItem,
                    contextInfo = "Dependência de itens é fraqueza. Hora de aprender."
                });
                
                if (consumables.Count > 1)
                {
                    contextualOffers.Add(new DeathNegotiationOffer(
                        "Confisco Total",
                        $"O item '{targetItem.actionName}' será confiscado permanentemente.",
                        DeathPenaltyType.RemoveAction,
                        0
                    )
                    {
                        targetAction = targetItem,
                        contextInfo = "Considere isto uma lição sobre independência."
                    });
                }
            }
        }
        
        // === TRIGGER: AlwaysOutsped ===
        var speedIssues = battleData.FirstOrDefault(obs => 
            obs.triggerType == BehaviorTriggerType.AlwaysOutsped);
        
        if (speedIssues != null)
        {
            contextualOffers.Add(new DeathNegotiationOffer(
                "Mais Lento Ainda",
                "Você já é patéticamente lento. Vamos tornar isso oficial. -3 Velocidade.",
                DeathPenaltyType.ReduceSpeed,
                3
            )
            {
                contextInfo = "Inimigos agem primeiro. Isso só vai piorar."
            });
        }
        
        // === TRIGGER: Moedas (via sistema de moedas direto) ===
        int currentCoins = GameManager.Instance?.CurrencySystem?.CurrentCoins ?? 0;
        if (currentCoins >= MIN_COINS_FOR_COIN_PENALTY)
        {
            float lossPercentage = Random.Range(0.4f, 0.6f);
            int coinLoss = Mathf.RoundToInt(currentCoins * lossPercentage);
            
            contextualOffers.Add(new DeathNegotiationOffer(
                "Tributo em Ouro",
                $"{lossPercentage:P0} de sua riqueza ({coinLoss} moedas) será perdida.",
                DeathPenaltyType.LoseCoins,
                coinLoss
            )
            {
                contextInfo = "A morte cobra seu preço. Literalmente."
            });
        }
        
        // === TRIGGER: NoDefensiveSkills ===
        var noDefensive = battleData.FirstOrDefault(obs => 
            obs.triggerType == BehaviorTriggerType.NoDefensiveSkills);
        
        if (noDefensive != null)
        {
            contextualOffers.Add(new DeathNegotiationOffer(
                "Enfraquecimento Global",
                "Você só ataca, nunca defende. Todas skills ofensivas perderão 8 de poder.",
                DeathPenaltyType.WeakenAllOffensiveActions,
                8
            )
            {
                contextInfo = "Força bruta tem limites. Você os encontrou."
            });
            
            contextualOffers.Add(new DeathNegotiationOffer(
                "Fadiga de Combate",
                "Sem habilidades defensivas, você se cansa mais. +3 MP em todas skills.",
                DeathPenaltyType.IncreaseActionCosts,
                3
            )
            {
                contextInfo = "Aprenda a se proteger ou pague o preço."
            });
        }
        
        // === TRIGGER: LowHealthNoCure ===
        var lowHealthNoCure = battleData.FirstOrDefault(obs => 
            obs.triggerType == BehaviorTriggerType.LowHealthNoCure);
        
        if (lowHealthNoCure != null)
        {
            contextualOffers.Add(new DeathNegotiationOffer(
                "Punição da Teimosia",
                "Você termina ferido e não se cura? Perca 35 HP máximo.",
                DeathPenaltyType.ReduceMaxHP,
                35
            )
            {
                contextInfo = "Orgulho vem antes da queda. Literalmente."
            });
        }
        
        // === TRIGGER: WeakSkillIgnored ===
        var weakSkill = battleData.FirstOrDefault(obs => 
            obs.triggerType == BehaviorTriggerType.WeakSkillIgnored);
        
        if (weakSkill != null)
        {
            string ignoredSkill = weakSkill.GetData<string>("skillName", "");
            int battlesIgnored = weakSkill.GetData<int>("battlesIgnored", 5);
            
            var targetSkill = GameManager.Instance?.PlayerBattleActions?.FirstOrDefault(a => 
                a != null && a.actionName == ignoredSkill);
            
            if (targetSkill != null)
            {
                contextualOffers.Add(new DeathNegotiationOffer(
                    "Técnica Esquecida",
                    $"Já que nunca usa '{ignoredSkill}', que tal perdê-la de vez?",
                    DeathPenaltyType.RemoveAction,
                    0
                )
                {
                    targetAction = targetSkill,
                    contextInfo = $"Não usada em {battlesIgnored} batalhas. Não fará falta."
                });
            }
        }
        
        // === TRIGGER: ItemExhausted ===
        var itemExhausted = battleData.FirstOrDefault(obs => 
            obs.triggerType == BehaviorTriggerType.ItemExhausted);
        
        if (itemExhausted != null)
        {
            // Oferece reduzir custo de skills para compensar falta de itens
            contextualOffers.Add(new DeathNegotiationOffer(
                "Dependência Punida",
                "Seus itens acabaram? Todas suas skills custarão +3 MP.",
                DeathPenaltyType.IncreaseActionCosts,
                3
            )
            {
                contextInfo = "Planeje melhor seus recursos."
            });
        }
        
        // Retorna oferta aleatória da lista contextual
        if (contextualOffers.Count > 0)
        {
            return contextualOffers[Random.Range(0, contextualOffers.Count)];
        }
        
        return null;
    }
    
    /// <summary>
    /// GARANTIA: Sempre gera pelo menos uma oferta válida
    /// </summary>
    private static DeathNegotiationOffer GenerateFixedOffer(BattleEntity player)
    {
        List<DeathNegotiationOffer> fixedOffers = new List<DeathNegotiationOffer>();
        
        // === CATEGORIA 1: Redução de Stats (SEMPRE DISPONÍVEL) ===
        fixedOffers.Add(new DeathNegotiationOffer(
            "Vitalidade Drenada",
            "Seu corpo não será o mesmo. -30 HP máximo permanentemente.",
            DeathPenaltyType.ReduceMaxHP,
            30
        )
        {
            contextInfo = "HP perdido nunca retorna."
        });
        
        fixedOffers.Add(new DeathNegotiationOffer(
            "Essência Mágica Esvaída",
            "Sua capacidade arcana diminuirá. -25 MP máximo permanentemente.",
            DeathPenaltyType.ReduceMaxMP,
            25
        )
        {
            contextInfo = "Menos MP significa menos opções em batalha."
        });
        
        fixedOffers.Add(new DeathNegotiationOffer(
            "Armadura Despedaçada",
            "Sua proteção será permanentemente comprometida. -10 Defesa.",
            DeathPenaltyType.ReduceDefense,
            10
        )
        {
            contextInfo = "Defesa protege contra TODOS os ataques. Esta perda será sentida."
        });
        
        fixedOffers.Add(new DeathNegotiationOffer(
            "Reflexos Danificados",
            "Seus movimentos se tornarão mais pesados. -3 Velocidade.",
            DeathPenaltyType.ReduceSpeed,
            3
        )
        {
            contextInfo = "Agir por último pode ser fatal."
        });
        
        // === CATEGORIA 2: Manipulação de Ações (SE DISPONÍVEL) ===
        var playerActions = GameManager.Instance?.PlayerBattleActions?
            .Where(a => a != null && !a.isConsumable)
            .ToList();
        
        if (playerActions != null && playerActions.Count > 1)
        {
            var randomAction = playerActions[Random.Range(0, playerActions.Count)];
            
            fixedOffers.Add(new DeathNegotiationOffer(
                "Técnica Esquecida",
                $"Você esquecerá completamente a habilidade '{randomAction.actionName}'.",
                DeathPenaltyType.RemoveAction,
                0
            )
            {
                targetAction = randomAction,
                contextInfo = "Uma vez perdida, não pode ser recuperada nesta batalha."
            });
            
            fixedOffers.Add(new DeathNegotiationOffer(
                "Domínio Enfraquecido",
                $"A habilidade '{randomAction.actionName}' perderá 12 pontos de poder.",
                DeathPenaltyType.WeakenAction,
                12
            )
            {
                targetAction = randomAction,
                contextInfo = "Skills enfraquecidas são menos efetivas em combate."
            });
            
            fixedOffers.Add(new DeathNegotiationOffer(
                "Sobrecarga Específica",
                $"'{randomAction.actionName}' custará +6 MP adicional por uso.",
                DeathPenaltyType.IncreaseSpecificActionCost,
                6
            )
            {
                targetAction = randomAction,
                contextInfo = "Prepare-se para gerenciar mana com mais cuidado."
            });
        }
        
        // === CATEGORIA 3: Penalidades Temporais (SEMPRE DISPONÍVEL) ===
        fixedOffers.Add(new DeathNegotiationOffer(
            "Pressão Temporal Extrema",
            "Você terá METADE do tempo para tomar decisões em turnos.",
            DeathPenaltyType.HalveDecisionTime,
            0
        )
        {
            contextInfo = "A pressa causa erros. Prepare-se para cometer muitos."
        });
        
        // === CATEGORIA 4: Custos Globais (SEMPRE DISPONÍVEL) ===
        fixedOffers.Add(new DeathNegotiationOffer(
            "Exaustão Crescente",
            "Todas as suas habilidades custarão +3 MP permanentemente.",
            DeathPenaltyType.IncreaseActionCosts,
            3
        )
        {
            contextInfo = "Isto afeta TODAS as suas skills, não apenas uma."
        });
        
        fixedOffers.Add(new DeathNegotiationOffer(
            "Sobrecarga Mágica Severa",
            "Todas as suas habilidades custarão +5 MP. Gerencie bem.",
            DeathPenaltyType.IncreaseActionCosts,
            5
        )
        {
            contextInfo = "Um preço alto, mas você ainda terá suas habilidades."
        });
        
        // === CATEGORIA 5: Tributos Monetários (SE TIVER MOEDAS) ===
        int currentCoins = GameManager.Instance?.CurrencySystem?.CurrentCoins ?? 0;
        if (currentCoins > 0)
        {
            int smallLoss = Mathf.Min(currentCoins, Random.Range(20, 35));
            int mediumLoss = Mathf.Min(currentCoins, Random.Range(40, 60));
            int largeLoss = Mathf.Min(currentCoins, Mathf.RoundToInt(currentCoins * 0.5f));
            
            fixedOffers.Add(new DeathNegotiationOffer(
                "Tributo Leve",
                $"Pague {smallLoss} moedas pela sua vida. Um preço justo.",
                DeathPenaltyType.LoseCoins,
                smallLoss
            )
            {
                contextInfo = "Dinheiro pode ser recuperado. Sua vida não."
            });
            
            if (currentCoins >= 40)
            {
                fixedOffers.Add(new DeathNegotiationOffer(
                    "Tributo Moderado",
                    $"Entregue {mediumLoss} moedas. Considere um investimento.",
                    DeathPenaltyType.LoseCoins,
                    mediumLoss
                )
                {
                    contextInfo = "Dói, mas é melhor que morrer."
                });
            }
            
            if (currentCoins >= 60)
            {
                fixedOffers.Add(new DeathNegotiationOffer(
                    "Tributo Pesado",
                    $"Metade de sua fortuna ({largeLoss} moedas) será minha.",
                    DeathPenaltyType.LoseCoins,
                    largeLoss
                )
                {
                    contextInfo = "Um preço alto, mas você mantém o resto."
                });
            }
        }
        
        // === CATEGORIA 6: Debuffs Permanentes (SEMPRE DISPONÍVEL) ===
        fixedOffers.Add(new DeathNegotiationOffer(
            "Marca da Fraqueza",
            "Uma maldição reduzirá seu ataque em 12 pontos até o fim da batalha.",
            DeathPenaltyType.PermanentDebuff,
            12
        )
        {
            contextInfo = "Este debuff não pode ser removido. Escolha com sabedoria."
        });
        
        fixedOffers.Add(new DeathNegotiationOffer(
            "Aura da Vulnerabilidade",
            "Você receberá 20% mais dano de todas as fontes até o fim da batalha.",
            DeathPenaltyType.PermanentVulnerability,
            20
        )
        {
            contextInfo = "Combinado com sua fragilidade atual... isso será brutal."
        });
        
        // === CATEGORIA 7: Penalidades de Consumíveis (SE TIVER) ===
        var consumables = GameManager.Instance?.PlayerBattleActions?
            .Where(a => a != null && a.isConsumable && a.currentUses > 0)
            .ToList();
        
        if (consumables != null && consumables.Count > 0)
        {
            var randomConsumable = consumables[Random.Range(0, consumables.Count)];
            int usesToRemove = Mathf.Min(randomConsumable.currentUses, Random.Range(2, 4));
            
            fixedOffers.Add(new DeathNegotiationOffer(
                "Confisco de Suprimentos",
                $"Seu item '{randomConsumable.actionName}' perderá {usesToRemove} usos.",
                DeathPenaltyType.RemoveItemUses,
                usesToRemove
            )
            {
                targetAction = randomConsumable,
                contextInfo = "Recursos são preciosos. Você acabou de perder alguns."
            });
        }
        
        // === CATEGORIA 8: Combinações (SEMPRE DISPONÍVEL) ===
        fixedOffers.Add(new DeathNegotiationOffer(
            "Pacto do Desesperado",
            "Perca 40 HP máximo, mas mantenha tudo o mais. Simples assim.",
            DeathPenaltyType.ReduceMaxHP,
            40
        )
        {
            contextInfo = "A escolha mais brutal, mas você não perde habilidades."
        });
        
        if (playerActions != null && playerActions.Count > 2)
        {
            fixedOffers.Add(new DeathNegotiationOffer(
                "Enfraquecimento Total",
                "Todas as suas skills ofensivas perderão 10 de poder.",
                DeathPenaltyType.WeakenAllOffensiveActions,
                10
            )
            {
                contextInfo = "Amplo impacto. Todas as suas ofensivas serão afetadas."
            });
        }
        
        // GARANTIA FINAL: Se por algum motivo a lista estiver vazia
        if (fixedOffers.Count == 0)
        {
            Debug.LogWarning("[DeathOffer] FALLBACK DE EMERGÊNCIA ativado!");
            fixedOffers.Add(new DeathNegotiationOffer(
                "Preço da Ressurreição",
                "Sua vida retorna, mas você perde 25 HP máximo.",
                DeathPenaltyType.ReduceMaxHP,
                25
            )
            {
                contextInfo = "Uma segunda chance não vem de graça."
            });
        }
        
        // Retorna oferta aleatória balanceada
        return fixedOffers[Random.Range(0, fixedOffers.Count)];
    }
}