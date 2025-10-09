// Assets/Scripts/Difficulty_System/DifficultySystem.cs

using UnityEngine;
using System.IO;

/// <summary>
/// Sistema central de dificuldade que aplica modificadores de negociações
/// </summary>
public class DifficultySystem : MonoBehaviour
{
    public static DifficultySystem Instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private bool saveModifiers = true;
    [SerializeField] private string saveFileName = "difficulty_modifiers.json";
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Current Modifiers")]
    [SerializeField] private DifficultyModifiers modifiers = new DifficultyModifiers();
    
    public DifficultyModifiers Modifiers => modifiers;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadModifiers();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void OnApplicationQuit()
    {
        SaveModifiers();
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveModifiers();
    }
    
    /// <summary>
    /// Aplica uma negociação aos modificadores
    /// </summary>
    public void ApplyNegotiation(CardAttribute playerAttr, CardAttribute enemyAttr, int value)
    {
        DebugLog($"=== APLICANDO NEGOCIAÇÃO ===");
        DebugLog($"Jogador: {playerAttr} {FormatValue(value)}");
        DebugLog($"Inimigo: {enemyAttr} {FormatValue(value)}");
        
        // Aplica modificador do jogador
        if (playerAttr != CardAttribute.PlayerMaxHP || value != 0) // Verifica se não é um placeholder
        {
            modifiers.ApplyModifier(playerAttr, value);
            DebugLog($"✅ Aplicado ao jogador: {AttributeHelper.GetDisplayName(playerAttr)} {FormatValue(value)}");
        }
        
        // Aplica modificador do inimigo
        if (enemyAttr != CardAttribute.EnemyMaxHP || value != 0) // Verifica se não é um placeholder
        {
            modifiers.ApplyModifier(enemyAttr, value);
            DebugLog($"✅ Aplicado aos inimigos: {AttributeHelper.GetDisplayName(enemyAttr)} {FormatValue(value)}");
        }
        
        SaveModifiers();
        
        DebugLog("\n" + modifiers.GetSummary());
    }
    
    /// <summary>
    /// Aplica modificadores a um Character (inimigo)
    /// </summary>
    public void ApplyToEnemy(Character enemy)
    {
        if (enemy == null || enemy.team != Team.Enemy) return;
        
        DebugLog($"Aplicando modificadores ao inimigo: {enemy.characterName}");
        
        // Aplica modificadores de stats
        enemy.maxHp = Mathf.Max(1, enemy.maxHp + modifiers.enemyMaxHPModifier);
        enemy.maxMp = Mathf.Max(0, enemy.maxMp + modifiers.enemyMaxMPModifier);
        enemy.defense = Mathf.Max(0, enemy.defense + modifiers.enemyDefenseModifier);
        enemy.speed = Mathf.Max(0.1f, enemy.speed + modifiers.enemySpeedModifier);
        
        // Aplica modificadores nas ações
        if (enemy.battleActions != null)
        {
            foreach (var action in enemy.battleActions)
            {
                if (action == null) continue;
                
                // Modifica poder das ações
                if (modifiers.enemyActionPowerModifier != 0)
                {
                    foreach (var effect in action.effects)
                    {
                        if (effect.effectType == ActionType.Attack)
                        {
                            effect.power = Mathf.Max(1, effect.power + modifiers.enemyActionPowerModifier);
                        }
                    }
                }
                
                // Modifica custo de mana
                if (modifiers.enemyActionManaCostModifier != 0)
                {
                    action.manaCost = Mathf.Max(0, action.manaCost + modifiers.enemyActionManaCostModifier);
                }
            }
        }
        
        DebugLog($"✅ {enemy.characterName} modificado - HP: {enemy.maxHp}, Speed: {enemy.speed}");
    }
    
