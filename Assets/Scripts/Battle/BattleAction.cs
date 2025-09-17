// Assets/Scripts/Battle/BattleAction.cs (Atualizado)

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
    
    [Header("Consumível (Opcional)")]
    public bool isConsumable = false;  // Se true, tem usos limitados
    public int maxUses = 1;           // Quantidade máxima de usos (só se isConsumable = true)
    public int shopPrice = 10;        // Preço na loja
    
    [System.NonSerialized]
    public int currentUses;           // Usos restantes (não serializado, gerenciado em runtime)
    
    void OnEnable()
    {
        // Quando a ação é criada/carregada, define os usos atuais como máximo se ainda não foi definido
        if (isConsumable && currentUses <= 0)
        {
            currentUses = maxUses;
        }
    }
    
    void Awake()
    {
        // Garante que consumíveis sempre iniciem com usos completos
        if (isConsumable && currentUses <= 0)
        {
            currentUses = maxUses;
        }
    }
    
    /// <summary>
    /// Usa a ação, diminuindo os usos se for consumível
    /// </summary>
    /// <returns>True se a ação ainda pode ser usada, False se esgotou</returns>
    public bool UseAction()
    {
        if (isConsumable)
        {
            if (currentUses > 0)
            {
                currentUses--;
                Debug.Log($"{actionName} usado. Usos restantes: {currentUses}");
                return currentUses > 0;
            }
            
            Debug.Log($"{actionName} não tem mais usos!");
            return false;
        }
        
        return true; // Ações não consumíveis sempre podem ser usadas (se tiver MP)
    }
    
    /// <summary>
    /// Verifica se a ação ainda pode ser usada
    /// </summary>
    public bool CanUse()
    {
        if (isConsumable)
        {
            return currentUses > 0;
        }
        return true; // Ações não consumíveis sempre podem ser usadas
    }
    
    /// <summary>
    /// Redefine os usos para o máximo (útil quando a ação é comprada/obtida)
    /// </summary>
    public void RefillUses()
    {
        if (isConsumable)
        {
            currentUses = maxUses;
        }
    }
    
    /// <summary>
    /// Cria uma cópia da ação para usar no inventário
    /// </summary>
    public BattleAction CreateInstance()
    {
        BattleAction instance = Instantiate(this);
        
        // Garante que a instância tenha usos completos se for consumível
        if (instance.isConsumable)
        {
            instance.currentUses = instance.maxUses;
        }
        
        return instance;
    }
}