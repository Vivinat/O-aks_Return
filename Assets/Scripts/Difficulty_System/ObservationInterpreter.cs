// Assets/Scripts/Difficulty_System/ObservationInterpreter.cs (BALANCED & OPTIMIZED)

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Interpreta observações comportamentais e gera ofertas de negociação contextuais
/// BALANCEADO: 4 vantagens + 4 desvantagens por trigger, valores ajustados
/// </summary>
public static class ObservationInterpreter
{
    /// <summary>
    /// Interpreta uma observação e gera ofertas (retorna múltiplas opções e sorteia aleatoriamente)
    /// </summary>
    public static List<NegotiationOffer> InterpretObservation(BehaviorObservation observation)
    {
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
        
        // Sorteia 1 vantagem e 1 desvantagem aleatórias
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
    
    #region Geradores de Ofertas - BALANCEADOS (4+4)
    
    /// <summary>
    /// Morte do jogador - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GeneratePlayerDeathOffers(BehaviorObservation obs, 
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        string killerEnemy = obs.GetData<string>("killerEnemy", "Inimigo");
        
        // VANTAGENS (4 opções) - Valores moderados
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Vingança Tardia",
            "Enfraqueça seus adversários após sua queda anterior.",
            CardAttribute.EnemyMaxHP, 
            -20,
            obs.triggerType,
            $"Morreu para: {killerEnemy}"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Lição Aprendida",
            "Fortaleça-se com a experiência da derrota.",
            CardAttribute.PlayerMaxHP, 15,
            obs.triggerType,
            $"Morreu para: {killerEnemy}"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Retribuição Ofensiva",
            "Transforme sua dor em poder destrutivo.",
            CardAttribute.PlayerOffensiveActionPower, 8,
            obs.triggerType,
            $"Morreu para: {killerEnemy}"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Segunda Chance Fortalecida",
            "Mais mana para o desafio.",
            CardAttribute.PlayerMaxMP, 20,
            obs.triggerType,
            $"Morreu para: {killerEnemy}"
        ));
        
        // DESVANTAGENS (4 opções) - Custo justo
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Sede de Sangue",
            "Seus inimigos se fortalecem com a memória de sua derrota.",
            CardAttribute.EnemyOffensiveActionPower, 
            -12,
            false,
            obs.triggerType,
            $"Morreu para: {killerEnemy}"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Trauma Persistente",
            "O medo da morte enfraquece seu corpo.",
            CardAttribute.PlayerDefense,
            -8,
            true,
            obs.triggerType,
            $"Morreu para: {killerEnemy}"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Desmoralização",
            "Sua força de vontade diminui.",
            CardAttribute.PlayerOffensiveActionPower, 
            -10,
            true,
            obs.triggerType,
            $"Morreu para: {killerEnemy}"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Exaustão Mental",
            "Seu MP sofre com o trauma.",
            CardAttribute.PlayerMaxMP, -12,
            true,
            obs.triggerType,
            $"Morreu para: {killerEnemy}"
        ));
    }
    
    /// <summary>
    /// Dependência de skill única - 4 vantagens + 4 desvantagens
    /// CORRIGIDO: Modifica APENAS a skill específica dominante
    /// </summary>
    private static void GenerateSingleSkillCarryOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        string skillName = obs.GetData<string>("skillName", "Habilidade");
        
        // === VANTAGENS (4 opções) - Todas modificam a SKILL ESPECÍFICA ===
        
        // 1. Aumenta PODER da skill específica
        advantages.Add(CreateSpecificSkillPowerOffer(
            "Mestre de Uma Arte",
            $"'{skillName}' se torna devastadora.",
            skillName,
            12, // +12 de poder na skill
            true,
            obs.triggerType
        ));
        
        // 2. Reduz CUSTO DE MANA da skill específica
        advantages.Add(CreateSpecificSkillManaCostOffer(
            "Eficiência Aperfeiçoada",
            $"'{skillName}' consome menos energia.",
            skillName,
            -4, // -4 de custo de mana
            true,
            obs.triggerType
        ));
        
        // 3. Aumenta PODER e reduz MANA da skill específica
        advantages.Add(CreateSpecificSkillFullOffer(
            "Domínio Absoluto",
            $"'{skillName}' atinge a perfeição.",
            skillName,
            10,  // +10 de poder
            -3,  // -3 de custo de mana
            true,
            obs.triggerType
        ));
        
        // 4. Apenas aumenta MUITO o poder da skill
        advantages.Add(CreateSpecificSkillPowerOffer(
            "Especialização Letal",
            $"Concentre todo seu poder em '{skillName}'.",
            skillName,
            15, // +15 de poder (muito alto)
            true,
            obs.triggerType
        ));
        
        // === DESVANTAGENS (4 opções) - Metade na skill, metade gerais ===
        
        // 1. Aumenta CUSTO DE MANA da skill específica
        disadvantages.Add(CreateSpecificSkillManaCostOffer(
            "Dependência Custosa",
            $"'{skillName}' drena muito mais energia.",
            skillName,
            5, // +5 de custo de mana
            false,
            obs.triggerType
        ));
        
        // 2. Reduz PODER da skill específica
        disadvantages.Add(CreateSpecificSkillPowerOffer(
            "Uso Excessivo",
            $"'{skillName}' perde eficácia pelo uso repetido.",
            skillName,
            -8, // -8 de poder
            false,
            obs.triggerType
        ));
        
        // 3. Desvantagem geral - Inimigos ganham defesa
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Adaptação Hostil",
            "Inimigos aprendem seus padrões.",
            CardAttribute.EnemyDefense,
            10,
            false, // Afeta inimigos
            obs.triggerType,
            $"Skill dominante: {skillName}"
        ));
        
        // 4. Desvantagem geral - Jogador perde MP máximo
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Esgotamento Mental",
            "Uso excessivo drena suas reservas.",
            CardAttribute.PlayerMaxMP,
            -10,
            true, // Afeta jogador
            obs.triggerType,
            $"Skill dominante: {skillName}"
        ));
    }    
    
    /// <summary>
    /// HP baixo frequente - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateFrequentLowHPOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS - Foco em sobrevivência
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Fortificação Massiva",
            "Aumente drasticamente seu HP máximo.",
            CardAttribute.PlayerMaxHP, 20,
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Escudo Aprimorado",
            "Fortaleça suas defesas contra ataques.",
            CardAttribute.PlayerDefense, 8,
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Cura Potencializada",
            "Suas habilidades defensivas ficam mais fortes.",
            CardAttribute.PlayerDefensiveActionPower, 12,
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        advantages.Add(NegotiationOffer.CreateDisadvantage(
            "Reclamar com o Gerente",
            "As ações de todos os inimigos custam mais mana.",
            CardAttribute.EnemyActionManaCost,   
            -10,                              
            false,                            
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Fragilidade Persistente",
            "Seus problemas defensivos deixam marcas.",
            false,
            CardAttribute.PlayerDefense, -8,
            CardAttribute.EnemyDefense, -6,
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Vitalidade Comprometida",
            "Seu HP máximo é reduzido.",
            false,
            CardAttribute.PlayerMaxHP, -20,
            CardAttribute.EnemyMaxHP, -25,
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Vulnerabilidade Crescente",
            "Você se torna mais fácil de ferir.",
            false,
            CardAttribute.PlayerDefense, -10,
            CardAttribute.EnemyActionPower, 8,
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Cura Reduzida",
            "Suas habilidades de recuperação enfraquecem.",
            false,
            CardAttribute.PlayerDefensiveActionPower, -12,
            CardAttribute.EnemyDefense, -8,
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
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Eficiência Mágica",
            "Aprenda a usar habilidades com menos mana.",
            CardAttribute.PlayerActionManaCost, -8,
            obs.triggerType,
            $"Skill ignorada: {skillName}"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Versatilidade Forçada",
            "Melhore todas suas habilidades ofensivas.",
            CardAttribute.PlayerActionPower, 8,
            obs.triggerType,
            $"Skill ignorada: {skillName}"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Reservas Expandidas",
            "Compense com mais MP.",
            CardAttribute.PlayerMaxMP, 15,
            obs.triggerType,
            $"Skill ignorada: {skillName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Especialização Alternativa",
            "Foque no que você realmente usa.",
            CardAttribute.PlayerOffensiveActionPower, 16,
            obs.triggerType,
            $"Skill ignorada: {skillName}"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Arsenal Limitado",
            "Falta de versatilidade enfraquece você.",
            false,
            CardAttribute.PlayerActionPower, -10,
            CardAttribute.EnemyMaxHP, -12,
            obs.triggerType,
            $"Skill ignorada: {skillName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Desperdício Mágico",
            "Suas habilidades custam mais mana.",
            false,
            CardAttribute.PlayerActionManaCost, 4,
            CardAttribute.EnemyActionManaCost, 3,
            obs.triggerType,
            $"Skill ignorada: {skillName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Poder Reduzido",
            "Falta de treino enfraquece todas ações.",
            false,
            CardAttribute.PlayerActionPower, -8,
            CardAttribute.EnemyActionPower, -6,
            obs.triggerType,
            $"Skill ignorada: {skillName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Atrofia de Habilidade",
            "Seus ataques especializados perdem força.",
            false,
            CardAttribute.PlayerOffensiveActionPower, -12,
            CardAttribute.EnemyDefense, -8,
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
        // VANTAGENS - Compensação pela lentidão
        advantages.Add(new NegotiationOffer(
            "Primeiro Golpe Poderoso",
            "Compense agir depois com ataques devastadores.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 18,
            CardAttribute.EnemyMaxHP, 15,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Resistência Superior",
            "Se não pode ser rápido, seja resistente.",
            true,
            CardAttribute.PlayerMaxHP, 28,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Defesa Impenetrável",
            "Fortifique-se enquanto espera sua vez.",
            true,
            CardAttribute.PlayerDefense, 15,
            CardAttribute.EnemyOffensiveActionPower, 10,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Contra-ataque Letal",
            "Transforme a desvantagem em oportunidade.",
            true,
            CardAttribute.PlayerSingleTargetActionPower, 22,
            CardAttribute.EnemyDefense, 12,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Corrida Perdida",
            "Seus adversários ficam ainda mais letais.",
            false,
            CardAttribute.PlayerMaxMP, -10,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Vulnerabilidade Exposta",
            "Agir por último te deixa vulnerável.",
            false,
            CardAttribute.PlayerDefense, -8,
            CardAttribute.EnemyOffensiveActionPower, 10,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Lentidão Crítica",
            "Sua baixa velocidade afeta tudo.",
            false,
            CardAttribute.PlayerActionPower, -10,
            CardAttribute.EnemySpeed, 2,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Desvantagem Tática",
            "Perder a iniciativa tem consequências.",
            false,
            CardAttribute.PlayerMaxHP, -15,
            CardAttribute.EnemyMaxHP, 12,
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
        // VANTAGENS - Aproveitar velocidade
        advantages.Add(new NegotiationOffer(
            "Domínio Tático",
            "Mais recursos para sua vantagem inicial.",
            true,
            CardAttribute.PlayerMaxMP, 20,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Iniciativa Letal",
            "Transforme velocidade em poder destrutivo.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 18,
            CardAttribute.EnemyMaxHP, 16,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Abertura Perfeita",
            "Maximize o dano no primeiro turno.",
            true,
            CardAttribute.PlayerSingleTargetActionPower, 24,
            CardAttribute.EnemyDefense, 15,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Velocidade Esmagadora",
            "Aproveite sua vantagem de velocidade.",
            true,
            CardAttribute.PlayerActionPower, 16,
            CardAttribute.EnemySpeed, 2,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        // DESVANTAGENS - Compensação por ser muito forte
        disadvantages.Add(new NegotiationOffer(
            "Força Bruta Inimiga",
            "Inimigos compensam com puro poder.",
            false,
            CardAttribute.PlayerDefense, -6,
            CardAttribute.EnemyActionPower, 18,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Fragilidade Veloz",
            "Velocidade às custas de resistência.",
            false,
            CardAttribute.PlayerMaxHP, -20,
            CardAttribute.EnemyMaxHP, 12,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Exaustão Rápida",
            "Agir primeiro drena sua energia.",
            false,
            CardAttribute.PlayerMaxMP, -15,
            CardAttribute.EnemyMaxMP, -8,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Contra-medidas Aprendidas",
            "Inimigos se preparam para sua velocidade.",
            false,
            CardAttribute.PlayerActionPower, -8,
            CardAttribute.EnemyDefense, 12,
            obs.triggerType,
            "Sempre age primeiro"
        ));
    }
    
    /// <summary>
    /// Dificuldade vs Tanks - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateStrugglesAgainstTanksOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS - Anti-tank
        advantages.Add(new NegotiationOffer(
            "Quebra-Couraças",
            "Perfure as defesas mais robustas.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 22,
            CardAttribute.EnemyDefense, 15,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Perfuração Letal",
            "Seus ataques ignoram parte da defesa inimiga.",
            true,
            CardAttribute.PlayerSingleTargetActionPower, 25,
            CardAttribute.EnemyMaxHP, 18,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Atrito Eficiente",
            "Cause dano constante em alvos resistentes.",
            true,
            CardAttribute.PlayerActionPower, 18,
            CardAttribute.EnemyDefense, 12,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Fúria Persistente",
            "Batalhas longas favorecem você.",
            true,
            CardAttribute.PlayerMaxMP, 25,
            CardAttribute.EnemyDefense, 10,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Fortaleza Impenetrável",
            "Inimigos endurecem suas defesas.",
            false,
            CardAttribute.PlayerActionPower, -8,
            CardAttribute.EnemyMaxHP, 28,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Muralha Crescente",
            "Defesas inimigas aumentam drasticamente.",
            false,
            CardAttribute.PlayerOffensiveActionPower, -10,
            CardAttribute.EnemyDefense, 20,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Frustração Desgastante",
            "Sua incapacidade de penetrar defesas te esgota.",
            false,
            CardAttribute.PlayerMaxMP, -15,
            CardAttribute.EnemyMaxHP, 15,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Impotência Ofensiva",
            "Seus ataques enfraquecem contra alvos resistentes.",
            false,
            CardAttribute.PlayerSingleTargetActionPower, -15,
            CardAttribute.EnemyDefense, 8,
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
        // VANTAGENS - Anti-speed
        advantages.Add(new NegotiationOffer(
            "Antecipação",
            "Aprenda a se defender de adversários velozes.",
            true,
            CardAttribute.PlayerDefense, 12,
            CardAttribute.EnemyMaxHP, 15,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Contra-ataque Preciso",
            "Ataques mais fortes compensam falta de velocidade.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 20,
            CardAttribute.EnemySpeed, -2,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Muralha Impenetrável",
            "Se não pode alcançá-los, seja impossível de ferir.",
            true,
            CardAttribute.PlayerMaxHP, 28,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Reflexos Aprimorados",
            "Melhore sua capacidade defensiva.",
            true,
            CardAttribute.PlayerDefense, 15,
            CardAttribute.EnemyOffensiveActionPower, 10,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Velocidade Cegante",
            "Inimigos ficam impossíveis de acompanhar.",
            false,
            CardAttribute.PlayerDefense, -8,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Ataques Implacáveis",
            "Inimigos rápidos te acertam com mais frequência.",
            false,
            CardAttribute.PlayerMaxHP, -20,
            CardAttribute.EnemyOffensiveActionPower, 12,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Lentidão Crítica",
            "Sua baixa velocidade te prejudica ainda mais.",
            false,
            CardAttribute.PlayerActionPower, -10,
            CardAttribute.EnemySpeed, 3,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Sobrecarga Defensiva",
            "Tentar se defender esgota você.",
            false,
            CardAttribute.PlayerMaxMP, -15,
            CardAttribute.EnemyActionPower, 10,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
    }
    
    /// <summary>
    /// Dificuldade vs Múltiplos - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateStrugglesAgainsSwarmsOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        int enemyCount = obs.GetData<int>("enemyCount", 3);
        
        // VANTAGENS - Anti-swarm
        advantages.Add(new NegotiationOffer(
            "Destruição em Massa",
            "Habilidades de área ganham poder devastador.",
            true,
            CardAttribute.PlayerAOEActionPower, 25,
            CardAttribute.EnemyActionPower, 10,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Tempestade de Lâminas",
            "Todos seus ataques ficam mais fortes contra grupos.",
            true,
            CardAttribute.PlayerActionPower, 18,
            CardAttribute.EnemyMaxHP, 12,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Resistência de Multidão",
            "Fortaleça-se para enfrentar números superiores.",
            true,
            CardAttribute.PlayerMaxHP, 32,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Defesa Circular",
            "Proteja-se de ataques de múltiplas direções.",
            true,
            CardAttribute.PlayerDefense, 16,
            CardAttribute.EnemyOffensiveActionPower, 10,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Horda Implacável",
            "Mais inimigos surgem, mais fortes.",
            false,
            CardAttribute.PlayerMaxHP, -15,
            CardAttribute.EnemyMaxHP, 25,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Sobrecarga Numérica",
            "Números esmagadores te enfraquecem.",
            false,
            CardAttribute.PlayerActionPower, -12,
            CardAttribute.EnemyActionPower, 10,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Fadiga de Combate",
            "Lutar contra múltiplos te esgota.",
            false,
            CardAttribute.PlayerMaxMP, -15,
            CardAttribute.EnemyMaxMP, -8,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Área Insuficiente",
            "Suas habilidades de área enfraquecem.",
            false,
            CardAttribute.PlayerAOEActionPower, -16,
            CardAttribute.EnemyMaxHP, -12,
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
        // VANTAGENS - Sobrevivência inicial
        advantages.Add(new NegotiationOffer(
            "Início Fortificado",
            "Comece com defesas massivas.",
            true,
            CardAttribute.PlayerMaxHP, 35,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Escudo Inicial",
            "Resistência aprimorada desde o começo.",
            true,
            CardAttribute.PlayerDefense, 18,
            CardAttribute.EnemyOffensiveActionPower, 12,
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Barreira Protetora",
            "Proteção extra nos primeiros turnos.",
            true,
            CardAttribute.PlayerDefense, 15,
            CardAttribute.EnemyActionPower, 16,
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Vitalidade Reforçada",
            "HP e defesa aumentados.",
            true,
            CardAttribute.PlayerMaxHP, 28,
            CardAttribute.EnemyMaxHP, 16,
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Assalto Inicial",
            "Inimigos investem tudo no início.",
            false,
            CardAttribute.PlayerDefense, -10,
            CardAttribute.EnemyActionPower, 25,
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Fragilidade Crítica",
            "Você começa mais vulnerável.",
            false,
            CardAttribute.PlayerMaxHP, -25,
            CardAttribute.EnemyDefense, -12,
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Abertura Fatal",
            "Primeiros turnos são ainda mais perigosos.",
            false,
            CardAttribute.PlayerDefense, -12,
            CardAttribute.EnemyOffensiveActionPower, 16,
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Início Desastroso",
            "Tudo começa mal para você.",
            false,
            CardAttribute.PlayerMaxHP, -20,
            CardAttribute.EnemyActionPower, 15,
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
        // VANTAGENS - Guerra de atrito
        advantages.Add(new NegotiationOffer(
            "Resistência Prolongada",
            "Ganhe fôlego para batalhas longas.",
            true,
            CardAttribute.PlayerMaxMP, 25,
            CardAttribute.EnemyMaxHP, 20,
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Regeneração Persistente",
            "Suas habilidades defensivas melhoram.",
            true,
            CardAttribute.PlayerDefensiveActionPower, 20,
            CardAttribute.EnemyOffensiveActionPower, 12,
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Stamina Infinita",
            "Batalhas longas favorecem você.",
            true,
            CardAttribute.PlayerMaxMP, 28,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Fortificação Crescente",
            "Você fica mais forte com o tempo.",
            true,
            CardAttribute.PlayerDefense, 16,
            CardAttribute.EnemyDefense, 10,
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Guerra de Atrito",
            "Batalhas longas favorecem adversários.",
            false,
            CardAttribute.PlayerActionManaCost, 5,
            CardAttribute.EnemyDefense, 15,
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Exaustão Inevitável",
            "Você se cansa em batalhas prolongadas.",
            false,
            CardAttribute.PlayerMaxMP, -20,
            CardAttribute.EnemyMaxMP, 12,
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Desgaste Acumulado",
            "Dano constante te enfraquece.",
            false,
            CardAttribute.PlayerDefense, -10,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Esgotamento Total",
            "Longas batalhas drenam tudo de você.",
            false,
            CardAttribute.PlayerMaxHP, -16,
            CardAttribute.EnemyMaxHP, 8,
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
        // VANTAGENS - Anti-chip damage
        advantages.Add(new NegotiationOffer(
            "Pele de Aço",
            "Fortifique-se contra ataques menores.",
            true,
            CardAttribute.PlayerDefense, 16,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Regeneração Constante",
            "Curas mais eficazes.",
            true,
            CardAttribute.PlayerDefensiveActionPower, 22,
            CardAttribute.EnemyOffensiveActionPower, 10,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Armadura Reforçada",
            "Reduza todo dano recebido.",
            true,
            CardAttribute.PlayerDefense, 20,
            CardAttribute.EnemyDefense, 10,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Vitalidade Robusta",
            "Mais HP para aguentar o atrito.",
            true,
            CardAttribute.PlayerMaxHP, 32,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Mil Cortes",
            "Inimigos atacam com mais força e frequência.",
            false,
            CardAttribute.PlayerDefense, -10,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Sangramento Perpétuo",
            "Dano acumulado aumenta.",
            false,
            CardAttribute.PlayerMaxHP, -20,
            CardAttribute.EnemyOffensiveActionPower, 12,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Armadura Desgastada",
            "Sua defesa se deteriora.",
            false,
            CardAttribute.PlayerDefense, -12,
            CardAttribute.EnemyActionPower, 10,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Feridas Abertas",
            "Cada hit dói mais.",
            false,
            CardAttribute.PlayerDefense, -8,
            CardAttribute.EnemyOffensiveActionPower, 15,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
    }
    
    /// <summary>
    /// Vulnerável a one-shots - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateOneHitKOVulnerableOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        int biggestHit = obs.GetData<int>("biggestHit", 50);
        
        // VANTAGENS - Anti-burst
        advantages.Add(new NegotiationOffer(
            "Fortaleza Viva",
            "Torne-se resistente a ataques devastadores.",
            true,
            CardAttribute.PlayerMaxHP, 38,
            CardAttribute.EnemyMaxHP, 28,
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Escudo Absoluto",
            "Defesa massiva contra golpes fortes.",
            true,
            CardAttribute.PlayerDefense, 20,
            CardAttribute.EnemyActionPower, 16,
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Barreira Suprema",
            "Proteção contra dano explosivo.",
            true,
            CardAttribute.PlayerDefense, 18,
            CardAttribute.EnemyOffensiveActionPower, 15,
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Corpo Adamantino",
            "HP e defesa aumentados drasticamente.",
            true,
            CardAttribute.PlayerMaxHP, 35,
            CardAttribute.EnemyDefense, 16,
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Golpe Executivo",
            "Adversários aperfeiçoam ataques letais.",
            false,
            CardAttribute.PlayerDefense, -12,
            CardAttribute.EnemyActionPower, 28,
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Fragilidade Extrema",
            "Você fica ainda mais vulnerável.",
            false,
            CardAttribute.PlayerMaxHP, -25,
            CardAttribute.EnemyOffensiveActionPower, 20,
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Armadura de Papel",
            "Defesa drasticamente reduzida.",
            false,
            CardAttribute.PlayerDefense, -16,
            CardAttribute.EnemyActionPower, 16,
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Alvo Fácil",
            "Inimigos miram em suas fraquezas.",
            false,
            CardAttribute.PlayerMaxHP, -20,
            CardAttribute.EnemyOffensiveActionPower, 16,
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
            CardAttribute.PlayerMaxHP, 32,
            CardAttribute.EnemyDefense, 12,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Regeneração Natural",
            "Habilidades defensivas melhoradas.",
            true,
            CardAttribute.PlayerDefensiveActionPower, 25,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Defesa Compensatória",
            "Mais defesa para evitar dano.",
            true,
            CardAttribute.PlayerDefense, 16,
            CardAttribute.EnemyOffensiveActionPower, 10,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Fortaleza Adaptativa",
            "HP e defesa aumentados.",
            true,
            CardAttribute.PlayerMaxHP, 28,
            CardAttribute.EnemyMaxHP, 16,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Feridas Abertas",
            "Feridas o tornam mais vulnerável.",
            false,
            CardAttribute.PlayerDefense, -10,
            CardAttribute.EnemyMaxHP, -15,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Corpo Fragilizado",
            "Falta de cura te deixa fraco.",
            false,
            CardAttribute.PlayerMaxHP, -20,
            CardAttribute.EnemyDefense, -12,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Hemorragia Perpétua",
            "Feridas não cicatrizam.",
            false,
            CardAttribute.PlayerDefensiveActionPower, -16,
            CardAttribute.EnemyActionPower, -8,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Vulnerabilidade Crítica",
            "Sem cura, você fica exposto.",
            false,
            CardAttribute.PlayerDefense, -12,
            CardAttribute.EnemyOffensiveActionPower, 10,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
    }
    
    /// <summary>
    /// Problemas de mana - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateManaIssuesOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS
        advantages.Add(new NegotiationOffer(
            "Reservas Mágicas",
            "Amplie drasticamente suas reservas de mana.",
            true,
            CardAttribute.PlayerMaxMP, 28,
            CardAttribute.EnemyMaxMP, 16,
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Eficiência Arcana",
            "Reduza o custo de todas habilidades.",
            true,
            CardAttribute.PlayerActionManaCost, -5,
            CardAttribute.EnemyActionPower, 10,
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Maestria Energética",
            "Use mana com muito mais eficiência.",
            true,
            CardAttribute.PlayerActionManaCost, -4,
            CardAttribute.EnemyMaxMP, 12,
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Poço Infinito",
            "MP massivamente aumentado.",
            true,
            CardAttribute.PlayerMaxMP, 32,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Exaustão Mágica",
            "Habilidades drenam ainda mais energia.",
            false,
            CardAttribute.PlayerActionManaCost, 6,
            CardAttribute.EnemyActionManaCost, -3,
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Reservas Esgotadas",
            "Seu MP máximo diminui.",
            false,
            CardAttribute.PlayerMaxMP, -20,
            CardAttribute.EnemyMaxMP, -12,
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Dreno Arcano",
            "Magia custa mais e você tem menos.",
            false,
            CardAttribute.PlayerMaxMP, -16,
            CardAttribute.EnemyDefense, -10,
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Desperdício Mágico",
            "Suas habilidades ficam muito caras.",
            false,
            CardAttribute.PlayerActionManaCost, 7,
            CardAttribute.EnemyActionPower, -8,
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
            CardAttribute.PlayerMaxHP, 28,
            CardAttribute.EnemyMaxHP, 20,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Força Interior",
            "Desenvolva poder independente.",
            true,
            CardAttribute.PlayerDefensiveActionPower, 25,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Recursos Internos",
            "Mais MP para habilidades próprias.",
            true,
            CardAttribute.PlayerMaxMP, 28,
            CardAttribute.EnemyDefense, 12,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Independência Total",
            "Stats permanentes aumentados.",
            true,
            CardAttribute.PlayerActionPower, 16,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Escassez Material",
            "Recursos se tornam ainda mais raros.",
            false,
            CardAttribute.CoinsEarned, -12,
            CardAttribute.EnemyDefense, -10,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Dependência Crítica",
            "Estratégia baseada em itens te enfraquece.",
            false,
            CardAttribute.PlayerDefense, -12,
            CardAttribute.EnemyActionPower, -10,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Fragilidade Sem Itens",
            "Sem consumíveis, você é fraco.",
            false,
            CardAttribute.PlayerMaxHP, -20,
            CardAttribute.EnemyMaxHP, -16,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Vício Custoso",
            "Sua dependência cobra um preço alto.",
            false,
            CardAttribute.PlayerActionPower, -10,
            CardAttribute.EnemyDefense, -8,
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
            CardAttribute.PlayerActionPower, 25,
            CardAttribute.CoinsEarned, -15,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Domínio Total",
            "Você está além do desafio.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 28,
            CardAttribute.EnemyMaxHP, 25,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Superioridade Marcial",
            "Todos seus ataques ficam mais fortes.",
            true,
            CardAttribute.PlayerActionPower, 22,
            CardAttribute.EnemyActionPower, 16,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Maestria Completa",
            "Aumente seu poder ofensivo.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 25,
            CardAttribute.EnemyDefense, 16,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Lição Aprendida",
            "Inimigos estudam sua técnica.",
            false,
            CardAttribute.PlayerMaxHP, -20,
            CardAttribute.EnemyDefense, 20,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Arrogância Fatal",
            "Confiança excessiva te enfraquece.",
            false,
            CardAttribute.PlayerDefense, -12,
            CardAttribute.EnemyActionPower, 16,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Desafio Amplificado",
            "Universo aumenta a dificuldade.",
            false,
            CardAttribute.PlayerMaxHP, -16,
            CardAttribute.EnemyMaxHP, 32,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Contra-medidas Desenvolvidas",
            "Inimigos se preparam melhor.",
            false,
            CardAttribute.PlayerActionPower, -12,
            CardAttribute.EnemyDefense, 20,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
    }
    
    /// <summary>
    /// Item esgotado - 4 vantagens + 4 desvantagens
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
            CardAttribute.PlayerMaxHP, 28,
            CardAttribute.EnemyMaxHP, 22,
            obs.triggerType,
            $"Item esgotado: {itemName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Adaptação Forçada",
            "Aprenda a lutar sem depender de itens.",
            true,
            CardAttribute.PlayerActionPower, 18,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            $"Item esgotado: {itemName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Autossuficiência Desenvolvida",
            "Habilidades próprias ficam mais fortes.",
            true,
            CardAttribute.PlayerDefensiveActionPower, 22,
            CardAttribute.EnemyDefense, 12,
            obs.triggerType,
            $"Item esgotado: {itemName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Compensação Natural",
            "Seus atributos base melhoram.",
            true,
            CardAttribute.PlayerMaxMP, 24,
            CardAttribute.EnemyMaxMP, 15,
            obs.triggerType,
            $"Item esgotado: {itemName}"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Dependência Crítica",
            "Estratégia baseada em itens te enfraquece.",
            false,
            CardAttribute.PlayerDefense, -12,
            CardAttribute.EnemyActionPower, -10,
            obs.triggerType,
            $"Item esgotado: {itemName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Escassez Permanente",
            "Recursos ficam ainda mais raros.",
            false,
            CardAttribute.CoinsEarned, -15,
            CardAttribute.EnemyMaxHP, -12,
            obs.triggerType,
            $"Item esgotado: {itemName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Fragilidade Exposta",
            "Sem itens, suas fraquezas aparecem.",
            false,
            CardAttribute.PlayerMaxHP, -16,
            CardAttribute.EnemyDefense, -12,
            obs.triggerType,
            $"Item esgotado: {itemName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Despreparo",
            "Falta de recursos te prejudica.",
            false,
            CardAttribute.PlayerActionPower, -10,
            CardAttribute.EnemyMaxHP, -10,
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
            CardAttribute.PlayerOffensiveActionPower, 28,
            CardAttribute.EnemyActionPower, 20,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Ataque Esmagador",
            "Todos ataques ganham poder massivo.",
            true,
            CardAttribute.PlayerActionPower, 25,
            CardAttribute.EnemyDefense, 16,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Destruição Pura",
            "Especialize-se em dano.",
            true,
            CardAttribute.PlayerSingleTargetActionPower, 30,
            CardAttribute.EnemyMaxHP, 20,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Força Bruta",
            "Compensação ofensiva total.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 26,
            CardAttribute.EnemyOffensiveActionPower, 16,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Vidro e Canhão",
            "Sem defesas, você é frágil.",
            false,
            CardAttribute.PlayerMaxHP, -18,
            CardAttribute.EnemyActionPower, 25,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Fragilidade Extrema",
            "Falta de defesa te enfraquece.",
            false,
            CardAttribute.PlayerDefense, -15,
            CardAttribute.EnemyMaxHP, -12,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Vulnerabilidade Total",
            "Você é um alvo fácil.",
            false,
            CardAttribute.PlayerMaxHP, -25,
            CardAttribute.EnemyActionPower, 16,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Defesa Inexistente",
            "Inimigos exploram sua fragilidade.",
            false,
            CardAttribute.PlayerDefense, -12,
            CardAttribute.EnemyOffensiveActionPower, 18,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
    }
    
    /// <summary>
    /// Morte repetida em boss - 4 vantagens + 4 desvantagens
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
            CardAttribute.PlayerActionPower, 32,
            CardAttribute.EnemyMaxHP, 28,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Lições do Fracasso",
            "Aprenda com cada derrota.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 30,
            CardAttribute.EnemyDefense, 20,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Resistência Forjada",
            "Fortifique-se através do sofrimento.",
            true,
            CardAttribute.PlayerMaxHP, 36,
            CardAttribute.EnemyActionPower, 25,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Determinação Inabalável",
            "Cada derrota te fortalece.",
            true,
            CardAttribute.PlayerDefense, 18,
            CardAttribute.EnemyOffensiveActionPower, 15,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Sede por Sangue",
            $"{bossName} se fortalece com cada vitória.",
            false,
            CardAttribute.PlayerMaxMP, -16,
            CardAttribute.EnemyActionPower, 28,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Trauma Profundo",
            "Mortes repetidas te enfraquecem mentalmente.",
            false,
            CardAttribute.PlayerMaxHP, -25,
            CardAttribute.EnemyMaxHP, 16,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Desmoralização Total",
            "Derrotas consecutivas te quebram.",
            false,
            CardAttribute.PlayerActionPower, -15,
            CardAttribute.EnemyDefense, 12,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Medo Paralisante",
            $"Pavor de {bossName} te enfraquece.",
            false,
            CardAttribute.PlayerDefense, -12,
            CardAttribute.EnemyOffensiveActionPower, 20,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
    }
    
    /// <summary>
    /// Loja ignorada - 4 vantagens + 4 desvantagens
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
            CardAttribute.CoinsEarned, 20,
            CardAttribute.ShopPrices, 10,
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Autossuficiência Premiada",
            "Não precisar de itens te fortalece.",
            true,
            CardAttribute.PlayerActionPower, 18,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Economia Sábia",
            "Guardar recursos tem benefícios.",
            true,
            CardAttribute.CoinsEarned, 22,
            CardAttribute.EnemyDefense, 10,
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Poupança Recompensada",
            "Mais moedas e preços melhores.",
            true,
            CardAttribute.CoinsEarned, 18,
            CardAttribute.EnemyMaxHP, 15,
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Inflação Galopante",
            "Comerciantes aumentam os preços.",
            false,
            CardAttribute.ShopPrices, 25,
            CardAttribute.EnemyMaxHP, -20,
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Escassez Forçada",
            "Recursos ficam mais caros.",
            false,
            CardAttribute.ShopPrices, 20,
            CardAttribute.EnemyActionPower, -10,
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Isolamento Custoso",
            "Ignorar comércio tem consequências.",
            false,
            CardAttribute.CoinsEarned, -12,
            CardAttribute.EnemyDefense, -12,
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Oportunidades Perdidas",
            "Perder lojas te enfraquece economicamente.",
            false,
            CardAttribute.CoinsEarned, -10,
            CardAttribute.EnemyMaxHP, -10,
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
            CardAttribute.PlayerOffensiveActionPower, 25,
            CardAttribute.EnemyActionPower, 16,
            obs.triggerType,
            "Vitória muito fácil"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Superioridade Consolidada",
            "Você está além deste desafio.",
            true,
            CardAttribute.PlayerActionPower, 22,
            CardAttribute.EnemyMaxHP, 20,
            obs.triggerType,
            "Vitória muito fácil"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Domínio Absoluto",
            "Vitórias fáceis provam sua força.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 26,
            CardAttribute.EnemyDefense, 16,
            obs.triggerType,
            "Vitória muito fácil"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Maestria Incomparável",
            "Você domina completamente o combate.",
            true,
            CardAttribute.PlayerSingleTargetActionPower, 28,
            CardAttribute.EnemyOffensiveActionPower, 15,
            obs.triggerType,
            "Vitória muito fácil"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Desafio Amplificado",
            "Universo aumenta a dificuldade para te testar.",
            false,
            CardAttribute.PlayerMaxHP, -16,
            CardAttribute.EnemyMaxHP, 32,
            obs.triggerType,
            "Vitória muito fácil"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Contra-medidas Desenvolvidas",
            "Inimigos se preparam melhor.",
            false,
            CardAttribute.PlayerActionPower, -12,
            CardAttribute.EnemyDefense, 25,
            obs.triggerType,
            "Vitória muito fácil"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Arrogância Custosa",
            "Confiança excessiva te enfraquece.",
            false,
            CardAttribute.PlayerDefense, -15,
            CardAttribute.EnemyActionPower, 20,
            obs.triggerType,
            "Vitória muito fácil"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Escalada de Poder",
            "Inimigos ficam dramaticamente mais fortes.",
            false,
            CardAttribute.PlayerMaxHP, -20,
            CardAttribute.EnemyActionPower, 25,
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
            CardAttribute.PlayerMaxMP, 32,
            CardAttribute.EnemyMaxMP, 20,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Eficiência Aprimorada",
            "Reduza drasticamente custos de mana.",
            true,
            CardAttribute.PlayerActionManaCost, -6,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Poder Justificado",
            "Skills caras ficam ainda mais fortes.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 28,
            CardAttribute.EnemyDefense, 16,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Reservas Infinitas",
            "MP massivamente aumentado.",
            true,
            CardAttribute.PlayerMaxMP, 36,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Fome Voraz",
            "Magias poderosas drenam ainda mais energia.",
            false,
            CardAttribute.PlayerActionManaCost, 8,
            CardAttribute.EnemyActionManaCost, 5,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Esgotamento Rápido",
            "Você fica sem mana rapidamente.",
            false,
            CardAttribute.PlayerMaxMP, -25,
            CardAttribute.EnemyMaxMP, -12,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Desperdício Extremo",
            "Custos exorbitantes te limitam.",
            false,
            CardAttribute.PlayerActionManaCost, 10,
            CardAttribute.EnemyDefense, -8,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Dependência Cara",
            "Skills caras te deixam vulnerável.",
            false,
            CardAttribute.PlayerMaxMP, -20,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
    }
    
    /// <summary>
    /// Sem dano em área - 4 vantagens + 4 desvantagens
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
            CardAttribute.PlayerSingleTargetActionPower, 32,
            CardAttribute.EnemyDefense, 16,
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Destruição Direcionada",
            "Ataques únicos ganham poder massivo.",
            true,
            CardAttribute.PlayerSingleTargetActionPower, 30,
            CardAttribute.EnemyMaxHP, 18,
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Perfuração Letal",
            "Especialize-se em eliminar alvos únicos.",
            true,
            CardAttribute.PlayerOffensiveActionPower, 25,
            CardAttribute.EnemyDefense, 15,
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Foco Absoluto",
            "Concentração total resulta em devastação.",
            true,
            CardAttribute.PlayerSingleTargetActionPower, 28,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Números Crescentes",
            "Sem AOE, enfrenta hordas maiores.",
            false,
            CardAttribute.PlayerMaxHP, -16,
            CardAttribute.EnemyMaxHP, -15,
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Sobrecarga de Alvos",
            "Múltiplos inimigos te sobrecarregam.",
            false,
            CardAttribute.PlayerActionPower, -12,
            CardAttribute.EnemyMaxHP, 16,
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Vulnerabilidade Tática",
            "Falta de área te expõe.",
            false,
            CardAttribute.PlayerDefense, -10,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Limitação Estratégica",
            "Incapacidade de lidar com grupos.",
            false,
            CardAttribute.PlayerSingleTargetActionPower, -16,
            CardAttribute.EnemyDefense, -12,
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
    }
    
    /// <summary>
    /// Ficou sem dinheiro após compras - 4 vantagens + 4 desvantagens
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
            CardAttribute.CoinsEarned, 28,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Investimento Sábio",
            "Seus gastos são recompensados.",
            true,
            CardAttribute.PlayerActionPower, 20,
            CardAttribute.EnemyDefense, 12,
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Recompensa por Coragem",
            "Ousadia financeira traz benefícios.",
            true,
            CardAttribute.CoinsEarned, 25,
            CardAttribute.ShopPrices, -10,
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Fortuna Renovada",
            "Você recupera recursos rapidamente.",
            true,
            CardAttribute.CoinsEarned, 30,
            CardAttribute.EnemyMaxHP, 15,
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Endividamento",
            "Gastos excessivos têm consequências.",
            false,
            CardAttribute.ShopPrices, 20,
            CardAttribute.EnemyMaxHP, -15,
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Pobreza Extrema",
            "Você ficou completamente sem recursos.",
            false,
            CardAttribute.CoinsEarned, -16,
            CardAttribute.EnemyDefense, -12,
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Crise Financeira",
            "Sua situação econômica piora.",
            false,
            CardAttribute.ShopPrices, 18,
            CardAttribute.EnemyActionPower, -8,
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Despesas Imprudentes",
            "Gastar demais te prejudica.",
            false,
            CardAttribute.CoinsEarned, -14,
            CardAttribute.EnemyMaxHP, -10,
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
    }
    
    /// <summary>
    /// Poucas moedas com lojas disponíveis - 4 vantagens + 4 desvantagens
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
            CardAttribute.CoinsEarned, 25,
            CardAttribute.EnemySpeed, 2,
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Misericórdia Divina",
            "Sua pobreza é aliviada.",
            true,
            CardAttribute.CoinsEarned, 28,
            CardAttribute.EnemyActionPower, 10,
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Desconto Compassivo",
            "Comerciantes têm pena de você.",
            true,
            CardAttribute.ShopPrices, -20,
            CardAttribute.EnemyDefense, 8,
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
        
        advantages.Add(new NegotiationOffer(
            "Fortuna Renovada",
            "Recursos surgem quando mais precisa.",
            true,
            CardAttribute.CoinsEarned, 26,
            CardAttribute.EnemyMaxHP, 12,
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(new NegotiationOffer(
            "Círculo Vicioso",
            "Escassez de recursos persiste.",
            false,
            CardAttribute.PlayerMaxMP, -12,
            CardAttribute.ShopPrices, -20,
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Pobreza Permanente",
            "Você ganha menos moedas.",
            false,
            CardAttribute.CoinsEarned, -15,
            CardAttribute.EnemyMaxHP, -16,
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Destituição Total",
            "Recursos escasseiam ainda mais.",
            false,
            CardAttribute.CoinsEarned, -12,
            CardAttribute.EnemyDefense, -15,
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
        
        disadvantages.Add(new NegotiationOffer(
            "Miséria Crescente",
            "Sua situação financeira piora.",
            false,
            CardAttribute.CoinsEarned, -10,
            CardAttribute.EnemyActionPower, -8,
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
    }
    
    #endregion
    
    #region Métodos Helper para Ofertas de Skill Específica
    
    /// <summary>
    /// Cria oferta que modifica APENAS O PODER de uma skill específica
    /// </summary>
    private static NegotiationOffer CreateSpecificSkillPowerOffer(
        string offerName,
        string description,
        string targetSkillName,
        int powerChange,
        bool isAdvantage,
        BehaviorTriggerType triggerType)
    {
        var offer = new NegotiationOffer
        {
            offerName = offerName,
            offerDescription = description,
            isAdvantage = isAdvantage,
            targetAttribute = CardAttribute.PlayerActionPower, // Usado como placeholder
            value = powerChange,
            affectsPlayer = true,
            sourceObservationType = triggerType,
            contextualInfo = $"Skill: {targetSkillName}"
        };
        
        // MARCA ESPECIAL: Esta oferta modifica skill específica
        offer.SetData("isSpecificSkill", true);
        offer.SetData("targetSkillName", targetSkillName);
        offer.SetData("modifyPower", true);
        offer.SetData("powerChange", powerChange);
        
        return offer;
    }
    
    /// <summary>
    /// Cria oferta que modifica APENAS O CUSTO DE MANA de uma skill específica
    /// </summary>
    private static NegotiationOffer CreateSpecificSkillManaCostOffer(
        string offerName,
        string description,
        string targetSkillName,
        int manaCostChange,
        bool isAdvantage,
        BehaviorTriggerType triggerType)
    {
        var offer = new NegotiationOffer
        {
            offerName = offerName,
            offerDescription = description,
            isAdvantage = isAdvantage,
            targetAttribute = CardAttribute.PlayerActionManaCost, // Usado como placeholder
            value = manaCostChange,
            affectsPlayer = true,
            sourceObservationType = triggerType,
            contextualInfo = $"Skill: {targetSkillName}"
        };
        
        // MARCA ESPECIAL
        offer.SetData("isSpecificSkill", true);
        offer.SetData("targetSkillName", targetSkillName);
        offer.SetData("modifyManaCost", true);
        offer.SetData("manaCostChange", manaCostChange);
        
        return offer;
    }
    
    /// <summary>
    /// Cria oferta que modifica PODER E MANA de uma skill específica
    /// </summary>
    private static NegotiationOffer CreateSpecificSkillFullOffer(
        string offerName,
        string description,
        string targetSkillName,
        int powerChange,
        int manaCostChange,
        bool isAdvantage,
        BehaviorTriggerType triggerType)
    {
        var offer = new NegotiationOffer
        {
            offerName = offerName,
            offerDescription = description,
            isAdvantage = isAdvantage,
            targetAttribute = CardAttribute.PlayerActionPower, // Placeholder
            value = powerChange,
            affectsPlayer = true,
            sourceObservationType = triggerType,
            contextualInfo = $"Skill: {targetSkillName}"
        };
        
        // MARCA ESPECIAL
        offer.SetData("isSpecificSkill", true);
        offer.SetData("targetSkillName", targetSkillName);
        offer.SetData("modifyPower", true);
        offer.SetData("modifyManaCost", true);
        offer.SetData("powerChange", powerChange);
        offer.SetData("manaCostChange", manaCostChange);
        
        return offer;
    }
    
    #endregion
}