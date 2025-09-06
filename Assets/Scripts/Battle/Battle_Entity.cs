// Assets/Scripts/Battle/BattleEntity.cs

using UnityEngine;
using UnityEngine.UI;

public class BattleEntity : MonoBehaviour
{
    public Character characterData;
    public Slider atbBar;
    public Slider hpBar; // Opcional: Adicione uma barra de vida

    // Status de batalha
    private int currentHp;
    private int currentMp;
    private float currentAtb;
    private const float ATB_MAX = 100f;

    public bool isReady = false;
    public bool isDead = false;

    void Start()
    {
        currentHp = characterData.maxHp;
        currentMp = characterData.maxMp;
        currentAtb = Random.Range(0, 20); // Começa com ATB aleatório para variar
        UpdateATBBar();
        UpdateHPBar();
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

    /// <summary>
    /// Aplica dano a esta entidade.
    /// </summary>
    /// <param name="damageAmount">O dano base da ação.</param>
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        // Fórmula de dano simples: Dano - Defesa
        int damageTaken = Mathf.Max(1, damageAmount - characterData.defense);
        currentHp -= damageTaken;
        
        Debug.Log($"{characterData.characterName} recebeu {damageTaken} de dano!");

        if (currentHp <= 0)
        {
            currentHp = 0;
            Die();
        }
        UpdateHPBar();
    }
    
    /// <summary>
    /// Cura esta entidade.
    /// </summary>
    /// <param name="healAmount">A quantidade de HP a restaurar.</param>
    public void Heal(int healAmount)
    {
        if (isDead) return;

        currentHp += healAmount;
        if (currentHp > characterData.maxHp)
        {
            currentHp = characterData.maxHp;
        }
        
        Debug.Log($"{characterData.characterName} curou {healAmount} de vida!");
        UpdateHPBar();
    }
    
    /// <summary>
    /// Verifica se tem MP suficiente e o consome.
    /// </summary>
    /// <returns>True se o MP foi consumido com sucesso.</returns>
    public bool ConsumeMana(int manaCost)
    {
        if (currentMp >= manaCost)
        {
            currentMp -= manaCost;
            return true;
        }

        Debug.Log($"{characterData.characterName} não tem MP suficiente!");
        return false;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{characterData.characterName} foi derrotado!");
        // Aqui você pode desativar o objeto ou tocar uma animação de morte
        gameObject.SetActive(false); 
    }

    private void UpdateATBBar()
    {
        if (atbBar != null) atbBar.value = currentAtb / ATB_MAX;
    }

    private void UpdateHPBar()
    {
        if (hpBar != null) hpBar.value = (float)currentHp / characterData.maxHp;
    }
}