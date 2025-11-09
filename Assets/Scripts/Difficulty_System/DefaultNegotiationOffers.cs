using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Gera ofertas padrão para negociação
public class DefaultNegotiationOffers : MonoBehaviour
{
    public static DefaultNegotiationOffers Instance { get; private set; }
    
    [SerializeField] private bool enableDefaultOffers = true;
    [SerializeField] private int numberOfDefaultOffers = 6;
    
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
            return new List<NegotiationOffer>();
        
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        string currentMap = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        List<NegotiationOffer> allPossibleOffers = new List<NegotiationOffer>();
        
        allPossibleOffers.AddRange(GenerateDefensiveOffers(currentMap));
        allPossibleOffers.AddRange(GenerateManaOffers(currentMap));
        allPossibleOffers.AddRange(GenerateActionSpecificOffers(currentMap));
        allPossibleOffers.AddRange(GenerateBossOffers(currentMap));
        allPossibleOffers.AddRange(GenerateEnemyOffers(currentMap));
        allPossibleOffers.AddRange(GenerateEconomyOffers(currentMap));
        
        ShuffleList(allPossibleOffers);
        
        var advantages = allPossibleOffers.Where(o => o.isAdvantage).Take(numberOfDefaultOffers / 2 + 1).ToList();
        var disadvantages = allPossibleOffers.Where(o => !o.isAdvantage).Take(numberOfDefaultOffers / 2 + 1).ToList();
        
        offers.AddRange(advantages);
        offers.AddRange(disadvantages);
        
