// Assets/Scripts/Difficulty_System/DefaultNegotiationOffers.cs (IMPROVED)

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gera ofertas de negociação padrão que estão sempre disponíveis
/// MELHORADO: Agora usa mais ofertas IntensityOnly e AttributeAndIntensity
/// </summary>
public class DefaultNegotiationOffers : MonoBehaviour
{
    public static DefaultNegotiationOffers Instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private bool enableDefaultOffers = true;
    [SerializeField] private int numberOfDefaultOffers = 5;
    
    [Header("Flexibility Settings")]
    [Tooltip("Probabilidade de gerar ofertas Fixed (0-1)")]
    [SerializeField] [Range(0f, 1f)] private float fixedOfferProbability = 0.3f;
    
    [Tooltip("Probabilidade de gerar ofertas IntensityOnly (0-1)")]
    [SerializeField] [Range(0f, 1f)] private float intensityOnlyProbability = 0.5f;
    // O resto será AttributeAndIntensity (0.2f se fixed=0.3 e intensity=0.5)
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public List<NegotiationOffer> GenerateDefaultOffers()
    {
        if (!enableDefaultOffers)
        {
            DebugLog("Ofertas padrão desabilitadas");
            return new List<NegotiationOffer>();
        }
        
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        string currentMap = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        DebugLog($"=== GERANDO OFERTAS PADRÃO PARA {currentMap} ===");
        
        // Pool de todas as ofertas possíveis
        List<NegotiationOffer> allPossibleOffers = new List<NegotiationOffer>();
        
        // 1. Ofertas de defesa (FLEXÍVEIS)
        allPossibleOffers.AddRange(GenerateDefensiveOffers(currentMap));
        
        // 2. Ofertas de mana (FLEXÍVEIS)
        allPossibleOffers.AddRange(GenerateManaOffers(currentMap));
        
        // 3. Ofertas de ações específicas (NOVO)
        allPossibleOffers.AddRange(GenerateActionSpecificOffers(currentMap));
        
        // 4. Ofertas relacionadas a boss
        allPossibleOffers.AddRange(GenerateBossOffers(currentMap));
        
        // 5. Ofertas relacionadas a inimigos comuns
        allPossibleOffers.AddRange(GenerateEnemyOffers(currentMap));
        
        // 6. Ofertas de economia (FLEXÍVEIS)
        allPossibleOffers.AddRange(GenerateEconomyOffers(currentMap));
        
        ShuffleList(allPossibleOffers);
        
        // Pega metade vantagens, metade desvantagens
        var advantages = allPossibleOffers.Where(o => o.isAdvantage).Take(numberOfDefaultOffers / 2 + 1).ToList();
        var disadvantages = allPossibleOffers.Where(o => !o.isAdvantage).Take(numberOfDefaultOffers / 2 + 1).ToList();
        
        offers.AddRange(advantages);
        offers.AddRange(disadvantages);
        
        DebugLog($"✅ Geradas {offers.Count} ofertas ({advantages.Count} vantagens, {disadvantages.Count} desvantagens)");
        
        return offers;
    }
    
    #region Geradores Específicos - MELHORADOS
    
    /// <summary>
    /// NOVO: Ofertas defensivas FLEXÍVEIS (IntensityOnly ou AttributeAndIntensity)
    /// </summary>
    private List<NegotiationOffer> GenerateDefensiveOffers(string mapName)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Fortificação (IntensityOnly ou AttributeAndIntensity)
        if (ShouldMakeFlexible())
        {
            // Oferece escolha entre HP e Defense
            offers.Add(new NegotiationOffer(
                "Fortificação Adaptativa",
                "Escolha como fortalecer suas defesas contra adversários.",
                true,
                CardAttribute.PlayerMaxHP, 18, // Base value para referência
                CardAttribute.EnemyActionPower, 15,
                BehaviorTriggerType.DefaultSessionOffer,
                "Oferta flexível de defesa"
            ));
        }
        else
        {
            // Fixed com valores balanceados
            offers.Add(new NegotiationOffer(
                "Fortificação Básica",
                "Aumente sua resistência contra ataques.",
                true,
                CardAttribute.PlayerDefense, 10,
                CardAttribute.EnemyActionPower, 12,
                BehaviorTriggerType.DefaultSessionOffer,
                "Oferta fixa de defesa"
            ));
        }
        
