using UnityEngine;
using System.Collections.Generic;

// Evento que leva para a cena de negociação
[CreateAssetMenu(menuName = "Events/Negotiation Event SO")]
public class NegotiationEventSO : EventTypeSO
{
    public List<NegotiationCardSO> cardPool = new List<NegotiationCardSO>();
    
    public int numberOfCards = 3;
    
    public Sprite negotiationBackground;
    
    // Seleciona cartas aleatórias do pool
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
}