// Assets/Scripts/Managers/NegotiationManager.cs (UPDATED - Dynamic Cards)

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Gerencia a cena de negociação usando cartas dinâmicas
/// </summary>
public class NegotiationManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI confirmButtonText;
    
    [Header("Dynamic Card Generation")]
    [SerializeField] private int numberOfCards = 3;
    
    [Header("Fallback - Se não houver observações suficientes")]
    [SerializeField] private NegotiationEventSO fallbackEvent;
    
    // Estado interno
    private List<NegotiationCardUI> cardUIList = new List<NegotiationCardUI>();
    private NegotiationCardUI selectedCard;
    private bool cardsVisible = true;
    private bool usingDynamicCards = false;
    
    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager não encontrado!");
            return;
        }
        
        SetupUI();
        GenerateAndDisplayCards();
        
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(ConfirmSelection);
            confirmButton.interactable = false;
        }
    }
    
    void Update()
    {
        CheckMenuStates();
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleStatusPanel();
        }
    }
    
    private void CheckMenuStates()
    {
        bool shouldHideCards = false;
        
        if (OptionsMenu.Instance != null && OptionsMenu.Instance.IsMenuOpen())
        {
            shouldHideCards = true;
        }
        
        if (StatusPanel.Instance != null && StatusPanel.Instance.IsOpen())
        {
            shouldHideCards = true;
        }
        
        if (shouldHideCards && cardsVisible)
        {
            HideCards();
        }
        else if (!shouldHideCards && !cardsVisible)
        {
            ShowCards();
        }
    }
    
    private void HideCards()
    {
        if (cardsContainer != null)
        {
            cardsContainer.gameObject.SetActive(false);
            cardsVisible = false;
            Debug.Log("Cartas escondidas - menu aberto");
        }
    }
    
    private void ShowCards()
    {
        if (cardsContainer != null)
        {
            cardsContainer.gameObject.SetActive(true);
            cardsVisible = true;
            Debug.Log("Cartas visíveis - menu fechado");
        }
    }
    
    private void ToggleStatusPanel()
    {
        if (StatusPanel.Instance == null)
        {
            Debug.LogWarning("StatusPanel não encontrado!");
            return;
        }
        
        if (OptionsMenu.Instance != null && OptionsMenu.Instance.IsMenuOpen())
        {
            Debug.Log("OptionsMenu está aberto - StatusPanel bloqueado");
            return;
        }
        
        StatusPanel.Instance.TogglePanel();
        Debug.Log($"StatusPanel toggled - Aberto: {StatusPanel.Instance.IsOpen()}");
    }
    
    private void SetupUI()
    {
        if (confirmButtonText != null)
        {
            confirmButtonText.text = "Selecione uma Carta";
        }
    }
    
    /// <summary>
    /// Gera cartas dinâmicas ou usa fallback
    /// </summary>
    private void GenerateAndDisplayCards()
    {
        if (cardsContainer == null || cardPrefab == null)
        {
            Debug.LogError("cardsContainer ou cardPrefab não configurados!");
            return;
        }
        
        ClearCards();
        
        // Tenta gerar cartas dinâmicas
        bool success = TryGenerateDynamicCards();
        
        if (!success)
        {
            Debug.LogWarning("Não foi possível gerar cartas dinâmicas - usando fallback");
            GenerateFallbackCards();
        }
    }
    
    /// <summary>
    /// Tenta gerar cartas dinâmicas a partir de observações
    /// </summary>
    private bool TryGenerateDynamicCards()
    {
        if (DynamicNegotiationCardGenerator.Instance == null)
        {
            Debug.LogWarning("DynamicNegotiationCardGenerator não encontrado!");
            return false;
        }
        
        // Processa observações
        DynamicNegotiationCardGenerator.Instance.ProcessObservations();
        
        // Verifica se há ofertas suficientes
        if (!DynamicNegotiationCardGenerator.Instance.HasEnoughOffers(numberOfCards))
        {
            int maxPossible = DynamicNegotiationCardGenerator.Instance.GetMaxPossibleCards();
            Debug.LogWarning($"Ofertas insuficientes. Máximo possível: {maxPossible}, Necessário: {numberOfCards}");
            return false;
        }
        
        // Gera cartas
        List<DynamicNegotiationCard> cards = DynamicNegotiationCardGenerator.Instance.GenerateCards(numberOfCards);
        
        if (cards.Count == 0)
        {
            Debug.LogWarning("Nenhuma carta dinâmica foi gerada!");
            return false;
        }
        
        Debug.Log($"✅ Gerando {cards.Count} cartas DINÂMICAS");
        
        // Cria UI para cada carta dinâmica
        foreach (var cardData in cards)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardsContainer);
            NegotiationCardUI cardUI = cardObj.GetComponent<NegotiationCardUI>();
            
            if (cardUI != null)
            {
                cardUI.SetupDynamic(cardData, this);
                cardUIList.Add(cardUI);
            }
        }
        
        usingDynamicCards = true;
        return true;
    }
    
    /// <summary>
    /// Gera cartas do fallback SO (sistema antigo)
    /// </summary>
    private void GenerateFallbackCards()
    {
        if (fallbackEvent == null)
        {
            Debug.LogError("Nenhum fallbackEvent configurado!");
            return;
        }
        
        List<NegotiationCardSO> cards = fallbackEvent.GetRandomCards();
        
        if (cards.Count == 0)
        {
            Debug.LogError("Nenhuma carta foi gerada do fallback!");
            return;
        }
        
        Debug.Log($"📋 Gerando {cards.Count} cartas de FALLBACK (SO)");
        
        foreach (var cardData in cards)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardsContainer);
            NegotiationCardUI cardUI = cardObj.GetComponent<NegotiationCardUI>();
            
            if (cardUI != null)
            {
                cardUI.Setup(cardData, this);
                cardUIList.Add(cardUI);
            }
        }
        
        usingDynamicCards = false;
    }
    
    public void SelectCard(NegotiationCardUI card)
    {
        if (card == null) return;
        
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
        
        Debug.Log($"✅ Carta selecionada");
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
        
        if (usingDynamicCards)
        {
            ProcessDynamicCard();
        }
        else
        {
            ProcessStaticCard();
        }
        
        ReturnToMap();
    }
    
    private void ProcessDynamicCard()
    {
        DynamicNegotiationCard cardData = selectedCard.GetDynamicCardData();
        CardAttribute playerAttr = selectedCard.GetSelectedPlayerAttribute();
        CardAttribute enemyAttr = selectedCard.GetSelectedEnemyAttribute();
        int value = selectedCard.GetSelectedValue();
        
        Debug.Log($"=== CARTA DINÂMICA CONFIRMADA ===");
        Debug.Log($"Tipo: {cardData.cardType}");
        Debug.Log($"Jogador: {playerAttr} = {value}");
        Debug.Log($"Inimigo: {enemyAttr} = {value}");
        
        // Aplica no sistema de dificuldade
        if (DifficultySystem.Instance != null)
        {
            // Extrai valores reais das ofertas (benefício e custo)
            int playerValue = cardData.playerBenefit.playerValue;
            int enemyValue = cardData.playerCost.enemyValue;
            int playerCostValue = cardData.playerCost.playerValue;
            
            // Se é tipo Fixed, usa os valores fixos
            if (cardData.cardType == NegotiationCardType.Fixed)
            {
                // Aplica benefício ao jogador
                if (playerValue != 0)
                {
                    DifficultySystem.Instance.ApplyNegotiation(playerAttr, CardAttribute.PlayerMaxHP, playerValue);
                }
                
                // Aplica custo ao jogador (se houver)
                if (playerCostValue != 0)
                {
                    DifficultySystem.Instance.ApplyNegotiation(
                        cardData.playerCost.playerAttribute, 
                        CardAttribute.PlayerMaxHP, 
                        playerCostValue
                    );
                }
                
                // Aplica buff nos inimigos
                if (enemyValue != 0)
                {
                    DifficultySystem.Instance.ApplyNegotiation(CardAttribute.PlayerMaxHP, enemyAttr, enemyValue);
                }
            }
            else
            {
                // Para IntensityOnly e AttributeAndIntensity, usa o valor selecionado
                // Benefício ao jogador
                DifficultySystem.Instance.ApplyNegotiation(playerAttr, CardAttribute.PlayerMaxHP, value);
                
                // Custo ao jogador ou buff nos inimigos
                if (playerCostValue != 0)
                {
                    // Se tem debuff no jogador
                    DifficultySystem.Instance.ApplyNegotiation(
                        cardData.playerCost.playerAttribute, 
                        CardAttribute.PlayerMaxHP, 
                        -value // Inverte porque é custo
                    );
                }
                
                if (enemyValue != 0 || cardData.playerCost.enemyValue != 0)
                {
                    // Se tem buff nos inimigos
                    DifficultySystem.Instance.ApplyNegotiation(CardAttribute.PlayerMaxHP, enemyAttr, value);
                }
            }
        }
        else
        {
            Debug.LogError("DifficultySystem não encontrado!");
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteNegotiationEvent(null, playerAttr, enemyAttr, value);
        }
    }
    
    private void ProcessStaticCard()
    {
        NegotiationCardSO cardData = selectedCard.GetCardData();
        CardAttribute playerAttr = selectedCard.GetSelectedPlayerAttribute();
        CardAttribute enemyAttr = selectedCard.GetSelectedEnemyAttribute();
        int value = selectedCard.GetSelectedValue();
        
        Debug.Log($"=== CARTA ESTÁTICA CONFIRMADA ===");
        Debug.Log($"Tipo: {cardData.cardType}");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteNegotiationEvent(cardData, playerAttr, enemyAttr, value);
        }
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