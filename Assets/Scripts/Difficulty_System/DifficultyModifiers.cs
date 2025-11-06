using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Registro de modificadores aplicados
/// </summary>
[System.Serializable]
public class DifficultyModifiers
{
    [Header("Player Base Stats - Applied")]
    public int playerMaxHPModifier = 0;
    public int playerMaxMPModifier = 0;
    public int playerDefenseModifier = 0;
    public float playerSpeedModifier = 0f;
    
    [Header("Player Action Modifiers - Applied")]
    public int playerActionPowerModifier = 0;
    public int playerActionManaCostModifier = 0;
    public int playerOffensiveActionPowerModifier = 0;
    public int playerDefensiveActionPowerModifier = 0;
    public int playerAOEActionPowerModifier = 0;
    public int playerSingleTargetActionPowerModifier = 0;
    
    [Header("Enemy Base Stats - Applied")]
    public int enemyMaxHPModifier = 0;
    public int enemyMaxMPModifier = 0;
    public int enemyDefenseModifier = 0;
    public float enemySpeedModifier = 0f;
    
    [Header("Enemy Action Modifiers - Applied")]
    public int enemyActionPowerModifier = 0;
    public int enemyActionManaCostModifier = 0;
    public int enemyOffensiveActionPowerModifier = 0;
    public int enemyAOEActionPowerModifier = 0;
    
    [Header("Economy Modifiers - Applied")]
    public int coinsEarnedModifier = 0;
    public int shopPricesModifier = 0;
    
    /// <summary>
    /// Registra que um modificador foi aplicado (NÃO aplica, só registra)
    /// </summary>
    public void RecordModifier(CardAttribute attribute, int value)
    {
        switch (attribute)
        {
            case CardAttribute.PlayerMaxHP:
                playerMaxHPModifier += value;
                break;
            case CardAttribute.PlayerMaxMP:
                playerMaxMPModifier += value;
                break;
            case CardAttribute.PlayerDefense:
                playerDefenseModifier += value;
                break;
            case CardAttribute.PlayerSpeed:
                playerSpeedModifier += value;
                break;
                
            case CardAttribute.PlayerActionPower:
                playerActionPowerModifier += value;
                break;
            case CardAttribute.PlayerActionManaCost:
                playerActionManaCostModifier += value;
                break;
            case CardAttribute.PlayerOffensiveActionPower:
                playerOffensiveActionPowerModifier += value;
                break;
            case CardAttribute.PlayerDefensiveActionPower:
                playerDefensiveActionPowerModifier += value;
                break;
            case CardAttribute.PlayerAOEActionPower:
                playerAOEActionPowerModifier += value;
                break;
            case CardAttribute.PlayerSingleTargetActionPower:
                playerSingleTargetActionPowerModifier += value;
                break;
                
            case CardAttribute.EnemyMaxHP:
                enemyMaxHPModifier += value;
                break;
            case CardAttribute.EnemyMaxMP:
                enemyMaxMPModifier += value;
                break;
            case CardAttribute.EnemyDefense:
                enemyDefenseModifier += value;
                break;
            case CardAttribute.EnemySpeed:
                enemySpeedModifier += value;
                break;
                
            case CardAttribute.EnemyActionPower:
                enemyActionPowerModifier += value;
                break;
            case CardAttribute.EnemyActionManaCost:
                enemyActionManaCostModifier += value;
                break;
            case CardAttribute.EnemyOffensiveActionPower:
                enemyOffensiveActionPowerModifier += value;
                break;
            case CardAttribute.EnemyAOEActionPower:
                enemyAOEActionPowerModifier += value;
                break;
                
            case CardAttribute.CoinsEarned:
                coinsEarnedModifier += value;
                break;
            case CardAttribute.ShopPrices:
                shopPricesModifier += value;
                break;
        }
    }
    
