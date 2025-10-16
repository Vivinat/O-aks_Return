// Assets/Scripts/Difficulty_System/ObservationInterpreter.cs (REBALANCED & EXPANDED)

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Interpreta observações comportamentais e gera ofertas de negociação contextuais
/// MELHORADO: Agora retorna MÚLTIPLAS ofertas e sorteia aleatoriamente
/// </summary>
public static class ObservationInterpreter
{
    /// <summary>
    /// Interpreta uma observação e gera ofertas (vantagens e desvantagens)
    /// NOVO: Retorna múltiplas opções e sorteia 1 vantagem + 1 desvantagem
    /// </summary>
    public static List<NegotiationOffer> InterpretObservation(BehaviorObservation observation)
    {
        // Gera pool de ofertas baseado no tipo de observação
        List<NegotiationOffer> allAdvantages = new List<NegotiationOffer>();
        List<NegotiationOffer> allDisadvantages = new List<NegotiationOffer>();
        
        switch (observation.triggerType)
        {
            case BehaviorTriggerType.PlayerDeath:
                GeneratePlayerDeathOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.SingleSkillCarry:
                GenerateSingleSkillCarryOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.FrequentLowHP:
                GenerateFrequentLowHPOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.WeakSkillIgnored:
                GenerateWeakSkillIgnoredOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.AlwaysOutsped:
                GenerateAlwaysOutspedOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.AlwaysFirstTurn:
                GenerateAlwaysFirstTurnOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.StrugglesAgainstTanks:
                GenerateStrugglesAgainstTanksOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.StrugglesAgainstFast:
                GenerateStrugglesAgainstFastOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.StrugglesAgainstSwarms:
                GenerateStrugglesAgainsSwarmsOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.AlwaysDiesEarly:
                GenerateAlwaysDiesEarlyOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.AlwaysDiesLate:
                GenerateAlwaysDiesLateOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.DeathByChipDamage:
                GenerateDeathByChipDamageOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.OneHitKOVulnerable:
                GenerateOneHitKOVulnerableOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.LowHealthNoCure:
                GenerateLowHealthNoCureOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.AllSkillsUseMana:
            case BehaviorTriggerType.LowManaStreak:
            case BehaviorTriggerType.ZeroManaStreak:
                GenerateManaIssuesOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.ConsumableDependency:
            case BehaviorTriggerType.RanOutOfConsumables:
                GenerateConsumableIssuesOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.NoDamageReceived:
                GenerateNoDamageReceivedOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.ItemExhausted:
                GenerateItemExhaustedOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.NoDefensiveSkills:
                GenerateNoDefensiveSkillsOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.RepeatedBossDeath:
                GenerateRepeatedBossDeathOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.ShopIgnored:
                GenerateShopIgnoredOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.BattleEasyVictory:
                GenerateBattleEasyVictoryOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.ExpensiveSkillsOnly:
                GenerateExpensiveSkillsOnlyOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.NoAOEDamage:
                GenerateNoAOEDamageOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.BrokeAfterShopping:
                GenerateBrokeAfterShoppingOffers(observation, allAdvantages, allDisadvantages);
                break;
                
            case BehaviorTriggerType.LowCoinsUnvisitedShops:
                GenerateLowCoinsUnvisitedShopsOffers(observation, allAdvantages, allDisadvantages);
                break;
        }
        
        // NOVO: Sorteia 1 vantagem e 1 desvantagem aleatórias
        List<NegotiationOffer> selectedOffers = new List<NegotiationOffer>();
        
        if (allAdvantages.Count > 0)
        {
            int randomAdvIndex = Random.Range(0, allAdvantages.Count);
            selectedOffers.Add(allAdvantages[randomAdvIndex]);
        }
        
        if (allDisadvantages.Count > 0)
        {
            int randomDisadvIndex = Random.Range(0, allDisadvantages.Count);
            selectedOffers.Add(allDisadvantages[randomDisadvIndex]);
        }
        
        return selectedOffers;
    }
    
    #region Geradores de Ofertas - EXPANDIDOS
    
    /// <summary>
    /// Morte do jogador - 5 vantagens + 5 desvantagens
    /// </summary>
    private static void GeneratePlayerDeathOffers(BehaviorObservation obs, 
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        string killerEnemy = obs.GetData<string>("killerEnemy", "Inimigo");
        
        // VANTAGENS (5 opções)
        advantages.Add(new NegotiationOffer(
            "Vingança Tardia",
            "Enfraqueça seus adversários após sua queda anterior.",
            true,
            CardAttribute.PlayerDefense, 12,
            CardAttribute.EnemyMaxHP, -25,
            obs.triggerType,
            $"Morreu para: {killerEnemy}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Lição Aprendida",
            "Fortaleça-se com a experiência da derrota.",
            true,
            CardAttribute.PlayerMaxHP, 30,
            CardAttribute.EnemyDefense, 15,
            obs.triggerType,
            $"Morreu para: {killerEnemy}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Resistência Aprimorada",
            "Sua defesa aumenta após a experiência brutal.",
            true,
            CardAttribute.PlayerDefense, 15,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            $"Morreu para: {killerEnemy}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Retribuição Ofensiva",
            "Transforme sua dor em poder destrutivo.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 18,
            CardAttribute.EnemyMaxHP, 20,
            obs.triggerType,
            $"Morreu para: {killerEnemy}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Segunda Chance Fortalecida",
            "Reviva com preparação melhor para o desafio.",
            true,
            CardAttribute.PlayerMaxMP, 25,
            CardAttribute.EnemySpeed, 2,
            obs.triggerType,
            $"Morreu para: {killerEnemy}"
        ));
        
        // DESVANTAGENS (5 opções)
        disadvantages.Add(new NegotiationOffer(
            "Sede de Sangue",
            "Seus inimigos se fortalecem com a memória de sua derrota.",
            false,
            CardAttribute.PlayerDefense, -8,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            $"Morreu para: {killerEnemy}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Trauma Persistente",
            "O medo da morte enfraquece seu corpo.",
            false,
            CardAttribute.PlayerMaxHP, -20,
            CardAttribute.EnemyMaxHP, -15,
            obs.triggerType,
            $"Morreu para: {killerEnemy}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Desmoralização",
            "Sua força de vontade diminui.",
            false,
            CardAttribute.PlayerOffensiveActionPower, -12,
            CardAttribute.EnemyDefense, -10,
            obs.triggerType,
            $"Morreu para: {killerEnemy}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Exaustão Mental",
            "Seu MP sofre com o trauma.",
            false,
            CardAttribute.PlayerMaxMP, -15,
            CardAttribute.EnemyActionManaCost, -5,
            obs.triggerType,
            $"Morreu para: {killerEnemy}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Fragilidade Crescente",
            "Cada derrota te deixa mais vulnerável.",
            false,
            CardAttribute.PlayerDefense, -10,
            CardAttribute.EnemyOffensiveActionPower, -8,
            obs.triggerType,
            $"Morreu para: {killerEnemy}"
        ));
    }
    
