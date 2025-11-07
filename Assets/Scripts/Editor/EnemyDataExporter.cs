using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Exporta dados de todos os inimigos para JSON
/// Usado para resetar o sistema após morte
/// </summary>
public class EnemyDataExporter : EditorWindow
{
    private string outputPath = "Assets/Data/EnemyBalanceData.json";
    private bool prettyPrint = true;
    private Vector2 scrollPosition;
    
    [MenuItem("Tools/Export Enemy Data to JSON")]
    public static void ShowWindow()
    {
        GetWindow<EnemyDataExporter>("Enemy Data Exporter");
    }

    void OnGUI()
    {
        GUILayout.Label("Enemy Data Exporter", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Caminho de saída
        GUILayout.Label("Output Path:", EditorStyles.label);
        EditorGUILayout.BeginHorizontal();
        outputPath = EditorGUILayout.TextField(outputPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.SaveFilePanel("Save Enemy Data", "Assets/Data", "EnemyBalanceData", "json");
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
        if (GUILayout.Button("Export Enemy Data", GUILayout.Height(40)))
        {
            ExportEnemyData();
        }

        GUILayout.Space(10);
        
        // Informações
        EditorGUILayout.HelpBox(
            "Este script exporta todos os dados dos inimigos de:\n" +
            "• Assets/Data/Characters/Enemies/Druids/\n" +
            "• Assets/Data/Characters/Enemies/Warriors/\n" +
            "• Assets/Data/Characters/Enemies/Monsters/\n" +
            "• Assets/Data/Characters/Enemies/Bosses/\n\n" +
            "O JSON gerado pode ser usado para balanceamento e análise.",
            MessageType.Info
        );
        
        if (!File.Exists(outputPath))
        {
            EditorGUILayout.HelpBox("Arquivo JSON não encontrado!", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("Arquivo JSON pronto.", MessageType.None);
        }
    }

    void ExportEnemyData()
    {
        EnemyDatabase database = new EnemyDatabase();
        
        // Carrega inimigos
        database.druids = LoadEnemiesFromFolder("Assets/Data/Characters/Enemies/Druids");
        database.warriors = LoadEnemiesFromFolder("Assets/Data/Characters/Enemies/Paladins");
        database.monsters = LoadEnemiesFromFolder("Assets/Data/Characters/Enemies/Monsters");
        database.bosses = LoadEnemiesFromFolder("Assets/Data/Characters/Enemies/Bosses");
        
        database.statistics = CalculateStatistics(database);
        
        string json = JsonUtility.ToJson(database, prettyPrint);
        
        try
        {
            string directory = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(outputPath, json);
            AssetDatabase.Refresh();
            
            int totalEnemies = database.druids.Count + database.warriors.Count + 
                             database.monsters.Count + database.bosses.Count;
            
            EditorUtility.DisplayDialog(
                "Export Successful", 
                $"Exported {totalEnemies} enemies to:\n{outputPath}", 
                "OK"
            );
            
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog(
                "Export Failed", 
                $"Failed to export enemy data:\n{e.Message}", 
                "OK"
            );
            Debug.LogError($"Failed to export enemy data: {e.Message}");
        }
    }

    List<EnemyData> LoadEnemiesFromFolder(string folderPath)
    {
        List<EnemyData> enemies = new List<EnemyData>();
        
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogWarning($"Folder not found: {folderPath}");
            return enemies;
        }

        string[] guids = AssetDatabase.FindAssets("t:Character", new[] { folderPath });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Character character = AssetDatabase.LoadAssetAtPath<Character>(path);
            
            if (character != null && character.team == Team.Enemy)
            {
                EnemyData enemyData = new EnemyData();
                enemyData.name = character.characterName;
                enemyData.assetPath = path;
                enemyData.maxHp = character.maxHp;
                enemyData.maxMp = character.maxMp;
                enemyData.defense = character.defense;
                enemyData.speed = character.speed;
                
                if (character.battleActions != null)
                {
                    enemyData.actions = new List<ActionData>();
                    
                    foreach (BattleAction action in character.battleActions)
                    {
                        if (action != null)
                        {
                            ActionData actionData = new ActionData();
                            actionData.name = action.actionName;
                            actionData.manaCost = action.manaCost;
                            actionData.targetType = action.targetType.ToString();
                            
                            if (action.effects != null && action.effects.Count > 0)
                            {
                                actionData.effectCount = action.effects.Count;
                                
                                var primaryEffect = action.effects[0];
                                actionData.primaryEffectType = primaryEffect.effectType.ToString();
                                actionData.power = primaryEffect.power;
                                
                                if (primaryEffect.statusEffect != StatusEffectType.None)
                                {
                                    actionData.statusEffect = primaryEffect.statusEffect.ToString();
                                    actionData.statusDuration = primaryEffect.statusDuration;
                                    actionData.statusPower = primaryEffect.statusPower;
                                }
                            }
                            
                            enemyData.actions.Add(actionData);
                        }
                    }
                }
                
                enemies.Add(enemyData);
            }
        }
        
        return enemies;
    }

    StatisticsData CalculateStatistics(EnemyDatabase database)
    {
        StatisticsData stats = new StatisticsData();
        
        List<EnemyData> allEnemies = new List<EnemyData>();
        allEnemies.AddRange(database.druids);
        allEnemies.AddRange(database.warriors);
        allEnemies.AddRange(database.monsters);
        
        List<EnemyData> bosses = database.bosses;
        
        if (allEnemies.Count > 0)
        {
            stats.normalEnemies = new CategoryStats();
            stats.normalEnemies.count = allEnemies.Count;
            stats.normalEnemies.avgHp = (float)allEnemies.Average(e => e.maxHp);
            stats.normalEnemies.avgMp = (float)allEnemies.Average(e => e.maxMp);
            stats.normalEnemies.avgDefense = (float)allEnemies.Average(e => e.defense);
            stats.normalEnemies.avgSpeed = allEnemies.Average(e => e.speed);
            stats.normalEnemies.minHp = allEnemies.Min(e => e.maxHp);
            stats.normalEnemies.maxHp = allEnemies.Max(e => e.maxHp);
        }
        
        if (bosses.Count > 0)
        {
            stats.bosses = new CategoryStats();
            stats.bosses.count = bosses.Count;
            stats.bosses.avgHp = (float)bosses.Average(e => e.maxHp);
            stats.bosses.avgMp = (float)bosses.Average(e => e.maxMp);
            stats.bosses.avgDefense = (float)bosses.Average(e => e.defense);
            stats.bosses.avgSpeed = bosses.Average(e => e.speed);
            stats.bosses.minHp = bosses.Min(e => e.maxHp);
            stats.bosses.maxHp = bosses.Max(e => e.maxHp);
        }
        
        return stats;
    }
}