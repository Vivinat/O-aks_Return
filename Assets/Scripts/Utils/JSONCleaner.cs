// Assets/Scripts/Utils/JSONCleaner.cs
// Este script funciona TANTO no Editor quanto em Builds (incluindo WebGL)

using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Limpa automaticamente os dados salvos em momentos específicos
/// DEVE estar em um GameObject na primeira cena (MainMenu) como DontDestroyOnLoad
/// Funciona em Editor, Standalone e WebGL
/// </summary>
public class JSONCleaner : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private bool clearOnMainMenu = true; // Limpa quando carrega o MainMenu
    [SerializeField] private bool clearOnDefeatScene = true;
    [SerializeField] private bool clearOnApplicationQuit = true;
    [SerializeField] private string defeatSceneName = "Defeat_Scene";
    [SerializeField] private string mainMenuSceneName = "Main_Menu";
    
    [Header("Files to Clear")]
    [SerializeField] private string[] jsonFilesToDelete = new string[]
    {
        "difficulty_modifiers.json",
    };
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private static JSONCleaner instance;
    private bool hasCleanedOnMainMenu = false; // Evita limpar múltiplas vezes
    private bool hasStartedGame = false; // Detecta se já saiu do menu principal

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Se já está no MainMenu ao iniciar, limpa imediatamente
            if (clearOnMainMenu && IsCurrentSceneMainMenu())
            {
                FullCleanup("Main Menu - Awake (Início do jogo)");
                hasCleanedOnMainMenu = true;
            }
            
            // Registra callback para detectar mudanças de cena
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            DebugLog("JSONCleaner inicializado (DontDestroyOnLoad)");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void OnApplicationQuit()
    {
        if (clearOnApplicationQuit)
        {
            FullCleanup("Application Quit");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isMainMenu = scene.name.Equals(mainMenuSceneName, System.StringComparison.OrdinalIgnoreCase);
        bool isDefeatScene = scene.name.Equals(defeatSceneName, System.StringComparison.OrdinalIgnoreCase);
        
        // Detecta se entrou no MainMenu
        if (clearOnMainMenu && isMainMenu)
        {
            // Se já havia iniciado um jogo antes (saiu do menu), faz limpeza COMPLETA
            if (hasStartedGame)
            {
                DebugLog("Retornou ao MainMenu após jogar - LIMPEZA COMPLETA");
                FullCleanup($"Return to Main Menu from game");
                hasStartedGame = false;
                hasCleanedOnMainMenu = true;
            }
            // Se ainda não limpou nesta sessão do MainMenu (primeira vez)
            else if (!hasCleanedOnMainMenu)
            {
                FullCleanup($"Main Menu Loaded: {scene.name}");
                hasCleanedOnMainMenu = true;
            }
        }
        // Se saiu do MainMenu ou Defeat Scene, marca que o jogo foi iniciado
        else if (!isMainMenu && !isDefeatScene)
        {
            hasStartedGame = true;
            hasCleanedOnMainMenu = false;
            DebugLog($"Jogo iniciado - cena: {scene.name}");
        }
        
        // Detecta se entrou na cena de derrota
        if (clearOnDefeatScene && isDefeatScene)
        {
            FullCleanup($"Defeat Scene Loaded: {scene.name}");
        }
    }

    /// <summary>
    /// Verifica se a cena atual é o MainMenu
    /// </summary>
    private bool IsCurrentSceneMainMenu()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        return currentScene.name.Equals(mainMenuSceneName, System.StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// LIMPEZA COMPLETA: JSON + GameStateResetter
    /// </summary>
    private void FullCleanup(string reason)
    {
        DebugLog($"🧹 === INICIANDO LIMPEZA COMPLETA ===");
        DebugLog($"Razão: {reason}");

        // 1. Deleta arquivos JSON
        ClearJsonFiles();

        // 2. Chama GameStateResetter para limpar TUDO
        ResetGameState();

        // 3. Reseta sistemas em memória
        ResetSystems();

        // Força o PlayerPrefs a salvar
        PlayerPrefs.Save();

        DebugLog($"✅ === LIMPEZA COMPLETA FINALIZADA ===");
        
#if UNITY_WEBGL && !UNITY_EDITOR
        DebugLog("🌐 WebGL: Dados removidos do IndexedDB do navegador");
#else
        DebugLog($"💾 Path: {Application.persistentDataPath}");
#endif
    }

    /// <summary>
    /// Deleta apenas os arquivos JSON
    /// </summary>
    private void ClearJsonFiles()
    {
        DebugLog("📄 Deletando arquivos JSON...");
        
        int deletedCount = 0;
        int failedCount = 0;

        foreach (string fileName in jsonFilesToDelete)
        {
            if (DeleteJsonFile(fileName))
            {
                deletedCount++;
            }
            else
            {
                failedCount++;
            }
        }

        DebugLog($"   Deletados: {deletedCount}, Falhas: {failedCount}");
    }

    /// <summary>
    /// Deleta um arquivo JSON específico
    /// </summary>
    private bool DeleteJsonFile(string fileName)
    {
        try
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                DebugLog($"   ✓ Deletado: {fileName}");
                return true;
            }
            else
            {
                DebugLog($"   ⊘ Não existe: {fileName}");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"   ✗ Erro ao deletar {fileName}: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Chama o GameStateResetter para resetar TUDO
    /// </summary>
    private void ResetGameState()
    {
        DebugLog("🔄 Chamando GameStateResetter...");
        
        try
        {
            GameStateResetter.ResetGameState();
            DebugLog("   ✓ GameStateResetter executado com sucesso");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"   ✗ Erro ao executar GameStateResetter: {e.Message}");
            Debug.LogError($"   Stack trace: {e.StackTrace}");
        }
    }

    /// <summary>
    /// Reseta os sistemas em memória (DifficultySystem e PlayerBehaviorAnalyzer)
    /// NOTA: Isso também é feito pelo GameStateResetter, mas garantimos aqui
    /// </summary>
    private void ResetSystems()
    {
        DebugLog("💾 Resetando sistemas em memória...");
        
        // Reseta DifficultySystem
        if (DifficultySystem.Instance != null)
        {
            DifficultySystem.Instance.ResetModifiers();
            DebugLog("   ✓ DifficultySystem resetado");
        }

        // Reseta PlayerBehaviorAnalyzer
        if (PlayerBehaviorAnalyzer.Instance != null)
        {
            PlayerBehaviorAnalyzer.Instance.ClearAllData();
            DebugLog("   ✓ PlayerBehaviorAnalyzer limpo");
        }
    }

    /// <summary>
    /// API pública para limpar dados manualmente
    /// </summary>
    public static void ClearDataManually()
    {
        if (instance != null)
        {
            instance.FullCleanup("Manual Clear");
        }
        else
        {
            Debug.LogWarning("[JSONCleaner] Instance não encontrada!");
        }
    }

    /// <summary>
    /// Reseta também os sistemas de dificuldade e behavior
    /// </summary>
    public static void FullReset()
    {
        if (instance != null)
        {
            instance.FullCleanup("Full Reset");
        }
        else
        {
            Debug.LogWarning("[JSONCleaner] Instance não encontrada para FullReset!");
        }
    }

    private void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"<color=orange>[JSONCleaner]</color> {message}");
        }
    }

    // ==================== MÉTODOS DE DEBUG ====================

    [ContextMenu("Clear Data Now")]
    private void ClearDataNow()
    {
        FullCleanup("Manual Context Menu");
    }

    [ContextMenu("Full Reset (Clear + Reset Systems)")]
    private void FullResetMenu()
    {
        FullReset();
    }

    [ContextMenu("Show File Paths")]
    private void ShowFilePaths()
    {
        Debug.Log($"=== FILE PATHS ===");
        Debug.Log($"Persistent Data Path: {Application.persistentDataPath}");
        
        foreach (string fileName in jsonFilesToDelete)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            bool exists = File.Exists(filePath);
            Debug.Log($"{fileName}: {(exists ? "✓ EXISTS" : "✗ NOT FOUND")}");
            
            if (exists)
            {
                FileInfo info = new FileInfo(filePath);
                Debug.Log($"  Size: {info.Length} bytes, Modified: {info.LastWriteTime}");
            }
        }
    }

    [ContextMenu("Toggle Main Menu Cleaning")]
    private void ToggleMainMenuCleaning()
    {
        clearOnMainMenu = !clearOnMainMenu;
        Debug.Log($"Clear on Main Menu: {(clearOnMainMenu ? "ENABLED" : "DISABLED")}");
    }

    [ContextMenu("Show Current State")]
    private void ShowCurrentState()
    {
        Debug.Log($"=== JSON CLEANER STATE ===");
        Debug.Log($"Has Started Game: {hasStartedGame}");
        Debug.Log($"Has Cleaned On Main Menu: {hasCleanedOnMainMenu}");
        Debug.Log($"Current Scene: {SceneManager.GetActiveScene().name}");
        Debug.Log($"Is Main Menu: {IsCurrentSceneMainMenu()}");
    }
}