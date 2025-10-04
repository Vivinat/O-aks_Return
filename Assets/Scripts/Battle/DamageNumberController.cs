// Assets/Scripts/Battle/DamageNumberController.cs
// VERSÃO CANVAS UI - Funciona com FloatingTextAdvanced

using UnityEngine;
using TMPro;

/// <summary>
/// Controlador central para criar números de dano e textos de status flutuantes
/// VERSÃO CANVAS UI
/// </summary>
public class DamageNumberController : MonoBehaviour
{
    public static DamageNumberController Instance { get; private set; }

    [Header("Canvas Configuration")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private GameObject floatingTextPrefab;

    [Header("Colors")]
    [SerializeField] private Color damageColor = new Color(1f, 0.3f, 0.3f); // Vermelho
    [SerializeField] private Color healColor = new Color(0.3f, 1f, 0.3f);   // Verde
    [SerializeField] private Color criticalColor = new Color(1f, 1f, 0.3f); // Amarelo
    [SerializeField] private Color statusPositiveColor = new Color(0.5f, 0.8f, 1f); // Azul claro
    [SerializeField] private Color statusNegativeColor = new Color(1f, 0.5f, 0f);   // Laranja
    [SerializeField] private Color statusRemoveColor = new Color(0.7f, 0.7f, 0.7f); // Cinza

    [Header("Font Sizes")]
    [SerializeField] private float baseFontSize = 36f;
    [SerializeField] private float criticalFontMultiplier = 1.5f;
    [SerializeField] private float statusFontSize = 28f;

    [Header("Damage Thresholds")]
    [SerializeField] private int smallDamageThreshold = 20;
    [SerializeField] private int mediumDamageThreshold = 50;
    [SerializeField] private int largeDamageThreshold = 100;

    [Header("Spawn Settings")]
    [SerializeField] private float verticalOffset = 50f;
    [SerializeField] private float horizontalSpread = 20f;

    private Camera mainCamera;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        mainCamera = Camera.main;
        
        // Se não tiver canvas atribuído, tenta encontrar
        if (targetCanvas == null)
        {
            targetCanvas = FindObjectOfType<Canvas>();
            if (targetCanvas != null)
            {
                Debug.Log("DamageNumberController: Canvas encontrado automaticamente!");
            }
        }
    }

    /// <summary>
    /// Mostra um número de dano
    /// </summary>
    public void ShowDamage(Vector3 position, int damageAmount, bool isCritical = false)
    {
        if (damageAmount <= 0) return;

        string text = damageAmount.ToString();
        Color color = isCritical ? criticalColor : damageColor;
        float fontSize = CalculateDamageFontSize(damageAmount, isCritical);

        CreateFloatingText(position, text, color, fontSize);
    }

    /// <summary>
    /// Mostra um número de cura
    /// </summary>
    public void ShowHealing(Vector3 position, int healAmount)
    {
        if (healAmount <= 0) return;

        string text = $"+{healAmount}";
        float fontSize = CalculateDamageFontSize(healAmount, false);

        CreateFloatingText(position, text, healColor, fontSize);
    }

    /// <summary>
    /// Mostra quando um status positivo é aplicado
    /// </summary>
    public void ShowStatusApplied(Vector3 position, string statusName, int duration)
    {
        string text = $"+{statusName}({duration})";
        CreateFloatingText(position, text, statusPositiveColor, statusFontSize);
    }

    /// <summary>
    /// Mostra quando um status negativo é aplicado
    /// </summary>
    public void ShowDebuffApplied(Vector3 position, string statusName, int duration)
    {
        string text = $"+{statusName}({duration})";
        CreateFloatingText(position, text, statusNegativeColor, statusFontSize);
    }

    /// <summary>
    /// Mostra quando um status é removido
    /// </summary>
    public void ShowStatusRemoved(Vector3 position, string statusName)
    {
        string text = $"-{statusName}";
        CreateFloatingText(position, text, statusRemoveColor, statusFontSize);
    }

    /// <summary>
    /// Calcula o tamanho da fonte baseado no dano
    /// </summary>
    private float CalculateDamageFontSize(int amount, bool isCritical)
    {
        float size = baseFontSize;

        // Aumenta o tamanho baseado no valor do dano
        if (amount >= largeDamageThreshold)
        {
            size *= 1.8f;
        }
        else if (amount >= mediumDamageThreshold)
        {
            size *= 1.4f;
        }
        else if (amount >= smallDamageThreshold)
        {
            size *= 1.2f;
        }

        // Críticos são ainda maiores
        if (isCritical)
        {
            size *= criticalFontMultiplier;
        }

        return size;
    }

