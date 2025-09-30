// Assets/Scripts/Negotiation/NegotiationCardUI.cs (UPDATED - No Zoom, Yellow Hover)

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
    
    private NegotiationCardSO cardData;
    private NegotiationManager negotiationManager;
    private bool isSelected = false;
    private bool isHovered = false;
    private Color targetColor;
    
    // Valores selecionados
    private CardAttribute selectedPlayerAttribute;
    private CardAttribute selectedEnemyAttribute;
    private int selectedValue;
    
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
        // Smooth color transition
        if (cardImage != null)
        {
            cardImage.color = Color.Lerp(
                cardImage.color,
                targetColor,
                Time.unscaledDeltaTime * colorTransitionSpeed
            );
        }
    }
    
    public void Setup(NegotiationCardSO card, NegotiationManager manager)
    {
        cardData = card;
        negotiationManager = manager;
        
        if (cardData == null)
        {
            Debug.LogError("NegotiationCardUI: Dados da carta são nulos!");
            return;
        }
        
        if (cardImage != null && cardData.cardSprite != null)
            cardImage.sprite = cardData.cardSprite;
        
        SetupByType();
        isSelected = false;
        UpdateVisuals();
        UpdateDescription();
    }
    
    private void SetupByType()
    {
        // Esconde tudo primeiro
        if (playerAttributePanel != null) playerAttributePanel.SetActive(false);
        if (enemyAttributePanel != null) enemyAttributePanel.SetActive(false);
        if (intensityPanel != null) intensityPanel.SetActive(false);
        
        switch (cardData.cardType)
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
        // Nenhum dropdown visível
        selectedPlayerAttribute = cardData.fixedPlayerAttribute;
        selectedEnemyAttribute = cardData.fixedEnemyAttribute;
        selectedValue = cardData.fixedValue;
        
        Debug.Log($"[FIXED] Jogador: {selectedPlayerAttribute} +{selectedValue}, Inimigo: {selectedEnemyAttribute} +{selectedValue}");
    }
    
    private void SetupIntensityOnly()
    {
        // Apenas dropdown de intensidade visível
        if (intensityPanel != null)
        {
            intensityPanel.SetActive(true);
            PopulateIntensityDropdown();
        }
        
        selectedPlayerAttribute = cardData.intensityOnlyPlayerAttribute;
        selectedEnemyAttribute = cardData.intensityOnlyEnemyAttribute;
        
        Debug.Log($"[INTENSITY ONLY] Jogador: {selectedPlayerAttribute}, Inimigo: {selectedEnemyAttribute}");
    }
    
    private void SetupAttributeAndIntensity()
    {
        // Todos os dropdowns visíveis
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
        
        Debug.Log($"[ATTRIBUTE AND INTENSITY] Dropdowns configurados");
    }
    
    private void PopulatePlayerAttributeDropdown()
    {
        if (playerAttributeDropdown == null) return;
        
        playerAttributeDropdown.ClearOptions();
        List<string> options = new List<string>();
        
        foreach (var attr in cardData.availablePlayerAttributes)
        {
            options.Add(AttributeHelper.GetDisplayName(attr));
        }
        
        playerAttributeDropdown.AddOptions(options);
        
        if (cardData.availablePlayerAttributes.Count > 0)
        {
            selectedPlayerAttribute = cardData.availablePlayerAttributes[0];
            playerAttributeDropdown.value = 0;
        }
    }
    
    private void PopulateEnemyAttributeDropdown()
    {
        if (enemyAttributeDropdown == null) return;
        
        enemyAttributeDropdown.ClearOptions();
        List<string> options = new List<string>();
        
        foreach (var attr in cardData.availableEnemyAttributes)
        {
            options.Add(AttributeHelper.GetDisplayName(attr));
        }
        
        enemyAttributeDropdown.AddOptions(options);
        
        if (cardData.availableEnemyAttributes.Count > 0)
        {
            selectedEnemyAttribute = cardData.availableEnemyAttributes[0];
            enemyAttributeDropdown.value = 0;
        }
    }
    
    private void PopulateIntensityDropdown()
    {
        if (intensityDropdown == null) return;
        
        intensityDropdown.ClearOptions();
        List<string> options = new List<string>();
        
        foreach (var intensity in cardData.availableIntensities)
        {
            int val = IntensityHelper.GetValue(intensity);
            options.Add($"{IntensityHelper.GetDisplayName(intensity)} (+{val})");
        }
        
        intensityDropdown.AddOptions(options);
        
        if (cardData.availableIntensities.Count > 0)
        {
            selectedValue = IntensityHelper.GetValue(cardData.availableIntensities[0]);
            intensityDropdown.value = 0;
        }
    }
    
    private void OnPlayerAttributeChanged(int index)
    {
        if (cardData.availablePlayerAttributes.Count > index)
        {
            selectedPlayerAttribute = cardData.availablePlayerAttributes[index];
            UpdateDescription();
            Debug.Log($"Atributo do jogador selecionado: {selectedPlayerAttribute}");
        }
    }
    
    private void OnEnemyAttributeChanged(int index)
    {
        if (cardData.availableEnemyAttributes.Count > index)
        {
            selectedEnemyAttribute = cardData.availableEnemyAttributes[index];
            UpdateDescription();
            Debug.Log($"Atributo dos inimigos selecionado: {selectedEnemyAttribute}");
        }
    }
    
    private void OnIntensityChanged(int index)
    {
        if (cardData.availableIntensities.Count > index)
        {
            selectedValue = IntensityHelper.GetValue(cardData.availableIntensities[index]);
            UpdateDescription();
            Debug.Log($"Intensidade selecionada: {selectedValue}");
        }
    }
    
    private void UpdateDescription()
    {
        if (descriptionText == null) return;
        descriptionText.text = cardData.GetFullDescription(selectedPlayerAttribute, selectedEnemyAttribute, selectedValue);
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
    
    public NegotiationCardSO GetCardData() => cardData;
    public CardAttribute GetSelectedPlayerAttribute() => selectedPlayerAttribute;
    public CardAttribute GetSelectedEnemyAttribute() => selectedEnemyAttribute;
    public int GetSelectedValue() => selectedValue;
}