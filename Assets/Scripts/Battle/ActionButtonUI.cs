// Assets/Scripts/UI/ActionButtonUI.cs (Atualizado para Itens)

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ActionButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private BattleAction actionData;
    private BattleHUD battleHUD;

    [Header("UI Components")]
    public Image iconImage; 
    public TextMeshProUGUI usesText; // NOVO: Para mostrar usos de itens

    public void Setup(BattleAction action, BattleHUD hud)
    {
        actionData = action;
        battleHUD = hud;
        
        // Configura o ícone
        if (iconImage != null && actionData.icon != null)
        {
            iconImage.sprite = actionData.icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
        }

        // NOVO: Configura texto de usos para consumíveis
        if (usesText != null)
        {
            if (actionData.isConsumable)
            {
                usesText.text = $"{actionData.currentUses}/{actionData.maxUses}";
                usesText.gameObject.SetActive(true);
            }
            else
            {
                usesText.gameObject.SetActive(false);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (battleHUD != null && actionData != null)
        {
            string description = actionData.description;
            
            // NOVO: Adiciona informações extras para consumíveis
            if (actionData.isConsumable)
            {
                description += $"\n\nUsos: {actionData.currentUses}/{actionData.maxUses}";
                description += "\n(Consumível - será removido quando esgotado)";
            }
            else if (actionData.manaCost > 0)
            {
                description += $"\n\nCusto de MP: {actionData.manaCost}";
            }
            
            battleHUD.ShowTooltip(actionData.actionName, actionData.GetDynamicDescription());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (battleHUD != null)
        {
            battleHUD.HideTooltip();
        }
    }
}