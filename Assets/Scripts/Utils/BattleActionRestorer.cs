// Assets/Scripts/Utils/BattleActionRestorer.cs

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Restaura os valores originais de BattleActions usando o JSON exportado
/// </summary>
public static class BattleActionRestorer
{
    private const string JSON_PATH = "Assets/Data/BattleActionsBalanceData.json";
    private static BattleActionDatabase cachedDatabase = null;
    
    /// <summary>
    /// Carrega e cacheia o database de BattleActions
    /// </summary>
    private static bool LoadDatabase()
    {
        if (cachedDatabase != null)
        {
            return true; // Já carregado
        }
        
        // Tenta carregar do arquivo JSON
        if (!File.Exists(JSON_PATH))
        {
            Debug.LogError($"❌ JSON de BattleActions não encontrado em: {JSON_PATH}");
            Debug.LogError("Execute Tools > Export BattleActions to JSON no Editor primeiro!");
            return false;
        }
        
        try
        {
            string jsonContent = File.ReadAllText(JSON_PATH);
            cachedDatabase = JsonUtility.FromJson<BattleActionDatabase>(jsonContent);
            
            if (cachedDatabase == null)
            {
                Debug.LogError("❌ Falha ao deserializar JSON de BattleActions");
                return false;
            }
            
            Debug.Log($"✅ Database carregado: {GetTotalActionsCount()} ações encontradas");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erro ao carregar JSON: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Restaura TODAS as BattleActions aos valores originais
    /// </summary>
    public static void RestoreAllBattleActions()
    {
        Debug.Log("=== RESTAURANDO TODAS AS BATTLEACTIONS ===");
        
        if (!LoadDatabase())
        {
            Debug.LogError("Não foi possível carregar o database. Restauração cancelada.");
            return;
        }
        
        int restoredCount = 0;
        int errorCount = 0;
        
        // Combina todas as listas
        List<BattleActionData> allActions = new List<BattleActionData>();
        allActions.AddRange(cachedDatabase.paladinActions);
        allActions.AddRange(cachedDatabase.rangerActions);
        allActions.AddRange(cachedDatabase.druidActions);
        allActions.AddRange(cachedDatabase.consumableItems);
        allActions.AddRange(cachedDatabase.otherActions);
        
        foreach (var actionData in allActions)
        {
            if (RestoreSingleAction(actionData))
            {
                restoredCount++;
            }
            else
            {
                errorCount++;
            }
        }
        
        Debug.Log($"✅ Restauração completa: {restoredCount} ações restauradas, {errorCount} erros");
        
        // Força Unity a salvar as mudanças
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        #endif
    }
    
    /// <summary>
    /// NOVO: Restaura as BattleActions de um Character específico
    /// (Usado pelo BattleManager para resetar o inimigo antes do turno)
    /// </summary>
    public static void RestoreSingleCharacterActions(Character character)
    {
        if (character == null || character.battleActions == null)
        {
            Debug.LogWarning("Character ou suas BattleActions são nulos.");
            return;
        }

        // Garante que o JSON com os valores base esteja carregado
        if (!LoadDatabase()) //
        {
            Debug.LogError("Não foi possível carregar o database. Restauração do inimigo cancelada.");
            return;
        }
        
        // Debug.Log($"Restaurando ações para: {character.characterName}");

        foreach (var action in character.battleActions)
        {
            if (action == null) continue;
            
            // Encontra os dados originais no JSON pelo nome
            BattleActionData originalData = FindActionData(action.actionName); //
            
            if (originalData != null)
            {
                // Restaura o SO para os valores base
                RestoreSingleAction(originalData); //
            }
            else
            {
                Debug.LogWarning($"⚠️ Dados originais não encontrados para: {action.actionName}");
            }
        }
    }
    
    /// <summary>
    /// Restaura uma única BattleAction
    /// </summary>
    public static bool RestoreSingleAction(BattleActionData data)
    {
        if (string.IsNullOrEmpty(data.assetPath))
        {
            Debug.LogWarning($"AssetPath vazio para ação: {data.actionName}");
            return false;
        }
        
        // Carrega o asset
        #if UNITY_EDITOR
        BattleAction action = UnityEditor.AssetDatabase.LoadAssetAtPath<BattleAction>(data.assetPath);
        #else
        // Em runtime, use Resources.Load ou outro método
        BattleAction action = Resources.Load<BattleAction>(GetResourcesPath(data.assetPath));
        #endif
        
        if (action == null)
        {
            Debug.LogWarning($"⚠️ Não foi possível carregar: {data.assetPath}");
            return false;
        }
        
        // Restaura valores numéricos
        bool wasModified = false;
        
        // Custo de mana
        if (action.manaCost != data.manaCost)
        {
            Debug.Log($"  Restaurando manaCost de '{action.actionName}': {action.manaCost} → {data.manaCost}");
            action.manaCost = data.manaCost;
            wasModified = true;
        }
        
        // Usos máximos (para consumíveis)
        if (action.isConsumable && action.maxUses != data.maxUses)
        {
            Debug.Log($"  Restaurando maxUses de '{action.actionName}': {action.maxUses} → {data.maxUses}");
            action.maxUses = data.maxUses;
            action.currentUses = data.maxUses; // Reseta usos atuais também
            wasModified = true;
        }
        
        // Preço de loja
        if (action.shopPrice != data.shopPrice)
        {
            Debug.Log($"  Restaurando shopPrice de '{action.actionName}': {action.shopPrice} → {data.shopPrice}");
            action.shopPrice = data.shopPrice;
            wasModified = true;
        }
        
        // Restaura efeitos
        if (action.effects != null && data.effects != null)
        {
            int effectCount = Mathf.Min(action.effects.Count, data.effects.Count);
            
            for (int i = 0; i < effectCount; i++)
            {
                var effect = action.effects[i];
                var effectData = data.effects[i];
                
                // Power
                if (effect.power != effectData.power)
                {
                    Debug.Log($"  Restaurando effect[{i}].power de '{action.actionName}': {effect.power} → {effectData.power}");
                    effect.power = effectData.power;
                    wasModified = true;
                }
                
                // Status power
                if (effect.statusPower != effectData.statusPower)
                {
                    Debug.Log($"  Restaurando effect[{i}].statusPower de '{action.actionName}': {effect.statusPower} → {effectData.statusPower}");
                    effect.statusPower = effectData.statusPower;
                    wasModified = true;
                }
                
                // Status duration
                if (effect.statusDuration != effectData.statusDuration)
                {
                    Debug.Log($"  Restaurando effect[{i}].statusDuration de '{action.actionName}': {effect.statusDuration} → {effectData.statusDuration}");
                    effect.statusDuration = effectData.statusDuration;
                    wasModified = true;
                }
                
                // Self effect power
                if (effect.hasSelfEffect && effect.selfEffectPower != effectData.selfEffectPower)
                {
                    Debug.Log($"  Restaurando effect[{i}].selfEffectPower de '{action.actionName}': {effect.selfEffectPower} → {effectData.selfEffectPower}");
                    effect.selfEffectPower = effectData.selfEffectPower;
                    wasModified = true;
                }
                
                // Self status power
                if (effect.hasSelfEffect && effect.selfStatusPower != effectData.selfStatusPower)
                {
                    Debug.Log($"  Restaurando effect[{i}].selfStatusPower de '{action.actionName}': {effect.selfStatusPower} → {effectData.selfStatusPower}");
                    effect.selfStatusPower = effectData.selfStatusPower;
                    wasModified = true;
                }
                
                // Self status duration
                if (effect.hasSelfEffect && effect.selfStatusDuration != effectData.selfStatusDuration)
                {
                    Debug.Log($"  Restaurando effect[{i}].selfStatusDuration de '{action.actionName}': {effect.selfStatusDuration} → {effectData.selfStatusDuration}");
                    effect.selfStatusDuration = effectData.selfStatusDuration;
                    wasModified = true;
                }
            }
        }
        
        if (wasModified)
        {
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(action);
            #endif
        }
        
        return true;
    }
    
    /// <summary>
    /// Restaura apenas as BattleActions do jogador
    /// </summary>
    public static void RestorePlayerBattleActions()
    {
        if (GameManager.Instance == null || GameManager.Instance.PlayerBattleActions == null)
        {
            Debug.LogWarning("GameManager ou PlayerBattleActions não encontrado");
            return;
        }
        
        Debug.Log("=== RESTAURANDO BATTLEACTIONS DO JOGADOR ===");
        
        if (!LoadDatabase())
        {
            Debug.LogError("Não foi possível carregar o database. Restauração cancelada.");
            return;
        }
        
        int restoredCount = 0;
        
        foreach (var action in GameManager.Instance.PlayerBattleActions)
        {
            if (action == null) continue;
            
            // Encontra os dados originais
            BattleActionData originalData = FindActionData(action.actionName);
            
            if (originalData != null)
            {
                if (RestoreSingleAction(originalData))
                {
                    restoredCount++;
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Dados originais não encontrados para: {action.actionName}");
            }
        }
        
        Debug.Log($"✅ {restoredCount} ações do jogador restauradas");
    }
    
    /// <summary>
    /// Procura dados de uma ação pelo nome
    /// </summary>
    private static BattleActionData FindActionData(string actionName)
    {
        if (cachedDatabase == null) return null;
        
        // Busca em todas as listas
        var allActions = new List<BattleActionData>();
        allActions.AddRange(cachedDatabase.paladinActions);
        allActions.AddRange(cachedDatabase.rangerActions);
        allActions.AddRange(cachedDatabase.druidActions);
        allActions.AddRange(cachedDatabase.consumableItems);
        allActions.AddRange(cachedDatabase.otherActions);
        
        return allActions.FirstOrDefault(a => a.actionName == actionName);
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
    /// Retorna total de ações no database
    /// </summary>
    private static int GetTotalActionsCount()
    {
        if (cachedDatabase == null) return 0;
        
        return cachedDatabase.paladinActions.Count +
               cachedDatabase.rangerActions.Count +
               cachedDatabase.druidActions.Count +
               cachedDatabase.consumableItems.Count +
               cachedDatabase.otherActions.Count;
    }
    
    /// <summary>
    /// Limpa o cache (útil se o JSON foi atualizado)
    /// </summary>
    public static void ClearCache()
    {
        cachedDatabase = null;
        Debug.Log("Cache de BattleActions limpo");
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// Menu de editor para testar restauração
    /// </summary>
    [UnityEditor.MenuItem("Tools/Restore All BattleActions")]
    public static void EditorRestoreAll()
    {
        RestoreAllBattleActions();
        Debug.Log("✅ Restauração manual completa via menu");
    }
    
    [UnityEditor.MenuItem("Tools/Restore Player BattleActions Only")]
    public static void EditorRestorePlayer()
    {
        RestorePlayerBattleActions();
        Debug.Log("✅ Restauração do jogador completa via menu");
    }
    #endif
}