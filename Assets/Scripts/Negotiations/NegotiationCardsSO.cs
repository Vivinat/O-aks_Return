// Assets/Scripts/Negotiation/NegotiationCardSO.cs (SIMPLIFIED)

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Negotiation Card", menuName = "Negotiation/Card")]
public class NegotiationCardSO : ScriptableObject
{
    [Header("═══ INFORMAÇÕES BÁSICAS ═══")]
    public string cardName = "Nova Carta";
    public Sprite cardSprite;
    
    [TextArea(2, 4)]
    public string cardDescription = "Descrição da negociação";
    
    [Header("═══ TIPO DE CARTA ═══")]
    public NegotiationCardType cardType = NegotiationCardType.Fixed;
    
    [Header("═══ CONFIGURAÇÃO: FIXED ═══")]
    [Tooltip("Apenas para tipo FIXED")]
    public int fixedValue = 20;
    
    [Tooltip("Atributo que o JOGADOR ganha (Fixed)")]
    public CardAttribute fixedPlayerAttribute = CardAttribute.PlayerMaxHP;
    
    [Tooltip("Atributo que os INIMIGOS ganham (Fixed)")]
    public CardAttribute fixedEnemyAttribute = CardAttribute.EnemyMaxHP;
    
    [Header("═══ CONFIGURAÇÃO: INTENSITY ONLY ═══")]
    [Tooltip("Apenas para tipo INTENSITY ONLY")]
    public CardAttribute intensityOnlyPlayerAttribute = CardAttribute.PlayerMaxHP;
    
    [Tooltip("Apenas para tipo INTENSITY ONLY")]
    public CardAttribute intensityOnlyEnemyAttribute = CardAttribute.EnemyMaxHP;
    
    [Header("═══ CONFIGURAÇÃO: INTENSITY AND ATTRIBUTE ═══")]
    [Tooltip("Apenas para tipo INTENSITY AND ATTRIBUTE - Lista de atributos disponíveis")]
    public List<CardAttribute> availablePlayerAttributes = new List<CardAttribute>();
    
    [Tooltip("Apenas para tipo INTENSITY AND ATTRIBUTE - Lista de atributos disponíveis")]
    public List<CardAttribute> availableEnemyAttributes = new List<CardAttribute>();
    
    [Header("═══ INTENSIDADES DISPONÍVEIS ═══")]
    [Tooltip("Para INTENSITY ONLY e INTENSITY AND ATTRIBUTE")]
    public List<CardIntensity> availableIntensities = new List<CardIntensity>
    {
        CardIntensity.Low,
        CardIntensity.Medium,
        CardIntensity.High
    };
    
    /// <summary>
    /// Retorna o texto completo formatado da carta
    /// </summary>
    public string GetFullDescription(CardAttribute? playerAttr, CardAttribute? enemyAttr, int value)
    {
        string desc = $"<b><size=110%>{cardName}</size></b>\n\n";
        desc += $"<i>{cardDescription}</i>\n\n";
        desc += $"<color=#90EE90><b>✓ Você Ganha:</b></color>\n";
        desc += $"+{value} de {AttributeHelper.GetDisplayName(playerAttr ?? fixedPlayerAttribute)}\n\n";
        desc += $"<color=#FF6B6B><b>✗ Inimigos Ganham:</b></color>\n";
        desc += $"+{value} de {AttributeHelper.GetDisplayName(enemyAttr ?? fixedEnemyAttribute)}";
        return desc;
    }
    
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(cardName)) return false;
        
        if (cardType == NegotiationCardType.AttributeAndIntensity)
        {
            if (availablePlayerAttributes.Count == 0 || availableEnemyAttributes.Count == 0)
            {
                Debug.LogError($"Carta '{name}': INTENSITY AND ATTRIBUTE requer listas de atributos!");
                return false;
            }
        }
        
        if (cardType != NegotiationCardType.Fixed && availableIntensities.Count == 0)
        {
            Debug.LogError($"Carta '{name}': Tipo {cardType} requer intensidades disponíveis!");
            return false;
        }
        
        return true;
    }
    
    void OnValidate()
    {
        // Ajuda visual no Inspector
        if (cardType == NegotiationCardType.Fixed)
        {
            // Limpa listas não usadas
            availablePlayerAttributes.Clear();
            availableEnemyAttributes.Clear();
            availableIntensities.Clear();
        }
        else if (cardType == NegotiationCardType.IntensityOnly)
        {
            // Limpa listas não usadas
            availablePlayerAttributes.Clear();
            availableEnemyAttributes.Clear();
            
            if (availableIntensities.Count == 0)
            {
                availableIntensities.Add(CardIntensity.Medium);
            }
        }
        else if (cardType == NegotiationCardType.AttributeAndIntensity)
        {
            if (availableIntensities.Count == 0)
            {
                availableIntensities.Add(CardIntensity.Medium);
            }
            
            if (availablePlayerAttributes.Count == 0)
            {
                availablePlayerAttributes.Add(CardAttribute.PlayerMaxHP);
            }
            
            if (availableEnemyAttributes.Count == 0)
            {
                availableEnemyAttributes.Add(CardAttribute.EnemyMaxHP);
            }
        }
    }
}