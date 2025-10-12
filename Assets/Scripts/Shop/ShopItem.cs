// Assets/Scripts/Shop/ShopItem.cs

using UnityEngine;

/// <summary>
/// Wrapper para itens da loja (pode ser BattleAction ou Powerup)
/// </summary>
public class ShopItem
{
    public enum ItemType
    {
        BattleAction,
        Powerup
    }
    
    public ItemType type;
    public BattleAction battleAction;
    public PowerupSO powerup;
    
    // Construtor para BattleAction
    public ShopItem(BattleAction action)
    {
        type = ItemType.BattleAction;
        battleAction = action;
        powerup = null;
    }
    
    // Construtor para Powerup
    public ShopItem(PowerupSO powerupData)
    {
        type = ItemType.Powerup;
        battleAction = null;
        powerup = powerupData;
    }
    
    // Propriedades úteis
    public string Name
    {
        get
        {
            if (type == ItemType.BattleAction && battleAction != null)
                return battleAction.actionName;
            if (type == ItemType.Powerup && powerup != null)
                return powerup.powerupName;
            return "Unknown";
        }
    }
    
    public string Description
    {
        get
        {
            if (type == ItemType.BattleAction && battleAction != null)
                return battleAction.description;
            if (type == ItemType.Powerup && powerup != null)
                return powerup.GetFormattedDescription();
            return "";
        }
    }
    
    public Sprite Icon
    {
        get
        {
            if (type == ItemType.BattleAction && battleAction != null)
                return battleAction.icon;
            if (type == ItemType.Powerup && powerup != null)
                return powerup.icon;
            return null;
        }
    }
    
    public int Price
    {
        get
        {
            if (type == ItemType.BattleAction && battleAction != null)
                return battleAction.shopPrice;
            if (type == ItemType.Powerup && powerup != null)
                return powerup.shopPrice;
            return 0;
        }
    }
    
    public bool IsConsumable
    {
        get
        {
            if (type == ItemType.BattleAction && battleAction != null)
                return battleAction.isConsumable;
            return false; // Powerups não são "consumíveis" no sentido de BattleAction
        }
    }
}