// Assets/Scripts/Battle/ActionEffectProcessor.cs
// Helper class to handle special action effects like mana restoration

using UnityEngine;

public static class ActionEffectProcessor
{
    /// <summary>
    /// Processes special action effects that don't fit the standard categories
    /// </summary>
    public static void ProcessSpecialEffect(BattleAction action, BattleEntity caster, BattleEntity target)
    {
        // Handle Mana Elixir special case
        if (action.actionName == "Mana Elixir")
        {
            target.RestoreMana(action.effects[0].power);
            Debug.Log($"{target.characterData.characterName} restored {action.effects[0].power} MP!");
            return;
        }
        
        // Handle other special consumables here if needed
        // For example, items that affect both HP and MP, or have unique mechanics
    }
    
    /// <summary>
    /// Checks if an action requires special processing
    /// </summary>
    public static bool RequiresSpecialProcessing(BattleAction action)
    {
        return action.actionName == "Mana Elixir";
        // Add other special items here as needed
    }
}