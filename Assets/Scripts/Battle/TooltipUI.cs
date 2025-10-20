// Assets/Scripts/UI/TooltipUI.cs (FIXED - Usa cópias runtime para consulta)

using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public RectTransform backgroundRect;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Exibe a tooltip com o nome e a descrição na posição especificada.
    /// ATUALIZADO: Usa cópias runtime para mostrar valores modificados
    /// </summary>
    public void Show(string name, string description, Vector2 position)
    {
        nameText.text = name;
        descriptionText.text = description;
        
        // Posiciona a tooltip
        transform.position = position;

        // Ajusta o tamanho do background automaticamente com base no texto
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// NOVO: Exibe tooltip para uma BattleAction, mostrando valores modificados
    /// </summary>
    public void ShowForAction(BattleAction originalAction, Vector2 position)
    {
        if (originalAction == null)
        {
            Hide();
            return;
        }
        
        // Tenta obter a cópia modificada para consulta
        BattleAction displayAction = originalAction;
        
        if (BattleActionRuntimeCopies.Instance != null)
        {
            BattleAction modifiedCopy = BattleActionRuntimeCopies.Instance.GetModifiedActionCopy(originalAction);
            if (modifiedCopy != null)
            {
                displayAction = modifiedCopy;
            }
        }
        
        // Usa GetDynamicDescription que já calcula valores corretamente
        string description = displayAction.GetDynamicDescription();
        
        Show(displayAction.actionName, description, position);
    }

    /// <summary>
    /// Esconde a tooltip.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}