using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/BattleAction Select Event SO")]
public class TreasureEventSO : EventTypeSO
{
    [Header("Trasure Specific Data")] public TreasurePoolSO poolForTheMap;
}
