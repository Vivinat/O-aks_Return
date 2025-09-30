// Assets/Scripts/UI/StatusPanel.cs (UPDATED - Scene-Aware)

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class StatusPanel : MonoBehaviour
{
    public static StatusPanel Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject statusPanel;
    [SerializeField] private Button closeButton;
    
    [Header("Character Stats")]
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI mpText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("Battle Skills")]
    [SerializeField] private Transform skillsContainer;
    [SerializeField] private GameObject skillSlotPrefab;

    [Header("Tooltip")]
    [SerializeField] private TooltipUI tooltipUI;
    [SerializeField] private RectTransform tooltipAnchor;

    // Estado interno
    private bool isOpen = false;
    private List<GameObject> skillSlots = new List<GameObject>();

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
        // Tecla 'E' para abrir/fechar o painel
        // Mas apenas se o OptionsMenu não estiver aberto
        if (Input.GetKeyDown(KeyCode.E))
        {
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

        Debug.Log("StatusPanel inicializado");
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
        if (statusPanel == null)
        {
            Debug.LogWarning("StatusPanel: statusPanel não foi atribuído!");
            return;
        }

        isOpen = true;
        statusPanel.SetActive(true);
        
        // Só pausa o tempo se NÃO for a cena de negociação
        if (!IsNegotiationScene())
        {
            Time.timeScale = 0f;
        }
        
        UpdateCharacterInfo();
        UpdateSkillsList();
        
        Debug.Log("Painel de status aberto");
    }

    public void ClosePanel()
    {
        if (statusPanel == null) return;

        isOpen = false;
        statusPanel.SetActive(false);
        
        // Só volta o tempo se NÃO for a cena de negociação
        if (!IsNegotiationScene())
        {
            Time.timeScale = 1f;
        }
        
        if (tooltipUI != null)
            tooltipUI.Hide();
        
        Debug.Log("Painel de status fechado");
    }

    /// <summary>
    /// Verifica se está na cena de negociação
    /// </summary>
    private bool IsNegotiationScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        return currentScene == "NegotiationScene";
    }

    private void UpdateCharacterInfo()
    {
        if (GameManager.Instance?.PlayerCharacterInfo == null)
        {
            Debug.LogWarning("Informações do personagem não encontradas!");
            return;
        }

        Character playerData = GameManager.Instance.PlayerCharacterInfo;

        if (characterNameText != null)
            characterNameText.text = playerData.characterName;

        if (hpText != null)
            hpText.text = $"HP: {playerData.maxHp}";
        
        if (mpText != null)
            mpText.text = $"MP: {playerData.maxMp}";
        
        if (attackText != null)
        {
            int totalAttack = CalculateTotalAttack();
            attackText.text = $"Attack: {totalAttack}";
        }
        
        if (defenseText != null)
            defenseText.text = $"Defense: {playerData.defense}";
        
        if (speedText != null)
            speedText.text = $"Speed: {playerData.speed:F1}";

        if (coinsText != null && GameManager.Instance.CurrencySystem != null)
            coinsText.text = $"Souls: {GameManager.Instance.CurrencySystem.CurrentCoins}";
    }

    private int CalculateTotalAttack()
    {
        int totalAttack = 0;
        
        if (GameManager.Instance?.PlayerBattleActions != null)
        {
            foreach (var action in GameManager.Instance.PlayerBattleActions)
            {
                if (action != null)
                {
                    ActionType actionType = action.GetPrimaryActionType();
                    if (actionType == ActionType.Attack && action.effects.Count > 0)
                    {
                        totalAttack = Mathf.Max(totalAttack, action.effects[0].power);
                    }
                }
            }
        }

        return totalAttack > 0 ? totalAttack : 10;
    }

    private void UpdateSkillsList()
    {
        ClearSkillSlots();

        if (GameManager.Instance?.PlayerBattleActions == null)
        {
            Debug.LogWarning("Lista de ações do jogador não encontrada!");
            return;
        }

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
            Debug.LogWarning($"Prefab {skillSlotPrefab.name} não tem StatusButtonUI! Adicione o componente.");
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

    void OnValidate()
    {
        if (statusPanel == null)
            Debug.LogWarning("StatusPanel: statusPanel não foi atribuído!");
            
        if (skillsContainer == null)
            Debug.LogWarning("StatusPanel: skillsContainer não foi atribuído!");
            
        if (skillSlotPrefab == null)
            Debug.LogWarning("StatusPanel: skillSlotPrefab não foi atribuído!");
            
        if (tooltipUI == null)
            Debug.LogWarning("StatusPanel: tooltipUI não foi atribuído!");
    }
}