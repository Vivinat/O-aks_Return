using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class StatusButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
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
            statusPanel.ShowTooltip(actionData.actionName, actionData.GetDynamicDescription());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (statusPanel != null)
        {
            statusPanel.HideTooltip();
        }
    }
}