using UnityEngine;

public static class ActionEffectProcessor
{
    public static void ProcessSpecialEffect(BattleAction action, BattleEntity caster, BattleEntity target)
    {
        if (action.actionName == "Mana Elixir")
        {
            target.RestoreMana(action.effects[0].power);
            Debug.Log($"{target.characterData.characterName} restored {action.effects[0].power} MP!");
            return;
        }
        
    }
    public static bool RequiresSpecialProcessing(BattleAction action)
    {
        return action.actionName == "Mana Elixir";
    }
}