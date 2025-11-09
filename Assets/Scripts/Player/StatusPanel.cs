using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class StatusPanel : MonoBehaviour
{
    public static StatusPanel Instance { get; private set; }

    [SerializeField] private GameObject statusPanel;
    [SerializeField] private Button closeButton;
    
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI mpText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI coinsText;

    [SerializeField] private Transform skillsContainer;
    [SerializeField] private GameObject skillSlotPrefab;

    [SerializeField] private TooltipUI tooltipUI;
    [SerializeField] private RectTransform tooltipAnchor;

    private bool isOpen = false;
    private List<GameObject> skillSlots = new List<GameObject>();
    private bool isPausedByMe = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializePanel();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (DeathNegotiationManager.Instance != null && 
                DeathNegotiationManager.Instance.IsNegotiationActive())
            {
                return;
            }
        
            if (OptionsMenu.Instance == null || !OptionsMenu.Instance.IsMenuOpen())
            {
                TogglePanel();
            }
        }
    }

    private void InitializePanel()
    {
        if (statusPanel != null)
            statusPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        if (tooltipUI != null)
            tooltipUI.Hide();
    }

    public void TogglePanel()
    {
        if (isOpen)
        {
            ClosePanel();
        }
        else
        {
            OpenPanel();
        }
    }

    public void OpenPanel()
    {
        AudioConstants.PlayMenuOpen();
        
        isOpen = true;
        statusPanel.SetActive(true);
        
        bool negotiationIsActive = DeathNegotiationManager.Instance != null && 
                                   DeathNegotiationManager.Instance.IsNegotiationActive();
        
        if (!negotiationIsActive)
        {
            Time.timeScale = 0f;
            isPausedByMe = true;
        }
        else
        {
            isPausedByMe = false;
        }
        
        UpdateCharacterInfo();
        UpdateSkillsList();
    }

    public void ClosePanel()
    {
        if (statusPanel == null) return;

        if (DeathNegotiationManager.Instance != null && 
            DeathNegotiationManager.Instance.IsNegotiationActive())
        {
            return;
        }

        isOpen = false;
        statusPanel.SetActive(false);
        
        if (isPausedByMe)
        {
            Time.timeScale = 1f;
            isPausedByMe = false;
        }
        
        if (tooltipUI != null)
            tooltipUI.Hide();
    }

    private void UpdateCharacterInfo()
    {

        Character playerData = GameManager.Instance.PlayerCharacterInfo;

        if (characterNameText != null)
            characterNameText.text = playerData.characterName;

        if (hpText != null)
            hpText.text = $"HP: {GameManager.Instance.GetPlayerCurrentHP()}/{playerData.maxHp}";
        
        if (mpText != null)
            mpText.text = $"MP: {GameManager.Instance.GetPlayerCurrentMP()}/{playerData.maxMp}";
        
        if (defenseText != null)
            defenseText.text = $"Defesa: {playerData.defense}";
        
        if (speedText != null)
            speedText.text = $"Velocidade: {playerData.speed:F1}";

        if (coinsText != null && GameManager.Instance.CurrencySystem != null)
            coinsText.text = $"Moedas: {GameManager.Instance.CurrencySystem.CurrentCoins}";
    }
    
    private void UpdateSkillsList()
    {
        ClearSkillSlots();

        foreach (var action in GameManager.Instance.PlayerBattleActions)
        {
            if (action != null)
            {
                CreateSkillSlot(action);
            }
        }
    }

    private void CreateSkillSlot(BattleAction action)
    {
        if (skillSlotPrefab == null || skillsContainer == null) return;

        GameObject slotObj = Instantiate(skillSlotPrefab, skillsContainer);
        StatusButtonUI slotComponent = slotObj.GetComponent<StatusButtonUI>();

        if (slotComponent != null)
        {
            slotComponent.Setup(action, this);
        }
        else
        {
            Debug.LogWarning($"Prefab {skillSlotPrefab.name} não tem StatusButtonUI!");
        }

        skillSlots.Add(slotObj);
    }

    private void ClearSkillSlots()
    {
        foreach (GameObject slot in skillSlots)
        {
            if (slot != null)
                Destroy(slot);
        }
        skillSlots.Clear();
    }

    public void ShowTooltip(string title, string description)
    {
        if (tooltipUI != null && tooltipAnchor != null)
        {
            tooltipUI.Show(title, description, tooltipAnchor.position);
        }
    }

    public void HideTooltip()
    {
        if (tooltipUI != null)
        {
            tooltipUI.Hide();
        }
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}