        return offers;
    }
    
    #region Geradores Específicos
    
    private List<NegotiationOffer> GenerateDefensiveOffers(string mapName)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        offers.Add(NegotiationOffer.CreateAdvantage(
            "Fortificação",
            "Aumente sua resistência.",
            CardAttribute.PlayerMaxHP,
            12,
            BehaviorTriggerType.DefaultSessionOffer,
            "Oferta de defesa"
        ));
        
        offers.Add(NegotiationOffer.CreateAdvantage(
            "Armadura Reforçada",
            "Fortaleça suas defesas.",
            CardAttribute.PlayerDefense,
            2,
            BehaviorTriggerType.DefaultSessionOffer,
            "Oferta de defesa"
        ));
        
        offers.Add(NegotiationOffer.CreateDisadvantage(
            "Armadura Frágil",
            "Sua proteção está comprometida.",
            CardAttribute.PlayerDefense,
            -2,
            true,
            BehaviorTriggerType.DefaultSessionOffer,
            "Desvantagem de defesa"
        ));
        
        offers.Add(NegotiationOffer.CreateDisadvantage(
            "Adversários Resistentes",
            "Inimigos ficam mais difíceis de derrotar.",
            CardAttribute.EnemyMaxHP,
            12,
            false, 
            BehaviorTriggerType.DefaultSessionOffer,
            "Desvantagem de defesa"
        ));
        
        return offers;
    }
    
    private List<NegotiationOffer> GenerateManaOffers(string mapName)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        offers.Add(NegotiationOffer.CreateAdvantage(
            "Reservas Arcanas",
            "Amplie suas reservas de mana.",
            CardAttribute.PlayerMaxMP,
            12,
            BehaviorTriggerType.DefaultSessionOffer,
            "Oferta de mana"
        ));
        
        offers.Add(NegotiationOffer.CreateAdvantage(
            "Eficiência Mágica",
            "Suas habilidades custam menos mana.",
            CardAttribute.PlayerActionManaCost,
            -4,
            BehaviorTriggerType.DefaultSessionOffer,
            "Oferta de mana"
        ));
        
        offers.Add(NegotiationOffer.CreateDisadvantage(
            "Fadiga Mágica",
            "Suas reservas de mana diminuem.",
            CardAttribute.PlayerMaxMP,
            -8,
            true,
            BehaviorTriggerType.DefaultSessionOffer,
            "Desvantagem de mana"
        ));
        
        offers.Add(NegotiationOffer.CreateDisadvantage(
            "Energia Hostil",
            "Inimigos ganham mais recursos mágicos.",
            CardAttribute.EnemyMaxMP,
            10,
            false,
            BehaviorTriggerType.DefaultSessionOffer,
            "Desvantagem de mana"
        ));
        
        return offers;
    }
    
    private List<NegotiationOffer> GenerateActionSpecificOffers(string mapName)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        offers.Add(NegotiationOffer.CreateAdvantage(
            "Foco Letal",
            "Ataques contra alvos únicos devastam.",
            CardAttribute.PlayerSingleTargetActionPower,
            6,
            BehaviorTriggerType.DefaultSessionOffer,
            "Buff single-target"
        ));
        
        offers.Add(NegotiationOffer.CreateAdvantage(
            "Destruição em Massa",
            "Ataques em área ganham poder.",
            CardAttribute.PlayerAOEActionPower,
            10,
            BehaviorTriggerType.DefaultSessionOffer,
            "Buff AOE"
        ));
        
        offers.Add(NegotiationOffer.CreateDisadvantage(
            "Força Enfraquecida",
            "Seus ataques perdem potência.",
            CardAttribute.PlayerOffensiveActionPower,
            -8,
            true,
            BehaviorTriggerType.DefaultSessionOffer,
            "Debuff ataques"
        ));
        
        offers.Add(NegotiationOffer.CreateDisadvantage(
            "Fúria Inimiga",
            "Inimigos ganham poder destrutivo.",
            CardAttribute.EnemyActionPower,
            10,
            false,
            BehaviorTriggerType.DefaultSessionOffer,
            "Debuff ataques"
        ));
        
        return offers;
    }
    
    private List<NegotiationOffer> GenerateBossOffers(string mapName)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        string bossName = GetBossNameFromMap(mapName);
        
        offers.Add(NegotiationOffer.CreateAdvantage(
            "Matador de Chefes",
            $"Ganhe força contra chefes poderosos.",
            CardAttribute.PlayerActionPower,
            6,
            BehaviorTriggerType.DefaultSessionOffer,
            $"Boss: {bossName}"
        ));
        
        offers.Add(NegotiationOffer.CreateDisadvantage(
            "Fúria do Boss",
            $"Os chefes se enfurecem.",
            CardAttribute.EnemyActionPower,
            15,
            false, 
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
        
        offers.Add(NegotiationOffer.CreateAdvantage(
            "Proteção Aprimorada",
            $"Fortifique-se contra {randomEnemy}.",
            CardAttribute.PlayerDefense,
            2,
            BehaviorTriggerType.DefaultSessionOffer,
            $"Inimigo alvo: {randomEnemy}"
        ));
        
        return offers;
    }
    
    private List<NegotiationOffer> GenerateEconomyOffers(string mapName)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        offers.Add(NegotiationOffer.CreateAdvantage(
            "Fortuna Crescente",
            "Ganhe mais moedas em batalhas.",
            CardAttribute.CoinsEarned,
            10,
            BehaviorTriggerType.DefaultSessionOffer,
            "Oferta de moedas"
        ));
        
        offers.Add(NegotiationOffer.CreateAdvantage(
            "Desconto Cósmico",
            "Itens custam menos nas lojas.",
            CardAttribute.ShopPrices,
            -8,
            BehaviorTriggerType.DefaultSessionOffer,
            "Desconto na loja"
        ));
        
        offers.Add(NegotiationOffer.CreateDisadvantage(
            "Pobreza Forçada",
            "Ganhe menos moedas.",
            CardAttribute.CoinsEarned,
            -8,
            true,
            BehaviorTriggerType.DefaultSessionOffer,
            "Desvantagem econômica"
        ));
        
        offers.Add(NegotiationOffer.CreateDisadvantage(
            "Inflação Galopante",
            "Itens custam mais nas lojas.",
            CardAttribute.ShopPrices,
            12,
            true,
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
    
    #endregion
    
    #region Public API
    
    public void ResetDefaultOffers()
    {
        // Reset para próxima sessão
    }
    
    public void SetEnabled(bool enabled)
    {
        enableDefaultOffers = enabled;
    }
    
    #endregion
}