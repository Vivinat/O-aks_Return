// Assets/Scripts/Managers/CurrencySystem.cs

using UnityEngine;

[System.Serializable]
public class CurrencySystem
{
    [SerializeField]
    private int currentCoins = 100; // Moedas iniciais do jogador
    
    public int CurrentCoins => currentCoins;
    
    /// <summary>
    /// Tenta gastar moedas
    /// </summary>
    /// <param name="amount">Quantidade a gastar</param>
    /// <returns>True se conseguiu gastar, False se não tem moedas suficientes</returns>
    public bool SpendCoins(int amount)
    {
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            Debug.Log($"Gastou {amount} moedas. Restam: {currentCoins}");
            return true;
        }
        
        Debug.Log($"Moedas insuficientes! Tem: {currentCoins}, precisa: {amount}");
        return false;
    }
    
    /// <summary>
    /// Adiciona moedas ao jogador
    /// </summary>
    /// <param name="amount">Quantidade a adicionar</param>
    public void AddCoins(int amount)
    {
        currentCoins += amount;
        Debug.Log($"Ganhou {amount} moedas. Total: {currentCoins}");
    }
    
    /// <summary>
    /// Verifica se o jogador tem moedas suficientes
    /// </summary>
    /// <param name="amount">Quantidade necessária</param>
    /// <returns>True se tem moedas suficientes</returns>
    public bool HasEnoughCoins(int amount)
    {
        return currentCoins >= amount;
    }
    
    /// <summary>
    /// Define a quantidade de moedas (útil para debug/cheats)
    /// </summary>
    /// <param name="amount">Nova quantidade de moedas</param>
    public void SetCoins(int amount)
    {
        currentCoins = Mathf.Max(0, amount);
        Debug.Log($"Moedas definidas para: {currentCoins}");
    }
}