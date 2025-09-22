// Assets/Scripts/Battle/BattleAnimationController.cs (CORRIGIDO)

using UnityEngine;
using System.Collections;

public class BattleAnimationController : MonoBehaviour
{
    private Animator characterAnimator;
    private SpriteRenderer spriteRenderer;

    // Efeito de Flash (agora baseado em cor)
    private Color originalColor;
    private Coroutine flashCoroutine;
    private Coroutine hurtResetCoroutine; // NOVO: Corrotina específica para reset de hurt

    [Header("Flash Settings (Color Swap)")]
    [SerializeField] private float flashDuration = 0.05f; // Tempo de cada "piscada" (preto ou branco)
    [SerializeField] private int flashCount = 5; // Quantas vezes pisca (preto-branco-preto...)

    [Header("Animation Settings")]
    [SerializeField] private float hurtAnimationDuration = 0.5f; // Duração da animação de dano

    void Awake()
    {
        // Pega as referências automaticamente
        characterAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // Tenta no próprio objeto
        
        // Se não encontrou no próprio objeto, tenta nos filhos (para flexibilidade)
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color; // Salva a cor original do sprite
        }
        else
        {
            Debug.LogWarning($"Nenhum SpriteRenderer encontrado em {gameObject.name} ou em seus filhos!");
        }
    }

    // Mantido para compatibilidade com BattleEntity
    public void SetFlashMaterial(Material newFlashMaterial)
    {
        // Este método não é mais usado para a funcionalidade de flash de cor,
        // mas é mantido para evitar erros de compilação em BattleEntity.
    }

    // Chamado por BattleEntity quando toma dano
    public void OnTakeDamage()
    {
        if (characterAnimator != null)
        {
            // CORREÇÃO: Para a corrotina anterior se ela estiver rodando
            if (hurtResetCoroutine != null)
            {
                StopCoroutine(hurtResetCoroutine);
                hurtResetCoroutine = null;
            }

            // CORREÇÃO: Reseta explicitamente o estado antes de aplicar o novo
            characterAnimator.SetBool("IsHurt", false);
            characterAnimator.ResetTrigger("ToIdle"); // Limpa triggers anteriores
            
            // Aplica a nova animação de dano
            characterAnimator.SetTrigger("IsHurt");
            
            // Inicia nova corrotina de reset
            hurtResetCoroutine = StartCoroutine(ResetToIdleAfterHurt());
        }
    }

    // Corrotina que reseta para Idle após a animação de dano
    private IEnumerator ResetToIdleAfterHurt()
    {
        // Espera a duração da animação de dano
        yield return new WaitForSeconds(hurtAnimationDuration);
        
        // Força o retorno ao Idle apenas se o Animator ainda existe
        if (characterAnimator != null)
        {
            characterAnimator.SetBool("IsHurt", false);
            characterAnimator.SetTrigger("ToIdle");
        }

        // Limpa a referência da corrotina
        hurtResetCoroutine = null;
    }

    // Chamado por BattleEntity quando morre
    public void OnDeath()
    {
        // CORREÇÃO: Para todas as corrotinas quando morre
        if (hurtResetCoroutine != null)
        {
            StopCoroutine(hurtResetCoroutine);
            hurtResetCoroutine = null;
        }

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        if (characterAnimator != null)
        {
            // Limpa qualquer estado de hurt antes de aplicar morte
            characterAnimator.SetBool("IsHurt", false);
            characterAnimator.SetBool("IsDead", true);
        }
    }

    // Chamado por BattleEntity quando ataca
    public void OnExecuteAction()
    {
        if (spriteRenderer != null)
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine); // Para qualquer flash anterior
            }
            flashCoroutine = StartCoroutine(ColorFlashEffect());
        }
    }

    private IEnumerator ColorFlashEffect()
    {
        // Define as cores para o flash
        Color black = Color.black; // Cor preta (0,0,0,255)
        Color white = Color.white; // Cor branca (255,255,255,255)

        for (int i = 0; i < flashCount; i++)
        {
            // PISCA PARA PRETO
            spriteRenderer.color = black;
            yield return new WaitForSeconds(flashDuration);

            // PISCA PARA BRANCO
            spriteRenderer.color = white;
            yield return new WaitForSeconds(flashDuration);
        }
        
        // Garante que a cor volte ao original no final
        spriteRenderer.color = originalColor;
        flashCoroutine = null;
    }

    // NOVO: Método para limpar tudo quando o objeto é destruído ou desabilitado
    private void OnDisable()
    {
        // Para todas as corrotinas ativas
        if (hurtResetCoroutine != null)
        {
            StopCoroutine(hurtResetCoroutine);
            hurtResetCoroutine = null;
        }

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        // Restaura a cor original se possível
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
}