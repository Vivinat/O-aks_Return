using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class NegotiationManager : MonoBehaviour
{
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject refreshButtonPrefab;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button declineButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI infoText;
    
    [SerializeField] private int numberOfCards = 3;
    [SerializeField] private bool useDynamicCards = true;
    [SerializeField] private List<NegotiationCardSO> fallbackCards;
    
    [SerializeField] private Color refreshUsedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    
    private List<GameObject> cardContainers = new List<GameObject>();
    private List<NegotiationCardUI> cardUIList = new List<NegotiationCardUI>();
    private List<GameObject> refreshButtonObjects = new List<GameObject>();
    private List<bool> refreshButtonUsed = new List<bool>();
    
    private NegotiationCardUI selectedCard;
    private List<DynamicNegotiationCard> currentDynamicCards = new List<DynamicNegotiationCard>();
    
    void Start()
    {
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
            SetupStaticNegotiation();
            return;
        }
        
        DynamicNegotiationCardGenerator.Instance.ProcessObservations();
        
        if (!DynamicNegotiationCardGenerator.Instance.HasEnoughOffers(numberOfCards))
        {
            int maxCards = DynamicNegotiationCardGenerator.Instance.GetMaxPossibleCards();
            
            if (maxCards == 0)
            {
                SetupStaticNegotiation();
                return;
            }
            
            numberOfCards = maxCards;
        }
        
        currentDynamicCards = DynamicNegotiationCardGenerator.Instance.GenerateCards(numberOfCards);
        
        if (currentDynamicCards.Count == 0)
        {
            SetupStaticNegotiation();
            return;
        }
        
        CreateDynamicCardUI();
    }
    
    private void SetupStaticNegotiation()
    {
        if (fallbackCards == null || fallbackCards.Count == 0)
        {
            return;
        }
        
        List<NegotiationCardSO> shuffled = new List<NegotiationCardSO>(fallbackCards);
        ShuffleList(shuffled);
        
        int cardsToUse = Mathf.Min(numberOfCards, shuffled.Count);
        
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
                    btnText.text = "Refresh";
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
            return;
        }
        
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
    }
    
    private void OnConfirmClicked()
    {
        if (selectedCard == null)
        {
            return;
        }
        
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
        ReturnToMap();
    }
    
    private void ApplyDynamicCard(NegotiationCardUI cardUI)
    {
        DynamicNegotiationCard card = cardUI.GetDynamicCardData();
        
        if (card == null)
        {
            return;
        }
        
        CardAttribute playerAttr = cardUI.GetSelectedPlayerAttribute();
        CardAttribute enemyAttr = cardUI.GetSelectedEnemyAttribute();
        CardIntensity intensity = cardUI.GetSelectedIntensity();
        
        int playerValue = IntensityHelper.GetScaledValue(intensity, card.playerBenefit.value);
        int enemyValue = IntensityHelper.GetScaledValue(intensity, card.playerCost.value);
        
        playerValue = CorrectManaCostSign(playerAttr, playerValue, true);
        enemyValue = CorrectManaCostSign(enemyAttr, enemyValue, false);
        
        NegotiationOffer advantage = card.playerBenefit;
        bool isSpecificSkillAdvantage = advantage.HasData("isSpecificSkill") && advantage.GetData<bool>("isSpecificSkill");
        
        if (isSpecificSkillAdvantage)
        {
            NegotiationOfferApplier.ApplyOffer(advantage, playerValue);
        }
        else
        {
            if (DifficultySystem.Instance != null)
            {
                DifficultySystem.Instance.ApplyNegotiation(playerAttr, CardAttribute.EnemyMaxHP, playerValue, 0);
            }
        }
        
        NegotiationOffer disadvantage = card.playerCost;
        bool isSpecificSkillCost = disadvantage.HasData("isSpecificSkill") && disadvantage.GetData<bool>("isSpecificSkill");
        
        if (isSpecificSkillCost)
        {
            NegotiationOfferApplier.ApplyOffer(disadvantage, enemyValue);
        }
        else
        {
            if (DifficultySystem.Instance != null)
            {
                DifficultySystem.Instance.ApplyNegotiation(CardAttribute.PlayerMaxHP, enemyAttr, 0, enemyValue);
            }
        }
    }
    
    private void ApplyStaticCard(NegotiationCardUI cardUI)
    {
        NegotiationCardSO card = cardUI.GetCardData();
    
        if (card == null)
        {
            return;
        }
    
        CardAttribute playerAttr = cardUI.GetSelectedPlayerAttribute();
        CardAttribute enemyAttr = cardUI.GetSelectedEnemyAttribute();
        CardIntensity intensity = cardUI.GetSelectedIntensity();
    
        int basePlayerValue = card.fixedPlayerValue;
        int baseEnemyValue = card.fixedEnemyValue;
    
        int playerValue = IntensityHelper.GetScaledValue(intensity, basePlayerValue);
        int enemyValue = IntensityHelper.GetScaledValue(intensity, baseEnemyValue);
    
        playerValue = CorrectManaCostSign(playerAttr, playerValue, true);
        enemyValue = CorrectManaCostSign(enemyAttr, enemyValue, false);
    
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
    
    private int CorrectManaCostSign(CardAttribute attribute, int value, bool isAdvantage)
    {
        if (attribute != CardAttribute.PlayerActionManaCost && 
            attribute != CardAttribute.EnemyActionManaCost)
        {
            return value;
        }
    
        if (attribute == CardAttribute.PlayerActionManaCost)
        {
            if (isAdvantage)
            {
                return -Mathf.Abs(value);
            }
            else
            {
                return Mathf.Abs(value);
            }
        }
    
        if (attribute == CardAttribute.EnemyActionManaCost)
        {
            if (isAdvantage)
            {
                return Mathf.Abs(value);
            }
            else
            {
                return -Mathf.Abs(value);
            }
        }
    
        return value;
    }
    
    void OnDestroy()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveAllListeners();
        
        if (declineButton != null)
            declineButton.onClick.RemoveAllListeners();
    }
}