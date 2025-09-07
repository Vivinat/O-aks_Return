// Assets/Scripts/Battle/BattleAction.cs

using UnityEngine;

// Define o que a ação faz fundamentalmente
public enum ActionType
{
    Attack,
    Heal,
    Buff,   // Aumenta status (ex: +Defesa)
    Debuff  // Diminui status (ex: -Velocidade)
}

// Define quem a ação pode ter como alvo
public enum TargetType
{
    SingleEnemy,    // Ataca um único inimigo
    SingleAlly,     // Ajuda um único aliado (incluindo a si mesmo)
    Self,           // Afeta apenas o próprio usuário
    AllEnemies,     // Ataca todos os inimigos
    AllAllies       // Ajuda todos os aliados
}

[CreateAssetMenu(fileName = "New Action", menuName = "Battle/Battle Action")]
public class BattleAction : ScriptableObject
{
    [Header("Informações Gerais")]
    public string actionName; // Nome que aparece na UI (Ex: "Bola de Fogo")
    
    [TextArea]
    public string description; // Descrição para o jogador saber o que faz
    public Sprite icon;

    [Header("Lógica da Ação")]
    public ActionType type;       // O que a ação faz (Ataca, Cura, etc.)
    public TargetType targetType; // Quem a ação pode atingir

    [Header("Valores")]
    public int power;       // O "poder" base da ação (dano, quantidade de cura, etc.)
    public int manaCost;    // Custo de MP para usar a ação
    
    //[Header("Efeitos Visuais e Sonoros (Opcional)")]
    //public GameObject hitEffectPrefab; // Prefab de partícula para quando a ação atinge o alvo
    //public AudioClip soundEffect;      // Som que a ação faz ao ser usada
}