// Assets/Scripts/Managers/TreasureManager.cs (UPDATED with Individual Refresh)

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
    public GameObject refreshButtonPrefab; // NOVO: Prefab do botão de refresh
    public Button skipButton;
    public TextMeshProUGUI skipButtonText;

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
    
    [Header("Refresh Settings")]
    public Color refreshUsedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    // --- Estado Interno ---
    private List<BattleAction> playerActions;
    private BattleAction selectedReward;
    private int selectedRewardButtonIndex = -1;
    private int selectedPlayerSlotIndex = -1;

    // Listas para gerenciar os botões criados
    private List<GameObject> rewardButtonObjects = new List<GameObject>();
    private List<GameObject> playerSlotObjects = new List<GameObject>();
    
    // NOVO: Sistema de refresh
    private List<BattleAction> currentRewardChoices = new List<BattleAction>();
    private List<GameObject> refreshButtonObjects = new List<GameObject>();
    private List<bool> refreshButtonUsed = new List<bool>();

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

    private void ResetSkipButton()
    {
        if (skipButtonText != null)
            skipButtonText.text = "Sair";
    }

    private void SetSaveMode()
    {
        if (skipButtonText != null)
            skipButtonText.text = "Salvar";
    }

    /// <summary>
    /// MODIFICADO: Gera escolhas evitando skills que o jogador já tem
    /// </summary>
    private void GenerateRewardChoices()
    {
        Debug.Log("Gerando escolhas de recompensa...");
        
        ClearRewardButtons();
        
        // Gera recompensas excluindo as que o jogador já tem
        List<BattleAction> choices = rewardPool.GetRandomRewards(numberOfChoices, playerActions);
        
        if (choices.Count < numberOfChoices)
        {
            Debug.LogWarning($"Apenas {choices.Count} recompensas únicas disponíveis (menos que {numberOfChoices})");
        }
        
        currentRewardChoices = choices;
        Debug.Log($"Geradas {choices.Count} escolhas únicas");
        
        // Inicializa lista de refresh buttons usados
        refreshButtonUsed.Clear();
        for (int i = 0; i < choices.Count; i++)
        {
            refreshButtonUsed.Add(false);
        }
        
        for (int i = 0; i < choices.Count; i++)
        {
            CreateRewardSlot(choices[i], i);
        }
    }
    
    /// <summary>
    /// NOVO: Cria um slot de recompensa com botão de refresh
    /// </summary>
    private void CreateRewardSlot(BattleAction action, int index)
    {
        if (action == null)
        {
            Debug.LogWarning("BattleAction null encontrada!");
            return;
        }

        // Cria container vertical para skill + botão refresh
        GameObject containerObj = new GameObject($"RewardSlot_{index}");
        containerObj.transform.SetParent(rewardOptionsContainer);
        
        VerticalLayoutGroup verticalLayout = containerObj.AddComponent<VerticalLayoutGroup>();
        verticalLayout.childAlignment = TextAnchor.MiddleCenter;
        verticalLayout.spacing = 5f;
        verticalLayout.childControlHeight = false;
        verticalLayout.childControlWidth = false;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.childForceExpandWidth = false;

        // Cria o botão de skill
        GameObject choiceInstance = Instantiate(rewardChoicePrefab, containerObj.transform);
        RewardButtonUI rewardButton = choiceInstance.GetComponent<RewardButtonUI>();
        
        if (rewardButton != null)
        {
            rewardButton.Setup(action, this);
        }
        
        Button buttonComponent = choiceInstance.GetComponent<Button>();
        if (buttonComponent != null)
        {
            int buttonIndex = index;
            buttonComponent.onClick.AddListener(() => OnRewardSelected(action, buttonIndex));
        }
        
        rewardButtonObjects.Add(choiceInstance);

        // Cria o botão de refresh
        if (refreshButtonPrefab != null)
        {
            GameObject refreshObj = Instantiate(refreshButtonPrefab, containerObj.transform);
            Button refreshBtn = refreshObj.GetComponent<Button>();
            
            if (refreshBtn != null)
            {
                int refreshIndex = index;
                refreshBtn.onClick.AddListener(() => OnRefreshClicked(refreshIndex));
                
                // Configura texto do botão
                TextMeshProUGUI btnText = refreshBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = "Refresh";
                }
            }
            
            refreshButtonObjects.Add(refreshObj);
        }
        else
        {
            Debug.LogWarning("refreshButtonPrefab não foi atribuído!");
        }
    }
    
    /// <summary>
    /// NOVO: Chamado quando um botão de refresh é clicado
    /// </summary>
    private void OnRefreshClicked(int slotIndex)
    {
        // Verifica se já foi usado
        if (refreshButtonUsed[slotIndex])
        {
            Debug.Log($"Botão de refresh {slotIndex} já foi usado!");
            AudioConstants.PlayCannotSelect();
            return;
        }
        
        Debug.Log($"Refresh solicitado para slot {slotIndex}");
        AudioConstants.PlayButtonSelect();
        
        // Marca como usado
        refreshButtonUsed[slotIndex] = true;
        
        // Desabilita visualmente o botão
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
        
        // Gera nova recompensa
        RefreshRewardSlot(slotIndex);
    }
    
    /// <summary>
    /// NOVO: Atualiza uma recompensa específica
    /// </summary>
    private void RefreshRewardSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= currentRewardChoices.Count)
        {
            Debug.LogError($"Índice de slot inválido: {slotIndex}");
            return;
        }
        
        // Cria lista de exclusões: jogador + outras escolhas atuais
        List<BattleAction> excludeList = new List<BattleAction>(playerActions);
        
        // Adiciona as outras escolhas atuais (incluindo a que vai ser substituída)
        excludeList.AddRange(currentRewardChoices);
        
        // Tenta pegar uma nova recompensa
        BattleAction newReward = rewardPool.GetSingleRandomReward(excludeList);
        
        if (newReward == null)
        {
            Debug.LogWarning("Não há mais recompensas únicas disponíveis para refresh!");
            AudioConstants.PlayCannotSelect();
            
            // Reverte o botão de refresh
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
        
        Debug.Log($"Slot {slotIndex}: '{currentRewardChoices[slotIndex]?.actionName}' → '{newReward.actionName}'");
        
        // Atualiza a lista interna
        currentRewardChoices[slotIndex] = newReward;
        
        // Atualiza o botão de recompensa
        if (slotIndex < rewardButtonObjects.Count)
        {
            RewardButtonUI rewardButton = rewardButtonObjects[slotIndex].GetComponent<RewardButtonUI>();
            if (rewardButton != null)
            {
                rewardButton.Setup(newReward, this);
            }
            
            // Atualiza o listener do botão
            Button buttonComponent = rewardButtonObjects[slotIndex].GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.RemoveAllListeners();
                int buttonIndex = slotIndex;
                buttonComponent.onClick.AddListener(() => OnRewardSelected(newReward, buttonIndex));
            }
        }
        
        // Se a recompensa refreshada estava selecionada, desseleciona
        if (selectedRewardButtonIndex == slotIndex)
        {
            selectedReward = null;
            selectedRewardButtonIndex = -1;
            UpdateRewardHighlights();
            ResetSkipButton();
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

            if (i < playerActions.Count && playerActions[i] != null)
            {
                slotButton.Setup(playerActions[i], this);
                Debug.Log($"Slot {i}: {playerActions[i].actionName}");
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

    private void OnRewardSelected(BattleAction chosenAction, int buttonIndex)
    {
        selectedReward = chosenAction;
        selectedRewardButtonIndex = buttonIndex;
        selectedPlayerSlotIndex = -1;
        
        UpdateRewardHighlights();
        UpdatePlayerSlotHighlights();
        ResetSkipButton();
        
        Debug.Log($"Recompensa selecionada: {chosenAction.actionName} (botão {buttonIndex})");
    }

    private void OnPlayerSlotSelected(int slotIndex)
    {
        if (selectedReward == null)
        {
            Debug.Log("Selecione uma recompensa primeiro!");
            AudioConstants.PlayCannotSelect();
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
        AudioConstants.PlayItemBuy();
    }

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
                    if (i >= playerActions.Count)
                    {
                        buttonImage.color = emptySlotColor;
                    }
                    else
                    {
                        buttonImage.color = defaultColor;
                    }
                }
            }
        }
    }

    /// <summary>
    /// MODIFICADO: Limpa também os botões de refresh
    /// </summary>
    private void ClearRewardButtons()
    {
        foreach (GameObject obj in rewardButtonObjects)
        {
            if (obj != null)
            {
                // Destrói o container pai se existir
                if (obj.transform.parent != null && obj.transform.parent != rewardOptionsContainer)
                {
                    Destroy(obj.transform.parent.gameObject);
                }
                else
                {
                    Destroy(obj);
                }
            }
        }
        rewardButtonObjects.Clear();
        
        foreach (GameObject obj in refreshButtonObjects)
        {
            if (obj != null) Destroy(obj);
        }
        refreshButtonObjects.Clear();
        
        currentRewardChoices.Clear();
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

    public void SkipSelection()
    {
        if (tooltipUI != null)
            tooltipUI.Hide();

        if (selectedReward != null && selectedPlayerSlotIndex >= 0)
        {
            SaveSelection();
        }
        
        EndTreasureEvent();
    }

    private void SaveSelection()
    {
        if (selectedPlayerSlotIndex >= playerActions.Count)
        {
            while (playerActions.Count <= selectedPlayerSlotIndex)
            {
                playerActions.Add(null);
            }
            playerActions[selectedPlayerSlotIndex] = selectedReward;
            Debug.Log($"Adicionada '{selectedReward.actionName}' ao slot {selectedPlayerSlotIndex}");
        }
        else
        {
            string oldAction = playerActions[selectedPlayerSlotIndex]?.actionName ?? "vazio";
            playerActions[selectedPlayerSlotIndex] = selectedReward;
            Debug.Log($"Substituída '{oldAction}' por '{selectedReward.actionName}' no slot {selectedPlayerSlotIndex}");
        }

        playerActions.RemoveAll(action => action == null);
    }

    private void EndTreasureEvent()
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