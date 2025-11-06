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

    public void Show(string name, string description, Vector2 position)
    {
        nameText.text = name;
        descriptionText.text = description;
        transform.position = position;

        // Ajusta o tamanho do background automaticamente
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}