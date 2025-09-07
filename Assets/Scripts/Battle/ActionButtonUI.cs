// Assets/Scripts/UI/ActionButtonUI.cs

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ActionButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private BattleAction actionData;
    private BattleHUD battleHUD;

    // Referência para a imagem do ícone, que será preenchida pelo BattleHUD
    public Image iconImage; 

    /// <summary>
    /// Configura os dados necessários para o tooltip.
    /// </summary>
    public void Setup(BattleAction action, BattleHUD hud)
    {
        actionData = action;
        battleHUD = hud;
        
        if (iconImage != null && actionData.icon != null)
        {
            iconImage.sprite = actionData.icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
        }
    }

    // Quando o mouse entra na área do botão
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (battleHUD != null && actionData != null)
        {
            battleHUD.ShowTooltip(actionData.actionName, actionData.description);
        }
    }

    // Quando o mouse sai da área do botão
    public void OnPointerExit(PointerEventData eventData)
    {
        if (battleHUD != null)
        {
            battleHUD.HideTooltip();
        }
    }
}