// Assets/Scripts/Difficult_System/ObservationInterpreter.cs

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Interpreta observações comportamentais e gera ofertas de negociação contextuais
/// </summary>
public static class ObservationInterpreter
{
    /// <summary>
    /// Interpreta uma observação e gera ofertas (vantagens e desvantagens)
    /// </summary>

    public static List<NegotiationOffer> InterpretObservation(BehaviorObservation observation)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        switch (observation.triggerType)
        {
            case BehaviorTriggerType.PlayerDeath:
                offers.AddRange(InterpretPlayerDeath(observation));
                break;
                
            case BehaviorTriggerType.SingleSkillCarry:
                offers.AddRange(InterpretSingleSkillCarry(observation));
                break;
                
            case BehaviorTriggerType.FrequentLowHP:
                offers.AddRange(InterpretFrequentLowHPFlexible(observation));
                break;
                
            case BehaviorTriggerType.WeakSkillIgnored:
                offers.AddRange(InterpretWeakSkillIgnored(observation));
                break;
                
            case BehaviorTriggerType.AlwaysOutsped:
                offers.AddRange(InterpretAlwaysOutsped(observation));
                break;
                
            case BehaviorTriggerType.AlwaysFirstTurn:
                offers.AddRange(InterpretAlwaysFirstTurn(observation));
                break;
                
            case BehaviorTriggerType.StrugglesAgainstTanks:
                offers.AddRange(InterpretStrugglesAgainstTanks(observation));
                break;
                
            case BehaviorTriggerType.StrugglesAgainstFast:
                offers.AddRange(InterpretStrugglesAgainstFast(observation));
                break;
                
            case BehaviorTriggerType.StrugglesAgainstSwarms:
                offers.AddRange(InterpretStrugglesAgainstSwarms(observation));
                break;
                
            case BehaviorTriggerType.AlwaysDiesEarly:
                offers.AddRange(InterpretAlwaysDiesEarly(observation));
                break;
                
            case BehaviorTriggerType.AlwaysDiesLate:
                offers.AddRange(InterpretAlwaysDiesLate(observation));
                break;
                
            case BehaviorTriggerType.DeathByChipDamage:
                offers.AddRange(InterpretDeathByChipDamage(observation));
                break;
                
            case BehaviorTriggerType.OneHitKOVulnerable:
                offers.AddRange(InterpretOneHitKOVulnerable(observation));
                break;
                
            case BehaviorTriggerType.LowHealthNoCure:
                offers.AddRange(InterpretLowHealthNoCure(observation));
                break;
                
            case BehaviorTriggerType.AllSkillsUseMana:
            case BehaviorTriggerType.LowManaStreak:
            case BehaviorTriggerType.ZeroManaStreak:
                offers.AddRange(InterpretManaIssues(observation));
                break;
                
            case BehaviorTriggerType.ConsumableDependency:
            case BehaviorTriggerType.RanOutOfConsumables:
                offers.AddRange(InterpretConsumableIssues(observation));
                break;
                
            // NOVOS CASOS
            case BehaviorTriggerType.NoDamageReceived:
                offers.AddRange(InterpretNoDamageReceived(observation));
                break;
                
            case BehaviorTriggerType.ItemExhausted:
                offers.AddRange(InterpretItemExhausted(observation));
                break;
                
            case BehaviorTriggerType.NoDefensiveSkills:
                offers.AddRange(InterpretNoDefensiveSkills(observation));
                break;
                
            case BehaviorTriggerType.RepeatedBossDeath:
                offers.AddRange(InterpretRepeatedBossDeath(observation));
                break;
                
            case BehaviorTriggerType.ShopIgnored:
                offers.AddRange(InterpretShopIgnored(observation));
                break;
                
            case BehaviorTriggerType.BattleEasyVictory:
                offers.AddRange(InterpretEasyVictoryFlexible(observation));
                break;
                
            case BehaviorTriggerType.ExpensiveSkillsOnly:
                offers.AddRange(InterpretExpensiveSkillsOnly(observation));
                break;
                
            case BehaviorTriggerType.NoAOEDamage:
                offers.AddRange(InterpretNoAOEDamage(observation));
                break;
                
            case BehaviorTriggerType.BrokeAfterShopping:
                offers.AddRange(InterpretBrokeAfterShopping(observation));
                break;
                
            case BehaviorTriggerType.LowCoinsUnvisitedShops:
                offers.AddRange(InterpretLowCoinsUnvisitedShops(observation));
                break;
        }
        
