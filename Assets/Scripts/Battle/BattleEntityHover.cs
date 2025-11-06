using UnityEngine;
using System.Text;
using System.Linq;

/// <summary>
/// Componente para gerenciar hover visual e tooltip de status effects em personagens de batalha
/// </summary>
[RequireComponent(typeof(BattleEntity))]
public class BattleEntityHover : MonoBehaviour
{
    [Header("Highlight Settings")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float pulseDuration = 0.5f;
    [SerializeField] private float pulseIntensity = 0.3f;
    
    [Header("Tooltip Settings")]
    [SerializeField] private Vector2 tooltipOffset = new Vector2(0, 50);
    
    private BattleEntity battleEntity;
    private SpriteRenderer spriteRenderer;
    private BattleHUD battleHUD;
    private Color originalColor;
    private bool isHighlighted = false;
    private float pulseTimer = 0f;
    
    void Awake()
    {
        battleEntity = GetComponent<BattleEntity>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    void Start()
    {
        battleHUD = FindObjectOfType<BattleHUD>();
        
        if (battleHUD == null)
        {
            Debug.LogWarning($"BattleEntityHover em {gameObject.name}: BattleHUD não encontrado!");
        }
    }
    
    void Update()
    {
        if (isHighlighted && spriteRenderer != null && !battleEntity.isDead)
        {
            // Efeito de pulso suave
            pulseTimer += Time.deltaTime;
            float pulseValue = Mathf.Sin(pulseTimer * (2f * Mathf.PI / pulseDuration)) * pulseIntensity + 1f;
            
            Color currentColor = Color.Lerp(originalColor, highlightColor, pulseValue * 0.5f);
            spriteRenderer.color = currentColor;
        }
    }
    
    void OnMouseEnter()
    {
        if (battleEntity.isDead) return;
        
        // Só mostra hover se não estiver em seleção de alvo
        if (!IsInTargetSelectionMode())
        {
            StartHighlight();
            ShowStatusTooltip();
        }
    }
    
    void OnMouseExit()
    {
        StopHighlight();
        HideTooltip();
    }
    
    /// <summary>
    /// Verifica se estamos em modo de seleção de alvo
    /// </summary>
    private bool IsInTargetSelectionMode()
    {
        if (battleHUD == null) return false;
        
        return battleHUD.targetSelectionPanel != null && 
               battleHUD.targetSelectionPanel.activeSelf;
    }
    
    private void StartHighlight()
    {
        if (!isHighlighted && spriteRenderer != null)
        {
            isHighlighted = true;
            pulseTimer = 0f;
        }
    }
    
    private void StopHighlight()
    {
        if (isHighlighted && spriteRenderer != null)
        {
            isHighlighted = false;
            spriteRenderer.color = originalColor;
        }
    }
    
    private void ShowStatusTooltip()
    {
        if (battleHUD == null || battleEntity == null) return;
        
        string tooltipTitle = GetTooltipTitle();
        string tooltipDescription = GetStatusEffectsDescription();
        
        if (string.IsNullOrEmpty(tooltipDescription))
        {
            tooltipDescription = GetBasicInfo();
        }
        
        Vector3 worldPos = transform.position + (Vector3)tooltipOffset;
        
        battleHUD.ShowTooltip(tooltipTitle, tooltipDescription);
    }
    
    private void HideTooltip()
    {
        if (battleHUD != null)
        {
            battleHUD.HideTooltip();
        }
    }
    
    private string GetTooltipTitle()
    {
        if (battleEntity?.characterData == null) return "???";
        
        return battleEntity.characterData.characterName;
    }
    
    private string GetBasicInfo()
    {
        StringBuilder info = new StringBuilder();
        
        info.Append($"<b>HP:</b> {battleEntity.GetCurrentHP()}/{battleEntity.GetMaxHP()}");
        
        if (battleEntity.GetMaxMP() > 0)
        {
            info.Append($"  <b>MP:</b> {battleEntity.GetCurrentMP()}/{battleEntity.GetMaxMP()}");
        }
        info.AppendLine();
        
        // Linha 2: DEF
        int currentDef = battleEntity.GetCurrentDefense();
        int baseDef = battleEntity.GetBaseDefense();
        
        info.Append($"<b>DEF:</b> {FormatStatWithModifier(currentDef, baseDef)}");
        
        return info.ToString();
    }
    
    /// <summary>
    /// Formata uma stat mostrando modificadores em cores
    /// </summary>
    private string FormatStatWithModifier(int current, int baseValue)
    {
        if (current == baseValue)
        {
            return current.ToString();
        }
        else if (current > baseValue)
        {
            int diff = current - baseValue;
            return $"{current} <color=#90EE90>(+{diff})</color>";
        }
        else
        {
            int diff = baseValue - current;
            return $"{current} <color=#FF6B6B>(-{diff})</color>";
        }
    }
    
    private string GetStatusEffectsDescription()
    {
        var activeEffects = battleEntity.GetActiveStatusEffects();
        
        if (activeEffects == null || activeEffects.Count == 0)
        {
            return string.Empty;
        }
        
        StringBuilder description = new StringBuilder();
        
        description.AppendLine(GetBasicInfo());
        description.AppendLine();
        description.AppendLine("<b>Status:</b>");
        
        foreach (var effect in activeEffects)
        {
            string effectLine = GetStatusEffectDescriptionCompact(effect);
            description.AppendLine(effectLine);
        }
        
        return description.ToString();
    }
    
    /// <summary>
    /// Versão compacta da descrição de status effect
    /// </summary>
    private string GetStatusEffectDescriptionCompact(StatusEffect effect)
    {
        string icon = GetStatusEffectIcon();
        string colorCode = GetStatusEffectColor(effect.type);
        
        return $"<color={colorCode}>{icon} {effect.effectName} ({effect.remainingTurns}t)</color>";
    }
    
    private string GetStatusEffectIcon()
    {
        return "•";
    }
    
    private string GetStatusEffectColor(StatusEffectType type)
    {
        switch (type)
        {
            // Buffs positivos - Verde
            case StatusEffectType.AttackUp:
            case StatusEffectType.DefenseUp:
            case StatusEffectType.SpeedUp:
            case StatusEffectType.Regeneration:
            case StatusEffectType.Protected:
            case StatusEffectType.Blessed:
                return "#90EE90";
            
            // Debuffs negativos - Vermelho
            case StatusEffectType.AttackDown:
            case StatusEffectType.DefenseDown:
            case StatusEffectType.SpeedDown:
            case StatusEffectType.Poison:
            case StatusEffectType.Vulnerable:
            case StatusEffectType.Cursed:
                return "#FF6B6B";
            
            default:
                return "#FFFFFF";
        }
    }
    
    void OnDisable()
    {
        StopHighlight();
        HideTooltip();
    }
    
    void OnDestroy()
    {
        StopHighlight();
        HideTooltip();
    }
}