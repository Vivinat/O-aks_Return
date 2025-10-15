// Assets/Scripts/Difficulty_System/DeathOfferGenerator.cs

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gera ofertas de segunda chance baseadas no comportamento do jogador
/// </summary>
public static class DeathOfferGenerator
{
    /// <summary>
    /// Gera uma oferta aleatória, priorizando ofertas contextuais
    /// </summary>
    public static DeathNegotiationOffer GenerateOffer(BattleEntity player)
    {
        // 70% chance de oferta contextual, 30% chance de oferta fixa
        if (Random.value < 0.7f)
        {
            var contextualOffer = GenerateContextualOffer(player);
            if (contextualOffer != null)
            {
                return contextualOffer;
            }
        }
        
        // Fallback para oferta fixa
        return GenerateFixedOffer(player);
    }
    
    /// <summary>
    /// Gera oferta baseada em dados comportamentais
    /// </summary>
    private static DeathNegotiationOffer GenerateContextualOffer(BattleEntity player)
    {
        if (PlayerBehaviorAnalyzer.Instance == null) return null;
        
        var battleData = PlayerBehaviorAnalyzer.Instance.GetAllObservations();
        if (battleData.Count == 0) return null;
        
        List<DeathNegotiationOffer> contextualOffers = new List<DeathNegotiationOffer>();
        
        // Analisa skill mais usada
        var singleSkillCarry = battleData.FirstOrDefault(obs => 
            obs.triggerType == BehaviorTriggerType.SingleSkillCarry);
        
        if (singleSkillCarry != null)
        {
            string skillName = singleSkillCarry.GetData<string>("skillName", "");
            var targetSkill = GameManager.Instance?.PlayerBattleActions?.FirstOrDefault(a => 
                a != null && a.actionName == skillName);
            
            if (targetSkill != null)
            {
                contextualOffers.Add(new DeathNegotiationOffer(
                    "Sacrifício da Técnica Favorita",
                    $"Sua skill favorita '{skillName}' será enfraquecida significativamente.",
                    DeathPenaltyType.WeakenAction,
                    15
                )
                {
                    targetAction = targetSkill,
                    contextInfo = $"Você usa esta skill constantemente. Perdê-la será doloroso."
                });
            }
        }
        
        // Analisa problemas de HP
        var lowHPPattern = battleData.FirstOrDefault(obs => 
            obs.triggerType == BehaviorTriggerType.FrequentLowHP);
        
        if (lowHPPattern != null)
        {
            contextualOffers.Add(new DeathNegotiationOffer(
                "Fragilidade Permanente",
                "Você já vive no limite. Que tal tornar isso oficial? -30 HP máximo.",
                DeathPenaltyType.ReduceMaxHP,
                30
            )
            {
                contextInfo = "Você frequentemente termina batalhas com pouca vida."
            });
        }
        
        // Analisa problemas de Mana
        var manaIssues = battleData.FirstOrDefault(obs => 
            obs.triggerType == BehaviorTriggerType.LowManaStreak ||
            obs.triggerType == BehaviorTriggerType.AllSkillsUseMana);
        
        if (manaIssues != null)
        {
            contextualOffers.Add(new DeathNegotiationOffer(
                "Exaustão Arcana",
                "Suas reservas mágicas nunca foram suficientes. -25 MP máximo.",
                DeathPenaltyType.ReduceMaxMP,
                25
            )
            {
                contextInfo = "Você luta constantemente com falta de mana."
            });
            
            contextualOffers.Add(new DeathNegotiationOffer(
                "Custo do Desespero",
                "Todas suas habilidades custarão +3 MP adicional.",
                DeathPenaltyType.IncreaseActionCosts,
                3
            )
            {
                contextInfo = "Já que você sempre fica sem mana, isso será... interessante."
            });
        }
        
        // Analisa dependência de consumíveis
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
                
                contextualOffers.Add(new DeathNegotiationOffer(
                    "Escassez Forçada",
                    $"Seu item '{targetItem.actionName}' perderá 2 usos.",
                    DeathPenaltyType.RemoveItemUses,
                    2
                )
                {
                    targetAction = targetItem,
                    contextInfo = "Você depende muito de consumíveis. Hora de aprender a viver sem."
                });
            }
        }
        
        // Analisa velocidade
        var speedIssues = battleData.FirstOrDefault(obs => 
            obs.triggerType == BehaviorTriggerType.AlwaysOutsped);
        
        if (speedIssues != null)
        {
            contextualOffers.Add(new DeathNegotiationOffer(
                "Mais Lento Ainda",
                "Você já é lento. Que tal piorar? -2 Velocidade.",
                DeathPenaltyType.ReduceSpeed,
                2
            )
            {
                contextInfo = "Inimigos sempre agem antes de você. Isso vai piorar."
            });
        }
        
        // Analisa riqueza
        int currentCoins = GameManager.Instance?.CurrencySystem?.CurrentCoins ?? 0;
        if (currentCoins >= 50)
        {
            int coinLoss = Mathf.RoundToInt(currentCoins * 0.5f);
            contextualOffers.Add(new DeathNegotiationOffer(
                "Tributo em Ouro",
                $"Metade de suas riquezas ({coinLoss} moedas) será perdida.",
                DeathPenaltyType.LoseCoins,
                coinLoss
            )
            {
                contextInfo = "A morte tem seu preço. Literalmente."
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
    /// Gera ofertas fixas como fallback
    /// </summary>
    private static DeathNegotiationOffer GenerateFixedOffer(BattleEntity player)
    {
        List<DeathNegotiationOffer> fixedOffers = new List<DeathNegotiationOffer>();
        
        // Ofertas de redução de stats
        fixedOffers.Add(new DeathNegotiationOffer(
            "Corpo Frágil",
            "Seu corpo não será o mesmo. -25 HP máximo permanentemente.",
            DeathPenaltyType.ReduceMaxHP,
            25
        ));
        
        fixedOffers.Add(new DeathNegotiationOffer(
            "Mente Exausta",
            "Sua capacidade mágica diminuirá. -20 MP máximo permanentemente.",
            DeathPenaltyType.ReduceMaxMP,
            20
        ));
        
        fixedOffers.Add(new DeathNegotiationOffer(
            "Armadura Danificada",
            "Sua defesa nunca mais será a mesma. -8 Defesa.",
            DeathPenaltyType.ReduceDefense,
            8
        ));
        
        fixedOffers.Add(new DeathNegotiationOffer(
            "Reflexos Comprometidos",
            "Você se moverá mais devagar. -2 Velocidade.",
            DeathPenaltyType.ReduceSpeed,
            2
        ));
        
        // Ofertas de ações
        var playerActions = GameManager.Instance?.PlayerBattleActions?
            .Where(a => a != null && !a.isConsumable)
            .ToList();
        
        if (playerActions != null && playerActions.Count > 1)
        {
            var randomAction = playerActions[Random.Range(0, playerActions.Count)];
            
            fixedOffers.Add(new DeathNegotiationOffer(
                "Técnica Esquecida",
                $"Você perderá a habilidade '{randomAction.actionName}' permanentemente.",
                DeathPenaltyType.RemoveAction,
                0
            )
            {
                targetAction = randomAction
            });
            
            fixedOffers.Add(new DeathNegotiationOffer(
                "Poder Diminuído",
                $"A habilidade '{randomAction.actionName}' será enfraquecida (-10 poder).",
                DeathPenaltyType.WeakenAction,
                10
            )
            {
                targetAction = randomAction
            });
        }
        
        // Ofertas de tempo
        fixedOffers.Add(new DeathNegotiationOffer(
            "Pressão Temporal",
            "Você terá METADE do tempo para tomar decisões.",
            DeathPenaltyType.HalveDecisionTime,
            0
        )
        {
            contextInfo = "A pressa é inimiga da perfeição."
        });
        
        // Ofertas de moedas
        int currentCoins = GameManager.Instance?.CurrencySystem?.CurrentCoins ?? 0;
        if (currentCoins > 0)
        {
            int coinLoss = Mathf.Min(currentCoins, Random.Range(30, 50));
            fixedOffers.Add(new DeathNegotiationOffer(
                "Tributo Monetário",
                $"Pague {coinLoss} moedas pela sua vida.",
                DeathPenaltyType.LoseCoins,
                coinLoss
            ));
        }
        
        // Ofertas de debuffs
        fixedOffers.Add(new DeathNegotiationOffer(
            "Maldição do Fraco",
            "Um debuff permanente reduzirá seu ataque em 10 pontos.",
            DeathPenaltyType.PermanentDebuff,
            10
        )
        {
            contextInfo = "Este debuff durará até o fim da batalha."
        });
        
        fixedOffers.Add(new DeathNegotiationOffer(
            "Fardo Mágico",
            "Todas suas habilidades custarão +2 MP.",
            DeathPenaltyType.IncreaseActionCosts,
            2
        ));
        
        // Retorna oferta aleatória
        return fixedOffers[Random.Range(0, fixedOffers.Count)];
    }
}