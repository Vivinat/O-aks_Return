using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BattleEntity : MonoBehaviour
{
    public Character characterData;
    public Slider atbBar;
    public Slider hpBar;
    public Slider mpBar;

    [Header("Text Values UI")]
    [Tooltip("Texto que mostra o valor atual de HP (ex: '85/100')")]
    public TextMeshProUGUI hpValueText;
    
    [Tooltip("Texto que mostra o valor atual de MP (ex: '30/50') - Apenas para o player")]
    public TextMeshProUGUI mpValueText;

    // Status de batalha
    private int currentHp;
    public int currentMp;
    private float currentAtb;
    private const float ATB_MAX = 100f;

    public bool isReady = false;
    public bool isDead = false;

    // Status Effects System
    private List<StatusEffect> activeStatusEffects = new List<StatusEffect>();

    // Controlador de animações
    private BattleAnimationController animationController;
    
    void Awake()
    {
        animationController = GetComponent<BattleAnimationController>();
        if (animationController == null)
        {
            animationController = gameObject.AddComponent<BattleAnimationController>();
        }
    }
    
    void Start()
    {
        if (characterData != null)
        {
            // Garante que a HUD esteja ligada e inicializa os status.
            EnableHUDElements(); 

            currentHp = characterData.maxHp; 
            currentMp = characterData.maxMp; 
            currentAtb = Random.Range(0, 20); 

            UpdateATBBar(); 
            UpdateHPBar(); 
            UpdateMPBar(); 
            UpdateValueTexts(); 
        }
        
        currentHp = characterData.maxHp;
        currentMp = characterData.maxMp;
        currentAtb = Random.Range(0, 20);

        // Garante que a HUD esteja visível caso tenha sido desativada antes
        EnableHUDElements();

        UpdateATBBar();
        UpdateHPBar();
        UpdateMPBar();
        UpdateValueTexts();
    }

    public void SetupAnimationController(Material flashMat)
    {
        if (animationController != null)
        {
            animationController.SetFlashMaterial(flashMat);
        }
    }

    public void UpdateATB(float deltaTime)
    {
        if (isDead || isReady) return;

        // Apply speed modifiers from status effects
        float speedModifier = GetSpeedModifier();
        float effectiveSpeed = characterData.speed * (1f + speedModifier / 100f);

        currentAtb += effectiveSpeed * deltaTime * 5f;
        if (currentAtb >= ATB_MAX)
        {
            currentAtb = ATB_MAX;
            isReady = true;
        }
        UpdateATBBar();
    }

    public void ResetATB()
    {
        currentAtb = 0;
        isReady = false;
        UpdateATBBar();
    }

    public void TakeDamage(int damageAmount, BattleEntity attacker = null, bool ignoresDefense = false)
    {
        if (isDead) return;

        int baseDamage;
        if (ignoresDefense)
        {
            baseDamage = damageAmount; // Dano direto, sem cálculo de defesa
        }
        else
        {
            // Cálculo de dano normal com defesa
            int effectiveDefense = characterData.defense + GetDefenseModifier();
            baseDamage = Mathf.Max(1, damageAmount - effectiveDefense);
        }
        
        // Aplica multiplicadores de dano (Vulnerable, Protected)
        float damageMultiplier = GetDamageMultiplier();
        int finalDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);
        
        // Registra o hit individual
        if (characterData.team == Team.Player)
        {
            BehaviorAnalysisIntegration.OnPlayerHitReceived(finalDamage);
        }
        
        currentHp -= finalDamage;
        Debug.Log($"{characterData.characterName} received {finalDamage} damage!");
        
        if (DamageNumberController.Instance != null)
        {
            Debug.Log("chamando numero flutuante");
            bool isCritical = finalDamage >= (characterData.maxHp * 0.3f); // Crítico se for 30%+ do HP máximo
            DamageNumberController.Instance.ShowDamage(transform.position, finalDamage, isCritical);
        }

        if (attacker != null)
        {
            BehaviorAnalysisIntegration.OnPlayerDamageReceived(this, attacker, finalDamage);
        }

        if (animationController != null)
        {
            animationController.OnTakeDamage();
        }

        if (currentHp <= 0)
        {
            currentHp = 0;
            Die();
        }
    
        UpdateHPBar();
        UpdateValueTexts();
    }


    public void Heal(int healAmount)
    {
        if (isDead) return;
    
        int oldHp = currentHp;
        currentHp = Mathf.Min(currentHp + healAmount, characterData.maxHp);
        int actualHealing = currentHp - oldHp;
    
        Debug.Log($"{characterData.characterName} healed {actualHealing} HP!");
        
        if (DamageNumberController.Instance != null && actualHealing >= 0)
        {
            DamageNumberController.Instance.ShowHealing(transform.position, actualHealing);
        }
    
        UpdateHPBar();
        UpdateValueTexts();
    }

    public void RestoreMana(int manaAmount)
    {
        if (isDead) return;
        
        int oldMp = currentMp;
        currentMp = Mathf.Min(currentMp + manaAmount, characterData.maxMp);
        int actualManaRestore = currentMp - oldMp;
        
        if (DamageNumberController.Instance != null && actualManaRestore >= 0)
        {
            DamageNumberController.Instance.ShowManaRestore(transform.position, actualManaRestore);
        }
        
        UpdateMPBar();
        UpdateValueTexts();
    }

    public bool ConsumeMana(int manaCost)
    {
        if (currentMp >= manaCost)
        {
            currentMp -= manaCost;
            UpdateMPBar();
            UpdateValueTexts();
            return true;
        }
        Debug.Log($"{characterData.characterName} doesn't have enough MP!");
        return false;
    }

    // Status Effects Management
    public void ApplyStatusEffect(StatusEffectType type, int power, int duration)
    {
        if (type == StatusEffectType.None || duration <= 0) return;

        // Check if we already have this status effect
        StatusEffect existingEffect = activeStatusEffects.FirstOrDefault(e => e.type == type);
        if (existingEffect != null)
        {
            // Refresh the effect with new values
            existingEffect.power = power;
            existingEffect.remainingTurns = duration;
            Debug.Log($"{characterData.characterName}'s {existingEffect.effectName} refreshed!");
        }
        else
        {
            // Add new status effect
            StatusEffect newEffect = new StatusEffect(type, power, duration);
            activeStatusEffects.Add(newEffect);
            Debug.Log($"{characterData.characterName} gains {newEffect.effectName}!");
            if (DamageNumberController.Instance != null)
            {
                DamageNumberController.Instance.ShowStatusEffect(transform.position, type, duration, false);
            }
        }
    }

    public void ProcessStatusEffectsTurn()
    {
        if (isDead) return;

        // Loop reverso de 'for'
        for (int i = activeStatusEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect effect = activeStatusEffects[i];
        
            bool shouldRemove = effect.ProcessTurnEffect(this);

            // Se o personagem morreu durante o efeito, paramos tudo
            if (isDead) break;

            // Se o efeito expirou, removemos diretamente da lista
            if (shouldRemove)
            {
                Debug.Log($"{characterData.characterName} loses {effect.effectName}");
                if (DamageNumberController.Instance != null)
                {
                    DamageNumberController.Instance.ShowStatusEffect(transform.position, effect.type, 0, true);
                }
                activeStatusEffects.RemoveAt(i);
            }
        }
    }

    private float GetSpeedModifier()
    {
        float modifier = 0f;
        foreach (StatusEffect effect in activeStatusEffects)
        {
            if (effect.type == StatusEffectType.SpeedUp)
                modifier += effect.power;
            else if (effect.type == StatusEffectType.SpeedDown)
                modifier -= effect.power;
        }
        return modifier;
    }

    private int GetDefenseModifier()
    {
        int modifier = 0;
        foreach (StatusEffect effect in activeStatusEffects)
        {
            if (effect.type == StatusEffectType.DefenseUp)
                modifier += effect.power;
            else if (effect.type == StatusEffectType.DefenseDown)
                modifier -= effect.power;
        }
        return modifier;
    }

    private int GetAttackModifier()
    {
        int modifier = 0;
        foreach (StatusEffect effect in activeStatusEffects)
        {
            if (effect.type == StatusEffectType.AttackUp)
                modifier += effect.power;
            else if (effect.type == StatusEffectType.AttackDown)
                modifier -= effect.power;
        }
        return modifier;
    }

    private float GetDamageMultiplier()
    {
        float multiplier = 1f;
        foreach (StatusEffect effect in activeStatusEffects)
        {
            if (effect.type == StatusEffectType.Vulnerable)
                multiplier += effect.power / 100f;
            else if (effect.type == StatusEffectType.Protected)
                multiplier -= effect.power / 100f;
        }
        return Mathf.Max(0.1f, multiplier); // Minimum 10% damage
    }

    public int GetModifiedAttackPower(int basePower)
    {
        int modifier = GetAttackModifier();
        return Mathf.Max(1, basePower + modifier);
    }

    public List<StatusEffect> GetActiveStatusEffects()
    {
        return new List<StatusEffect>(activeStatusEffects);
    }

    public void ClearAllStatusEffects()
    {
        activeStatusEffects.Clear();
        Debug.Log($"{characterData.characterName} has all status effects cleared!");
    }

    // Aciona o efeito de flash quando executa uma ação
    public void OnExecuteAction()
    {
        if (animationController != null)
        {
            animationController.OnExecuteAction();
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{characterData.characterName} foi derrotado!");

        // Clear all status effects on death
        ClearAllStatusEffects();

        // Se for jogador, tenta ativar segunda chance
        if (characterData.team == Team.Player)
        {
            TryActivateSecondChance();
            return; // Não continua o processo de morte se a segunda chance for ativada
        }
        
        if (currentHp == 0 && characterData.team == Team.Player)
        {
            BehaviorAnalysisIntegration.OnPlayerDeath(this);
        }

        // Desativa os sliders da HUD imediatamente (apenas inimigos chegam aqui)
        DisableHUDElements();
    
        if (characterData.deathSound != null)
        {
            AudioConstants.PlayDeathSound(characterData.deathSound);
        }

        // Aciona a animação de morte
        if (animationController != null)
        {
            animationController.OnDeath();
        }

        // Desativa o sprite após um delay para a animação tocar
        StartCoroutine(DeactivateSpriteAfterDelay());
    }
    
    /// <summary>
    /// Tenta ativar o sistema de segunda chance
    /// </summary>
    private void TryActivateSecondChance()
    {
        if (DeathNegotiationManager.Instance == null)
        {
            Debug.LogWarning("DeathNegotiationManager não encontrado! Prosseguindo com morte normal.");
            CompleteDeath();
            return;
        }
        
        if (DeathNegotiationManager.Instance.HasUsedSecondChance())
        {
            Debug.Log("Segunda chance já foi usada. Morte definitiva.");
            CompleteDeath();
            return;
        }
        
        BattleManager battleManager = FindObjectOfType<BattleManager>();
        if (battleManager == null)
        {
            Debug.LogWarning("BattleManager não encontrado!");
            CompleteDeath();
            return;
        }
        
        Debug.Log("=== ATIVANDO SISTEMA DE SEGUNDA CHANCE ===");
        
        // Salva referência do personagem ativo atual
        BattleEntity currentActiveCharacter = battleManager.GetActiveCharacter();
        
        // Inicia negociação
        DeathNegotiationManager.Instance.StartNegotiation(this, battleManager, (accepted) =>
        {
            if (!accepted)
            {
                // Jogador recusou ou não conseguiu negociar
                Debug.Log("Negociação falhou. Morte confirmada.");
                CompleteDeath();
            }
            else
            {
                Debug.Log("Negociação aceita! Jogador revivido.");
                
                ClearNegativeStatusEffects();
                Debug.Log("Status effects negativos removidos após ressurreição!");
                
                if (currentActiveCharacter != null && !currentActiveCharacter.isDead)
                {
                    Debug.Log($"Resetando ATB de {currentActiveCharacter.characterData.characterName} para evitar turno duplo");
                    currentActiveCharacter.ResetATB();
                }
                
                if (battleManager != null)
                {
                    battleManager.currentState = BattleState.RUNNING;
                }
            }
        });
    }
    
    /// <summary>
    /// Remove apenas status effects negativos
    /// </summary>
    public void ClearNegativeStatusEffects()
    {
        List<StatusEffectType> negativeEffects = new List<StatusEffectType>
        {
            StatusEffectType.Poison,
            StatusEffectType.AttackDown,
            StatusEffectType.DefenseDown,
            StatusEffectType.SpeedDown,
            StatusEffectType.Vulnerable,
            StatusEffectType.Cursed,
            StatusEffectType.DefenseDown
        };
    
        int removedCount = activeStatusEffects.RemoveAll(effect => negativeEffects.Contains(effect.type));
    
        if (removedCount > 0)
        {
            Debug.Log($"{characterData.characterName} teve {removedCount} status effects negativos removidos!");
        }
    }

    /// <summary>
    /// Processo de morte (
    /// </summary>
    private void CompleteDeath()
    {
        DisableHUDElements();
        
        if (characterData.deathSound != null)
        {
            AudioConstants.PlayDeathSound(characterData.deathSound);
        }

        if (animationController != null)
        {
            animationController.OnDeath();
        }

        StartCoroutine(DeactivateSpriteAfterDelay());
    }
    
    private void DisableHUDElements()
    {
        if (atbBar != null)
        {
            atbBar.gameObject.SetActive(false);
        }
    
        if (hpBar != null)
        {
            hpBar.gameObject.SetActive(false);
        }
    
        if (mpBar != null)
        {
            mpBar.gameObject.SetActive(false);
        }

        if (hpValueText != null)
        {
            hpValueText.gameObject.SetActive(false);
        }
    
        if (mpValueText != null)
        {
            mpValueText.gameObject.SetActive(false);
        }

        string entityName = characterData != null ? characterData.characterName : gameObject.name;
        Debug.Log($"HUD de {entityName} desativada");
    }

    // Método alternativo p
    private void FadeHUDElements()
    {
        SetSliderAlpha(atbBar, 0.3f);
        SetSliderAlpha(hpBar, 0.3f);
        SetSliderAlpha(mpBar, 0.3f);

        SetTextAlpha(hpValueText, 0.3f);
        SetTextAlpha(mpValueText, 0.3f);

        Debug.Log($"HUD de {characterData.characterName} com fade aplicado");
    }

    private void SetSliderAlpha(Slider slider, float alpha)
    {
        if (slider == null) return;

        // Altera a transparência de todos os componentes Image do slider
        Image[] images = slider.GetComponentsInChildren<Image>();
        foreach (Image img in images)
        {
            Color color = img.color;
            color.a = alpha;
            img.color = color;
        }
    }

    private void SetTextAlpha(TextMeshProUGUI text, float alpha)
    {
        if (text == null) return;

        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }

    public void EnableHUDElements()
    {
        if (atbBar != null)
        {
            atbBar.gameObject.SetActive(true);
        }
        
        if (hpBar != null)
        {
            hpBar.gameObject.SetActive(true);
        }
        
        if (mpBar != null)
        {
            mpBar.gameObject.SetActive(true);
        }

        if (hpValueText != null)
        {
            hpValueText.gameObject.SetActive(true);
        }
        
        if (mpValueText != null)
        {
            mpValueText.gameObject.SetActive(true);
        }

        SetSliderAlpha(atbBar, 1f);
        SetSliderAlpha(hpBar, 1f);
        SetSliderAlpha(mpBar, 1f);
        SetTextAlpha(hpValueText, 1f);
        SetTextAlpha(mpValueText, 1f);

        Debug.Log($"HUD de {characterData.characterName} reativada");
    }

    private IEnumerator DeactivateSpriteAfterDelay()
    {
        // Espera para a animação de morte
        yield return new WaitForSeconds(1.5f);
        
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = false;
        }
    }

    private void UpdateATBBar()
    {
        if (atbBar != null && !isDead) 
            atbBar.value = currentAtb / ATB_MAX;
    }

    private void UpdateHPBar()
    {
        if (hpBar != null && !isDead) 
            hpBar.value = (float)currentHp / characterData.maxHp;
    }

    private void UpdateMPBar()
    {
        if (mpBar != null && !isDead) 
            mpBar.value = (float)currentMp / characterData.maxMp;
    }


    /// <summary>
    /// Atualiza textos
    /// </summary>
    private void UpdateValueTexts()
    {
        if (isDead) return;

        // Atualiza texto de HP
        if (hpValueText != null)
        {
            hpValueText.text = $"{currentHp}";
        }

        // Atualiza texto de MP 
        if (mpValueText != null)
        {
            mpValueText.text = $"{currentMp}";
        }
    }
    
    /// <summary>
    /// Retorna o HP atual
    /// </summary>
    public int GetCurrentHP()
    {
        return currentHp;
    }

    /// <summary>
    /// Retorna o MP atual
    /// </summary>
    public int GetCurrentMP()
    {
        return currentMp;
    }

    /// <summary>
    /// Retorna o HP máximo
    /// </summary>
    public int GetMaxHP()
    {
        return characterData.maxHp;
    }

    /// <summary>
    /// Retorna o MP máximo
    /// </summary>
    public int GetMaxMP()
    {
        return characterData.maxMp;
    }

    /// <summary>
    /// Força uma atualização dos textos
    /// </summary>
    public void ForceUpdateValueTexts()
    {
        UpdateValueTexts();
    }
    
    /// <summary>
    /// Retorna a defesa base do personagem
    /// </summary>
    public int GetBaseDefense()
    {
        if (characterData == null) return 0;
        return characterData.defense;
    }
    
    /// <summary>
    /// Retorna a defesa atual (base + modificadores de status)
    /// </summary>
    public int GetCurrentDefense()
    {
        return GetBaseDefense() + GetDefenseModifier();
    }
}