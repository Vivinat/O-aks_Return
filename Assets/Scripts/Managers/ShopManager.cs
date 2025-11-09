using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System.Collections;

public class ShopManager : MonoBehaviour
{
    private ShopEventSO shopData;
    private const int MAX_PLAYER_ACTIONS = 4;

    public Transform shopItemsContainer;
    public GameObject shopItemPrefab;
    public GameObject refreshButtonPrefab;
    public Button exitButton;
    public TextMeshProUGUI exitButtonText;
    public TextMeshProUGUI coinsDisplay;

    public GameObject playerActionsPanel;
    public GameObject playerActionSlotPrefab;

    public TooltipUI tooltipUI;
    public RectTransform tooltipAnchor;

    public Color highlightColor = new Color(1f, 0.9f, 0.4f);
    public Color defaultColor = Color.white;
    public Color emptySlotColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
    public Color cantAffordColor = new Color(0.7f, 0.4f, 0.4f);
    public Color refreshUsedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    
    public GameObject purchaseInstructionPanel;
    public TextMeshProUGUI purchaseInstructionText;

    private List<BattleAction> playerActions;
    private ShopItem selectedShopItem;
    private int selectedShopItemIndex = -1;
    private int selectedPlayerSlotIndex = -1;
    private bool hasPendingPurchase = false;
    private bool playerBoughtSomething = false;
    private int currentItemPrice = 0;

    private List<GameObject> shopButtonObjects = new List<GameObject>();
    private List<GameObject> playerSlotObjects = new List<GameObject>();
    private List<ShopItem> currentShopItems = new List<ShopItem>();
    private List<int> shopModifiedPrices = new List<int>();
    private List<GameObject> refreshButtonObjects = new List<GameObject>();
    private List<bool> refreshButtonUsed = new List<bool>();

    void Start()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (GameManager.Instance.CurrentEvent is ShopEventSO shop)
        {
            shopData = shop;
        }
        else
        {
            return;
        }

        playerActions = GameManager.Instance.PlayerBattleActions;
        if (playerActions == null)
        {
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
        
        currentShopItems.Clear();
        shopModifiedPrices.Clear();
        refreshButtonUsed.Clear();
        
        for (int i = 0; i < shopData.numberOfChoices; i++)
        {
            ShopItem item = GenerateRandomShopItem();
            if (item != null)
            {
                currentShopItems.Add(item);
                
                int displayPrice = item.Price;
                if (DifficultySystem.Instance != null)
                {
                    displayPrice = DifficultySystem.Instance.GetModifiedShopPrice(item.Price);
                }
                shopModifiedPrices.Add(displayPrice);
                refreshButtonUsed.Add(false);
            }
        }
        
        for (int i = 0; i < currentShopItems.Count; i++)
        {
            CreateShopItemSlot(currentShopItems[i], shopModifiedPrices[i], i);
        }
        
        UpdateShopItemStates();
    }
    
    private ShopItem GenerateRandomShopItem()
    {
        float roll = Random.value;
        
        if (roll < shopData.powerupChance && shopData.powerupsForSale != null && shopData.powerupsForSale.Count > 0)
        {
            int randomIndex = Random.Range(0, shopData.powerupsForSale.Count);
            return new ShopItem(shopData.powerupsForSale[randomIndex]);
        }
        else if (shopData.actionsForSale != null && shopData.actionsForSale.Count > 0)
        {
            int randomIndex = Random.Range(0, shopData.actionsForSale.Count);
            BattleAction action = shopData.actionsForSale[randomIndex];
            
            if (action.isConsumable && action.currentUses <= 0)
            {
                action.currentUses = action.maxUses;
            }
            
            return new ShopItem(action);
        }
        
        return null;
    }
    
