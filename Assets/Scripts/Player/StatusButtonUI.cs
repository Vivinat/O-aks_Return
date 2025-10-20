// Assets/Scripts/UI/StatusButtonUI.cs (UPDATED - Com Descrição Dinâmica)

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
            // ===== USA CÓPIA MODIFICADA PARA TOOLTIP =====
            BattleAction displayAction = GetActionForDisplay();
            
            // Usa GetDynamicDescription() que já calcula tudo corretamente
            statusPanel.ShowTooltip(displayAction.actionName, displayAction.GetDynamicDescription());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (statusPanel != null)
        {
            statusPanel.HideTooltip();
        }
    }
    
    /// <summary>
    /// NOVO: Retorna a ação apropriada para display (cópia modificada se disponível)
    /// </summary>
    private BattleAction GetActionForDisplay()
    {
        // Se não há actionData, retorna null
        if (actionData == null) return null;
        
        // Tenta obter a cópia modificada
        if (BattleActionRuntimeCopies.Instance != null)
        {
            BattleAction modifiedCopy = BattleActionRuntimeCopies.Instance.GetModifiedActionCopy(actionData);
            
            // Se encontrou a cópia, usa ela
            if (modifiedCopy != null)
            {
                return modifiedCopy;
            }
        }
        
        // Se não encontrou cópia, usa o original (fallback seguro)
        return actionData;
    }
}