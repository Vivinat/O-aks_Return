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

    [Header("Visual")]
    public Sprite characterSprite; 

    [Header("Stats")]
    public int maxHp;
    public int maxMp;
    public int defense;
    public float speed;
    
    [Header("Audio")]
    [Tooltip("Som que toca quando este personagem morre")]
    public AudioClip deathSound;

    [Header("Battle")]
    public List<BattleAction> battleActions;
}