        return offers;
    }
    
    #region Interpretation Methods
    
    private static List<NegotiationOffer> InterpretPlayerDeath(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        string killerEnemy = obs.GetData<string>("killerEnemy", "Inimigo");
        
        // VANTAGEM: Enfraquecer o inimigo que matou o jogador
        offers.Add(new NegotiationOffer(
            "Vingança Tardia",
            $"As forças cósmicas enfraquecem seus adversários após sua queda anterior.",
            true, // vantagem
            CardAttribute.PlayerMaxHP, 0, // sem buff pro jogador
            CardAttribute.EnemyMaxHP, -20, // nerf nos inimigos
            obs.triggerType,
            $"Morreu para: {killerEnemy}"
        ));
        
        // DESVANTAGEM: Inimigos ficam mais fortes
        offers.Add(new NegotiationOffer(
            "Sede de Sangue",
            $"Seus inimigos se fortalecem com a memória de sua derrota.",
            false, // desvantagem
            CardAttribute.PlayerDefense, -5, // debuff no jogador
            CardAttribute.EnemyActionPower, 10, // buff nos inimigos
            obs.triggerType,
            $"Morreu para: {killerEnemy}"
        ));
        
        return offers;
    }
    
    private static List<NegotiationOffer> InterpretSingleSkillCarry(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        string skillName = obs.GetData<string>("skillName", "Habilidade");
        
        // VANTAGEM: Aumentar poder das ações
        offers.Add(new NegotiationOffer(
            "Mestre de Uma Arte",
            $"Especialize-se ainda mais em sua técnica favorita: '{skillName}'.",
            true,
            CardAttribute.PlayerActionPower, 15,
            CardAttribute.EnemyDefense, 10,
            obs.triggerType,
            $"Skill dominante: {skillName}"
        ));
        
        // DESVANTAGEM: Custo de mana aumenta
        offers.Add(new NegotiationOffer(
            "Dependência Custosa",
            $"Sua dependência de '{skillName}' cobra seu preço.",
            false,
            CardAttribute.PlayerActionManaCost, 5,
            CardAttribute.EnemySpeed, -5,
            obs.triggerType,
            $"Skill dominante: {skillName}"
        ));
        
        return offers;
    }
    
    /// <summary>
    /// VERSÃO FLEXÍVEL: Oferece escolha de atributo defensivo
    /// </summary>
    private static List<NegotiationOffer> InterpretFrequentLowHPFlexible(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();

        // VANTAGEM: Escolha de stat defensivo
        var flexibleOffer = new NegotiationOffer(
            "Fortificação Adaptativa",
            "Escolha como fortalecer suas defesas.",
            true, // vantagem
            CardAttribute.PlayerMaxHP, 30,
            CardAttribute.EnemyMaxHP, 20,
            obs.triggerType,
            "HP crítico frequente"
        );
        offers.Add(flexibleOffer);

        // DESVANTAGEM: Para balancear
        offers.Add(new NegotiationOffer(
            "Fragilidade Persistente",
            "Seus problemas defensivos deixam marcas permanentes.",
            false, // desvantagem
            CardAttribute.PlayerDefense, -8,
            CardAttribute.EnemyDefense, -5,
            obs.triggerType,
            "HP crítico frequente"
        ));

        return offers;
    }
    
    private static List<NegotiationOffer> InterpretWeakSkillIgnored(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        string skillName = obs.GetData<string>("skillName", "Habilidade");
        
        // VANTAGEM: Reduzir custo de mana de todas as ações
        offers.Add(new NegotiationOffer(
            "Eficiência Mágica",
            $"Aprenda a usar todas suas habilidades com menos esforço.",
            true,
            CardAttribute.PlayerActionManaCost, -3,
            CardAttribute.EnemyActionPower, 8,
            obs.triggerType,
            $"Skill ignorada: {skillName}"
        ));
        
        // DESVANTAGEM: Poder de ações reduzido
        offers.Add(new NegotiationOffer(
            "Arsenal Limitado",
            $"Sua falta de versatilidade enfraquece seu arsenal.",
            false,
            CardAttribute.PlayerActionPower, -10,
            CardAttribute.EnemyMaxHP, -10,
            obs.triggerType,
            $"Skill ignorada: {skillName}"
        ));
        
        return offers;
    }
    
    private static List<NegotiationOffer> InterpretAlwaysOutsped(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Aumentar velocidade do jogador
        offers.Add(new NegotiationOffer(
            "Reflexos Aprimorados",
            "As forças cósmicas aceleram seus movimentos.",
            true,
            CardAttribute.PlayerSpeed, 3,
            CardAttribute.EnemySpeed, 2,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        // DESVANTAGEM: Inimigos ainda mais rápidos
        offers.Add(new NegotiationOffer(
            "Corrida Perdida",
            "Seus adversários aprendem a se mover ainda mais rápido.",
            false,
            CardAttribute.PlayerMaxMP, -10,
            CardAttribute.EnemySpeed, 4,
            obs.triggerType,
            "Sempre age por último"
        ));
        
        return offers;
    }
    
    private static List<NegotiationOffer> InterpretAlwaysFirstTurn(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Aumentar ainda mais a velocidade (domínio total)
        offers.Add(new NegotiationOffer(
            "Domínio Temporal",
            "Sua velocidade esmagadora esmaga qualquer resistência.",
            true,
            CardAttribute.PlayerSpeed, 2,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        // DESVANTAGEM: Inimigos compensam com poder
        offers.Add(new NegotiationOffer(
            "Força Bruta",
            "Seus inimigos trocam velocidade por puro poder destrutivo.",
            false,
            CardAttribute.PlayerDefense, -5,
            CardAttribute.EnemyActionPower, 20,
            obs.triggerType,
            "Sempre age primeiro"
        ));
        
        return offers;
    }
    
    private static List<NegotiationOffer> InterpretStrugglesAgainstTanks(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Aumentar dano contra tanques
        offers.Add(new NegotiationOffer(
            "Quebra-Couraças",
            "Suas ações perfuram as defesas mais robustas.",
            true,
            CardAttribute.PlayerActionPower, 20,
            CardAttribute.EnemyDefense, 15,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        // DESVANTAGEM: Inimigos ainda mais resistentes
        offers.Add(new NegotiationOffer(
            "Fortaleza Impenetrável",
            "Seus adversários endurecem suas defesas.",
            false,
            CardAttribute.PlayerActionPower, -8,
            CardAttribute.EnemyMaxHP, 30,
            obs.triggerType,
            "Dificuldade vs Tanks"
        ));
        
        return offers;
    }
    
    private static List<NegotiationOffer> InterpretStrugglesAgainstFast(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Aumentar velocidade ou defesa
        offers.Add(new NegotiationOffer(
            "Reação Instantânea",
            "Aprenda a lidar com adversários velozes.",
            true,
            CardAttribute.PlayerSpeed, 2,
            CardAttribute.EnemyMaxHP, 15,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        // DESVANTAGEM: Inimigos ainda mais rápidos
        offers.Add(new NegotiationOffer(
            "Velocidade Cegante",
            "Seus inimigos se tornam impossíveis de acompanhar.",
            false,
            CardAttribute.PlayerDefense, -7,
            CardAttribute.EnemySpeed, 3,
            obs.triggerType,
            "Dificuldade vs Rápidos"
        ));
        
        return offers;
    }
    
    private static List<NegotiationOffer> InterpretStrugglesAgainstSwarms(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        int enemyCount = obs.GetData<int>("enemyCount", 3);
        
        // VANTAGEM: Buff de área
        offers.Add(new NegotiationOffer(
            "Destruição em Massa",
            "Suas habilidades ganham poder devastador contra grupos.",
            true,
            CardAttribute.PlayerActionPower, 15,
            CardAttribute.EnemyActionPower, 10,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        // DESVANTAGEM: Mais inimigos mais fortes
        offers.Add(new NegotiationOffer(
            "Horda Implacável",
            "Mais inimigos surgem, mais fortes do que antes.",
            false,
            CardAttribute.PlayerMaxHP, -15,
            CardAttribute.EnemyMaxHP, 25,
            obs.triggerType,
            $"Dificuldade vs {enemyCount} inimigos"
        ));
        
        return offers;
    }
    
    private static List<NegotiationOffer> InterpretAlwaysDiesEarly(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Muito HP inicial
        offers.Add(new NegotiationOffer(
            "Início Fortificado",
            "Comece a batalha com defesas reforçadas.",
            true,
            CardAttribute.PlayerMaxHP, 40,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        // DESVANTAGEM: Inimigos começam mais fortes
        offers.Add(new NegotiationOffer(
            "Assalto Inicial",
            "Seus inimigos investem tudo no início da batalha.",
            false,
            CardAttribute.PlayerDefense, -10,
            CardAttribute.EnemyActionPower, 25,
            obs.triggerType,
            "Morte precoce frequente"
        ));
        
        return offers;
    }
    
    private static List<NegotiationOffer> InterpretAlwaysDiesLate(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Regeneração ou resistência
        offers.Add(new NegotiationOffer(
            "Resistência Prolongada",
            "Ganhe fôlego para batalhas longas.",
            true,
            CardAttribute.PlayerMaxMP, 20,
            CardAttribute.EnemyMaxHP, 20,
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        // DESVANTAGEM: Inimigos se fortalecem com tempo
        offers.Add(new NegotiationOffer(
            "Guerra de Atrito",
            "Batalhas longas favorecem seus adversários.",
            false,
            CardAttribute.PlayerActionManaCost, 4,
            CardAttribute.EnemyDefense, 12,
            obs.triggerType,
            "Morte tardia por atrito"
        ));
        
        return offers;
    }
    
    private static List<NegotiationOffer> InterpretDeathByChipDamage(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Mais defesa
        offers.Add(new NegotiationOffer(
            "Pele de Aço",
            "Fortifique-se contra ataques menores e constantes.",
            true,
            CardAttribute.PlayerDefense, 12,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
        
        // DESVANTAGEM: Mais ataques rápidos
        offers.Add(new NegotiationOffer(
            "Mil Cortes",
            "Seus inimigos aprendem a atacar com mais frequência.",
            false,
            CardAttribute.PlayerSpeed, -2,
            CardAttribute.EnemySpeed, 3,
            obs.triggerType,
            "Morte por dano acumulado"
        ));
        
        return offers;
    }
    
    private static List<NegotiationOffer> InterpretOneHitKOVulnerable(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        int biggestHit = obs.GetData<int>("biggestHit", 50);
        
        // VANTAGEM: Muito mais HP e defesa
        offers.Add(new NegotiationOffer(
            "Fortaleza Viva",
            "Torne-se resistente a ataques devastadores.",
            true,
            CardAttribute.PlayerMaxHP, 50,
            CardAttribute.EnemyMaxHP, 30,
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        // DESVANTAGEM: Ataques inimigos mais fortes
        offers.Add(new NegotiationOffer(
            "Golpe Executivo",
            "Seus adversários aperfeiçoam seus ataques letais.",
            false,
            CardAttribute.PlayerDefense, -12,
            CardAttribute.EnemyActionPower, 30,
            obs.triggerType,
            $"Vulnerável a one-shots ({biggestHit} dano)"
        ));
        
        return offers;
    }
    
    private static List<NegotiationOffer> InterpretLowHealthNoCure(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Mais HP
        offers.Add(new NegotiationOffer(
            "Vitalidade Resiliente",
            "Compense a falta de cura com mais resistência.",
            true,
            CardAttribute.PlayerMaxHP, 35,
            CardAttribute.EnemyDefense, 10,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        // DESVANTAGEM: Menos defesa
        offers.Add(new NegotiationOffer(
            "Feridas Abertas",
            "Suas feridas o tornam mais vulnerável.",
            false,
            CardAttribute.PlayerDefense, -10,
            CardAttribute.EnemyMaxHP, -15,
            obs.triggerType,
            "HP baixo sem itens de cura"
        ));
        
        return offers;
    }
    
    private static List<NegotiationOffer> InterpretManaIssues(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Mais MP ou custo reduzido
        offers.Add(new NegotiationOffer(
            "Reservas Mágicas",
            "Amplie suas reservas de mana.",
            true,
            CardAttribute.PlayerMaxMP, 25,
            CardAttribute.EnemyMaxMP, 15,
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        // DESVANTAGEM: Custo aumentado
        offers.Add(new NegotiationOffer(
            "Exaustão Mágica",
            "Suas habilidades drenam ainda mais energia.",
            false,
            CardAttribute.PlayerActionManaCost, 5,
            CardAttribute.EnemyActionManaCost, -3,
            obs.triggerType,
            "Problemas de mana constantes"
        ));
        
        return offers;
    }
    
    private static List<NegotiationOffer> InterpretConsumableIssues(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Stats permanentes em troca de consumíveis
        offers.Add(new NegotiationOffer(
            "Autossuficiência",
            "Troque dependência de itens por poder próprio.",
            true,
            CardAttribute.PlayerMaxHP, 25,
            CardAttribute.EnemyMaxHP, 20,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        // DESVANTAGEM: Moedas reduzidas (menos acesso a itens)
        offers.Add(new NegotiationOffer(
            "Escassez Material",
            "Recursos se tornam ainda mais raros.",
            false,
            CardAttribute.CoinsEarned, -10,
            CardAttribute.EnemyDefense, -8,
            obs.triggerType,
            "Dependência de consumíveis"
        ));
        
        return offers;
    }
    
    #endregion
    
    // Assets/Scripts/Difficulty_System/ObservationInterpreter.cs (CONTINUAÇÃO)

    #region Interpretation Methods - PARTE 2

    /// <summary>
    /// Detecta quando o jogador não recebeu dano (vitória perfeita)
    /// </summary>
    private static List<NegotiationOffer> InterpretNoDamageReceived(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        string enemyName = obs.GetData<string>("randomEnemy", "Inimigo");
        
        // VANTAGEM: Você está muito forte - menos recursos
        offers.Add(new NegotiationOffer(
            "Confiança Absoluta",
            "Sua força é inquestionável. Você não precisa de tantos recursos.",
            true, // vantagem
            CardAttribute.PlayerActionPower, 25,
            CardAttribute.CoinsEarned, -15,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        // DESVANTAGEM: Inimigos aprendem com a derrota
        offers.Add(new NegotiationOffer(
            "Lição Aprendida",
            "Seus inimigos estudam sua técnica e se fortalecem.",
            false, // desvantagem
            CardAttribute.PlayerMaxHP, -20,
            CardAttribute.EnemyDefense, 20,
            obs.triggerType,
            $"Vitória perfeita vs {enemyName}"
        ));
        
        return offers;
    }

    /// <summary>
    /// Detecta quando um item foi totalmente esgotado
    /// </summary>
    private static List<NegotiationOffer> InterpretItemExhausted(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        string itemName = obs.GetData<string>("exhaustedItem", "Item");
        
        // VANTAGEM: Compensar com stats permanentes
        offers.Add(new NegotiationOffer(
            "Força Interior",
            $"Ao esgotar '{itemName}', você desenvolve resistência própria.",
            true,
            CardAttribute.PlayerMaxHP, 30,
            CardAttribute.EnemyMaxHP, 25,
            obs.triggerType,
            $"Item esgotado: {itemName}"
        ));
        
        // DESVANTAGEM: Dependência de consumíveis
        offers.Add(new NegotiationOffer(
            "Dependência Crítica",
            "Sua estratégia baseada em itens o enfraquece sem eles.",
            false,
            CardAttribute.PlayerDefense, -12,
            CardAttribute.EnemyActionPower, -10,
            obs.triggerType,
            $"Item esgotado: {itemName}"
        ));
        
        return offers;
    }

    /// <summary>
    /// Detecta quando o jogador não tem skills defensivas
    /// </summary>
    private static List<NegotiationOffer> InterpretNoDefensiveSkills(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Especialização ofensiva
        offers.Add(new NegotiationOffer(
            "A Melhor Defesa",
            "Puro foco ofensivo. Ataque é a sua defesa.",
            true,
            CardAttribute.PlayerActionPower, 30,
            CardAttribute.EnemyActionPower, 20,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        // DESVANTAGEM: Fragilidade compensada
        offers.Add(new NegotiationOffer(
            "Vidro e Canhão",
            "Sem defesas, você precisa de mais vida para sobreviver.",
            false,
            CardAttribute.PlayerMaxHP, 40,
            CardAttribute.EnemySpeed, 3,
            obs.triggerType,
            "Build sem skills defensivas"
        ));
        
        return offers;
    }

    /// <summary>
    /// Detecta morte repetida no mesmo boss
    /// </summary>
    private static List<NegotiationOffer> InterpretRepeatedBossDeath(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        string bossName = obs.GetData<string>("bossName", "Boss");
        
        // VANTAGEM: Buff específico contra esse boss
        offers.Add(new NegotiationOffer(
            "Vingança Direcionada",
            $"As forças cósmicas concedem poder contra {bossName}.",
            true,
            CardAttribute.PlayerActionPower, 35,
            CardAttribute.EnemyMaxHP, 30, // Todos os inimigos ficam mais fortes
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        // DESVANTAGEM: Boss fica ainda mais forte
        offers.Add(new NegotiationOffer(
            "Sede por Sangue",
            $"{bossName} se fortalece com cada vitória sobre você.",
            false,
            CardAttribute.PlayerMaxMP, -15,
            CardAttribute.EnemyActionPower, 25,
            obs.triggerType,
            $"Mortes repetidas: {bossName}"
        ));
        
        return offers;
    }

    /// <summary>
    /// Detecta quando o jogador ignora a loja
    /// </summary>
    private static List<NegotiationOffer> InterpretShopIgnored(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        int playerCoins = obs.GetData<int>("playerCoins", 0);
        
        // VANTAGEM: Recompensa por economia
        offers.Add(new NegotiationOffer(
            "Guardião de Recursos",
            "Sua disciplina financeira é recompensada com poder.",
            true,
            CardAttribute.CoinsEarned, 20,
            CardAttribute.ShopPrices, 10,
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        // DESVANTAGEM: Preços sobem
        offers.Add(new NegotiationOffer(
            "Inflação Galopante",
            "Comerciantes aumentam os preços para compensar.",
            false,
            CardAttribute.ShopPrices, 25,
            CardAttribute.EnemyMaxHP, -20,
            obs.triggerType,
            $"Loja ignorada com {playerCoins} moedas"
        ));
        
        return offers;
    }
    
    /// <summary>
    /// VERSÃO FLEXÍVEL: Permite escolher qual stat ofensivo buffar
    /// </summary>
    private static List<NegotiationOffer> InterpretEasyVictoryFlexible(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();

        // VANTAGEM: Escolha de stat ofensivo
        var flexibleOffer = new NegotiationOffer(
            "Poder Crescente",
            "Canalize sua confiança em poder ofensivo.",
            true,
            CardAttribute.PlayerActionPower, 25,
            CardAttribute.EnemyActionPower, 15,
            obs.triggerType,
            "Vitória muito fácil"
        );
        offers.Add(flexibleOffer);

        // DESVANTAGEM: Para balancear
        offers.Add(new NegotiationOffer(
            "Desafio Amplificado",
            "O universo aumenta a dificuldade para te testar.",
            false,
            CardAttribute.PlayerMaxHP, -15,
            CardAttribute.EnemyMaxHP, 35,
            obs.triggerType,
            "Vitória muito fácil"
        ));

        return offers;
    }

    /// <summary>
    /// Detecta build com apenas skills caras
    /// </summary>
    private static List<NegotiationOffer> InterpretExpensiveSkillsOnly(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        float avgCost = obs.GetData<float>("averageManaCost", 20f);
        
        // VANTAGEM: Especialista em mana
        offers.Add(new NegotiationOffer(
            "Maestria Arcana",
            "Domine o uso de magias poderosas com mais eficiência.",
            true,
            CardAttribute.PlayerMaxMP, 35,
            CardAttribute.EnemyMaxMP, 20,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        // DESVANTAGEM: Custo ainda maior
        offers.Add(new NegotiationOffer(
            "Fome Voraz",
            "Suas magias poderosas drenam ainda mais energia.",
            false,
            CardAttribute.PlayerActionManaCost, 8,
            CardAttribute.EnemyActionManaCost, 5,
            obs.triggerType,
            $"Custo médio: {avgCost:F0} MP"
        ));
        
        return offers;
    }

    /// <summary>
    /// Detecta falta de dano em área
    /// </summary>
    private static List<NegotiationOffer> InterpretNoAOEDamage(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        float avgEnemies = obs.GetData<float>("averageEnemyCount", 2f);
        
        // VANTAGEM: Single-target specialist
        offers.Add(new NegotiationOffer(
            "Assassino Preciso",
            "Foco absoluto em alvos únicos resulta em dano devastador.",
            true,
            CardAttribute.PlayerActionPower, 40,
            CardAttribute.EnemyDefense, 15,
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
        
        // DESVANTAGEM: Mais inimigos para compensar
        offers.Add(new NegotiationOffer(
            "Números Crescentes",
            "Sem AOE, você enfrenta hordas maiores.",
            false,
            CardAttribute.PlayerMaxHP, -15,
            CardAttribute.EnemyMaxHP, -15, // Inimigos individualmente mais fracos, mas em maior número
            obs.triggerType,
            $"Sem AOE vs {avgEnemies:F0} inimigos"
        ));
        
        return offers;
    }

    /// <summary>
    /// Detecta quando o jogador fica sem dinheiro após compras
    /// </summary>
    private static List<NegotiationOffer> InterpretBrokeAfterShopping(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        int coinsLeft = obs.GetData<int>("coinsLeft", 0);
        
        // VANTAGEM: Investimento sábio
        offers.Add(new NegotiationOffer(
            "Tudo ou Nada",
            "Gastar tudo mostra comprometimento. Seja recompensado.",
            true,
            CardAttribute.CoinsEarned, 30,
            CardAttribute.EnemyActionPower, 12,
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
        
        // DESVANTAGEM: Dívida cósmica
        offers.Add(new NegotiationOffer(
            "Endividamento",
            "Gastos excessivos têm consequências.",
            false,
            CardAttribute.ShopPrices, 20,
            CardAttribute.EnemyMaxHP, -15,
            obs.triggerType,
            $"Restaram apenas {coinsLeft} moedas"
        ));
        
        return offers;
    }

    /// <summary>
    /// Detecta poucas moedas com lojas disponíveis
    /// </summary>
    private static List<NegotiationOffer> InterpretLowCoinsUnvisitedShops(BehaviorObservation obs)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        int currentCoins = obs.GetData<int>("currentCoins", 0);
        int shopsCount = obs.GetData<int>("unvisitedShopsCount", 1);
        
        // VANTAGEM: Boost de economia
        offers.Add(new NegotiationOffer(
            "Filantropia Cósmica",
            "As forças do universo concedem moedas para aliviar sua pobreza.",
            true,
            CardAttribute.CoinsEarned, 25,
            CardAttribute.EnemySpeed, 2,
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
        
        // DESVANTAGEM: Pobreza persistente
        offers.Add(new NegotiationOffer(
            "Círculo Vicioso",
            "Sua escassez de recursos persiste.",
            false,
            CardAttribute.PlayerMaxMP, -10,
            CardAttribute.ShopPrices, -20, // Preços menores para ajudar
            obs.triggerType,
            $"{currentCoins} moedas, {shopsCount} lojas disponíveis"
        ));
        
        return offers;
    }

    #endregion
}