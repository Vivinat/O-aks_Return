// Assets/Scripts/UI/StatusButtonUI.cs (Corrected)

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class StatusButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    public Image iconImage;

    private BattleAction actionData;
    private StatusPanel statusPanel;

    public void Setup(BattleAction action, StatusPanel panel)
    {
        this.actionData = action;
        this.statusPanel = panel;

        if (iconImage != null && actionData != null)
        {
            iconImage.sprite = actionData.icon;
            iconImage.enabled = (actionData.icon != null);
        }
    }
    
    public BattleAction GetAction()
    {
        return actionData;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (statusPanel != null && actionData != null)
        {
            string description = actionData.description;
            
            // Adiciona informações extras para consumíveis
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
            
            // Get power from the first effect (since we use the new multi-effect system)
            if (actionData.effects != null && actionData.effects.Count > 0)
            {
                description += $"\nPoder: {actionData.effects[0].power}";
            }
            
            statusPanel.ShowTooltip(actionData.actionName, description);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (statusPanel != null)
        {
            statusPanel.HideTooltip();
        }
    }

    private string GetActionTypeDescription()
    {
        ActionType primaryType = actionData.GetPrimaryActionType();
        
        switch (primaryType)
        {
            case ActionType.Attack:
                return "Ofensiva";
            case ActionType.Heal:
                return "Cura";
            case ActionType.Buff:
                return "Aprimoramento";
            case ActionType.Debuff:
                return "Enfraquecimento";
            case ActionType.Mixed:
                return "Efeito Misto";
            default:
                return "Especial";
        }
    }

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
            case TargetType.Everyone:
                return "Todos os personagens";
            default:
                return "Alvo especial";
        }
    }
}