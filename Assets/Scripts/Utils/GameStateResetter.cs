// Assets/Scripts/Utils/GameStateResetter.cs

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Utilitário para resetar completamente o estado do jogo ao morrer
/// </summary>
public static class GameStateResetter
{
    /// <summary>
    /// Reseta COMPLETAMENTE o estado do jogo para começar do zero
    /// Chamado quando o jogador morre e volta ao menu principal
    /// </summary>
    public static void ResetGameState()
    {
        Debug.Log("=== INICIANDO RESET COMPLETO DO ESTADO DO JOGO ===");
        
        // 0. NOVO: Restaura Characters de inimigos aos valores originais PRIMEIRO
        RestoreEnemyCharacters();
        
        // 0. NOVO: Restaura BattleActions aos valores originais PRIMEIRO
        RestoreBattleActions();
        
        // 1. Reseta PlayerBehaviorAnalyzer (mantém só observações de morte)
        ResetPlayerBehaviorAnalyzer();
        
        // 2. Reseta DifficultyModifiers
        ResetDifficultyModifiers();
        
        // 3. Reseta stats do jogador e inventário
        ResetPlayerCharacter();
        
        // 4. Limpa pools de ofertas do gerador dinâmico
        ClearDynamicOfferPools();
        
        // 5. Limpa dados de mapas salvos
        ClearMapData();
        
        // 6. Reseta moedas
        ResetCurrency();
        
        Debug.Log("✅ Reset completo do jogo finalizado!");
    }
    
    /// <summary>
    /// NOVO: Restaura BattleActions aos valores originais usando JSON
    /// </summary>
    private static void RestoreBattleActions()
    {
        Debug.Log("Restaurando BattleActions aos valores originais...");
        
        try
        {
            // Restaura TODAS as BattleActions do jogo
            BattleActionRestorer.RestoreAllBattleActions();
            
            Debug.Log("✅ BattleActions restauradas com sucesso");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erro ao restaurar BattleActions: {e.Message}");
            Debug.LogError("As ações podem estar em estado inconsistente!");
        }
    }
    
    /// <summary>
    /// Reseta observações, mantendo apenas as de morte do jogador
    /// </summary>
    private static void ResetPlayerBehaviorAnalyzer()
    {
        if (PlayerBehaviorAnalyzer.Instance == null)
        {
            Debug.LogWarning("PlayerBehaviorAnalyzer.Instance não encontrado");
            return;
        }
        
        Debug.Log("Resetando PlayerBehaviorAnalyzer...");
        
        // Pega observações atuais
        var allObservations = PlayerBehaviorAnalyzer.Instance.GetAllObservations();
        
        // Filtra apenas observações de morte
        List<BehaviorObservation> deathObservations = allObservations.FindAll(obs => 
            obs.triggerType == BehaviorTriggerType.PlayerDeath ||
            obs.triggerType == BehaviorTriggerType.RepeatedBossDeath
        );
        
        Debug.Log($"Preservando {deathObservations.Count} observações de morte");
        
        // Limpa TUDO
        PlayerBehaviorAnalyzer.Instance.ClearAllData();
        
        // Re-adiciona apenas as observações de morte
        foreach (var deathObs in deathObservations)
        {
            PlayerBehaviorAnalyzer.Instance.AddObservationDirectly(deathObs);
            Debug.Log($"  Preservada: {deathObs.triggerType} - {deathObs.GetData<string>("killerEnemy", "Unknown")}");
        }
    }
    
    /// <summary>
    /// Reseta modificadores de dificuldade
    /// </summary>
    private static void ResetDifficultyModifiers()
    {
        if (DifficultySystem.Instance == null)
        {
            Debug.LogWarning("DifficultySystem.Instance não encontrado");
            return;
        }
        
        Debug.Log("Resetando DifficultySystem...");
        DifficultySystem.Instance.ResetModifiers();
    }
    
