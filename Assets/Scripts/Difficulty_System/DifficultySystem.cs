using UnityEngine;
using System.IO;

/// <summary>
/// Sistema que aplica modificações imediatamente nos ScriptableObjects
/// </summary>
public class DifficultySystem : MonoBehaviour
{
    public static DifficultySystem Instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private bool saveModifiers = true;
    [SerializeField] private string saveFileName = "difficulty_modifiers.json";
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Historical Registry (Read-Only)")]
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
    
    public void ApplyNegotiation(CardAttribute playerAttr, CardAttribute enemyAttr, int playerValue, int enemyValue)
    {
        DebugLog($"=== APLICANDO NEGOCIAÇÃO ===");
        DebugLog($"Jogador: {playerAttr} {FormatValue(playerValue)}");
        DebugLog($"Inimigo: {enemyAttr} {FormatValue(enemyValue)}");
        
        if (playerAttr != CardAttribute.PlayerMaxHP || playerValue != 0)
        {
            ApplyModifierImmediate(playerAttr, playerValue, true);
            modifiers.RecordModifier(playerAttr, playerValue);
        }
        
        if (enemyAttr != CardAttribute.EnemyMaxHP || enemyValue != 0)
        {
            ApplyModifierImmediate(enemyAttr, enemyValue, false);
            modifiers.RecordModifier(enemyAttr, enemyValue);
        }
        
        SaveModifiers();
        DebugLog("\n" + modifiers.GetSummary());
    }
    
    private void ApplyModifierImmediate(CardAttribute attribute, int value, bool isPlayerAttribute)
    {
        if (GameManager.Instance == null)
        {
            DebugLog("GameManager não encontrado!");
            return;
        }
        
        switch (attribute)
        {
            case CardAttribute.PlayerMaxHP:
                if (GameManager.Instance.PlayerCharacterInfo != null)
                {
                    GameManager.Instance.PlayerCharacterInfo.maxHp = 
                        Mathf.Max(1, GameManager.Instance.PlayerCharacterInfo.maxHp + value);
                    DebugLog($"Player MaxHP modificado: {FormatValue(value)}");
                }
                break;
            
            case CardAttribute.PlayerMaxMP:
                if (GameManager.Instance.PlayerCharacterInfo != null)
                {
                    GameManager.Instance.PlayerCharacterInfo.maxMp = 
                        Mathf.Max(0, GameManager.Instance.PlayerCharacterInfo.maxMp + value);
                    DebugLog($"Player MaxMP modificado: {FormatValue(value)}");
                }
                break;
            
            case CardAttribute.PlayerDefense:
                if (GameManager.Instance.PlayerCharacterInfo != null)
                {
                    GameManager.Instance.PlayerCharacterInfo.defense = 
                        Mathf.Max(0, GameManager.Instance.PlayerCharacterInfo.defense + value);
                    DebugLog($"Player Defense modificada: {FormatValue(value)}");
                }
                break;
            
            case CardAttribute.PlayerSpeed:
                if (GameManager.Instance.PlayerCharacterInfo != null)
                {
                    GameManager.Instance.PlayerCharacterInfo.speed = 
                        Mathf.Max(0.1f, GameManager.Instance.PlayerCharacterInfo.speed + value);
                    DebugLog($"Player Speed modificada: {FormatValue(value)}");
                }
                break;
            
            case CardAttribute.PlayerActionPower:
                ApplyPlayerActionModifier(ActionType.Attack, value, null);
                DebugLog($"Player Action Power modificado: {FormatValue(value)}");
                break;
            
            case CardAttribute.PlayerActionManaCost:
                ApplyPlayerManaCostModifier(value);
                DebugLog($"Player Mana Cost modificado: {FormatValue(value)}");
                break;
            
            case CardAttribute.PlayerOffensiveActionPower:
                ApplyPlayerActionModifier(ActionType.Attack, value, null);
                DebugLog($"Player Offensive Power modificado: {FormatValue(value)}");
                break;
            
            case CardAttribute.PlayerDefensiveActionPower:
                ApplyPlayerActionModifier(ActionType.Heal, value, null);
                ApplyPlayerActionModifier(ActionType.Buff, value, null);
                DebugLog($"Player Defensive Power modificado: {FormatValue(value)}");
                break;
            
            case CardAttribute.PlayerAOEActionPower:
                ApplyPlayerActionModifier(ActionType.Attack, value, TargetType.AllEnemies);
                DebugLog($"Player AOE Power modificado: {FormatValue(value)}");
                break;
            
            case CardAttribute.PlayerSingleTargetActionPower:
                ApplyPlayerActionModifier(ActionType.Attack, value, TargetType.SingleEnemy);
                DebugLog($"Player Single-Target Power modificado: {FormatValue(value)}");
                break;
            
            case CardAttribute.CoinsEarned:
            case CardAttribute.ShopPrices:
                DebugLog($"{attribute} registrado (aplicado em runtime)");
                break;
            
            case CardAttribute.EnemyMaxHP:
            case CardAttribute.EnemyMaxMP:
            case CardAttribute.EnemyDefense:
            case CardAttribute.EnemySpeed:
            case CardAttribute.EnemyActionPower:
            case CardAttribute.EnemyActionManaCost:
            case CardAttribute.EnemyOffensiveActionPower:
            case CardAttribute.EnemyAOEActionPower:
                DebugLog($"{attribute} registrado (aplicado ao spawnar inimigos)");
                break;
        }
    }
    
