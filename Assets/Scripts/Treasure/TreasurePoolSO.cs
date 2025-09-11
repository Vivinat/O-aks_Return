// Assets/Scripts/Data/BattleActionRewardPoolSO.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewRewardPool", menuName = "Events/Battle Action Reward Pool")]
public class TreasurePoolSO : ScriptableObject
{
    public List<BattleAction> possibleRewards;

    // Método auxiliar para sortear ações únicas
    public List<BattleAction> GetRandomRewards(int count)
    {
        List<BattleAction> allRewards = new List<BattleAction>(possibleRewards);
        List<BattleAction> selectedRewards = new List<BattleAction>();

        for (int i = 0; i < count && allRewards.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, allRewards.Count);
            selectedRewards.Add(allRewards[randomIndex]);
            //allRewards.RemoveAt(randomIndex); // Evita repetição na mesma seleção
        }
        return selectedRewards;
    }
}