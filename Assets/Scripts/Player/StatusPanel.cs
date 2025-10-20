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
    private bool isPausedByMe = false; // <<< ADICIONE ESTA LINHA

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

    // SUBSTITUA A FUNÇÃO UPDATE() INTEIRA PELA VERSÃO ABAIXO
    void Update()
    {
        // Tecla 'E' para abrir/fechar o painel
        if (Input.GetKeyDown(KeyCode.E)) //
        {
            // --- CORREÇÃO ---
            // Se a negociação estiver ativa, NÃO faça nada aqui.
            // O DeathNegotiationManager está no controle.
            if (DeathNegotiationManager.Instance != null && 
                DeathNegotiationManager.Instance.IsNegotiationActive())
            {
                return; // Deixa o DeathNegotiationManager controlar
            }
            // --- FIM DA CORREÇÃO ---
        
            // Verifica se o OptionsMenu não está aberto
            if (OptionsMenu.Instance == null || !OptionsMenu.Instance.IsMenuOpen()) //
            {
                TogglePanel(); //
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
        if (statusPanel == null) //
        {
            Debug.LogWarning("StatusPanel: statusPanel não foi atribuído!"); //
            return;
        }
        AudioConstants.PlayMenuOpen();
        
        isOpen = true; //
        statusPanel.SetActive(true); //
        
        // --- CORREÇÃO DO BUG TIME.SCALE ---
        bool negotiationIsActive = DeathNegotiationManager.Instance != null && 
                                   DeathNegotiationManager.Instance.IsNegotiationActive();
        
        // Só pausa o tempo se a negociação NÃO estiver ativa
        // (pois a negociação já pausou o tempo)
        if (!negotiationIsActive)
        {
            Time.timeScale = 0f;
            isPausedByMe = true; // Marcamos que NÓS pausamos o jogo
        }
        else
        {
            // Se a negociação está ativa, o tempo JÁ ESTÁ 0.
            // Não fomos nós que pausamos.
            isPausedByMe = false;
        }
        // --- FIM DA CORREÇÃO ---
        
        UpdateCharacterInfo(); //
        UpdateSkillsList(); //
        
        Debug.Log("Painel de status aberto"); //
    }

    public void ClosePanel()
    {
        if (statusPanel == null) return; //

        // --- INÍCIO DA SOLUÇÃO SIMPLES (BLOQUEIO) ---
        // Verifica se o DeathNegotiationManager existe E está ativo
        if (DeathNegotiationManager.Instance != null && 
            DeathNegotiationManager.Instance.IsNegotiationActive())
        {
            Debug.Log("StatusPanel: Fechamento bloqueado. Negociação em andamento.");
            // Opcional: Tocar um som de "bloqueado"
            return; // IMPEDE O FECHAMENTO
        }
        // --- FIM DA SOLUÇÃO SIMPLES ---

        isOpen = false; //
        statusPanel.SetActive(false); //
        
        // --- CORREÇÃO DO BUG TIME.SCALE ---
        // Só resume o tempo se fomos NÓS que pausamos
        if (isPausedByMe)
        {
            Time.timeScale = 1f;
            isPausedByMe = false;
        }
        // Removemos a checagem 'IsNegotiationScene()' que causava o bug
        // --- FIM DA CORREÇÃO ---
        
        if (tooltipUI != null) //
            tooltipUI.Hide(); //
        
        Debug.Log("Painel de status fechado"); //
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