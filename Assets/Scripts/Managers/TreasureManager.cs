using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class TreasureManager : MonoBehaviour
{
    private TreasurePoolSO rewardPool;
    public int numberOfChoices = 3;
    private const int MAX_PLAYER_ACTIONS = 4;

    public Transform rewardOptionsContainer;
    public GameObject rewardChoicePrefab;
    public GameObject refreshButtonPrefab;
    public Button skipButton;
    public TextMeshProUGUI skipButtonText;

    public GameObject playerActionsPanel;
    public GameObject playerActionSlotPrefab;

    public TooltipUI tooltipUI;
    public RectTransform tooltipAnchor;

    public Color highlightColor = new Color(1f, 0.9f, 0.4f);
    public Color defaultColor = Color.white;
    public Color emptySlotColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
    
    public Color refreshUsedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private List<BattleAction> playerActions;
    private BattleAction selectedReward;
    private int selectedRewardButtonIndex = -1;
    private int selectedPlayerSlotIndex = -1;

    private List<GameObject> rewardButtonObjects = new List<GameObject>();
    private List<GameObject> playerSlotObjects = new List<GameObject>();
    private List<BattleAction> currentRewardChoices = new List<BattleAction>();
    private List<GameObject> refreshButtonObjects = new List<GameObject>();
    private List<bool> refreshButtonUsed = new List<bool>();

    void Start()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        rewardPool = GameManager.battleActionsPool;
        if (rewardPool == null)
        {
            return;
        }

        playerActions = GameManager.Instance.PlayerBattleActions;
        if (playerActions == null)
        {
            playerActions = new List<BattleAction>();
        }

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
            skipButtonText.text = "Descansar";
    }

    private void SetSaveMode()
    {
        if (skipButtonText != null)
            skipButtonText.text = "Salvar";
    }

    private void GenerateRewardChoices()
    {
        ClearRewardButtons();
        
        List<BattleAction> choices = rewardPool.GetRandomRewards(numberOfChoices, playerActions);
        currentRewardChoices = choices;
        
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
    
    private void CreateRewardSlot(BattleAction action, int index)
    {
        if (action == null)
        {
            return;
        }

        GameObject containerObj = new GameObject($"RewardSlot_{index}");
        containerObj.transform.SetParent(rewardOptionsContainer);
        
        VerticalLayoutGroup verticalLayout = containerObj.AddComponent<VerticalLayoutGroup>();
        verticalLayout.childAlignment = TextAnchor.MiddleCenter;
        verticalLayout.spacing = 5f;
        verticalLayout.childControlHeight = false;
        verticalLayout.childControlWidth = false;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.childForceExpandWidth = false;

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

        if (refreshButtonPrefab != null)
        {
            GameObject refreshObj = Instantiate(refreshButtonPrefab, containerObj.transform);
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
    }
    
    private void OnRefreshClicked(int slotIndex)
    {
        if (refreshButtonUsed[slotIndex])
        {
            AudioConstants.PlayCannotSelect();
            return;
        }
        
        AudioConstants.PlayButtonSelect();
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
        
        RefreshRewardSlot(slotIndex);
    }
    
    private void RefreshRewardSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= currentRewardChoices.Count)
        {
            return;
        }
        
        List<BattleAction> excludeList = new List<BattleAction>(playerActions);
        excludeList.AddRange(currentRewardChoices);
        
        BattleAction newReward = rewardPool.GetSingleRandomReward(excludeList);
        
        if (newReward == null)
        {
            AudioConstants.PlayCannotSelect();
            
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
        
        currentRewardChoices[slotIndex] = newReward;
        
        if (slotIndex < rewardButtonObjects.Count)
        {
            RewardButtonUI rewardButton = rewardButtonObjects[slotIndex].GetComponent<RewardButtonUI>();
            if (rewardButton != null)
            {
                rewardButton.Setup(newReward, this);
            }
            
            Button buttonComponent = rewardButtonObjects[slotIndex].GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.RemoveAllListeners();
                int buttonIndex = slotIndex;
                buttonComponent.onClick.AddListener(() => OnRewardSelected(newReward, buttonIndex));
            }
        }
        
        if (selectedRewardButtonIndex == slotIndex)
        {
            selectedReward = null;
            selectedRewardButtonIndex = -1;
            UpdateRewardHighlights();
            ResetSkipButton();
        }
    }

    private void PopulatePlayerActionsPanel()
    {
        if (playerActionsPanel == null)
        {
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
                continue;
            }

            if (i < playerActions.Count && playerActions[i] != null)
            {
                slotButton.Setup(playerActions[i], this);
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
    }

    private void OnRewardSelected(BattleAction chosenAction, int buttonIndex)
    {
        selectedReward = chosenAction;
        selectedRewardButtonIndex = buttonIndex;
        selectedPlayerSlotIndex = -1;
        
        UpdateRewardHighlights();
        UpdatePlayerSlotHighlights();
        ResetSkipButton();
    }

    private void OnPlayerSlotSelected(int slotIndex)
    {
        if (selectedReward == null)
        {
            AudioConstants.PlayCannotSelect();
            return;
        }

        selectedPlayerSlotIndex = slotIndex;
        UpdatePlayerSlotHighlights();
        SetSaveMode();
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

    private void ClearRewardButtons()
    {
        foreach (GameObject obj in rewardButtonObjects)
        {
            if (obj != null)
            {
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
        else
        {
            if (GameManager.Instance != null && GameManager.Instance.PlayerCharacterInfo != null)
            {
                int hpAtual = GameManager.Instance.GetPlayerCurrentHP();
                int mpAtual = GameManager.Instance.GetPlayerCurrentMP();
                int maxHP = GameManager.Instance.PlayerCharacterInfo.maxHp;
                int maxMP = GameManager.Instance.PlayerCharacterInfo.maxMp;

                GameManager.Instance.SetPlayerCurrentHP(Mathf.Min(hpAtual + 50, maxHP));
                GameManager.Instance.SetPlayerCurrentMP(Mathf.Min(mpAtual + 50, maxMP));
            }
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
        }
        else
        {
            playerActions[selectedPlayerSlotIndex] = selectedReward;
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