// Assets/Scripts/Difficulty_System/DifficultyModifiers.cs (EXPANDED)

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Armazena todos os modificadores de dificuldade aplicados por negociações
/// EXPANDIDO: Inclui modificadores específicos para tipos de ações
/// </summary>
[System.Serializable]
public class DifficultyModifiers
{
    [Header("Player Base Stats")]
    public int playerMaxHPModifier = 0;
    public int playerMaxMPModifier = 0;
    public int playerDefenseModifier = 0;
    public float playerSpeedModifier = 0f;
    
    [Header("Player Action Modifiers - General")]
    public int playerActionPowerModifier = 0;
    public int playerActionManaCostModifier = 0;
    
    [Header("Player Action Modifiers - Specific (NOVO)")]
    public int playerOffensiveActionPowerModifier = 0;    // Apenas ataques
    public int playerDefensiveActionPowerModifier = 0;    // Cura/buffs
    public int playerAOEActionPowerModifier = 0;          // Ataques em área
    public int playerSingleTargetActionPowerModifier = 0; // Ataques single-target
    
    [Header("Enemy Base Stats")]
    public int enemyMaxHPModifier = 0;
    public int enemyMaxMPModifier = 0;
    public int enemyDefenseModifier = 0;
    public float enemySpeedModifier = 0f;
    
    [Header("Enemy Action Modifiers - General")]
    public int enemyActionPowerModifier = 0;
    public int enemyActionManaCostModifier = 0;
    
    [Header("Enemy Action Modifiers - Specific (NOVO)")]
    public int enemyOffensiveActionPowerModifier = 0;     // Apenas ataques inimigos
    public int enemyAOEActionPowerModifier = 0;           // Ataques em área inimigos
    
    [Header("Economy Modifiers")]
    public int coinsEarnedModifier = 0;
    public int shopPricesModifier = 0;
    
    /// <summary>
    /// Aplica um modificador baseado no atributo (EXPANDIDO)
    /// </summary>
    public void ApplyModifier(CardAttribute attribute, int value)
    {
        switch (attribute)
        {
            // Player base stats
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
                
            // Player action modifiers - general
            case CardAttribute.PlayerActionPower:
                playerActionPowerModifier += value;
                break;
            case CardAttribute.PlayerActionManaCost:
                playerActionManaCostModifier += value;
                break;
                
            // Player action modifiers - specific (NOVO)
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
                
            // Enemy base stats
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
                
            // Enemy action modifiers - general
            case CardAttribute.EnemyActionPower:
                enemyActionPowerModifier += value;
                break;
            case CardAttribute.EnemyActionManaCost:
                enemyActionManaCostModifier += value;
                break;
                
            // Enemy action modifiers - specific (NOVO)
            case CardAttribute.EnemyOffensiveActionPower:
                enemyOffensiveActionPowerModifier += value;
                break;
            case CardAttribute.EnemyAOEActionPower:
                enemyAOEActionPowerModifier += value;
                break;
                
            // Economy
            case CardAttribute.CoinsEarned:
                coinsEarnedModifier += value;
                break;
            case CardAttribute.ShopPrices:
                shopPricesModifier += value;
                break;
        }
    }
    
    /// <summary>
    /// Retorna o modificador para um atributo específico (EXPANDIDO)
    /// </summary>
    public int GetModifier(CardAttribute attribute)
    {
        switch (attribute)
        {
            // Player base stats
            case CardAttribute.PlayerMaxHP: return playerMaxHPModifier;
            case CardAttribute.PlayerMaxMP: return playerMaxMPModifier;
            case CardAttribute.PlayerDefense: return playerDefenseModifier;
            case CardAttribute.PlayerSpeed: return Mathf.RoundToInt(playerSpeedModifier);
            
            // Player action modifiers
            case CardAttribute.PlayerActionPower: return playerActionPowerModifier;
            case CardAttribute.PlayerActionManaCost: return playerActionManaCostModifier;
            case CardAttribute.PlayerOffensiveActionPower: return playerOffensiveActionPowerModifier;
            case CardAttribute.PlayerDefensiveActionPower: return playerDefensiveActionPowerModifier;
            case CardAttribute.PlayerAOEActionPower: return playerAOEActionPowerModifier;
            case CardAttribute.PlayerSingleTargetActionPower: return playerSingleTargetActionPowerModifier;
            
            // Enemy base stats
            case CardAttribute.EnemyMaxHP: return enemyMaxHPModifier;
            case CardAttribute.EnemyMaxMP: return enemyMaxMPModifier;
            case CardAttribute.EnemyDefense: return enemyDefenseModifier;
            case CardAttribute.EnemySpeed: return Mathf.RoundToInt(enemySpeedModifier);
            
            // Enemy action modifiers
            case CardAttribute.EnemyActionPower: return enemyActionPowerModifier;
            case CardAttribute.EnemyActionManaCost: return enemyActionManaCostModifier;
            case CardAttribute.EnemyOffensiveActionPower: return enemyOffensiveActionPowerModifier;
            case CardAttribute.EnemyAOEActionPower: return enemyAOEActionPowerModifier;
            
            // Economy
            case CardAttribute.CoinsEarned: return coinsEarnedModifier;
            case CardAttribute.ShopPrices: return shopPricesModifier;
            
            default: return 0;
        }
    }
    
