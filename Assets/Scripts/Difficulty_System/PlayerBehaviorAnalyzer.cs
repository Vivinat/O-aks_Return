// Assets/Scripts/Analytics/PlayerBehaviorAnalyzer.cs (FIXED)

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class PlayerBehaviorAnalyzer : MonoBehaviour
{
    public static PlayerBehaviorAnalyzer Instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private bool enableLogging = true;
    [SerializeField] private bool saveToFile = true;
    [SerializeField] private string saveFileName = "player_behavior.json";
    
    private PlayerBehaviorProfile playerProfile = new PlayerBehaviorProfile();
    
    private bool isBattleActive = false;
    private BattleManager currentBattleManager;
    private List<BattleEntity> playerTeamAtStart = new List<BattleEntity>();
    private List<BattleEntity> enemyTeamAtStart = new List<BattleEntity>();
    
    private List<BattleAction> lastShopItems = new List<BattleAction>();
    private bool playerBoughtSomething = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPlayerProfile();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SavePlayerProfile();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SavePlayerProfile();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) SavePlayerProfile();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isBattleActive)
        {
            FinalizeBattleAnalysis();
        }
        
        if (scene.name.ToLower().Contains("battle"))
        {
            StartBattleMonitoring();
        }
        else if (scene.name.ToLower().Contains("shop"))
        {
            StartShopMonitoring();
        }
        else if (scene.name.ToLower().Contains("map"))
        {
            AnalyzeMapBehavior();
        }
    }

    #region Battle Monitoring

    private void StartBattleMonitoring()
    {
        Log("Iniciando monitoramento de batalha");
        
        isBattleActive = true;
        playerProfile.currentBattle.Reset();
        
        currentBattleManager = FindObjectOfType<BattleManager>();
        if (currentBattleManager == null)
        {
            Log("BattleManager não encontrado!");
            return;
        }
        
        Invoke(nameof(CaptureBattleStartState), 0.1f);
    }

    private void CaptureBattleStartState()
    {
        if (currentBattleManager == null) return;
        
        playerTeamAtStart.Clear();
        enemyTeamAtStart.Clear();
        
        if (currentBattleManager.playerTeam != null)
        {
            playerTeamAtStart.AddRange(currentBattleManager.playerTeam);
        }
        
        if (currentBattleManager.enemyTeam != null)
        {
            enemyTeamAtStart.AddRange(currentBattleManager.enemyTeam);
        }
        
        var player = playerTeamAtStart.FirstOrDefault();
        if (player != null)
        {
            playerProfile.currentBattle.startingHP = player.GetCurrentHP();
            playerProfile.currentBattle.startingMP = player.GetCurrentMP();
        }
        
        foreach (var enemy in enemyTeamAtStart)
        {
            playerProfile.currentBattle.enemiesInBattle.Add(enemy.characterData.characterName);
        }
        
        if (GameManager.Instance != null && GameManager.Instance.PlayerBattleActions != null)
        {
            foreach (var action in GameManager.Instance.PlayerBattleActions)
            {
                if (action != null)
                {
                    playerProfile.currentBattle.unusedSkills.Add(action.actionName);
                }
            }
        }
        
        Log($"Estado inicial capturado - HP: {playerProfile.currentBattle.startingHP}, MP: {playerProfile.currentBattle.startingMP}");
    }

    public void RecordPlayerSkillUsage(BattleAction skill, BattleEntity user)
    {
        if (!isBattleActive || skill == null) return;
        
        playerProfile.currentBattle.RecordSkillUsage(skill.actionName);
        playerProfile.currentBattle.unusedSkills.Remove(skill.actionName);
        
        Log($"Skill usada: {skill.actionName}");
    }

    public void RecordPlayerDamageReceived(BattleEntity attacker, int damage)
    {
        if (!isBattleActive || attacker == null) return;
        
        playerProfile.currentBattle.RecordEnemyDamage(attacker.characterData.characterName, damage);
        
        Log($"Dano recebido de {attacker.characterData.characterName}: {damage}");
    }

    public void RecordPlayerDeath()
    {
        if (!isBattleActive) return;
        
        playerProfile.currentBattle.playerDied = true;
        Log("Morte do jogador registrada");
    }

    private void FinalizeBattleAnalysis()
    {
        if (!isBattleActive) return;
    
        var player = playerTeamAtStart.FirstOrDefault();
        if (player != null)
        {
            playerProfile.currentBattle.endingHP = player.GetCurrentHP();
            playerProfile.currentBattle.endingMP = player.GetCurrentMP();
        
            Debug.Log($"=== BATTLE END DEBUG ===");
            Debug.Log($"Player Starting HP/MP: {playerProfile.currentBattle.startingHP}/{playerProfile.currentBattle.startingMP}");
            Debug.Log($"Player Ending HP/MP: {playerProfile.currentBattle.endingHP}/{playerProfile.currentBattle.endingMP}");
            Debug.Log($"Player isDead: {player.isDead}");
            Debug.Log($"Total damage recorded: {playerProfile.currentBattle.enemyDamageDealt.Values.Sum()}");
            Debug.Log($"=========================");
        }
        
        AnalyzeBattlePatterns();
        
        isBattleActive = false;
        currentBattleManager = null;
        playerTeamAtStart.Clear();
        enemyTeamAtStart.Clear();
    }

    private void AnalyzeBattlePatterns()
    {
        string currentMap = SceneManager.GetActiveScene().name;
        var battleData = playerProfile.currentBattle;
        
        if (battleData.playerDied)
        {
            string killerEnemy = battleData.GetMostDamagingEnemy();
            
            var observation = new BehaviorObservation(BehaviorTriggerType.PlayerDeath, currentMap);
            observation.SetData("killerEnemy", killerEnemy);
            observation.SetData("totalDamageReceived", battleData.enemyDamageDealt.Values.Sum());
            
            playerProfile.AddObservation(observation);
            
            CheckRepeatedBossDeath(killerEnemy);
        }
        
        CheckSkillOveruse(currentMap);
        CheckLowHealthNoCure(currentMap);
        CheckNoDamageReceived(currentMap);
        CheckCriticalHealth(currentMap);
        CheckExhaustedItems(currentMap);
        CheckUnusedSkills(currentMap);
        CheckDefensiveSkills(currentMap);
        CheckEasyBossVictory(currentMap);
        CheckManaIssues(currentMap);
    }

    private void CheckSkillOveruse(string mapName)
    {
        foreach (var skill in playerProfile.currentBattle.skillUsageCount)
        {
            if (playerProfile.currentBattle.IsSkillOverused(skill.Key, 0.5f))
            {
                var observation = new BehaviorObservation(BehaviorTriggerType.SkillOveruse, mapName);
                observation.SetData("skillName", skill.Key);
                observation.SetData("usagePercentage", (float)skill.Value / playerProfile.currentBattle.totalActionsUsed);
                
                playerProfile.AddObservation(observation);
            }
        }
    }

    private void CheckLowHealthNoCure(string mapName)
    {
        float healthPercentage = (float)playerProfile.currentBattle.endingHP / playerProfile.currentBattle.startingHP;
        
        if (healthPercentage < 0.5f)
        {
            // FIXED: Use GetPrimaryActionType() instead of .type
            bool hasCureItems = GameManager.Instance.PlayerBattleActions?.Any(action => 
                action != null && action.GetPrimaryActionType() == ActionType.Heal && action.isConsumable && action.CanUse()) ?? false;
            
            if (!hasCureItems)
            {
                var observation = new BehaviorObservation(BehaviorTriggerType.LowHealthNoCure, mapName);
                observation.SetData("healthPercentage", healthPercentage);
                observation.SetData("endingHP", playerProfile.currentBattle.endingHP);
                
                playerProfile.AddObservation(observation);
            }
        }
    }

    private void CheckNoDamageReceived(string mapName)
    {
        if (playerProfile.currentBattle.enemyDamageDealt.Count == 0 || 
            playerProfile.currentBattle.enemyDamageDealt.Values.Sum() == 0)
        {
            if (playerProfile.currentBattle.enemiesInBattle.Count > 0)
            {
                string randomEnemy = playerProfile.currentBattle.enemiesInBattle[
                    Random.Range(0, playerProfile.currentBattle.enemiesInBattle.Count)];
                
                var observation = new BehaviorObservation(BehaviorTriggerType.NoDamageReceived, mapName);
                observation.SetData("randomEnemy", randomEnemy);
                
                playerProfile.AddObservation(observation);
            }
        }
    }

    private void CheckCriticalHealth(string mapName)
    {
        float healthPercentage = (float)playerProfile.currentBattle.endingHP / playerProfile.currentBattle.startingHP;
        
        if (healthPercentage < 0.25f)
        {
            string mostDamagingEnemy = playerProfile.currentBattle.GetMostDamagingEnemy();
            
            var observation = new BehaviorObservation(BehaviorTriggerType.CriticalHealth, mapName);
            observation.SetData("mostDamagingEnemy", mostDamagingEnemy);
            observation.SetData("healthPercentage", healthPercentage);
            
            playerProfile.AddObservation(observation);
        }
    }

    private void CheckExhaustedItems(string mapName)
    {
        if (GameManager.Instance?.PlayerBattleActions != null)
        {
            foreach (var action in GameManager.Instance.PlayerBattleActions)
            {
                if (action != null && action.isConsumable && action.currentUses == 0)
                {
                    var observation = new BehaviorObservation(BehaviorTriggerType.ItemExhausted, mapName);
                    observation.SetData("exhaustedItem", action.actionName);
                    
                    playerProfile.AddObservation(observation);
                }
            }
        }
    }

    private void CheckUnusedSkills(string mapName)
    {
        foreach (string unusedSkill in playerProfile.currentBattle.unusedSkills)
        {
            var observation = new BehaviorObservation(BehaviorTriggerType.UnusedSkill, mapName);
            observation.SetData("unusedSkill", unusedSkill);
            
            playerProfile.AddObservation(observation);
        }
    }

    private void CheckDefensiveSkills(string mapName)
    {
        // FIXED: Use GetPrimaryActionType() instead of .type
        bool hasDefensiveSkills = GameManager.Instance?.PlayerBattleActions?.Any(action => 
            action != null && (action.GetPrimaryActionType() == ActionType.Heal || action.GetPrimaryActionType() == ActionType.Buff)) ?? false;
        
        if (!hasDefensiveSkills)
        {
            var observation = new BehaviorObservation(BehaviorTriggerType.NoDefensiveSkills, mapName);
            playerProfile.AddObservation(observation);
        }
    }

    private void CheckRepeatedBossDeath(string enemyName)
    {
        bool isBoss = enemyName.ToLower().Contains("boss") || 
                     SceneManager.GetActiveScene().name.ToLower().Contains("boss") ||
                     FindObjectOfType<BossNode>() != null;
        
        if (isBoss)
        {
            playerProfile.session.RecordBossDeath(enemyName);
            
            if (playerProfile.session.HasRepeatedBossDeath(enemyName))
            {
                var observation = new BehaviorObservation(BehaviorTriggerType.RepeatedBossDeath, SceneManager.GetActiveScene().name);
                observation.SetData("bossName", enemyName);
                
                playerProfile.AddObservation(observation);
            }
        }
    }

    private void CheckEasyBossVictory(string mapName)
    {
        bool isBoss = FindObjectOfType<BossNode>() != null || mapName.ToLower().Contains("boss");
        
        if (isBoss && !playerProfile.currentBattle.playerDied)
        {
            float healthPercentage = (float)playerProfile.currentBattle.endingHP / playerProfile.currentBattle.startingHP;
            bool usedNoItems = !playerProfile.currentBattle.skillUsageCount.Keys.Any(skill =>
                GameManager.Instance.PlayerBattleActions?.Any(action => 
                    action?.actionName == skill && action.isConsumable) ?? false);
            
            if (healthPercentage > 0.5f && usedNoItems)
            {
                string bossName = playerProfile.currentBattle.enemiesInBattle.FirstOrDefault() ?? "Unknown Boss";
                
                var observation = new BehaviorObservation(BehaviorTriggerType.BossEasyVictory, mapName);
                observation.SetData("bossName", bossName);
                observation.SetData("healthPercentage", healthPercentage);
                
                playerProfile.AddObservation(observation);
            }
        }
    }

    private void CheckManaIssues(string mapName)
    {
        float manaPercentage = (float)playerProfile.currentBattle.endingMP / playerProfile.currentBattle.startingMP;
        
        bool allSkillsUseMana = GameManager.Instance?.PlayerBattleActions?.All(action =>
            action == null || action.manaCost > 0) ?? false;
        
        if (allSkillsUseMana && manaPercentage < 0.5f)
        {
            var observation = new BehaviorObservation(BehaviorTriggerType.AllSkillsUseMana, mapName);
            observation.SetData("manaPercentage", manaPercentage);
            observation.SetData("playerSkills", GameManager.Instance.PlayerBattleActions?.Select(a => a?.actionName).ToList());
            
            playerProfile.AddObservation(observation);
        }
        
        if (manaPercentage < 0.1f)
        {
            playerProfile.session.consecutiveZeroManaBattles++;
            playerProfile.session.consecutiveLowManaBattles++;
        }
        else if (manaPercentage < 0.5f)
        {
            playerProfile.session.consecutiveLowManaBattles++;
            playerProfile.session.consecutiveZeroManaBattles = 0;
        }
        else
        {
            playerProfile.session.consecutiveLowManaBattles = 0;
            playerProfile.session.consecutiveZeroManaBattles = 0;
        }
        
        if (playerProfile.session.consecutiveLowManaBattles >= 2)
        {
            var observation = new BehaviorObservation(BehaviorTriggerType.LowManaStreak, mapName);
            observation.SetData("streakLength", playerProfile.session.consecutiveLowManaBattles);
            
            playerProfile.AddObservation(observation);
        }
        
        if (playerProfile.session.consecutiveZeroManaBattles >= 2)
        {
            var observation = new BehaviorObservation(BehaviorTriggerType.ZeroManaStreak, mapName);
            observation.SetData("streakLength", playerProfile.session.consecutiveZeroManaBattles);
            
            playerProfile.AddObservation(observation);
        }
    }

    #endregion

    #region Shop Monitoring

    private void StartShopMonitoring()
    {
        Log("Iniciando monitoramento de loja");
        
        lastShopItems.Clear();
        playerBoughtSomething = false;
        
        Invoke(nameof(CaptureShopItems), 0.5f);
    }

    private void CaptureShopItems()
    {
        var shopManager = FindObjectOfType<ShopManager>();
        if (shopManager == null) return;
    }

    public void RecordShopPurchase(BattleAction purchasedItem)
    {
        playerBoughtSomething = true;
        Log($"Compra registrada: {purchasedItem?.actionName}");
    }

    public void RecordShopExit(List<BattleAction> availableItems)
    {
        if (!playerBoughtSomething && availableItems != null && availableItems.Count > 0)
        {
            string mapName = SceneManager.GetActiveScene().name;
            
            var observation = new BehaviorObservation(BehaviorTriggerType.ShopIgnored, mapName);
            observation.SetData("ignoredItems", availableItems.Select(item => item?.actionName).ToList());
            observation.SetData("playerCoins", GameManager.Instance?.CurrencySystem?.CurrentCoins ?? 0);
            
            playerProfile.AddObservation(observation);
            
            Log($"Loja ignorada com {availableItems.Count} itens disponíveis");
        }
    }

    #endregion

    #region Map Analysis

    private void AnalyzeMapBehavior()
    {
        Log("Analisando comportamento no mapa");
        CheckLowCoinsWithShops();
    }

    private void CheckLowCoinsWithShops()
    {
        if (GameManager.Instance?.CurrencySystem == null) return;
        
        int currentCoins = GameManager.Instance.CurrencySystem.CurrentCoins;
        
        if (currentCoins < 50)
        {
            var mapNodes = FindObjectsOfType<MapNode>();
            bool hasUnvisitedShops = mapNodes.Any(node => 
                node.eventType != null && 
                node.eventType is ShopEventSO && 
                !node.IsCompleted());
            
            if (hasUnvisitedShops)
            {
                int suggestedCoins = Random.Range(20, 51);
                
                var observation = new BehaviorObservation(BehaviorTriggerType.LowCoinsUnvisitedShops, SceneManager.GetActiveScene().name);
                observation.SetData("currentCoins", currentCoins);
                observation.SetData("suggestedCoins", suggestedCoins);
                observation.SetData("unvisitedShopsCount", mapNodes.Count(n => n.eventType is ShopEventSO && !n.IsCompleted()));
                
                playerProfile.AddObservation(observation);
                
                Log($"Poucas moedas ({currentCoins}) com lojas disponíveis - sugerindo {suggestedCoins}");
            }
        }
    }

    #endregion

    #region Data Persistence

    private void SavePlayerProfile()
    {
        if (!saveToFile) return;
        
        try
        {
            string dataPath = Path.Combine(Application.persistentDataPath, saveFileName);
            string jsonData = JsonUtility.ToJson(playerProfile, true);
            File.WriteAllText(dataPath, jsonData);
            
            Log($"Profile salvo em: {dataPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao salvar profile: {e.Message}");
        }
    }

    private void LoadPlayerProfile()
    {
        if (!saveToFile) return;
        
        try
        {
            string dataPath = Path.Combine(Application.persistentDataPath, saveFileName);
            
            if (File.Exists(dataPath))
            {
                string jsonData = File.ReadAllText(dataPath);
                playerProfile = JsonUtility.FromJson<PlayerBehaviorProfile>(jsonData);
                
                if (playerProfile == null)
                {
                    playerProfile = new PlayerBehaviorProfile();
                }
                
                playerProfile.CleanOldObservations();
                
                Log($"Profile carregado: {playerProfile.observations.Count} observações");
            }
            else
            {
                playerProfile = new PlayerBehaviorProfile();
                Log("Novo profile criado");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao carregar profile: {e.Message}");
            playerProfile = new PlayerBehaviorProfile();
        }
    }

    #endregion

    #region Public API

    public List<BehaviorObservation> GetAllObservations()
    {
        return new List<BehaviorObservation>(playerProfile.observations);
    }

    public List<BehaviorObservation> GetObservationsByType(BehaviorTriggerType type)
    {
        return playerProfile.GetObservationsByType(type);
    }

    public List<BehaviorObservation> GetNegotiationTriggers(int maxResults = 5)
    {
        return playerProfile.observations
            .OrderByDescending(obs => obs.sessionCount)
            .ThenByDescending(obs => obs.timestamp)
            .Take(maxResults)
            .ToList();
    }

    public void ConsumeObservation(BehaviorObservation observation)
    {
        if (playerProfile.observations.Contains(observation))
        {
            playerProfile.observations.Remove(observation);
            SavePlayerProfile();
            Log($"Observação consumida: {observation.triggerType}");
        }
    }

    public void ConsumeObservationByType(BehaviorTriggerType type, string dataKey = null, object dataValue = null)
    {
        var toRemove = playerProfile.observations.FindAll(obs => 
        {
            if (obs.triggerType != type) return false;
            
            if (dataKey == null) return true;
            
            return obs.HasData(dataKey) && obs.GetData<object>(dataKey)?.Equals(dataValue) == true;
        });
        
        foreach (var obs in toRemove)
        {
            playerProfile.observations.Remove(obs);
        }
        
        if (toRemove.Count > 0)
        {
            SavePlayerProfile();
            Log($"Consumidas {toRemove.Count} observações do tipo {type}");
        }
    }
    
    public void RecordBattleEnd()
    {
        if (!isBattleActive) return;
    
        var player = playerTeamAtStart.FirstOrDefault();
        if (player != null && !player.isDead)
        {
            playerProfile.currentBattle.endingHP = player.GetCurrentHP();
            playerProfile.currentBattle.endingMP = player.GetCurrentMP();
        
            Debug.Log($"=== DADOS CORRETOS CAPTURADOS ===");
            Debug.Log($"HP Final: {playerProfile.currentBattle.endingHP}");
            Debug.Log($"MP Final: {playerProfile.currentBattle.endingMP}");
            Debug.Log($"================================");
        }
    }

    public void ConsumeObservations(List<BehaviorObservation> observationsToConsume)
    {
        int removedCount = 0;
        
        foreach (var obs in observationsToConsume)
        {
            if (playerProfile.observations.Remove(obs))
            {
                removedCount++;
            }
        }
        
        if (removedCount > 0)
        {
            SavePlayerProfile();
            Log($"Consumidas {removedCount} observações em negociação");
        }
    }

    public List<BehaviorObservation> GetUnresolvedNegotiationTriggers(int maxResults = 5)
    {
        return playerProfile.observations
            .Where(obs => !obs.GetData<bool>("resolved", false))
            .OrderByDescending(obs => obs.sessionCount)
            .ThenByDescending(obs => obs.timestamp)
            .Take(maxResults)
            .ToList();
    }

    public void MarkObservationAsResolved(BehaviorObservation observation)
    {
        if (playerProfile.observations.Contains(observation))
        {
            observation.SetData("resolved", true);
            observation.SetData("resolvedTimestamp", System.DateTime.Now.Ticks);
            SavePlayerProfile();
            Log($"Observação marcada como resolvida: {observation.triggerType}");
        }
    }

    public void ForceAnalysis()
    {
        if (isBattleActive)
        {
            AnalyzeBattlePatterns();
        }
        
        AnalyzeMapBehavior();
        SavePlayerProfile();
        
        Log("Análise manual executada");
    }

    public void ClearAllData()
    {
        playerProfile = new PlayerBehaviorProfile();
        SavePlayerProfile();
        Log("Todos os dados foram limpos");
    }

    public string GetSummaryStats()
    {
        var stats = new System.Text.StringBuilder();
        stats.AppendLine($"=== ESTATÍSTICAS DO JOGADOR ===");
        stats.AppendLine($"Total de observações: {playerProfile.observations.Count}");
        
        var typeGroups = playerProfile.observations.GroupBy(obs => obs.triggerType);
        foreach (var group in typeGroups)
        {
            stats.AppendLine($"{group.Key}: {group.Count()}");
        }
        
        stats.AppendLine($"Mortes em boss registradas: {playerProfile.session.bossDeathHistory.Count}");
        stats.AppendLine($"Batalhas consecutivas com MP baixo: {playerProfile.session.consecutiveLowManaBattles}");
        stats.AppendLine($"Batalhas consecutivas com MP zerado: {playerProfile.session.consecutiveZeroManaBattles}");
        
        return stats.ToString();
    }

    #endregion

    #region Utility

    private void Log(string message)
    {
        if (enableLogging)
        {
            Debug.Log($"[BehaviorAnalyzer] {message}");
        }
    }

    #endregion

    #region Debug

    [ContextMenu("Show Summary Stats")]
    private void ShowSummaryStats()
    {
        Debug.Log(GetSummaryStats());
    }

    [ContextMenu("Force Analysis")]
    private void ForceAnalysisMenu()
    {
        ForceAnalysis();
    }

    [ContextMenu("Clear All Data")]
    private void ClearAllDataMenu()
    {
        ClearAllData();
    }

    [ContextMenu("Save Profile")]
    private void SaveProfileMenu()
    {
        SavePlayerProfile();
    }

    #endregion
}