using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Restaura os valores originais de BattleActions usando o JSON exportado
public static class BattleActionRestorer
{
    private const string JSON_PATH = "Assets/Data/BattleActionsBalanceData.json";
    private static BattleActionDatabase cachedDatabase = null;
    
    private static bool LoadDatabase()
    {
        if (cachedDatabase != null)
        {
            return true;
        }
        
        const string RESOURCES_JSON_PATH = "BattleActionsBalanceData";
        
        try
        {
            TextAsset jsonAsset = Resources.Load<TextAsset>(RESOURCES_JSON_PATH);
            
            if (jsonAsset == null)
            {
                return false;
            }
            
            string jsonContent = jsonAsset.text;
            cachedDatabase = JsonUtility.FromJson<BattleActionDatabase>(jsonContent);
            
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao carregar JSON: {e.Message}");
            return false;
        }
    }
    
    // Restaura as BattleActions
    public static void RestoreAllBattleActions()
    {
        
        int restoredCount = 0;
        int errorCount = 0;
        
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
        
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        #endif
    }
    
    
    public static void RestoreSingleCharacterActions(Character character)
    {
        foreach (var action in character.battleActions)
        {
            if (action == null) continue;
            
            BattleActionData originalData = FindActionData(action.actionName);
            
            if (originalData != null)
            {
                RestoreSingleAction(originalData);
            }
            else
            {
                Debug.LogWarning($"Dados originais não encontrados para: {action.actionName}");
            }
        }
    }
    
    // Restaura uma única BattleAction
    public static bool RestoreSingleAction(BattleActionData data)
    {

        #if UNITY_EDITOR
        BattleAction action = UnityEditor.AssetDatabase.LoadAssetAtPath<BattleAction>(data.assetPath);
        #else
        BattleAction action = Resources.Load<BattleAction>(GetResourcesPath(data.assetPath));
        #endif
        
        bool wasModified = false;
        
        if (action.manaCost != data.manaCost)
        {
            action.manaCost = data.manaCost;
            wasModified = true;
        }
        
        if (action.isConsumable && action.maxUses != data.maxUses)
        {
            action.maxUses = data.maxUses;
            action.currentUses = data.maxUses;
            wasModified = true;
        }
        
        if (action.shopPrice != data.shopPrice)
        {
            action.shopPrice = data.shopPrice;
            wasModified = true;
        }
        
        if (action.effects != null && data.effects != null)
        {
            int effectCount = Mathf.Min(action.effects.Count, data.effects.Count);
            
            for (int i = 0; i < effectCount; i++)
            {
                var effect = action.effects[i];
                var effectData = data.effects[i];
                
                if (effect.power != effectData.power)
                {
                    effect.power = effectData.power;
                    wasModified = true;
                }
                
                if (effect.statusPower != effectData.statusPower)
                {
                    effect.statusPower = effectData.statusPower;
                    wasModified = true;
                }
                
                if (effect.statusDuration != effectData.statusDuration)
                {
                    effect.statusDuration = effectData.statusDuration;
                    wasModified = true;
                }
                
                if (effect.hasSelfEffect && effect.selfEffectPower != effectData.selfEffectPower)
                {
                    effect.selfEffectPower = effectData.selfEffectPower;
                    wasModified = true;
                }
                
                if (effect.hasSelfEffect && effect.selfStatusPower != effectData.selfStatusPower)
                {
                    effect.selfStatusPower = effectData.selfStatusPower;
                    wasModified = true;
                }
                
                if (effect.hasSelfEffect && effect.selfStatusDuration != effectData.selfStatusDuration)
                {
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
    
    // Restaura apenas as BattleActions do jogador
    public static void RestorePlayerBattleActions()
    {
        
        int restoredCount = 0;
        
        foreach (var action in GameManager.Instance.PlayerBattleActions)
        {
            if (action == null) continue;
            
            BattleActionData originalData = FindActionData(action.actionName);
            
            if (originalData != null)
            {
                if (RestoreSingleAction(originalData))
                {
                    restoredCount++;
                }
            }
        }
    }
    
    private static BattleActionData FindActionData(string actionName)
    {
        if (cachedDatabase == null) return null;
        
        var allActions = new List<BattleActionData>();
        allActions.AddRange(cachedDatabase.paladinActions);
        allActions.AddRange(cachedDatabase.rangerActions);
        allActions.AddRange(cachedDatabase.druidActions);
        allActions.AddRange(cachedDatabase.consumableItems);
        allActions.AddRange(cachedDatabase.otherActions);
        
        return allActions.FirstOrDefault(a => a.actionName == actionName);
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
    
    private static int GetTotalActionsCount()
    {
        if (cachedDatabase == null) return 0;
        
        return cachedDatabase.paladinActions.Count +
               cachedDatabase.rangerActions.Count +
               cachedDatabase.druidActions.Count +
               cachedDatabase.consumableItems.Count +
               cachedDatabase.otherActions.Count;
    }
    
    public static void ClearCache()
    {
        cachedDatabase = null;
    }
    
    #if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Restore All BattleActions")]
    public static void EditorRestoreAll()
    {
        RestoreAllBattleActions();
    }
    
    [UnityEditor.MenuItem("Tools/Restore Player BattleActions Only")]
    public static void EditorRestorePlayer()
    {
        RestorePlayerBattleActions();
    }
    #endif
}