    private void CreateShopItemSlot(ShopItem item, int displayPrice, int index)
    {
        GameObject containerObj = new GameObject($"ShopSlot_{index}");
        containerObj.transform.SetParent(shopItemsContainer);
        
        VerticalLayoutGroup verticalLayout = containerObj.AddComponent<VerticalLayoutGroup>();
        verticalLayout.childAlignment = TextAnchor.MiddleCenter;
        verticalLayout.spacing = 5f;
        verticalLayout.childControlHeight = false;
        verticalLayout.childControlWidth = false;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.childForceExpandWidth = false;

        GameObject shopInstance = Instantiate(shopItemPrefab, containerObj.transform);
        ShopItemUI shopButton = shopInstance.GetComponent<ShopItemUI>();
        
        if (shopButton != null)
        {
            shopButton.SetupForSale(item, this, displayPrice);
        }
        
        shopButtonObjects.Add(shopInstance);

        Button buttonComponent = shopInstance.GetComponent<Button>();
        if (buttonComponent != null)
        {
            int buttonIndex = index;
            buttonComponent.onClick.AddListener(() => OnShopItemSelected(item, buttonIndex, displayPrice));
        }

        if (refreshButtonPrefab != null)
        {
            GameObject refreshObj = Instantiate(refreshButtonPrefab, containerObj.transform);
            Button refreshBtn = refreshObj.GetComponent<Button>();
            
            if (refreshBtn != null)
            {
                int refreshIndex = index;
                refreshBtn.onClick.AddListener(() => OnRefreshClicked(refreshIndex));
                
                TextMeshProUGUI btnText = refreshBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = "Refresh";
                }
            }
            
            refreshButtonObjects.Add(refreshObj);
        }
    }
    
    private void OnRefreshClicked(int slotIndex)
    {
        if (refreshButtonUsed[slotIndex])
        {
            AudioConstants.PlayCannotSelect();
            return;
        }
        
        AudioConstants.PlayButtonSelect();
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
        
        RefreshShopSlot(slotIndex);
    }
    
    private void RefreshShopSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= currentShopItems.Count)
        {
            return;
        }
        
        ShopItem newItem = GenerateRandomShopItem();
        
        if (newItem == null)
        {
            AudioConstants.PlayCannotSelect();
            
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
        
        currentShopItems[slotIndex] = newItem;
        
        int newPrice = newItem.Price;
        if (DifficultySystem.Instance != null)
        {
            newPrice = DifficultySystem.Instance.GetModifiedShopPrice(newItem.Price);
        }
        shopModifiedPrices[slotIndex] = newPrice;
        
        if (slotIndex < shopButtonObjects.Count)
        {
            ShopItemUI shopButton = shopButtonObjects[slotIndex].GetComponent<ShopItemUI>();
            if (shopButton != null)
            {
                shopButton.SetupForSale(newItem, this, newPrice);
            }
            
            Button buttonComponent = shopButtonObjects[slotIndex].GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.RemoveAllListeners();
                int buttonIndex = slotIndex;
                buttonComponent.onClick.AddListener(() => OnShopItemSelected(newItem, buttonIndex, newPrice));
            }
        }
        
        if (selectedShopItemIndex == slotIndex)
        {
            CancelPendingPurchase();
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
    
    public void OnShopItemSelected(ShopItem item, int buttonIndex, int modifiedPrice)
    {
        if (hasPendingPurchase && selectedShopItemIndex == buttonIndex && item.type == ShopItem.ItemType.Powerup)
        {
            if (ConfirmPurchase())
            {
                ApplyPowerup(selectedShopItem.powerup);
                CompletePurchase();
                AudioConstants.PlayItemBuy();
            }
            return;
        }

        if (hasPendingPurchase)
        {
            CancelPendingPurchase();
        }

        if (!GameManager.Instance.CurrencySystem.HasEnoughCoins(modifiedPrice))
        {
            AudioConstants.PlayCannotSelect();
            return;
        }
    
        AudioConstants.PlayButtonSelect();

        selectedShopItem = item;
        selectedShopItemIndex = buttonIndex;
        selectedPlayerSlotIndex = -1;
        hasPendingPurchase = true;
        currentItemPrice = modifiedPrice;
    
        UpdateShopHighlights();
        UpdatePlayerSlotHighlights();
    
        if (item.type == ShopItem.ItemType.Powerup)
        {
            ShowPowerupConfirmation();
        }
        else
        {
            ShowSlotSelectionMode();
        }
    }
    
    private void ShowPowerupConfirmation()
    {
        if (purchaseInstructionPanel != null)
        {
            purchaseInstructionPanel.SetActive(true);
            if (purchaseInstructionText != null)
            {
                purchaseInstructionText.text = $"Comprar '{selectedShopItem.Name}' por {currentItemPrice} moedas?\n(Efeito aplicado imediatamente)\n\nClique novamente para confirmar";
            }
        }
        
        StartCoroutine(WaitForPowerupConfirmation());
    }
    
    private IEnumerator WaitForPowerupConfirmation()
    {
        float timeout = 0f;
        bool confirmed = false;
        
        while (timeout < 10f && !confirmed)
        {
            if (Input.GetMouseButtonDown(0))
            {
                confirmed = true;
            }
            
            timeout += Time.deltaTime;
            yield return null;
        }
        
        if (!confirmed)
        {
            CancelPendingPurchase();
        }
    }

    private void ShowSlotSelectionMode()
    {
        if (purchaseInstructionPanel != null)
        {
            purchaseInstructionPanel.SetActive(true);
            if (purchaseInstructionText != null)
            {
                purchaseInstructionText.text = $"Escolha um slot para '{selectedShopItem.Name}' (Custo: {currentItemPrice})";
            }
        }
    }

    public void OnPlayerSlotSelected(int slotIndex)
    {
        if (!hasPendingPurchase || selectedShopItem == null) return;
        
        if (selectedShopItem.type == ShopItem.ItemType.Powerup)
        {
            if (ConfirmPurchase())
            {
                ApplyPowerup(selectedShopItem.powerup);
                CompletePurchase();
                AudioConstants.PlayItemBuy();
            }
            return;
        }

        selectedPlayerSlotIndex = slotIndex;
        
        if (ConfirmPurchase())
        {
            ProcessSlotAssignment();
            CompletePurchase();
            AudioConstants.PlayItemBuy();
        }
    }
    
    private void ApplyPowerup(PowerupSO powerup)
    {
        if (powerup == null) return;
        
        Character playerChar = GameManager.Instance.PlayerCharacterInfo;
        if (playerChar != null)
        {
            powerup.ApplyToCharacter(playerChar);
        }
    }

    private bool ConfirmPurchase()
    {
        int price = currentItemPrice;
        
        if (GameManager.Instance.CurrencySystem.SpendCoins(price))
        {
            AudioConstants.PlayButtonSelect();
            return true;
        }
        else
        {
            AudioConstants.PlayCannotSelect();
            CancelPendingPurchase();
            return false;
        }
    }

    private void CompletePurchase()
    {
        int indexToRemove = selectedShopItemIndex;

        UpdateCoinsDisplay();
        
        if (selectedShopItem.type == ShopItem.ItemType.BattleAction)
        {
            RefreshPlayerSlotsDisplay();
        }
        
        playerBoughtSomething = true;
        
        if (selectedShopItem.type == ShopItem.ItemType.BattleAction)
        {
            BehaviorAnalysisIntegration.OnShopPurchase(selectedShopItem.battleAction);
        }
    
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
            if (i >= currentShopItems.Count || i >= shopModifiedPrices.Count || shopButtonObjects[i] == null) continue;

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
            if (obj != null)
            {
                if (obj.transform.parent != null && obj.transform.parent != shopItemsContainer)
                {
                    Destroy(obj.transform.parent.gameObject);
                }
                else
                {
                    Destroy(obj);
                }
            }
        }
        shopButtonObjects.Clear();
        
        foreach (GameObject obj in refreshButtonObjects)
        {
            if (obj != null) Destroy(obj);
        }
        refreshButtonObjects.Clear();
        
        currentShopItems.Clear();
        shopModifiedPrices.Clear();
        refreshButtonUsed.Clear();
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
                }
            }
        }
    
        List<BattleAction> shopActions = currentShopItems
            .Where(item => item.type == ShopItem.ItemType.BattleAction)
            .Select(item => item.battleAction)
            .ToList();
            
        BehaviorAnalysisIntegration.OnShopExit(shopActions);
    
        EndShopEvent();
    }

    private void ProcessSlotAssignment()
    {
        if (selectedShopItem.type != ShopItem.ItemType.BattleAction)
        {
            return;
        }
        
        if (selectedPlayerSlotIndex >= playerActions.Count)
        {
            while (playerActions.Count <= selectedPlayerSlotIndex)
            {
                playerActions.Add(null);
            }
        }
        
        playerActions[selectedPlayerSlotIndex] = selectedShopItem.battleAction;
        playerActions.RemoveAll(action => action == null);
    }
    
    private void RemoveShopItem(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= shopButtonObjects.Count)
        {
            return;
        }

        GameObject buttonToRemove = shopButtonObjects[itemIndex];

        if (buttonToRemove != null)
        {
            if (buttonToRemove.transform.parent != null && buttonToRemove.transform.parent != shopItemsContainer)
            {
                Destroy(buttonToRemove.transform.parent.gameObject);
            }
            else
            {
                buttonToRemove.SetActive(false);
            }
        }

        shopButtonObjects.RemoveAt(itemIndex);
        currentShopItems.RemoveAt(itemIndex);
        shopModifiedPrices.RemoveAt(itemIndex);
        
        if (itemIndex < refreshButtonObjects.Count)
        {
            refreshButtonObjects.RemoveAt(itemIndex);
        }
        if (itemIndex < refreshButtonUsed.Count)
        {
            refreshButtonUsed.RemoveAt(itemIndex);
        }

        UpdateShopButtonIndices();
        ForceShopRefresh();
    }

    private void UpdateShopButtonIndices()
    {
        for (int i = 0; i < shopButtonObjects.Count; i++)
        {
            Button button = shopButtonObjects[i].GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();

                int newIndex = i;
                ShopItem item = currentShopItems[newIndex];
                int modifiedPrice = shopModifiedPrices[newIndex];
                
                button.onClick.AddListener(() => OnShopItemSelected(item, newIndex, modifiedPrice));
            }
        }
        
        for (int i = 0; i < refreshButtonObjects.Count; i++)
        {
            Button refreshBtn = refreshButtonObjects[i].GetComponent<Button>();
            if (refreshBtn != null)
            {
                refreshBtn.onClick.RemoveAllListeners();
                int newIndex = i;
                refreshBtn.onClick.AddListener(() => OnRefreshClicked(newIndex));
            }
        }
    }

    private void ForceShopRefresh()
    {
        if (shopItemsContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(shopItemsContainer.GetComponent<RectTransform>());
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