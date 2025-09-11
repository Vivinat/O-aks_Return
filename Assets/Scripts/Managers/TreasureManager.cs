// Assets/Scripts/Managers/TreasureManager.cs

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class TreasureManager : MonoBehaviour
{
    [Header("Data")]
    private TreasurePoolSO rewardPool;
    public int numberOfChoices = 3;
    private const int MAX_PLAYER_ACTIONS = 4;

    [Header("UI References")]
    public Transform rewardOptionsContainer;
    public GameObject rewardChoicePrefab;
    public Button skipButton;

    [Header("Player Actions UI")]
    public GameObject playerActionsPanel; // <- Mudei o nome para ser mais claro
    public GameObject playerActionSlotPrefab; // <- Mudei o nome para ser mais claro

    [Header("Tooltip UI")]
    public TooltipUI tooltipUI;
    public RectTransform tooltipAnchor;

    [Header("Highlight Visuals")]
    public Color highlightColor = new Color(1f, 0.9f, 0.4f); // Amarelo
    public Color defaultColor = Color.white;

    // --- Estado Interno ---
    private List<BattleAction> playerActions;
    private BattleAction selectedReward; // A recompensa que o jogador clicou e está "segurando"

    // Listas para gerenciar os botões criados
    private List<RewardButtonUI> rewardButtons = new List<RewardButtonUI>();
    private List<RewardButtonUI> playerActionButtons = new List<RewardButtonUI>();

    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager não encontrado!");
            return;
        }

        rewardPool = GameManager.battleActionsPool;
        playerActions = GameManager.Instance.PlayerBattleActions;

        tooltipUI.Hide();
        skipButton.onClick.AddListener(SkipSelection);

        // --- LÓGICA PRINCIPAL ---
        // 1. Gera as 3 recompensas para escolher
        GenerateRewardChoices();
        // 2. Mostra as ações que o jogador já tem
        PopulatePlayerActionsPanel();
    }

    /// <summary>
    /// Cria e exibe os 3 botões de recompensa.
    /// </summary>
    private void GenerateRewardChoices()
    {
        // Limpa estado anterior
        foreach (Transform child in rewardOptionsContainer) Destroy(child.gameObject);
        rewardButtons.Clear();

        List<BattleAction> choices = rewardPool.GetRandomRewards(numberOfChoices);
        
        foreach (BattleAction action in choices)
        {
            GameObject choiceInstance = Instantiate(rewardChoicePrefab, rewardOptionsContainer);
            RewardButtonUI rewardButton = choiceInstance.GetComponent<RewardButtonUI>();
            
            rewardButton.Setup(action, this);
            rewardButtons.Add(rewardButton);

            choiceInstance.GetComponent<Button>().onClick.AddListener(() => OnRewardSelected(action, rewardButton));
        }
    }

    /// <summary>
    /// Cria e exibe os botões das ações atuais do jogador.
    /// </summary>
    private void PopulatePlayerActionsPanel()
    {
        // Limpa estado anterior
        foreach (Transform child in playerActionsPanel.transform) Destroy(child.gameObject);
        playerActionButtons.Clear();

        // Cria um botão para cada ação que o jogador possui
        for (int i = 0; i < playerActions.Count; i++)
        {
            int index = i; // Captura de variável para o lambda
            BattleAction currentAction = playerActions[i];

            GameObject slotInstance = Instantiate(playerActionSlotPrefab, playerActionsPanel.transform);
            RewardButtonUI playerActionButton = slotInstance.GetComponent<RewardButtonUI>();

            playerActionButton.Setup(currentAction, this);
            playerActionButtons.Add(playerActionButton);
            
            // Ação de clique: Tenta substituir a skill neste slot
            slotInstance.GetComponent<Button>().onClick.AddListener(() => OnPlayerActionSlotClicked(index));
        }
    }

    /// <summary>
    /// Chamado quando o jogador clica em uma das 3 recompensas.
    /// </summary>
    private void OnRewardSelected(BattleAction chosenAction, RewardButtonUI clickedButton)
    {
        selectedReward = chosenAction;
        UpdateHighlights();
        Debug.Log($"Recompensa selecionada: {chosenAction.actionName}");
    }

    /// <summary>
    /// Chamado quando o jogador clica em um dos seus slots de ação.
    /// </summary>
    private void OnPlayerActionSlotClicked(int slotIndex)
    {
        // Se o jogador não selecionou uma recompensa primeiro, não faz nada.
        if (selectedReward == null)
        {
            Debug.Log("Selecione uma recompensa primeiro!");
            return;
        }

        // Lógica de substituição
        Debug.Log($"Substituindo '{playerActions[slotIndex].actionName}' por '{selectedReward.actionName}'.");
        playerActions[slotIndex] = selectedReward;
        EndTreasureEvent();
    }

    /// <summary>
    /// Se o jogador tem menos de 4 ações, ele pode simplesmente adicionar a nova.
    /// </summary>
    public void AddSelectedReward()
    {
        if (selectedReward == null)
        {
            Debug.Log("Selecione uma recompensa para adicionar!");
            return;
        }

        if (playerActions.Count < MAX_PLAYER_ACTIONS)
        {
            playerActions.Add(selectedReward);
            EndTreasureEvent();
        }
        else
        {
            Debug.Log("Slots cheios! Substitua uma ação existente.");
        }
    }


    /// <summary>
    /// Atualiza a cor dos botões de recompensa para mostrar qual está selecionado.
    /// </summary>
    private void UpdateHighlights()
    {
        foreach (var button in rewardButtons)
        {
            // Pega a imagem do botão (não do ícone filho) para mudar a cor de fundo/borda
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                // Se a ação deste botão é a que está selecionada, muda a cor. Senão, volta ao padrão.
                bool isSelected = (button.GetAction() == selectedReward);
                buttonImage.color = isSelected ? highlightColor : defaultColor;
            }
        }
    }
    
    // Pequena adição no RewardButtonUI para podermos pegar a action associada
    // Adicione este método ao seu script RewardButtonUI.cs:
    // public BattleAction GetAction() { return actionData; }

    public void SkipSelection()
    {
        tooltipUI.Hide();
        EndTreasureEvent();
    }

    private void EndTreasureEvent()
    {
        GameManager.Instance.ReturnToMap();
    }

    // --- MÉTODOS DO TOOLTIP (sem alterações) ---
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