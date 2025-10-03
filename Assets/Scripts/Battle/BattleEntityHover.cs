// Assets/Scripts/Battle/BattleEntityHover.cs

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
        // Não faz nada se o personagem está morto
        if (battleEntity.isDead) return;
        
        // Só mostra hover se não estiver em seleção de alvo
        // (TargetSelector tem prioridade)
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
        
        // Se não há status effects, mostra apenas informações básicas
        if (string.IsNullOrEmpty(tooltipDescription))
        {
            tooltipDescription = GetBasicInfo();
        }
        
        // Calcula posição do tooltip acima do personagem
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
        
        info.AppendLine($"HP: {battleEntity.GetCurrentHP()}/{battleEntity.GetMaxHP()}");
        
        return info.ToString();
    }
    
    private string GetStatusEffectsDescription()
    {
        var activeEffects = battleEntity.GetActiveStatusEffects();
        
        if (activeEffects == null || activeEffects.Count == 0)
        {
            return string.Empty;
        }
        
        StringBuilder description = new StringBuilder();
        
        // Informações básicas primeiro
        description.AppendLine(GetBasicInfo());
        description.AppendLine("<b>Status Effects:</b>");
        
        foreach (var effect in activeEffects)
        {
            string effectLine = GetStatusEffectDescription(effect);
            description.AppendLine(effectLine);
        }
        
        return description.ToString();
    }
    
    private string GetStatusEffectDescription(StatusEffect effect)
    {
        string icon = GetStatusEffectIcon(effect.type);
        string colorCode = GetStatusEffectColor(effect.type);
        string shortDescription = GetShortEffectDescription(effect.type, effect.power);
        
        return $"<color={colorCode}>{icon} {effect.effectName}</color> ({effect.remainingTurns} turnos)\n   {shortDescription}";
    }
    
    private string GetStatusEffectIcon(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.AttackUp: return "⚔️";
            case StatusEffectType.AttackDown: return "🗡️";
            case StatusEffectType.DefenseUp: return "🛡️";
            case StatusEffectType.DefenseDown: return "🪓";
            case StatusEffectType.SpeedUp: return "⚡";
            case StatusEffectType.SpeedDown: return "🐌";
            case StatusEffectType.Poison: return "☠️";
            case StatusEffectType.Regeneration: return "💚";
            case StatusEffectType.Vulnerable: return "💔";
            case StatusEffectType.Protected: return "✨";
            case StatusEffectType.Blessed: return "🌟";
            case StatusEffectType.Cursed: return "💀";
            default: return "•";
        }
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
    
    private string GetShortEffectDescription(StatusEffectType type, int power)
    {
        switch (type)
        {
            case StatusEffectType.AttackUp:
                return $"Ataque aumentado em +{power}";
            
            case StatusEffectType.AttackDown:
                return $"Ataque reduzido em -{power}";
            
            case StatusEffectType.DefenseUp:
                return $"Defesa aumentada em +{power}";
            
            case StatusEffectType.DefenseDown:
                return $"Defesa reduzida em -{power}";
            
            case StatusEffectType.SpeedUp:
                return $"Velocidade aumentada em {power}%";
            
            case StatusEffectType.SpeedDown:
                return $"Velocidade reduzida em {power}%";
            
            case StatusEffectType.Poison:
                return $"Perde {power} HP por turno";
            
            case StatusEffectType.Regeneration:
                return $"Recupera {power} HP por turno";
            
            case StatusEffectType.Vulnerable:
                return $"Recebe {power}% mais dano";
            
            case StatusEffectType.Protected:
                return $"Recebe {power}% menos dano";
            
            case StatusEffectType.Blessed:
                return $"Cura divina de {power} HP por turno";
            
            case StatusEffectType.Cursed:
                return $"Maldição causa {power} de dano por turno";
            
            default:
                return "Efeito desconhecido";
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