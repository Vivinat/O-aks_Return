// Assets/Scripts/Managers/ShopManager.cs

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("Data")]
    private ShopEventSO shopData;
    private const int MAX_PLAYER_ACTIONS = 4;

    [Header("UI References")]
    public Transform shopItemsContainer;
    public GameObject shopItemPrefab; // Prefab com ShopItemUI (mostra preço e usos)
    public Button exitButton;
    public TextMeshProUGUI exitButtonText;
    public TextMeshProUGUI coinsDisplay;

    [Header("Player Actions UI")]
    public GameObject playerActionsPanel;
    public GameObject playerActionSlotPrefab; // Prefab com ShopItemUI (para slots do jogador)

    [Header("Tooltip UI")]
    public TooltipUI tooltipUI;
    public RectTransform tooltipAnchor;

    [Header("Highlight Visuals")]
    public Color highlightColor = new Color(1f, 0.9f, 0.4f);
    public Color defaultColor = Color.white;
    public Color emptySlotColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
    public Color cantAffordColor = new Color(0.7f, 0.4f, 0.4f);

    [Header("Purchase State")]
    public GameObject purchaseInstructionPanel; // Painel que mostra "Escolha um slot"
    public TextMeshProUGUI purchaseInstructionText;

    // --- Estado Interno ---
    private List<BattleAction> playerActions;
    private BattleAction selectedShopItem;
    private int selectedShopItemIndex = -1;
    private int selectedPlayerSlotIndex = -1;
    private int shopItemPrice = 0;
    private bool hasPurchased = false; // Se o jogador comprou algo e precisa escolher slot

    // Listas para gerenciar os botões criados
    private List<GameObject> shopButtonObjects = new List<GameObject>();
    private List<GameObject> playerSlotObjects = new List<GameObject>();

    void Start()
    {
        Debug.Log("ShopManager: Iniciando...");
        
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

        Debug.Log($"Jogador tem {playerActions.Count} habilidades");

        if (tooltipUI != null)
            tooltipUI.Hide();

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitShop);

        if (purchaseInstructionPanel != null)
            purchaseInstructionPanel.SetActive(false);

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
        shopItemPrice = 0;
        hasPurchased = false;
        
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
        Debug.Log("Gerando itens da loja...");
        
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
        
        Debug.Log($"Geradas {selectedActions.Count} ações para a loja");
        
        for (int i = 0; i < selectedActions.Count; i++)
        {
            BattleAction action = selectedActions[i];
            if (action == null) continue;

            // NOVO: Garante que itens consumíveis à venda mostrem usos corretos
            if (action.isConsumable && action.currentUses <= 0)
            {
                action.currentUses = action.maxUses;
            }

            int buttonIndex = i;
            
            GameObject shopInstance = Instantiate(shopItemPrefab, shopItemsContainer);
            ShopItemUI shopButton = shopInstance.GetComponent<ShopItemUI>();
            
            if (shopButton == null)
            {
                Debug.LogError("ShopItemUI não encontrado no prefab!");
                continue;
            }
            
            // Setup usando ShopItemUI para itens à venda
            shopButton.SetupForSale(action, this);
            shopButtonObjects.Add(shopInstance);

            bool canAfford = GameManager.Instance.CurrencySystem.HasEnoughCoins(action.shopPrice);
            
            Button buttonComponent = shopInstance.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(() => OnShopItemSelected(action, buttonIndex));
                
                Image buttonImage = buttonComponent.GetComponent<Image>();
                if (buttonImage != null && !canAfford)
                {
                    buttonImage.color = cantAffordColor;
                }
            }
        }
    }

    private void PopulatePlayerActionsPanel()
    {
        Debug.Log($"Populando painel do jogador com {playerActions.Count} ações...");
        
        if (playerActionsPanel == null)
        {
            Debug.LogError("playerActionsPanel não foi atribuído!");
            return;
        }

        playerActionsPanel.SetActive(true);
        ClearPlayerSlots();

        for (int i = 0; i < MAX_PLAYER_ACTIONS; i++)
        {
            int slotIndex = i;
            
            GameObject slotInstance = Instantiate(playerActionSlotPrefab, playerActionsPanel.transform);
            ShopItemUI slotButton = slotInstance.GetComponent<ShopItemUI>();

            if (slotButton == null)
            {
                Debug.LogError("ShopItemUI não encontrado no prefab do slot!");
                continue;
            }

            if (i < playerActions.Count && playerActions[i] != null)
            {
                slotButton.SetupPlayerSlot(playerActions[i], this);
                Debug.Log($"Slot {i}: {playerActions[i].actionName}");
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
        if (buttonUI.iconImage != null)
        {
            buttonUI.iconImage.enabled = false;
        }
        
        // Esconde preço e usos para slots vazios
        if (buttonUI.priceText != null)
        {
            buttonUI.priceText.gameObject.SetActive(false);
        }
        
        if (buttonUI.usesText != null)
        {
            buttonUI.usesText.gameObject.SetActive(false);
        }
        
        Image buttonImage = buttonUI.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = emptySlotColor;
        }
        
        Debug.Log($"Slot {slotIndex}: vazio");
    }

    public void OnShopItemSelected(BattleAction action, int buttonIndex)
    {
        // Se já comprou algo, não pode selecionar outro item
        if (hasPurchased) return;

        if (!GameManager.Instance.CurrencySystem.HasEnoughCoins(action.shopPrice))
        {
            Debug.Log($"Não é possível comprar {action.actionName} - moedas insuficientes!");
            return;
        }

        // Compra imediatamente
        if (GameManager.Instance.CurrencySystem.SpendCoins(action.shopPrice))
        {
            selectedShopItem = action.CreateInstance(); // Cria instância com usos completos
            selectedShopItemIndex = buttonIndex;
            selectedPlayerSlotIndex = -1;
            hasPurchased = true;
            
            UpdateCoinsDisplay();
            UpdateShopHighlights();
            UpdatePlayerSlotHighlights();
            
            // Mostra instrução para escolher slot
            ShowSlotSelectionMode();
            
            Debug.Log($"Comprou {action.actionName} por {action.shopPrice} moedas! Escolha um slot.");
        }
    }

    private void ShowSlotSelectionMode()
    {
        if (exitButtonText != null)
            exitButtonText.text = "Confirmar";
            
        if (purchaseInstructionPanel != null)
        {
            purchaseInstructionPanel.SetActive(true);
            if (purchaseInstructionText != null)
            {
                purchaseInstructionText.text = $"Escolha onde colocar '{selectedShopItem.actionName}'";
            }
        }
    }

    public void OnPlayerSlotSelected(int slotIndex)
    {
        // Só permite seleção se comprou algo
        if (!hasPurchased || selectedShopItem == null) return;

        selectedPlayerSlotIndex = slotIndex;
        UpdatePlayerSlotHighlights();
        
        if (slotIndex >= playerActions.Count)
        {
            Debug.Log($"Slot vazio {slotIndex} selecionado para '{selectedShopItem.actionName}'");
        }
        else
        {
            string currentAction = playerActions[slotIndex]?.actionName ?? "vazio";
            Debug.Log($"Slot {slotIndex} selecionado para substituir '{currentAction}' por '{selectedShopItem.actionName}'");
        }
    }

    private void UpdateShopHighlights()
    {
        for (int i = 0; i < shopButtonObjects.Count; i++)
        {
            Image buttonImage = shopButtonObjects[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                bool isSelected = (i == selectedShopItemIndex);
                
                if (isSelected)
                {
                    buttonImage.color = highlightColor;
                }
                else
                {
                    // Se já comprou, deixa todos os outros itens indisponíveis
                    if (hasPurchased)
                    {
                        buttonImage.color = cantAffordColor;
                    }
                    else
                    {
                        ShopItemUI shopUI = shopButtonObjects[i].GetComponent<ShopItemUI>();
                        if (shopUI != null)
                        {
                            BattleAction action = shopUI.GetAction();
                            bool canAfford = GameManager.Instance.CurrencySystem.HasEnoughCoins(action.shopPrice);
                            buttonImage.color = canAfford ? defaultColor : cantAffordColor;
                        }
                        else
                        {
                            buttonImage.color = defaultColor;
                        }
                    }
                }
            }
        }
        
        // Desabilita cliques nos outros itens se já comprou
        if (hasPurchased)
        {
            for (int i = 0; i < shopButtonObjects.Count; i++)
            {
                if (i != selectedShopItemIndex)
                {
                    Button btn = shopButtonObjects[i].GetComponent<Button>();
                    if (btn != null) btn.interactable = false;
                }
            }
        }
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
                    if (i >= playerActions.Count)
                    {
                        buttonImage.color = emptySlotColor;
                    }
                    else
                    {
                        buttonImage.color = defaultColor;
                    }
                }
            }
        }
    }

    private void ClearShopButtons()
    {
        foreach (GameObject obj in shopButtonObjects)
        {
            if (obj != null) Destroy(obj);
        }
        shopButtonObjects.Clear();
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

        // Se comprou algo e selecionou um slot, confirma
        if (hasPurchased && selectedPlayerSlotIndex >= 0)
        {
            ProcessSlotAssignment();
        }
        
        EndShopEvent();
    }

    private void ProcessSlotAssignment()
    {
        if (selectedPlayerSlotIndex >= playerActions.Count)
        {
            // Adicionar a uma posição vazia
            while (playerActions.Count <= selectedPlayerSlotIndex)
            {
                playerActions.Add(null);
            }
            playerActions[selectedPlayerSlotIndex] = selectedShopItem;
            Debug.Log($"Adicionado '{selectedShopItem.actionName}' ao slot {selectedPlayerSlotIndex}");
        }
        else
        {
            // Substituir habilidade existente
            string oldAction = playerActions[selectedPlayerSlotIndex]?.actionName ?? "vazio";
            playerActions[selectedPlayerSlotIndex] = selectedShopItem;
            Debug.Log($"Substituído '{oldAction}' por '{selectedShopItem.actionName}' no slot {selectedPlayerSlotIndex}");
        }

        // Remove nulls da lista
        playerActions.RemoveAll(action => action == null);
    }

    private void EndShopEvent()
    {
        if (GameManager.Instance.PlayerCharacterInfo != null)
        {
            GameManager.Instance.PlayerCharacterInfo.battleActions = new List<BattleAction>(playerActions);
        }
        
        GameManager.Instance.ReturnToMap();
    }

    // --- MÉTODOS DO TOOLTIP (compatibilidade com RewardButtonUI) ---
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