// Assets/Scripts/Editor/BattleActionDataExporter.cs

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Exporta dados de todas as BattleActions para JSON para facilitar balanceamento
/// </summary>
public class BattleActionDataExporter : EditorWindow
{
    private string outputPath = "Assets/Data/BattleActionsBalanceData.json";
    private bool prettyPrint = true;
    private Vector2 scrollPosition;
    
    [MenuItem("Tools/Export BattleActions to JSON")]
    public static void ShowWindow()
    {
        GetWindow<BattleActionDataExporter>("BattleAction Exporter");
    }

    void OnGUI()
    {
        GUILayout.Label("BattleAction Data Exporter", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Caminho de saída
        GUILayout.Label("Output Path:", EditorStyles.label);
        EditorGUILayout.BeginHorizontal();
        outputPath = EditorGUILayout.TextField(outputPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.SaveFilePanel("Save BattleAction Data", "Assets/Data", "BattleActionsBalanceData", "json");
            if (!string.IsNullOrEmpty(path))
            {
                outputPath = "Assets" + path.Substring(Application.dataPath.Length);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        prettyPrint = EditorGUILayout.Toggle("Pretty Print JSON", prettyPrint);
        
        GUILayout.Space(20);

        // Botão de exportação
        if (GUILayout.Button("Export BattleAction Data", GUILayout.Height(40)))
        {
            ExportBattleActionData();
        }

        GUILayout.Space(10);
        
        // Informações
        EditorGUILayout.HelpBox(
            "Este script exporta todas as BattleActions de:\n" +
            "• Assets/Data/BattleActions/Paladin/\n" +
            "• Assets/Data/BattleActions/Ranger/\n" +
            "• Assets/Data/BattleActions/Druid/\n" +
            "• Todos os subdiretórios em BattleActions/\n\n" +
            "O JSON gerado pode ser usado para balanceamento e análise.",
            MessageType.Info
        );
    }

    void ExportBattleActionData()
    {
        BattleActionDatabase database = new BattleActionDatabase();
        
        // Carrega ações de cada categoria
        database.paladinActions = LoadActionsFromFolder("Assets/Data/BattleActions/Paladin");
        database.rangerActions = LoadActionsFromFolder("Assets/Data/BattleActions/Ranger");
        database.druidActions = LoadActionsFromFolder("Assets/Data/BattleActions/Druid");
        
        // Carrega itens consumíveis se existirem
        database.consumableItems = LoadActionsFromFolder("Assets/Data/BattleActions/Consumables");
        
        // Carrega outras ações (qualquer outra pasta em BattleActions)
        database.otherActions = LoadActionsFromOtherFolders();
        
        // Calcula estatísticas
        database.statistics = CalculateStatistics(database);
        
        // Converte para JSON
        string json = JsonUtility.ToJson(database, prettyPrint);
        
        // Salva arquivo
        try
        {
            File.WriteAllText(outputPath, json);
            AssetDatabase.Refresh();
            
            int totalActions = database.paladinActions.Count + database.rangerActions.Count + 
                             database.druidActions.Count + database.consumableItems.Count +
                             database.otherActions.Count;
            
            EditorUtility.DisplayDialog(
                "Export Successful", 
                $"Successfully exported {totalActions} battle actions to:\n{outputPath}", 
                "OK"
            );
            
            Debug.Log($"✅ BattleAction data exported successfully!");
            Debug.Log($"📊 Total actions: {totalActions}");
            Debug.Log($"   - Paladin: {database.paladinActions.Count}");
            Debug.Log($"   - Ranger: {database.rangerActions.Count}");
            Debug.Log($"   - Druid: {database.druidActions.Count}");
            Debug.Log($"   - Consumables: {database.consumableItems.Count}");
            Debug.Log($"   - Other: {database.otherActions.Count}");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog(
                "Export Failed", 
                $"Failed to export battle action data:\n{e.Message}", 
                "OK"
            );
            Debug.LogError($"Failed to export battle action data: {e.Message}");
        }
    }

    List<BattleActionData> LoadActionsFromFolder(string folderPath)
    {
        List<BattleActionData> actions = new List<BattleActionData>();
        
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogWarning($"Folder not found: {folderPath}");
            return actions;
        }

        // Encontra todos os assets de BattleAction na pasta
        string[] guids = AssetDatabase.FindAssets("t:BattleAction", new[] { folderPath });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BattleAction action = AssetDatabase.LoadAssetAtPath<BattleAction>(path);
            
            if (action != null)
            {
                actions.Add(ConvertToBattleActionData(action, path));
            }
        }
        
        return actions;
    }

    List<BattleActionData> LoadActionsFromOtherFolders()
    {
        List<BattleActionData> actions = new List<BattleActionData>();
        
        string basePath = "Assets/Data/BattleActions";
        if (!AssetDatabase.IsValidFolder(basePath))
        {
            return actions;
        }

        // Pastas já processadas
        HashSet<string> processedFolders = new HashSet<string>
        {
            "Assets/Data/BattleActions/Paladin",
            "Assets/Data/BattleActions/Ranger",
            "Assets/Data/BattleActions/Druid",
            "Assets/Data/BattleActions/Consumables"
        };

        // Busca todas as BattleActions em BattleActions
        string[] allGuids = AssetDatabase.FindAssets("t:BattleAction", new[] { basePath });
        
        foreach (string guid in allGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            // Verifica se não está em uma pasta já processada
            bool isInProcessedFolder = false;
            foreach (string folder in processedFolders)
            {
                if (path.StartsWith(folder))
                {
                    isInProcessedFolder = true;
                    break;
                }
            }
            
            if (!isInProcessedFolder)
            {
                BattleAction action = AssetDatabase.LoadAssetAtPath<BattleAction>(path);
                if (action != null)
                {
                    actions.Add(ConvertToBattleActionData(action, path));
                }
            }
        }
        
        return actions;
    }

    BattleActionData ConvertToBattleActionData(BattleAction action, string path)
    {
        BattleActionData data = new BattleActionData();
        
        data.actionName = action.actionName;
        data.assetPath = path;
        data.description = action.description;
        data.targetType = action.targetType.ToString();
        data.manaCost = action.manaCost;
        data.isConsumable = action.isConsumable;
        data.maxUses = action.maxUses;
        data.shopPrice = action.shopPrice;
        
        // Processa efeitos
        if (action.effects != null && action.effects.Count > 0)
        {
            data.effects = new List<EffectData>();
            
            foreach (var effect in action.effects)
            {
                EffectData effectData = new EffectData();
                effectData.effectType = effect.effectType.ToString();
                effectData.power = effect.power;
                effectData.statusEffect = effect.statusEffect.ToString();
                effectData.statusDuration = effect.statusDuration;
                effectData.statusPower = effect.statusPower;
                effectData.hasSelfEffect = effect.hasSelfEffect;
                effectData.selfEffectType = effect.selfEffectType.ToString();
                effectData.selfEffectPower = effect.selfEffectPower;
                effectData.selfStatusEffect = effect.selfStatusEffect.ToString();
                effectData.selfStatusDuration = effect.selfStatusDuration;
                effectData.selfStatusPower = effect.selfStatusPower;
                
                data.effects.Add(effectData);
            }
        }
        
        return data;
    }

    BattleActionStatistics CalculateStatistics(BattleActionDatabase database)
    {
        BattleActionStatistics stats = new BattleActionStatistics();
        
        // Combina todas as ações
        List<BattleActionData> allActions = new List<BattleActionData>();
        allActions.AddRange(database.paladinActions);
        allActions.AddRange(database.rangerActions);
        allActions.AddRange(database.druidActions);
        allActions.AddRange(database.consumableItems);
        allActions.AddRange(database.otherActions);
        
        if (allActions.Count > 0)
        {
            stats.totalActions = allActions.Count;
            stats.consumableCount = allActions.Count(a => a.isConsumable);
            stats.nonConsumableCount = stats.totalActions - stats.consumableCount;
            
            // Estatísticas de mana
            var actionsWithManaCost = allActions.Where(a => a.manaCost > 0).ToList();
            if (actionsWithManaCost.Count > 0)
            {
                stats.avgManaCost = (float)actionsWithManaCost.Average(a => a.manaCost);
                stats.minManaCost = actionsWithManaCost.Min(a => a.manaCost);
                stats.maxManaCost = actionsWithManaCost.Max(a => a.manaCost);
            }
            
            // Estatísticas de poder
            var actionsWithPower = allActions.Where(a => a.effects != null && a.effects.Count > 0).ToList();
            if (actionsWithPower.Count > 0)
            {
                var powers = actionsWithPower.SelectMany(a => a.effects).Select(e => e.power).Where(p => p > 0).ToList();
                if (powers.Count > 0)
                {
                    stats.avgPower = (float)powers.Average();
                    stats.minPower = powers.Min();
                    stats.maxPower = powers.Max();
                }
            }
            
            // Distribuição por tipo de alvo
            stats.targetTypeDistribution = new Dictionary<string, int>();
            foreach (var action in allActions)
            {
                if (!stats.targetTypeDistribution.ContainsKey(action.targetType))
                {
                    stats.targetTypeDistribution[action.targetType] = 0;
                }
                stats.targetTypeDistribution[action.targetType]++;
            }
        }
        
        return stats;
    }
}

// ========== DATA STRUCTURES ==========

[System.Serializable]
public class BattleActionDatabase
{
    public List<BattleActionData> paladinActions = new List<BattleActionData>();
    public List<BattleActionData> rangerActions = new List<BattleActionData>();
    public List<BattleActionData> druidActions = new List<BattleActionData>();
    public List<BattleActionData> consumableItems = new List<BattleActionData>();
    public List<BattleActionData> otherActions = new List<BattleActionData>();
    public BattleActionStatistics statistics;
}

[System.Serializable]
public class BattleActionData
{
    public string actionName;
    public string assetPath;
    public string description;
    public string targetType;
    public int manaCost;
    public bool isConsumable;
    public int maxUses;
    public int shopPrice;
    public List<EffectData> effects = new List<EffectData>();
}

[System.Serializable]
public class EffectData
{
    public string effectType;
    public int power;
    public string statusEffect;
    public int statusDuration;
    public int statusPower;
    public bool hasSelfEffect;
    public string selfEffectType;
    public int selfEffectPower;
    public string selfStatusEffect;
    public int selfStatusDuration;
    public int selfStatusPower;
}

[System.Serializable]
public class BattleActionStatistics
{
    public int totalActions;
    public int consumableCount;
    public int nonConsumableCount;
    public float avgManaCost;
    public int minManaCost;
    public int maxManaCost;
    public float avgPower;
    public int minPower;
    public int maxPower;
    public Dictionary<string, int> targetTypeDistribution = new Dictionary<string, int>();
}