// Assets/Scripts/Analytics/BattleManagerHooks.cs

using UnityEngine;

/// <summary>
/// Extensão do BattleManager para integrar com o sistema de análise comportamental
/// Este script adiciona os hooks necessários sem modificar o BattleManager original
/// </summary>
public class BattleManagerHooks : MonoBehaviour
{
    private BattleManager battleManager;
    private PlayerBehaviorAnalyzer behaviorAnalyzer;
    
    void Start()
    {
        battleManager = GetComponent<BattleManager>();
        if (battleManager == null)
        {
            Debug.LogError("BattleManagerHooks: BattleManager não encontrado!");
            return;
        }
        
        behaviorAnalyzer = PlayerBehaviorAnalyzer.Instance;
        if (behaviorAnalyzer == null)
        {
            Debug.LogWarning("BattleManagerHooks: PlayerBehaviorAnalyzer não encontrado!");
        }
    }
    
    void Update()
    {
        if (battleManager == null || behaviorAnalyzer == null) return;
        
        // Monitora mudanças no estado da batalha
        MonitorBattleState();
    }
    
    private void MonitorBattleState()
    {
        // Verifica se jogador morreu
        if (battleManager.currentState == BattleState.LOST)
        {
            behaviorAnalyzer.RecordPlayerDeath();
        }
        
        // Monitora ações em execução
        if (battleManager.currentState == BattleState.PERFORMING_ACTION)
        {
            // Este hook será chamado quando uma ação estiver sendo executada
            // O registro da ação será feito através do método modificado ExecuteAction
        }
    }
}

/// <summary>
/// Classe estática com métodos auxiliares para integração
/// Deve ser chamada nos pontos apropriados do seu código existente
/// </summary>
public static class BehaviorAnalysisIntegration
{
    /// <summary>
    /// Chama este método quando o jogador usa uma skill
    /// Adicione no método ExecuteAction do BattleManager
    /// </summary>
    public static void OnPlayerSkillUsed(BattleAction action, BattleEntity user)
    {
        if (PlayerBehaviorAnalyzer.Instance != null && 
            user != null && 
            user.characterData.team == Team.Player)
        {
            PlayerBehaviorAnalyzer.Instance.RecordPlayerSkillUsage(action, user);
        }
    }
    
    /// <summary>
    /// Chama este método quando o jogador recebe dano
    /// Adicione no método TakeDamage do BattleEntity
    /// </summary>
    public static void OnPlayerDamageReceived(BattleEntity victim, BattleEntity attacker, int damage)
    {
        if (PlayerBehaviorAnalyzer.Instance != null && 
            victim != null && victim.characterData.team == Team.Player &&
            attacker != null && attacker.characterData.team == Team.Enemy)
        {
            PlayerBehaviorAnalyzer.Instance.RecordPlayerDamageReceived(attacker, damage);
        }
    }
    
    /// <summary>
    /// Chama este método quando o jogador compra algo na loja
    /// Adicione no ShopManager quando uma compra é confirmada
    /// </summary>
    public static void OnShopPurchase(BattleAction purchasedItem)
    {
        if (PlayerBehaviorAnalyzer.Instance != null)
        {
            PlayerBehaviorAnalyzer.Instance.RecordShopPurchase(purchasedItem);
        }
    }
    
    /// <summary>
    /// Chama este método quando o jogador sai da loja
    /// Adicione no ShopManager no método ExitShop
    /// </summary>
    public static void OnShopExit(System.Collections.Generic.List<BattleAction> availableItems)
    {
        if (PlayerBehaviorAnalyzer.Instance != null)
        {
            PlayerBehaviorAnalyzer.Instance.RecordShopExit(availableItems);
        }
    }
}