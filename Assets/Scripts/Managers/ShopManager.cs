// Assets/Scripts/Managers/ShopManager.cs (FIXED)

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System.Collections;

public class ShopManager : MonoBehaviour
{
    [Header("Data")]
    private ShopEventSO shopData;
    private const int MAX_PLAYER_ACTIONS = 4;

    [Header("UI References")]
    public Transform shopItemsContainer;
    public GameObject shopItemPrefab;
    public Button exitButton;
    public TextMeshProUGUI exitButtonText;
    public TextMeshProUGUI coinsDisplay;

    [Header("Player Actions UI")]
    public GameObject playerActionsPanel;
    public GameObject playerActionSlotPrefab;

    [Header("Tooltip UI")]
    public TooltipUI tooltipUI;
    public RectTransform tooltipAnchor;

    [Header("Highlight Visuals")]
    public Color highlightColor = new Color(1f, 0.9f, 0.4f);
    public Color defaultColor = Color.white;
    public Color emptySlotColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
    public Color cantAffordColor = new Color(0.7f, 0.4f, 0.4f);
    
    [Header("Purchase State")]
    public GameObject purchaseInstructionPanel;
    public TextMeshProUGUI purchaseInstructionText;

    // --- Estado Interno ---
    private List<BattleAction> playerActions;
    private BattleAction selectedShopItem;
    private int selectedShopItemIndex = -1;
    private int selectedPlayerSlotIndex = -1;
    private bool hasPendingPurchase = false;
    
    private bool playerBoughtSomething = false;
    private int currentItemPrice = 0; // NOVO: Preço modificado do item selecionado

