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
            -2,
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
    /// </summary>
    private static void GenerateSingleSkillCarryOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        string skillName = obs.GetData<string>("skillName", "Habilidade");
        
        // === VANTAGENS (4 opções) - Todas modificam a SKILL ESPECÍFICA ===
        
        advantages.Add(CreateSpecificSkillPowerOffer(
            "Mestre de Uma Arte",
            $"'{skillName}' se torna devastadora.",
            skillName,
            10, 
            true,
            obs.triggerType
        ));
        
        // 2. Reduz CUSTO DE MANA da skill específica
        advantages.Add(CreateSpecificSkillManaCostOffer(
            "Eficiência Aperfeiçoada",
            $"'{skillName}' consome menos energia.",
            skillName,
            -5,
            true,
            obs.triggerType
        ));
        
        // 3. Aumenta PODER e reduz MANA da skill específica
        advantages.Add(CreateSpecificSkillFullOffer(
            "Domínio Absoluto",
            $"'{skillName}' atinge a perfeição.",
            skillName,
            10, 
            -5,  
            true,
            obs.triggerType
        ));
        
        advantages.Add(CreateSpecificSkillPowerOffer(
            "Especialização Letal",
            $"Concentre todo seu poder em '{skillName}'.",
            skillName,
            10, 
            true,
            obs.triggerType
        ));

        disadvantages.Add(CreateSpecificSkillManaCostOffer(
            "Dependência Custosa",
            $"'{skillName}' drena muito mais energia.",
            skillName,
            10,
            false,
            obs.triggerType
        ));
        
        disadvantages.Add(CreateSpecificSkillPowerOffer(
            "Uso Excessivo",
            $"'{skillName}' perde eficácia pelo uso repetido.",
            skillName,
            -10, 
            false,
            obs.triggerType
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Adaptação Hostil",
            "Inimigos aprendem seus padrões.",
            CardAttribute.EnemyDefense,
            2,
            false, 
            obs.triggerType,
            $"Skill dominante: {skillName}"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Esgotamento Mental",
            "Uso excessivo drena suas reservas.",
            CardAttribute.PlayerMaxMP,
            -10,
            true, 
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
            CardAttribute.PlayerDefense, 2,
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
            10,                              
            false,                            
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Fragilidade Persistente",
            "Seus problemas defensivos deixam marcas.",
            CardAttribute.PlayerDefense, -2,
            true,
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Vitalidade Comprometida",
            "Seu HP máximo é reduzido.",
            CardAttribute.PlayerMaxHP, -20,
            true,
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Vulnerabilidade Crescente",
            "Você se torna mais fácil de ferir.",
            CardAttribute.EnemyActionPower, 8,
            true,
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Cura Reduzida",
            "Suas habilidades de recuperação enfraquecem.",
            CardAttribute.PlayerDefensiveActionPower, -12,
            true,
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
        
          advantages.Add(CreateSpecificSkillPowerOffer(
            "Chama Ardente",
            $"'{skillName}' se torna mais poderosa.",
            skillName,
            6, 
            true,
            obs.triggerType
        ));
        
        advantages.Add(CreateSpecificSkillManaCostOffer(
            "Fênix de Mana",
            $"'{skillName}' consome menos mana.",
            skillName,
            -12,
            true,
            obs.triggerType
        ));
        
        // 3. Aumenta PODER e reduz MANA da skill específica
        advantages.Add(CreateSpecificSkillFullOffer(
            "Ressurgência",
            $"'{skillName}' se equipara as demais.",
            skillName,
            6, 
            -12,  
            true,
            obs.triggerType
        ));
        
        advantages.Add(CreateSpecificSkillPowerOffer(
            "Competição",
            $"Concentre todo seu poder em '{skillName}'.",
            skillName,
            15, 
            true,
            obs.triggerType
        ));
        
        // DESVANTAGENS
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Fragilidade Persistente",
            "Seus problemas defensivos deixam marcas.",
            CardAttribute.PlayerDefense, -2,
            true,
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Vitalidade Comprometida",
            "Seu HP máximo é reduzido.",
            CardAttribute.PlayerMaxHP, -20,
            true,
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Vulnerabilidade Crescente",
            "Você se torna mais fácil de ferir.",
            CardAttribute.EnemyActionPower, 8,
            true,
            obs.triggerType,
            "HP crítico frequente"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Cura Reduzida",
            "Suas habilidades de recuperação enfraquecem.",
            CardAttribute.PlayerDefensiveActionPower, -12,
            true,
            obs.triggerType,
            "HP crítico frequente"
        ));
    }
    
    /// <summary>
    /// Sempre age por último - 4 vantagens + 4 desvantagens
    /// </summary>
    private static void GenerateAlwaysOutspedOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Resistência Superior",
            "Se não pode ser rápido, seja resistente.",
            CardAttribute.PlayerMaxHP, 15,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Defesa Impenetrável",
            "Fortifique-se enquanto espera sua vez.",
            CardAttribute.PlayerDefense, 2,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Contra-ataque Letal",
            "Transforme a desvantagem em oportunidade.",
            CardAttribute.PlayerSingleTargetActionPower, 8,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Vulnerabilidade Exposta",
            "Agir por último te deixa vulnerável.",
            CardAttribute.PlayerDefense, -2,
            true,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Lentidão Crítica",
            "Sua baixa velocidade afeta tudo.",
            CardAttribute.PlayerActionPower, -10,
            true,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Desvantagem Tática",
            "Perder a iniciativa tem consequências.",
            CardAttribute.EnemyMaxHP, 12,
            true,
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
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Domínio Tático",
            "Mais recursos para sua vantagem inicial.",
            CardAttribute.PlayerMaxMP, 10,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Iniciativa Letal",
            "Transforme velocidade em poder destrutivo.",
            CardAttribute.PlayerOffensiveActionPower, 8,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Aguçado",
            "Sua agilidade permite achar mais moedas!",
            CardAttribute.CoinsEarned, 15,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        // DESVANTAGENS - Compensação por ser muito forte
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Força Bruta Inimiga",
            "Inimigos compensam com puro poder.",
            CardAttribute.EnemyActionPower, 8,
            false,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Exaustão Rápida",
            "Agir primeiro drena sua energia.",
            CardAttribute.PlayerMaxMP, -15,
            true,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Contra-medidas Aprendidas",
            "Inimigos se preparam para sua velocidade.",
            CardAttribute.EnemyDefense, 2,
            false,
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
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Quebra-Couraças",
            "Perfure as defesas mais robustas.",
            CardAttribute.PlayerOffensiveActionPower, 8,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Perfuração Letal",
            "Seus ataques ignoram parte da defesa inimiga.",
            CardAttribute.PlayerSingleTargetActionPower, 8,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Atrito Defensivo",
            "Aumente suas defesas.",
            CardAttribute.PlayerDefense, 2,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Fúria Persistente",
            "Batalhas longas favorecem você.",
            CardAttribute.PlayerMaxHP, 15,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        // DESVANTAGENS - CORRIGIDAS (apenas uma mudança cada, valores 4-12)
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Fortaleza Impenetrável",
            "Inimigos endurecem suas defesas.",
            CardAttribute.EnemyMaxHP,
            12,
            false, // Afeta inimigos
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
    
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Muralha Crescente",
            "Defesas inimigas aumentam drasticamente.",
            CardAttribute.EnemyDefense,
            2,
            false, // Afeta inimigos
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
    
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Frustração Desgastante",
            "Sua incapacidade de penetrar defesas te esgota.",
            CardAttribute.PlayerMaxMP,
            -8,
            true, // Afeta jogador
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
    
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Impotência Ofensiva",
            "Seus ataques enfraquecem contra alvos resistentes.",
            CardAttribute.PlayerSingleTargetActionPower,
            -8,
            true, // Afeta jogador
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
        // VANTAGENS - Anti-speed (apenas uma mudança cada, valores 4-12)
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Antecipação",
            "Aprenda a se defender de adversários velozes.",
            CardAttribute.PlayerDefense, 
            2,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Contra-ataque Preciso",
            "Ataques mais fortes compensam falta de velocidade.",
            CardAttribute.PlayerOffensiveActionPower, 
            8,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Muralha Impenetrável",
            "Se não pode alcançá-los, seja impossível de ferir.",
            CardAttribute.PlayerMaxHP, 
            15,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        // DESVANTAGENS (apenas uma mudança cada, valores 4-12)
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Velocidade Cegante",
            "Inimigos ficam impossíveis de acompanhar.",
            CardAttribute.EnemyActionPower,
            10,
            false, // Afeta inimigos
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Ataques Implacáveis",
            "Inimigos rápidos te acertam com mais frequência.",
            CardAttribute.EnemyOffensiveActionPower,
            12,
            false, // Afeta inimigos
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Lentidão Crítica",
            "Sua baixa velocidade te prejudica ainda mais.",
            CardAttribute.PlayerActionPower,
            -8,
            true, // Afeta jogador
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Fragilidade Exposta",
            "Sua defesa enfraquece contra adversários velozes.",
            CardAttribute.PlayerDefense,
            -2,
            true, // Afeta jogador
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
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Destruição em Massa",
            "Habilidades de área ganham poder devastador.",
            CardAttribute.PlayerAOEActionPower, 
            12,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Tempestade de Lâminas",
            "Todos seus ataques ficam mais fortes contra grupos.",
            CardAttribute.PlayerActionPower, 
            8,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Resistência de Multidão",
            "Fortaleça-se para enfrentar números superiores.",
            CardAttribute.PlayerMaxHP, 
            15,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        advantages.Add(NegotiationOffer.CreateDisadvantage(
            "Enfraquecer a Horda",
            "Reduza a vida de cada inimigo no grupo.",
            CardAttribute.EnemyMaxHP,
            -10,
            false, // Debuff nos inimigos = vantagem para o jogador
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        // DESVANTAGENS (apenas uma mudança cada, valores 4-12)
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Horda Implacável",
            "Mais inimigos surgem, mais fortes.",
            CardAttribute.EnemyMaxHP,
            15,
            false, // Afeta inimigos
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Sobrecarga Numérica",
            "Números esmagadores te enfraquecem.",
            CardAttribute.PlayerActionPower,
            -8,
            true, // Afeta jogador
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Fadiga de Combate",
            "Lutar contra múltiplos te esgota.",
            CardAttribute.PlayerMaxMP,
            -10,
            true, // Afeta jogador
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Área Insuficiente",
            "Suas habilidades de área enfraquecem.",
            CardAttribute.PlayerAOEActionPower,
            -8,
            true, // Afeta jogador
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
        // VANTAGENS - Sobrevivência inicial (apenas uma mudança cada, valores 4-12)
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Início Fortificado",
            "Comece com defesas massivas.",
            CardAttribute.PlayerMaxHP, 
            10,
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Escudo Inicial",
            "Resistência aprimorada desde o começo.",
            CardAttribute.PlayerDefense, 
            2,
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        advantages.Add(NegotiationOffer.CreateDisadvantage(
            "Enfraquecimento Inimigo",
            "Reduza o poder ofensivo dos adversários.",
            CardAttribute.EnemyOffensiveActionPower,
            -8,
            false, // Debuff nos inimigos = vantagem para o jogador
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Vitalidade Reforçada",
            "Aumente drasticamente seu HP máximo.",
            CardAttribute.PlayerMaxHP, 
            20,
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        // DESVANTAGENS (apenas uma mudança cada, valores 4-12)
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Assalto Inicial",
            "Inimigos investem tudo no início.",
            CardAttribute.EnemyActionPower,
            12,
            false, // Afeta inimigos
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Fragilidade Crítica",
            "Você começa mais vulnerável.",
            CardAttribute.PlayerMaxHP,
            -12,
            true, // Afeta jogador
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Abertura Fatal",
            "Primeiros turnos são ainda mais perigosos.",
            CardAttribute.EnemyOffensiveActionPower,
            6,
            false, // Afeta inimigos
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Defesas Comprometidas",
            "Sua defesa inicial é reduzida.",
            CardAttribute.PlayerDefense,
            -2,
            true, // Afeta jogador
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
        // VANTAGENS - Guerra de atrito (apenas uma mudança cada, valores 4-12)
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Resistência Prolongada",
            "Ganhe fôlego para batalhas longas.",
            CardAttribute.PlayerMaxMP, 
            10,
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Regeneração Persistente",
            "Suas habilidades defensivas melhoram.",
            CardAttribute.PlayerDefensiveActionPower, 
            6,
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        advantages.Add(NegotiationOffer.CreateDisadvantage(
            "Enfraquecimento Gradual",
            "Reduza o poder ofensivo dos inimigos ao longo do tempo.",
            CardAttribute.EnemyOffensiveActionPower,
            -8,
            false, // Debuff nos inimigos = vantagem para o jogador
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Fortificação Crescente",
            "Você fica mais forte com o tempo.",
            CardAttribute.PlayerDefense, 
            2,
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        // DESVANTAGENS (apenas uma mudança cada, valores 4-12)
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Guerra de Atrito",
            "Batalhas longas favorecem adversários.",
            CardAttribute.EnemyDefense,
            2,
            false, // Afeta inimigos
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Exaustão Inevitável",
            "Você se cansa em batalhas prolongadas.",
            CardAttribute.PlayerMaxMP,
            -10,
            true, // Afeta jogador
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Desgaste Acumulado",
            "Dano constante te enfraquece.",
            CardAttribute.PlayerDefense,
            -2,
            true, // Afeta jogador
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Fadiga Crescente",
            "Suas habilidades custam mais mana com o tempo.",
            CardAttribute.PlayerActionManaCost,
            10,
            true, // Afeta jogador
            obs.triggerType,
            "Morte tardia por atrito"
        ));
    }

    /// <summary>
    /// Morte por chip damage - 3 vantagens + 3 desvantagens
    /// </summary>
    private static void GenerateDeathByChipDamageOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS - Anti-chip damage (apenas uma mudança cada, valores 4-12)
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Pele de Aço",
            "Fortifique-se contra ataques menores.",
            CardAttribute.PlayerDefense, 
            2,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
    
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Regeneração Constante",
            "Curas mais eficazes.",
            CardAttribute.PlayerDefensiveActionPower, 
            12,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
    
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Vitalidade Robusta",
            "Mais HP para aguentar o atrito.",
            CardAttribute.PlayerMaxHP, 
            12,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
    
        // DESVANTAGENS (apenas uma mudança cada, valores 4-12)
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Mil Cortes",
            "Inimigos atacam com mais força e frequência.",
            CardAttribute.EnemyActionPower,
            10,
            false, // Afeta inimigos
            obs.triggerType,
            "Morte por dano acumulado"
        ));
    
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Armadura Desgastada",
            "Sua defesa se deteriora.",
            CardAttribute.PlayerDefense,
            -2,
            true, // Afeta jogador
            obs.triggerType,
            "Morte por dano acumulado"
        ));
    
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Feridas Acumuladas",
            "Você perde vitalidade máxima.",
            CardAttribute.PlayerMaxHP,
            -8,
            true, // Afeta jogador
            obs.triggerType,
            "Morte por dano acumulado"
        ));
    }
    
    /// <summary>
    /// Vulnerável a one-shots - 3 vantagens + 3 desvantagens
    /// </summary>
    private static void GenerateOneHitKOVulnerableOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        int biggestHit = obs.GetData<int>("biggestHit", 50);
        
        // VANTAGENS - Anti-burst (apenas uma mudança cada, valores 4-12)
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Fortaleza Viva",
            "Torne-se resistente a ataques devastadores.",
            CardAttribute.PlayerMaxHP, 
            10,
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Escudo Absoluto",
            "Defesa massiva contra golpes fortes.",
            CardAttribute.PlayerDefense, 
            2,
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        advantages.Add(NegotiationOffer.CreateDisadvantage(
            "Enfraquecimento Hostil",
            "Reduza o poder ofensivo dos adversários.",
            CardAttribute.EnemyOffensiveActionPower,
            -6,
            false, // Debuff nos inimigos = vantagem para o jogador
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        // DESVANTAGENS (apenas uma mudança cada, valores 4-12)
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Golpe Executivo",
            "Adversários aperfeiçoam ataques letais.",
            CardAttribute.EnemyActionPower,
            12,
            false, // Afeta inimigos
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Fragilidade Extrema",
            "Você fica ainda mais vulnerável.",
            CardAttribute.PlayerMaxHP,
            -12,
            true, // Afeta jogador
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Armadura de Papel",
            "Defesa drasticamente reduzida.",
            CardAttribute.PlayerDefense,
            -2,
            true, // Afeta jogador
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
    }
    
    /// <summary>
    /// HP baixo sem cura - 3 vantagens + 3 desvantagens
    /// </summary>
    private static void GenerateLowHealthNoCureOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS (apenas uma mudança cada, valores 4-12)
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Vitalidade Resiliente",
            "Compense falta de cura com mais resistência.",
            CardAttribute.PlayerMaxHP, 
            12,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Regeneração Natural",
            "Habilidades defensivas melhoradas.",
            CardAttribute.PlayerDefensiveActionPower, 
            12,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Defesa Compensatória",
            "Mais defesa para evitar dano.",
            CardAttribute.PlayerDefense, 
            2,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        // DESVANTAGENS (apenas uma mudança cada, valores 4-12)
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Feridas Abertas",
            "Feridas o tornam mais vulnerável.",
            CardAttribute.PlayerDefense,
            -2,
            true, // Afeta jogador
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Corpo Fragilizado",
            "Falta de cura te deixa fraco.",
            CardAttribute.PlayerMaxHP,
            -12,
            true, // Afeta jogador
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Hemorragia Perpétua",
            "Feridas não cicatrizam adequadamente.",
            CardAttribute.PlayerDefensiveActionPower,
            -10,
            true, // Afeta jogador
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
    }
    
    /// <summary>
    /// Problemas de mana - 3 vantagens + 3 desvantagens
    /// </summary>
    private static void GenerateManaIssuesOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS (apenas uma mudança cada, valores 4-12)
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Reservas Mágicas",
            "Amplie drasticamente suas reservas de mana.",
            CardAttribute.PlayerMaxMP, 
            10,
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Eficiência Arcana",
            "Reduza o custo de todas habilidades.",
            CardAttribute.PlayerActionManaCost, 
            -5,
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        advantages.Add(NegotiationOffer.CreateDisadvantage(
            "Esgotamento Inimigo",
            "Aumente o custo de mana das habilidades inimigas.",
            CardAttribute.EnemyActionManaCost,
            5,
            false, // Afeta inimigos = vantagem para o jogador
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        // DESVANTAGENS (apenas uma mudança cada, valores 4-12)
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Exaustão Mágica",
            "Habilidades drenam ainda mais energia.",
            CardAttribute.PlayerActionManaCost,
            5,
            true, // Afeta jogador
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Reservas Esgotadas",
            "Seu MP máximo diminui.",
            CardAttribute.PlayerMaxMP,
            -10,
            true, // Afeta jogador
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Dreno Arcano",
            "Suas reservas mágicas se deterioram.",
            CardAttribute.PlayerMaxMP,
            -12,
            true, // Afeta jogador
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
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Autossuficiência",
            "Troque dependência de itens por poder próprio.",
            CardAttribute.PlayerMaxHP, 15,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Força Interior",
            "Desenvolva poder independente.",
            CardAttribute.PlayerDefensiveActionPower, 8,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Recursos Internos",
            "Mais MP para habilidades próprias.",
            CardAttribute.PlayerMaxMP, 10,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Independência Total",
            "Stats permanentes aumentados.",
            CardAttribute.PlayerActionPower, 8,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Escassez Material",
            "Recursos se tornam ainda mais raros.",
            CardAttribute.CoinsEarned, -12,
            true,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Dependência Crítica",
            "Estratégia baseada em itens te enfraquece.",
            CardAttribute.PlayerDefense, 
            -2,
            true,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Fragilidade Sem Itens",
            "Sem consumíveis, você é fraco.",
            CardAttribute.EnemyMaxMP, 
            15,
            false,
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
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Tributagem",
            "Alguém com sua força merece recompensas.",
            CardAttribute.CoinsEarned, 15,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Domínio Total",
            "Você está além do desafio.",
            CardAttribute.PlayerOffensiveActionPower, 
            8,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        advantages.Add(NegotiationOffer.CreateDisadvantage(
            "Temido",
            "Seus inimigos te temem",
            CardAttribute.EnemyActionManaCost, -8,
            false,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        // DESVANTAGENS
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Lição Aprendida",
            "Inimigos estudam sua técnica.",
            CardAttribute.EnemyActionPower, 5,
            false,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Arrogância Fatal",
            "Confiança excessiva te enfraquece.",
            CardAttribute.PlayerActionManaCost, -5,
            true,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Desafio Amplificado",
            "Universo aumenta a dificuldade.",
            CardAttribute.EnemyMaxHP, 20,
            false,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Contra-medidas Desenvolvidas",
            "Inimigos se preparam melhor.",
            CardAttribute.EnemyMaxMP, 
            10,
            false,
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
        
        // VANTAGENS - Modificar o item específico que foi esgotado (valores 4-12)
        
        // 1. Aumenta quantidade/poder do item
        advantages.Add(CreateSpecificSkillPowerOffer(
            "Reservas Renovadas",
            $"'{itemName}' se torna mais eficaz quando usado.",
            itemName,
            8, 
            true,
            obs.triggerType
        ));
        
        
        // 3. Compensação geral por esgotar item
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Força Interior",
            "Desenvolva resistência própria sem depender de itens.",
            CardAttribute.PlayerMaxHP, 
            10,
            obs.triggerType,
            $"Item esgotado: {itemName}"
        ));
        
        // 1. Enfraquece o item específico
        disadvantages.Add(CreateSpecificSkillPowerOffer(
            "Degradação",
            $"'{itemName}' perde eficácia pelo uso excessivo.",
            itemName,
            -8,
            false,
            obs.triggerType
        ));
        
        // 2. Reduz recursos gerais
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Escassez Permanente",
            "Recursos ficam ainda mais raros.",
            CardAttribute.CoinsEarned,
            -15,
            true, // Afeta jogador
            obs.triggerType,
            $"Item esgotado: {itemName}"
        ));
        
        // 3. Enfraquece jogador por dependência
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Dependência Crítica",
            "Estratégia baseada em itens te enfraquece.",
            CardAttribute.PlayerDefense,
            -2,
            true, // Afeta jogador
            obs.triggerType,
            $"Item esgotado: {itemName}"
        ));
    }
    
    /// <summary>
    /// Sem skills defensivas - 3 vantagens + 3 desvantagens
    /// </summary>
    private static void GenerateNoDefensiveSkillsOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS (apenas uma mudança cada, valores 4-12)
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "A Melhor Defesa",
            "Puro foco ofensivo devastador.",
            CardAttribute.PlayerOffensiveActionPower, 
            10,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Ataque Esmagador",
            "Todos ataques ganham poder massivo.",
            CardAttribute.PlayerActionPower, 
            8,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Destruição Pura",
            "Especialize-se em dano single-target.",
            CardAttribute.PlayerSingleTargetActionPower, 
            10,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        // DESVANTAGENS (apenas uma mudança cada, valores 4-12)
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Vidro e Canhão",
            "Sem defesas, você é frágil.",
            CardAttribute.PlayerMaxHP,
            -20,
            true, // Afeta jogador
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Fragilidade Extrema",
            "Falta de defesa te enfraquece.",
            CardAttribute.PlayerDefense,
            -2,
            true, // Afeta jogador
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Ataques Implacáveis",
            "Inimigos exploram sua fragilidade com ataques mais fortes.",
            CardAttribute.EnemyOffensiveActionPower,
            12,
            false, // Afeta inimigos
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
        
        // VANTAGENS (apenas uma mudança cada, valores 4-12)
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Vingança Direcionada",
            $"Forças cósmicas concedem poder contra {bossName}.",
            CardAttribute.PlayerActionPower, 
            12,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Lições do Fracasso",
            "Aprenda com cada derrota.",
            CardAttribute.PlayerOffensiveActionPower, 
            12,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Resistência Forjada",
            "Fortifique-se através do sofrimento.",
            CardAttribute.PlayerMaxHP, 
            15,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Determinação Inabalável",
            "Cada derrota te fortalece.",
            CardAttribute.PlayerDefense, 
            2,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        // DESVANTAGENS (apenas uma mudança cada, valores 4-12)
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Sede por Sangue",
            $"{bossName} se fortalece com cada vitória.",
            CardAttribute.EnemyActionPower,
            20,
            false, // Afeta inimigos
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Trauma Profundo",
            "Mortes repetidas te enfraquecem mentalmente.",
            CardAttribute.PlayerMaxHP,
            -20,
            true, // Afeta jogador
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Desmoralização Total",
            "Derrotas consecutivas te quebram.",
            CardAttribute.PlayerActionPower,
            -20,
            true, // Afeta jogador
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Medo Paralisante",
            $"Pavor de {bossName} te enfraquece.",
            CardAttribute.PlayerDefense,
            -2,
            true, // Afeta jogador
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
        
        // VANTAGENS (apenas uma mudança cada, valores 4-12)
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Guardião de Recursos",
            "Disciplina financeira é recompensada.",
            CardAttribute.CoinsEarned, 
            12,
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Autossuficiência Premiada",
            "Não precisar de itens te fortalece.",
            CardAttribute.PlayerActionPower, 
            10,
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        advantages.Add(NegotiationOffer.CreateDisadvantage(
            "Descontos Generosos",
            "Comerciantes reduzem os preços para você.",
            CardAttribute.ShopPrices,
            -8,
            false, // Afeta preços (beneficia jogador)
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Poupança Recompensada",
            "Ganhe mais moedas por batalhas.",
            CardAttribute.CoinsEarned, 
            10,
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        // DESVANTAGENS (apenas uma mudança cada, valores 4-12)
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Inflação Galopante",
            "Comerciantes aumentam os preços.",
            CardAttribute.ShopPrices,
            12,
            false, // Afeta preços
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Escassez Forçada",
            "Recursos ficam mais caros.",
            CardAttribute.ShopPrices,
            14,
            false, // Afeta preços
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Isolamento Custoso",
            "Ignorar comércio tem consequências.",
            CardAttribute.CoinsEarned,
            -15,
            true, // Afeta jogador
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Oportunidades Perdidas",
            "Perder lojas te enfraquece economicamente.",
            CardAttribute.CoinsEarned,
            -12,
            true, // Afeta jogador
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
    }
    
    /// <summary>
    /// Vitória muito fácil - 3 vantagens + 3 desvantagens
    /// </summary>
    private static void GenerateBattleEasyVictoryOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        // VANTAGENS (apenas uma mudança cada, valores 4-12)
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Poder Crescente",
            "Canalize confiança em poder ofensivo.",
            CardAttribute.PlayerOffensiveActionPower, 
            8,
            obs.triggerType,
            "Vitória muito fácil"
        ));
    
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Superioridade Consolidada",
            "Você está além deste desafio.",
            CardAttribute.PlayerActionPower, 
            10,
            obs.triggerType,
            "Vitória muito fácil"
        ));
    
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Domínio Absoluto",
            "Vitórias fáceis provam sua força.",
            CardAttribute.PlayerSingleTargetActionPower, 
            10,
            obs.triggerType,
            "Vitória muito fácil"
        ));
    
        // DESVANTAGENS (apenas uma mudança cada, valores 4-12)
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Desafio Amplificado",
            "Universo aumenta a dificuldade para te testar.",
            CardAttribute.EnemyMaxHP,
            20,
            false, // Afeta inimigos
            obs.triggerType,
            "Vitória muito fácil"
        ));
    
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Contra-medidas Desenvolvidas",
            "Inimigos se preparam melhor.",
            CardAttribute.EnemyDefense,
            2,
            false, // Afeta inimigos
            obs.triggerType,
            "Vitória muito fácil"
        ));
    
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Arrogância Custosa",
            "Confiança excessiva te enfraquece.",
            CardAttribute.PlayerDefense,
            -2,
            true, // Afeta jogador
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
        
        // VANTAGENS (apenas uma mudança cada, valores 4-12)
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Maestria Arcana",
            "Domine magias poderosas com mais eficiência.",
            CardAttribute.PlayerMaxMP, 
            15,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Eficiência Aprimorada",
            "Reduza drasticamente custos de mana.",
            CardAttribute.PlayerActionManaCost, 
            -6,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Poder Justificado",
            "Skills ficam ainda mais fortes.",
            CardAttribute.PlayerOffensiveActionPower, 
            10,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        advantages.Add(NegotiationOffer.CreateDisadvantage(
            "Dreno Inimigo",
            "Aumente o custo de mana das habilidades inimigas.",
            CardAttribute.EnemyActionManaCost,
            5,
            false, // Afeta inimigos = vantagem para o jogador
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        // DESVANTAGENS (apenas uma mudança cada, valores 4-12)
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Fome Voraz",
            "Magias poderosas drenam ainda mais energia.",
            CardAttribute.PlayerActionManaCost,
            10,
            true, // Afeta jogador
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Esgotamento Rápido",
            "Você fica sem mana rapidamente.",
            CardAttribute.PlayerMaxMP,
            -15,
            true, // Afeta jogador
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Desperdício Extremo",
            "Custos exorbitantes te limitam.",
            CardAttribute.PlayerActionManaCost,
            15,
            true, // Afeta jogador
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
    }
    
    /// <summary>
    /// Sem dano em área - 2 vantagens + 2 desvantagens
    /// </summary>
    private static void GenerateNoAOEDamageOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        float avgEnemies = obs.GetData<float>("averageEnemyCount", 2f);
    
        // VANTAGENS (apenas uma mudança cada, valores 4-12)
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Assassino Preciso",
            "Foco absoluto em alvos únicos.",
            CardAttribute.PlayerSingleTargetActionPower, 
            12,
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
    
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Destruição Direcionada",
            "Ataques únicos ganham poder devastador.",
            CardAttribute.PlayerOffensiveActionPower, 
            10,
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
    
        // DESVANTAGENS (apenas uma mudança cada, valores 4-12)
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Sobrecarga de Alvos",
            "Múltiplos inimigos te sobrecarregam.",
            CardAttribute.EnemyMaxHP,
            10,
            false, // Afeta inimigos
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
    
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Vulnerabilidade Tática",
            "Falta de área te expõe.",
            CardAttribute.PlayerDefense,
            -2,
            true, // Afeta jogador
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
        
        // VANTAGENS (apenas uma mudança cada, valores 4-12)
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Tudo ou Nada",
            "Gastar tudo mostra comprometimento.",
            CardAttribute.CoinsEarned, 
            12,
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Investimento Sábio",
            "Seus gastos são recompensados.",
            CardAttribute.PlayerActionPower, 
            8,
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
        
        advantages.Add(NegotiationOffer.CreateDisadvantage(
            "Descontos Compensatórios",
            "Comerciantes reduzem preços por pena.",
            CardAttribute.ShopPrices,
            -8,
            false, // Afeta preços = vantagem para o jogador
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
        
        // DESVANTAGENS (apenas uma mudança cada, valores 4-12)
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Endividamento",
            "Gastos excessivos têm consequências.",
            CardAttribute.ShopPrices,
            12,
            false, // Afeta preços
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Pobreza Extrema",
            "Você ficou completamente sem recursos.",
            CardAttribute.CoinsEarned,
            -15,
            true, // Afeta jogador
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Crise Financeira",
            "Sua situação econômica piora drasticamente.",
            CardAttribute.CoinsEarned,
            -18,
            true, // Afeta jogador
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
    }

    /// <summary>
    /// Poucas moedas com lojas disponíveis - 3 vantagens + 3 desvantagens
    /// </summary>
    private static void GenerateLowCoinsUnvisitedShopsOffers(BehaviorObservation obs,
        List<NegotiationOffer> advantages, List<NegotiationOffer> disadvantages)
    {
        int currentCoins = obs.GetData<int>("currentCoins", 0);
        int shopsCount = obs.GetData<int>("unvisitedShopsCount", 1);
        
        // VANTAGENS (apenas uma mudança cada, valores 4-12)
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Filantropia Cósmica",
            "Forças do universo concedem moedas.",
            CardAttribute.CoinsEarned, 
            12,
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
        
        advantages.Add(NegotiationOffer.CreateAdvantage(
            "Misericórdia Divina",
            "Sua pobreza é aliviada.",
            CardAttribute.CoinsEarned, 
            10,
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
        
        advantages.Add(NegotiationOffer.CreateDisadvantage(
            "Desconto Compassivo",
            "Comerciantes têm pena de você.",
            CardAttribute.ShopPrices,
            -10,
            false, // Afeta preços = vantagem para o jogador
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
        
        // DESVANTAGENS (apenas uma mudança cada, valores 4-12)
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Círculo Vicioso",
            "Escassez de recursos persiste.",
            CardAttribute.ShopPrices,
            12,
            false, // Afeta preços
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Pobreza Permanente",
            "Você ganha menos moedas.",
            CardAttribute.CoinsEarned,
            -10,
            true, // Afeta jogador
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
        
        disadvantages.Add(NegotiationOffer.CreateDisadvantage(
            "Destituição Total",
            "Recursos escasseiam ainda mais.",
            CardAttribute.CoinsEarned,
            -12,
            true, // Afeta jogador
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