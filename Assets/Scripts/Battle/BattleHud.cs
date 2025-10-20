// Assets/Scripts/UI/BattleHUD.cs (Atualizado com Timer de Turno)

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
    
    [Header("Turn Timer UI")]
    public TextMeshProUGUI turnTimerText;
    public float turnTimeLimit = 30f;
    
    [Header("Prefabs")]
    public GameObject actionButtonPrefab;

    private BattleEntity activeCharacter;
    private BattleAction selectedAction;
    private Coroutine timerCoroutine;
    private float currentTurnTime;
    private bool isTimerActive = false;
    private float decisionTimeMultiplier = 1f;

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

        // Esconde o timer inicialmente
        if (turnTimerText != null)
        {
            turnTimerText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Atualiza o display do timer se estiver ativo
        if (isTimerActive && turnTimerText != null)
        {
            UpdateTimerDisplay();
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

        HideEnemyAction();

        // Limpa botões antigos
        foreach (Transform child in actionPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Filtra ações disponíveis
        List<BattleAction> availableActions = GetAvailableActions(character);

        foreach (BattleAction action in availableActions)
        {
            GameObject buttonObj = Instantiate(actionButtonPrefab, actionPanel.transform);
        
            ActionButtonUI buttonUI = buttonObj.GetComponent<ActionButtonUI>();
            buttonUI.Setup(action, this);
        
            Button buttonComponent = buttonObj.GetComponent<Button>();
            buttonComponent.onClick.AddListener(() => OnActionSelected(action));

            bool isAvailable = IsActionAvailable(character, action);
            buttonComponent.interactable = isAvailable;
            
            if (!isAvailable && action.isConsumable)
            {
                Image buttonImage = buttonComponent.GetComponent<Image>();
                if (buttonImage != null)
                {
                    Color disabledColor = buttonImage.color;
                    disabledColor.a = 0.5f;
                    buttonImage.color = disabledColor;
                }
            }
        }

        // NOVO: Inicia o timer de turno quando o menu é mostrado
        StartTurnTimer();
    }

    /// <summary>
    /// NOVO: Inicia o timer de turno
    /// </summary>
    private void StartTurnTimer()
    {
        // Para qualquer timer anterior
        StopTurnTimer();

        // CORREÇÃO: Aplica o multiplicador ao tempo limite
        currentTurnTime = turnTimeLimit * decisionTimeMultiplier;
        isTimerActive = true;
    
        if (turnTimerText != null)
        {
            turnTimerText.gameObject.SetActive(true);
            UpdateTimerDisplay();
        }

        timerCoroutine = StartCoroutine(TurnTimerCoroutine());
    }

    /// <summary>
    /// NOVO: Para o timer de turno
    /// </summary>
    private void StopTurnTimer()
    {
        isTimerActive = false;
        
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        if (turnTimerText != null)
        {
            turnTimerText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// NOVO: Corrotina do timer de turno
    /// </summary>
    private IEnumerator TurnTimerCoroutine()
    {
        while (currentTurnTime > 0)
        {
            // Usa Time.unscaledDeltaTime para que funcione mesmo com Time.timeScale = 0
            // MAS verificamos se o jogo não está pausado pelo menu de opções
            if (Time.timeScale > 0)
            {
                currentTurnTime -= Time.deltaTime;
            }
            
            yield return null;
        }

        // Tempo esgotado!
        OnTurnTimeout();
    }

    /// <summary>
    /// NOVO: Atualiza o display do timer
    /// </summary>
    private void UpdateTimerDisplay()
    {
        if (turnTimerText == null) return;

        int seconds = Mathf.CeilToInt(currentTurnTime);
        
        // Muda a cor conforme o tempo vai acabando
        if (currentTurnTime <= 10f)
        {
            // Vermelho quando faltam 10 segundos ou menos
            turnTimerText.color = Color.Lerp(Color.yellow, Color.red, 1f - (currentTurnTime / 10f));
        }
        else if (currentTurnTime <= 20f)
        {
            // Amarelo quando faltam 20 segundos ou menos
            turnTimerText.color = Color.yellow;
        }
        else
        {
            // Branco quando tem tempo
            turnTimerText.color = Color.white;
        }

        turnTimerText.text = $"Tempo: {seconds}s";
    }

    /// <summary>
    /// NOVO: Chamado quando o tempo do turno acaba
    /// </summary>
    private void OnTurnTimeout()
    {
        Debug.Log($"{activeCharacter.characterData.characterName} perdeu o turno por timeout!");
        
        // Para o timer
        StopTurnTimer();

        // Fecha todos os painéis
        actionPanel.SetActive(false);
        targetSelectionPanel.SetActive(false);
        tooltipUI.Hide();

        if (cancelTargetButton != null)
        {
            cancelTargetButton.gameObject.SetActive(false);
        }

        StopAllHighlights();

        // Notifica o BattleManager sobre o timeout
        if (battleManager != null && activeCharacter != null)
        {
            battleManager.OnPlayerTurnTimeout(activeCharacter);
        }

        activeCharacter = null;
        selectedAction = null;
    }

    /// <summary>
    /// Obtém apenas as ações disponíveis para o personagem
    /// </summary>
    private List<BattleAction> GetAvailableActions(BattleEntity character)
    {
        List<BattleAction> availableActions = new List<BattleAction>();

        foreach (BattleAction action in character.characterData.battleActions)
        {
            if (action == null) continue;

            if (action.isConsumable)
            {
                if (action.CanUse())
                {
                    availableActions.Add(action);
                }
            }
            else
            {
                availableActions.Add(action);
            }
        }

        return availableActions;
    }

    /// <summary>
    /// Verifica se uma ação específica está disponível
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
        // NOVO: Para o timer quando uma ação é selecionada
        StopTurnTimer();

        actionPanel.SetActive(false);
        selectedAction = action;
        tooltipUI.Hide();
        AudioConstants.PlayButtonSelect();

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

        // NOVO: Reinicia o timer quando entra na seleção de alvo
        StartTurnTimer();
    }

    public void CancelTargetSelection()
    {
        Debug.Log("Seleção de alvo cancelada");
        AudioConstants.PlayCannotSelect();
        
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
                // NOVO: Para o timer quando um alvo é selecionado
                StopTurnTimer();

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

    /// <summary>
    /// NOVO: Mostra o texto da ação do inimigo usando o targetInstructionText
    /// </summary>
    public void ShowEnemyAction(string actionText)
    {
        if (targetInstructionText != null)
        {
            targetSelectionPanel.SetActive(true);
            
            if (cancelTargetButton != null)
            {
                cancelTargetButton.gameObject.SetActive(false);
            }
            
            targetInstructionText.text = actionText;
            
            Debug.Log($"Mostrando ação do inimigo: {actionText}");
        }
    }

    /// <summary>
    /// NOVO: Esconde o texto da ação do inimigo
    /// </summary>
    public void HideEnemyAction()
    {
        if (targetSelectionPanel != null && targetSelectionPanel.activeSelf)
        {
            if (cancelTargetButton != null && !cancelTargetButton.gameObject.activeSelf)
            {
                targetSelectionPanel.SetActive(false);
                Debug.Log("Escondendo ação do inimigo");
            }
        }
    }

    /// <summary>
    /// NOVO: Método alternativo para mostrar temporariamente uma mensagem
    /// </summary>
    public void ShowTemporaryMessage(string message, float duration = 2f)
    {
        StartCoroutine(ShowTemporaryMessageCoroutine(message, duration));
    }

    /// <summary>
    /// NOVO: Corrotina para mostrar mensagem temporária
    /// </summary>
    private System.Collections.IEnumerator ShowTemporaryMessageCoroutine(string message, float duration)
    {
        ShowEnemyAction(message);
        yield return new WaitForSeconds(duration);
        HideEnemyAction();
    }

    /// <summary>
    /// NOVO: Para ser chamado quando o menu de opções é aberto
    /// </summary>
    public void OnGamePaused()
    {
        // O timer continua rodando mas não decrementa quando Time.timeScale = 0
        // Isso é tratado automaticamente na corrotina
        Debug.Log("BattleHUD: Jogo pausado");
    }

    /// <summary>
    /// NOVO: Para ser chamado quando o menu de opções é fechado
    /// </summary>
    public void OnGameResumed()
    {
        // O timer continua automaticamente quando Time.timeScale volta a 1
        Debug.Log("BattleHUD: Jogo retomado");
    }
    
    public void SetDecisionTimeMultiplier(float multiplier)
    {
        decisionTimeMultiplier = multiplier;
        Debug.Log($"Tempo de decisão multiplicado por {multiplier}");
    }
}