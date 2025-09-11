// Assets/Scripts/UI/RewardButtonUI.cs

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // Adicione esta linha para usar o tipo Image

public class RewardButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // NOVO: Referência pública para a imagem do ícone.
    // Vamos preencher isso no Inspector do prefab.
    public Image iconImage;

    private BattleAction actionData;
    private TreasureManager treasureManager;

    /// <summary>
    /// Configura o botão com os dados da ação e a referência ao manager da cena.
    /// </summary>
    public void Setup(BattleAction action, TreasureManager manager)
    {
        this.actionData = action;
        this.treasureManager = manager;

        // NOVO: Atribui o sprite diretamente à imagem de ícone referenciada.
        if (iconImage != null && actionData != null)
        {
            iconImage.sprite = actionData.icon;
            iconImage.enabled = (actionData.icon != null); // Habilita/desabilita a imagem se houver ícone
        }
    }
    
    /// <summary>
    /// Retorna a BattleAction associada a este botão.
    /// </summary>
    public BattleAction GetAction()
    {
        return actionData;
    }

    // Quando o mouse entra na área do botão
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (treasureManager != null && actionData != null)
        {
            treasureManager.ShowTooltip(actionData.actionName, actionData.description);
        }
    }

    // Quando o mouse sai da área do botão
    public void OnPointerExit(PointerEventData eventData)
    {
        if (treasureManager != null)
        {
            treasureManager.HideTooltip();
        }
    }
}