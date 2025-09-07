// Assets/Scripts/UI/TooltipUI.cs

using UnityEngine;
using TMPro;
using UnityEngine.UI; // Para TextMeshPro

public class TooltipUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public RectTransform backgroundRect; // Opcional: para ajustar o tamanho do fundo

    void Awake()
    {
        // Começa desativado
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Exibe a tooltip com o nome e a descrição na posição especificada.
    /// </summary>
    public void Show(string name, string description, Vector2 position)
    {
        nameText.text = name;
        descriptionText.text = description;
        
        // Posiciona a tooltip
        transform.position = position;

        // Opcional: Ajusta o tamanho do background automaticamente com base no texto
        // Esta parte pode ser mais complexa dependendo da sua UI, um ContentSizeFitter ajuda
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Esconde a tooltip.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}