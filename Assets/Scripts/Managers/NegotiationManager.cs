// Assets/Scripts/Negotiation/NegotiationManager.cs (UPDATED)

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Gerencia a cena de negociação e a seleção de cartas
/// </summary>
public class NegotiationManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI confirmButtonText;
    
    [Header("Instructions")]
    [SerializeField] private TextMeshProUGUI instructionText;
    
    // Estado interno
    private List<NegotiationCardUI> cardUIList = new List<NegotiationCardUI>();
    private NegotiationCardUI selectedCard;
    
    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager não encontrado!");
            return;
        }
        
        if (!(GameManager.Instance.CurrentEvent is NegotiationEventSO negotiationEvent))
        {
            Debug.LogError("Evento atual não é um NegotiationEvent!");
            return;
        }
        
        SetupUI(negotiationEvent);
        GenerateCards(negotiationEvent);
        
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(ConfirmSelection);
            confirmButton.interactable = false; // Começa desabilitado
        }
        
        UpdateInstructions();
    }
    
    void Update()
    {
        // Permite abrir o painel de status com a tecla E
        if (Input.GetKeyDown(KeyCode.E) && StatusPanel.Instance != null)
        {
            StatusPanel.Instance.TogglePanel();
        }
    }
    
    private void SetupUI(NegotiationEventSO negotiationEvent)
    {
        
        // Configura texto inicial do botão
        if (confirmButtonText != null)
        {
            confirmButtonText.text = "Selecione uma Carta";
        }
    }
    
    private void GenerateCards(NegotiationEventSO negotiationEvent)
    {
        if (cardsContainer == null || cardPrefab == null)
        {
            Debug.LogError("cardsContainer ou cardPrefab não configurados!");
            return;
        }
        
        // Limpa cartas antigas
        ClearCards();
        
        // Obtém cartas aleatórias
        List<NegotiationCardSO> cards = negotiationEvent.GetRandomCards();
        
        if (cards.Count == 0)
        {
            Debug.LogError("Nenhuma carta foi gerada!");
            return;
        }
        
        Debug.Log($"Gerando {cards.Count} cartas de negociação");
        
        // Cria UI para cada carta
        foreach (var cardData in cards)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardsContainer);
            NegotiationCardUI cardUI = cardObj.GetComponent<NegotiationCardUI>();
            
            if (cardUI != null)
            {
                cardUI.Setup(cardData, this);
                cardUIList.Add(cardUI);
            }
            else
            {
                Debug.LogError("Prefab da carta não tem componente NegotiationCardUI!");
            }
        }
    }
    
    /// <summary>
    /// Chamado quando o jogador seleciona uma carta
    /// </summary>
    public void SelectCard(NegotiationCardUI card)
    {
        if (card == null) return;
        
        // Se já havia uma carta selecionada, desmarca ela
        if (selectedCard != null && selectedCard != card)
        {
            selectedCard.SetSelected(false);
            Debug.Log($"Carta '{selectedCard.GetCardData().cardName}' desmarcada");
        }
        
        // Marca a nova carta como selecionada
        selectedCard = card;
        selectedCard.SetSelected(true);
        
        // Habilita o botão de confirmar
        if (confirmButton != null)
        {
            confirmButton.interactable = true;
        }
        
        if (confirmButtonText != null)
        {
            confirmButtonText.text = "Confirmar Negociação";
        }
        
        UpdateInstructions();
        
        Debug.Log($"✅ Carta selecionada: {card.GetCardData().cardName}");
    }
    
    private void UpdateInstructions()
    {
        if (instructionText == null) return;
        
        if (selectedCard == null)
        {
            instructionText.text = "Escolha uma carta de negociação para continuar.\nPressione 'E' para ver seu status.";
        }
        else
        {
            instructionText.text = $"<color=#FFD700><b>Carta Selecionada:</b></color> {selectedCard.GetCardData().cardName}\n\nClique em '<b>Confirmar Negociação</b>' para aplicar os efeitos.\nOu clique em outra carta para mudar sua escolha.\n\n<size=80%>Pressione 'E' para ver seu status</size>";
        }
    }
    
    // Modifique o método ConfirmSelection no NegotiationManager.cs

// Modifique apenas o método ConfirmSelection

    private void ConfirmSelection()
    {
        if (selectedCard == null)
        {
            Debug.LogWarning("Nenhuma carta foi selecionada!");
            return;
        }
    
        NegotiationCardSO cardData = selectedCard.GetCardData();
        CardAttribute playerAttr = selectedCard.GetSelectedPlayerAttribute();
        CardAttribute enemyAttr = selectedCard.GetSelectedEnemyAttribute();
        int value = selectedCard.GetSelectedValue();
    
        Debug.Log($"");
        Debug.Log($"╔═══════════════════════════════════════════════╗");
        Debug.Log($"║         NEGOCIAÇÃO CONFIRMADA                 ║");
        Debug.Log($"╚═══════════════════════════════════════════════╝");
        Debug.Log($"");
        Debug.Log($"📜 Carta: {cardData.cardName}");
        Debug.Log($"🎯 Tipo: {cardData.cardType}");
        Debug.Log($"⚖️  Valor Amarrado: +{value}");
        Debug.Log($"");
        Debug.Log($"┌───────────────────────────────────────────────┐");
        Debug.Log($"│  📈 JOGADOR GANHA:                            │");
        Debug.Log($"│     {AttributeHelper.GetDisplayName(playerAttr)}: +{value}");
        Debug.Log($"│                                               │");
        Debug.Log($"│  📉 INIMIGOS GANHAM:                          │");
        Debug.Log($"│     {AttributeHelper.GetDisplayName(enemyAttr)}: +{value}");
        Debug.Log($"└───────────────────────────────────────────────┘");
        Debug.Log($"");
    
        // Notifica o GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteNegotiationEvent(cardData, playerAttr, enemyAttr, value);
        }
    
        ReturnToMap();
    }
    
    private void ReturnToMap()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMap();
        }
    }
    
    private void ClearCards()
    {
        foreach (var card in cardUIList)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        cardUIList.Clear();
    }
    
    void OnValidate()
    {
        if (cardsContainer == null)
            Debug.LogWarning("NegotiationManager: cardsContainer não foi atribuído!");
        
        if (cardPrefab == null)
            Debug.LogWarning("NegotiationManager: cardPrefab não foi atribuído!");
        
        if (confirmButton == null)
            Debug.LogWarning("NegotiationManager: confirmButton não foi atribuído!");
    }
}