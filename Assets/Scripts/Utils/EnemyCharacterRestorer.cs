using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Restaura os valores originais de Characters de inimigos usando o JSON exportado
public static class EnemyCharacterRestorer
{
    private const string JSON_PATH = "Assets/Data/EnemyBalanceData.json";
    private static EnemyDatabase cachedDatabase = null;
    
    private static bool LoadDatabase()
    {
        if (cachedDatabase != null)
        {
            return true;
        }
        
        const string RESOURCES_JSON_PATH = "EnemyBalanceData";
        
        try
        {
            TextAsset jsonAsset = Resources.Load<TextAsset>(RESOURCES_JSON_PATH);
            
            
            string jsonContent = jsonAsset.text;
            cachedDatabase = JsonUtility.FromJson<EnemyDatabase>(jsonContent);
            
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao carregar JSON: {e.Message}");
            return false;
        }
    }
    
    // Restaura os Characters de inimigos aos valores originais
    public static void RestoreAllEnemyCharacters()
    {
        int restoredCount = 0;
        int errorCount = 0;
        
        List<EnemyData> allEnemies = new List<EnemyData>();
        allEnemies.AddRange(cachedDatabase.druids);
        allEnemies.AddRange(cachedDatabase.warriors);
        allEnemies.AddRange(cachedDatabase.monsters);
        allEnemies.AddRange(cachedDatabase.bosses);
        
        foreach (var enemyData in allEnemies)
        {
            if (RestoreSingleEnemy(enemyData))
            {
                restoredCount++;
            }
            else
            {
                errorCount++;
            }
        }
        
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        #endif
    }
    
    private static bool RestoreSingleEnemy(EnemyData data)
    {
        
        #if UNITY_EDITOR
        Character character = UnityEditor.AssetDatabase.LoadAssetAtPath<Character>(data.assetPath);
        #else
        Character character = Resources.Load<Character>(GetResourcesPath(data.assetPath));
        #endif
        
        bool wasModified = false;
        
        if (character.maxHp != data.maxHp)
        {
            character.maxHp = data.maxHp;
            wasModified = true;
        }
        
        if (character.maxMp != data.maxMp)
        {
            character.maxMp = data.maxMp;
            wasModified = true;
        }
        
        if (character.defense != data.defense)
        {
            character.defense = data.defense;
            wasModified = true;
        }
        
        if (!Mathf.Approximately(character.speed, data.speed))
        {
            character.speed = data.speed;
            wasModified = true;
        }
        
        if (character.battleActions != null && data.actions != null)
        {
            int actionCount = Mathf.Min(character.battleActions.Count, data.actions.Count);
            
            for (int i = 0; i < actionCount; i++)
            {
                var action = character.battleActions[i];
                var actionData = data.actions[i];
                
                if (action == null) continue;
                
                if (action.manaCost != actionData.manaCost)
                {
                    action.manaCost = actionData.manaCost;
                    wasModified = true;
                }
                
                if (action.effects != null && action.effects.Count > 0)
                {
                    var effect = action.effects[0];
                    
                    if (effect.power != actionData.power)
                    {
                        effect.power = actionData.power;
                        wasModified = true;
                    }
                    
                    if (effect.statusPower != actionData.statusPower)
                    {
                        effect.statusPower = actionData.statusPower;
                        wasModified = true;
                    }
                    
                    if (effect.statusDuration != actionData.statusDuration)
                    {
                        effect.statusDuration = actionData.statusDuration;
                        wasModified = true;
                    }
                }
            }
        }
        
        if (wasModified)
        {
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(character);
            #endif
        }
        
        return true;
    }
    
    private static EnemyData FindEnemyData(string enemyName)
    {
        if (cachedDatabase == null) return null;
        
        var allEnemies = new List<EnemyData>();
        allEnemies.AddRange(cachedDatabase.druids);
        allEnemies.AddRange(cachedDatabase.warriors);
        allEnemies.AddRange(cachedDatabase.monsters);
        allEnemies.AddRange(cachedDatabase.bosses);
        
        return allEnemies.FirstOrDefault(e => e.name == enemyName);
    }
    
    private static string GetResourcesPath(string assetPath)
    {
        if (assetPath.Contains("Resources/"))
        {
            int index = assetPath.IndexOf("Resources/") + "Resources/".Length;
            string resourcePath = assetPath.Substring(index);
            resourcePath = resourcePath.Replace(".asset", "");
            return resourcePath;
        }
        
        return assetPath;
    }
    
    private static int GetTotalEnemiesCount()
    {
        if (cachedDatabase == null) return 0;
        
        return cachedDatabase.druids.Count +
               cachedDatabase.warriors.Count +
               cachedDatabase.monsters.Count +
               cachedDatabase.bosses.Count;
    }
    
    public static void ClearCache()
    {
        cachedDatabase = null;
    }
    
    #if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Restore All Enemy Characters")]
    public static void EditorRestoreAll()
    {
        RestoreAllEnemyCharacters();
    }
    #endif
}