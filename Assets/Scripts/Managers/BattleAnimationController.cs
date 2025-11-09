using UnityEngine;
using System.Collections;

public class BattleAnimationController : MonoBehaviour
{
    private Animator characterAnimator;
    private SpriteRenderer spriteRenderer;

    private Color originalColor;
    private Coroutine flashCoroutine;
    private Coroutine hurtResetCoroutine;

    [SerializeField] private float flashDuration = 0.05f;
    [SerializeField] private int flashCount = 5;

    [SerializeField] private float hurtAnimationDuration = 0.5f;

    void Awake()
    {
        characterAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
    }

    public void SetFlashMaterial(Material newFlashMaterial)
    {
    }

    public void OnTakeDamage()
    {
        if (characterAnimator != null)
        {
            if (hurtResetCoroutine != null)
            {
                StopCoroutine(hurtResetCoroutine);
                hurtResetCoroutine = null;
            }

            characterAnimator.SetBool("IsHurt", false);
            characterAnimator.ResetTrigger("ToIdle");
            characterAnimator.SetTrigger("IsHurt");
            
            hurtResetCoroutine = StartCoroutine(ResetToIdleAfterHurt());
        }
    }

    private IEnumerator ResetToIdleAfterHurt()
    {
        yield return new WaitForSeconds(hurtAnimationDuration);
        
        if (characterAnimator != null)
        {
            characterAnimator.SetBool("IsHurt", false);
            characterAnimator.SetTrigger("ToIdle");
        }

        hurtResetCoroutine = null;
    }

    public void OnDeath()
    {
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
            characterAnimator.SetBool("IsHurt", false);
            characterAnimator.SetBool("IsDead", true);
        }
    }

    public void OnExecuteAction()
    {
        if (spriteRenderer != null)
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(ColorFlashEffect());
        }
    }

    private IEnumerator ColorFlashEffect()
    {
        Color black = Color.black;
        Color white = Color.white;

        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = black;
            yield return new WaitForSeconds(flashDuration);

            spriteRenderer.color = white;
            yield return new WaitForSeconds(flashDuration);
        }
        
        spriteRenderer.color = originalColor;
        flashCoroutine = null;
    }

    private void OnDisable()
    {
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

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
}