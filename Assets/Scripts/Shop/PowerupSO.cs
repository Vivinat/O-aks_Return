// Assets/Scripts/Shop/PowerupSO.cs

using UnityEngine;

/// <summary>
/// Tipos de powerups disponíveis
/// </summary>

public enum PowerupType
{
    MaxHP,      // Aumenta HP máximo
    MaxMP,      // Aumenta MP máximo
    Defense,    // Aumenta defesa
    Speed,      // Aumenta velocidade
    HealHP,     // Cura HP atual
    RestoreMP   // Restaura MP atual
}

[CreateAssetMenu(fileName = "New Powerup", menuName = "Shop/Powerup")]
public class PowerupSO : ScriptableObject
{
    [Header("Basic Info")]
    public string powerupName;
    
    [TextArea]
    public string description;
    
    public Sprite icon;
    
    [Header("Effect")]
    public PowerupType powerupType;
    public int value; // Quanto aumenta o atributo
    
    [Header("Shop")]
    public int shopPrice = 50;
    
    /// <summary>
    /// Retorna uma descrição formatada do powerup
    /// </summary>
    public string GetFormattedDescription()
    {
        string baseDesc = description;
        
        baseDesc += $"\n\nPreço: {shopPrice} moedas";
        
        return baseDesc;
    }
    
    /// <summary>
    /// Aplica o efeito do powerup ao personagem
    /// </summary>
    public void ApplyToCharacter(Character character)
    {
        if (character == null)
        {
            Debug.LogError("Character é null!");
            return;
        }
        
        switch (powerupType)
        {
            case PowerupType.MaxHP:
                character.maxHp += value;
                Debug.Log($"{character.characterName} ganhou +{value} HP máximo! Novo total: {character.maxHp}");
                break;
                
            case PowerupType.MaxMP:
                character.maxMp += value;
                Debug.Log($"{character.characterName} ganhou +{value} MP máximo! Novo total: {character.maxMp}");
                break;
                
            case PowerupType.Defense:
                character.defense += value;
                Debug.Log($"{character.characterName} ganhou +{value} Defesa! Novo total: {character.defense}");
                break;
                
            case PowerupType.Speed:
                character.speed += value;
                Debug.Log($"{character.characterName} ganhou +{value} Velocidade! Novo total: {character.speed}");
                break;
                
            case PowerupType.HealHP:
                // Cura HP diretamente (limitado ao máximo)
                int currentHp = GetCurrentHP(character);
                int newHp = Mathf.Min(currentHp + value, character.maxHp);
                SetCurrentHP(character, newHp);
                int actualHeal = newHp - currentHp;
                Debug.Log($"{character.characterName} recuperou {actualHeal} HP! HP atual: {newHp}/{character.maxHp}");
                break;
                
            case PowerupType.RestoreMP:
                // Restaura MP diretamente (limitado ao máximo)
                int currentMp = GetCurrentMP(character);
                int newMp = Mathf.Min(currentMp + value, character.maxMp);
                SetCurrentMP(character, newMp);
                int actualRestore = newMp - currentMp;
                Debug.Log($"{character.characterName} recuperou {actualRestore} MP! MP atual: {newMp}/{character.maxMp}");
                break;
        }
    }
    
    // ===== MÉTODOS AUXILIARES PARA MANIPULAR HP/MP ATUAL =====
    
    /// <summary>
    /// Pega o HP atual do Character (armazenado no GameManager)
    /// </summary>
    private int GetCurrentHP(Character character)
    {
        if (GameManager.Instance != null)
        {
            return GameManager.Instance.GetPlayerCurrentHP();
        }
        return character.maxHp; // Fallback: assume HP cheio
    }
    
    /// <summary>
    /// Define o HP atual do Character (armazenado no GameManager)
    /// </summary>
    private void SetCurrentHP(Character character, int value)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerCurrentHP(value);
        }
    }
    
    /// <summary>
    /// Pega o MP atual do Character (armazenado no GameManager)
    /// </summary>
    private int GetCurrentMP(Character character)
    {
        if (GameManager.Instance != null)
        {
            return GameManager.Instance.GetPlayerCurrentMP();
        }
        return character.maxMp; // Fallback: assume MP cheio
    }
    
    /// <summary>
    /// Define o MP atual do Character (armazenado no GameManager)
    /// </summary>
    private void SetCurrentMP(Character character, int value)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerCurrentMP(value);
        }
    }
}