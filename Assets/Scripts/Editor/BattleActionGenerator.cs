// Assets/Scripts/Editor/CompleteBattleActionGenerator.cs

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BattleActionGenerator : EditorWindow
{
    [MenuItem("Tools/Generate All Battle Actions")]
    public static void ShowWindow()
    {
        GetWindow<BattleActionGenerator>("Battle Action Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Complete Battle Action Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("This will create 30 balanced battle actions:");
        GUILayout.Label("• 10 Consumable Items");
        GUILayout.Label("• 13 Mana-based Spells");
        GUILayout.Label("• 7 Unlimited Basic Actions");
        GUILayout.Space(10);
        
        if (GUILayout.Button("Generate All 30 Battle Actions", GUILayout.Height(40)))
        {
            GenerateAllBattleActions();
        }
    }

    private static void GenerateAllBattleActions()
    {
        CreateDirectories();
        
        GenerateConsumableItems();
        GenerateManaSpells();
        GenerateUnlimitedActions();
        
        AssetDatabase.Refresh();
        Debug.Log("Successfully generated all 30 Battle Actions!");
        EditorUtility.DisplayDialog("Complete!", "Successfully generated 30 Battle Actions in their respective folders!", "OK");
    }

    private static void CreateDirectories()
    {
        string basePath = "Assets/Data";
        
        if (!AssetDatabase.IsValidFolder(basePath))
            AssetDatabase.CreateFolder("Assets", "Data");
            
        basePath = "Assets/Data/BattleActions";
        if (!AssetDatabase.IsValidFolder(basePath))
            AssetDatabase.CreateFolder("Assets/Data", "BattleActions");
            
        if (!AssetDatabase.IsValidFolder($"{basePath}/Items"))
            AssetDatabase.CreateFolder(basePath, "Items");
            
        if (!AssetDatabase.IsValidFolder($"{basePath}/Mana"))
            AssetDatabase.CreateFolder(basePath, "Mana");
            
        if (!AssetDatabase.IsValidFolder($"{basePath}/Unlimited"))
            AssetDatabase.CreateFolder(basePath, "Unlimited");
    }

    private static void GenerateConsumableItems()
    {
        string path = "Assets/Data/BattleActions/Items";
        
        // Healing Potions
        CreateAction(path, "Lesser Healing Potion", "A small vial of blessed water that restores modest health", 
            TargetType.Self, 0, true, 3, 15, 
            new ActionEffect { effectType = ActionType.Heal, power = 25 });
            
        CreateAction(path, "Greater Healing Potion", "A powerful elixir blessed by high paladins", 
            TargetType.Self, 0, true, 2, 40, 
            new ActionEffect { effectType = ActionType.Heal, power = 60 });
            
        CreateAction(path, "Mana Elixir", "Crystallized arcane essence that restores magical energy", 
            TargetType.Self, 0, true, 2, 25, 
            new ActionEffect { effectType = ActionType.Heal, power = 35 }); // Special: Restores MP instead
            
        // Offensive Consumables
        CreateAction(path, "Poison Vial", "A vial of concentrated necrotoxin", 
            TargetType.SingleEnemy, 0, true, 2, 30, 
            new ActionEffect { effectType = ActionType.Attack, power = 20, statusEffect = StatusEffectType.Poison, statusDuration = 4, statusPower = 8 });
            
        CreateAction(path, "Cursed Blade", "A sacrificial dagger infused with dark magic - shatters after use", 
            TargetType.SingleEnemy, 0, true, 1, 60, 
            new ActionEffect { effectType = ActionType.Attack, power = 50, statusEffect = StatusEffectType.Cursed, statusDuration = 3, statusPower = 12 });
            
        CreateAction(path, "Holy Grenade", "Explosive divine energy that damages all foes", 
            TargetType.AllEnemies, 0, true, 1, 80, 
            new ActionEffect { effectType = ActionType.Attack, power = 30 });
            
        // Buff Consumables
        CreateAction(path, "Blessing Scroll", "Sacred parchment that grants divine protection", 
            TargetType.Self, 0, true, 2, 35, 
            new ActionEffect { effectType = ActionType.Buff, power = 0, statusEffect = StatusEffectType.Protected, statusDuration = 6, statusPower = 30 });
            
        CreateAction(path, "Berserker Brew", "Alchemical concoction that boosts strength but weakens defense", 
            TargetType.Self, 0, true, 2, 30, 
            new ActionEffect { effectType = ActionType.Buff, power = 0, statusEffect = StatusEffectType.AttackUp, statusDuration = 5, statusPower = 12,
                hasSelfEffect = true, selfEffectType = ActionType.Debuff, selfStatusEffect = StatusEffectType.DefenseDown, selfStatusDuration = 5, selfStatusPower = 8 });
                
        CreateAction(path, "Quicksilver Potion", "Liquid mercury that enhances reflexes and movement speed", 
            TargetType.Self, 0, true, 3, 25, 
            new ActionEffect { effectType = ActionType.Buff, power = 0, statusEffect = StatusEffectType.SpeedUp, statusDuration = 4, statusPower = 20 });
            
        // Debuff Consumables
        CreateAction(path, "Paralyzing Dust", "Nerve toxin powder that slows enemy reactions", 
            TargetType.SingleEnemy, 0, true, 2, 25, 
            new ActionEffect { effectType = ActionType.Debuff, power = 0, statusEffect = StatusEffectType.SpeedDown, statusDuration = 4, statusPower = 15 });
    }

    private static void GenerateManaSpells()
    {
        string path = "Assets/Data/BattleActions/Mana";
        
        // Light/Paladin Spells (Low to High Tier)
        CreateAction(path, "Holy Strike", "Channel divine energy into a powerful attack", 
            TargetType.SingleEnemy, 6, false, 0, 0, 
            new ActionEffect { effectType = ActionType.Attack, power = 32 });
            
        CreateAction(path, "Divine Healing", "Restore health through sacred magic", 
            TargetType.Self, 10, false, 0, 0, 
            new ActionEffect { effectType = ActionType.Heal, power = 45 });
            
        CreateAction(path, "Sacred Shield", "Envelop yourself in protective holy light", 
            TargetType.Self, 8, false, 0, 0, 
            new ActionEffect { effectType = ActionType.Buff, power = 0, statusEffect = StatusEffectType.DefenseUp, statusDuration = 5, statusPower = 10 });
            
        CreateAction(path, "Purifying Light", "Unleash radiant energy against all darkness", 
            TargetType.AllEnemies, 18, false, 0, 0, 
            new ActionEffect { effectType = ActionType.Attack, power = 28 });
            
        CreateAction(path, "Righteous Wrath", "Divine fury increases attack but leaves you exposed", 
            TargetType.Self, 12, false, 0, 0, 
            new ActionEffect { effectType = ActionType.Buff, power = 0, statusEffect = StatusEffectType.AttackUp, statusDuration = 4, statusPower = 15,
                hasSelfEffect = true, selfEffectType = ActionType.Debuff, selfStatusEffect = StatusEffectType.DefenseDown, selfStatusDuration = 4, selfStatusPower = 8 });
                
        CreateAction(path, "Guardian's Blessing", "Heal yourself while gaining regenerative powers", 
            TargetType.Self, 15, false, 0, 0, 
            new ActionEffect { effectType = ActionType.Heal, power = 30, statusEffect = StatusEffectType.Blessed, statusDuration = 5, statusPower = 10 });
            
        // Dark/Necromancer Spells
        CreateAction(path, "Shadow Bolt", "Hurl concentrated darkness at your enemy", 
            TargetType.SingleEnemy, 5, false, 0, 0, 
            new ActionEffect { effectType = ActionType.Attack, power = 30 });
            
        CreateAction(path, "Vampiric Drain", "Steal life force from your target", 
            TargetType.SingleEnemy, 12, false, 0, 0, 
            new ActionEffect { effectType = ActionType.Attack, power = 25, hasSelfEffect = true, selfEffectType = ActionType.Heal, selfEffectPower = 20 });
            
        CreateAction(path, "Curse of Frailty", "Weaken enemy defenses with necromantic magic", 
            TargetType.SingleEnemy, 7, false, 0, 0, 
            new ActionEffect { effectType = ActionType.Debuff, power = 0, statusEffect = StatusEffectType.DefenseDown, statusDuration = 6, statusPower = 10 });
            
        CreateAction(path, "Soul Siphon", "Drain enemy vitality over time while weakening them", 
            TargetType.SingleEnemy, 10, false, 0, 0, 
            new ActionEffect { effectType = ActionType.Attack, power = 15, statusEffect = StatusEffectType.Cursed, statusDuration = 4, statusPower = 12 });
            
        CreateAction(path, "Dark Sacrifice", "Unleash devastating power at the cost of your own life force", 
            TargetType.SingleEnemy, 20, false, 0, 0, 
            new ActionEffect { effectType = ActionType.Attack, power = 65, hasSelfEffect = true, selfEffectType = ActionType.Attack, selfEffectPower = 18 });
            
        CreateAction(path, "Death Coil", "Damage enemy or heal yourself with the same dark energy", 
            TargetType.SingleEnemy, 14, false, 0, 0, 
            new ActionEffect { effectType = ActionType.Attack, power = 40, hasSelfEffect = true, selfEffectType = ActionType.Heal, selfEffectPower = 25 });
            
        // Neutral/Arcane Spells
        CreateAction(path, "Arcane Missiles", "Fire multiple bolts of pure magical energy", 
            TargetType.SingleEnemy, 16, false, 0, 0, 
            new ActionEffect { effectType = ActionType.Attack, power = 48 });
    }

    private static void GenerateUnlimitedActions()
    {
        string path = "Assets/Data/BattleActions/Unlimited";
        
        CreateAction(path, "Strike", "A basic physical attack with your weapon", 
            TargetType.SingleEnemy, 0, false, 0, 0, 
            new ActionEffect { effectType = ActionType.Attack, power = 22 });
            
        CreateAction(path, "Power Strike", "A devastating blow that leaves you momentarily vulnerable", 
            TargetType.SingleEnemy, 0, false, 0, 0, 
            new ActionEffect { effectType = ActionType.Attack, power = 38, hasSelfEffect = true, selfEffectType = ActionType.Debuff, 
                selfStatusEffect = StatusEffectType.DefenseDown, selfStatusDuration = 2, selfStatusPower = 5 });
                
        CreateAction(path, "Defensive Stance", "Focus on protection at the cost of mobility", 
            TargetType.Self, 0, false, 0, 0, 
            new ActionEffect { effectType = ActionType.Buff, power = 0, statusEffect = StatusEffectType.DefenseUp, statusDuration = 4, statusPower = 12,
                hasSelfEffect = true, selfEffectType = ActionType.Debuff, selfStatusEffect = StatusEffectType.SpeedDown, selfStatusDuration = 4, selfStatusPower = 12 });
                
        CreateAction(path, "Battle Focus", "Concentrate your energy to enhance attack power", 
            TargetType.Self, 0, false, 0, 0, 
            new ActionEffect { effectType = ActionType.Buff, power = 0, statusEffect = StatusEffectType.AttackUp, statusDuration = 4, statusPower = 8 });
            
        CreateAction(path, "Intimidating Shout", "Reduce enemy attack power through fear", 
            TargetType.SingleEnemy, 0, false, 0, 0, 
            new ActionEffect { effectType = ActionType.Debuff, power = 0, statusEffect = StatusEffectType.AttackDown, statusDuration = 4, statusPower = 6 });
            
        CreateAction(path, "Swift Strike", "Quick attack that boosts your speed for follow-up actions", 
            TargetType.SingleEnemy, 0, false, 0, 0, 
            new ActionEffect { effectType = ActionType.Attack, power = 18, hasSelfEffect = true, selfEffectType = ActionType.Buff, 
                selfStatusEffect = StatusEffectType.SpeedUp, selfStatusDuration = 3, selfStatusPower = 12 });
                
        CreateAction(path, "Reckless Charge", "All-out attack that damages both you and your enemy", 
            TargetType.SingleEnemy, 0, false, 0, 0, 
            new ActionEffect { effectType = ActionType.Attack, power = 35, hasSelfEffect = true, selfEffectType = ActionType.Attack, selfEffectPower = 10 });
    }

    private static void CreateAction(string folderPath, string name, string description, TargetType targetType, 
        int manaCost, bool isConsumable, int maxUses, int shopPrice, ActionEffect effect)
    {
        BattleAction action = ScriptableObject.CreateInstance<BattleAction>();
        
        action.actionName = name;
        action.description = description;
        action.targetType = targetType;
        action.manaCost = manaCost;
        action.isConsumable = isConsumable;
        action.maxUses = maxUses;
        action.shopPrice = shopPrice;
        
        action.effects = new List<ActionEffect> { effect };
        
        string fileName = name.Replace(" ", "").Replace("'", "");
        string assetPath = $"{folderPath}/{fileName}.asset";
        
        AssetDatabase.CreateAsset(action, assetPath);
        EditorUtility.SetDirty(action);
        
        Debug.Log($"Created: {name} at {assetPath}");
    }
}