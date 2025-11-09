using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DynamicNegotiationCardGenerator : MonoBehaviour
{
    public static DynamicNegotiationCardGenerator Instance { get; private set; }
    
    [SerializeField] private int maxObservationsToProcess = 10;
    [SerializeField] private bool clearProcessedObservations = true;
    
    [SerializeField] [Range(0f, 1f)] private float fixedProbability = 0.4f;
    
    [SerializeField] [Range(0f, 1f)] private float intensityOnlyProbability = 0.3f;
    
    private List<NegotiationOffer> advantagePool = new List<NegotiationOffer>();
    private List<NegotiationOffer> disadvantagePool = new List<NegotiationOffer>();
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
            return;
        }
        
        advantagePool.Clear();
        disadvantagePool.Clear();
        usedAdvantages.Clear();
        usedDisadvantages.Clear();
        
        if (DefaultNegotiationOffers.Instance != null)
        {
            List<NegotiationOffer> defaultOffers = DefaultNegotiationOffers.Instance.GenerateDefaultOffers();
            
            foreach (var offer in defaultOffers)
            {
                if (offer.isAdvantage)
                {
                    advantagePool.Add(offer);
                }
                else
                {
                    disadvantagePool.Add(offer);
                }
            }
        }
        
        var observations = PlayerBehaviorAnalyzer.Instance.GetUnresolvedNegotiationTriggers(maxObservationsToProcess);
        List<BehaviorObservation> processedObservations = new List<BehaviorObservation>();
        
        foreach (var obs in observations)
        {
            List<NegotiationOffer> offers = ObservationInterpreter.InterpretObservation(obs);
            
            if (offers.Count > 0)
            {
                foreach (var offer in offers)
                {
                    if (offer.isAdvantage)
                    {
                        advantagePool.Add(offer);
                    }
                    else
                    {
                        disadvantagePool.Add(offer);
                    }
                }
                
                processedObservations.Add(obs);
            }
        }
        
        ShuffleList(advantagePool);
        ShuffleList(disadvantagePool);
        
        if (clearProcessedObservations && PlayerBehaviorAnalyzer.Instance != null)
        {
            PlayerBehaviorAnalyzer.Instance.ConsumeObservations(processedObservations);
        }
    }
    
    public List<DynamicNegotiationCard> GenerateCards(int numberOfCards)
    {
        List<DynamicNegotiationCard> cards = new List<DynamicNegotiationCard>();
        
        int possibleCards = Mathf.Min(advantagePool.Count, disadvantagePool.Count);
        possibleCards = Mathf.Min(possibleCards, numberOfCards);
        
        if (possibleCards == 0)
        {
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
        }
        
        return cards;
    }
    
    private NegotiationOffer FindBestMatch(NegotiationOffer advantage, List<NegotiationOffer> disadvantages)
    {
        if (disadvantages.Count == 0)
            return null;
        
        if (disadvantages.Count == 1)
            return disadvantages[0];
        
        Dictionary<NegotiationOffer, float> scores = new Dictionary<NegotiationOffer, float>();
        
        foreach (var disadvantage in disadvantages)
        {
            float score = CalculateMatchScore(advantage, disadvantage);
            scores[disadvantage] = score;
        }
        
        var sorted = scores.OrderByDescending(kvp => kvp.Value).ToList();
        
        int topCount = Mathf.Min(3, sorted.Count);
        int selectedIndex = Random.Range(0, topCount);
        
        return sorted[selectedIndex].Key;
    }
    
    private float CalculateMatchScore(NegotiationOffer advantage, NegotiationOffer disadvantage)
    {
        float score = 0f;
    
        if (GetAttributeCategory(advantage.targetAttribute) == GetAttributeCategory(disadvantage.targetAttribute))
        {
            score += 30f;
        }
    
        int valueDiff = Mathf.Abs(advantage.value - disadvantage.value);
        if (valueDiff < 10)
        {
            score += 20f - valueDiff;
        }
    
        if (advantage.sourceObservationType == disadvantage.sourceObservationType)
        {
            score += 25f;
        }
    
        if (AreAttributesComplementary(advantage.targetAttribute, disadvantage.targetAttribute))
        {
            score += 15f;
        }
    
        float balance = (float)advantage.value / Mathf.Max(1, Mathf.Abs(disadvantage.value));
        if (balance >= 0.8f && balance <= 1.2f)
        {
            score += 10f;
        }
    
        return score;
    }
    
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
    
    private bool AreAttributesComplementary(CardAttribute attr1, CardAttribute attr2)
    {
        if ((attr1 == CardAttribute.PlayerMaxHP && attr2 == CardAttribute.EnemyDefense) ||
            (attr1 == CardAttribute.PlayerDefense && attr2 == CardAttribute.EnemyMaxHP))
            return true;
        
        if ((attr1 == CardAttribute.PlayerActionPower && attr2 == CardAttribute.EnemyDefense) ||
            (attr1 == CardAttribute.PlayerDefense && attr2 == CardAttribute.EnemyActionPower))
            return true;
        
        if ((attr1 == CardAttribute.PlayerMaxMP && attr2 == CardAttribute.EnemyActionManaCost) ||
            (attr1 == CardAttribute.PlayerActionManaCost && attr2 == CardAttribute.EnemyMaxMP))
            return true;
        
        if (attr1 == CardAttribute.PlayerSpeed || attr2 == CardAttribute.EnemySpeed)
            return true;
        
        return false;
    }
    
    public DynamicNegotiationCard GenerateSingleCard()
    {
        List<NegotiationOffer> availableAdvantages = advantagePool
            .Where(a => !usedAdvantages.Contains(a))
            .ToList();
        
        List<NegotiationOffer> availableDisadvantages = disadvantagePool
            .Where(d => !usedDisadvantages.Contains(d))
            .ToList();
        
        if (availableAdvantages.Count == 0 || availableDisadvantages.Count == 0)
        {
            return null;
        }
        
        int advIndex = Random.Range(0, availableAdvantages.Count);
        NegotiationOffer advantage = availableAdvantages[advIndex];
        
        NegotiationOffer disadvantage = FindBestMatch(advantage, availableDisadvantages);
        
        if (disadvantage == null)
        {
            return null;
        }
        
        usedAdvantages.Add(advantage);
        usedDisadvantages.Add(disadvantage);
        
        NegotiationCardType cardType = ChooseIntelligentCardType(advantage, disadvantage);
        
        DynamicNegotiationCard card = new DynamicNegotiationCard(advantage, disadvantage, cardType);
        
        return card;
    }
    
    public void ReleaseCardOffers(DynamicNegotiationCard card)
    {
        if (card == null) return;
        
        usedAdvantages.Remove(card.playerBenefit);
        usedDisadvantages.Remove(card.playerCost);
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
    
    [ContextMenu("Force Process Observations")]
    public void ForceProcessObservations()
    {
        ProcessObservations();
    }
}

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