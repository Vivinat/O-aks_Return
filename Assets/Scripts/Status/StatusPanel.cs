// Assets/Scripts/UI/StatusPanel.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
    [SerializeField] private GameObject skillSlotPrefab; // Deve ter StatusButtonUI

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
        if (Input.GetKeyDown(KeyCode.E))
        {
            TogglePanel();
        }
    }

    private void InitializePanel()
    {
        // Garante que o painel comece fechado
        if (statusPanel != null)
            statusPanel.SetActive(false);

        // Configura o botão de fechar
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        // Esconde o tooltip inicialmente
        if (tooltipUI != null)
            tooltipUI.Hide();

        Debug.Log("StatusPanel inicializado");
    }

    /// <summary>
    /// Alterna entre abrir e fechar o painel
    /// </summary>
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

    /// <summary>
    /// Abre o painel de status
    /// </summary>
    public void OpenPanel()
    {
        if (statusPanel == null)
        {
            Debug.LogWarning("StatusPanel: statusPanel não foi atribuído!");
            return;
        }

        isOpen = true;
        statusPanel.SetActive(true);
        
        // Pausa o jogo (similar ao sistema de diálogo)
        Time.timeScale = 0f;
        
        // Atualiza as informações
        UpdateCharacterInfo();
        UpdateSkillsList();
        
        Debug.Log("Painel de status aberto");
    }

    /// <summary>
    /// Fecha o painel de status
    /// </summary>
    public void ClosePanel()
    {
        if (statusPanel == null) return;

        isOpen = false;
        statusPanel.SetActive(false);
        
        // Resume o jogo
        Time.timeScale = 1f;
        
        // Esconde o tooltip
        if (tooltipUI != null)
            tooltipUI.Hide();
        
        Debug.Log("Painel de status fechado");
    }

    /// <summary>
    /// Atualiza as informações do personagem
    /// </summary>
    private void UpdateCharacterInfo()
    {
        if (GameManager.Instance?.PlayerCharacterInfo == null)
        {
            Debug.LogWarning("Informações do personagem não encontradas!");
            return;
        }

        Character playerData = GameManager.Instance.PlayerCharacterInfo;

        // Nome do personagem
        if (characterNameText != null)
            characterNameText.text = playerData.characterName;

        // Stats básicos
        if (hpText != null)
            hpText.text = $"HP: {playerData.maxHp}";
        
        if (mpText != null)
            mpText.text = $"MP: {playerData.maxMp}";
        
        if (attackText != null)
        {
            // Como não temos ataque base no Character, vamos calcular baseado nas skills
            int totalAttack = CalculateTotalAttack();
            attackText.text = $"Ataque: {totalAttack}";
        }
        
        if (defenseText != null)
            defenseText.text = $"Defesa: {playerData.defense}";
        
        if (speedText != null)
            speedText.text = $"Velocidade: {playerData.speed:F1}";

        // Moedas
        if (coinsText != null && GameManager.Instance.CurrencySystem != null)
            coinsText.text = $"Moedas: {GameManager.Instance.CurrencySystem.CurrentCoins}";
    }

    /// <summary>
    /// Calcula o poder de ataque total baseado nas skills
    /// </summary>
    private int CalculateTotalAttack()
    {
        int totalAttack = 0;
        
        if (GameManager.Instance?.PlayerBattleActions != null)
        {
            foreach (var action in GameManager.Instance.PlayerBattleActions)
            {
                if (action != null && action.type == ActionType.Attack)
                {
                    totalAttack = Mathf.Max(totalAttack, action.power);
                }
            }
        }

        return totalAttack > 0 ? totalAttack : 10; // Valor padrão se não tiver skills de ataque
    }

    /// <summary>
    /// Atualiza a lista de habilidades do personagem
    /// </summary>
    private void UpdateSkillsList()
    {
        // Limpa slots antigos
        ClearSkillSlots();

        if (GameManager.Instance?.PlayerBattleActions == null)
        {
            Debug.LogWarning("Lista de ações do jogador não encontrada!");
            return;
        }

        // Cria slots para cada habilidade
        foreach (var action in GameManager.Instance.PlayerBattleActions)
        {
            if (action != null)
            {
                CreateSkillSlot(action);
            }
        }
    }

    /// <summary>
    /// Cria um slot de habilidade usando o StatusButtonUI
    /// </summary>
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

    /// <summary>
    /// Limpa todos os slots de habilidade
    /// </summary>
    private void ClearSkillSlots()
    {
        foreach (GameObject slot in skillSlots)
        {
            if (slot != null)
                Destroy(slot);
        }
        skillSlots.Clear();
    }

    /// <summary>
    /// Mostra tooltip (chamado pelos StatusButtonUI)
    /// </summary>
    public void ShowTooltip(string title, string description)
    {
        if (tooltipUI != null && tooltipAnchor != null)
        {
            tooltipUI.Show(title, description, tooltipAnchor.position);
        }
    }

    /// <summary>
    /// Esconde tooltip
    /// </summary>
    public void HideTooltip()
    {
        if (tooltipUI != null)
        {
            tooltipUI.Hide();
        }
    }

    /// <summary>
    /// Verifica se o painel está aberto
    /// </summary>
    public bool IsOpen()
    {
        return isOpen;
    }

    void OnValidate()
    {
        // Validação no Editor
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