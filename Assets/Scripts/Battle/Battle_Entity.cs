// Assets/Scripts/Battle/BattleEntity.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // Adicionado para a corrotina

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

    // Controlador de animações
    private BattleAnimationController animationController;

    void Awake()
    {
        // Garante que o controlador de animação exista
        animationController = GetComponent<BattleAnimationController>();
        if (animationController == null)
        {
            animationController = gameObject.AddComponent<BattleAnimationController>();
        }
    }

    void Start()
    {
        currentHp = characterData.maxHp;
        currentMp = characterData.maxMp;
        currentAtb = Random.Range(0, 20);

        UpdateATBBar();
        UpdateHPBar();
        UpdateMPBar();
        UpdateValueTexts(); // NOVO: Atualiza os textos no início
    }

    // Método para o BattleManager configurar o material de flash
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

        currentAtb += characterData.speed * deltaTime * 5f;
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

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        int damageTaken = Mathf.Max(1, damageAmount - characterData.defense);
        currentHp -= damageTaken;
        Debug.Log($"{characterData.characterName} recebeu {damageTaken} de dano!");

        // Aciona a animação de dano no jogador
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
        UpdateValueTexts(); // NOVO: Atualiza o texto após tomar dano
    }

    public void Heal(int healAmount)
    {
        if (isDead) return;
        
        currentHp = Mathf.Min(currentHp + healAmount, characterData.maxHp);
        Debug.Log($"{characterData.characterName} curou {healAmount} de vida!");
        
        UpdateHPBar();
        UpdateValueTexts(); // NOVO: Atualiza o texto após curar
    }

    public bool ConsumeMana(int manaCost)
    {
        if (currentMp >= manaCost)
        {
            currentMp -= manaCost;
            UpdateMPBar();
            UpdateValueTexts(); // NOVO: Atualiza o texto após consumir mana
            return true;
        }
        Debug.Log($"{characterData.characterName} não tem MP suficiente!");
        return false;
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

        // Desativa os sliders da HUD imediatamente
        DisableHUDElements();

        // Aciona a animação de morte no jogador
        if (animationController != null)
        {
            animationController.OnDeath();
        }

        // Desativa o sprite após um delay para a animação tocar
        StartCoroutine(DeactivateSpriteAfterDelay());
    }

    // Método para desativar elementos da HUD quando o personagem morre
    private void DisableHUDElements()
    {
        // Opção 1: Desativa completamente os sliders
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

        // NOVO: Esconde os textos de valores também
        if (hpValueText != null)
        {
            hpValueText.gameObject.SetActive(false);
        }
        
        if (mpValueText != null)
        {
            mpValueText.gameObject.SetActive(false);
        }

        Debug.Log($"HUD de {characterData.characterName} desativada");
    }

    // Método alternativo para fazer fade dos sliders em vez de desativar
    private void FadeHUDElements()
    {
        // Opção 2: Faz fade dos sliders para 50% de transparência
        SetSliderAlpha(atbBar, 0.3f);
        SetSliderAlpha(hpBar, 0.3f);
        SetSliderAlpha(mpBar, 0.3f);

        // NOVO: Faz fade dos textos também
        SetTextAlpha(hpValueText, 0.3f);
        SetTextAlpha(mpValueText, 0.3f);

        Debug.Log($"HUD de {characterData.characterName} com fade aplicado");
    }

    // Método auxiliar para alterar a transparência de um slider
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

    // NOVO: Método auxiliar para alterar a transparência de um texto
    private void SetTextAlpha(TextMeshProUGUI text, float alpha)
    {
        if (text == null) return;

        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }

    // Método público para reativar a HUD (útil para debug ou reviver)
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

        // NOVO: Reativa os textos também
        if (hpValueText != null)
        {
            hpValueText.gameObject.SetActive(true);
        }
        
        if (mpValueText != null)
        {
            mpValueText.gameObject.SetActive(true);
        }

        // Restaura a opacidade total
        SetSliderAlpha(atbBar, 1f);
        SetSliderAlpha(hpBar, 1f);
        SetSliderAlpha(mpBar, 1f);
        SetTextAlpha(hpValueText, 1f);
        SetTextAlpha(mpValueText, 1f);

        Debug.Log($"HUD de {characterData.characterName} reativada");
    }

    // Desativa apenas o renderer para a lógica continuar existindo se necessário
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

    // ===== NOVO MÉTODO PARA ATUALIZAR TEXTOS DE VALORES =====

    /// <summary>
    /// NOVO: Atualiza os textos que mostram os valores numéricos de HP e MP
    /// </summary>
    private void UpdateValueTexts()
    {
        if (isDead) return;

        // Atualiza texto de HP (para todos os personagens)
        if (hpValueText != null)
        {
            hpValueText.text = $"{currentHp}/{characterData.maxHp}";
        }

        // Atualiza texto de MP (geralmente só para o player)
        if (mpValueText != null)
        {
            mpValueText.text = $"{currentMp}/{characterData.maxMp}";
        }
    }

    // ===== MÉTODOS PÚBLICOS PARA ACESSO AOS VALORES =====

    /// <summary>
    /// NOVO: Retorna o HP atual
    /// </summary>
    public int GetCurrentHP()
    {
        return currentHp;
    }

    /// <summary>
    /// NOVO: Retorna o MP atual
    /// </summary>
    public int GetCurrentMP()
    {
        return currentMp;
    }

    /// <summary>
    /// NOVO: Retorna o HP máximo
    /// </summary>
    public int GetMaxHP()
    {
        return characterData.maxHp;
    }

    /// <summary>
    /// NOVO: Retorna o MP máximo
    /// </summary>
    public int GetMaxMP()
    {
        return characterData.maxMp;
    }

    /// <summary>
    /// NOVO: Força uma atualização dos textos (útil para debug ou mudanças externas)
    /// </summary>
    public void ForceUpdateValueTexts()
    {
        UpdateValueTexts();
    }
}