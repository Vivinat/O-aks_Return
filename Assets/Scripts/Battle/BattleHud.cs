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
    }
    
    public void ShowActionMenu(BattleEntity character)
    {
        activeCharacter = character;
        actionPanel.SetActive(true);
        targetSelectionPanel.SetActive(false);
        tooltipUI.Hide();

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
            
            // 3. *** LÓGICA DE CLIQUE CENTRALIZADA AQUI ***
            // Adiciona um listener diretamente no componente Button.
            // Quando clicado, ele chamará OnActionSelected, passando a ação correta.
            Button buttonComponent = buttonObj.GetComponent<Button>();
            buttonComponent.onClick.AddListener(() => OnActionSelected(action));
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
            targetSelectionPanel.SetActive(true);
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
    public void ShowTooltip(string name, string description) // Não precisamos mais da 'position'
    {
        if (tooltipUI != null && tooltipAnchor != null)
        {
            // Usa a posição da âncora em vez da posição do mouse/botão
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