        // DESVANTAGEM
        offers.Add(new NegotiationOffer(
            "Armadura Frágil",
            "Sua proteção está comprometida, mas enfraquece inimigos.",
            false,
            CardAttribute.PlayerDefense, -10,
            CardAttribute.EnemyMaxHP, -18,
            BehaviorTriggerType.DefaultSessionOffer,
            "Desvantagem de defesa"
        ));
        
        return offers;
    }
    
    /// <summary>
    /// NOVO: Ofertas de mana FLEXÍVEIS
    /// </summary>
    private List<NegotiationOffer> GenerateManaOffers(string mapName)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Reservas de mana (AttributeAndIntensity)
        if (ShouldMakeFlexible())
        {
            offers.Add(new NegotiationOffer(
                "Reservas Arcanas",
                "Escolha entre aumentar seu MP ou reduzir custos de mana.",
                true,
                CardAttribute.PlayerMaxMP, 15,
                CardAttribute.EnemyMaxMP, 12,
                BehaviorTriggerType.DefaultSessionOffer,
                "Oferta flexível de mana"
            ));
        }
        else
        {
            offers.Add(new NegotiationOffer(
                "Reservas de Mana",
                "Expanda suas reservas de mana.",
                true,
                CardAttribute.PlayerMaxMP, 15,
                CardAttribute.EnemyMaxMP, 12,
                BehaviorTriggerType.DefaultSessionOffer,
                "Oferta fixa de mana"
            ));
        }
        
        // DESVANTAGEM
        offers.Add(new NegotiationOffer(
            "Fadiga Mágica",
            "Suas reservas de mana estão esgotadas.",
            false,
            CardAttribute.PlayerMaxMP, -12,
            CardAttribute.EnemyActionManaCost, -6,
            BehaviorTriggerType.DefaultSessionOffer,
            "Desvantagem de mana"
        ));
        
        return offers;
    }
    
    /// <summary>
    /// NOVO: Ofertas específicas para tipos de ações
    /// </summary>
    private List<NegotiationOffer> GenerateActionSpecificOffers(string mapName)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Especialização em ataques únicos
        offers.Add(new NegotiationOffer(
            "Foco Letal",
            "Seus ataques contra alvos únicos se tornam devastadores.",
            true,
            CardAttribute.PlayerSingleTargetActionPower, 20,
            CardAttribute.EnemyDefense, 12,
            BehaviorTriggerType.DefaultSessionOffer,
            "Buff single-target"
        ));
        
        // VANTAGEM: Especialização em AOE
        offers.Add(new NegotiationOffer(
            "Destruição em Massa",
            "Seus ataques em área ganham poder adicional.",
            true,
            CardAttribute.PlayerAOEActionPower, 15,
            CardAttribute.EnemyMaxHP, 18,
            BehaviorTriggerType.DefaultSessionOffer,
            "Buff AOE"
        ));
        
        // VANTAGEM: Melhor cura
        offers.Add(new NegotiationOffer(
            "Cura Aprimorada",
            "Suas habilidades de cura e proteção são mais eficazes.",
            true,
            CardAttribute.PlayerDefensiveActionPower, 18,
            CardAttribute.EnemyOffensiveActionPower, 12,
            BehaviorTriggerType.DefaultSessionOffer,
            "Buff healing"
        ));
        
        // DESVANTAGEM: Ataques fracos
        offers.Add(new NegotiationOffer(
            "Força Enfraquecida",
            "Seus ataques perdem potência.",
            false,
            CardAttribute.PlayerOffensiveActionPower, -15,
            CardAttribute.EnemyMaxHP, -20,
            BehaviorTriggerType.DefaultSessionOffer,
            "Debuff ataques"
        ));
        
        return offers;
    }
    
    private List<NegotiationOffer> GenerateBossOffers(string mapName)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        bool hasBoss = FindObjectOfType<BossNode>() != null || mapName.ToLower().Contains("boss");
        if (!hasBoss) return offers;
        
        string bossName = GetBossNameFromMap(mapName);
        
        // VANTAGEM: Enfraquecimento do Boss (valores balanceados)
        offers.Add(new NegotiationOffer(
            "Enfraquecimento do Chefe",
            $"Você ganha força contra o boss {bossName}, mas ele se defende melhor.",
            true,
            CardAttribute.PlayerActionPower, 18,
            CardAttribute.EnemyDefense, 15,
            BehaviorTriggerType.DefaultSessionOffer,
            $"Boss: {bossName}"
        ));
        
        // VANTAGEM: Armadura quebrada do boss
        offers.Add(new NegotiationOffer(
            "Armadura Quebrada",
            $"O boss {bossName} tem sua defesa reduzida.",
            true,
            CardAttribute.PlayerActionPower, 22,
            CardAttribute.EnemyDefense, 18,
            BehaviorTriggerType.DefaultSessionOffer,
            $"Boss: {bossName}"
        ));
        
        // DESVANTAGEM: Boss mais forte (valores balanceados)
        offers.Add(new NegotiationOffer(
            "Fúria do Chefe",
            $"O boss {bossName} se enfurece e ganha poder.",
            false,
            CardAttribute.PlayerMaxHP, -18,
            CardAttribute.EnemyActionPower, 25,
            BehaviorTriggerType.DefaultSessionOffer,
            $"Boss: {bossName}"
        ));
        
        // VANTAGEM: Dreno de mana do boss
        offers.Add(new NegotiationOffer(
            "Dreno Arcano",
            $"O boss {bossName} perde parte de sua mana.",
            true,
            CardAttribute.PlayerMaxMP, 12,
            CardAttribute.EnemyMaxMP, 18,
            BehaviorTriggerType.DefaultSessionOffer,
            $"Boss: {bossName}"
        ));
        
        return offers;
    }
    
    private List<NegotiationOffer> GenerateEnemyOffers(string mapName)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        List<string> possibleEnemies = GetPossibleEnemiesFromMap(mapName);
        if (possibleEnemies.Count == 0) possibleEnemies.Add("Inimigo");
        
        string randomEnemy = possibleEnemies[Random.Range(0, possibleEnemies.Count)];
        
        // VANTAGEM: Debilitação (valores balanceados)
        offers.Add(new NegotiationOffer(
            "Debilitação Direcionada",
            $"Enfraquece os ataques de {randomEnemy}.",
            true,
            CardAttribute.PlayerDefense, 12,
            CardAttribute.EnemyActionPower, 10,
            BehaviorTriggerType.DefaultSessionOffer,
            $"Inimigo alvo: {randomEnemy}"
        ));
        
        // VANTAGEM: Lentidão (valores balanceados - Speed usa escala menor)
        offers.Add(new NegotiationOffer(
            "Lentidão Seletiva",
            $"{randomEnemy} se move mais devagar.",
            true,
            CardAttribute.PlayerSpeed, 2,
            CardAttribute.EnemySpeed, -3,
            BehaviorTriggerType.DefaultSessionOffer,
            $"Inimigo alvo: {randomEnemy}"
        ));
        
        // DESVANTAGEM: Inimigo mais forte (valores balanceados)
        offers.Add(new NegotiationOffer(
            "Treinamento Hostil",
            $"{randomEnemy} se torna mais letal.",
            false,
            CardAttribute.PlayerActionPower, -12,
            CardAttribute.EnemyActionPower, 18,
            BehaviorTriggerType.DefaultSessionOffer,
            $"Inimigo alvo: {randomEnemy}"
        ));
        
        return offers;
    }
    
    /// <summary>
    /// NOVO: Ofertas de economia FLEXÍVEIS
    /// </summary>
    private List<NegotiationOffer> GenerateEconomyOffers(string mapName)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Mais moedas (IntensityOnly)
        if (ShouldMakeFlexible())
        {
            offers.Add(new NegotiationOffer(
                "Fortuna Crescente",
                "Escolha quanto deseja ampliar seus ganhos de moedas.",
                true,
                CardAttribute.CoinsEarned, 20,
                CardAttribute.ShopPrices, 15,
                BehaviorTriggerType.DefaultSessionOffer,
                "Oferta flexível de moedas"
            ));
        }
        else
        {
            offers.Add(new NegotiationOffer(
                "Bolsos Profundos",
                "Ganhe mais moedas em batalhas.",
                true,
                CardAttribute.CoinsEarned, 18,
                CardAttribute.ShopPrices, 12,
                BehaviorTriggerType.DefaultSessionOffer,
                "Oferta fixa de moedas"
            ));
        }
        
        // VANTAGEM: Preços reduzidos
        offers.Add(new NegotiationOffer(
            "Desconto Cósmico",
            "Itens custam menos nas lojas.",
            true,
            CardAttribute.ShopPrices, -15,
            CardAttribute.EnemyMaxHP, 18,
            BehaviorTriggerType.DefaultSessionOffer,
            "Desconto na loja"
        ));
        
        // DESVANTAGEM: Menos moedas (valores balanceados)
        offers.Add(new NegotiationOffer(
            "Pobreza Forçada",
            "Ganhe menos moedas, mas inimigos também enfraquecem.",
            false,
            CardAttribute.CoinsEarned, -12,
            CardAttribute.EnemyMaxHP, -18,
            BehaviorTriggerType.DefaultSessionOffer,
            "Desvantagem econômica"
        ));
        
        return offers;
    }
    
    #endregion
    
    #region Helper Methods
    
    /// <summary>
    /// NOVO: Decide se deve criar uma oferta flexível baseado nas probabilidades
    /// </summary>
    private bool ShouldMakeFlexible()
    {
        float roll = Random.value;
        return roll > fixedOfferProbability;
    }
    
    private string GetBossNameFromMap(string mapName)
    {
        BossNode bossNode = FindObjectOfType<BossNode>();
        if (bossNode != null)
        {
            MapNode mapNode = bossNode.GetComponent<MapNode>();
            if (mapNode != null && mapNode.eventType is BattleEventSO battleEvent)
            {
                if (battleEvent.enemies != null && battleEvent.enemies.Count > 0)
                {
                    return battleEvent.enemies[0].characterName;
                }
            }
        }
        
        // Fallback
        if (mapName.ToLower().Contains("1")) return "Mawron";
        else if (mapName.ToLower().Contains("2")) return "Waldemor";
        else return "Fentho";
    }
    
    private List<string> GetPossibleEnemiesFromMap(string mapName)
    {
        List<string> enemies = new List<string>();
        MapNode[] nodes = FindObjectsOfType<MapNode>();
        
        foreach (var node in nodes)
        {
            if (node.eventType is BattleEventSO battleEvent)
            {
                if (battleEvent.enemies != null)
                {
                    foreach (var enemy in battleEvent.enemies)
                    {
                        if (enemy != null && !enemies.Contains(enemy.characterName))
                        {
                            enemies.Add(enemy.characterName);
                        }
                    }
                }
            }
        }
        
        if (enemies.Count == 0)
        {
            enemies.Add("Patrulheiro");
            enemies.Add("Druida");
            enemies.Add("Guarda");
        }
        
        return enemies;
    }
    
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    private void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"<color=green>[DefaultOffers]</color> {message}");
        }
    }
    
    #endregion
    
    #region Public API
    
    public void ResetDefaultOffers()
    {
        DebugLog("Ofertas padrão resetadas para próxima sessão");
    }
    
    public void SetEnabled(bool enabled)
    {
        enableDefaultOffers = enabled;
        DebugLog($"Ofertas padrão {(enabled ? "ativadas" : "desativadas")}");
    }
    
    #endregion
}