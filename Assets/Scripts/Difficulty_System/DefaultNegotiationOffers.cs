// Assets/Scripts/Difficulty_System/DefaultNegotiationOffers.cs (CORRIGIDO - 1 efeito por oferta)

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gera ofertas padrão CORRETAS
/// Cada oferta tem APENAS UM efeito
/// Valores balanceados: 4, 8, 12
/// </summary>
public class DefaultNegotiationOffers : MonoBehaviour
{
    public static DefaultNegotiationOffers Instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private bool enableDefaultOffers = true;
    [SerializeField] private int numberOfDefaultOffers = 6;
    
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
        
        allPossibleOffers.AddRange(GenerateDefensiveOffers(currentMap));
        allPossibleOffers.AddRange(GenerateManaOffers(currentMap));
        allPossibleOffers.AddRange(GenerateActionSpecificOffers(currentMap));
        allPossibleOffers.AddRange(GenerateBossOffers(currentMap));
        allPossibleOffers.AddRange(GenerateEnemyOffers(currentMap));
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
    
    #region Geradores Específicos - CORRIGIDOS
    
    private List<NegotiationOffer> GenerateDefensiveOffers(string mapName)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Mais HP
        offers.Add(NegotiationOffer.CreateAdvantage(
            "Fortificação",
            "Aumente sua resistência.",
            CardAttribute.PlayerMaxHP,
            12,
            BehaviorTriggerType.DefaultSessionOffer,
            "Oferta de defesa"
        ));
        
        // VANTAGEM: Mais Defesa
        offers.Add(NegotiationOffer.CreateAdvantage(
            "Armadura Reforçada",
            "Fortaleça suas defesas.",
            CardAttribute.PlayerDefense,
            8,
            BehaviorTriggerType.DefaultSessionOffer,
            "Oferta de defesa"
        ));
        
        // DESVANTAGEM: Menos Defesa
        offers.Add(NegotiationOffer.CreateDisadvantage(
            "Armadura Frágil",
            "Sua proteção está comprometida.",
            CardAttribute.PlayerDefense,
            -6,
            true, // Afeta jogador
            BehaviorTriggerType.DefaultSessionOffer,
            "Desvantagem de defesa"
        ));
        
        // DESVANTAGEM: Inimigos ganham HP
        offers.Add(NegotiationOffer.CreateDisadvantage(
            "Adversários Resistentes",
            "Inimigos ficam mais difíceis de derrotar.",
            CardAttribute.EnemyMaxHP,
            12,
            false, // Afeta inimigos
            BehaviorTriggerType.DefaultSessionOffer,
            "Desvantagem de defesa"
        ));
        
