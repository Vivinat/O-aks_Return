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
        
    }
    
    /// <summary>
    /// Checks if an action requires special processing
    /// </summary>
    public static bool RequiresSpecialProcessing(BattleAction action)
    {
        return action.actionName == "Mana Elixir";
    }
}