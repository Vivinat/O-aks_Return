// Assets/Scripts/DefeatSceneDataCleaner.cs
// Este script funciona TANTO no Editor quanto em Builds (incluindo WebGL)

using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Limpa automaticamente os dados salvos quando o jogador perde
/// Funciona em Editor, Standalone e WebGL
/// </summary>
public class DefeatSceneDataCleaner : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private bool clearOnDefeatScene = true;
    [SerializeField] private bool clearOnApplicationQuit = true;
    [SerializeField] private string defeatSceneName = "Defeat_Scene";
    
    [Header("Files to Clear")]
    [SerializeField] private string[] jsonFilesToDelete = new string[]
    {
        "difficulty_modifiers.json",
    };
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private static DefeatSceneDataCleaner instance;

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnApplicationQuit()
    {
        if (clearOnApplicationQuit)
        {
            ClearAllData("Application Quit");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!clearOnDefeatScene) return;

        // Detecta se entrou na cena de derrota
        if (scene.name.Equals(defeatSceneName, System.StringComparison.OrdinalIgnoreCase))
        {
            ClearAllData($"Defeat Scene Loaded: {scene.name}");
        }
    }

    /// <summary>
    /// Limpa todos os arquivos JSON especificados
    /// Funciona em todas as plataformas incluindo WebGL
    /// </summary>
    private void ClearAllData(string reason)
    {
        DebugLog($"🧹 Iniciando limpeza de dados: {reason}");

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

        // Força o PlayerPrefs a salvar (em WebGL isso sincroniza o IndexedDB)
        PlayerPrefs.Save();

        DebugLog($"✅ Limpeza concluída! Deletados: {deletedCount}, Falhas: {failedCount}");
        
#if UNITY_WEBGL && !UNITY_EDITOR
        DebugLog("🌐 WebGL: Dados removidos do IndexedDB do navegador");
#else
        DebugLog($"💾 Path: {Application.persistentDataPath}");
#endif
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
                DebugLog($"✓ Deletado: {fileName}");
                return true;
            }
            else
            {
                DebugLog($"⊘ Arquivo não existe: {fileName}");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"✗ Erro ao deletar {fileName}: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// API pública para limpar dados manualmente
    /// </summary>
    public static void ClearDataManually()
    {
        if (instance != null)
        {
            instance.ClearAllData("Manual Clear");
        }
        else
        {
            Debug.LogWarning("[DefeatSceneDataCleaner] Instance não encontrada!");
        }
    }

    /// <summary>
    /// Reseta também os sistemas de dificuldade e behavior
    /// </summary>
    public static void FullReset()
    {
        if (instance != null)
        {
            instance.ClearAllData("Full Reset");
        }

        // Reseta os sistemas via suas instâncias
        if (DifficultySystem.Instance != null)
        {
            DifficultySystem.Instance.ResetModifiers();
        }

        if (PlayerBehaviorAnalyzer.Instance != null)
        {
            PlayerBehaviorAnalyzer.Instance.ClearAllData();
        }

        Debug.Log("🔄 Full Reset concluído!");
    }

    private void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"<color=orange>[DataCleaner]</color> {message}");
        }
    }

    // ==================== MÉTODOS DE DEBUG ====================

    [ContextMenu("Clear Data Now")]
    private void ClearDataNow()
    {
        ClearAllData("Manual Context Menu");
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
}