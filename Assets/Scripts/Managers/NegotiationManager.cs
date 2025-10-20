// Assets/Scripts/Negotiation/NegotiationManager.cs (UPDATED - With Refresh System)

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class NegotiationManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject refreshButtonPrefab; // NOVO: Prefab do botão de refresh
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button declineButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI infoText;
    
    [Header("Configuration")]
    [SerializeField] private int numberOfCards = 3;
    [SerializeField] private bool useDynamicCards = true; // Se false, usa SOs
    [SerializeField] private List<NegotiationCardSO> fallbackCards; // Cards SO para fallback
    
    [Header("Refresh Settings")]
    [SerializeField] private Color refreshUsedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    // Estado interno
    private List<GameObject> cardContainers = new List<GameObject>(); // Containers (carta + botão)
    private List<NegotiationCardUI> cardUIList = new List<NegotiationCardUI>();
    private List<GameObject> refreshButtonObjects = new List<GameObject>();
    private List<bool> refreshButtonUsed = new List<bool>();
    
    private NegotiationCardUI selectedCard;
    private List<DynamicNegotiationCard> currentDynamicCards = new List<DynamicNegotiationCard>();
    
    void Start()
    {
        DebugLog("=== NEGOTIATION MANAGER INICIANDO ===");
        
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClicked);
        
        if (declineButton != null)
            declineButton.onClick.AddListener(OnDeclineClicked);
        
        SetupNegotiation();
    }
    
    private void SetupNegotiation()
    {
        if (useDynamicCards)
        {
            SetupDynamicNegotiation();
        }
        else
        {
            SetupStaticNegotiation();
        }
        
        UpdateConfirmButton();
    }
    
    /// <summary>
    /// Configura negociação com cartas dinâmicas (sistema novo)
    /// </summary>
    private void SetupDynamicNegotiation()
    {
        if (DynamicNegotiationCardGenerator.Instance == null)
        {
            DebugLog("⚠️ DynamicNegotiationCardGenerator não encontrado! Usando cartas estáticas.");
            SetupStaticNegotiation();
            return;
        }
        
        // Processa observações e gera pool de ofertas
        DynamicNegotiationCardGenerator.Instance.ProcessObservations();
        
        // Verifica se há ofertas suficientes
        if (!DynamicNegotiationCardGenerator.Instance.HasEnoughOffers(numberOfCards))
        {
            int maxCards = DynamicNegotiationCardGenerator.Instance.GetMaxPossibleCards();
            
            if (maxCards == 0)
            {
                DebugLog("⚠️ Nenhuma oferta disponível! Usando cartas estáticas.");
                SetupStaticNegotiation();
                return;
            }
            
            DebugLog($"⚠️ Apenas {maxCards} ofertas disponíveis (pedido: {numberOfCards})");
            numberOfCards = maxCards;
        }
        
        // Gera cartas com matching inteligente
        currentDynamicCards = DynamicNegotiationCardGenerator.Instance.GenerateCards(numberOfCards);
        
        if (currentDynamicCards.Count == 0)
        {
            DebugLog("⚠️ Falha ao gerar cartas dinâmicas! Usando cartas estáticas.");
            SetupStaticNegotiation();
            return;
        }
        
        DebugLog($"✓ {currentDynamicCards.Count} cartas dinâmicas geradas");
        
        // Cria UI das cartas
        CreateDynamicCardUI();
    }
    
    /// <summary>
    /// Configura negociação com cartas estáticas (sistema antigo - fallback)
    /// </summary>
    private void SetupStaticNegotiation()
    {
        if (fallbackCards == null || fallbackCards.Count == 0)
        {
            DebugLog("⚠️ Nenhuma carta de fallback disponível!");
            return;
        }
        
        // Embaralha e pega N cartas
        List<NegotiationCardSO> shuffled = new List<NegotiationCardSO>(fallbackCards);
        ShuffleList(shuffled);
        
        int cardsToUse = Mathf.Min(numberOfCards, shuffled.Count);
        
        DebugLog($"Usando {cardsToUse} cartas estáticas (fallback)");
        
        for (int i = 0; i < cardsToUse; i++)
        {
            CreateStaticCardSlot(shuffled[i], i);
        }
    }
    
    /// <summary>
    /// NOVO: Cria UI para cartas dinâmicas com botões de refresh
    /// </summary>
    private void CreateDynamicCardUI()
    {
        ClearCards();
        
        refreshButtonUsed.Clear();
        
        for (int i = 0; i < currentDynamicCards.Count; i++)
        {
            CreateDynamicCardSlot(currentDynamicCards[i], i);
            refreshButtonUsed.Add(false);
        }
    }
    
    /// <summary>
    /// NOVO: Cria um slot com carta dinâmica + botão de refresh
    /// </summary>
    private void CreateDynamicCardSlot(DynamicNegotiationCard card, int index)
    {
        // Cria container vertical para carta + botão refresh
        GameObject containerObj = new GameObject($"CardSlot_{index}");
        containerObj.transform.SetParent(cardsContainer);
        containerObj.transform.localScale = Vector3.one;
        
        VerticalLayoutGroup verticalLayout = containerObj.AddComponent<VerticalLayoutGroup>();
        verticalLayout.childAlignment = TextAnchor.UpperCenter;
        verticalLayout.spacing = 10f;
        verticalLayout.childControlHeight = false;
        verticalLayout.childControlWidth = false;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.childForceExpandWidth = false;
        
        // Cria a carta
        GameObject cardObj = Instantiate(cardPrefab, containerObj.transform);
        cardObj.transform.localScale = Vector3.one;
        
        NegotiationCardUI cardUI = cardObj.GetComponent<NegotiationCardUI>();
        if (cardUI != null)
        {
            cardUI.SetupDynamic(card, this);
            cardUIList.Add(cardUI);
        }
        else
        {
            DebugLog("⚠️ NegotiationCardUI não encontrado no prefab!");
        }
        
        // Cria o botão de refresh
        if (refreshButtonPrefab != null)
        {
            GameObject refreshObj = Instantiate(refreshButtonPrefab, containerObj.transform);
            refreshObj.transform.localScale = Vector3.one;
            
            Button refreshBtn = refreshObj.GetComponent<Button>();
            if (refreshBtn != null)
            {
                int refreshIndex = index;
                refreshBtn.onClick.AddListener(() => OnRefreshClicked(refreshIndex));
                
                // Configura texto do botão
                TextMeshProUGUI btnText = refreshBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = "🔄 Refresh";
                }
            }
            
            refreshButtonObjects.Add(refreshObj);
        }
        
        cardContainers.Add(containerObj);
    }
    
    /// <summary>
    /// Cria um slot com carta estática (sem refresh)
    /// </summary>
    private void CreateStaticCardSlot(NegotiationCardSO card, int index)
    {
        GameObject cardObj = Instantiate(cardPrefab, cardsContainer);
        cardObj.transform.localScale = Vector3.one;
        
        NegotiationCardUI cardUI = cardObj.GetComponent<NegotiationCardUI>();
        if (cardUI != null)
        {
            cardUI.Setup(card, this);
            cardUIList.Add(cardUI);
        }
        
        cardContainers.Add(cardObj);
    }
    
    /// <summary>
    /// NOVO: Chamado quando um botão de refresh é clicado
    /// </summary>
    private void OnRefreshClicked(int slotIndex)
    {
        // Verifica se já foi usado
        if (refreshButtonUsed[slotIndex])
        {
            DebugLog($"Botão de refresh {slotIndex} já foi usado!");
            return;
        }
        
        DebugLog($"Refresh solicitado para slot {slotIndex}");
        
        // Marca como usado
        refreshButtonUsed[slotIndex] = true;
        
        // Desabilita visualmente o botão
        if (slotIndex < refreshButtonObjects.Count)
        {
            Button refreshBtn = refreshButtonObjects[slotIndex].GetComponent<Button>();
            if (refreshBtn != null)
            {
                refreshBtn.interactable = false;
                
                Image btnImage = refreshBtn.GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.color = refreshUsedColor;
                }
            }
        }
        
        // Gera nova carta
        RefreshCardSlot(slotIndex);
    }
    
    /// <summary>
    /// NOVO: Atualiza uma carta específica
    /// </summary>
    private void RefreshCardSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= currentDynamicCards.Count)
        {
            DebugLog($"⚠️ Índice de slot inválido: {slotIndex}");
            return;
        }
        
        // IMPORTANTE: Libera as ofertas da carta antiga de volta para a pool
        DynamicNegotiationCard oldCard = currentDynamicCards[slotIndex];
        if (oldCard != null && DynamicNegotiationCardGenerator.Instance != null)
        {
            DynamicNegotiationCardGenerator.Instance.ReleaseCardOffers(oldCard);
        }
        
        // Gera nova carta única
        DynamicNegotiationCard newCard = DynamicNegotiationCardGenerator.Instance.GenerateSingleCard();
        
        if (newCard == null)
        {
            DebugLog("⚠️ Não há mais cartas únicas disponíveis para refresh!");
            
            // Reverte o botão de refresh
            refreshButtonUsed[slotIndex] = false;
            if (slotIndex < refreshButtonObjects.Count)
            {
                Button refreshBtn = refreshButtonObjects[slotIndex].GetComponent<Button>();
                if (refreshBtn != null)
                {
                    refreshBtn.interactable = true;
                    Image btnImage = refreshBtn.GetComponent<Image>();
                    if (btnImage != null)
                    {
                        btnImage.color = Color.white;
                    }
                }
            }
            
            // Devolve as ofertas que acabamos de liberar
            if (oldCard != null && DynamicNegotiationCardGenerator.Instance != null)
            {
                // Re-marca como usadas já que não conseguimos substituir
                var generator = DynamicNegotiationCardGenerator.Instance;
                // Não há método público para isso, então apenas deixamos
            }
            
            return;
        }
        
        DebugLog($"Slot {slotIndex}: '{oldCard?.GetCardName()}' → '{newCard.GetCardName()}'");
        
        // Atualiza a lista interna
        currentDynamicCards[slotIndex] = newCard;
        
        // Atualiza a UI da carta
        if (slotIndex < cardUIList.Count)
        {
            NegotiationCardUI cardUI = cardUIList[slotIndex];
            if (cardUI != null)
            {
                cardUI.SetupDynamic(newCard, this);
            }
        }
        
        // Se a carta refreshada estava selecionada, desseleciona
        if (selectedCard != null && cardUIList.IndexOf(selectedCard) == slotIndex)
        {
            selectedCard.SetSelected(false);
            selectedCard = null;
            UpdateConfirmButton();
        }
    }
    
    /// <summary>
    /// Chamado quando uma carta é selecionada
    /// </summary>
    public void SelectCard(NegotiationCardUI card)
    {
        // Desseleciona carta anterior
        if (selectedCard != null)
        {
            selectedCard.SetSelected(false);
        }
        
        // Seleciona nova carta
        selectedCard = card;
        selectedCard.SetSelected(true);
        
        UpdateConfirmButton();
        
        DebugLog($"Carta selecionada: {GetSelectedCardName()}");
    }
    
    private string GetSelectedCardName()
    {
        if (selectedCard == null) return "Nenhuma";
        
        if (useDynamicCards)
        {
            return selectedCard.GetDynamicCardData()?.GetCardName() ?? "Desconhecida";
        }
        else
        {
            return selectedCard.GetCardData()?.cardName ?? "Desconhecida";
        }
    }
    
    private void OnConfirmClicked()
    {
        if (selectedCard == null)
        {
            DebugLog("⚠️ Nenhuma carta selecionada!");
            return;
        }
        
        DebugLog($"=== CONFIRMANDO NEGOCIAÇÃO ===");
        
        if (useDynamicCards)
        {
            ApplyDynamicCard(selectedCard);
        }
        else
        {
            ApplyStaticCard(selectedCard);
        }
        
        ReturnToMap();
    }
    
    private void OnDeclineClicked()
    {
        DebugLog("Negociação recusada - retornando ao mapa");
        ReturnToMap();
    }
    
    /// <summary>
    /// Aplica efeitos de uma carta dinâmica (ATUALIZADO para suportar skills específicas)
    /// </summary>
    private void ApplyDynamicCard(NegotiationCardUI cardUI)
    {
        DynamicNegotiationCard card = cardUI.GetDynamicCardData();
        
        if (card == null)
        {
            DebugLog("⚠️ Dados da carta dinâmica inválidos!");
            return;
        }
        
        CardAttribute playerAttr = cardUI.GetSelectedPlayerAttribute();
        CardAttribute enemyAttr = cardUI.GetSelectedEnemyAttribute();
        CardIntensity intensity = cardUI.GetSelectedIntensity();
        
        // Calcula valores reais aplicando o multiplicador aos valores base
        int playerValue = IntensityHelper.GetScaledValue(intensity, card.playerBenefit.value);
        int enemyValue = IntensityHelper.GetScaledValue(intensity, card.playerCost.value);
        
        DebugLog($"=== APLICANDO CARTA: {card.GetCardName()} ===");
        DebugLog($"Intensidade: {IntensityHelper.GetIntensityDisplayName(intensity)} ({IntensityHelper.GetMultiplier(intensity)}x)");
        
        // === APLICA VANTAGEM ===
        NegotiationOffer advantage = card.playerBenefit;
        
        // Verifica se é skill específica
        bool isSpecificSkill = advantage.HasData("isSpecificSkill") && advantage.GetData<bool>("isSpecificSkill");
        
        if (isSpecificSkill)
        {
            // Aplica modificação na skill específica
            DebugLog($"  Aplicando vantagem em SKILL ESPECÍFICA");
            NegotiationOfferApplier.ApplyOffer(advantage, playerValue);
        }
        else
        {
            // Aplica modificador geral
            DebugLog($"  Jogador: {playerAttr} {FormatValue(value)}");
            
            if (DifficultySystem.Instance != null)
            {
                DifficultySystem.Instance.Modifiers.ApplyModifier(playerAttr, value);
            }
        }
        
        // === APLICA DESVANTAGEM ===
        NegotiationOffer disadvantage = card.playerCost;
        
        bool isSpecificSkillCost = disadvantage.HasData("isSpecificSkill") && disadvantage.GetData<bool>("isSpecificSkill");
        
        if (isSpecificSkillCost)
        {
            // Aplica modificação na skill específica (custo)
            DebugLog($"  Aplicando desvantagem em SKILL ESPECÍFICA");
            NegotiationOfferApplier.ApplyOffer(disadvantage, value);
        }
        else
        {
            // Aplica modificador geral
            if (disadvantage.affectsPlayer)
            {
                // Debuff no jogador
                DebugLog($"  Jogador perde: {playerAttr} {FormatValue(value)}");
            }
            else
            {
                // Buff nos inimigos
                DebugLog($"  Inimigos ganham: {enemyAttr} {FormatValue(value)}");
            }
            
            if (DifficultySystem.Instance != null)
            {
                DifficultySystem.Instance.Modifiers.ApplyModifier(enemyAttr, value);
            }
        }
        
        DebugLog("=== NEGOCIAÇÃO APLICADA COM SUCESSO ===");
    }
    
    /// <summary>
    /// Aplica efeitos de uma carta estática
    /// </summary>
    private void ApplyStaticCard(NegotiationCardUI cardUI)
    {
        NegotiationCardSO card = cardUI.GetCardData();
        
        if (card == null)
        {
            DebugLog("⚠️ Dados da carta estática inválidos!");
            return;
        }
        
        CardAttribute playerAttr = cardUI.GetSelectedPlayerAttribute();
        CardAttribute enemyAttr = cardUI.GetSelectedEnemyAttribute();
        CardIntensity intensity = cardUI.GetSelectedIntensity();
        
        // Usa valores base separados para player e enemy
        int basePlayerValue = card.fixedPlayerValue;
        int baseEnemyValue = card.fixedEnemyValue;
        
        // Calcula valores reais aplicando o multiplicador aos valores base
        int playerValue = IntensityHelper.GetScaledValue(intensity, basePlayerValue);
        int enemyValue = IntensityHelper.GetScaledValue(intensity, baseEnemyValue);
        
        DebugLog($"Aplicando carta: {card.cardName}");
        DebugLog($"Intensidade: {IntensityHelper.GetIntensityDisplayName(intensity)} ({IntensityHelper.GetMultiplier(intensity)}x)");
        DebugLog($"  Jogador: {playerAttr} {FormatValue(playerValue)}");
        DebugLog($"  Inimigos: {enemyAttr} {FormatValue(enemyValue)}");
        
        if (DifficultySystem.Instance != null)
        {
            DifficultySystem.Instance.ApplyNegotiation(playerAttr, enemyAttr, value);
        }
    }
    
    private void UpdateConfirmButton()
    {
        if (confirmButton != null)
        {
            confirmButton.interactable = (selectedCard != null);
        }
    }
    
    private void ReturnToMap()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMap();
        }
        else
        {
            DebugLog("⚠️ GameManager não encontrado!");
        }
    }
    
    private void ClearCards()
    {
        foreach (GameObject container in cardContainers)
        {
            if (container != null)
                Destroy(container);
        }
        
        cardContainers.Clear();
        cardUIList.Clear();
        refreshButtonObjects.Clear();
        refreshButtonUsed.Clear();
        selectedCard = null;
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
    
    private string FormatValue(int value)
    {
        return value > 0 ? $"+{value}" : value.ToString();
    }
    
    private void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"<color=cyan>[NegotiationManager]</color> {message}");
        }
    }
    
    void OnDestroy()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveAllListeners();
        
        if (declineButton != null)
            declineButton.onClick.RemoveAllListeners();
    }
    
    
}