using UnityEngine;

/// <summary>
/// Wrapper para itens da loja
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
    
    public ShopItem(BattleAction action)
    {
        type = ItemType.BattleAction;
        battleAction = action;
        powerup = null;
    }
    
    public ShopItem(PowerupSO powerupData)
    {
        type = ItemType.Powerup;
        battleAction = null;
        powerup = powerupData;
    }
    
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
                return battleAction.GetDynamicDescription(); 
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
            return false; 
        }
    }
}