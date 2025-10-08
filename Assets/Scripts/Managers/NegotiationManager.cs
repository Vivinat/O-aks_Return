// Assets/Scripts/Negotiation/NegotiationManager.cs (FIXED - UI Control)

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
    
    // Estado interno
    private List<NegotiationCardUI> cardUIList = new List<NegotiationCardUI>();
    private NegotiationCardUI selectedCard;
    private bool cardsVisible = true;
    
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
            confirmButton.interactable = false;
        }
    }
    
    void Update()
    {
        // Verifica se algum menu está aberto
        CheckMenuStates();
        
        // Permite abrir o painel de status com a tecla E
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleStatusPanel();
        }
    }
    
    /// <summary>
    /// Verifica o estado dos menus e esconde/mostra as cartas
    /// </summary>
    private void CheckMenuStates()
    {
        bool shouldHideCards = false;
        
        // Verifica OptionsMenu
        if (OptionsMenu.Instance != null && OptionsMenu.Instance.IsMenuOpen())
        {
            shouldHideCards = true;
        }
        
        // Verifica StatusPanel
        if (StatusPanel.Instance != null && StatusPanel.Instance.IsOpen())
        {
            shouldHideCards = true;
        }
        
        // Atualiza visibilidade das cartas
        if (shouldHideCards && cardsVisible)
        {
            HideCards();
        }
        else if (!shouldHideCards && !cardsVisible)
        {
            ShowCards();
        }
    }
    
    /// <summary>
    /// Esconde as cartas
    /// </summary>
    private void HideCards()
    {
        if (cardsContainer != null)
        {
            cardsContainer.gameObject.SetActive(false);
            cardsVisible = false;
            Debug.Log("Cartas escondidas - menu aberto");
        }
    }
    
    /// <summary>
    /// Mostra as cartas
    /// </summary>
    private void ShowCards()
    {
        if (cardsContainer != null)
        {
            cardsContainer.gameObject.SetActive(true);
            cardsVisible = true;
            Debug.Log("Cartas visíveis - menu fechado");
        }
    }
    
    /// <summary>
    /// Abre/fecha o painel de status
    /// </summary>
    private void ToggleStatusPanel()
    {
        if (StatusPanel.Instance == null)
        {
            Debug.LogWarning("StatusPanel não encontrado!");
            return;
        }
        
        // Se o OptionsMenu está aberto, não faz nada
        if (OptionsMenu.Instance != null && OptionsMenu.Instance.IsMenuOpen())
        {
            Debug.Log("OptionsMenu está aberto - StatusPanel bloqueado");
            return;
        }
        
        // Toggle do status panel
        StatusPanel.Instance.TogglePanel();
        
        Debug.Log($"StatusPanel toggled - Aberto: {StatusPanel.Instance.IsOpen()}");
    }
    
    private void SetupUI(NegotiationEventSO negotiationEvent)
    {
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
        
        ClearCards();
        
        List<NegotiationCardSO> cards = negotiationEvent.GetRandomCards();
        
        if (cards.Count == 0)
        {
            Debug.LogError("Nenhuma carta foi gerada!");
            return;
        }
        
        Debug.Log($"Gerando {cards.Count} cartas de negociação");
        
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
        
        // Não permite seleção se algum menu está aberto
        if (StatusPanel.Instance != null && StatusPanel.Instance.IsOpen())
        {
            Debug.Log("StatusPanel está aberto - seleção bloqueada");
            return;
        }
        
        if (OptionsMenu.Instance != null && OptionsMenu.Instance.IsMenuOpen())
        {
            Debug.Log("OptionsMenu está aberto - seleção bloqueada");
            return;
        }
        
        AudioConstants.PlayButtonSelect();
        
        if (selectedCard != null && selectedCard != card)
        {
            selectedCard.SetSelected(false);
            Debug.Log($"Carta '{selectedCard.GetCardData().cardName}' desmarcada");
        }
        
        selectedCard = card;
        selectedCard.SetSelected(true);
        
        if (confirmButton != null)
        {
            confirmButton.interactable = true;
        }
        
        if (confirmButtonText != null)
        {
            confirmButtonText.text = "Confirmar Negociação";
        }
        
        Debug.Log($"✅ Carta selecionada: {card.GetCardData().cardName}");
    }
    
    private void ConfirmSelection()
    {
        if (selectedCard == null)
        {
            Debug.LogWarning("Nenhuma carta foi selecionada!");
            AudioConstants.PlayCannotSelect();
            return;
        }
        AudioConstants.PlayButtonSelect();
        
        NegotiationCardSO cardData = selectedCard.GetCardData();
        CardAttribute playerAttr = selectedCard.GetSelectedPlayerAttribute();
        CardAttribute enemyAttr = selectedCard.GetSelectedEnemyAttribute();
        int value = selectedCard.GetSelectedValue();
        
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