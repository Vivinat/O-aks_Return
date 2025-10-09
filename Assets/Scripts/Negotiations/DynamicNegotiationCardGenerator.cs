// Assets/Scripts/Negotiation/DynamicNegotiationCardGenerator.cs

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
    // AttributeAndIntensity será o resto (1 - fixed - intensity)
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private List<NegotiationOffer> advantagePool = new List<NegotiationOffer>();
    private List<NegotiationOffer> disadvantagePool = new List<NegotiationOffer>();
    
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
                        DebugLog($"  ✓ Vantagem: {offer.offerName}");
                    }
                    else
                    {
                        disadvantagePool.Add(offer);
                        DebugLog($"  ✗ Desvantagem: {offer.offerName}");
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
    
    /// <summary>
    /// Gera cartas dinâmicas com tipos variados
    /// </summary>
    public List<DynamicNegotiationCard> GenerateCards(int numberOfCards)
    {
        List<DynamicNegotiationCard> cards = new List<DynamicNegotiationCard>();
        
        int possibleCards = Mathf.Min(advantagePool.Count, disadvantagePool.Count);
        possibleCards = Mathf.Min(possibleCards, numberOfCards);
        
        DebugLog($"=== GERANDO {possibleCards} CARTAS ===");
        
        if (possibleCards == 0)
        {
            DebugLog("⚠️ Pools vazias - não é possível gerar cartas dinâmicas!");
            return cards;
        }
        
        for (int i = 0; i < possibleCards; i++)
        {
            NegotiationOffer advantage = advantagePool[i];
            NegotiationOffer disadvantage = disadvantagePool[i];
            
            // Escolhe tipo de carta aleatoriamente
            NegotiationCardType cardType = ChooseRandomCardType();
            
            DynamicNegotiationCard card = new DynamicNegotiationCard(advantage, disadvantage, cardType);
            cards.Add(card);
            
            DebugLog($"Carta {i + 1} ({cardType}): {card.GetCardName()}");
        }
        
        return cards;
    }
    
    private NegotiationCardType ChooseRandomCardType()
    {
        float roll = Random.value;
        
        if (roll < fixedProbability)
        {
            return NegotiationCardType.Fixed;
        }
        else if (roll < fixedProbability + intensityOnlyProbability)
        {
            return NegotiationCardType.IntensityOnly;
        }
        else
        {
            return NegotiationCardType.AttributeAndIntensity;
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