    /// <summary>
    /// Cria o texto flutuante na posição especificada
    /// VERSÃO CANVAS UI - Converte world position para screen position
    /// </summary>
    private void CreateFloatingText(Vector3 worldPosition, string text, Color color, float fontSize)
    {
        if (floatingTextPrefab == null)
        {
            Debug.LogError("DamageNumberController: floatingTextPrefab não foi atribuído!");
            return;
        }

        if (targetCanvas == null)
        {
            Debug.LogError("DamageNumberController: Canvas não encontrado! Crie um Canvas na cena ou atribua um no Inspector.");
            return;
        }

        // Converte posição do mundo (3D) para posição na tela
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        
        // Instancia o texto como filho do Canvas
        GameObject textObj = Instantiate(floatingTextPrefab, targetCanvas.transform);
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        
        if (rectTransform != null)
        {
            // Converte screen position para posição local do Canvas
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetCanvas.transform as RectTransform,
                screenPosition,
                targetCanvas.worldCamera,
                out Vector2 localPoint
            );
            
            // Adiciona offset vertical e horizontal aleatório
            localPoint.y += verticalOffset;
            localPoint.x += Random.Range(-horizontalSpread, horizontalSpread);
            
            rectTransform.anchoredPosition = localPoint;
        }
        
        FloatingTextAdvanced floatingText = textObj.GetComponentInChildren<FloatingTextAdvanced>(); // Usar GetInChildren aqui também é uma boa prática
        if (floatingText != null)
        {
            floatingText.Setup(text, color, fontSize);
        }
        else
        {
            Debug.LogError("O script FloatingTextAdvanced não foi encontrado no prefab instanciado!", textObj);
        }
    }

    /// <summary>
    /// Método auxiliar para determinar se um status é positivo ou negativo
    /// </summary>
    public void ShowStatusEffect(Vector3 position, StatusEffectType statusType, int duration, bool isRemoving = false)
    {
        string statusName = GetStatusEffectShortName(statusType);

        if (isRemoving)
        {
            ShowStatusRemoved(position, statusName);
        }
        else
        {
            bool isPositive = IsPositiveStatus(statusType);
            
            if (isPositive)
            {
                ShowStatusApplied(position, statusName, duration);
            }
            else
            {
                ShowDebuffApplied(position, statusName, duration);
            }
        }
    }

    /// <summary>
    /// Retorna nome curto do status para exibição
    /// </summary>
    private string GetStatusEffectShortName(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.AttackUp: return "ATK↑";
            case StatusEffectType.AttackDown: return "ATK↓";
            case StatusEffectType.DefenseUp: return "DEF↑";
            case StatusEffectType.DefenseDown: return "DEF↓";
            case StatusEffectType.SpeedUp: return "SPD↑";
            case StatusEffectType.SpeedDown: return "SPD↓";
            case StatusEffectType.Poison: return "Veneno";
            case StatusEffectType.Regeneration: return "Regen";
            case StatusEffectType.Vulnerable: return "Vulnerável";
            case StatusEffectType.Protected: return "Proteção";
            case StatusEffectType.Blessed: return "Bênção";
            case StatusEffectType.Cursed: return "Maldição";
            default: return type.ToString();
        }
    }

    /// <summary>
    /// Verifica se um status é positivo (buff) ou negativo (debuff)
    /// </summary>
    private bool IsPositiveStatus(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.AttackUp:
            case StatusEffectType.DefenseUp:
            case StatusEffectType.SpeedUp:
            case StatusEffectType.Regeneration:
            case StatusEffectType.Protected:
            case StatusEffectType.Blessed:
                return true;

            case StatusEffectType.AttackDown:
            case StatusEffectType.DefenseDown:
            case StatusEffectType.SpeedDown:
            case StatusEffectType.Poison:
            case StatusEffectType.Vulnerable:
            case StatusEffectType.Cursed:
                return false;

            default:
                return false;
        }
    }

    void OnValidate()
    {
        if (floatingTextPrefab == null)
        {
            Debug.LogWarning("DamageNumberController: floatingTextPrefab não foi atribuído!");
        }
        
        if (targetCanvas == null)
        {
            Debug.LogWarning("DamageNumberController: targetCanvas não foi atribuído! Tentarei encontrar automaticamente.");
        }
    }
}