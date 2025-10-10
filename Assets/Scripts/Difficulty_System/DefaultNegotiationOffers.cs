// Assets/Scripts/Difficulty_System/DefaultNegotiationOffers.cs

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gera ofertas de negociação padrão que estão sempre disponíveis
/// Estas ofertas são contextuais ao mapa/nível atual
/// </summary>
public class DefaultNegotiationOffers : MonoBehaviour
{
    public static DefaultNegotiationOffers Instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private bool enableDefaultOffers = true;
    [SerializeField] private int numberOfDefaultOffers = 5; // Quantas ofertas base gerar
    
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
    
    /// <summary>
    /// Gera ofertas padrão baseadas no contexto do mapa atual
    /// </summary>
    public List<NegotiationOffer> GenerateDefaultOffers()
    {
        if (!enableDefaultOffers)
        {
            DebugLog("Ofertas padrão desabilitadas");
            return new List<NegotiationOffer>();
        }
        
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // Pega informações do mapa atual se disponível
        string currentMap = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        DebugLog($"=== GERANDO OFERTAS PADRÃO PARA {currentMap} ===");
        
        // Pool de todas as ofertas possíveis
        List<NegotiationOffer> allPossibleOffers = new List<NegotiationOffer>();
        
        // 1. Ofertas de defesa do jogador
        allPossibleOffers.AddRange(GeneratePlayerDefenseOffers(currentMap));
        
        // 2. Ofertas de MP do jogador
        allPossibleOffers.AddRange(GeneratePlayerManaOffers(currentMap));
        
        // 3. Ofertas relacionadas a boss (se houver)
        allPossibleOffers.AddRange(GenerateBossOffers(currentMap));
        
        // 4. Ofertas relacionadas a inimigos comuns
        allPossibleOffers.AddRange(GenerateEnemyOffers(currentMap));
        
        // 5. Ofertas de economia
        allPossibleOffers.AddRange(GenerateEconomyOffers(currentMap));
        
        // Embaralha e seleciona as melhores
        ShuffleList(allPossibleOffers);
        
        // Pega metade vantagens, metade desvantagens
        var advantages = allPossibleOffers.Where(o => o.isAdvantage).Take(numberOfDefaultOffers / 2 + 1).ToList();
        var disadvantages = allPossibleOffers.Where(o => !o.isAdvantage).Take(numberOfDefaultOffers / 2 + 1).ToList();
        
        offers.AddRange(advantages);
        offers.AddRange(disadvantages);
        
        DebugLog($"✅ Geradas {offers.Count} ofertas padrão ({advantages.Count} vantagens, {disadvantages.Count} desvantagens)");
        
        return offers;
    }
    
    #region Geradores Específicos
    
    private List<NegotiationOffer> GeneratePlayerDefenseOffers(string mapName)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Mais defesa
        offers.Add(new NegotiationOffer(
            "Fortificação Básica",
            "Aumente sua resistência contra ataques.",
            true, // vantagem
            CardAttribute.PlayerDefense, 8,
            CardAttribute.EnemyActionPower, 10,
            BehaviorTriggerType.DefaultSessionOffer,
            "Oferta padrão de defesa"
        ));
        
        // DESVANTAGEM: Menos defesa
        offers.Add(new NegotiationOffer(
            "Armadura Frágil",
            "Sua proteção está comprometida.",
            false, // desvantagem
            CardAttribute.PlayerDefense, -10,
            CardAttribute.EnemyMaxHP, -20,
            BehaviorTriggerType.DefaultSessionOffer,
            "Oferta padrão de defesa"
        ));
        
