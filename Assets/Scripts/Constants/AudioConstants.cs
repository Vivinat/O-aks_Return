// Assets/Scripts/Audio/AudioConstants.cs

using UnityEngine;

/// <summary>
/// Classe estática que centraliza todas as referências de áudio do jogo.
/// Configure os AudioClips no Inspector através de um componente AudioLibrary.
/// </summary>
public class AudioConstants : MonoBehaviour
{
    public static AudioConstants Instance { get; private set; }

    [Header("═══ UI SOUNDS ═══")]
    [Tooltip("Som quando um botão é clicado")]
    public AudioClip selectButton;
    
    [Tooltip("Som quando um menu é aberto")]
    public AudioClip openMenu;
    
    [Tooltip("Som quando uma ação inválida é tentada (sem recursos)")]
    public AudioClip cannotSelect;
    
    [Tooltip("Compra de Item")]
    public AudioClip itemBuy;

    [Header("═══ DEATH SOUNDS ═══")]
    [Tooltip("Som de morte para monstros/criaturas")]
    public AudioClip deathMonster;
    
    [Tooltip("Som de morte para humanos/humanoides")]
    public AudioClip deathHuman;

    [Header("═══ BATTLE SOUNDS ═══")]
    [Tooltip("Som de derrota na batalha")]
    public AudioClip defeatSound;
    
    [Tooltip("Som de texto sendo digitado")]
    public AudioClip textTypeSound;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ AudioConstants inicializado");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Static Helper Methods

    /// <summary>
    /// Toca o som de seleção de botão
    /// </summary>
    public static void PlayButtonSelect()
    {
        if (Instance != null && Instance.selectButton != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(Instance.selectButton);
        }
    }

    /// <summary>
    /// Toca o som de abertura de menu
    /// </summary>
    public static void PlayMenuOpen()
    {
        if (Instance != null && Instance.openMenu != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(Instance.openMenu);
        }
    }

    /// <summary>
    /// Toca o som de ação inválida
    /// </summary>
    public static void PlayCannotSelect()
    {
        if (Instance != null && Instance.cannotSelect != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(Instance.cannotSelect);
        }
    }

    /// <summary>
    /// Toca um som de morte específico
    /// </summary>
    public static void PlayDeathSound(AudioClip deathSound)
    {
        if (deathSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(deathSound);
        }
    }
    
    public static void PlayItemBuy()
    {
        if (Instance.itemBuy != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(Instance.itemBuy);
        }
    }

    /// <summary>
    /// Toca o som de derrota
    /// </summary>
    public static void PlayDefeat()
    {
        if (Instance != null && Instance.defeatSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(Instance.defeatSound);
        }
    }

    /// <summary>
    /// Toca o som de digitação de texto
    /// </summary>
    public static void PlayTextType()
    {
        if (Instance != null && Instance.textTypeSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(Instance.textTypeSound);
        }
    }

    #endregion

    void OnValidate()
    {
        // Validação no Editor
        if (selectButton == null)
            Debug.LogWarning("AudioConstants: selectButton não foi atribuído!");
        
        if (openMenu == null)
            Debug.LogWarning("AudioConstants: openMenu não foi atribuído!");
        
        if (cannotSelect == null)
            Debug.LogWarning("AudioConstants: cannotSelect não foi atribuído!");
    }
}