    /// <summary>
    /// Reseta todos os modificadores
    /// </summary>
    public void Reset()
    {
        // Player base stats
        playerMaxHPModifier = 0;
        playerMaxMPModifier = 0;
        playerDefenseModifier = 0;
        playerSpeedModifier = 0f;
        
        // Player action modifiers - general
        playerActionPowerModifier = 0;
        playerActionManaCostModifier = 0;
        
        // Player action modifiers - specific
        playerOffensiveActionPowerModifier = 0;
        playerDefensiveActionPowerModifier = 0;
        playerAOEActionPowerModifier = 0;
        playerSingleTargetActionPowerModifier = 0;
        
        // Enemy base stats
        enemyMaxHPModifier = 0;
        enemyMaxMPModifier = 0;
        enemyDefenseModifier = 0;
        enemySpeedModifier = 0f;
        
        // Enemy action modifiers - general
        enemyActionPowerModifier = 0;
        enemyActionManaCostModifier = 0;
        
        // Enemy action modifiers - specific
        enemyOffensiveActionPowerModifier = 0;
        enemyAOEActionPowerModifier = 0;
        
        // Economy
        coinsEarnedModifier = 0;
        shopPricesModifier = 0;
    }
    
    /// <summary>
    /// Retorna um resumo dos modificadores ativos (MELHORADO)
    /// </summary>
    public string GetSummary()
    {
        System.Text.StringBuilder summary = new System.Text.StringBuilder();
        summary.AppendLine("=== MODIFICADORES ATIVOS ===");
        
        // Player base stats
        summary.AppendLine("\n<color=#90EE90>JOGADOR - STATS BASE:</color>");
        if (playerMaxHPModifier != 0) summary.AppendLine($"  HP Máximo: {FormatValue(playerMaxHPModifier)}");
        if (playerMaxMPModifier != 0) summary.AppendLine($"  MP Máximo: {FormatValue(playerMaxMPModifier)}");
        if (playerDefenseModifier != 0) summary.AppendLine($"  Defesa: {FormatValue(playerDefenseModifier)}");
        if (playerSpeedModifier != 0) summary.AppendLine($"  Velocidade: {FormatValue(playerSpeedModifier)}");
        
        // Player action modifiers
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
                summary.AppendLine($"  Poder Defensivo/Cura: {FormatValue(playerDefensiveActionPowerModifier)}");
            if (playerAOEActionPowerModifier != 0) 
                summary.AppendLine($"  Poder AOE: {FormatValue(playerAOEActionPowerModifier)}");
            if (playerSingleTargetActionPowerModifier != 0) 
                summary.AppendLine($"  Poder Single-Target: {FormatValue(playerSingleTargetActionPowerModifier)}");
            if (playerActionManaCostModifier != 0) 
                summary.AppendLine($"  Custo de Mana: {FormatValue(playerActionManaCostModifier)}");
        }
        
        // Enemy base stats
        summary.AppendLine("\n<color=#FF6B6B>INIMIGOS - STATS BASE:</color>");
        if (enemyMaxHPModifier != 0) summary.AppendLine($"  HP Máximo: {FormatValue(enemyMaxHPModifier)}");
        if (enemyMaxMPModifier != 0) summary.AppendLine($"  MP Máximo: {FormatValue(enemyMaxMPModifier)}");
        if (enemyDefenseModifier != 0) summary.AppendLine($"  Defesa: {FormatValue(enemyDefenseModifier)}");
        if (enemySpeedModifier != 0) summary.AppendLine($"  Velocidade: {FormatValue(enemySpeedModifier)}");
        
        // Enemy action modifiers
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
                summary.AppendLine($"  Custo de Mana: {FormatValue(enemyActionManaCostModifier)}");
        }
        
        // Economy
        summary.AppendLine("\n<color=#FFD700>ECONOMIA:</color>");
        if (coinsEarnedModifier != 0) summary.AppendLine($"  Moedas Ganhas: {FormatValue(coinsEarnedModifier)}");
        if (shopPricesModifier != 0) summary.AppendLine($"  Preços da Loja: {FormatValue(shopPricesModifier)}");
        
        return summary.ToString();
    }
    
    private string FormatValue(float value)
    {
        return value > 0 ? $"+{value}" : value.ToString();
    }
}