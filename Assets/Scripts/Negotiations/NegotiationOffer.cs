// Assets/Scripts/Negotiation/NegotiationOffer.cs (CORRIGIDO)

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representa UMA ÚNICA oferta - ou vantagem OU desvantagem
/// NUNCA ambas ao mesmo tempo
/// </summary>
[System.Serializable]
public class NegotiationOffer
{
    public string offerName;
    public string offerDescription;
    public bool isAdvantage; // true = vantagem, false = desvantagem
    
    // APENAS UM efeito por oferta
    public CardAttribute targetAttribute;
    public int value;
    public bool affectsPlayer; // true = afeta jogador, false = afeta inimigos
    
    // Metadados
    public BehaviorTriggerType sourceObservationType;
    public string contextualInfo;
    
    // NOVO: Dados customizados para ofertas especiais
    private Dictionary<string, object> customData = new Dictionary<string, object>();
    
    /// <summary>
    /// NOVO: Define dado customizado
    /// </summary>
    public void SetData(string key, object value)
    {
        customData[key] = value;
    }
    
    /// <summary>
    /// NOVO: Obtém dado customizado
    /// </summary>
    public T GetData<T>(string key, T defaultValue = default(T))
    {
        if (customData.TryGetValue(key, out object value) && value is T)
        {
            return (T)value;
        }
        return defaultValue;
    }
    
    /// <summary>
    /// NOVO: Verifica se tem dado customizado
    /// </summary>
    public bool HasData(string key)
    {
        return customData.ContainsKey(key);
    }
    
    /// <summary>
    /// Construtor para VANTAGEM (sempre beneficia o jogador)
    /// </summary>
    public static NegotiationOffer CreateAdvantage(
        string name, 
        string description,
        CardAttribute playerAttr, 
        int playerValue,
        BehaviorTriggerType sourceType, 
        string context = "")
    {
        return new NegotiationOffer
        {
            offerName = name,
            offerDescription = description,
            isAdvantage = true,
            targetAttribute = playerAttr,
            value = playerValue,
            affectsPlayer = true,
            sourceObservationType = sourceType,
            contextualInfo = context
        };
    }
    
    /// <summary>
    /// Construtor para DESVANTAGEM (sempre prejudica o jogador OU beneficia inimigos)
    /// </summary>
    public static NegotiationOffer CreateDisadvantage(
        string name,
        string description,
        CardAttribute attribute,
        int value,
        bool affectsPlayer, // true = debuff no jogador, false = buff nos inimigos
        BehaviorTriggerType sourceType,
        string context = "")
    {
        return new NegotiationOffer
        {
            offerName = name,
            offerDescription = description,
            isAdvantage = false,
            targetAttribute = attribute,
            value = value,
            affectsPlayer = affectsPlayer,
            sourceObservationType = sourceType,
            contextualInfo = context
        };
    }
    
    /// <summary>
    /// Retorna descrição formatada clara
    /// </summary>
    public string GetFormattedDescription()
    {
        string desc = $"<b>{offerName}</b>\n\n";
        desc += $"<i>{offerDescription}</i>\n\n";
        
        if (isAdvantage)
        {
            // Vantagem sempre beneficia jogador
            string sign = value > 0 ? "+" : "";
            desc += $"<color=#90EE90><b>✓ Você ganha:</b></color>\n";
            desc += $"{sign}{value} {AttributeHelper.GetDisplayName(targetAttribute)}";
        }
        else
        {
            // Desvantagem - ou debuff no jogador OU buff nos inimigos
            desc += $"<color=#FF6B6B><b>✗ Custo:</b></color>\n";
            
            if (affectsPlayer)
            {
                // Debuff no jogador
                string sign = value > 0 ? "+" : "";
                desc += $"Você perde: {sign}{value} {AttributeHelper.GetDisplayName(targetAttribute)}";
            }
            else
            {
                // Buff nos inimigos
                string sign = value > 0 ? "+" : "";
                desc += $"Inimigos ganham: {sign}{value} {AttributeHelper.GetDisplayName(targetAttribute)}";
            }
        }
        
        return desc;
    }
}