// Assets/Scripts/Negotiation/DynamicNegotiationCardGenerator.cs (UPDATED - Smart Matching)

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DynamicNegotiationCardGenerator : MonoBehaviour
{
    public static DynamicNegotiationCardGenerator Instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private int maxObservationsToProcess = 10;
    [SerializeField] private bool clearProcessedObservations = true;
    
    [Header("Card Type Distribution")]
    [Tooltip("Probabilidade de carta Fixed (0-1)")]
    [SerializeField] [Range(0f, 1f)] private float fixedProbability = 0.4f;
    
    [Tooltip("Probabilidade de carta IntensityOnly (0-1)")]
    [SerializeField] [Range(0f, 1f)] private float intensityOnlyProbability = 0.3f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private List<NegotiationOffer> advantagePool = new List<NegotiationOffer>();
    private List<NegotiationOffer> disadvantagePool = new List<NegotiationOffer>();
    
    // NOVO: Pools usados para evitar repetição em refresh
    private List<NegotiationOffer> usedAdvantages = new List<NegotiationOffer>();
    private List<NegotiationOffer> usedDisadvantages = new List<NegotiationOffer>();
    
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
    
    public void ProcessObservations()
    {
        if (PlayerBehaviorAnalyzer.Instance == null)
        {
            DebugLog("PlayerBehaviorAnalyzer não encontrado!");
            return;
        }
        
        advantagePool.Clear();
        disadvantagePool.Clear();
        usedAdvantages.Clear();
        usedDisadvantages.Clear();
        
        // Adiciona ofertas padrão PRIMEIRO
        if (DefaultNegotiationOffers.Instance != null)
        {
            List<NegotiationOffer> defaultOffers = DefaultNegotiationOffers.Instance.GenerateDefaultOffers();
            
            foreach (var offer in defaultOffers)
            {
                if (offer.isAdvantage)
                {
                    advantagePool.Add(offer);
                    DebugLog($"  ✓ Vantagem padrão: {offer.offerName}");
                }
                else
                {
                    disadvantagePool.Add(offer);
                    DebugLog($"  ✗ Desvantagem padrão: {offer.offerName}");
                }
            }
        }
        
        // Processa observações comportamentais
        var observations = PlayerBehaviorAnalyzer.Instance.GetUnresolvedNegotiationTriggers(maxObservationsToProcess);
        
        DebugLog($"=== PROCESSANDO {observations.Count} OBSERVAÇÕES ===");
        
        List<BehaviorObservation> processedObservations = new List<BehaviorObservation>();
        
        foreach (var obs in observations)
        {
            DebugLog($"Processando: {obs.triggerType}");
            
            List<NegotiationOffer> offers = ObservationInterpreter.InterpretObservation(obs);
            
            if (offers.Count > 0)
            {
                foreach (var offer in offers)
                {
                    if (offer.isAdvantage)
                    {
                        advantagePool.Add(offer);
                        DebugLog($"  ✓ Vantagem comportamental: {offer.offerName}");
                    }
                    else
                    {
                        disadvantagePool.Add(offer);
                        DebugLog($"  ✗ Desvantagem comportamental: {offer.offerName}");
                    }
                }
                
                processedObservations.Add(obs);
            }
        }
        
        ShuffleList(advantagePool);
        ShuffleList(disadvantagePool);
        
        DebugLog($"=== POOLS FINAIS ===");
        DebugLog($"Vantagens: {advantagePool.Count}");
        DebugLog($"Desvantagens: {disadvantagePool.Count}");
        
        if (clearProcessedObservations && PlayerBehaviorAnalyzer.Instance != null)
        {
            PlayerBehaviorAnalyzer.Instance.ConsumeObservations(processedObservations);
            DebugLog($"Removidas {processedObservations.Count} observações do histórico");
        }
    }
    
    public List<DynamicNegotiationCard> GenerateCards(int numberOfCards)
    {
        List<DynamicNegotiationCard> cards = new List<DynamicNegotiationCard>();
        
        int possibleCards = Mathf.Min(advantagePool.Count, disadvantagePool.Count);
        possibleCards = Mathf.Min(possibleCards, numberOfCards);
        
        DebugLog($"=== GERANDO {possibleCards} CARTAS COM CASAMENTO INTELIGENTE ===");
        
        if (possibleCards == 0)
        {
            DebugLog("⚠️ Pools vazias - não é possível gerar cartas dinâmicas!");
            return cards;
        }
        
        List<NegotiationOffer> availableAdvantages = new List<NegotiationOffer>(advantagePool);
        List<NegotiationOffer> availableDisadvantages = new List<NegotiationOffer>(disadvantagePool);
        
        for (int i = 0; i < possibleCards; i++)
        {
            if (availableAdvantages.Count == 0 || availableDisadvantages.Count == 0)
                break;
            
            int advIndex = Random.Range(0, availableAdvantages.Count);
            NegotiationOffer advantage = availableAdvantages[advIndex];
            availableAdvantages.RemoveAt(advIndex);
            
            NegotiationOffer disadvantage = FindBestMatch(advantage, availableDisadvantages);
            availableDisadvantages.Remove(disadvantage);
            
            usedAdvantages.Add(advantage);
            usedDisadvantages.Add(disadvantage);
            
            NegotiationCardType cardType = ChooseIntelligentCardType(advantage, disadvantage);
            
            DynamicNegotiationCard card = new DynamicNegotiationCard(advantage, disadvantage, cardType);
            cards.Add(card);
            
            // CORRIGIDO: targetAttribute em vez de playerAttribute/enemyAttribute
            DebugLog($"Carta {i + 1} ({cardType}): {card.GetCardName()}");
            DebugLog($"  Vantagem: {advantage.offerName} ({advantage.targetAttribute}, valor: {advantage.value})");
            DebugLog($"  Custo: {disadvantage.offerName} ({disadvantage.targetAttribute}, valor: {disadvantage.value})");
            DebugLog($"  Score de match: {CalculateMatchScore(advantage, disadvantage):F2}");
        }
        
        return cards;
    }
    
    /// <summary>
    /// NOVO: Encontra a melhor desvantagem para casar com uma vantagem
    /// </summary>
    private NegotiationOffer FindBestMatch(NegotiationOffer advantage, List<NegotiationOffer> disadvantages)
    {
        if (disadvantages.Count == 0)
            return null;
        
        if (disadvantages.Count == 1)
            return disadvantages[0];
        
        // Calcula score para cada desvantagem
        Dictionary<NegotiationOffer, float> scores = new Dictionary<NegotiationOffer, float>();
        
        foreach (var disadvantage in disadvantages)
        {
            float score = CalculateMatchScore(advantage, disadvantage);
            scores[disadvantage] = score;
        }
        
        // Ordena por score (maior = melhor match)
        var sorted = scores.OrderByDescending(kvp => kvp.Value).ToList();
        
        // Pega um dos 3 melhores aleatoriamente (para variedade)
        int topCount = Mathf.Min(3, sorted.Count);
        int selectedIndex = Random.Range(0, topCount);
        
        return sorted[selectedIndex].Key;
    }
    
    /// <summary>
    /// NOVO: Calcula score de compatibilidade entre vantagem e desvantagem
    /// </summary>
    private float CalculateMatchScore(NegotiationOffer advantage, NegotiationOffer disadvantage)
    {
        float score = 0f;
    
        // 1. Categoria de atributo similar (+30 pontos)
        // CORRIGIDO: targetAttribute
        if (GetAttributeCategory(advantage.targetAttribute) == GetAttributeCategory(disadvantage.targetAttribute))
        {
            score += 30f;
        }
    
        // 2. Valores similares (+20 pontos se diferença < 10)
        // CORRIGIDO: value
        int valueDiff = Mathf.Abs(advantage.value - disadvantage.value);
        if (valueDiff < 10)
        {
            score += 20f - valueDiff;
        }
    
        // 3. Mesmo tipo de observação (+25 pontos)
        if (advantage.sourceObservationType == disadvantage.sourceObservationType)
        {
            score += 25f;
        }
    
        // 4. Atributos complementares (+15 pontos)
        // CORRIGIDO: targetAttribute
        if (AreAttributesComplementary(advantage.targetAttribute, disadvantage.targetAttribute))
        {
            score += 15f;
        }
    
        // 5. Balance check: vantagem não deve ser muito maior que custo (+10 pontos se balanceado)
        // CORRIGIDO: value
        float balance = (float)advantage.value / Mathf.Max(1, Mathf.Abs(disadvantage.value));
        if (balance >= 0.8f && balance <= 1.2f)
        {
            score += 10f;
        }
    
        return score;
    }
    
    /// <summary>
    /// NOVO: Retorna categoria do atributo para matching
    /// </summary>
    private AttributeCategory GetAttributeCategory(CardAttribute attr)
    {
        switch (attr)
        {
            case CardAttribute.PlayerMaxHP:
            case CardAttribute.EnemyMaxHP:
                return AttributeCategory.Health;
            
            case CardAttribute.PlayerMaxMP:
            case CardAttribute.EnemyMaxMP:
            case CardAttribute.PlayerActionManaCost:
            case CardAttribute.EnemyActionManaCost:
                return AttributeCategory.Mana;
            
            case CardAttribute.PlayerDefense:
            case CardAttribute.EnemyDefense:
                return AttributeCategory.Defense;
            
            case CardAttribute.PlayerSpeed:
            case CardAttribute.EnemySpeed:
                return AttributeCategory.Speed;
            
            case CardAttribute.PlayerActionPower:
            case CardAttribute.EnemyActionPower:
                return AttributeCategory.Power;
            
            case CardAttribute.CoinsEarned:
            case CardAttribute.ShopPrices:
                return AttributeCategory.Economy;
            
            default:
                return AttributeCategory.Other;
        }
    }
    
    /// <summary>
    /// NOVO: Verifica se atributos são complementares (exemplo: HP com Defesa)
    /// </summary>
    private bool AreAttributesComplementary(CardAttribute attr1, CardAttribute attr2)
    {
        // HP complementa Defesa
        if ((attr1 == CardAttribute.PlayerMaxHP && attr2 == CardAttribute.EnemyDefense) ||
            (attr1 == CardAttribute.PlayerDefense && attr2 == CardAttribute.EnemyMaxHP))
            return true;
        
        // Poder complementa Defesa
        if ((attr1 == CardAttribute.PlayerActionPower && attr2 == CardAttribute.EnemyDefense) ||
            (attr1 == CardAttribute.PlayerDefense && attr2 == CardAttribute.EnemyActionPower))
            return true;
        
        // MP complementa custo de mana
        if ((attr1 == CardAttribute.PlayerMaxMP && attr2 == CardAttribute.EnemyActionManaCost) ||
            (attr1 == CardAttribute.PlayerActionManaCost && attr2 == CardAttribute.EnemyMaxMP))
            return true;
        
        // Speed é complementar a tudo (timing)
        if (attr1 == CardAttribute.PlayerSpeed || attr2 == CardAttribute.EnemySpeed)
            return true;
        
        return false;
    }
    
    /// <summary>
    /// NOVO: Gera uma carta única para refresh
    /// </summary>
    public DynamicNegotiationCard GenerateSingleCard()
    {
        // Remove ofertas já usadas das pools disponíveis
        List<NegotiationOffer> availableAdvantages = advantagePool
            .Where(a => !usedAdvantages.Contains(a))
            .ToList();
        
        List<NegotiationOffer> availableDisadvantages = disadvantagePool
            .Where(d => !usedDisadvantages.Contains(d))
            .ToList();
        
        if (availableAdvantages.Count == 0 || availableDisadvantages.Count == 0)
        {
            DebugLog("⚠️ Não há ofertas suficientes disponíveis para refresh!");
            return null;
        }
        
        // Pega uma vantagem aleatória
        int advIndex = Random.Range(0, availableAdvantages.Count);
        NegotiationOffer advantage = availableAdvantages[advIndex];
        
        // Encontra melhor match
        NegotiationOffer disadvantage = FindBestMatch(advantage, availableDisadvantages);
        
        if (disadvantage == null)
        {
            DebugLog("⚠️ Não foi possível encontrar match para refresh!");
            return null;
        }
        
        // Marca como usadas
        usedAdvantages.Add(advantage);
        usedDisadvantages.Add(disadvantage);
        
        NegotiationCardType cardType = ChooseIntelligentCardType(advantage, disadvantage);
        
        DynamicNegotiationCard card = new DynamicNegotiationCard(advantage, disadvantage, cardType);
        
        DebugLog($"Carta de refresh gerada: {card.GetCardName()}");
        DebugLog($"  Score de match: {CalculateMatchScore(advantage, disadvantage):F2}");
        
        return card;
    }
    
    /// <summary>
    /// NOVO: Libera ofertas de uma carta para a pool (quando carta é refreshada)
    /// </summary>
    public void ReleaseCardOffers(DynamicNegotiationCard card)
    {
        if (card == null) return;
        
        // Remove as ofertas da carta das listas de "usadas"
        usedAdvantages.Remove(card.playerBenefit);
        usedDisadvantages.Remove(card.playerCost);
        
        DebugLog($"Ofertas da carta '{card.GetCardName()}' liberadas de volta para a pool");
    }
    
    private NegotiationCardType ChooseIntelligentCardType(NegotiationOffer advantage, NegotiationOffer disadvantage)
    {
        bool playerAttrsRelated = AreAttributesRelated(advantage.targetAttribute);
        bool enemyAttrsRelated = AreAttributesRelated(disadvantage.targetAttribute);
        
        float roll = Random.value;
        
        if (roll < 0.4f && (playerAttrsRelated || enemyAttrsRelated))
        {
            return NegotiationCardType.AttributeAndIntensity;
        }
        else if (roll < 0.7f)
        {
            return NegotiationCardType.IntensityOnly;
        }
        else
        {
            return NegotiationCardType.Fixed;
        }
    }
    
    private bool AreAttributesRelated(CardAttribute attr)
    {
        switch (attr)
        {
            case CardAttribute.PlayerMaxHP:
            case CardAttribute.PlayerDefense:
            case CardAttribute.PlayerMaxMP:
                return true;
            
            case CardAttribute.PlayerActionPower:
            case CardAttribute.PlayerSpeed:
                return true;
            
            case CardAttribute.EnemyMaxHP:
            case CardAttribute.EnemyDefense:
            case CardAttribute.EnemyMaxMP:
                return true;
            
            case CardAttribute.EnemyActionPower:
            case CardAttribute.EnemySpeed:
                return true;
            
            default:
                return false;
        }
    }
    
    public bool HasEnoughOffers(int numberOfCards)
    {
        return advantagePool.Count >= numberOfCards && disadvantagePool.Count >= numberOfCards;
    }
    
    public int GetMaxPossibleCards()
    {
        return Mathf.Min(advantagePool.Count, disadvantagePool.Count);
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
            Debug.Log($"<color=magenta>[NegotiationGen]</color> {message}");
        }
    }
    
    [ContextMenu("Force Process Observations")]
    public void ForceProcessObservations()
    {
        ProcessObservations();
    }
}

/// <summary>
/// Categoria de atributo para matching
/// </summary>
public enum AttributeCategory
{
    Health,
    Mana,
    Defense,
    Speed,
    Power,
    Economy,
    Other
}