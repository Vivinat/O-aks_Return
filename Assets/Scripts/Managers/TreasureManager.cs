// Assets/Scripts/Managers/TreasureManager.cs

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

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
    public TextMeshProUGUI skipButtonText; // Texto do botão skip/salvar

    [Header("Player Actions UI")]
    public GameObject playerActionsPanel;
    public GameObject playerActionSlotPrefab;

    [Header("Tooltip UI")]
    public TooltipUI tooltipUI;
    public RectTransform tooltipAnchor;

    [Header("Highlight Visuals")]
    public Color highlightColor = new Color(1f, 0.9f, 0.4f);
    public Color defaultColor = Color.white;
    public Color emptySlotColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);

    // --- Estado Interno ---
    private List<BattleAction> playerActions;
    private BattleAction selectedReward;
    private int selectedRewardButtonIndex = -1;
    private int selectedPlayerSlotIndex = -1;

    // Listas para gerenciar os botões criados
    private List<GameObject> rewardButtonObjects = new List<GameObject>();
    private List<GameObject> playerSlotObjects = new List<GameObject>();

    void Start()
    {
        Debug.Log("TreasureManager: Iniciando...");
        
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager não encontrado!");
            return;
        }

        rewardPool = GameManager.battleActionsPool;
        if (rewardPool == null)
        {
            Debug.LogError("TreasurePool não foi definido no GameManager!");
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

        if (skipButton != null)
            skipButton.onClick.AddListener(SkipSelection);

        ResetSkipButton();
        GenerateRewardChoices();
        PopulatePlayerActionsPanel();
    }

    /// <summary>
    /// Reseta o botão skip para o estado inicial
    /// </summary>
    private void ResetSkipButton()
    {
        if (skipButtonText != null)
            skipButtonText.text = "Sair";
    }

    /// <summary>
    /// Muda o botão skip para modo salvar
    /// </summary>
    private void SetSaveMode()
    {
        if (skipButtonText != null)
            skipButtonText.text = "Salvar";
    }

    /// <summary>
    /// Cria e exibe os 3 botões de recompensa.
    /// </summary>
    private void GenerateRewardChoices()
    {
        Debug.Log("Gerando escolhas de recompensa...");
        
        ClearRewardButtons();
        List<BattleAction> choices = rewardPool.GetRandomRewards(numberOfChoices);
        Debug.Log($"Geradas {choices.Count} escolhas");
        
        for (int i = 0; i < choices.Count; i++)
        {
            BattleAction action = choices[i];
            if (action == null)
            {
                Debug.LogWarning("BattleAction null encontrada na pool!");
                continue;
            }

            int buttonIndex = i;
            
            GameObject choiceInstance = Instantiate(rewardChoicePrefab, rewardOptionsContainer);
            RewardButtonUI rewardButton = choiceInstance.GetComponent<RewardButtonUI>();
            
            if (rewardButton == null)
            {
                Debug.LogError("RewardButtonUI não encontrado no prefab!");
                continue;
            }
            
            rewardButton.Setup(action, this);
            rewardButtonObjects.Add(choiceInstance);

            Button buttonComponent = choiceInstance.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(() => OnRewardSelected(action, buttonIndex));
            }
        }
    }

    /// <summary>
    /// Cria e exibe os slots das ações do jogador (incluindo vazios)
    /// </summary>
    private void PopulatePlayerActionsPanel()
    {
        Debug.Log($"Populando painel do jogador com {playerActions.Count} ações...");
        
        if (playerActionsPanel == null)
        {
            Debug.LogError("playerActionsPanel não foi atribuído no Inspector!");
            return;
        }

        playerActionsPanel.SetActive(true);
        ClearPlayerSlots();

        // Sempre cria 4 slots
        for (int i = 0; i < MAX_PLAYER_ACTIONS; i++)
        {
            int slotIndex = i;
            
            GameObject slotInstance = Instantiate(playerActionSlotPrefab, playerActionsPanel.transform);
            RewardButtonUI slotButton = slotInstance.GetComponent<RewardButtonUI>();

            if (slotButton == null)
            {
                Debug.LogError("RewardButtonUI não encontrado no prefab do slot!");
                continue;
            }

            // Se existe uma ação neste slot, configura normalmente
            if (i < playerActions.Count && playerActions[i] != null)
            {
                slotButton.Setup(playerActions[i], this);
                Debug.Log($"Slot {i}: {playerActions[i].actionName}");
            }
            else
            {
                // Slot vazio
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

    /// <summary>
    /// Configura um slot vazio
    /// </summary>
    private void SetupEmptySlot(RewardButtonUI buttonUI, int slotIndex)
    {
        if (buttonUI.iconImage != null)
        {
            buttonUI.iconImage.enabled = false;
        }
        
        Image buttonImage = buttonUI.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = emptySlotColor;
        }
        
        Debug.Log($"Slot {slotIndex}: vazio");
    }

    /// <summary>
    /// Chamado quando o jogador clica em uma recompensa
    /// </summary>
    private void OnRewardSelected(BattleAction chosenAction, int buttonIndex)
    {
        selectedReward = chosenAction;
        selectedRewardButtonIndex = buttonIndex;
        selectedPlayerSlotIndex = -1; // Reset seleção do slot
        
        UpdateRewardHighlights();
        UpdatePlayerSlotHighlights();
        ResetSkipButton();
        
        Debug.Log($"Recompensa selecionada: {chosenAction.actionName} (botão {buttonIndex})");
    }

    /// <summary>
    /// Chamado quando o jogador clica em um slot de habilidade
    /// </summary>
    private void OnPlayerSlotSelected(int slotIndex)
    {
        if (selectedReward == null)
        {
            Debug.Log("Selecione uma recompensa primeiro!");
            return;
        }

        selectedPlayerSlotIndex = slotIndex;
        UpdatePlayerSlotHighlights();
        SetSaveMode();
        
        if (slotIndex >= playerActions.Count)
        {
            Debug.Log($"Slot vazio {slotIndex} selecionado para adicionar '{selectedReward.actionName}'");
        }
        else
        {
            string currentAction = playerActions[slotIndex]?.actionName ?? "vazio";
            Debug.Log($"Slot {slotIndex} selecionado para substituir '{currentAction}' por '{selectedReward.actionName}'");
        }
    }

    /// <summary>
    /// Atualiza o highlight dos botões de recompensa
    /// </summary>
    private void UpdateRewardHighlights()
    {
        for (int i = 0; i < rewardButtonObjects.Count; i++)
        {
            Image buttonImage = rewardButtonObjects[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                bool isSelected = (i == selectedRewardButtonIndex);
                buttonImage.color = isSelected ? highlightColor : defaultColor;
            }
        }
    }

    /// <summary>
    /// Atualiza o highlight dos slots do jogador
    /// </summary>
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
                    // Volta à cor original (normal ou slot vazio)
                    if (i >= playerActions.Count)
                    {
                        buttonImage.color = emptySlotColor; // Slot vazio
                    }
                    else
                    {
                        buttonImage.color = defaultColor; // Slot com habilidade
                    }
                }
            }
        }
    }

    /// <summary>
    /// Limpa os botões de recompensa
    /// </summary>
    private void ClearRewardButtons()
    {
        foreach (GameObject obj in rewardButtonObjects)
        {
            if (obj != null) Destroy(obj);
        }
        rewardButtonObjects.Clear();
    }

    /// <summary>
    /// Limpa os slots do jogador
    /// </summary>
    private void ClearPlayerSlots()
    {
        foreach (GameObject obj in playerSlotObjects)
        {
            if (obj != null) Destroy(obj);
        }
        playerSlotObjects.Clear();
    }

    /// <summary>
    /// Função principal do botão Skip/Salvar
    /// </summary>
    public void SkipSelection()
    {
        if (tooltipUI != null)
            tooltipUI.Hide();

        // Se temos uma seleção completa (recompensa + slot), salva a mudança
        if (selectedReward != null && selectedPlayerSlotIndex >= 0)
        {
            SaveSelection();
        }
        
        EndTreasureEvent();
    }

    /// <summary>
    /// Salva a seleção do jogador
    /// </summary>
    private void SaveSelection()
    {
        if (selectedPlayerSlotIndex >= playerActions.Count)
        {
            // Adicionar a uma posição vazia
            // Precisa expandir a lista até a posição necessária
            while (playerActions.Count <= selectedPlayerSlotIndex)
            {
                playerActions.Add(null);
            }
            playerActions[selectedPlayerSlotIndex] = selectedReward;
            Debug.Log($"Adicionada '{selectedReward.actionName}' ao slot {selectedPlayerSlotIndex}");
        }
        else
        {
            // Substituir habilidade existente
            string oldAction = playerActions[selectedPlayerSlotIndex]?.actionName ?? "vazio";
            playerActions[selectedPlayerSlotIndex] = selectedReward;
            Debug.Log($"Substituída '{oldAction}' por '{selectedReward.actionName}' no slot {selectedPlayerSlotIndex}");
        }

        // Remove nulls da lista para manter consistência
        playerActions.RemoveAll(action => action == null);
    }

    /// <summary>
    /// Finaliza o evento e retorna ao mapa
    /// </summary>
    private void EndTreasureEvent()
    {
        if (GameManager.Instance.PlayerCharacterInfo != null)
        {
            GameManager.Instance.PlayerCharacterInfo.battleActions = new List<BattleAction>(playerActions);
        }
        
        GameManager.Instance.ReturnToMap();
    }

    // --- MÉTODOS DO TOOLTIP ---
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