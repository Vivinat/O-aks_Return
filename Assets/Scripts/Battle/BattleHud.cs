// Assets/Scripts/UI/BattleHUD.cs

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Use TextMeshPro para textos mais nítidos

public class BattleHUD : MonoBehaviour
{
    public BattleManager battleManager;

    [Header("UI Panels")]
    public GameObject actionPanel;      // Painel que contém os botões de ação
    public GameObject targetSelectionPanel; // Painel que diz "Escolha um Alvo"
    public TooltipUI tooltipUI;
    public RectTransform tooltipAnchor; 

    [Header("Target Selection UI")]
    public Button cancelTargetButton; // NOVO: Botão para cancelar seleção de alvo
    public TextMeshProUGUI targetInstructionText; // NOVO: Texto de instrução (opcional)
    
    [Header("Prefabs")]
    public GameObject actionButtonPrefab; // Prefab de um botão que vamos criar

    private BattleEntity activeCharacter;
    private BattleAction selectedAction;

    void Start()
    {
        // Garante que os painéis comecem desativados
        actionPanel.SetActive(false);
        targetSelectionPanel.SetActive(false);
        tooltipUI.Hide();

        // NOVO: Configura o botão de cancelar e o deixa invisível inicialmente
        if (cancelTargetButton != null)
        {
            cancelTargetButton.onClick.AddListener(CancelTargetSelection);
            cancelTargetButton.gameObject.SetActive(false); // Começa desativado
        }
    }
    
    public void ShowActionMenu(BattleEntity character)
    {
        activeCharacter = character;
        actionPanel.SetActive(true);
        targetSelectionPanel.SetActive(false);
        tooltipUI.Hide();

        // NOVO: Para todos os highlights quando voltamos ao menu de ações
        StopAllHighlights();

        // NOVO: Esconde o botão de cancelar quando no menu de ações
        if (cancelTargetButton != null)
        {
            cancelTargetButton.gameObject.SetActive(false);
        }

        // Limpa botões antigos
        foreach (Transform child in actionPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Para cada ação do personagem...
        foreach (BattleAction action in character.characterData.battleActions)
        {
            // 1. Instancia o prefab do botão
            GameObject buttonObj = Instantiate(actionButtonPrefab, actionPanel.transform);
        
            // 2. Configura o tooltip no script do botão
            ActionButtonUI buttonUI = buttonObj.GetComponent<ActionButtonUI>();
            buttonUI.Setup(action, this);
        
            // 3. Adiciona o listener de clique
            Button buttonComponent = buttonObj.GetComponent<Button>();
            buttonComponent.onClick.AddListener(() => OnActionSelected(action));

            // 4. Desabilita o botão se não houver mana suficiente
            if (character.currentMp < action.manaCost)
            {
                buttonComponent.interactable = false;
            }
        }
    }
    
    /// <summary>
    /// Chamado pelo ActionButtonUI quando um botão de ação é clicado.
    /// </summary>
    public void OnActionSelected(BattleAction action)
    {
        actionPanel.SetActive(false);
        selectedAction = action;
        tooltipUI.Hide();

        // O resto da lógica de seleção de alvo permanece o mesmo
        if (action.targetType == TargetType.Self || action.targetType == TargetType.AllEnemies || action.targetType == TargetType.AllAllies)
        {
            List<BattleEntity> targets = GetAutoTargets(action.targetType);
            battleManager.ExecuteAction(selectedAction, targets);
        }
        else
        {
            // NOVO: Atualiza o texto de instrução baseado no tipo de alvo
            ShowTargetSelectionPanel(action);
        }
    }

    /// <summary>
    /// NOVO: Mostra o painel de seleção de alvo com instruções específicas
    /// </summary>
    private void ShowTargetSelectionPanel(BattleAction action)
    {
        targetSelectionPanel.SetActive(true);

        // NOVO: Mostra o botão de cancelar quando entramos em modo de seleção de alvo
        if (cancelTargetButton != null)
        {
            cancelTargetButton.gameObject.SetActive(true);
        }

        // Atualiza o texto de instrução se existir
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

    /// <summary>
    /// NOVO: Cancela a seleção de alvo e volta para o menu de ações
    /// </summary>
    public void CancelTargetSelection()
    {
        Debug.Log("Seleção de alvo cancelada");
        
        targetSelectionPanel.SetActive(false);
        selectedAction = null;
        
        // NOVO: Esconde o botão de cancelar
        if (cancelTargetButton != null)
        {
            cancelTargetButton.gameObject.SetActive(false);
        }
        
        // NOVO: Para todos os highlights ativos
        StopAllHighlights();
        
        // Volta para o menu de ações do personagem ativo
        if (activeCharacter != null)
        {
            ShowActionMenu(activeCharacter);
        }
    }

    /// <summary>
    /// NOVO: Para todos os highlights ativos na cena
    /// </summary>
    private void StopAllHighlights()
    {
        TargetSelector[] allTargetSelectors = FindObjectsOfType<TargetSelector>();
        foreach (TargetSelector selector in allTargetSelectors)
        {
            selector.ForceStopHighlight();
        }
    }
    
    /// <summary>
    /// Chamado por um BattleEntity (via TargetSelector) quando ele é clicado como alvo.
    /// </summary>
    public void OnTargetSelected(BattleEntity target)
    {
        // Só executa se estivermos no modo de seleção de alvo
        if (targetSelectionPanel.activeSelf)
        {
            // Verifica se o alvo é válido para o tipo de ação selecionado
            bool isValidTarget = false;
            if (selectedAction.targetType == TargetType.SingleEnemy && target.characterData.team == Team.Enemy)
                isValidTarget = true;
            else if (selectedAction.targetType == TargetType.SingleAlly && target.characterData.team == Team.Player)
                isValidTarget = true;

            if (isValidTarget && !target.isDead)
            {
                targetSelectionPanel.SetActive(false);
                
                // NOVO: Esconde o botão de cancelar quando um alvo é selecionado
                if (cancelTargetButton != null)
                {
                    cancelTargetButton.gameObject.SetActive(false);
                }
                
                // NOVO: Para todos os highlights quando um alvo é selecionado
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

    /// <summary>
    /// Retorna a lista de alvos para ações automáticas (sem seleção manual).
    /// </summary>
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

    // Métodos para mostrar/esconder a tooltip
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