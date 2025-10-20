// Assets/Scripts/Difficulty_System/DifficultySystem.cs (FIXED - Corrigido para usar IntensityHelper)

using UnityEngine;
using System.IO;

/// <summary>
/// Sistema central de dificuldade que aplica modificadores de negociações
/// MELHORADO: Suporta modificadores específicos de tipos de ações
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
    /// Aplica modificadores a um Character (inimigo)
    /// MELHORADO: Suporta modificadores específicos de ações
    /// </summary>
    public void ApplyToEnemy(Character enemy)
    {
        if (enemy == null || enemy.team != Team.Enemy) return;
        
        DebugLog($"Aplicando modificadores ao inimigo: {enemy.characterName}");
        
        // Stats básicos
        enemy.maxHp = Mathf.Max(1, enemy.maxHp + modifiers.enemyMaxHPModifier);
        enemy.maxMp = Mathf.Max(0, enemy.maxMp + modifiers.enemyMaxMPModifier);
        enemy.defense = Mathf.Max(0, enemy.defense + modifiers.enemyDefenseModifier);
        enemy.speed = Mathf.Max(0.1f, enemy.speed + modifiers.enemySpeedModifier);
        
        // Aplica modificadores nas ações (MELHORADO)
        if (enemy.battleActions != null)
        {
            foreach (var action in enemy.battleActions)
            {
                if (action == null || action.effects == null) continue;
                
                foreach (var effect in action.effects)
                {
                    // Modificador geral de poder
                    if (modifiers.enemyActionPowerModifier != 0 && effect.effectType == ActionType.Attack)
                    {
                        effect.power = Mathf.Max(1, effect.power + modifiers.enemyActionPowerModifier);
                    }
                    
                    // NOVO: Modificador específico de ataques ofensivos
                    if (modifiers.enemyOffensiveActionPowerModifier != 0 && effect.effectType == ActionType.Attack)
                    {
                        effect.power = Mathf.Max(1, effect.power + modifiers.enemyOffensiveActionPowerModifier);
                    }
                    
                    // NOVO: Modificador específico de AOE
                    if (modifiers.enemyAOEActionPowerModifier != 0 && IsAOEAction(action))
                    {
                        if (effect.effectType == ActionType.Attack)
                        {
                            effect.power = Mathf.Max(1, effect.power + modifiers.enemyAOEActionPowerModifier);
                        }
                    }
                }
                
                // Modificador de custo de mana
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
    /// MELHORADO: Suporta modificadores específicos de ações
    /// </summary>
    public void ApplyToPlayer(Character player)
    {
        if (player == null || player.team != Team.Player) return;
        
        DebugLog($"Aplicando modificadores ao jogador: {player.characterName}");
        
        // Stats básicos
        player.maxHp = Mathf.Max(1, player.maxHp + modifiers.playerMaxHPModifier);
        player.maxMp = Mathf.Max(0, player.maxMp + modifiers.playerMaxMPModifier);
        player.defense = Mathf.Max(0, player.defense + modifiers.playerDefenseModifier);
        player.speed = Mathf.Max(0.1f, player.speed + modifiers.playerSpeedModifier);
        
        // Aplica modificadores nas ações (MELHORADO)
        if (player.battleActions != null)
        {
            foreach (var action in player.battleActions)
            {
                if (action == null || action.effects == null) continue;
                
                foreach (var effect in action.effects)
                {
                    // Modificador geral de poder (ataque)
                    if (modifiers.playerActionPowerModifier != 0 && effect.effectType == ActionType.Attack)
                    {
                        effect.power = Mathf.Max(1, effect.power + modifiers.playerActionPowerModifier);
                    }
                    
                    // NOVO: Modificador específico de ataques ofensivos
                    if (modifiers.playerOffensiveActionPowerModifier != 0 && effect.effectType == ActionType.Attack)
                    {
                        effect.power = Mathf.Max(1, effect.power + modifiers.playerOffensiveActionPowerModifier);
                    }
                    
                    // NOVO: Modificador específico de habilidades defensivas (cura/buff)
                    if (modifiers.playerDefensiveActionPowerModifier != 0)
                    {
                        if (effect.effectType == ActionType.Heal || effect.effectType == ActionType.Buff)
                        {
                            effect.power = Mathf.Max(1, effect.power + modifiers.playerDefensiveActionPowerModifier);
                        }
                    }
                    
                    // NOVO: Modificador específico de AOE
                    if (modifiers.playerAOEActionPowerModifier != 0 && IsAOEAction(action))
                    {
                        if (effect.effectType == ActionType.Attack)
                        {
                            effect.power = Mathf.Max(1, effect.power + modifiers.playerAOEActionPowerModifier);
                        }
                    }
                    
                    // NOVO: Modificador específico de single-target
                    if (modifiers.playerSingleTargetActionPowerModifier != 0 && IsSingleTargetAction(action))
                    {
                        if (effect.effectType == ActionType.Attack)
                        {
                            effect.power = Mathf.Max(1, effect.power + modifiers.playerSingleTargetActionPowerModifier);
                        }
                    }
                }
                
                // Modificador de custo de mana
                if (modifiers.playerActionManaCostModifier != 0)
                {
                    action.manaCost = Mathf.Max(0, action.manaCost + modifiers.playerActionManaCostModifier);
                }
            }
        }
        
        DebugLog($"✅ {player.characterName} modificado - HP: {player.maxHp}, Speed: {player.speed}");
    }
    
    /// <summary>
    /// NOVO: Verifica se uma ação é AOE
    /// </summary>
    private bool IsAOEAction(BattleAction action)
    {
        return action.targetType == TargetType.AllEnemies || 
               action.targetType == TargetType.AllAllies || 
               action.targetType == TargetType.Everyone;
    }
    
    /// <summary>
    /// NOVO: Verifica se uma ação é single-target
    /// </summary>
    private bool IsSingleTargetAction(BattleAction action)
    {
        return action.targetType == TargetType.SingleEnemy || 
               action.targetType == TargetType.SingleAlly || 
               action.targetType == TargetType.Self;
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
    
    [ContextMenu("Reset All Modifiers")]
    public void ResetModifiers()
    {
        modifiers.Reset();
        SaveModifiers();
        DebugLog("✅ Todos os modificadores foram resetados!");
    }
    
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