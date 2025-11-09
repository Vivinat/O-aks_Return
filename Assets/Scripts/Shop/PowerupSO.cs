using UnityEngine;

public enum PowerupType
{
    MaxHP,
    MaxMP,
    Defense,
    Speed,
    HealHP,
    RestoreMP
}

[CreateAssetMenu(fileName = "New Powerup", menuName = "Shop/Powerup")]
public class PowerupSO : ScriptableObject
{
    public string powerupName;
    
    [TextArea]
    public string description;
    
    public Sprite icon;
    
    public PowerupType powerupType;
    public int value;
    
    public int shopPrice = 50;
    
    public string GetFormattedDescription()
    {
        string baseDesc = description;
        baseDesc += $"\n\nPreço: {shopPrice} moedas";
        return baseDesc;
    }
    
    // Aplica o efeito do powerup ao personagem
    public void ApplyToCharacter(Character character)
    {
        
        switch (powerupType)
        {
            case PowerupType.MaxHP:
                character.maxHp += value;
                break;
                
            case PowerupType.MaxMP:
                character.maxMp += value;
                break;
                
            case PowerupType.Defense:
                character.defense += value;
                break;
                
            case PowerupType.Speed:
                character.speed += value;
                break;
                
            case PowerupType.HealHP:
                int currentHp = GetCurrentHP(character);
                int newHp = Mathf.Min(currentHp + value, character.maxHp);
                SetCurrentHP(character, newHp);
                break;
                
            case PowerupType.RestoreMP:
                int currentMp = GetCurrentMP(character);
                int newMp = Mathf.Min(currentMp + value, character.maxMp);
                SetCurrentMP(character, newMp);
                break;
        }
    }
    
    private int GetCurrentHP(Character character)
    {
        if (GameManager.Instance != null)
        {
            return GameManager.Instance.GetPlayerCurrentHP();
        }
        return character.maxHp;
    }
    
    private void SetCurrentHP(Character character, int value)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerCurrentHP(value);
        }
    }
    
    private int GetCurrentMP(Character character)
    {
        if (GameManager.Instance != null)
        {
            return GameManager.Instance.GetPlayerCurrentMP();
        }
        return character.maxMp;
    }
    
    private void SetCurrentMP(Character character, int value)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerCurrentMP(value);
        }
    }
}