    /// <summary>
    /// Dependência de skill única - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateSingleSkillCarryOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        string skillName = obs.GetData<string>("skillName", "Habilidade");
        
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Mestre de Uma Arte",
            $"Especialize-se ainda mais em '{skillName}'.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 20,
            CardAttribute.EnemyDefense, 15,
            obs.triggerType,
            $"Skill dominante: {skillName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Eficiência Aperfeiçoada",
            $"Reduza o custo de todas suas ações.",
            true,
            CardAttribute.PlayerActionManaCost, -5,
            CardAttribute.EnemyMaxMP, 12,
            obs.triggerType,
            $"Skill dominante: {skillName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Poder Concentrado",
            $"Foque toda sua energia em seus ataques principais.",
            true,
            CardAttribute.PlayerSingleTargetActionPower, 25,
            CardAttribute.EnemyMaxHP, 18,
            obs.triggerType,
            $"Skill dominante: {skillName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Domínio Técnico",
            $"Compense a falta de variedade com maestria.",
            true,
            CardAttribute.PlayerActionPower, 18,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            $"Skill dominante: {skillName}"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Dependência Custosa",
            $"Sua dependência de '{skillName}' cobra seu preço.",
            false,
            CardAttribute.PlayerActionManaCost, 6,
            CardAttribute.EnemySpeed, -2,
            obs.triggerType,
            $"Skill dominante: {skillName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Arsenal Limitado",
            $"Falta de versatilidade enfraquece seu potencial.",
            false,
            CardAttribute.PlayerActionPower, -12,
            CardAttribute.EnemyMaxHP, -15,
            obs.triggerType,
            $"Skill dominante: {skillName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Previsibilidade Fatal",
            $"Inimigos aprendem seus padrões.",
            false,
            CardAttribute.PlayerMaxHP, -18,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            $"Skill dominante: {skillName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Esgotamento Mágico",
            $"Uso excessivo da mesma habilidade drena você.",
            false,
            CardAttribute.PlayerMaxMP, -20,
            CardAttribute.EnemyMaxMP, -10,
            obs.triggerType,
            $"Skill dominante: {skillName}"
        ));
    }
    
    /// <summary>
    /// HP baixo frequente - 5 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateFrequentLowHPOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Fortificação Massiva",
            "Aumente drasticamente seu HP máximo.",
            true,
            CardAttribute.PlayerMaxHP, 40,
            CardAttribute.EnemyMaxHP, 25,
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Escudo Aprimorado",
            "Fortaleça suas defesas contra ataques.",
            true,
            CardAttribute.PlayerDefense, 18,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Cura Potencializada",
            "Suas habilidades defensivas ficam mais fortes.",
            true,
            CardAttribute.PlayerDefensiveActionPower, 25,
            CardAttribute.EnemyOffensiveActionPower, 12,
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Resistência Balanceada",
            "Equilíbrio entre HP e defesa.",
            true,
            CardAttribute.PlayerMaxHP, 25,
            CardAttribute.EnemyDefense, 12,
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Contra-ataque Feroz",
            "Quando ferido, você ataca com mais força.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 22,
            CardAttribute.EnemyMaxHP, 25,
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Fragilidade Persistente",
            "Seus problemas defensivos deixam marcas.",
            false,
            CardAttribute.PlayerDefense, -10,
            CardAttribute.EnemyDefense, -8,
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Vitalidade Comprometida",
            "Seu HP máximo é reduzido.",
            false,
            CardAttribute.PlayerMaxHP, -25,
            CardAttribute.EnemyMaxHP, -30,
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Vulnerabilidade Crescente",
            "Você se torna mais fácil de ferir.",
            false,
            CardAttribute.PlayerDefense, -12,
            CardAttribute.EnemyActionPower, 10,
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Cura Reduzida",
            "Suas habilidades de recuperação enfraquecem.",
            false,
            CardAttribute.PlayerDefensiveActionPower, -15,
            CardAttribute.EnemyDefense, -10,
            obs.triggerType,
            "HP crítico frequente"
        ));
    }
    
    /// <summary>
    /// Skill ignorada - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateWeakSkillIgnoredOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        string skillName = obs.GetData<string>("skillName", "Habilidade");
        
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Eficiência Mágica",
            "Aprenda a usar habilidades com menos mana.",
            true,
            CardAttribute.PlayerActionManaCost, -4,
            CardAttribute.EnemyActionPower, 10,
            obs.triggerType,
            $"Skill ignorada: {skillName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Versatilidade Forçada",
            "Melhore todas suas ações.",
            true,
            CardAttribute.PlayerActionPower, 15,
            CardAttribute.EnemyMaxHP, 18,
            obs.triggerType,
            $"Skill ignorada: {skillName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Reservas Expandidas",
            "Compense com mais MP.",
            true,
            CardAttribute.PlayerMaxMP, 30,
            CardAttribute.EnemyMaxMP, 15,
            obs.triggerType,
            $"Skill ignorada: {skillName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Especialização Alternativa",
            "Foque no que você realmente usa.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 20,
            CardAttribute.EnemyOffensiveActionPower, 12,
            obs.triggerType,
            $"Skill ignorada: {skillName}"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Arsenal Limitado",
            "Falta de versatilidade enfraquece você.",
            false,
            CardAttribute.PlayerActionPower, -12,
            CardAttribute.EnemyMaxHP, -15,
            obs.triggerType,
            $"Skill ignorada: {skillName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Desperdício Mágico",
            "Suas habilidades custam mais mana.",
            false,
            CardAttribute.PlayerActionManaCost, 5,
            CardAttribute.EnemyActionManaCost, 4,
            obs.triggerType,
            $"Skill ignorada: {skillName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Poder Reduzido",
            "Falta de treino enfraquece todas ações.",
            false,
            CardAttribute.PlayerActionPower, -10,
            CardAttribute.EnemyActionPower, -8,
            obs.triggerType,
            $"Skill ignorada: {skillName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Atrofia de Habilidade",
            "Seus ataques especializados perdem força.",
            false,
            CardAttribute.PlayerOffensiveActionPower, -15,
            CardAttribute.EnemyDefense, -10,
            obs.triggerType,
            $"Skill ignorada: {skillName}"
        ));
    }
    
    /// <summary>
    /// Sempre age por último - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateAlwaysOutspedOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Primeiro Golpe Poderoso",
            "Compense agir depois com ataques devastadores.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 20,
            CardAttribute.EnemyMaxHP, 18,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Resistência Superior",
            "Se não pode ser rápido, seja resistente.",
            true,
            CardAttribute.PlayerMaxHP, 35,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Defesa Impenetrável",
            "Fortifique-se enquanto espera sua vez.",
            true,
            CardAttribute.PlayerDefense, 18,
            CardAttribute.EnemyOffensiveActionPower, 12,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Contra-ataque Letal",
            "Transforme a desvantagem em oportunidade.",
            true,
            CardAttribute.PlayerSingleTargetActionPower, 25,
            CardAttribute.EnemyDefense, 15,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Corrida Perdida",
            "Seus adversários ficam ainda mais letais.",
            false,
            CardAttribute.PlayerMaxMP, -12,
            CardAttribute.EnemyActionPower, 18,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Vulnerabilidade Exposta",
            "Agir por último te deixa vulnerável.",
            false,
            CardAttribute.PlayerDefense, -10,
            CardAttribute.EnemyOffensiveActionPower, 12,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Lentidão Crítica",
            "Sua baixa velocidade afeta tudo.",
            false,
            CardAttribute.PlayerActionPower, -12,
            CardAttribute.EnemySpeed, 2,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Desvantagem Tática",
            "Perder a iniciativa tem consequências.",
            false,
            CardAttribute.PlayerMaxHP, -20,
            CardAttribute.EnemyMaxHP, 15,
            obs.triggerType,
            "Sempre age por último"
        ));
    }
    
    /// <summary>
    /// Sempre age primeiro - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateAlwaysFirstTurnOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Domínio Tático",
            "Mais recursos para sua vantagem inicial.",
            true,
            CardAttribute.PlayerMaxMP, 25,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Iniciativa Letal",
            "Transforme velocidade em poder destrutivo.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 22,
            CardAttribute.EnemyMaxHP, 20,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Abertura Perfeita",
            "Maximize o dano no primeiro turno.",
            true,
            CardAttribute.PlayerSingleTargetActionPower, 28,
            CardAttribute.EnemyDefense, 18,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Velocidade Esmagadora",
            "Aproveite sua vantagem de velocidade.",
            true,
            CardAttribute.PlayerActionPower, 20,
            CardAttribute.EnemySpeed, 2,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Força Bruta Inimiga",
            "Inimigos compensam com puro poder.",
            false,
            CardAttribute.PlayerDefense, -8,
            CardAttribute.EnemyActionPower, 22,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Fragilidade Veloz",
            "Velocidade às custas de resistência.",
            false,
            CardAttribute.PlayerMaxHP, -25,
            CardAttribute.EnemyMaxHP, 15,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Exaustão Rápida",
            "Agir primeiro drena sua energia.",
            false,
            CardAttribute.PlayerMaxMP, -18,
            CardAttribute.EnemyMaxMP, -10,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Contra-medidas Aprendidas",
            "Inimigos se preparam para sua velocidade.",
            false,
            CardAttribute.PlayerActionPower, -10,
            CardAttribute.EnemyDefense, 15,
            obs.triggerType,
            "Sempre age primeiro"
        ));
    }
    
    /// <summary>
    /// Dificuldade vs Tanks - 5 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateStrugglesAgainstTanksOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Quebra-Couraças",
            "Perfure as defesas mais robustas.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 25,
            CardAttribute.EnemyDefense, 18,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Perfuração Letal",
            "Seus ataques ignoram parte da defesa inimiga.",
            true,
            CardAttribute.PlayerSingleTargetActionPower, 30,
            CardAttribute.EnemyMaxHP, 20,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Atrito Eficiente",
            "Cause dano constante em alvos resistentes.",
            true,
            CardAttribute.PlayerActionPower, 22,
            CardAttribute.EnemyDefense, 15,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Destruidor de Fortalezas",
            "Especialize-se em quebrar defesas.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 28,
            CardAttribute.EnemyMaxHP, 25,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Fúria Persistente",
            "Batalhas longas favorecem você.",
            true,
            CardAttribute.PlayerMaxMP, 30,
            CardAttribute.EnemyDefense, 12,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Fortaleza Impenetrável",
            "Inimigos endurecem suas defesas.",
            false,
            CardAttribute.PlayerActionPower, -10,
            CardAttribute.EnemyMaxHP, 35,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Muralha Crescente",
            "Defesas inimigas aumentam drasticamente.",
            false,
            CardAttribute.PlayerOffensiveActionPower, -12,
            CardAttribute.EnemyDefense, 25,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Frustração Desgastante",
            "Sua incapacidade de penetrar defesas te esgota.",
            false,
            CardAttribute.PlayerMaxMP, -20,
            CardAttribute.EnemyMaxHP, 20,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Impotência Ofensiva",
            "Seus ataques enfraquecem contra alvos resistentes.",
            false,
            CardAttribute.PlayerSingleTargetActionPower, -18,
            CardAttribute.EnemyDefense, 10,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
    }
    
    /// <summary>
    /// Dificuldade vs Rápidos - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateStrugglesAgainstFastOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Antecipação",
            "Aprenda a se defender de adversários velozes.",
            true,
            CardAttribute.PlayerDefense, 15,
            CardAttribute.EnemyMaxHP, 18,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Contra-ataque Preciso",
            "Ataques mais fortes compensam falta de velocidade.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 25,
            CardAttribute.EnemySpeed, -2,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Muralha Impenetrável",
            "Se não pode alcançá-los, seja impossível de ferir.",
            true,
            CardAttribute.PlayerMaxHP, 35,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Reflexos Aprimorados",
            "Melhore sua capacidade defensiva.",
            true,
            CardAttribute.PlayerDefense, 18,
            CardAttribute.EnemyOffensiveActionPower, 12,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Velocidade Cegante",
            "Inimigos ficam impossíveis de acompanhar.",
            false,
            CardAttribute.PlayerDefense, -10,
            CardAttribute.EnemyActionPower, 18,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Ataques Implacáveis",
            "Inimigos rápidos te acertam com mais frequência.",
            false,
            CardAttribute.PlayerMaxHP, -25,
            CardAttribute.EnemyOffensiveActionPower, 15,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Lentidão Crítica",
            "Sua baixa velocidade te prejudica ainda mais.",
            false,
            CardAttribute.PlayerActionPower, -12,
            CardAttribute.EnemySpeed, 3,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Sobrecarga Defensiva",
            "Tentar se defender esgota você.",
            false,
            CardAttribute.PlayerMaxMP, -18,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
    }
    
    /// <summary>
    /// Dificuldade vs Múltiplos - 5 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateStrugglesAgainsSwarmsOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        int enemyCount = obs.GetData<int>("enemyCount", 3);
        
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Destruição em Massa",
            "Habilidades de área ganham poder devastador.",
            true,
            CardAttribute.PlayerAOEActionPower, 30,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Tempestade de Lâminas",
            "Todos seus ataques ficam mais fortes contra grupos.",
            true,
            CardAttribute.PlayerActionPower, 22,
            CardAttribute.EnemyMaxHP, 15,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Resistência de Multidão",
            "Fortaleça-se para enfrentar números superiores.",
            true,
            CardAttribute.PlayerMaxHP, 40,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Maestria em Grupo",
            "Especialize-se em combate contra múltiplos.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 25,
            CardAttribute.EnemyDefense, 15,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Defesa Circular",
            "Proteja-se de ataques de múltiplas direções.",
            true,
            CardAttribute.PlayerDefense, 20,
            CardAttribute.EnemyOffensiveActionPower, 12,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Horda Implacável",
            "Mais inimigos surgem, mais fortes.",
            false,
            CardAttribute.PlayerMaxHP, -18,
            CardAttribute.EnemyMaxHP, 30,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Sobrecarga Numérica",
            "Números esmagadores te enfraquecem.",
            false,
            CardAttribute.PlayerActionPower, -15,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Fadiga de Combate",
            "Lutar contra múltiplos te esgota.",
            false,
            CardAttribute.PlayerMaxMP, -20,
            CardAttribute.EnemyMaxMP, -10,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Área Insuficiente",
            "Suas habilidades de área enfraquecem.",
            false,
            CardAttribute.PlayerAOEActionPower, -20,
            CardAttribute.EnemyMaxHP, -15,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
    }
    
    /// <summary>
    /// Sempre morre cedo - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateAlwaysDiesEarlyOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Início Fortificado",
            "Comece com defesas massivas.",
            true,
            CardAttribute.PlayerMaxHP, 45,
            CardAttribute.EnemyActionPower, 18,
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Escudo Inicial",
            "Resistência aprimorada desde o começo.",
            true,
            CardAttribute.PlayerDefense, 22,
            CardAttribute.EnemyOffensiveActionPower, 15,
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Barreira Protetora",
            "Proteção extra nos primeiros turnos.",
            true,
            CardAttribute.PlayerDefense, 18,
            CardAttribute.EnemyActionPower, 20,
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Vitalidade Reforçada",
            "HP e defesa aumentados.",
            true,
            CardAttribute.PlayerMaxHP, 35,
            CardAttribute.EnemyMaxHP, 20,
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Assalto Inicial",
            "Inimigos investem tudo no início.",
            false,
            CardAttribute.PlayerDefense, -12,
            CardAttribute.EnemyActionPower, 30,
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Fragilidade Crítica",
            "Você começa mais vulnerável.",
            false,
            CardAttribute.PlayerMaxHP, -30,
            CardAttribute.EnemyDefense, -15,
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Abertura Fatal",
            "Primeiros turnos são ainda mais perigosos.",
            false,
            CardAttribute.PlayerDefense, -15,
            CardAttribute.EnemyOffensiveActionPower, 20,
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Início Desastroso",
            "Tudo começa mal para você.",
            false,
            CardAttribute.PlayerMaxHP, -25,
            CardAttribute.EnemyActionPower, 18,
            obs.triggerType,
            "Morte precoce frequente"
        ));
    }
    
    /// <summary>
    /// Sempre morre tarde - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateAlwaysDiesLateOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Resistência Prolongada",
            "Ganhe fôlego para batalhas longas.",
            true,
            CardAttribute.PlayerMaxMP, 30,
            CardAttribute.EnemyMaxHP, 25,
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Regeneração Persistente",
            "Suas habilidades defensivas melhoram.",
            true,
            CardAttribute.PlayerDefensiveActionPower, 25,
            CardAttribute.EnemyOffensiveActionPower, 15,
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Stamina Infinita",
            "Batalhas longas favorecem você.",
            true,
            CardAttribute.PlayerMaxMP, 35,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Fortificação Crescente",
            "Você fica mais forte com o tempo.",
            true,
            CardAttribute.PlayerDefense, 20,
            CardAttribute.EnemyDefense, 12,
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Guerra de Atrito",
            "Batalhas longas favorecem adversários.",
            false,
            CardAttribute.PlayerActionManaCost, 6,
            CardAttribute.EnemyDefense, 18,
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Exaustão Inevitável",
            "Você se cansa em batalhas prolongadas.",
            false,
            CardAttribute.PlayerMaxMP, -25,
            CardAttribute.EnemyMaxMP, 15,
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Desgaste Acumulado",
            "Dano constante te enfraquece.",
            false,
            CardAttribute.PlayerDefense, -12,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Esgotamento Total",
            "Longas batalhas drenam tudo de você.",
            false,
            CardAttribute.PlayerMaxHP, -20,
            CardAttribute.EnemyMaxHP, 10,
            obs.triggerType,
            "Morte tardia por atrito"
        ));
    }
    
    /// <summary>
    /// Morte por chip damage - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateDeathByChipDamageOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Pele de Aço",
            "Fortifique-se contra ataques menores.",
            true,
            CardAttribute.PlayerDefense, 20,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Regeneração Constante",
            "Curas mais eficazes.",
            true,
            CardAttribute.PlayerDefensiveActionPower, 28,
            CardAttribute.EnemyOffensiveActionPower, 12,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Armadura Reforçada",
            "Reduza todo dano recebido.",
            true,
            CardAttribute.PlayerDefense, 25,
            CardAttribute.EnemyDefense, 12,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Vitalidade Robusta",
            "Mais HP para aguentar o atrito.",
            true,
            CardAttribute.PlayerMaxHP, 40,
            CardAttribute.EnemyActionPower, 18,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Mil Cortes",
            "Inimigos atacam com mais força e frequência.",
            false,
            CardAttribute.PlayerDefense, -12,
            CardAttribute.EnemyActionPower, 18,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Sangramento Perpétuo",
            "Dano acumulado aumenta.",
            false,
            CardAttribute.PlayerMaxHP, -25,
            CardAttribute.EnemyOffensiveActionPower, 15,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Armadura Desgastada",
            "Sua defesa se deteriora.",
            false,
            CardAttribute.PlayerDefense, -15,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Feridas Abertas",
            "Cada hit dói mais.",
            false,
            CardAttribute.PlayerDefense, -10,
            CardAttribute.EnemyOffensiveActionPower, 18,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
    }
    
    /// <summary>
    /// Vulnerável a one-shots - 5 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateOneHitKOVulnerableOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        int biggestHit = obs.GetData<int>("biggestHit", 50);
        
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Fortaleza Viva",
            "Torne-se resistente a ataques devastadores.",
            true,
            CardAttribute.PlayerMaxHP, 50,
            CardAttribute.EnemyMaxHP, 35,
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Escudo Absoluto",
            "Defesa massiva contra golpes fortes.",
            true,
            CardAttribute.PlayerDefense, 25,
            CardAttribute.EnemyActionPower, 20,
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Barreira Suprema",
            "Proteção contra dano explosivo.",
            true,
            CardAttribute.PlayerDefense, 22,
            CardAttribute.EnemyOffensiveActionPower, 18,
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Corpo Adamantino",
            "HP e defesa aumentados drasticamente.",
            true,
            CardAttribute.PlayerMaxHP, 45,
            CardAttribute.EnemyDefense, 20,
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Proteção Divina",
            "Resistência aprimorada.",
            true,
            CardAttribute.PlayerDefense, 30,
            CardAttribute.EnemyMaxHP, 25,
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Golpe Executivo",
            "Adversários aperfeiçoam ataques letais.",
            false,
            CardAttribute.PlayerDefense, -15,
            CardAttribute.EnemyActionPower, 35,
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Fragilidade Extrema",
            "Você fica ainda mais vulnerável.",
            false,
            CardAttribute.PlayerMaxHP, -30,
            CardAttribute.EnemyOffensiveActionPower, 25,
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Armadura de Papel",
            "Defesa drasticamente reduzida.",
            false,
            CardAttribute.PlayerDefense, -20,
            CardAttribute.EnemyActionPower, 20,
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Alvo Fácil",
            "Inimigos miram em suas fraquezas.",
            false,
            CardAttribute.PlayerMaxHP, -25,
            CardAttribute.EnemyOffensiveActionPower, 20,
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
    }
    
    /// <summary>
    /// HP baixo sem cura - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateLowHealthNoCureOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Vitalidade Resiliente",
            "Compense falta de cura com mais resistência.",
            true,
            CardAttribute.PlayerMaxHP, 40,
            CardAttribute.EnemyDefense, 15,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Regeneração Natural",
            "Habilidades defensivas melhoradas.",
            true,
            CardAttribute.PlayerDefensiveActionPower, 30,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Defesa Compensatória",
            "Mais defesa para evitar dano.",
            true,
            CardAttribute.PlayerDefense, 20,
            CardAttribute.EnemyOffensiveActionPower, 12,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Fortaleza Adaptativa",
            "HP e defesa aumentados.",
            true,
            CardAttribute.PlayerMaxHP, 35,
            CardAttribute.EnemyMaxHP, 20,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Feridas Abertas",
            "Feridas o tornam mais vulnerável.",
            false,
            CardAttribute.PlayerDefense, -12,
            CardAttribute.EnemyMaxHP, -18,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Corpo Fragilizado",
            "Falta de cura te deixa fraco.",
            false,
            CardAttribute.PlayerMaxHP, -25,
            CardAttribute.EnemyDefense, -15,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Hemorragia Perpétua",
            "Feridas não cicatrizam.",
            false,
            CardAttribute.PlayerDefensiveActionPower, -20,
            CardAttribute.EnemyActionPower, -10,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Vulnerabilidade Crítica",
            "Sem cura, você fica exposto.",
            false,
            CardAttribute.PlayerDefense, -15,
            CardAttribute.EnemyOffensiveActionPower, 12,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
    }
    
    /// <summary>
    /// Problemas de mana - 5 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateManaIssuesOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Reservas Mágicas",
            "Amplie drasticamente suas reservas de mana.",
            true,
            CardAttribute.PlayerMaxMP, 35,
            CardAttribute.EnemyMaxMP, 20,
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Eficiência Arcana",
            "Reduza o custo de todas habilidades.",
            true,
            CardAttribute.PlayerActionManaCost, -6,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Maestria Energética",
            "Use mana com muito mais eficiência.",
            true,
            CardAttribute.PlayerActionManaCost, -5,
            CardAttribute.EnemyMaxMP, 15,
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Poço Infinito",
            "MP massivamente aumentado.",
            true,
            CardAttribute.PlayerMaxMP, 40,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Canalização Perfeita",
            "Custo e reservas melhorados.",
            true,
            CardAttribute.PlayerMaxMP, 30,
            CardAttribute.EnemyDefense, 12,
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Exaustão Mágica",
            "Habilidades drenam ainda mais energia.",
            false,
            CardAttribute.PlayerActionManaCost, 7,
            CardAttribute.EnemyActionManaCost, -4,
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Reservas Esgotadas",
            "Seu MP máximo diminui.",
            false,
            CardAttribute.PlayerMaxMP, -25,
            CardAttribute.EnemyMaxMP, -15,
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Dreno Arcano",
            "Magia custa mais e você tem menos.",
            false,
            CardAttribute.PlayerMaxMP, -20,
            CardAttribute.EnemyDefense, -12,
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Desperdício Mágico",
            "Suas habilidades ficam muito caras.",
            false,
            CardAttribute.PlayerActionManaCost, 8,
            CardAttribute.EnemyActionPower, -10,
            obs.triggerType,
            "Problemas de mana constantes"
        ));
    }
    
    /// <summary>
    /// Dependência de consumíveis - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateConsumableIssuesOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Autossuficiência",
            "Troque dependência de itens por poder próprio.",
            true,
            CardAttribute.PlayerMaxHP, 35,
            CardAttribute.EnemyMaxHP, 25,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Força Interior",
            "Desenvolva poder independente.",
            true,
            CardAttribute.PlayerDefensiveActionPower, 30,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Recursos Internos",
            "Mais MP para habilidades próprias.",
            true,
            CardAttribute.PlayerMaxMP, 35,
            CardAttribute.EnemyDefense, 15,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Independência Total",
            "Stats permanentes aumentados.",
            true,
            CardAttribute.PlayerActionPower, 20,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Escassez Material",
            "Recursos se tornam ainda mais raros.",
            false,
            CardAttribute.CoinsEarned, -15,
            CardAttribute.EnemyDefense, -12,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Dependência Crítica",
            "Estratégia baseada em itens te enfraquece.",
            false,
            CardAttribute.PlayerDefense, -15,
            CardAttribute.EnemyActionPower, -12,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Fragilidade Sem Itens",
            "Sem consumíveis, você é fraco.",
            false,
            CardAttribute.PlayerMaxHP, -25,
            CardAttribute.EnemyMaxHP, -20,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Vício Custoso",
            "Sua dependência cobra um preço alto.",
            false,
            CardAttribute.PlayerActionPower, -12,
            CardAttribute.EnemyDefense, -10,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
    }
    
    /// <summary>
    /// Vitória perfeita - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateNoDamageReceivedOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        string enemyName = obs.GetData<string>("randomEnemy", "Inimigo");
        
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Confiança Absoluta",
            "Sua força é inquestionável.",
            true,
            CardAttribute.PlayerActionPower, 30,
            CardAttribute.CoinsEarned, -18,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Domínio Total",
            "Você está além do desafio.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 35,
            CardAttribute.EnemyMaxHP, 30,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Superioridade Marcial",
            "Todos seus ataques ficam mais fortes.",
            true,
            CardAttribute.PlayerActionPower, 28,
            CardAttribute.EnemyActionPower, 20,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Maestria Completa",
            "Aumente seu poder ofensivo.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 30,
            CardAttribute.EnemyDefense, 20,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Lição Aprendida",
            "Inimigos estudam sua técnica.",
            false,
            CardAttribute.PlayerMaxHP, -25,
            CardAttribute.EnemyDefense, 25,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Arrogância Fatal",
            "Confiança excessiva te enfraquece.",
            false,
            CardAttribute.PlayerDefense, -15,
            CardAttribute.EnemyActionPower, 20,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Desafio Amplificado",
            "Universo aumenta a dificuldade.",
            false,
            CardAttribute.PlayerMaxHP, -20,
            CardAttribute.EnemyMaxHP, 40,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Contra-medidas Desenvolvidas",
            "Inimigos se preparam melhor.",
            false,
            CardAttribute.PlayerActionPower, -15,
            CardAttribute.EnemyDefense, 25,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
    }
    
    /// <summary>
    /// Item esgotado - 3 vantagens + 3 desvantagens
    /// </summary>
    private static void GenerateItemExhaustedOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        string itemName = obs.GetData<string>("exhaustedItem", "Item");
        
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Força Interior",
            $"Ao esgotar '{itemName}', desenvolve resistência própria.",
            true,
            CardAttribute.PlayerMaxHP, 35,
            CardAttribute.EnemyMaxHP, 28,
            obs.triggerType,
            $"Item esgotado: {itemName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Adaptação Forçada",
            "Aprenda a lutar sem depender de itens.",
            true,
            CardAttribute.PlayerActionPower, 22,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            $"Item esgotado: {itemName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Autossuficiência Desenvolvida",
            "Habilidades próprias ficam mais fortes.",
            true,
            CardAttribute.PlayerDefensiveActionPower, 28,
            CardAttribute.EnemyDefense, 15,
            obs.triggerType,
            $"Item esgotado: {itemName}"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Dependência Crítica",
            "Estratégia baseada em itens te enfraquece.",
            false,
            CardAttribute.PlayerDefense, -15,
            CardAttribute.EnemyActionPower, -12,
            obs.triggerType,
            $"Item esgotado: {itemName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Escassez Permanente",
            "Recursos ficam ainda mais raros.",
            false,
            CardAttribute.CoinsEarned, -18,
            CardAttribute.EnemyMaxHP, -15,
            obs.triggerType,
            $"Item esgotado: {itemName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Fragilidade Exposta",
            "Sem itens, suas fraquezas aparecem.",
            false,
            CardAttribute.PlayerMaxHP, -20,
            CardAttribute.EnemyDefense, -15,
            obs.triggerType,
            $"Item esgotado: {itemName}"
        ));
    }
    
    /// <summary>
    /// Sem skills defensivas - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateNoDefensiveSkillsOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "A Melhor Defesa",
            "Puro foco ofensivo devastador.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 35,
            CardAttribute.EnemyActionPower, 25,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Ataque Esmagador",
            "Todos ataques ganham poder massivo.",
            true,
            CardAttribute.PlayerActionPower, 30,
            CardAttribute.EnemyDefense, 20,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Destruição Pura",
            "Especialize-se em dano.",
            true,
            CardAttribute.PlayerSingleTargetActionPower, 35,
            CardAttribute.EnemyMaxHP, 25,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Força Bruta",
            "Compensação ofensiva total.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 32,
            CardAttribute.EnemyOffensiveActionPower, 20,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Vidro e Canhão",
            "Sem defesas, você é frágil.",
            false,
            CardAttribute.PlayerMaxHP, 45,
            CardAttribute.EnemyActionPower, 30,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Fragilidade Extrema",
            "Falta de defesa te enfraquece.",
            false,
            CardAttribute.PlayerDefense, -18,
            CardAttribute.EnemyMaxHP, -15,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Vulnerabilidade Total",
            "Você é um alvo fácil.",
            false,
            CardAttribute.PlayerMaxHP, -30,
            CardAttribute.EnemyActionPower, 20,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Defesa Inexistente",
            "Inimigos exploram sua fragilidade.",
            false,
            CardAttribute.PlayerDefense, -15,
            CardAttribute.EnemyOffensiveActionPower, 22,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
    }
    
    /// <summary>
    /// Morte repetida em boss - 5 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateRepeatedBossDeathOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        string bossName = obs.GetData<string>("bossName", "Boss");
        
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Vingança Direcionada",
            $"Forças cósmicas concedem poder contra {bossName}.",
            true,
            CardAttribute.PlayerActionPower, 40,
            CardAttribute.EnemyMaxHP, 35,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Lições do Fracasso",
            "Aprenda com cada derrota.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 38,
            CardAttribute.EnemyDefense, 25,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Resistência Forjada",
            "Fortifique-se através do sofrimento.",
            true,
            CardAttribute.PlayerMaxHP, 45,
            CardAttribute.EnemyActionPower, 30,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Determinação Inabalável",
            "Cada derrota te fortalece.",
            true,
            CardAttribute.PlayerDefense, 22,
            CardAttribute.EnemyOffensiveActionPower, 18,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Preparação Total",
            "Múltiplas tentativas te preparam perfeitamente.",
            true,
            CardAttribute.PlayerMaxMP, 35,
            CardAttribute.EnemyMaxMP, 20,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Sede por Sangue",
            $"{bossName} se fortalece com cada vitória.",
            false,
            CardAttribute.PlayerMaxMP, -20,
            CardAttribute.EnemyActionPower, 35,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Trauma Profundo",
            "Mortes repetidas te enfraquecem mentalmente.",
            false,
            CardAttribute.PlayerMaxHP, -30,
            CardAttribute.EnemyMaxHP, 20,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Desmoralização Total",
            "Derrotas consecutivas te quebram.",
            false,
            CardAttribute.PlayerActionPower, -18,
            CardAttribute.EnemyDefense, 15,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Medo Paralisante",
            "Pavor de {bossName} te enfraquece.",
            false,
            CardAttribute.PlayerDefense, -15,
            CardAttribute.EnemyOffensiveActionPower, 25,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
    }
    
    /// <summary>
    /// Loja ignorada - 3 vantagens + 3 desvantagens
    /// </summary>
    private static void GenerateShopIgnoredOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        int playerCoins = obs.GetData<int>("playerCoins", 0);
        
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Guardião de Recursos",
            "Disciplina financeira é recompensada.",
            true,
            CardAttribute.CoinsEarned, 25,
            CardAttribute.ShopPrices, 12,
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Autossuficiência Premiada",
            "Não precisar de itens te fortalece.",
            true,
            CardAttribute.PlayerActionPower, 22,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Economia Sábia",
            "Guardar recursos tem benefícios.",
            true,
            CardAttribute.CoinsEarned, 28,
            CardAttribute.EnemyDefense, 12,
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Inflação Galopante",
            "Comerciantes aumentam os preços.",
            false,
            CardAttribute.ShopPrices, 30,
            CardAttribute.EnemyMaxHP, -25,
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Escassez Forçada",
            "Recursos ficam mais caros.",
            false,
            CardAttribute.ShopPrices, 25,
            CardAttribute.EnemyActionPower, -12,
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Isolamento Custoso",
            "Ignorar comércio tem consequências.",
            false,
            CardAttribute.CoinsEarned, -15,
            CardAttribute.EnemyDefense, -15,
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
    }
    
    /// <summary>
    /// Vitória muito fácil - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateBattleEasyVictoryOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Poder Crescente",
            "Canalize confiança em poder ofensivo.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 30,
            CardAttribute.EnemyActionPower, 20,
            obs.triggerType,
            "Vitória muito fácil"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Superioridade Consolidada",
            "Você está além deste desafio.",
            true,
            CardAttribute.PlayerActionPower, 28,
            CardAttribute.EnemyMaxHP, 25,
            obs.triggerType,
            "Vitória muito fácil"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Domínio Absoluto",
            "Vitórias fáceis provam sua força.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 32,
            CardAttribute.EnemyDefense, 20,
            obs.triggerType,
            "Vitória muito fácil"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Maestria Incomparável",
            "Você domina completamente o combate.",
            true,
            CardAttribute.PlayerSingleTargetActionPower, 35,
            CardAttribute.EnemyOffensiveActionPower, 18,
            obs.triggerType,
            "Vitória muito fácil"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Desafio Amplificado",
            "Universo aumenta a dificuldade para te testar.",
            false,
            CardAttribute.PlayerMaxHP, -20,
            CardAttribute.EnemyMaxHP, 40,
            obs.triggerType,
            "Vitória muito fácil"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Contra-medidas Desenvolvidas",
            "Inimigos se preparam melhor.",
            false,
            CardAttribute.PlayerActionPower, -15,
            CardAttribute.EnemyDefense, 30,
            obs.triggerType,
            "Vitória muito fácil"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Arrogância Custosa",
            "Confiança excessiva te enfraquece.",
            false,
            CardAttribute.PlayerDefense, -18,
            CardAttribute.EnemyActionPower, 25,
            obs.triggerType,
            "Vitória muito fácil"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Escalada de Poder",
            "Inimigos ficam dramaticamente mais fortes.",
            false,
            CardAttribute.PlayerMaxHP, -25,
            CardAttribute.EnemyActionPower, 30,
            obs.triggerType,
            "Vitória muito fácil"
        ));
    }
    
    /// <summary>
    /// Apenas skills caras - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateExpensiveSkillsOnlyOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        float avgCost = obs.GetData<float>("averageManaCost", 20f);
        
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Maestria Arcana",
            "Domine magias poderosas com mais eficiência.",
            true,
            CardAttribute.PlayerMaxMP, 40,
            CardAttribute.EnemyMaxMP, 25,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Eficiência Aprimorada",
            "Reduza drasticamente custos de mana.",
            true,
            CardAttribute.PlayerActionManaCost, -7,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Poder Justificado",
            "Skills caras ficam ainda mais fortes.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 35,
            CardAttribute.EnemyDefense, 20,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Reservas Infinitas",
            "MP massivamente aumentado.",
            true,
            CardAttribute.PlayerMaxMP, 45,
            CardAttribute.EnemyActionPower, 18,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Fome Voraz",
            "Magias poderosas drenam ainda mais energia.",
            false,
            CardAttribute.PlayerActionManaCost, 10,
            CardAttribute.EnemyActionManaCost, 6,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Esgotamento Rápido",
            "Você fica sem mana rapidamente.",
            false,
            CardAttribute.PlayerMaxMP, -30,
            CardAttribute.EnemyMaxMP, -15,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Desperdício Extremo",
            "Custos exorbitantes te limitam.",
            false,
            CardAttribute.PlayerActionManaCost, 12,
            CardAttribute.EnemyDefense, -10,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Dependência Cara",
            "Skills caras te deixam vulnerável.",
            false,
            CardAttribute.PlayerMaxMP, -25,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
    }
    
    /// <summary>
    /// Sem dano em área - 5 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateNoAOEDamageOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        float avgEnemies = obs.GetData<float>("averageEnemyCount", 2f);
        
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Assassino Preciso",
            "Foco absoluto em alvos únicos.",
            true,
            CardAttribute.PlayerSingleTargetActionPower, 40,
            CardAttribute.EnemyDefense, 20,
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Destruição Direcionada",
            "Ataques únicos ganham poder massivo.",
            true,
            CardAttribute.PlayerSingleTargetActionPower, 38,
            CardAttribute.EnemyMaxHP, 22,
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Perfuração Letal",
            "Especialize-se em eliminar alvos únicos.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 30,
            CardAttribute.EnemyDefense, 18,
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Foco Absoluto",
            "Concentração total resulta em devastação.",
            true,
            CardAttribute.PlayerSingleTargetActionPower, 35,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Maestria Single-Target",
            "Torne-se mestre em eliminar indivíduos.",
            true,
            CardAttribute.PlayerSingleTargetActionPower, 42,
            CardAttribute.EnemyOffensiveActionPower, 18,
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Números Crescentes",
            "Sem AOE, enfrenta hordas maiores.",
            false,
            CardAttribute.PlayerMaxHP, -20,
            CardAttribute.EnemyMaxHP, -18,
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Sobrecarga de Alvos",
            "Múltiplos inimigos te sobrecarregam.",
            false,
            CardAttribute.PlayerActionPower, -15,
            CardAttribute.EnemyMaxHP, 20,
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Vulnerabilidade Tática",
            "Falta de área te expõe.",
            false,
            CardAttribute.PlayerDefense, -12,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Limitação Estratégica",
            "Incapacidade de lidar com grupos.",
            false,
            CardAttribute.PlayerSingleTargetActionPower, -20,
            CardAttribute.EnemyDefense, -15,
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
    }
    
    /// <summary>
    /// Ficou sem dinheiro após compras - 3 vantagens + 3 desvantagens
    /// </summary>
    private static void GenerateBrokeAfterShoppingOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        int coinsLeft = obs.GetData<int>("coinsLeft", 0);
        
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Tudo ou Nada",
            "Gastar tudo mostra comprometimento.",
            true,
            CardAttribute.CoinsEarned, 35,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Investimento Sábio",
            "Seus gastos são recompensados.",
            true,
            CardAttribute.PlayerActionPower, 25,
            CardAttribute.EnemyDefense, 15,
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Recompensa por Coragem",
            "Ousadia financeira traz benefícios.",
            true,
            CardAttribute.CoinsEarned, 30,
            CardAttribute.ShopPrices, -12,
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Endividamento",
            "Gastos excessivos têm consequências.",
            false,
            CardAttribute.ShopPrices, 25,
            CardAttribute.EnemyMaxHP, -18,
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Pobreza Extrema",
            "Você ficou completamente sem recursos.",
            false,
            CardAttribute.CoinsEarned, -20,
            CardAttribute.EnemyDefense, -15,
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Crise Financeira",
            "Sua situação econômica piora.",
            false,
            CardAttribute.ShopPrices, 22,
            CardAttribute.EnemyActionPower, -10,
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
    }
    
    /// <summary>
    /// Poucas moedas com lojas disponíveis - 4 vantagens + 3 desvantagens
    /// </summary>
    private static void GenerateLowCoinsUnvisitedShopsOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        int currentCoins = obs.GetData<int>("currentCoins", 0);
        int shopsCount = obs.GetData<int>("unvisitedShopsCount", 1);
        
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Filantropia Cósmica",
            "Forças do universo concedem moedas.",
            true,
            CardAttribute.CoinsEarned, 30,
            CardAttribute.EnemySpeed, 2,
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Misericórdia Divina",
            "Sua pobreza é aliviada.",
            true,
            CardAttribute.CoinsEarned, 35,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Desconto Compassivo",
            "Comerciantes têm pena de você.",
            true,
            CardAttribute.ShopPrices, -25,
            CardAttribute.EnemyDefense, 10,
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Fortuna Renovada",
            "Recursos surgem quando mais precisa.",
            true,
            CardAttribute.CoinsEarned, 32,
            CardAttribute.EnemyMaxHP, 15,
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Círculo Vicioso",
            "Escassez de recursos persiste.",
            false,
            CardAttribute.PlayerMaxMP, -15,
            CardAttribute.ShopPrices, -25,
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Pobreza Permanente",
            "Você ganha menos moedas.",
            false,
            CardAttribute.CoinsEarned, -18,
            CardAttribute.EnemyMaxHP, -20,
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Destituição Total",
            "Recursos escasseiam ainda mais.",
            false,
            CardAttribute.CoinsEarned, -15,
            CardAttribute.EnemyDefense, -18,
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
    }
    
    #endregion
}