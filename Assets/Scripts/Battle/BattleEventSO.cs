using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Battle Event SO")]
public class BattleEventSO : EventTypeSO
{
    [Header("Battle Specific Data")]
    public List<Character> enemies; 
    
    [Header("Battle Visual")]
    public Sprite battleBackground;
}