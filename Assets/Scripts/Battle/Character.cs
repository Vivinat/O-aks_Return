// Assets/Scripts/Entities/Character.cs

using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    Player,
    Enemy
}

[CreateAssetMenu(fileName = "New Character", menuName = "Entities/Character")]
public class Character : ScriptableObject
{
    [Header("Info")]
    public string characterName;
    public Team team;

    [Header("Stats")]
    public int maxHp;
    public int maxMp;
    public int defense;
    public float speed; // Influenciará a velocidade de preenchimento do ATB

    [Header("Battle")]
    public List<BattleAction> battleActions;
    // public AI_Behaviour aiBehaviour; // Opcional para IA
}