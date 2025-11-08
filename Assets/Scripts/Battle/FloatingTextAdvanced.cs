using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Floating text com animações avançadas para Canvas UI
/// </summary>
public class FloatingTextAdvanced : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float floatSpeed = 100f;
    [SerializeField] private float floatDistance = 80f;
    [SerializeField] private float fadeStartTime = 0.5f;
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Random Movement")]
    [SerializeField] private float horizontalRandomness = 30f;
    
    [Header("Scale Animation")]
    [SerializeField] private bool useScaleAnimation = true;
    [SerializeField] private float scaleStartMultiplier = 1.5f;
    [SerializeField] private float scaleEndMultiplier = 1f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Rotation")]
    [SerializeField] private bool useRotation = false;
    [SerializeField] private float rotationSpeed = 45f;
    
    [Header("Bounce Effect")]
    [SerializeField] private bool useBounce = true;
    [SerializeField] private float bounceHeight = 20f;
    [SerializeField] private float bounceSpeed = 8f;
    
    private TextMeshProUGUI textMesh;
    private RectTransform rectTransform;
    private Color originalColor;
    private Vector2 startPosition;
    private Vector2 endPosition;
    private Vector3 startScale;
    private float elapsedTime = 0f;
    private Vector2 randomOffset;

    void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        
        if (textMesh == null)
        {
            Debug.LogError("TextMeshProUGUI não encontrado!", this.gameObject);
        }

        rectTransform = GetComponent<RectTransform>();
        
        if (textMesh != null)
        {
            originalColor = textMesh.color;
        }
        
        startScale = transform.localScale;
    }    

    void Start()
    {
        startPosition = rectTransform.anchoredPosition;
        
        randomOffset = new Vector2(
            Random.Range(-horizontalRandomness, horizontalRandomness),
            0
        );
        
        endPosition = startPosition + Vector2.up * floatDistance + randomOffset;
        
        StartCoroutine(AnimateText());
    }

    public void Setup(string text, Color color, float fontSize)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
            textMesh.fontSize = fontSize;
            originalColor = color;
        }
    }

    private IEnumerator AnimateText()
    {
        while (elapsedTime < lifetime)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / lifetime;
            
            float curveValue = movementCurve.Evaluate(normalizedTime);
            Vector2 basePosition = Vector2.Lerp(startPosition, endPosition, curveValue);
            
            if (useBounce)
            {
                float bounceOffset = Mathf.Sin(elapsedTime * bounceSpeed) * bounceHeight * (1f - normalizedTime);
                basePosition.y += bounceOffset;
            }
            
            rectTransform.anchoredPosition = basePosition;
            
            if (useScaleAnimation)
            {
                float scaleValue = scaleCurve.Evaluate(normalizedTime);
                float scaleMultiplier = Mathf.Lerp(scaleStartMultiplier, scaleEndMultiplier, scaleValue);
                transform.localScale = startScale * scaleMultiplier;
            }
            
            if (useRotation)
            {
                float rotation = rotationSpeed * elapsedTime;
                transform.rotation = Quaternion.Euler(0, 0, rotation);
            }
            
            if (elapsedTime >= fadeStartTime)
            {
                float fadeProgress = (elapsedTime - fadeStartTime) / (lifetime - fadeStartTime);
                Color currentColor = originalColor;
                currentColor.a = Mathf.Lerp(1f, 0f, fadeProgress);
                
                if (textMesh != null)
                {
                    textMesh.color = currentColor;
                }
            }
            
            yield return null;
        }
        
        Destroy(gameObject);
    }

    public void ForceDestroy()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }
}