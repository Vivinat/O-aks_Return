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
    public string characterName;
    public Team team;

    public Sprite characterSprite; 

    public int maxHp;
    public int maxMp;
    public int defense;
    public float speed;
    
    public AudioClip deathSound;

    public List<BattleAction> battleActions;
}