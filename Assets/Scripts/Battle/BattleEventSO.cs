using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Battle Event SO")]
public class BattleEventSO : EventTypeSO
{
    public List<Character> enemies;
    public Sprite battleBackground;
}