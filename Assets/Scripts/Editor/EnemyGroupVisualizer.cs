using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Analisa e exporta dados dos grupos de inimigos
public class EnemyGroupVisualizer : EditorWindow
{
    private Vector2 scrollPosition;
    private EnemyGroupDatabase groupDatabase;
    private bool dataLoaded = false;
    private string outputPath = "Assets/Data/EnemyGroupsData.json";
    private bool prettyPrint = true;
    
    private bool showDruidGroups = true;
    private bool showWarriorGroups = true;
    private bool showMonsterGroups = true;
    private bool showBossGroups = true;
    
    [MenuItem("Tools/Analyze Enemy Groups")]
    public static void ShowWindow()
    {
        GetWindow<EnemyGroupVisualizer>("Enemy Groups Analyzer");
    }

    void OnGUI()
    {
        GUILayout.Label("Enemy Groups Analyzer", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Load/Refresh Enemy Groups", GUILayout.Height(40)))
        {
            LoadGroupData();
        }

        if (!dataLoaded)
        {
            EditorGUILayout.HelpBox(
                "Clique em 'Load/Refresh Enemy Groups' para carregar os dados dos grupos de inimigos.",
                MessageType.Info
            );
            return;
        }

        GUILayout.Space(10);

        // Filtros
        GUILayout.Label("Filtros:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        showDruidGroups = EditorGUILayout.ToggleLeft("Druids", showDruidGroups, GUILayout.Width(100));
        showWarriorGroups = EditorGUILayout.ToggleLeft("Paladins", showWarriorGroups, GUILayout.Width(100));
        showMonsterGroups = EditorGUILayout.ToggleLeft("Monsters", showMonsterGroups, GUILayout.Width(100));
        showBossGroups = EditorGUILayout.ToggleLeft("Bosses", showBossGroups, GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        DrawStatistics();

        GUILayout.Space(10);

        GUILayout.Label("Grupos de Inimigos:", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        if (showDruidGroups) DrawGroupCategory("Druid Groups", groupDatabase.druidGroups, new Color(0.4f, 0.8f, 0.4f));
        if (showWarriorGroups) DrawGroupCategory("Warrior Groups", groupDatabase.warriorGroups, new Color(0.8f, 0.4f, 0.4f));
        if (showMonsterGroups) DrawGroupCategory("Monster Groups", groupDatabase.monsterGroups, new Color(0.6f, 0.4f, 0.8f));
        if (showBossGroups) DrawGroupCategory("Boss Groups", groupDatabase.bossGroups, new Color(1f, 0.6f, 0f));
        
        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        // Exportar para JSON
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Export Path:", GUILayout.Width(80));
        outputPath = EditorGUILayout.TextField(outputPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.SaveFilePanel("Save Enemy Groups Data", "Assets/Data", "EnemyGroupsData", "json");
            if (!string.IsNullOrEmpty(path))
            {
                outputPath = "Assets" + path.Substring(Application.dataPath.Length);
            }
        }
        EditorGUILayout.EndHorizontal();

        prettyPrint = EditorGUILayout.Toggle("Pretty Print JSON", prettyPrint);

        if (GUILayout.Button("Export to JSON", GUILayout.Height(35)))
        {
            ExportToJSON();
        }
    }

    void LoadGroupData()
    {
        groupDatabase = new EnemyGroupDatabase();
        
        groupDatabase.druidGroups = LoadGroupsFromFolder("Assets/Data/Characters/Enemies/Druids/Groups");
        groupDatabase.warriorGroups = LoadGroupsFromFolder("Assets/Data/Characters/Enemies/Paladins/Groups");
        groupDatabase.monsterGroups = LoadGroupsFromFolder("Assets/Data/Characters/Enemies/Monsters/Groups");
        groupDatabase.bossGroups = LoadGroupsFromFolder("Assets/Data/Characters/Enemies/Bosses/Groups");
        
        groupDatabase.statistics = CalculateStatistics();
        
        dataLoaded = true;
        
        int totalGroups = groupDatabase.druidGroups.Count + groupDatabase.warriorGroups.Count + 
                         groupDatabase.monsterGroups.Count + groupDatabase.bossGroups.Count;
        
    }

    List<EnemyGroupData> LoadGroupsFromFolder(string folderPath)
    {
        List<EnemyGroupData> groups = new List<EnemyGroupData>();
        
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogWarning($"Folder not found: {folderPath}");
            return groups;
        }

        string[] guids = AssetDatabase.FindAssets("t:BattleEventSO", new[] { folderPath });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BattleEventSO battleEvent = AssetDatabase.LoadAssetAtPath<BattleEventSO>(path);
            
            if (battleEvent != null)
            {
                EnemyGroupData groupData = new EnemyGroupData();
                groupData.eventName = battleEvent.name;
                groupData.assetPath = path;
                groupData.sceneName = battleEvent.sceneToLoad;
                
                if (battleEvent.enemies != null)
                {
                    groupData.enemyCount = battleEvent.enemies.Count;
                    groupData.enemies = new List<EnemyInGroupData>();
                    
                    foreach (var enemy in battleEvent.enemies)
                    {
                        if (enemy != null)
                        {
                            EnemyInGroupData enemyData = new EnemyInGroupData();
                            enemyData.name = enemy.characterName;
                            enemyData.hp = enemy.maxHp;
                            enemyData.mp = enemy.maxMp;
                            enemyData.defense = enemy.defense;
                            enemyData.speed = enemy.speed;
                            
                            groupData.enemies.Add(enemyData);
                            
                            groupData.totalHp += enemy.maxHp;
                            groupData.totalMp += enemy.maxMp;
                        }
                    }
                    
                    if (groupData.enemyCount > 0)
                    {
                        groupData.avgHp = groupData.totalHp / groupData.enemyCount;
                        groupData.avgMp = groupData.totalMp / groupData.enemyCount;
                    }
                }
                
                groups.Add(groupData);
            }
        }
        
        return groups.OrderBy(g => g.enemyCount).ToList();
    }

    GroupStatistics CalculateStatistics()
    {
        GroupStatistics stats = new GroupStatistics();
        
        List<EnemyGroupData> allGroups = new List<EnemyGroupData>();
        allGroups.AddRange(groupDatabase.druidGroups);
        allGroups.AddRange(groupDatabase.warriorGroups);
        allGroups.AddRange(groupDatabase.monsterGroups);
        allGroups.AddRange(groupDatabase.bossGroups);
        
        if (allGroups.Count > 0)
        {
            stats.totalGroups = allGroups.Count;
            stats.avgEnemiesPerGroup = (float)allGroups.Average(g => g.enemyCount);
            stats.minEnemiesInGroup = allGroups.Min(g => g.enemyCount);
            stats.maxEnemiesInGroup = allGroups.Max(g => g.enemyCount);
            stats.avgTotalHpPerGroup = (float)allGroups.Average(g => g.totalHp);
            stats.avgTotalMpPerGroup = (float)allGroups.Average(g => g.totalMp);
            
            // Distribuição por tamanho de grupo
            stats.groupSizeDistribution = new Dictionary<int, int>();
            foreach (var group in allGroups)
            {
                if (!stats.groupSizeDistribution.ContainsKey(group.enemyCount))
                {
                    stats.groupSizeDistribution[group.enemyCount] = 0;
                }
                stats.groupSizeDistribution[group.enemyCount]++;
            }
        }
        
        return stats;
    }

    void DrawStatistics()
    {
        if (groupDatabase.statistics == null) return;
        
        var stats = groupDatabase.statistics;
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        if (stats.groupSizeDistribution != null && stats.groupSizeDistribution.Count > 0)
        {
            GUILayout.Space(5);
            GUILayout.Label("Distribuição por Tamanho:", EditorStyles.miniBoldLabel);
            foreach (var kvp in stats.groupSizeDistribution.OrderBy(x => x.Key))
            {
                EditorGUILayout.LabelField($"  {kvp.Key} inimigos: {kvp.Value} grupos");
            }
        }
        
        EditorGUILayout.EndVertical();
    }

    void DrawGroupCategory(string categoryName, List<EnemyGroupData> groups, Color categoryColor)
    {
        if (groups == null || groups.Count == 0) return;
        
        GUILayout.Space(10);
        
        var oldColor = GUI.backgroundColor;
        GUI.backgroundColor = categoryColor;
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = oldColor;
        
        GUILayout.Label($"{categoryName} ({groups.Count})", EditorStyles.boldLabel);
        
        EditorGUI.indentLevel++;
        
        foreach (var group in groups)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{group.eventName}", EditorStyles.boldLabel);
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                BattleEventSO asset = AssetDatabase.LoadAssetAtPath<BattleEventSO>(group.assetPath);
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField($"Scene: {group.sceneName}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Inimigos: {group.enemyCount} | HP Total: {group.totalHp} | MP Total: {group.totalMp}");
            
            if (group.enemies != null && group.enemies.Count > 0)
            {
                GUILayout.Space(3);
                EditorGUI.indentLevel++;
                
                foreach (var enemy in group.enemies)
                {
                    string enemyInfo = $"• {enemy.name} - HP:{enemy.hp} MP:{enemy.mp} DEF:{enemy.defense} SPD:{enemy.speed:F1}";
                    EditorGUILayout.LabelField(enemyInfo, EditorStyles.miniLabel);
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
            GUILayout.Space(3);
        }
        
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    void ExportToJSON()
    {
        if (groupDatabase == null)
        {
            EditorUtility.DisplayDialog("No Data", "Please load group data first!", "OK");
            return;
        }

        try
        {
            string json = JsonUtility.ToJson(groupDatabase, prettyPrint);
            File.WriteAllText(outputPath, json);
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog(
                "Export Successful",
                $"Enemy groups data exported to:\n{outputPath}",
                "OK"
            );
            
            Debug.Log($"Enemy groups data exported to {outputPath}");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog(
                "Export Failed",
                $"Failed to export data:\n{e.Message}",
                "OK"
            );
            Debug.LogError($"Failed to export data: {e.Message}");
        }
    }
}

[System.Serializable]
public class EnemyGroupDatabase
{
    public List<EnemyGroupData> druidGroups = new List<EnemyGroupData>();
    public List<EnemyGroupData> warriorGroups = new List<EnemyGroupData>();
    public List<EnemyGroupData> monsterGroups = new List<EnemyGroupData>();
    public List<EnemyGroupData> bossGroups = new List<EnemyGroupData>();
    public GroupStatistics statistics;
}

[System.Serializable]
public class EnemyGroupData
{
    public string eventName;
    public string assetPath;
    public string sceneName;
    public int enemyCount;
    public int totalHp;
    public int totalMp;
    public float avgHp;
    public float avgMp;
    public List<EnemyInGroupData> enemies = new List<EnemyInGroupData>();
}

[System.Serializable]
public class EnemyInGroupData
{
    public string name;
    public int hp;
    public int mp;
    public int defense;
    public float speed;
}

[System.Serializable]
public class GroupStatistics
{
    public int totalGroups;
    public float avgEnemiesPerGroup;
    public int minEnemiesInGroup;
    public int maxEnemiesInGroup;
    public float avgTotalHpPerGroup;
    public float avgTotalMpPerGroup;
    public Dictionary<int, int> groupSizeDistribution = new Dictionary<int, int>();
}