using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Limpa automaticamente os dados salvos em momentos específicos
/// </summary>
public class JSONCleaner : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private bool clearOnMainMenu = true;
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
    private bool hasCleanedOnMainMenu = false;
    private bool hasStartedGame = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (clearOnMainMenu && IsCurrentSceneMainMenu())
            {
                FullCleanup("Main Menu - Awake (Início do jogo)");
                hasCleanedOnMainMenu = true;
            }
            
            SceneManager.sceneLoaded += OnSceneLoaded;
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
        
        if (clearOnMainMenu && isMainMenu)
        {
            if (hasStartedGame)
            {
                FullCleanup($"Return to Main Menu from game");
                hasStartedGame = false;
                hasCleanedOnMainMenu = true;
            }
            else if (!hasCleanedOnMainMenu)
            {
                FullCleanup($"Main Menu Loaded: {scene.name}");
                hasCleanedOnMainMenu = true;
            }
        }
        else if (!isMainMenu && !isDefeatScene)
        {
            hasStartedGame = true;
            hasCleanedOnMainMenu = false;
        }
        
        if (clearOnDefeatScene && isDefeatScene)
        {
            FullCleanup($"Defeat Scene Loaded: {scene.name}");
        }
    }

    private bool IsCurrentSceneMainMenu()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        return currentScene.name.Equals(mainMenuSceneName, System.StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// limpeza
    /// </summary>
    private void FullCleanup(string reason)
    {
        ClearJsonFiles();
        ResetGameState();
        ResetSystems();
        PlayerPrefs.Save();
    }

    private void ClearJsonFiles()
    {
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
    }

    private bool DeleteJsonFile(string fileName)
    {
        try
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao deletar {fileName}: {e.Message}");
            return false;
        }
    }

    private void ResetGameState()
    {
        try
        {
            GameStateResetter.ResetGameState();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao executar GameStateResetter: {e.Message}");
        }
    }

    private void ResetSystems()
    {
        if (DifficultySystem.Instance != null)
        {
            DifficultySystem.Instance.ResetModifiers();
        }

        if (PlayerBehaviorAnalyzer.Instance != null)
        {
            PlayerBehaviorAnalyzer.Instance.ClearAllData();
        }
    }

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
        Debug.Log($"Persistent Data Path: {Application.persistentDataPath}");
        
        foreach (string fileName in jsonFilesToDelete)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            bool exists = File.Exists(filePath);
            Debug.Log($"{fileName}: {(exists ? "EXISTS" : "NOT FOUND")}");
            
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
        Debug.Log($"Has Started Game: {hasStartedGame}");
        Debug.Log($"Has Cleaned On Main Menu: {hasCleanedOnMainMenu}");
        Debug.Log($"Current Scene: {SceneManager.GetActiveScene().name}");
        Debug.Log($"Is Main Menu: {IsCurrentSceneMainMenu()}");
    }
}