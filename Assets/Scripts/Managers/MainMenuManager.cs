// Assets/Scripts/Managers/MainMenuManager.cs

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Gerencia o menu principal do jogo
/// NOTA: A limpeza de dados é feita automaticamente pelo JSONCleaner ao carregar esta cena
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button exitGameButton;
    
    [Header("Scene Configuration")]
    [SerializeField] private string firstMapScene = "Map1";

    void Start()
    {
        SetupButtons();
        
        // Garante que o tempo está normal (caso tenha voltado de uma pausa)
        Time.timeScale = 1f;
        
        Debug.Log("MainMenu carregado");
    }

    private void SetupButtons()
    {
        // Botão Iniciar Jogo
        if (startGameButton != null)
        {
            startGameButton.onClick.RemoveAllListeners();
            startGameButton.onClick.AddListener(StartGame);
        }
        else
        {
            Debug.LogWarning("MainMenu: startGameButton não foi atribuído!");
        }

        // Botão Opções
        if (optionsButton != null)
        {
            optionsButton.onClick.RemoveAllListeners();
            optionsButton.onClick.AddListener(OpenOptions);
        }
        else
        {
            Debug.LogWarning("MainMenu: optionsButton não foi atribuído!");
        }

        // Botão Sair
        if (exitGameButton != null)
        {
            exitGameButton.onClick.RemoveAllListeners();
            exitGameButton.onClick.AddListener(ExitGame);
        }
    }

    /// <summary>
    /// Inicia o jogo carregando a primeira cena
    /// </summary>
    public void StartGame()
    {
        Debug.Log($"Iniciando jogo - Carregando: {firstMapScene}");
        AudioConstants.PlayButtonSelect();
        
        // Limpa dados básicos de progressão
        ResetGameData();
        
        // Carrega a primeira cena do jogo
        SceneManager.LoadScene(firstMapScene);
    }

    /// <summary>
    /// Abre o menu de opções
    /// </summary>
    public void OpenOptions()
    {
        Debug.Log("Abrindo menu de opções");
        
        if (OptionsMenu.Instance != null)
        {
            OptionsMenu.Instance.OpenOptionsMenu();
        }
        else
        {
            Debug.LogError("OptionsMenu não encontrado! Certifique-se de que existe na cena.");
        }
    }

    /// <summary>
    /// Sai do jogo
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("Saindo do jogo...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    /// <summary>
    /// Reseta dados básicos do jogo para um novo início
    /// NOTA: A limpeza de JSONs é feita pelo JSONCleaner automaticamente
    /// </summary>
    private void ResetGameData()
    {
        // Limpa dados de progressão de nós
        PlayerPrefs.DeleteKey("LastCompletedNode");
        PlayerPrefs.DeleteKey("CompletedBossNode");
        PlayerPrefs.DeleteKey("NextSceneAfterBoss");
        
        // Limpa estados de mapas
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearMapData("Map1");
            GameManager.Instance.ClearMapData("Map2");
            GameManager.Instance.ClearMapData("Map3");
        }
        
        PlayerPrefs.Save();
        
        Debug.Log("Dados básicos do jogo resetados para novo início");
    }

    void OnValidate()
    {
        // Validação no Editor
        if (startGameButton == null)
            Debug.LogWarning("MainMenuManager: startGameButton não foi atribuído!");
            
        if (optionsButton == null)
            Debug.LogWarning("MainMenuManager: optionsButton não foi atribuído!");
            
        if (string.IsNullOrEmpty(firstMapScene))
            Debug.LogWarning("MainMenuManager: firstMapScene não foi definido!");
    }
}