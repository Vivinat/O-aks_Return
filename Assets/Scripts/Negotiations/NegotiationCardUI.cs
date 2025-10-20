// Assets/Scripts/Negotiation/NegotiationCardUI.cs (COMPLETE - FIXED)

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class NegotiationCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Card Visual")]
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    [Header("Dropdowns")]
    [SerializeField] private GameObject playerAttributePanel;
    [SerializeField] private TMP_Dropdown playerAttributeDropdown;
    
    [SerializeField] private GameObject enemyAttributePanel;
    [SerializeField] private TMP_Dropdown enemyAttributeDropdown;
    
    [SerializeField] private GameObject intensityPanel;
    [SerializeField] private TMP_Dropdown intensityDropdown;
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color selectedColor = Color.red;
    [SerializeField] private float colorTransitionSpeed = 8f;
    
    // Dados - pode ser SO ou dinâmico
    private NegotiationCardSO cardDataSO;
    private DynamicNegotiationCard cardDataDynamic;
    private bool isDynamicCard = false;
    
    private NegotiationManager negotiationManager;
    private bool isSelected = false;
    private bool isHovered = false;
    private Color targetColor;
    
    // Valores selecionados
    private CardAttribute selectedPlayerAttribute;
    private CardAttribute selectedEnemyAttribute;
    private CardIntensity selectedIntensity;
    
    void Awake()
    {
        targetColor = normalColor;
        
        if (playerAttributeDropdown != null)
            playerAttributeDropdown.onValueChanged.AddListener(OnPlayerAttributeChanged);
        
        if (enemyAttributeDropdown != null)
            enemyAttributeDropdown.onValueChanged.AddListener(OnEnemyAttributeChanged);
        
        if (intensityDropdown != null)
            intensityDropdown.onValueChanged.AddListener(OnIntensityChanged);
    }
    
    void Update()
    {
        if (cardImage != null)
        {
            cardImage.color = Color.Lerp(
                cardImage.color,
                targetColor,
                Time.unscaledDeltaTime * colorTransitionSpeed
            );
        }
    }
    
    /// <summary>
    /// Setup para cartas estáticas (SO)
    /// </summary>
    public void Setup(NegotiationCardSO card, NegotiationManager manager)
    {
        cardDataSO = card;
        negotiationManager = manager;
        isDynamicCard = false;
        
        if (cardDataSO == null)
        {
            Debug.LogError("NegotiationCardUI: Dados da carta SO são nulos!");
            return;
        }
        
        if (cardImage != null && cardDataSO.cardSprite != null)
            cardImage.sprite = cardDataSO.cardSprite;
        
        SetupByType(cardDataSO.cardType);
        UpdateVisuals();
        UpdateDescription();
    }
    
    /// <summary>
    /// Setup para cartas dinâmicas
    /// </summary>
    public void SetupDynamic(DynamicNegotiationCard card, NegotiationManager manager)
    {
        cardDataDynamic = card;
        negotiationManager = manager;
        isDynamicCard = true;
        
        if (cardDataDynamic == null)
        {
            Debug.LogError("NegotiationCardUI: Dados da carta dinâmica são nulos!");
            return;
        }
        
        if (cardImage != null && cardDataDynamic.cardSprite != null)
            cardImage.sprite = cardDataDynamic.cardSprite;
        
        SetupByType(cardDataDynamic.cardType);
        UpdateVisuals();
        UpdateDescription();
    }
    
    private void SetupByType(NegotiationCardType type)
    {
        // Esconde tudo primeiro
        if (playerAttributePanel != null) playerAttributePanel.SetActive(false);
        if (enemyAttributePanel != null) enemyAttributePanel.SetActive(false);
        if (intensityPanel != null) intensityPanel.SetActive(false);
        
        switch (type)
        {
            case NegotiationCardType.Fixed:
                SetupFixed();
                break;
            
            case NegotiationCardType.IntensityOnly:
                SetupIntensityOnly();
                break;
            
            case NegotiationCardType.AttributeAndIntensity:
                SetupAttributeAndIntensity();
                break;
        }
    }
    
    private void SetupFixed()
    {
        if (isDynamicCard)
        {
            selectedPlayerAttribute = cardDataDynamic.playerBenefit.targetAttribute;
            selectedEnemyAttribute = cardDataDynamic.playerCost.targetAttribute;
            selectedIntensity = CardIntensity.Low;
        }
        else
        {
            selectedPlayerAttribute = cardDataSO.fixedPlayerAttribute;
            selectedEnemyAttribute = cardDataSO.fixedEnemyAttribute;
            selectedIntensity = CardIntensity.Low;
        }
    }
    
    private void SetupIntensityOnly()
    {
        if (intensityPanel != null)
        {
            intensityPanel.SetActive(true);
            PopulateIntensityDropdown();
        }
        
        if (isDynamicCard)
        {
            selectedPlayerAttribute = cardDataDynamic.playerBenefit.targetAttribute;
            selectedEnemyAttribute = cardDataDynamic.playerCost.targetAttribute;
        }
        else
        {
            selectedPlayerAttribute = cardDataSO.intensityOnlyPlayerAttribute;
            selectedEnemyAttribute = cardDataSO.intensityOnlyEnemyAttribute;
        }
    }
    
    private void SetupAttributeAndIntensity()
    {
        if (playerAttributePanel != null)
        {
            playerAttributePanel.SetActive(true);
            PopulatePlayerAttributeDropdown();
        }
        
        if (enemyAttributePanel != null)
        {
            enemyAttributePanel.SetActive(true);
            PopulateEnemyAttributeDropdown();
        }
        
        if (intensityPanel != null)
        {
            intensityPanel.SetActive(true);
            PopulateIntensityDropdown();
        }
    }
    
    private void PopulatePlayerAttributeDropdown()
    {
        if (playerAttributeDropdown == null) return;
        
        playerAttributeDropdown.ClearOptions();
        List<string> options = new List<string>();
        
        List<CardAttribute> availableAttrs = isDynamicCard 
            ? cardDataDynamic.availablePlayerAttributes 
            : cardDataSO.availablePlayerAttributes;
        
        foreach (var attr in availableAttrs)
        {
            options.Add(AttributeHelper.GetDisplayName(attr));
        }
        
        playerAttributeDropdown.AddOptions(options);
        
        if (availableAttrs.Count > 0)
        {
            selectedPlayerAttribute = availableAttrs[0];
            playerAttributeDropdown.value = 0;
        }
    }
    
    private void PopulateEnemyAttributeDropdown()
    {
        if (enemyAttributeDropdown == null) return;
        
        enemyAttributeDropdown.ClearOptions();
        List<string> options = new List<string>();
        
        List<CardAttribute> availableAttrs = isDynamicCard 
            ? cardDataDynamic.availableEnemyAttributes 
            : cardDataSO.availableEnemyAttributes;
        
        foreach (var attr in availableAttrs)
        {
            options.Add(AttributeHelper.GetDisplayName(attr));
        }
        
        enemyAttributeDropdown.AddOptions(options);
        
        if (availableAttrs.Count > 0)
        {
            selectedEnemyAttribute = availableAttrs[0];
            enemyAttributeDropdown.value = 0;
        }
    }
    
    private void PopulateIntensityDropdown()
    {
        if (intensityDropdown == null) return;
    
        intensityDropdown.ClearOptions();
        List<string> options = new List<string>();
    
        List<CardIntensity> availableIntensities = isDynamicCard 
            ? cardDataDynamic.availableIntensities 
            : cardDataSO.availableIntensities;
    
        foreach (var intensity in availableIntensities)
        {
            // APENAS o nome da intensidade - sem valores
            options.Add(IntensityHelper.GetIntensityDisplayName(intensity));
        }
    
        intensityDropdown.AddOptions(options);
    
        if (availableIntensities.Count > 0)
        {
            selectedIntensity = availableIntensities[0];
            intensityDropdown.value = 0;
        }
    }
    
    private void OnPlayerAttributeChanged(int index)
    {
        List<CardAttribute> availableAttrs = isDynamicCard 
            ? cardDataDynamic.availablePlayerAttributes 
            : cardDataSO.availablePlayerAttributes;
    
        if (availableAttrs.Count > index)
        {
            selectedPlayerAttribute = availableAttrs[index];
            UpdateDescription();
        }
    }

    private void OnEnemyAttributeChanged(int index)
    {
        List<CardAttribute> availableAttrs = isDynamicCard 
            ? cardDataDynamic.availableEnemyAttributes 
            : cardDataSO.availableEnemyAttributes;
    
        if (availableAttrs.Count > index)
        {
            selectedEnemyAttribute = availableAttrs[index];
            UpdateDescription();
        }
    }
    
    private void OnIntensityChanged(int index)
    {
        List<CardIntensity> availableIntensities = isDynamicCard 
            ? cardDataDynamic.availableIntensities 
            : cardDataSO.availableIntensities;
        
        if (availableIntensities.Count > index)
        {
            selectedIntensity = availableIntensities[index];
            UpdateDescription();
        }
    }
    
    private void UpdateDescription()
    {
        if (descriptionText == null) return;
    
        if (isDynamicCard)
        {
            descriptionText.text = cardDataDynamic.GetFullDescription(
                selectedPlayerAttribute, 
                selectedEnemyAttribute, 
                selectedIntensity
            );
        }
        else
        {
            descriptionText.text = cardDataSO.GetFullDescription(
                selectedPlayerAttribute, 
                selectedEnemyAttribute, 
                selectedIntensity
            );
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected)
        {
            isHovered = true;
            UpdateVisuals();
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateVisuals();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (negotiationManager != null)
        {
            negotiationManager.SelectCard(this);
        }
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisuals();
    }
    
    private void UpdateVisuals()
    {
        if (isSelected)
        {
            targetColor = selectedColor;
        }
        else if (isHovered)
        {
            targetColor = hoverColor;
        }
        else
        {
            targetColor = normalColor;
        }
    }
    
    // Getters
    public NegotiationCardSO GetCardData() => cardDataSO;
    public DynamicNegotiationCard GetDynamicCardData() => cardDataDynamic;
    public CardAttribute GetSelectedPlayerAttribute() => selectedPlayerAttribute;
    public CardAttribute GetSelectedEnemyAttribute() => selectedEnemyAttribute;
    public CardIntensity GetSelectedIntensity() => selectedIntensity;
    
    /// <summary>
    /// Retorna o valor final calculado (base * multiplicador)
    /// </summary>
    public int GetSelectedValue()
    {
        if (isDynamicCard && cardDataDynamic != null)
        {
            return IntensityHelper.GetScaledValue(selectedIntensity, cardDataDynamic.playerBenefit.value);
        }
        else if (!isDynamicCard && cardDataSO != null)
        {
            // Para cartas estáticas, usa o valor base apropriado baseado no tipo
            int baseValue = cardDataSO.cardType switch
            {
                NegotiationCardType.Fixed => cardDataSO.fixedPlayerValue,
                NegotiationCardType.IntensityOnly => cardDataSO.fixedPlayerValue,
                NegotiationCardType.AttributeAndIntensity => cardDataSO.fixedPlayerValue,
                _ => 0
            };
            
            return IntensityHelper.GetScaledValue(selectedIntensity, baseValue);
        }
        return 0;
    }
}