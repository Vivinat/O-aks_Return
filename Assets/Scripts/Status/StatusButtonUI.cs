// Assets/Scripts/UI/StatusButtonUI.cs

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Botão específico para o StatusPanel, baseado no RewardButtonUI
/// </summary>
public class StatusButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    public Image iconImage;
    private BattleAction actionData;
    private StatusPanel statusPanel;

    /// <summary>
    /// Configura o botão com os dados da ação e a referência ao StatusPanel
    /// </summary>
    public void Setup(BattleAction action, StatusPanel panel)
    {
        this.actionData = action;
        this.statusPanel = panel;

        // Configura o ícone
        if (iconImage != null && actionData != null)
        {
            iconImage.sprite = actionData.icon;
            iconImage.enabled = (actionData.icon != null); // Habilita/desabilita a imagem se houver ícone
        }
    }
    
    /// <summary>
    /// Retorna a BattleAction associada a este botão
    /// </summary>
    public BattleAction GetAction()
    {
        return actionData;
    }

    /// <summary>
    /// Quando o mouse entra na área do botão
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (statusPanel != null && actionData != null)
        {
            string description = actionData.description;
            
            // Adiciona informações extras para consumíveis (igual ao ActionButtonUI)
            if (actionData.isConsumable)
            {
                description += $"\n\nUsos: {actionData.currentUses}/{actionData.maxUses}";
                description += "\n(Consumível - será removido quando esgotado)";
            }
            else if (actionData.manaCost > 0)
            {
                description += $"\n\nCusto de MP: {actionData.manaCost}";
            }
            
            // Adiciona informações técnicas extras
            description += $"\n\nTipo: {GetActionTypeDescription()}";
            description += $"\nAlvo: {GetTargetTypeDescription()}";
            description += $"\nPoder: {actionData.power}";
            
            statusPanel.ShowTooltip(actionData.actionName, description);
        }
    }

    /// <summary>
    /// Quando o mouse sai da área do botão
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (statusPanel != null)
        {
            statusPanel.HideTooltip();
        }
    }

    /// <summary>
    /// Retorna descrição amigável do tipo de ação
    /// </summary>
    private string GetActionTypeDescription()
    {
        switch (actionData.type)
        {
            case ActionType.Attack:
                return "Ofensiva";
            case ActionType.Heal:
                return "Cura";
            case ActionType.Buff:
                return "Aprimoramento";
            case ActionType.Debuff:
                return "Enfraquecimento";
            default:
                return "Especial";
        }
    }

    /// <summary>
    /// Retorna descrição amigável do tipo de alvo
    /// </summary>
    private string GetTargetTypeDescription()
    {
        switch (actionData.targetType)
        {
            case TargetType.SingleEnemy:
                return "Inimigo único";
            case TargetType.SingleAlly:
                return "Aliado único";
            case TargetType.Self:
                return "Próprio usuário";
            case TargetType.AllEnemies:
                return "Todos os inimigos";
            case TargetType.AllAllies:
                return "Todos os aliados";
            default:
                return "Alvo especial";
        }
    }
}