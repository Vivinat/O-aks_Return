using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;


// Exporta dados de todas as BattleActions para JSON
// Usado para resetar o sistema após morte

public class BattleActionDataExporter : EditorWindow
{
    private string outputPath = "Assets/Data/BattleActionsBalanceData.json";
    private bool prettyPrint = true;
    
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

        if (GUILayout.Button("Export BattleAction Data", GUILayout.Height(40)))
        {
            ExportBattleActionData();
        }

        GUILayout.Space(10);
        
        if (!File.Exists(outputPath))
        {
            EditorGUILayout.HelpBox("Arquivo JSON não encontrado!", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("Arquivo JSON pronto.", MessageType.None);
        }
    }

    void ExportBattleActionData()
    {
        string directory = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        BattleActionDatabase database = new BattleActionDatabase();
        
        database.paladinActions = LoadActionsFromFolder("Assets/Data/BattleActions/Paladin");
        database.rangerActions = LoadActionsFromFolder("Assets/Data/BattleActions/Ranger");
        database.druidActions = LoadActionsFromFolder("Assets/Data/BattleActions/Druid");
        database.consumableItems = LoadActionsFromFolder("Assets/Data/BattleActions/Consumables");
        database.otherActions = LoadActionsFromOtherFolders();
        database.statistics = CalculateStatistics(database);
        
        string json = JsonUtility.ToJson(database, prettyPrint);
        
        try
        {
            File.WriteAllText(outputPath, json);
            AssetDatabase.Refresh();
            
            int totalActions = database.paladinActions.Count + database.rangerActions.Count + 
                             database.druidActions.Count + database.consumableItems.Count +
                             database.otherActions.Count;
            
            EditorUtility.DisplayDialog("Export Successful", 
                $"Exported {totalActions} actions to:\n{outputPath}", "OK");
            
            Debug.Log($"BattleActions exportadas: {totalActions} ações");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Export Failed", $"Erro: {e.Message}", "OK");
        }
    }

    List<BattleActionData> LoadActionsFromFolder(string folderPath)
    {
        List<BattleActionData> actions = new List<BattleActionData>();
        
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            return actions;
        }

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
        
        if (!AssetDatabase.IsValidFolder(basePath)) return actions;

        HashSet<string> processedFolders = new HashSet<string>
        {
            "Assets/Data/BattleActions/Paladin",
            "Assets/Data/BattleActions/Ranger",
            "Assets/Data/BattleActions/Druid",
            "Assets/Data/BattleActions/Consumables"
        };

        string[] allGuids = AssetDatabase.FindAssets("t:BattleAction", new[] { basePath });
        
        foreach (string guid in allGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            bool isInProcessedFolder = processedFolders.Any(folder => path.StartsWith(folder));
            
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
        BattleActionData data = new BattleActionData
        {
            actionName = action.actionName,
            assetPath = path,
            description = action.description,
            targetType = action.targetType.ToString(),
            manaCost = action.manaCost,
            isConsumable = action.isConsumable,
            maxUses = action.maxUses,
            shopPrice = action.shopPrice
        };
        
        if (action.effects != null && action.effects.Count > 0)
        {
            data.effects = new List<EffectData>();
            
            foreach (var effect in action.effects)
            {
                data.effects.Add(new EffectData
                {
                    effectType = effect.effectType.ToString(),
                    power = effect.power,
                    statusEffect = effect.statusEffect.ToString(),
                    statusDuration = effect.statusDuration,
                    statusPower = effect.statusPower,
                    hasSelfEffect = effect.hasSelfEffect,
                    selfEffectType = effect.selfEffectType.ToString(),
                    selfEffectPower = effect.selfEffectPower,
                    selfStatusEffect = effect.selfStatusEffect.ToString(),
                    selfStatusDuration = effect.selfStatusDuration,
                    selfStatusPower = effect.selfStatusPower
                });
            }
        }
        
        return data;
    }

    BattleActionStatistics CalculateStatistics(BattleActionDatabase database)
    {
        BattleActionStatistics stats = new BattleActionStatistics();
        
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
            
            var actionsWithManaCost = allActions.Where(a => a.manaCost > 0).ToList();
            if (actionsWithManaCost.Count > 0)
            {
                stats.avgManaCost = (float)actionsWithManaCost.Average(a => a.manaCost);
                stats.minManaCost = actionsWithManaCost.Min(a => a.manaCost);
                stats.maxManaCost = actionsWithManaCost.Max(a => a.manaCost);
            }
            
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