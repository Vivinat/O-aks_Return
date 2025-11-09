using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "NewRewardPool", menuName = "Events/Battle Action Reward Pool")]
public class TreasurePoolSO : ScriptableObject
{
    public List<BattleAction> possibleRewards;
    
    public List<BattleAction> GetRandomRewards(int count, List<BattleAction> excludeActions = null)
    {
        // Cria pool de ações disponíveis
        List<BattleAction> availableRewards = new List<BattleAction>(possibleRewards);
        
        if (excludeActions != null && excludeActions.Count > 0)
        {
            availableRewards = availableRewards
                .Where(action => !excludeActions.Any(excluded => 
                    excluded != null && action != null && excluded.actionName == action.actionName))
                .ToList();
        }

        List<BattleAction> selectedRewards = new List<BattleAction>();

        for (int i = 0; i < count && availableRewards.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableRewards.Count);
            selectedRewards.Add(availableRewards[randomIndex]);
            availableRewards.RemoveAt(randomIndex); // Remove para evitar duplicatas
        }
        
        return selectedRewards;
    }
    
    public BattleAction GetSingleRandomReward(List<BattleAction> excludeActions = null)
    {
        var rewards = GetRandomRewards(1, excludeActions);
        return rewards.Count > 0 ? rewards[0] : null;
    }
    
    public bool HasEnoughRewards(int requiredCount, List<BattleAction> excludeActions = null)
    {
        List<BattleAction> availableRewards = new List<BattleAction>(possibleRewards);
        
        if (excludeActions != null && excludeActions.Count > 0)
        {
            availableRewards = availableRewards
                .Where(action => !excludeActions.Any(excluded => 
                    excluded != null && action != null && excluded.actionName == action.actionName))
                .ToList();
        }
        
        return availableRewards.Count >= requiredCount;
    }
}