// Assets/Scripts/Events/BattleEventSO.cs

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Battle Event SO")]
public class BattleEventSO : EventTypeSO
{
    [Header("Battle Specific Data")]
    public List<Character> enemies; // A lista de inimigos para este encontro
    
    [Header("Battle Visual")]
    public Sprite battleBackground;
}