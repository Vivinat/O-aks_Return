// Assets/Scripts/UI/BattleHUD.cs (Atualizado para Itens)

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleHUD : MonoBehaviour
{
    public BattleManager battleManager;

    [Header("UI Panels")]
    public GameObject actionPanel;
    public GameObject targetSelectionPanel;
    public TooltipUI tooltipUI;
    public RectTransform tooltipAnchor; 

    [Header("Target Selection UI")]
    public Button cancelTargetButton;
    public TextMeshProUGUI targetInstructionText;
    
    [Header("Prefabs")]
    public GameObject actionButtonPrefab;

    private BattleEntity activeCharacter;
    private BattleAction selectedAction;

    void Start()
    {
        actionPanel.SetActive(false);
        targetSelectionPanel.SetActive(false);
        tooltipUI.Hide();

        if (cancelTargetButton != null)
        {
            cancelTargetButton.onClick.AddListener(CancelTargetSelection);
            cancelTargetButton.gameObject.SetActive(false);
        }
    }
    
    public void ShowActionMenu(BattleEntity character)
    {
        activeCharacter = character;
        actionPanel.SetActive(true);
        targetSelectionPanel.SetActive(false);
        tooltipUI.Hide();

        StopAllHighlights();

        if (cancelTargetButton != null)
        {
            cancelTargetButton.gameObject.SetActive(false);
        }

        // Limpa botões antigos
        foreach (Transform child in actionPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // NOVO: Filtra ações disponíveis
        List<BattleAction> availableActions = GetAvailableActions(character);

        foreach (BattleAction action in availableActions)
        {
            GameObject buttonObj = Instantiate(actionButtonPrefab, actionPanel.transform);
        
            ActionButtonUI buttonUI = buttonObj.GetComponent<ActionButtonUI>();
            buttonUI.Setup(action, this);
        
            Button buttonComponent = buttonObj.GetComponent<Button>();
            buttonComponent.onClick.AddListener(() => OnActionSelected(action));

            // NOVO: Lógica de disponibilidade atualizada
            bool isAvailable = IsActionAvailable(character, action);
            buttonComponent.interactable = isAvailable;
            
            // NOVO: Feedback visual para consumíveis sem uso
            if (!isAvailable && action.isConsumable)
            {
                Image buttonImage = buttonComponent.GetComponent<Image>();
                if (buttonImage != null)
                {
                    Color disabledColor = buttonImage.color;
                    disabledColor.a = 0.5f; // Torna semi-transparente
                    buttonImage.color = disabledColor;
                }
            }
        }
    }

    /// <summary>
    /// NOVO: Obtém apenas as ações disponíveis para o personagem
    /// </summary>
    private List<BattleAction> GetAvailableActions(BattleEntity character)
    {
        List<BattleAction> availableActions = new List<BattleAction>();

        foreach (BattleAction action in character.characterData.battleActions)
        {
            if (action == null) continue;

            // Se é um consumível, só adiciona se tiver usos
            if (action.isConsumable)
            {
                if (action.CanUse())
                {
                    availableActions.Add(action);
                }
            }
            else
            {
                // Ação normal sempre é adicionada (verificação de MP é feita na disponibilidade)
                availableActions.Add(action);
            }
        }

        return availableActions;
    }

    /// <summary>
    /// NOVO: Verifica se uma ação específica está disponível
    /// </summary>
    private bool IsActionAvailable(BattleEntity character, BattleAction action)
    {
        if (action.isConsumable)
        {
            return action.CanUse();
        }
        else
        {
            return character.currentMp >= action.manaCost;
        }
    }
    
    public void OnActionSelected(BattleAction action)
    {
        actionPanel.SetActive(false);
        selectedAction = action;
        tooltipUI.Hide();

        if (action.targetType == TargetType.Self || action.targetType == TargetType.AllEnemies || action.targetType == TargetType.AllAllies)
        {
            List<BattleEntity> targets = GetAutoTargets(action.targetType);
            battleManager.ExecuteAction(selectedAction, targets);
        }
        else
        {
            ShowTargetSelectionPanel(action);
        }
    }

    private void ShowTargetSelectionPanel(BattleAction action)
    {
        targetSelectionPanel.SetActive(true);

        if (cancelTargetButton != null)
        {
            cancelTargetButton.gameObject.SetActive(true);
        }

        if (targetInstructionText != null)
        {
            string instruction = "";
            switch (action.targetType)
            {
                case TargetType.SingleEnemy:
                    instruction = $"Escolha um inimigo para usar '{action.actionName}'";
                    break;
                case TargetType.SingleAlly:
                    instruction = $"Escolha um aliado para usar '{action.actionName}'";
                    break;
                default:
                    instruction = "Escolha um alvo";
                    break;
            }
            targetInstructionText.text = instruction;
        }
    }

    public void CancelTargetSelection()
    {
        Debug.Log("Seleção de alvo cancelada");
        
        targetSelectionPanel.SetActive(false);
        selectedAction = null;
        
        if (cancelTargetButton != null)
        {
            cancelTargetButton.gameObject.SetActive(false);
        }
        
        StopAllHighlights();
        
        if (activeCharacter != null)
        {
            ShowActionMenu(activeCharacter);
        }
    }

    private void StopAllHighlights()
    {
        TargetSelector[] allTargetSelectors = FindObjectsOfType<TargetSelector>();
        foreach (TargetSelector selector in allTargetSelectors)
        {
            selector.ForceStopHighlight();
        }
    }
    
    public void OnTargetSelected(BattleEntity target)
    {
        if (targetSelectionPanel.activeSelf)
        {
            bool isValidTarget = false;
            if (selectedAction.targetType == TargetType.SingleEnemy && target.characterData.team == Team.Enemy)
                isValidTarget = true;
            else if (selectedAction.targetType == TargetType.SingleAlly && target.characterData.team == Team.Player)
                isValidTarget = true;

            if (isValidTarget && !target.isDead)
            {
                targetSelectionPanel.SetActive(false);
                
                if (cancelTargetButton != null)
                {
                    cancelTargetButton.gameObject.SetActive(false);
                }
                
                StopAllHighlights();
                
                List<BattleEntity> targets = new List<BattleEntity> { target };
                battleManager.ExecuteAction(selectedAction, targets);
            }
            else
            {
                Debug.Log($"Alvo inválido para a ação '{selectedAction.actionName}'. Escolha outro.");
            }
        }
    }

    private List<BattleEntity> GetAutoTargets(TargetType type)
    {
        List<BattleEntity> autoTargets = new List<BattleEntity>();
        switch (type)
        {
            case TargetType.Self:
                autoTargets.Add(activeCharacter);
                break;
            case TargetType.AllEnemies:
                autoTargets.AddRange(battleManager.enemyTeam.Where(e => !e.isDead));
                break;
            case TargetType.AllAllies:
                autoTargets.AddRange(battleManager.playerTeam.Where(p => !p.isDead));
                break;
        }
        return autoTargets;
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