// Assets/Scripts/Battle/EnemyHighlight.cs

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyHighlight : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Color highlightColor = Color.yellow;
    public float pulseDuration = 0.5f; // Duração de cada pulso
    public float pulseIntensity = 0.3f; // Intensidade do efeito de piscar

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isHighlighted = false;
    private float pulseTimer = 0f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (isHighlighted)
        {
            // Efeito de piscar usando seno para transição suave
            pulseTimer += Time.deltaTime;
            float pulseValue = Mathf.Sin(pulseTimer * (2f * Mathf.PI / pulseDuration)) * pulseIntensity + 1f;
            
            // Interpola entre a cor original e a cor de highlight
            Color currentColor = Color.Lerp(originalColor, highlightColor, pulseValue * 0.5f);
            spriteRenderer.color = currentColor;
        }
    }

    /// <summary>
    /// Ativa o highlight no inimigo
    /// </summary>
    public void StartHighlight()
    {
        if (!isHighlighted)
        {
            isHighlighted = true;
            pulseTimer = 0f;
        }
    }

    /// <summary>
    /// Desativa o highlight e volta à cor original
    /// </summary>
    public void StopHighlight()
    {
        if (isHighlighted)
        {
            isHighlighted = false;
            spriteRenderer.color = originalColor;
        }
    }

    /// <summary>
    /// Permite atualizar a cor original (útil se a cor do sprite mudar por outros motivos)
    /// </summary>
    public void UpdateOriginalColor()
    {
        if (!isHighlighted)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void OnDisable()
    {
        // Garante que o highlight pare se o objeto for desativado
        StopHighlight();
    }
}