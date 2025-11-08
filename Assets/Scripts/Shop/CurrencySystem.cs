using UnityEngine;

[System.Serializable]
public class CurrencySystem
{
    [SerializeField]
    private int currentCoins = 100;
    
    public int CurrentCoins => currentCoins;
    
    public bool SpendCoins(int amount)
    {
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            return true;
        }
        
        return false;
    }
    
    public int RemoveCoins(int amount)
    {
        int coinsToRemove = Mathf.Min(amount, currentCoins);
        currentCoins -= coinsToRemove;
        return coinsToRemove;
    }
    
    public void AddCoins(int amount)
    {
        currentCoins += amount;
    }
    
    public bool HasEnoughCoins(int amount)
    {
        return currentCoins >= amount;
    }
    
    public void SetCoins(int amount)
    {
        currentCoins = Mathf.Max(0, amount);
    }
}