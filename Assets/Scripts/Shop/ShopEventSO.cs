// Assets/Scripts/Events/ShopEventSO.cs (UPDATED with Powerups)

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Shop Event SO")]
public class ShopEventSO : EventTypeSO
{
    [Header("Items for Sale")]
    public List<BattleAction> actionsForSale; // Ações/skills para venda
    public List<PowerupSO> powerupsForSale;   // Powerups para venda
    
    [Header("Shop Configuration")]
    public int numberOfChoices = 3; // Quantos itens mostrar no total
    
    [Range(0f, 1f)]
    [Tooltip("Probabilidade de um slot ser um powerup vs uma BattleAction (0 = só actions, 1 = só powerups)")]
    public float powerupChance = 0.3f;
}