    // Listas para gerenciar os botões criados
    private List<GameObject> shopButtonObjects = new List<GameObject>();
    private List<GameObject> playerSlotObjects = new List<GameObject>();
    private List<BattleAction> shopActions = new List<BattleAction>();
    private List<int> shopModifiedPrices = new List<int>(); // NOVO: Armazena preços modificados

    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager não encontrado!");
            return;
        }

        if (GameManager.Instance.CurrentEvent is ShopEventSO shop)
        {
            shopData = shop;
        }
        else
        {
            Debug.LogError("Evento atual não é uma loja!");
            return;
        }

        playerActions = GameManager.Instance.PlayerBattleActions;
        if (playerActions == null)
        {
            Debug.LogError("PlayerBattleActions é null! Inicializando lista vazia.");
            playerActions = new List<BattleAction>();
        }

        if (tooltipUI != null)
            tooltipUI.Hide();

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitShop);

        if (purchaseInstructionPanel != null)
            purchaseInstructionPanel.SetActive(false);
        
        playerBoughtSomething = false;

        ResetState();
        UpdateCoinsDisplay();
        GenerateShopItems();
        PopulatePlayerActionsPanel();
    }

    private void ResetState()
    {
        selectedShopItem = null;
        selectedShopItemIndex = -1;
        selectedPlayerSlotIndex = -1;
        hasPendingPurchase = false;
        currentItemPrice = 0;
        
        if (exitButtonText != null)
            exitButtonText.text = "Sair";
            
        if (purchaseInstructionPanel != null)
            purchaseInstructionPanel.SetActive(false);
    }

    private void UpdateCoinsDisplay()
    {
        if (coinsDisplay != null && GameManager.Instance != null)
        {
            coinsDisplay.text = $"Moedas: {GameManager.Instance.CurrencySystem.CurrentCoins}";
        }
    }

    private void GenerateShopItems()
    {
        ClearShopButtons();
        
        if (shopData.actionsForSale == null || shopData.actionsForSale.Count == 0)
        {
            Debug.LogWarning("Nenhuma ação para venda encontrada!");
            return;
        }
        
        var selectedActions = shopData.actionsForSale
            .OrderBy(x => Random.value)
            .Take(shopData.numberOfChoices)
            .ToList();
        
        shopActions = selectedActions;
        shopModifiedPrices.Clear(); // NOVO: Limpa preços anteriores
        
        for (int i = 0; i < selectedActions.Count; i++)
        {
            BattleAction action = selectedActions[i];
            if (action == null) continue;

            if (action.isConsumable && action.currentUses <= 0)
            {
                action.currentUses = action.maxUses;
            }
            
            // NOVO: Calcula e armazena preço modificado
            int displayPrice = action.shopPrice;
            if (DifficultySystem.Instance != null)
            {
                displayPrice = DifficultySystem.Instance.GetModifiedShopPrice(action.shopPrice);
            }
            shopModifiedPrices.Add(displayPrice);

            int buttonIndex = i;
            
            GameObject shopInstance = Instantiate(shopItemPrefab, shopItemsContainer);
            ShopItemUI shopButton = shopInstance.GetComponent<ShopItemUI>();
            
            if (shopButton == null) continue;
            
            shopButton.SetupForSale(action, this, displayPrice);
            shopButtonObjects.Add(shopInstance);

            Button buttonComponent = shopInstance.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(() => OnShopItemSelected(action, buttonIndex, displayPrice));
            }
        }
        
        UpdateShopItemStates();
    }

    private void PopulatePlayerActionsPanel()
    {
        if (playerActionsPanel == null) return;
        playerActionsPanel.SetActive(true);
        ClearPlayerSlots();

        for (int i = 0; i < MAX_PLAYER_ACTIONS; i++)
        {
            int slotIndex = i;
            
            GameObject slotInstance = Instantiate(playerActionSlotPrefab, playerActionsPanel.transform);
            ShopItemUI slotButton = slotInstance.GetComponent<ShopItemUI>();

            if (slotButton == null) continue;

            if (i < playerActions.Count && playerActions[i] != null)
            {
                slotButton.SetupPlayerSlot(playerActions[i], this);
            }
            else
            {
                SetupEmptySlot(slotButton, slotIndex);
            }

            playerSlotObjects.Add(slotInstance);
            
            Button buttonComponent = slotInstance.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(() => OnPlayerSlotSelected(slotIndex));
            }
        }
    }

    private void SetupEmptySlot(ShopItemUI buttonUI, int slotIndex)
    {
        if (buttonUI.iconImage != null) buttonUI.iconImage.enabled = false;
        if (buttonUI.priceText != null) buttonUI.priceText.gameObject.SetActive(false);
        if (buttonUI.usesText != null) buttonUI.usesText.gameObject.SetActive(false);
        Image buttonImage = buttonUI.GetComponent<Image>();
        if (buttonImage != null) buttonImage.color = emptySlotColor;
    }

    public void OnShopItemSelected(BattleAction action, int buttonIndex, int modifiedPrice)
    {
        if (hasPendingPurchase)
        {
            CancelPendingPurchase();
        }

        if (!GameManager.Instance.CurrencySystem.HasEnoughCoins(modifiedPrice))
        {
            Debug.Log($"Moedas insuficientes para {action.actionName}!");
            AudioConstants.PlayCannotSelect();
            return;
        }
        AudioConstants.PlayButtonSelect();

        selectedShopItem = action.CreateInstance();
        selectedShopItemIndex = buttonIndex;
        selectedPlayerSlotIndex = -1;
        hasPendingPurchase = true;
        currentItemPrice = modifiedPrice;
        
        UpdateShopHighlights();
        UpdatePlayerSlotHighlights();
        
        ShowSlotSelectionMode();
    }

    private void ShowSlotSelectionMode()
    {
        if (purchaseInstructionPanel != null)
        {
            purchaseInstructionPanel.SetActive(true);
            if (purchaseInstructionText != null)
            {
                purchaseInstructionText.text = $"Escolha um slot para '{selectedShopItem.actionName}' (Custo: {currentItemPrice})";
            }
        }
    }

    public void OnPlayerSlotSelected(int slotIndex)
    {
        if (!hasPendingPurchase || selectedShopItem == null) return;

        selectedPlayerSlotIndex = slotIndex;
        
        if (ConfirmPurchase())
        {
            ProcessSlotAssignment();
            CompletePurchase();
            AudioConstants.PlayItemBuy();
        }
    }

    private bool ConfirmPurchase()
    {
        int price = currentItemPrice;
        
        if (GameManager.Instance.CurrencySystem.SpendCoins(price))
        {
            AudioConstants.PlayButtonSelect();
            Debug.Log($"Compra de {selectedShopItem.actionName} por {price} moedas confirmada!");
            return true;
        }
        else
        {
            AudioConstants.PlayCannotSelect();
            Debug.Log("Falha na compra - verificação final de moedas falhou!");
            CancelPendingPurchase();
            return false;
        }
    }

    private void CompletePurchase()
    {
        int indexToRemove = selectedShopItemIndex;

        UpdateCoinsDisplay();
        RefreshPlayerSlotsDisplay();
        
        playerBoughtSomething = true;
    
        BehaviorAnalysisIntegration.OnShopPurchase(selectedShopItem);
    
        selectedShopItem = null;
        selectedShopItemIndex = -1;
        selectedPlayerSlotIndex = -1;
        hasPendingPurchase = false;
        currentItemPrice = 0;
    
        if (purchaseInstructionPanel != null)
            purchaseInstructionPanel.SetActive(false);
    
        if (indexToRemove != -1)
        {
            RemoveShopItem(indexToRemove);
        }
    }
    
    private void CancelPendingPurchase()
    {
        selectedShopItem = null;
        selectedShopItemIndex = -1;
        selectedPlayerSlotIndex = -1;
        hasPendingPurchase = false;
        currentItemPrice = 0;
        
        if (purchaseInstructionPanel != null)
            purchaseInstructionPanel.SetActive(false);
        
        UpdateShopHighlights();
        UpdatePlayerSlotHighlights();
        
        AudioConstants.PlayCannotSelect();
    }

    private void UpdateShopItemStates()
    {
        for (int i = 0; i < shopButtonObjects.Count; i++)
        {
            if (i >= shopActions.Count || i >= shopModifiedPrices.Count || shopButtonObjects[i] == null) continue;

            Button button = shopButtonObjects[i].GetComponent<Button>();
            Image buttonImage = shopButtonObjects[i].GetComponent<Image>();
            
            if (button == null || buttonImage == null) continue;
            
            bool canAfford = GameManager.Instance.CurrencySystem.HasEnoughCoins(shopModifiedPrices[i]);
            bool isSelected = (i == selectedShopItemIndex && hasPendingPurchase);
            
            if (isSelected)
            {
                buttonImage.color = highlightColor;
            }
            else if (!canAfford)
            {
                buttonImage.color = cantAffordColor;
            }
            else
            {
                buttonImage.color = defaultColor;
            }
            button.interactable = true;
        }
    }

    private void UpdateShopHighlights()
    {
        UpdateShopItemStates();
    }

    private void UpdatePlayerSlotHighlights()
    {
        for (int i = 0; i < playerSlotObjects.Count; i++)
        {
            Image buttonImage = playerSlotObjects[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                bool isSelected = (i == selectedPlayerSlotIndex);
                
                if (isSelected)
                {
                    buttonImage.color = highlightColor;
                }
                else
                {
                    buttonImage.color = (i >= playerActions.Count) ? emptySlotColor : defaultColor;
                }
            }
        }
    }

    private void RefreshPlayerSlotsDisplay()
    {
        ClearPlayerSlots();
        PopulatePlayerActionsPanel();
    }

    private void ClearShopButtons()
    {
        foreach (GameObject obj in shopButtonObjects)
        {
            if (obj != null) Destroy(obj);
        }
        shopButtonObjects.Clear();
        shopActions.Clear();
        shopModifiedPrices.Clear(); // NOVO: Limpa preços também
    }

    private void ClearPlayerSlots()
    {
        foreach (GameObject obj in playerSlotObjects)
        {
            if (obj != null) Destroy(obj);
        }
        playerSlotObjects.Clear();
    }

    public void ExitShop()
    {
        if (tooltipUI != null)
            tooltipUI.Hide();

        if (hasPendingPurchase)
        {
            CancelPendingPurchase();
        }
        
        if (playerBoughtSomething && GameManager.Instance?.CurrencySystem != null)
        {
            int coinsLeft = GameManager.Instance.CurrencySystem.CurrentCoins;
        
            if (coinsLeft < 30)
            {
                float percentageSpent = 0.8f;
            
                if (PlayerBehaviorAnalyzer.Instance != null)
                {
                    var observation = new BehaviorObservation(
                        BehaviorTriggerType.BrokeAfterShopping, 
                        UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
                    );
                    observation.SetData("percentageSpent", percentageSpent);
                    observation.SetData("coinsLeft", coinsLeft);
                
                    PlayerBehaviorAnalyzer.Instance.AddObservationDirectly(observation);
                
                    Debug.Log($"BrokeAfterShopping detectado: Restam apenas {coinsLeft} moedas");
                }
            }
        }
    
        BehaviorAnalysisIntegration.OnShopExit(shopActions);
    
        EndShopEvent();
    }

    private void ProcessSlotAssignment()
    {
        if (selectedPlayerSlotIndex >= playerActions.Count)
        {
            while (playerActions.Count <= selectedPlayerSlotIndex)
            {
                playerActions.Add(null);
            }
        }
        string oldActionName = playerActions[selectedPlayerSlotIndex]?.actionName ?? "vazio";
        playerActions[selectedPlayerSlotIndex] = selectedShopItem;
        Debug.Log($"Slot {selectedPlayerSlotIndex} ('{oldActionName}') substituído por '{selectedShopItem.actionName}'.");

        playerActions.RemoveAll(action => action == null);
    }
    
    /// <summary>
    /// Remove um item da loja de forma mais robusta.
    /// </summary>
    private void RemoveShopItem(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= shopButtonObjects.Count)
        {
            Debug.LogError($"Índice inválido para remoção: {itemIndex}");
            return;
        }

        GameObject buttonToRemove = shopButtonObjects[itemIndex];
        string itemName = shopActions[itemIndex].actionName;
        
        Debug.Log($"Iniciando remoção do item: {itemName} (índice {itemIndex})");

        if (buttonToRemove != null)
        {
            buttonToRemove.SetActive(false);
            Debug.Log($"Botão do {itemName} desativado.");
        }

        shopButtonObjects.RemoveAt(itemIndex);
        shopActions.RemoveAt(itemIndex);
        shopModifiedPrices.RemoveAt(itemIndex); // NOVO: Remove preço também
        Debug.Log($"Item {itemName} removido das listas de dados.");

        if (buttonToRemove != null)
        {
            StartCoroutine(DestroyButtonDelayed(buttonToRemove, itemName));
        }

        UpdateShopButtonIndices();
        ForceShopRefresh();
        
        Debug.Log($"Remoção de {itemName} concluída. Restam {shopButtonObjects.Count} itens na loja.");
    }

    private IEnumerator DestroyButtonDelayed(GameObject buttonToDestroy, string itemName)
    {
        yield return new WaitForEndOfFrame();
        
        if (buttonToDestroy != null)
        {
            Destroy(buttonToDestroy);
            Debug.Log($"GameObject do botão de {itemName} destruído com sucesso.");
        }
        
        yield return new WaitForEndOfFrame();
        ForceShopRefresh();
    }

    /// <summary>
    /// CORRIGIDO: Atualiza os listeners dos botões com os novos índices E preços corretos
    /// </summary>
    private void UpdateShopButtonIndices()
    {
        for (int i = 0; i < shopButtonObjects.Count; i++)
        {
            Button button = shopButtonObjects[i].GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();

                int newIndex = i;
                BattleAction action = shopActions[newIndex];
                int modifiedPrice = shopModifiedPrices[newIndex]; // NOVO: Pega o preço da lista
                
                // CORRIGIDO: Passa os 3 argumentos
                button.onClick.AddListener(() => OnShopItemSelected(action, newIndex, modifiedPrice));
            }
        }
        Debug.Log("Índices dos botões da loja foram atualizados.");
    }

    private void ForceShopRefresh()
    {
        if (shopItemsContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(shopItemsContainer.GetComponent<RectTransform>());
            Debug.Log("Layout da loja atualizado.");
        }
    }

    private void EndShopEvent()
    {
        if (GameManager.Instance.PlayerCharacterInfo != null)
        {
            GameManager.Instance.PlayerCharacterInfo.battleActions = new List<BattleAction>(playerActions);
        }
        GameManager.Instance.ReturnToMap();
    }

    public void ShowTooltip(string name, string description)
    {
        if (tooltipUI != null && tooltipAnchor != null)
        {
            tooltipUI.Show(name, description, tooltipAnchor.position);
        }
    }

    public void HideTooltip()
    {
        if (tooltipUI != null)
        {
            tooltipUI.Hide();
        }
    }
}