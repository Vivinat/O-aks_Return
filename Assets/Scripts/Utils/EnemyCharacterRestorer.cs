// Assets/Scripts/Utils/EnemyCharacterRestorer.cs

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Restaura os valores originais de Characters de inimigos usando o JSON exportado
/// </summary>
public static class EnemyCharacterRestorer
{
    private const string JSON_PATH = "Assets/Data/EnemyBalanceData.json";
    private static EnemyDatabase cachedDatabase = null;
    
    /// <summary>
    /// Carrega e cacheia o database de inimigos
    /// (CORRIGIDO PARA FUNCIONAR NA BUILD)
    /// </summary>
    private static bool LoadDatabase()
    {
        if (cachedDatabase != null)
        {
            return true; // Já carregado
        }
        
        // Caminho do JSON dentro da pasta "Resources" (sem a extensão .json)
        const string RESOURCES_JSON_PATH = "EnemyBalanceData";
        
        try
        {
            // Carrega o arquivo como um TextAsset
            TextAsset jsonAsset = Resources.Load<TextAsset>(RESOURCES_JSON_PATH);
            
            if (jsonAsset == null)
            {
                Debug.LogError($"❌ JSON de inimigos não encontrado em: Assets/Resources/{RESOURCES_JSON_PATH}.json");
                Debug.LogError("Certifique-se que o arquivo está na pasta Resources!");
                return false;
            }
            
            string jsonContent = jsonAsset.text;
            cachedDatabase = JsonUtility.FromJson<EnemyDatabase>(jsonContent);
            
            if (cachedDatabase == null)
            {
                Debug.LogError("❌ Falha ao deserializar JSON de inimigos");
                return false;
            }
            
            Debug.Log($"✅ Database carregado: {GetTotalEnemiesCount()} inimigos encontrados");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erro ao carregar JSON: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Restaura TODOS os Characters de inimigos aos valores originais
    /// </summary>
    public static void RestoreAllEnemyCharacters()
    {
        Debug.Log("=== RESTAURANDO TODOS OS CHARACTERS DE INIMIGOS ===");
        
        if (!LoadDatabase())
        {
            Debug.LogError("Não foi possível carregar o database. Restauração cancelada.");
            return;
        }
        
        int restoredCount = 0;
        int errorCount = 0;
        
        // Combina todas as listas
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
        
        Debug.Log($"✅ Restauração completa: {restoredCount} inimigos restaurados, {errorCount} erros");
        
        // Força Unity a salvar as mudanças
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        #endif
    }
    
    /// <summary>
    /// Restaura um único Character de inimigo
    /// </summary>
    private static bool RestoreSingleEnemy(EnemyData data)
    {
        if (string.IsNullOrEmpty(data.assetPath))
        {
            Debug.LogWarning($"AssetPath vazio para inimigo: {data.name}");
            return false;
        }
        
        // Carrega o asset
        #if UNITY_EDITOR
        Character character = UnityEditor.AssetDatabase.LoadAssetAtPath<Character>(data.assetPath);
        #else
        // Em runtime, use Resources.Load ou outro método
        Character character = Resources.Load<Character>(GetResourcesPath(data.assetPath));
        #endif
        
        if (character == null)
        {
            Debug.LogWarning($"⚠️ Não foi possível carregar: {data.assetPath}");
            return false;
        }
        
        // Restaura valores base
        bool wasModified = false;
        
        // HP
        if (character.maxHp != data.maxHp)
        {
            Debug.Log($"  Restaurando maxHp de '{character.characterName}': {character.maxHp} → {data.maxHp}");
            character.maxHp = data.maxHp;
            wasModified = true;
        }
        
        // MP
        if (character.maxMp != data.maxMp)
        {
            Debug.Log($"  Restaurando maxMp de '{character.characterName}': {character.maxMp} → {data.maxMp}");
            character.maxMp = data.maxMp;
            wasModified = true;
        }
        
        // Defense
        if (character.defense != data.defense)
        {
            Debug.Log($"  Restaurando defense de '{character.characterName}': {character.defense} → {data.defense}");
            character.defense = data.defense;
            wasModified = true;
        }
        
        // Speed
        if (!Mathf.Approximately(character.speed, data.speed))
        {
            Debug.Log($"  Restaurando speed de '{character.characterName}': {character.speed} → {data.speed}");
            character.speed = data.speed;
            wasModified = true;
        }
        
        // Restaura BattleActions
        if (character.battleActions != null && data.actions != null)
        {
            int actionCount = Mathf.Min(character.battleActions.Count, data.actions.Count);
            
            for (int i = 0; i < actionCount; i++)
            {
                var action = character.battleActions[i];
                var actionData = data.actions[i];
                
                if (action == null) continue;
                
                // Mana Cost
                if (action.manaCost != actionData.manaCost)
                {
                    Debug.Log($"  Restaurando action[{i}].manaCost de '{character.characterName}': {action.manaCost} → {actionData.manaCost}");
                    action.manaCost = actionData.manaCost;
                    wasModified = true;
                }
                
                // Restaura efeitos da ação
                if (action.effects != null && action.effects.Count > 0)
                {
                    var effect = action.effects[0]; // Primeiro efeito (principal)
                    
                    // Power
                    if (effect.power != actionData.power)
                    {
                        Debug.Log($"  Restaurando action[{i}].power de '{character.characterName}': {effect.power} → {actionData.power}");
                        effect.power = actionData.power;
                        wasModified = true;
                    }
                    
                    // Status Power
                    if (effect.statusPower != actionData.statusPower)
                    {
                        Debug.Log($"  Restaurando action[{i}].statusPower de '{character.characterName}': {effect.statusPower} → {actionData.statusPower}");
                        effect.statusPower = actionData.statusPower;
                        wasModified = true;
                    }
                    
                    // Status Duration
                    if (effect.statusDuration != actionData.statusDuration)
                    {
                        Debug.Log($"  Restaurando action[{i}].statusDuration de '{character.characterName}': {effect.statusDuration} → {actionData.statusDuration}");
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
    
    /// <summary>
    /// Procura dados de um inimigo pelo nome
    /// </summary>
    private static EnemyData FindEnemyData(string enemyName)
    {
        if (cachedDatabase == null) return null;
        
        // Busca em todas as listas
        var allEnemies = new List<EnemyData>();
        allEnemies.AddRange(cachedDatabase.druids);
        allEnemies.AddRange(cachedDatabase.warriors);
        allEnemies.AddRange(cachedDatabase.monsters);
        allEnemies.AddRange(cachedDatabase.bosses);
        
        return allEnemies.FirstOrDefault(e => e.name == enemyName);
    }
    
    /// <summary>
    /// Converte asset path para Resources path
    /// </summary>
    private static string GetResourcesPath(string assetPath)
    {
        // Remove "Assets/Resources/" e ".asset"
        if (assetPath.Contains("Resources/"))
        {
            int index = assetPath.IndexOf("Resources/") + "Resources/".Length;
            string resourcePath = assetPath.Substring(index);
            resourcePath = resourcePath.Replace(".asset", "");
            return resourcePath;
        }
        
        return assetPath;
    }
    
    /// <summary>
    /// Retorna total de inimigos no database
    /// </summary>
    private static int GetTotalEnemiesCount()
    {
        if (cachedDatabase == null) return 0;
        
        return cachedDatabase.druids.Count +
               cachedDatabase.warriors.Count +
               cachedDatabase.monsters.Count +
               cachedDatabase.bosses.Count;
    }
    
    /// <summary>
    /// Limpa o cache (útil se o JSON foi atualizado)
    /// </summary>
    public static void ClearCache()
    {
        cachedDatabase = null;
        Debug.Log("Cache de inimigos limpo");
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// Menu de editor para testar restauração
    /// </summary>
    [UnityEditor.MenuItem("Tools/Restore All Enemy Characters")]
    public static void EditorRestoreAll()
    {
        RestoreAllEnemyCharacters();
        Debug.Log("✅ Restauração manual completa via menu");
    }
    #endif
}