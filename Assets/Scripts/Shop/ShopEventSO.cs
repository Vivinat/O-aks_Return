using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Shop Event SO")]
public class ShopEventSO : EventTypeSO
{
    public List<BattleAction> actionsForSale; 
    public List<PowerupSO> powerupsForSale;   
    
    public int numberOfChoices = 3; 
    
    [Range(0f, 1f)]
    public float powerupChance = 0.3f;
}