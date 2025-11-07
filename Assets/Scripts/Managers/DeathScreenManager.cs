using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Gerencia a tela de morte do jogador
/// </summary>
public class DeathScreenManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI deathMessageText;
    [SerializeField] private Button returnToMenuButton;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float messageDelay = 0.5f;
    
    [Header("Death Messages")]
    [SerializeField] private string[] deathMessages = new string[]
    {
        "Derrotado novamente? Que novidade!",
        "Volta pro asilo, vovô!",
        "Ave, Logrif! Teu lugar de direito lhe espera!",
        "Não se fazem mais arquidemônios como antigamente...",
        "A parte boa de ser imortal é que pode perder pra sempre!",
        "Você vem sempre aqui?",
        "Insira gargalhadas incompreensíveis de seres além de sua compreensão",
        "Um senhor do escuro jovem nunca passaria por essa vergonha!",
        "O universo vai se lembrar de sua morte. Para bem, e para mal."
    };
    
    void Start()
    {
        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
        
        AudioConstants.PlayDefeat();
        AudioManager.Instance.StopMusic();
        Time.timeScale = 1f;
        
        StartCoroutine(ShowDeathScreen());
    }
    
    private IEnumerator ShowDeathScreen()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        yield return new WaitForSeconds(messageDelay);
        
        if (deathMessageText != null)
        {
            string randomMessage = deathMessages[Random.Range(0, deathMessages.Length)];
            deathMessageText.text = randomMessage;
        }
        
        if (canvasGroup != null)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }
    }
    
    private void ReturnToMainMenu()
    {
        GameStateResetter.ResetGameState();
        SceneManager.LoadScene("MainMenu");
    }
    
    void OnValidate()
    {
        if (deathMessageText == null)
            Debug.LogWarning("DeathScreenManager: deathMessageText não foi atribuído!");
            
        if (returnToMenuButton == null)
            Debug.LogWarning("DeathScreenManager: returnToMenuButton não foi atribuído!");
    }
}