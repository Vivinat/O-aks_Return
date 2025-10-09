// Assets/Scripts/Negotiation/NegotiationOffer.cs

using UnityEngine;

/// <summary>
/// Representa uma oferta de negociação gerada dinamicamente a partir de observações
/// </summary>
[System.Serializable]
public class NegotiationOffer
{
    public string offerName;
    public string offerDescription;
    public bool isAdvantage; // true = vantagem para jogador, false = desvantagem
    
    // Efeito no jogador
    public CardAttribute playerAttribute;
    public int playerValue;
    
    // Efeito nos inimigos
    public CardAttribute enemyAttribute;
    public int enemyValue;
    
    // Metadados para rastreamento
    public BehaviorTriggerType sourceObservationType;
    public string contextualInfo; // Info extra sobre a observação que gerou esta oferta
    
    public NegotiationOffer(string name, string description, bool advantage,
                           CardAttribute playerAttr, int playerVal,
                           CardAttribute enemyAttr, int enemyVal,
                           BehaviorTriggerType sourceType, string context = "")
    {
        offerName = name;
        offerDescription = description;
        isAdvantage = advantage;
        playerAttribute = playerAttr;
        playerValue = playerVal;
        enemyAttribute = enemyAttr;
        enemyValue = enemyVal;
        sourceObservationType = sourceType;
        contextualInfo = context;
    }
    
    /// <summary>
    /// Cria uma descrição formatada para exibição
    /// </summary>
    public string GetFormattedDescription()
    {
        string desc = $"<b>{offerName}</b>\n\n";
        desc += $"<i>{offerDescription}</i>\n\n";
        
        if (playerValue != 0)
        {
            string playerSign = playerValue > 0 ? "+" : "";
            desc += $"<color=#90EE90><b>Você:</b></color> {playerSign}{playerValue} {AttributeHelper.GetDisplayName(playerAttribute)}\n";
        }
        
        if (enemyValue != 0)
        {
            string enemySign = enemyValue > 0 ? "+" : "";
            desc += $"<color=#FF6B6B><b>Inimigos:</b></color> {enemySign}{enemyValue} {AttributeHelper.GetDisplayName(enemyAttribute)}";
        }
        
        return desc;
    }
}