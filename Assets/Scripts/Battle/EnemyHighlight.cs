using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyHighlight : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Color highlightColor = Color.yellow;
    public float pulseDuration = 0.5f; 
    public float pulseIntensity = 0.3f; 

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
            pulseTimer += Time.deltaTime;
            float pulseValue = Mathf.Sin(pulseTimer * (2f * Mathf.PI / pulseDuration)) * pulseIntensity + 1f;
            Color currentColor = Color.Lerp(originalColor, highlightColor, pulseValue * 0.5f);
            spriteRenderer.color = currentColor;
        }
    }

    public void StartHighlight()
    {
        if (!isHighlighted)
        {
            isHighlighted = true;
            pulseTimer = 0f;
        }
    }

    public void StopHighlight()
    {
        if (isHighlighted)
        {
            isHighlighted = false;
            spriteRenderer.color = originalColor;
        }
    }

    // Atualiza a cor original se o sprite mudar
    public void UpdateOriginalColor()
    {
        if (!isHighlighted)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void OnDisable()
    {
        StopHighlight();
    }
}