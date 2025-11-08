using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Evento que leva para a cena de negociação
/// </summary>
[CreateAssetMenu(menuName = "Events/Negotiation Event SO")]
public class NegotiationEventSO : EventTypeSO
{
    [Header("Negotiation Configuration")]
    [Tooltip("Pool de cartas disponíveis para este evento")]
    public List<NegotiationCardSO> cardPool = new List<NegotiationCardSO>();
    
    [Tooltip("Número de cartas para apresentar (padrão: 3)")]
    public int numberOfCards = 3;
    
    [Header("Visual Configuration")]
    [Tooltip("Fundo da cena de negociação (opcional)")]
    public Sprite negotiationBackground;
    
    /// <summary>
    /// Seleciona cartas aleatórias do pool
    /// </summary>
    public List<NegotiationCardSO> GetRandomCards()
    {
        List<NegotiationCardSO> validCards = new List<NegotiationCardSO>();
        
        foreach (var card in cardPool)
        {
            if (card != null && card.IsValid())
            {
                validCards.Add(card);
            }
        }
        
        if (validCards.Count == 0)
        {
            Debug.LogError($"NegotiationEvent '{name}': Nenhuma carta válida no pool!");
            return new List<NegotiationCardSO>();
        }
        
        List<NegotiationCardSO> shuffled = new List<NegotiationCardSO>(validCards);
        
        for (int i = 0; i < shuffled.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffled.Count);
            var temp = shuffled[i];
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }
        
        int cardsToReturn = Mathf.Min(numberOfCards, shuffled.Count);
        return shuffled.GetRange(0, cardsToReturn);
    }
    
    void OnValidate()
    {
        if (eventType != EventType.DifficultAdjust)
        {
            Debug.LogWarning($"NegotiationEvent '{name}': EventType deve ser DifficultAdjust!");
            eventType = EventType.DifficultAdjust;
        }
        
        if (cardPool.Count == 0)
        {
            Debug.LogWarning($"NegotiationEvent '{name}': Pool de cartas está vazio!");
        }
        
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            sceneToLoad = "NegotiationScene";
        }
    }
}