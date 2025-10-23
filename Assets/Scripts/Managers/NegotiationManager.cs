// Assets/Scripts/Negotiation/NegotiationManager.cs (COMPLETE - FIXED)

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
    [SerializeField] private GameObject refreshButtonPrefab;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button declineButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI infoText;
    
    [Header("Configuration")]
    [SerializeField] private int numberOfCards = 3;
    [SerializeField] private bool useDynamicCards = true;
    [SerializeField] private List<NegotiationCardSO> fallbackCards;
    
    [Header("Refresh Settings")]
    [SerializeField] private Color refreshUsedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private List<GameObject> cardContainers = new List<GameObject>();
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
    
    private void SetupDynamicNegotiation()
    {
        if (DynamicNegotiationCardGenerator.Instance == null)
        {
            DebugLog("⚠️ DynamicNegotiationCardGenerator não encontrado! Usando cartas estáticas.");
            SetupStaticNegotiation();
            return;
        }
        
        DynamicNegotiationCardGenerator.Instance.ProcessObservations();
        
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
        
        currentDynamicCards = DynamicNegotiationCardGenerator.Instance.GenerateCards(numberOfCards);
        
        if (currentDynamicCards.Count == 0)
        {
            DebugLog("⚠️ Falha ao gerar cartas dinâmicas! Usando cartas estáticas.");
            SetupStaticNegotiation();
            return;
        }
        
        DebugLog($"✓ {currentDynamicCards.Count} cartas dinâmicas geradas");
        CreateDynamicCardUI();
    }
    
    private void SetupStaticNegotiation()
    {
        if (fallbackCards == null || fallbackCards.Count == 0)
        {
            DebugLog("⚠️ Nenhuma carta de fallback disponível!");
            return;
        }
        
        List<NegotiationCardSO> shuffled = new List<NegotiationCardSO>(fallbackCards);
        ShuffleList(shuffled);
        
        int cardsToUse = Mathf.Min(numberOfCards, shuffled.Count);
        
        DebugLog($"Usando {cardsToUse} cartas estáticas (fallback)");
        
        for (int i = 0; i < cardsToUse; i++)
        {
            CreateStaticCardSlot(shuffled[i], i);
        }
    }
    
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
    
    private void CreateDynamicCardSlot(DynamicNegotiationCard card, int index)
    {
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
        
        if (refreshButtonPrefab != null)
        {
            GameObject refreshObj = Instantiate(refreshButtonPrefab, containerObj.transform);
            refreshObj.transform.localScale = Vector3.one;
            
            Button refreshBtn = refreshObj.GetComponent<Button>();
            if (refreshBtn != null)
            {
                int refreshIndex = index;
                refreshBtn.onClick.AddListener(() => OnRefreshClicked(refreshIndex));
                
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
    
    private void OnRefreshClicked(int slotIndex)
    {
        if (refreshButtonUsed[slotIndex])
        {
            DebugLog($"Botão de refresh {slotIndex} já foi usado!");
            return;
        }
        
        DebugLog($"Refresh solicitado para slot {slotIndex}");
        
        refreshButtonUsed[slotIndex] = true;
        
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
        
        RefreshCardSlot(slotIndex);
    }
    
    private void RefreshCardSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= currentDynamicCards.Count)
        {
            DebugLog($"⚠️ Índice de slot inválido: {slotIndex}");
            return;
        }
        
        DynamicNegotiationCard oldCard = currentDynamicCards[slotIndex];
        if (oldCard != null && DynamicNegotiationCardGenerator.Instance != null)
        {
            DynamicNegotiationCardGenerator.Instance.ReleaseCardOffers(oldCard);
        }
        
        DynamicNegotiationCard newCard = DynamicNegotiationCardGenerator.Instance.GenerateSingleCard();
        
        if (newCard == null)
        {
            DebugLog("⚠️ Não há mais cartas únicas disponíveis para refresh!");
            
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
            
            return;
        }
        
        DebugLog($"Slot {slotIndex}: '{oldCard?.GetCardName()}' → '{newCard.GetCardName()}'");
        
        currentDynamicCards[slotIndex] = newCard;
        
        if (slotIndex < cardUIList.Count)
        {
            NegotiationCardUI cardUI = cardUIList[slotIndex];
            if (cardUI != null)
            {
                cardUI.SetupDynamic(newCard, this);
            }
        }
        
        if (selectedCard != null && cardUIList.IndexOf(selectedCard) == slotIndex)
        {
            selectedCard.SetSelected(false);
            selectedCard = null;
            UpdateConfirmButton();
        }
    }
    
    public void SelectCard(NegotiationCardUI card)
    {
        if (selectedCard != null)
        {
            selectedCard.SetSelected(false);
        }
        
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
    
// Assets/Scripts/Negotiation/NegotiationManager.cs

    /// <summary>
    /// ✅ CORRIGIDO: Aplica vantagem e desvantagem independentemente,
    /// permitindo misturar "Skill Específica" com "Modificador Geral".
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
        
        // ✅ CORREÇÃO FINAL: Força sinal correto para custos de mana
        playerValue = CorrectManaCostSign(playerAttr, playerValue, true);  // Vantagem
        enemyValue = CorrectManaCostSign(enemyAttr, enemyValue, false);    // Desvantagem
        
        DebugLog($"=== APLICANDO CARTA: {card.GetCardName()} ===");
        DebugLog($"Intensidade: {IntensityHelper.GetIntensityDisplayName(intensity)} ({IntensityHelper.GetMultiplier(intensity)}x)");
        
        // === 1. APLICA VANTAGEM ===
        NegotiationOffer advantage = card.playerBenefit;
        bool isSpecificSkillAdvantage = advantage.HasData("isSpecificSkill") && advantage.GetData<bool>("isSpecificSkill");
        
        if (isSpecificSkillAdvantage)
        {
            // Vantagem é SKILL ESPECÍFICA
            DebugLog($"  Aplicando VANTAGEM (Skill): {advantage.offerName}");
            NegotiationOfferApplier.ApplyOffer(advantage, playerValue);
        }
        else
        {
            // Vantagem é GERAL (Modificador de Atributo)
            DebugLog($"  Aplicando VANTAGEM (Geral): {playerAttr} {FormatValue(playerValue)}");
            if (DifficultySystem.Instance != null)
            {
                // Passa 0 para o lado do inimigo, pois esta é apenas a vantagem
                DifficultySystem.Instance.ApplyNegotiation(playerAttr, CardAttribute.EnemyMaxHP, playerValue, 0);
            }
        }
        
        // === 2. APLICA DESVANTAGEM ===
        NegotiationOffer disadvantage = card.playerCost;
        bool isSpecificSkillCost = disadvantage.HasData("isSpecificSkill") && disadvantage.GetData<bool>("isSpecificSkill");
        
        if (isSpecificSkillCost)
        {
            // Desvantagem é SKILL ESPECÍFICA
            DebugLog($"  Aplicando DESVANTAGEM (Skill): {disadvantage.offerName}");
            NegotiationOfferApplier.ApplyOffer(disadvantage, enemyValue);
        }
        else
        {
            // Desvantagem é GERAL (Modificador de Atributo)
            // (Este era o passo que estava faltando no seu log)
            DebugLog($"  Aplicando DESVANTAGEM (Geral): {enemyAttr} {FormatValue(enemyValue)}");
            if (DifficultySystem.Instance != null)
            {
                // Passa 0 para o lado do jogador, pois esta é apenas a desvantagem
                DifficultySystem.Instance.ApplyNegotiation(CardAttribute.PlayerMaxHP, enemyAttr, 0, enemyValue);
            }
        }
        
        DebugLog("=== NEGOCIAÇÃO APLICADA COM SUCESSO (IMEDIATA) ===");
    }

    
    /// <summary>
    /// ✅ MODIFICADO: ApplyStaticCard com correção de sinal
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
    
        int basePlayerValue = card.fixedPlayerValue;
        int baseEnemyValue = card.fixedEnemyValue;
    
        int playerValue = IntensityHelper.GetScaledValue(intensity, basePlayerValue);
        int enemyValue = IntensityHelper.GetScaledValue(intensity, baseEnemyValue);
    
        // ✅ CORREÇÃO FINAL: Força sinal correto para custos de mana
        playerValue = CorrectManaCostSign(playerAttr, playerValue, true);  // Vantagem
        enemyValue = CorrectManaCostSign(enemyAttr, enemyValue, false);    // Desvantagem
    
        DebugLog($"Aplicando carta: {card.cardName}");
        DebugLog($"Intensidade: {IntensityHelper.GetIntensityDisplayName(intensity)} ({IntensityHelper.GetMultiplier(intensity)}x)");
        DebugLog($"  Jogador: {playerAttr} {FormatValue(playerValue)}");
        DebugLog($"  Inimigos: {enemyAttr} {FormatValue(enemyValue)}");
    
        if (DifficultySystem.Instance != null)
        {
            DifficultySystem.Instance.ApplyNegotiation(playerAttr, enemyAttr, playerValue, enemyValue);
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
    
    /// <summary>
    /// ✅ CORREÇÃO FINAL: Garante sinal correto antes de aplicar modificadores
    /// </summary>
    private int CorrectManaCostSign(CardAttribute attribute, int value, bool isAdvantage)
    {
        // Se não for custo de mana, retorna valor original
        if (attribute != CardAttribute.PlayerActionManaCost && 
            attribute != CardAttribute.EnemyActionManaCost)
        {
            return value;
        }
    
        // === CUSTO DE MANA DO JOGADOR ===
        if (attribute == CardAttribute.PlayerActionManaCost)
        {
            if (isAdvantage)
            {
                // ✅ VANTAGEM: Reduzir custo = NEGATIVO
                int corrected = -Mathf.Abs(value);
                DebugLog($"  🔧 Correção PlayerManaCost (vantagem): {value} → {corrected}");
                return corrected;
            }
            else
            {
                // ❌ DESVANTAGEM: Aumentar custo = POSITIVO
                int corrected = Mathf.Abs(value);
                DebugLog($"  🔧 Correção PlayerManaCost (desvantagem): {value} → {corrected}");
                return corrected;
            }
        }
    
        // === CUSTO DE MANA DOS INIMIGOS ===
        if (attribute == CardAttribute.EnemyActionManaCost)
        {
            if (isAdvantage)
            {
                // ✅ VANTAGEM (para jogador): Aumentar custo inimigo = POSITIVO
                int corrected = Mathf.Abs(value);
                DebugLog($"  🔧 Correção EnemyManaCost (vantagem): {value} → {corrected}");
                return corrected;
            }
            else
            {
                // ❌ DESVANTAGEM: Reduzir custo inimigo = NEGATIVO
                int corrected = -Mathf.Abs(value);
                DebugLog($"  🔧 Correção EnemyManaCost (desvantagem): {value} → {corrected}");
                return corrected;
            }
        }
    
        return value;
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