// Assets/Scripts/Difficulty_System/DifficultyModifiers.cs

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Armazena todos os modificadores de dificuldade aplicados por negociações
/// </summary>
[System.Serializable]
public class DifficultyModifiers
{
    [Header("Player Modifiers")]
    public int playerMaxHPModifier = 0;
    public int playerMaxMPModifier = 0;
    public int playerDefenseModifier = 0;
    public float playerSpeedModifier = 0f;
    public int playerActionPowerModifier = 0;
    public int playerActionManaCostModifier = 0;
    
    [Header("Enemy Modifiers")]
    public int enemyMaxHPModifier = 0;
    public int enemyMaxMPModifier = 0;
    public int enemyDefenseModifier = 0;
    public float enemySpeedModifier = 0f;
    public int enemyActionPowerModifier = 0;
    public int enemyActionManaCostModifier = 0;
    
    [Header("Economy Modifiers")]
    public int coinsEarnedModifier = 0;
    public int shopPricesModifier = 0;
    
    /// <summary>
    /// Aplica um modificador baseado no atributo
    /// </summary>
    public void ApplyModifier(CardAttribute attribute, int value)
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
                
            case CardAttribute.CoinsEarned:
                coinsEarnedModifier += value;
                break;
            case CardAttribute.ShopPrices:
                shopPricesModifier += value;
                break;
        }
    }
    
    /// <summary>
    /// Retorna o modificador para um atributo específico
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
            
            case CardAttribute.EnemyMaxHP: return enemyMaxHPModifier;
            case CardAttribute.EnemyMaxMP: return enemyMaxMPModifier;
            case CardAttribute.EnemyDefense: return enemyDefenseModifier;
            case CardAttribute.EnemySpeed: return Mathf.RoundToInt(enemySpeedModifier);
            case CardAttribute.EnemyActionPower: return enemyActionPowerModifier;
            case CardAttribute.EnemyActionManaCost: return enemyActionManaCostModifier;
            
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
        playerMaxHPModifier = 0;
        playerMaxMPModifier = 0;
        playerDefenseModifier = 0;
        playerSpeedModifier = 0f;
        playerActionPowerModifier = 0;
        playerActionManaCostModifier = 0;
        
        enemyMaxHPModifier = 0;
        enemyMaxMPModifier = 0;
        enemyDefenseModifier = 0;
        enemySpeedModifier = 0f;
        enemyActionPowerModifier = 0;
        enemyActionManaCostModifier = 0;
        
        coinsEarnedModifier = 0;
        shopPricesModifier = 0;
    }
    
    /// <summary>
    /// Retorna um resumo dos modificadores ativos
    /// </summary>
    public string GetSummary()
    {
        System.Text.StringBuilder summary = new System.Text.StringBuilder();
        summary.AppendLine("=== MODIFICADORES ATIVOS ===");
        
        summary.AppendLine("\n<color=#90EE90>JOGADOR:</color>");
        if (playerMaxHPModifier != 0) summary.AppendLine($"  HP Máximo: {FormatValue(playerMaxHPModifier)}");
        if (playerMaxMPModifier != 0) summary.AppendLine($"  MP Máximo: {FormatValue(playerMaxMPModifier)}");
        if (playerDefenseModifier != 0) summary.AppendLine($"  Defesa: {FormatValue(playerDefenseModifier)}");
        if (playerSpeedModifier != 0) summary.AppendLine($"  Velocidade: {FormatValue(playerSpeedModifier)}");
        if (playerActionPowerModifier != 0) summary.AppendLine($"  Poder de Ações: {FormatValue(playerActionPowerModifier)}");
        if (playerActionManaCostModifier != 0) summary.AppendLine($"  Custo de Mana: {FormatValue(playerActionManaCostModifier)}");
        
        summary.AppendLine("\n<color=#FF6B6B>INIMIGOS:</color>");
        if (enemyMaxHPModifier != 0) summary.AppendLine($"  HP Máximo: {FormatValue(enemyMaxHPModifier)}");
        if (enemyMaxMPModifier != 0) summary.AppendLine($"  MP Máximo: {FormatValue(enemyMaxMPModifier)}");
        if (enemyDefenseModifier != 0) summary.AppendLine($"  Defesa: {FormatValue(enemyDefenseModifier)}");
        if (enemySpeedModifier != 0) summary.AppendLine($"  Velocidade: {FormatValue(enemySpeedModifier)}");
        if (enemyActionPowerModifier != 0) summary.AppendLine($"  Poder de Ações: {FormatValue(enemyActionPowerModifier)}");
        if (enemyActionManaCostModifier != 0) summary.AppendLine($"  Custo de Mana: {FormatValue(enemyActionManaCostModifier)}");
        
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