    /// <summary>
    /// Retorna o modificador registrado para um atributo
    /// </summary>
    public int GetModifier(CardAttribute attribute)
    {
        switch (attribute)
        {
            case CardAttribute.PlayerMaxHP: return playerMaxHPModifier;
            case CardAttribute.PlayerMaxMP: return playerMaxMPModifier;
            case CardAttribute.PlayerDefense: return playerDefenseModifier;
            case CardAttribute.PlayerSpeed: return Mathf.RoundToInt(playerSpeedModifier);
            
            case CardAttribute.PlayerActionPower: return playerActionPowerModifier;
            case CardAttribute.PlayerActionManaCost: return playerActionManaCostModifier;
            case CardAttribute.PlayerOffensiveActionPower: return playerOffensiveActionPowerModifier;
            case CardAttribute.PlayerDefensiveActionPower: return playerDefensiveActionPowerModifier;
            case CardAttribute.PlayerAOEActionPower: return playerAOEActionPowerModifier;
            case CardAttribute.PlayerSingleTargetActionPower: return playerSingleTargetActionPowerModifier;
            
            case CardAttribute.EnemyMaxHP: return enemyMaxHPModifier;
            case CardAttribute.EnemyMaxMP: return enemyMaxMPModifier;
            case CardAttribute.EnemyDefense: return enemyDefenseModifier;
            case CardAttribute.EnemySpeed: return Mathf.RoundToInt(enemySpeedModifier);
            
            case CardAttribute.EnemyActionPower: return enemyActionPowerModifier;
            case CardAttribute.EnemyActionManaCost: return enemyActionManaCostModifier;
            case CardAttribute.EnemyOffensiveActionPower: return enemyOffensiveActionPowerModifier;
            case CardAttribute.EnemyAOEActionPower: return enemyAOEActionPowerModifier;
            
            case CardAttribute.CoinsEarned: return coinsEarnedModifier;
            case CardAttribute.ShopPrices: return shopPricesModifier;
            
            default: return 0;
        }
    }
    
    /// <summary>
    /// Reseta todos os registros
    /// </summary>
    public void Reset()
    {
        playerMaxHPModifier = 0;
        playerMaxMPModifier = 0;
        playerDefenseModifier = 0;
        playerSpeedModifier = 0f;
        
        playerActionPowerModifier = 0;
        playerActionManaCostModifier = 0;
        playerOffensiveActionPowerModifier = 0;
        playerDefensiveActionPowerModifier = 0;
        playerAOEActionPowerModifier = 0;
        playerSingleTargetActionPowerModifier = 0;
        
        enemyMaxHPModifier = 0;
        enemyMaxMPModifier = 0;
        enemyDefenseModifier = 0;
        enemySpeedModifier = 0f;
        
        enemyActionPowerModifier = 0;
        enemyActionManaCostModifier = 0;
        enemyOffensiveActionPowerModifier = 0;
        enemyAOEActionPowerModifier = 0;
        
        coinsEarnedModifier = 0;
        shopPricesModifier = 0;
    }
    
