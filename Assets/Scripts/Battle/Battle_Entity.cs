// Assets/Scripts/Battle/BattleEntity.cs

using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Adicionado para a corrotina

public class BattleEntity : MonoBehaviour
{
    public Character characterData;
    public Slider atbBar;
    public Slider hpBar;
    public Slider mpBar;

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
    }

    // NOVO: Método para o BattleManager configurar o material de flash
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
    }

    public void Heal(int healAmount)
    {
        if (isDead) return;
        currentHp = Mathf.Min(currentHp + healAmount, characterData.maxHp);
        Debug.Log($"{characterData.characterName} curou {healAmount} de vida!");
        UpdateHPBar();
    }

    public bool ConsumeMana(int manaCost)
    {
        if (currentMp >= manaCost)
        {
            currentMp -= manaCost;
            UpdateMPBar();
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

        // Aciona a animação de morte no jogador
        if (animationController != null)
        {
            animationController.OnDeath();
        }

        // Desativa o sprite após um delay para a animação tocar
        StartCoroutine(DeactivateSpriteAfterDelay());
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
        if (atbBar != null) atbBar.value = currentAtb / ATB_MAX;
    }

    private void UpdateHPBar()
    {
        if (hpBar != null) hpBar.value = (float)currentHp / characterData.maxHp;
    }

    private void UpdateMPBar()
    {
        if (mpBar != null) mpBar.value = (float)currentMp / characterData.maxMp;
    }
}