    /// <summary>
    /// Reseta stats do jogador para valores base e inventário para skill inicial
    /// </summary>
    private static void ResetPlayerCharacter()
    {
        if (GameManager.Instance == null || GameManager.Instance.PlayerCharacterInfo == null)
        {
            Debug.LogWarning("GameManager ou PlayerCharacterInfo não encontrado");
            return;
        }
        
        Debug.Log("Resetando personagem do jogador...");
        
        Character player = GameManager.Instance.PlayerCharacterInfo;
        
        // Valores base
        player.maxHp = 100;
        player.maxMp = 100;
        player.defense = 15;
        player.speed = 2.5f;
        
        // Reseta HP/MP atuais
        GameManager.Instance.SetPlayerCurrentHP(100);
        GameManager.Instance.SetPlayerCurrentMP(100);
        
        // NOVO: Pega a skill inicial configurada no GameManager
        BattleAction ataqueSombrio = GameManager.Instance.GetInitialPlayerSkill();
        
        if (ataqueSombrio != null)
        {
            player.battleActions = new List<BattleAction> { ataqueSombrio };
            GameManager.Instance.PlayerBattleActions = new List<BattleAction> { ataqueSombrio };
            
            Debug.Log($"✅ Inventário resetado para: {ataqueSombrio.actionName}");
        }
        else
        {
            Debug.LogError("CRÍTICO: initialPlayerSkill não está configurado no GameManager!");
            Debug.LogError("Configure o campo 'Initial Player Skill' no Inspector do GameManager!");
            
            player.battleActions = new List<BattleAction>();
            GameManager.Instance.PlayerBattleActions = new List<BattleAction>();
        }
        
        Debug.Log($"Stats resetados: HP={player.maxHp}, MP={player.maxMp}, DEF={player.defense}, SPD={player.speed}");
    }
    
    /// <summary>
    /// Limpa pools de ofertas dinâmicas
    /// </summary>
    private static void ClearDynamicOfferPools()
    {
        if (DynamicNegotiationCardGenerator.Instance == null)
        {
            Debug.LogWarning("DynamicNegotiationCardGenerator.Instance não encontrado");
            return;
        }
        
        Debug.Log("Limpando pools de ofertas dinâmicas...");
        
        // As pools são limpas automaticamente no próximo ProcessObservations()
        // Mas vamos forçar um reprocessamento com estado limpo
        DynamicNegotiationCardGenerator.Instance.ProcessObservations();
    }
    
    /// <summary>
    /// Limpa dados salvos de mapas
    /// </summary>
    private static void ClearMapData()
    {
        Debug.Log("Limpando dados de mapas...");
        
        // Limpa PlayerPrefs relacionados a mapas
        PlayerPrefs.DeleteKey("LastCompletedNode");
        PlayerPrefs.DeleteKey("CompletedBossNode");
        PlayerPrefs.DeleteKey("NextSceneAfterBoss");
        
        // NOVO: Limpa TODOS os estados de mapas salvos no GameManager
        if (GameManager.Instance != null)
        {
            // Limpa Map1
            GameManager.Instance.ClearMapData("Map1");
            
            // Limpa Map2
            GameManager.Instance.ClearMapData("Map2");
            
            // Limpa Map3
            GameManager.Instance.ClearMapData("Map3");
            
            // Se tiver mais mapas, adicione aqui
            Debug.Log("✅ Estados de todos os mapas foram limpos do GameManager");
        }
        
        Debug.Log("✅ Dados de mapas limpos");
    }
    
    /// <summary>
    /// Reseta moedas para 0
    /// </summary>
    private static void ResetCurrency()
    {
        if (GameManager.Instance?.CurrencySystem == null)
        {
            Debug.LogWarning("CurrencySystem não encontrado");
            return;
        }
        
        Debug.Log("Resetando moedas...");
        
        // Reseta para 0 moedas
        int currentCoins = GameManager.Instance.CurrencySystem.CurrentCoins;
        if (currentCoins > 0)
        {
            GameManager.Instance.CurrencySystem.SpendCoins(currentCoins);
        }
        
        Debug.Log("Moedas resetadas para 0");
    }
    
    /// <summary>
    /// NOVO: Restaura Characters de inimigos aos valores originais usando JSON
    /// </summary>
    private static void RestoreEnemyCharacters()
    {
        Debug.Log("Restaurando Characters de inimigos aos valores originais...");
    
        try
        {
            EnemyCharacterRestorer.RestoreAllEnemyCharacters();
            Debug.Log("Characters de inimigos restaurados com sucesso");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao restaurar Characters de inimigos: {e.Message}");
            Debug.LogError("Os inimigos podem estar em estado inconsistente!");
        }
    }
}