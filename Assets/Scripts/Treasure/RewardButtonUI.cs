using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 

public class RewardButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;

    private BattleAction actionData;
    private TreasureManager treasureManager;


    public void Setup(BattleAction action, TreasureManager manager)
    {
        this.actionData = action;
        this.treasureManager = manager;

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
        if (treasureManager != null && actionData != null)
        {
            treasureManager.ShowTooltip(actionData.actionName, actionData.GetDynamicDescription());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (treasureManager != null)
        {
            treasureManager.HideTooltip();
        }
    }
}