    /// <summary>
    /// Aplica modificadores ao jogador
    /// </summary>
    public void ApplyToPlayer(Character player)
    {
        if (player == null || player.team != Team.Player) return;
        
        DebugLog($"Aplicando modificadores ao jogador: {player.characterName}");
        
        // Aplica modificadores de stats
        player.maxHp = Mathf.Max(1, player.maxHp + modifiers.playerMaxHPModifier);
        player.maxMp = Mathf.Max(0, player.maxMp + modifiers.playerMaxMPModifier);
        player.defense = Mathf.Max(0, player.defense + modifiers.playerDefenseModifier);
        player.speed = Mathf.Max(0.1f, player.speed + modifiers.playerSpeedModifier);
        
        // Aplica modificadores nas ações
        if (player.battleActions != null)
        {
            foreach (var action in player.battleActions)
            {
                if (action == null) continue;
                
                // Modifica poder das ações
                if (modifiers.playerActionPowerModifier != 0)
                {
                    foreach (var effect in action.effects)
                    {
                        if (effect.effectType == ActionType.Attack)
                        {
                            effect.power = Mathf.Max(1, effect.power + modifiers.playerActionPowerModifier);
                        }
                    }
                }
                
                // Modifica custo de mana
                if (modifiers.playerActionManaCostModifier != 0)
                {
                    action.manaCost = Mathf.Max(0, action.manaCost + modifiers.playerActionManaCostModifier);
                }
            }
        }
        
        DebugLog($"✅ {player.characterName} modificado - HP: {player.maxHp}, Speed: {player.speed}");
    }
    
    /// <summary>
    /// Calcula moedas modificadas
    /// </summary>
    public int GetModifiedCoins(int baseCoins)
    {
        int modified = Mathf.Max(0, baseCoins + modifiers.coinsEarnedModifier);
        
        if (modified != baseCoins)
        {
            DebugLog($"Moedas modificadas: {baseCoins} -> {modified}");
        }
        
        return modified;
    }
    
    /// <summary>
    /// Calcula preço de loja modificado
    /// </summary>
    public int GetModifiedShopPrice(int basePrice)
    {
        int modified = Mathf.Max(1, basePrice + modifiers.shopPricesModifier);
        
        if (modified != basePrice)
        {
            DebugLog($"Preço modificado: {basePrice} -> {modified}");
        }
        
        return modified;
    }
    
    /// <summary>
    /// Reseta todos os modificadores
    /// </summary>
    [ContextMenu("Reset All Modifiers")]
    public void ResetModifiers()
    {
        modifiers.Reset();
        SaveModifiers();
        DebugLog("✅ Todos os modificadores foram resetados!");
    }
    
    /// <summary>
    /// Salva modificadores em arquivo
    /// </summary>
    private void SaveModifiers()
    {
        if (!saveModifiers) return;
        
        try
        {
            string dataPath = Path.Combine(Application.persistentDataPath, saveFileName);
            string jsonData = JsonUtility.ToJson(modifiers, true);
            File.WriteAllText(dataPath, jsonData);
            
            DebugLog($"Modificadores salvos em: {dataPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao salvar modificadores: {e.Message}");
        }
    }
    
    /// <summary>
    /// Carrega modificadores do arquivo
    /// </summary>
    private void LoadModifiers()
    {
        if (!saveModifiers) return;
        
        try
        {
            string dataPath = Path.Combine(Application.persistentDataPath, saveFileName);
            
            if (File.Exists(dataPath))
            {
                string jsonData = File.ReadAllText(dataPath);
                modifiers = JsonUtility.FromJson<DifficultyModifiers>(jsonData);
                
                if (modifiers == null)
                {
                    modifiers = new DifficultyModifiers();
                }
                
                DebugLog($"Modificadores carregados!");
                DebugLog(modifiers.GetSummary());
            }
            else
            {
                modifiers = new DifficultyModifiers();
                DebugLog("Nenhum modificador salvo - iniciando limpo");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao carregar modificadores: {e.Message}");
            modifiers = new DifficultyModifiers();
        }
    }
    
    private string FormatValue(int value)
    {
        return value > 0 ? $"+{value}" : value.ToString();
    }
    
    private void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"<color=orange>[DifficultySystem]</color> {message}");
        }
    }
    
    [ContextMenu("Show Current Modifiers")]
    private void ShowCurrentModifiers()
    {
        Debug.Log(modifiers.GetSummary());
    }
}