        return offers;
    }
    
    private List<NegotiationOffer> GeneratePlayerManaOffers(string mapName)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Mais MP
        offers.Add(new NegotiationOffer(
            "Reservas Arcanas",
            "Expanda suas reservas de mana.",
            true,
            CardAttribute.PlayerMaxMP, 15,
            CardAttribute.EnemyMaxMP, 10,
            BehaviorTriggerType.DefaultSessionOffer,
            "Oferta padrão de mana"
        ));
        
        // DESVANTAGEM: Menos MP
        offers.Add(new NegotiationOffer(
            "Fadiga Mágica",
            "Suas reservas de mana estão esgotadas.",
            false,
            CardAttribute.PlayerMaxMP, -15,
            CardAttribute.EnemyActionManaCost, -5,
            BehaviorTriggerType.DefaultSessionOffer,
            "Oferta padrão de mana"
        ));
        
        return offers;
    }
    
    private List<NegotiationOffer> GenerateBossOffers(string mapName)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // Verifica se há boss no mapa
        bool hasBoss = FindObjectOfType<BossNode>() != null || mapName.ToLower().Contains("boss");
        
        if (!hasBoss)
        {
            DebugLog("Nenhum boss detectado - pulando ofertas de boss");
            return offers;
        }
        
        string bossName = GetBossNameFromMap(mapName);
        
        // VANTAGEM: Enfraquece boss em velocidade
        offers.Add(new NegotiationOffer(
            "Lentidão do Chefe",
            $"O boss {bossName} perde velocidade.",
            true,
            CardAttribute.PlayerSpeed, 2,
            CardAttribute.EnemySpeed, 3, // Boss também fica mais rápido mas menos que o jogador ganha
            BehaviorTriggerType.DefaultSessionOffer,
            $"Boss: {bossName}"
        ));
        
        // VANTAGEM: Enfraquece boss em defesa
        offers.Add(new NegotiationOffer(
            "Armadura Quebrada",
            $"O boss {bossName} tem sua defesa reduzida.",
            true,
            CardAttribute.PlayerActionPower, 15,
            CardAttribute.EnemyDefense, 15, // Boss fica mais defensivo mas jogador ganha mais ataque
            BehaviorTriggerType.DefaultSessionOffer,
            $"Boss: {bossName}"
        ));
        
        // DESVANTAGEM: Boss fica mais forte
        offers.Add(new NegotiationOffer(
            "Fúria do Chefe",
            $"O boss {bossName} se enfurece e ganha poder.",
            false,
            CardAttribute.PlayerMaxHP, -15,
            CardAttribute.EnemyActionPower, 25,
            BehaviorTriggerType.DefaultSessionOffer,
            $"Boss: {bossName}"
        ));
        
        // VANTAGEM: Reduz MP do boss
        offers.Add(new NegotiationOffer(
            "Dreno Arcano",
            $"O boss {bossName} perde parte de sua mana.",
            true,
            CardAttribute.PlayerMaxMP, 10,
            CardAttribute.EnemyMaxMP, 15,
            BehaviorTriggerType.DefaultSessionOffer,
            $"Boss: {bossName}"
        ));
        
        return offers;
    }
    
    private List<NegotiationOffer> GenerateEnemyOffers(string mapName)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // Pega lista de inimigos possíveis do mapa atual
        List<string> possibleEnemies = GetPossibleEnemiesFromMap(mapName);
        
        if (possibleEnemies.Count == 0)
        {
            DebugLog("Nenhum inimigo comum detectado - usando ofertas genéricas");
            possibleEnemies.Add("Inimigo");
        }
        
        string randomEnemy = possibleEnemies[Random.Range(0, possibleEnemies.Count)];
        
        // VANTAGEM: Enfraquece inimigo específico em ataque
        offers.Add(new NegotiationOffer(
            "Debilitação Direcionada",
            $"Enfraquece os ataques de {randomEnemy}.",
            true,
            CardAttribute.PlayerDefense, 10,
            CardAttribute.EnemyActionPower, 8,
            BehaviorTriggerType.DefaultSessionOffer,
            $"Inimigo alvo: {randomEnemy}"
        ));
        
        // VANTAGEM: Reduz velocidade de inimigo
        offers.Add(new NegotiationOffer(
            "Lentidão Seletiva",
            $"{randomEnemy} se move mais devagar.",
            true,
            CardAttribute.PlayerSpeed, 1,
            CardAttribute.EnemySpeed, -2,
            BehaviorTriggerType.DefaultSessionOffer,
            $"Inimigo alvo: {randomEnemy}"
        ));
        
        // DESVANTAGEM: Inimigo mais forte
        offers.Add(new NegotiationOffer(
            "Treinamento Hostil",
            $"{randomEnemy} se torna mais letal.",
            false,
            CardAttribute.PlayerActionPower, -8,
            CardAttribute.EnemyActionPower, 15,
            BehaviorTriggerType.DefaultSessionOffer,
            $"Inimigo alvo: {randomEnemy}"
        ));
        
        return offers;
    }
    
    private List<NegotiationOffer> GenerateEconomyOffers(string mapName)
    {
        List<NegotiationOffer> offers = new List<NegotiationOffer>();
        
        // VANTAGEM: Mais moedas
        offers.Add(new NegotiationOffer(
            "Bolsos Profundos",
            "Ganhe mais moedas em batalhas.",
            true,
            CardAttribute.CoinsEarned, 15,
            CardAttribute.ShopPrices, 10,
            BehaviorTriggerType.DefaultSessionOffer,
            "Oferta econômica"
        ));
        
        // VANTAGEM: Preços reduzidos
        offers.Add(new NegotiationOffer(
            "Desconto Cósmico",
            "Itens custam menos nas lojas.",
            true,
            CardAttribute.ShopPrices, -15,
            CardAttribute.EnemyMaxHP, 15,
            BehaviorTriggerType.DefaultSessionOffer,
            "Oferta econômica"
        ));
        
        // DESVANTAGEM: Menos moedas
        offers.Add(new NegotiationOffer(
            "Pobreza Forçada",
            "Ganhe menos moedas, mas inimigos também.",
            false,
            CardAttribute.CoinsEarned, -10,
            CardAttribute.EnemyMaxHP, -15,
            BehaviorTriggerType.DefaultSessionOffer,
            "Oferta econômica"
        ));
        
        return offers;
    }
    
    #endregion
    
    #region Helper Methods
    
    private string GetBossNameFromMap(string mapName)
    {
        // Tenta encontrar o boss node
        BossNode bossNode = FindObjectOfType<BossNode>();
    
        if (bossNode != null)
        {
            // O BossNode está no mesmo GameObject que o MapNode
            MapNode mapNode = bossNode.GetComponent<MapNode>();
        
            if (mapNode != null && mapNode.eventType is BattleEventSO battleEvent)
            {
                if (battleEvent.enemies != null && battleEvent.enemies.Count > 0)
                {
                    return battleEvent.enemies[0].characterName;
                }
            }
        }
    
        // Fallback: gera nome baseado no mapa
        if (mapName.ToLower().Contains("1"))
            return "Mawron";
        else if (mapName.ToLower().Contains("2"))
            return "Waldemor";
        else
            return "Fentho";
    }
    
    private List<string> GetPossibleEnemiesFromMap(string mapName)
    {
        List<string> enemies = new List<string>();
        
        // Procura por todos os MapNodes com BattleEventSO
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
        
        // Se não encontrou nada, adiciona alguns genéricos
        if (enemies.Count == 0)
        {
            enemies.Add("Patrulheiro");
            enemies.Add("Druida");
            enemies.Add("Guarda");
        }
        
        DebugLog($"Inimigos possíveis no mapa: {string.Join(", ", enemies)}");
        
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
    
    /// <summary>
    /// Reseta ofertas padrão (chamado ao fim do jogo)
    /// </summary>
    public void ResetDefaultOffers()
    {
        DebugLog("Ofertas padrão resetadas para próxima sessão");
        // Não precisa fazer nada específico pois são geradas dinamicamente
    }
    
    /// <summary>
    /// Ativa/desativa ofertas padrão
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        enableDefaultOffers = enabled;
        DebugLog($"Ofertas padrão {(enabled ? "ativadas" : "desativadas")}");
    }
    
    #endregion
}