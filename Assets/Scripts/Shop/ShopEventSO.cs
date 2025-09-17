// Assets/Scripts/Events/ShopEventSO.cs

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Shop Event SO")]
public class ShopEventSO : EventTypeSO
{
    [Header("Shop Specific Data")]
    public List<BattleAction> actionsForSale; // Todas as ações disponíveis na loja
    
    [Header("Shop Configuration")]
    public int numberOfChoices = 3;           // Quantas ações mostrar
}