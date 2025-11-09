using UnityEngine;

//referências de áudio do jogo.
public class AudioConstants : MonoBehaviour
{
    public static AudioConstants Instance { get; private set; }
    
    public AudioClip selectButton;
    public AudioClip openMenu;
    public AudioClip cannotSelect;
    public AudioClip itemBuy;
    public AudioClip deathMonster;
    public AudioClip deathHuman;
    public AudioClip defeatSound;
    public AudioClip textTypeSound;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("AudioConstants inicializado");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Static Helper Methods
    
    // Toca o som de seleção de botão
    public static void PlayButtonSelect()
    {
        if (Instance != null && Instance.selectButton != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(Instance.selectButton);
        }
    }

    // Toca o som de abertura de menu
    public static void PlayMenuOpen()
    {
        if (Instance != null && Instance.openMenu != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(Instance.openMenu);
        }
    }

    // Toca o som de ação inválida
    public static void PlayCannotSelect()
    {
        if (Instance != null && Instance.cannotSelect != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(Instance.cannotSelect);
        }
    }

    // Toca um som de morte específico
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

    // Toca o som de derrota
    public static void PlayDefeat()
    {
        if (Instance != null && Instance.defeatSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(Instance.defeatSound);
        }
    }

    // Toca o som de digitação de texto
    public static void PlayTextType()
    {
        if (Instance != null && Instance.textTypeSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(Instance.textTypeSound);
        }
    }

    #endregion
    
}