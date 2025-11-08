
using UnityEngine;
using System.Collections.Generic;

public static class GameStateResetter
{
    public static void ResetGameState()
    {
        RestoreEnemyCharacters();
        RestoreBattleActions();
        ResetPlayerBehaviorAnalyzer();
        ResetDifficultyModifiers();
        ResetPlayerCharacter();
        ClearDynamicOfferPools();
        ClearMapData();
        ResetCurrency();
    }
    
    private static void RestoreBattleActions()
    {
        try
        {
            BattleActionRestorer.RestoreAllBattleActions();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao restaurar BattleActions: {e.Message}");
        }
    }
    
    private static void ResetPlayerBehaviorAnalyzer()
    {
        var allObservations = PlayerBehaviorAnalyzer.Instance.GetAllObservations();
        
        List<BehaviorObservation> deathObservations = allObservations.FindAll(obs => 
            obs.triggerType == BehaviorTriggerType.PlayerDeath ||
            obs.triggerType == BehaviorTriggerType.RepeatedBossDeath
        );
        
        PlayerBehaviorAnalyzer.Instance.ClearAllData();
        
        foreach (var deathObs in deathObservations)
        {
            PlayerBehaviorAnalyzer.Instance.AddObservationDirectly(deathObs);
        }
    }
    
    private static void ResetDifficultyModifiers()
    {
        DifficultySystem.Instance.ResetModifiers();
    }
    
    private static void ResetPlayerCharacter()
    {
        
        Character player = GameManager.Instance.PlayerCharacterInfo;
        
        player.maxHp = 100;
        player.maxMp = 100;
        player.defense = 15;
        player.speed = 2.5f;
        
        GameManager.Instance.SetPlayerCurrentHP(100);
        GameManager.Instance.SetPlayerCurrentMP(100);
        
        BattleAction ataqueSombrio = GameManager.Instance.GetInitialPlayerSkill();
        
        if (ataqueSombrio != null)
        {
            player.battleActions = new List<BattleAction> { ataqueSombrio };
            GameManager.Instance.PlayerBattleActions = new List<BattleAction> { ataqueSombrio };
        }
        else
        {
            Debug.LogError("CRÍTICO: initialPlayerSkill não está configurado no GameManager!");
            player.battleActions = new List<BattleAction>();
            GameManager.Instance.PlayerBattleActions = new List<BattleAction>();
        }
    }
    
    private static void ClearDynamicOfferPools()
    {
        if (DynamicNegotiationCardGenerator.Instance == null)
        {
            Debug.LogWarning("DynamicNegotiationCardGenerator.Instance não encontrado");
            return;
        }
        
        DynamicNegotiationCardGenerator.Instance.ProcessObservations();
    }
    
    private static void ClearMapData()
    {
        PlayerPrefs.DeleteKey("LastCompletedNode");
        PlayerPrefs.DeleteKey("CompletedBossNode");
        PlayerPrefs.DeleteKey("NextSceneAfterBoss");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearMapData("Map1");
            GameManager.Instance.ClearMapData("Map2");
            GameManager.Instance.ClearMapData("Map3");
        }
    }

    private static void ResetCurrency()
    {
        if (GameManager.Instance?.CurrencySystem == null)
        {
            Debug.LogWarning("CurrencySystem não encontrado");
            return;
        }
        
        int currentCoins = GameManager.Instance.CurrencySystem.CurrentCoins;
        if (currentCoins > 0)
        {
            GameManager.Instance.CurrencySystem.SpendCoins(currentCoins);
        }
    }

    private static void RestoreEnemyCharacters()
    {
        try
        {
            EnemyCharacterRestorer.RestoreAllEnemyCharacters();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao restaurar Characters de inimigos: {e.Message}");
        }
    }
}