        return offers;
    }
    
    private List<NegotiationOffer> GenerateManaOffers(string mapName)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Mais MP
        offers.Add(NegotiationOffer.CreateAdvantage(
            "Reservas Arcanas",
            "Amplie suas reservas de mana.",
            CardAttribute.PlayerMaxMP,
            12,
            BehaviorTriggerType.DefaultSessionOffer,
            "Oferta de mana"
        ));
        
        // VANTAGEM: Custo reduzido
        offers.Add(NegotiationOffer.CreateAdvantage(
            "Eficiência Mágica",
            "Suas habilidades custam menos mana.",
            CardAttribute.PlayerActionManaCost,
            -4,
            BehaviorTriggerType.DefaultSessionOffer,
            "Oferta de mana"
        ));
        
        // DESVANTAGEM: Menos MP
        offers.Add(NegotiationOffer.CreateDisadvantage(
            "Fadiga Mágica",
            "Suas reservas de mana diminuem.",
            CardAttribute.PlayerMaxMP,
            -8,
            true, // Afeta jogador
            BehaviorTriggerType.DefaultSessionOffer,
            "Desvantagem de mana"
        ));
        
        // DESVANTAGEM: Inimigos ganham MP
        offers.Add(NegotiationOffer.CreateDisadvantage(
            "Energia Hostil",
            "Inimigos ganham mais recursos mágicos.",
            CardAttribute.EnemyMaxMP,
            10,
            false, // Afeta inimigos
            BehaviorTriggerType.DefaultSessionOffer,
            "Desvantagem de mana"
        ));
        
        return offers;
    }
    
    private List<NegotiationOffer> GenerateActionSpecificOffers(string mapName)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Ataques únicos mais fortes
        offers.Add(NegotiationOffer.CreateAdvantage(
            "Foco Letal",
            "Ataques contra alvos únicos devastam.",
            CardAttribute.PlayerSingleTargetActionPower,
            12,
            BehaviorTriggerType.DefaultSessionOffer,
            "Buff single-target"
        ));
        
        // VANTAGEM: AOE mais forte
        offers.Add(NegotiationOffer.CreateAdvantage(
            "Destruição em Massa",
            "Ataques em área ganham poder.",
            CardAttribute.PlayerAOEActionPower,
            10,
            BehaviorTriggerType.DefaultSessionOffer,
            "Buff AOE"
        ));
        
        // DESVANTAGEM: Ataques mais fracos
        offers.Add(NegotiationOffer.CreateDisadvantage(
            "Força Enfraquecida",
            "Seus ataques perdem potência.",
            CardAttribute.PlayerOffensiveActionPower,
            -8,
            true, // Afeta jogador
            BehaviorTriggerType.DefaultSessionOffer,
            "Debuff ataques"
        ));
        
        // DESVANTAGEM: Inimigos atacam mais forte
        offers.Add(NegotiationOffer.CreateDisadvantage(
            "Fúria Inimiga",
            "Inimigos ganham poder destrutivo.",
            CardAttribute.EnemyActionPower,
            10,
            false, // Afeta inimigos
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
        
        // VANTAGEM: Mais poder
        offers.Add(NegotiationOffer.CreateAdvantage(
            "Matador de Chefes",
            $"Ganhe força contra {bossName}.",
            CardAttribute.PlayerActionPower,
            12,
            BehaviorTriggerType.DefaultSessionOffer,
            $"Boss: {bossName}"
        ));
        
        // DESVANTAGEM: Boss mais forte
        offers.Add(NegotiationOffer.CreateDisadvantage(
            "Fúria do Boss",
            $"{bossName} se enfurece.",
            CardAttribute.EnemyActionPower,
            15,
            false, // Afeta inimigos
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
        
        // VANTAGEM: Mais defesa
        offers.Add(NegotiationOffer.CreateAdvantage(
            "Proteção Aprimorada",
            $"Fortifique-se contra {randomEnemy}.",
            CardAttribute.PlayerDefense,
            8,
            BehaviorTriggerType.DefaultSessionOffer,
            $"Inimigo alvo: {randomEnemy}"
        ));
        
        // DESVANTAGEM: Inimigos mais rápidos
        offers.Add(NegotiationOffer.CreateDisadvantage(
            "Agilidade Hostil",
            $"{randomEnemy} fica mais rápido.",
            CardAttribute.EnemySpeed,
            2,
            false, // Afeta inimigos
            BehaviorTriggerType.DefaultSessionOffer,
            $"Inimigo alvo: {randomEnemy}"
        ));
        
        return offers;
    }
    
    private List<NegotiationOffer> GenerateEconomyOffers(string mapName)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Mais moedas
        offers.Add(NegotiationOffer.CreateAdvantage(
            "Fortuna Crescente",
            "Ganhe mais moedas em batalhas.",
            CardAttribute.CoinsEarned,
            10,
            BehaviorTriggerType.DefaultSessionOffer,
            "Oferta de moedas"
        ));
        
        // VANTAGEM: Descontos
        offers.Add(NegotiationOffer.CreateAdvantage(
            "Desconto Cósmico",
            "Itens custam menos nas lojas.",
            CardAttribute.ShopPrices,
            -8,
            BehaviorTriggerType.DefaultSessionOffer,
            "Desconto na loja"
        ));
        
        // DESVANTAGEM: Menos moedas
        offers.Add(NegotiationOffer.CreateDisadvantage(
            "Pobreza Forçada",
            "Ganhe menos moedas.",
            CardAttribute.CoinsEarned,
            -8,
            true, // Afeta jogador
            BehaviorTriggerType.DefaultSessionOffer,
            "Desvantagem econômica"
        ));
        
        // DESVANTAGEM: Preços maiores
        offers.Add(NegotiationOffer.CreateDisadvantage(
            "Inflação Galopante",
            "Itens custam mais nas lojas.",
            CardAttribute.ShopPrices,
            12,
            true, // Afeta jogador
            BehaviorTriggerType.DefaultSessionOffer,
            "Desvantagem econômica"
        ));
        
        return offers;
    }
    
    #endregion
    
    #region Helper Methods
    
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
        
        if (mapName.ToLower().Contains("1")) return "Mawron";
        else if (mapName.ToLower().Contains("2")) return "Valdemor";
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