    /// <summary>
    /// Retorna um resumo dos modificadores registrados
    /// </summary>
    public string GetSummary()
    {
        System.Text.StringBuilder summary = new System.Text.StringBuilder();
        summary.AppendLine("=== HISTÓRICO DE MODIFICAÇÕES ===");
        
        // Player base stats
        if (playerMaxHPModifier != 0 || playerMaxMPModifier != 0 || 
            playerDefenseModifier != 0 || playerSpeedModifier != 0)
        {
            summary.AppendLine("\n<color=#90EE90>JOGADOR - STATS:</color>");
            if (playerMaxHPModifier != 0) summary.AppendLine($"  HP Máximo: {FormatValue(playerMaxHPModifier)}");
            if (playerMaxMPModifier != 0) summary.AppendLine($"  MP Máximo: {FormatValue(playerMaxMPModifier)}");
            if (playerDefenseModifier != 0) summary.AppendLine($"  Defesa: {FormatValue(playerDefenseModifier)}");
            if (playerSpeedModifier != 0) summary.AppendLine($"  Velocidade: {FormatValue(playerSpeedModifier)}");
        }
        
        // Player actions
        bool hasPlayerActionMods = playerActionPowerModifier != 0 || 
                                   playerActionManaCostModifier != 0 ||
                                   playerOffensiveActionPowerModifier != 0 ||
                                   playerDefensiveActionPowerModifier != 0 ||
                                   playerAOEActionPowerModifier != 0 ||
                                   playerSingleTargetActionPowerModifier != 0;
        
        if (hasPlayerActionMods)
        {
            summary.AppendLine("\n<color=#90EE90>JOGADOR - AÇÕES:</color>");
            if (playerActionPowerModifier != 0) 
                summary.AppendLine($"  Poder Geral: {FormatValue(playerActionPowerModifier)}");
            if (playerOffensiveActionPowerModifier != 0) 
                summary.AppendLine($"  Poder Ofensivo: {FormatValue(playerOffensiveActionPowerModifier)}");
            if (playerDefensiveActionPowerModifier != 0) 
                summary.AppendLine($"  Poder Defensivo: {FormatValue(playerDefensiveActionPowerModifier)}");
            if (playerAOEActionPowerModifier != 0) 
                summary.AppendLine($"  Poder AOE: {FormatValue(playerAOEActionPowerModifier)}");
            if (playerSingleTargetActionPowerModifier != 0) 
                summary.AppendLine($"  Poder Single: {FormatValue(playerSingleTargetActionPowerModifier)}");
            if (playerActionManaCostModifier != 0) 
                summary.AppendLine($"  Custo Mana: {FormatValue(playerActionManaCostModifier)}");
        }
        
        // Enemy stats
        if (enemyMaxHPModifier != 0 || enemyMaxMPModifier != 0 || 
            enemyDefenseModifier != 0 || enemySpeedModifier != 0)
        {
            summary.AppendLine("\n<color=#FF6B6B>INIMIGOS - STATS:</color>");
            if (enemyMaxHPModifier != 0) summary.AppendLine($"  HP Máximo: {FormatValue(enemyMaxHPModifier)}");
            if (enemyMaxMPModifier != 0) summary.AppendLine($"  MP Máximo: {FormatValue(enemyMaxMPModifier)}");
            if (enemyDefenseModifier != 0) summary.AppendLine($"  Defesa: {FormatValue(enemyDefenseModifier)}");
            if (enemySpeedModifier != 0) summary.AppendLine($"  Velocidade: {FormatValue(enemySpeedModifier)}");
        }
        
        // Enemy actions
        bool hasEnemyActionMods = enemyActionPowerModifier != 0 || 
                                  enemyActionManaCostModifier != 0 ||
                                  enemyOffensiveActionPowerModifier != 0 ||
                                  enemyAOEActionPowerModifier != 0;
        
        if (hasEnemyActionMods)
        {
            summary.AppendLine("\n<color=#FF6B6B>INIMIGOS - AÇÕES:</color>");
            if (enemyActionPowerModifier != 0) 
                summary.AppendLine($"  Poder Geral: {FormatValue(enemyActionPowerModifier)}");
            if (enemyOffensiveActionPowerModifier != 0) 
                summary.AppendLine($"  Poder Ofensivo: {FormatValue(enemyOffensiveActionPowerModifier)}");
            if (enemyAOEActionPowerModifier != 0) 
                summary.AppendLine($"  Poder AOE: {FormatValue(enemyAOEActionPowerModifier)}");
            if (enemyActionManaCostModifier != 0) 
                summary.AppendLine($"  Custo Mana: {FormatValue(enemyActionManaCostModifier)}");
        }
        
        // Economy
        if (coinsEarnedModifier != 0 || shopPricesModifier != 0)
        {
            summary.AppendLine("\n<color=#FFD700>ECONOMIA:</color>");
            if (coinsEarnedModifier != 0) summary.AppendLine($"  Moedas: {FormatValue(coinsEarnedModifier)}");
            if (shopPricesModifier != 0) summary.AppendLine($"  Preços: {FormatValue(shopPricesModifier)}");
        }
        
        return summary.ToString();
    }
    
    private string FormatValue(float value)
    {
        return value > 0 ? $"+{value}" : value.ToString();
    }
}