    private void ApplyPlayerActionModifier(ActionType actionType, int powerChange, TargetType? targetFilter)
    {
        if (GameManager.Instance?.PlayerBattleActions == null) return;
        
        foreach (var action in GameManager.Instance.PlayerBattleActions)
        {
            if (action == null || action.effects == null) continue;
            
            if (targetFilter.HasValue && action.targetType != targetFilter.Value)
                continue;
            
            foreach (var effect in action.effects)
            {
                if (effect.effectType == actionType)
                {
                    int oldPower = effect.power;
                    effect.power = Mathf.Max(1, effect.power + powerChange);
                    
                    DebugLog($"  '{action.actionName}' poder: {oldPower} → {effect.power}");
                }
            }
        }
    }
    
    private void ApplyPlayerManaCostModifier(int costChange)
    {
        if (GameManager.Instance?.PlayerBattleActions == null) return;
        
        foreach (var action in GameManager.Instance.PlayerBattleActions)
        {
            if (action == null) continue;
            
            int oldCost = action.manaCost;
            action.manaCost = Mathf.Max(0, action.manaCost + costChange);
            
            if (oldCost != action.manaCost)
                DebugLog($"  '{action.actionName}' custo: {oldCost} → {action.manaCost} MP");
        }
    }
    
    public void ApplyToEnemy(Character enemy, bool applyStats, bool applyActions)
    {
        if (enemy == null || enemy.team != Team.Enemy) return;
        
        if (applyStats)
        {
            DebugLog($"Aplicando modificadores ao inimigo: {enemy.characterName}");
            
            enemy.maxHp = Mathf.Max(1, enemy.maxHp + modifiers.enemyMaxHPModifier);
            enemy.maxMp = Mathf.Max(0, enemy.maxMp + modifiers.enemyMaxMPModifier);
            enemy.defense = Mathf.Max(0, enemy.defense + modifiers.enemyDefenseModifier);
            enemy.speed = Mathf.Max(0.1f, enemy.speed + modifiers.enemySpeedModifier);
        }

        if (applyActions)
        {
            if (enemy.battleActions != null)
            {
                foreach (var action in enemy.battleActions)
                {
                    if (action == null || action.effects == null) continue;

                    foreach (var effect in action.effects)
                    {
                        if (modifiers.enemyActionPowerModifier != 0 && effect.effectType == ActionType.Attack)
                            effect.power = Mathf.Max(1, effect.power + modifiers.enemyActionPowerModifier);

                        if (modifiers.enemyOffensiveActionPowerModifier != 0 && effect.effectType == ActionType.Attack)
                            effect.power = Mathf.Max(1, effect.power + modifiers.enemyOffensiveActionPowerModifier);

                        if (modifiers.enemyAOEActionPowerModifier != 0 && IsAOEAction(action))
                        {
                            if (effect.effectType == ActionType.Attack)
                                effect.power = Mathf.Max(1, effect.power + modifiers.enemyAOEActionPowerModifier);
                        }
                    }

                    if (modifiers.enemyActionManaCostModifier != 0)
                        action.manaCost = Mathf.Max(0, action.manaCost + modifiers.enemyActionManaCostModifier);
                }
            }
        }

        DebugLog($"{enemy.characterName} modificado");
    }
    
    public void ApplyToEnemy_Stats(Character enemy)
    {
        ApplyToEnemy(enemy, true, false); 
    }
    
    public void ApplyToEnemy_Actions(Character enemy)
    {
        ApplyToEnemy(enemy, false, true);
    }
    
    private bool IsAOEAction(BattleAction action)
    {
        return action.targetType == TargetType.AllEnemies || 
               action.targetType == TargetType.AllAllies || 
               action.targetType == TargetType.Everyone;
    }
    
    public int GetModifiedCoins(int baseCoins)
    {
        return Mathf.Max(0, baseCoins + modifiers.coinsEarnedModifier);
    }
    
    public int GetModifiedShopPrice(int basePrice)
    {
        return Mathf.Max(1, basePrice + modifiers.shopPricesModifier);
    }
    
    [ContextMenu("Reset All Modifiers")]
    public void ResetModifiers()
    {
        modifiers.Reset();
        SaveModifiers();
        DebugLog("Todos os modificadores foram resetados!");
    }
    
    private void SaveModifiers()
    {
        if (!saveModifiers) return;
        
        try
        {
            string dataPath = Path.Combine(Application.persistentDataPath, saveFileName);
            string jsonData = JsonUtility.ToJson(modifiers, true);
            File.WriteAllText(dataPath, jsonData);
            
            DebugLog($"Registro salvo em: {dataPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao salvar registro: {e.Message}");
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
                    modifiers = new DifficultyModifiers();
                
                DebugLog($"Registro carregado!");
                DebugLog(modifiers.GetSummary());
            }
            else
            {
                modifiers = new DifficultyModifiers();
                DebugLog("Nenhum registro salvo - iniciando limpo");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao carregar registro: {e.Message}");
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
            Debug.Log($"<color=orange>[DifficultySystem]</color> {message}");
    }
    
    [ContextMenu("Show Current Modifiers")]
    private void ShowCurrentModifiers()
    {
        Debug.Log(modifiers.GetSummary());
    }
}