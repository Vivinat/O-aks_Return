// Assets/Scripts/Analytics/PlayerBehaviorAnalyzer.cs

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
            Log("BattleManager não encontrado");
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
        
        playerProfile.currentBattle.totalEnemiesInBattle = enemyTeamAtStart.Count;
        
        foreach (var enemy in enemyTeamAtStart)
        {
            playerProfile.currentBattle.enemiesInBattle.Add(enemy.characterData.characterName);
        
            if (enemy.characterData.maxHp > 100)
            {
                playerProfile.currentBattle.tankEnemiesCount++;
            }
        
            if (enemy.characterData.speed > 5f)
            {
                playerProfile.currentBattle.fastEnemiesCount++;
            }
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
    }

    public void RecordPlayerSkillUsage(BattleAction skill, BattleEntity user)
    {
        if (!isBattleActive || skill == null) return;
        
        playerProfile.currentBattle.RecordSkillUsage(skill.actionName);
        playerProfile.currentBattle.unusedSkills.Remove(skill.actionName);
    }

    public void RecordPlayerDamageReceived(BattleEntity attacker, int damage)
    {
        if (!isBattleActive || attacker == null) return;
        
        playerProfile.currentBattle.RecordEnemyDamage(attacker.characterData.characterName, damage);
    }

    public void RecordPlayerDeath()
    {
        if (!isBattleActive) return;
        
        playerProfile.currentBattle.playerDied = true;
    }

    private void FinalizeBattleAnalysis()
    {
        if (!isBattleActive) return;
    
        var player = playerTeamAtStart.FirstOrDefault();
        if (player != null)
        {
            playerProfile.currentBattle.endingHP = player.GetCurrentHP();
            playerProfile.currentBattle.endingMP = player.GetCurrentMP();
            
            if (player.isDead && currentBattleManager != null)
            {
                playerProfile.currentBattle.turnOfDeath = currentBattleManager.GetCurrentTurn();
            }
        }
        
        if (!player.isDead)
        {
            float hpPercentage = (float)playerProfile.currentBattle.endingHP / playerProfile.currentBattle.startingHP;
            playerProfile.session.RecordBattleEndHP(hpPercentage);
            
            playerProfile.session.totalVictories++;
            
            bool usedConsumables = playerProfile.currentBattle.skillUsageCount.Keys.Any(skill =>
                GameManager.Instance.PlayerBattleActions?.Any(action => 
                    action?.actionName == skill && action.isConsumable) ?? false);
            
            if (usedConsumables)
            {
                playerProfile.session.victoriesWithConsumables++;
            }
        }
    
        foreach (string unusedSkill in playerProfile.currentBattle.unusedSkills)
        {
            playerProfile.session.RecordSkillNotUsed(unusedSkill);
        }
    
        foreach (string usedSkill in playerProfile.currentBattle.skillUsageCount.Keys)
        {
            playerProfile.session.ResetSkillUsageCounter(usedSkill);
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
    
        CheckLowHealthNoCure(currentMap);
        CheckNoDamageReceived(currentMap);
        CheckExhaustedItems(currentMap);
        CheckDefensiveSkills(currentMap);
        CheckEasyBattleVictory(currentMap);
        CheckManaIssues(currentMap);
        CheckSingleSkillCarry(currentMap);     
        CheckFrequentLowHP(currentMap);         
        CheckWeakSkillIgnored(currentMap);      
        CheckAlwaysOutsped(currentMap);
        CheckAlwaysFirstTurn(currentMap);
        CheckStrugglesAgainstTanks(currentMap);
        CheckStrugglesAgainstFast(currentMap);
        CheckStrugglesAgainstSwarms(currentMap);
        CheckAlwaysDiesEarly(currentMap);
        CheckAlwaysDiesLate(currentMap);
        CheckDeathByChipDamage(currentMap);
        CheckOneHitKOVulnerable(currentMap);
        CheckExpensiveSkillsOnly(currentMap);
        CheckNoAOEDamage(currentMap);
        CheckBrokeAfterShopping(currentMap);
        CheckRanOutOfConsumables(currentMap);
        CheckConsumableDependency(currentMap);
    }
    
    public void RecordPlayerSkillDamage(BattleAction skill, int damage)
    {
        if (!isBattleActive || skill == null) return;
    
        playerProfile.currentBattle.RecordSkillDamage(skill.actionName, damage);
    }

    public void RecordTurnAction(BattleEntity actor)
    {
        if (!isBattleActive || actor == null) return;
    
        string actorName = actor.characterData.team == Team.Player ? "Player" : actor.characterData.characterName;
        playerProfile.currentBattle.RecordTurnOrder(actorName);
    }

    public void RecordPlayerHitReceived(int damage)
    {
        if (!isBattleActive) return;
    
        playerProfile.currentBattle.RecordHitReceived(damage);
    }

    private void CheckLowHealthNoCure(string mapName)
    {
        float healthPercentage = (float)playerProfile.currentBattle.endingHP / playerProfile.currentBattle.startingHP;
        
        if (healthPercentage < 0.5f)
        {
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

    private void CheckDefensiveSkills(string mapName)
    {
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

    private void CheckEasyBattleVictory(string mapName)
    {
        if (!playerProfile.currentBattle.playerDied)
        {
            float healthPercentage = (float)playerProfile.currentBattle.endingHP / playerProfile.currentBattle.startingHP;
            bool usedNoItems = !playerProfile.currentBattle.skillUsageCount.Keys.Any(skill =>
                GameManager.Instance.PlayerBattleActions?.Any(action => 
                    action?.actionName == skill && action.isConsumable) ?? false);
            
            if (healthPercentage > 0.5f && usedNoItems)
            {
                string enemiesNames = string.Join(", ", playerProfile.currentBattle.enemiesInBattle);
                if (string.IsNullOrEmpty(enemiesNames))
                {
                    enemiesNames = "Unknown Enemy";
                }
                
                bool isBoss = FindObjectOfType<BossNode>() != null || mapName.ToLower().Contains("boss");
                
                var observation = new BehaviorObservation(BehaviorTriggerType.BattleEasyVictory, mapName);
                observation.SetData("enemyNames", enemiesNames);
                observation.SetData("healthPercentage", healthPercentage);
                observation.SetData("isBoss", isBoss);
                observation.SetData("enemyCount", playerProfile.currentBattle.enemiesInBattle.Count);
                
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
    
    #region Battle Analysis - Eventos Melhorados

    private void CheckSingleSkillCarry(string mapName)
    {
        var battleData = playerProfile.currentBattle;
        
        if (battleData.skillDamageDealt.Count == 0) return;
        
        string topDamageSkill = battleData.GetHighestDamageSkill();
        if (string.IsNullOrEmpty(topDamageSkill)) return;
        
        float damagePercentage = battleData.GetSkillDamagePercentage(topDamageSkill);
        
        if (damagePercentage > 0.6f)
        {
            var observation = new BehaviorObservation(BehaviorTriggerType.SingleSkillCarry, mapName);
            observation.SetData("skillName", topDamageSkill);
            observation.SetData("damagePercentage", damagePercentage);
            observation.SetData("totalDamage", battleData.skillDamageDealt[topDamageSkill]);
            
            playerProfile.AddObservation(observation);
        }
    }

    private void CheckFrequentLowHP(string mapName)
    {
        if (playerProfile.session.HasFrequentLowHPPattern(0.3f, 3))
        {
            float averageHP = playerProfile.session.recentBattleEndHPPercentages.Average();
            
            var observation = new BehaviorObservation(BehaviorTriggerType.FrequentLowHP, mapName);
            observation.SetData("averageEndingHP", averageHP);
            observation.SetData("battleCount", playerProfile.session.recentBattleEndHPPercentages.Count);
            
            playerProfile.AddObservation(observation);
        }
    }

    private void CheckWeakSkillIgnored(string mapName)
    {
        foreach (var skillEntry in playerProfile.session.skillNeverUsedCount)
        {
            if (playerProfile.session.IsSkillChronicallyIgnored(skillEntry.Key, 5))
            {
                var observation = new BehaviorObservation(BehaviorTriggerType.WeakSkillIgnored, mapName);
                observation.SetData("skillName", skillEntry.Key);
                observation.SetData("battlesIgnored", skillEntry.Value);
                
                playerProfile.AddObservation(observation);
            }
        }
    }

    #endregion

    #region Battle Analysis - Velocidade/ATB

    private void CheckAlwaysOutsped(string mapName)
    {
        var battleData = playerProfile.currentBattle;
        
        if (battleData.turnOrder.Count < 5) return;
        
        float playerFirstPercentage = battleData.GetPlayerFirstTurnPercentage();
        
        if (playerFirstPercentage < 0.2f)
        {
            var observation = new BehaviorObservation(BehaviorTriggerType.AlwaysOutsped, mapName);
            observation.SetData("playerFirstPercentage", playerFirstPercentage);
            observation.SetData("enemyNames", string.Join(", ", battleData.enemiesInBattle));
            
            playerProfile.AddObservation(observation);
        }
    }

    private void CheckAlwaysFirstTurn(string mapName)
    {
        var battleData = playerProfile.currentBattle;
        
        if (battleData.turnOrder.Count < 5) return;
        
        float playerFirstPercentage = battleData.GetPlayerFirstTurnPercentage();
        
        if (playerFirstPercentage > 0.8f)
        {
            var observation = new BehaviorObservation(BehaviorTriggerType.AlwaysFirstTurn, mapName);
            observation.SetData("playerFirstPercentage", playerFirstPercentage);
            observation.SetData("enemyNames", string.Join(", ", battleData.enemiesInBattle));
            
            playerProfile.AddObservation(observation);
        }
    }

    #endregion

    #region Battle Analysis - Tipo de Inimigo

    private void CheckStrugglesAgainstTanks(string mapName)
    {
        var battleData = playerProfile.currentBattle;
        
        if (battleData.tankEnemiesCount == 0) return;
        
        bool diedToTank = battleData.playerDied;
        bool longBattle = battleData.turnOrder.Count > 40;
        
        if (diedToTank || longBattle)
        {
            var observation = new BehaviorObservation(BehaviorTriggerType.StrugglesAgainstTanks, mapName);
            observation.SetData("tankCount", battleData.tankEnemiesCount);
            observation.SetData("died", diedToTank);
            observation.SetData("turnCount", battleData.turnOrder.Count / 4);
            
            playerProfile.AddObservation(observation);
        }
    }

    private void CheckStrugglesAgainstFast(string mapName)
    {
        var battleData = playerProfile.currentBattle;
        
        if (battleData.fastEnemiesCount == 0) return;
        
        if (battleData.playerDied)
        {
            var observation = new BehaviorObservation(BehaviorTriggerType.StrugglesAgainstFast, mapName);
            observation.SetData("fastCount", battleData.fastEnemiesCount);
            observation.SetData("totalDamage", battleData.enemyDamageDealt.Values.Sum());
            
            playerProfile.AddObservation(observation);
        }
    }

    private void CheckStrugglesAgainstSwarms(string mapName)
    {
        var battleData = playerProfile.currentBattle;
        
        if (battleData.totalEnemiesInBattle < 3) return;
        
        bool died = battleData.playerDied;
        float hpPercentage = battleData.startingHP > 0 ? 
            (float)battleData.endingHP / battleData.startingHP : 1f;
        bool lowHP = hpPercentage < 0.3f;
        
        if (died || lowHP)
        {
            var observation = new BehaviorObservation(BehaviorTriggerType.StrugglesAgainstSwarms, mapName);
            observation.SetData("enemyCount", battleData.totalEnemiesInBattle);
            observation.SetData("died", died);
            observation.SetData("endingHPPercentage", hpPercentage);
            
            playerProfile.AddObservation(observation);
        }
    }

    #endregion
    
    #region Battle Analysis - Padrões de Morte

    private void CheckAlwaysDiesEarly(string mapName)
    {
        var battleData = playerProfile.currentBattle;
        
        if (!battleData.playerDied) return;
        
        if (battleData.turnOfDeath > 0 && battleData.turnOfDeath <= 4)
        {
            var observation = new BehaviorObservation(BehaviorTriggerType.AlwaysDiesEarly, mapName);
            observation.SetData("turnOfDeath", battleData.turnOfDeath);
            observation.SetData("killerEnemy", battleData.GetMostDamagingEnemy());
            observation.SetData("totalDamageReceived", battleData.enemyDamageDealt.Values.Sum());
            
            playerProfile.AddObservation(observation);
        }
    }

    private void CheckAlwaysDiesLate(string mapName)
    {
        var battleData = playerProfile.currentBattle;
        
        if (!battleData.playerDied) return;
        
        if (battleData.turnOfDeath > 10)
        {
            var observation = new BehaviorObservation(BehaviorTriggerType.AlwaysDiesLate, mapName);
            observation.SetData("turnOfDeath", battleData.turnOfDeath);
            observation.SetData("killerEnemy", battleData.GetMostDamagingEnemy());
            observation.SetData("totalDamageReceived", battleData.enemyDamageDealt.Values.Sum());
            
            playerProfile.AddObservation(observation);
        }
    }

    private void CheckDeathByChipDamage(string mapName)
    {
        var battleData = playerProfile.currentBattle;
        
        if (!battleData.playerDied) return;
        if (battleData.hitsReceived.Count < 5) return;
        
        float averageHitSize = battleData.GetAverageHitSize();
        int totalHits = battleData.hitsReceived.Count;
        
        if (averageHitSize < 20f && totalHits >= 8)
        {
            var observation = new BehaviorObservation(BehaviorTriggerType.DeathByChipDamage, mapName);
            observation.SetData("averageHitSize", averageHitSize);
            observation.SetData("totalHits", totalHits);
            observation.SetData("totalDamageReceived", battleData.enemyDamageDealt.Values.Sum());
            
            playerProfile.AddObservation(observation);
        }
    }

    #endregion

    #region Battle Analysis - One-Shot

    private void CheckOneHitKOVulnerable(string mapName)
    {
        var battleData = playerProfile.currentBattle;
        
        if (battleData.hitsReceived.Count == 0) return;
        
        int biggestHit = 0;
        foreach (int hit in battleData.hitsReceived)
        {
            if (hit > biggestHit)
            {
                biggestHit = hit;
            }
        }
        
        float hitPercentage = (float)biggestHit / battleData.startingHP;
        
        if (hitPercentage > 0.4f)
        {
            var observation = new BehaviorObservation(BehaviorTriggerType.OneHitKOVulnerable, mapName);
            observation.SetData("biggestHit", biggestHit);
            observation.SetData("hitPercentage", hitPercentage);
            observation.SetData("oneShotter", battleData.GetMostDamagingEnemy());
            observation.SetData("startingHP", battleData.startingHP);
            
            playerProfile.AddObservation(observation);
        }
    }

    #endregion

    #region Battle Analysis - Build

    private void CheckExpensiveSkillsOnly(string mapName)
    {
        if (GameManager.Instance?.PlayerBattleActions == null) return;
        
        var actions = GameManager.Instance.PlayerBattleActions;
        var skills = actions.Where(a => a != null && !a.isConsumable).ToList();
        
        if (skills.Count == 0) return;
        
        float averageManaCost = (float)skills.Average(s => s.manaCost);
        bool allExpensive = skills.All(s => s.manaCost >= 20);
        
        if (allExpensive && averageManaCost >= 20f)
        {
            var observation = new BehaviorObservation(BehaviorTriggerType.ExpensiveSkillsOnly, mapName);
            observation.SetData("averageManaCost", averageManaCost);
            observation.SetData("skillCount", skills.Count);
            observation.SetData("skillNames", skills.Select(s => s.actionName).ToList());
            
            playerProfile.AddObservation(observation);
        }
    }

    private void CheckNoAOEDamage(string mapName)
    {
        if (GameManager.Instance?.PlayerBattleActions == null) return;
        
        var actions = GameManager.Instance.PlayerBattleActions;
        
        bool hasAOE = actions.Any(a => 
            a != null && 
            (a.targetType == TargetType.AllEnemies || a.targetType == TargetType.Everyone) &&
            a.GetPrimaryActionType() == ActionType.Attack);
        
        if (!hasAOE && playerProfile.currentBattle.totalEnemiesInBattle >= 2)
        {
            float averageEnemyCount = playerProfile.currentBattle.totalEnemiesInBattle;
            
            var observation = new BehaviorObservation(BehaviorTriggerType.NoAOEDamage, mapName);
            observation.SetData("hasAOE", false);
            observation.SetData("averageEnemyCount", averageEnemyCount);
            observation.SetData("skillNames", actions.Select(a => a?.actionName).ToList());
            
            playerProfile.AddObservation(observation);
        }
    }

    #endregion

    #region Battle Analysis - Recursos

    private void CheckBrokeAfterShopping(string mapName)
    {
        // Implementar no ShopManager
    }

    private void CheckRanOutOfConsumables(string mapName)
    {
        if (GameManager.Instance?.PlayerBattleActions == null) return;
        
        var consumables = GameManager.Instance.PlayerBattleActions
            .Where(a => a != null && a.isConsumable)
            .ToList();
        
        if (consumables.Count == 0) return;
        
        var usedConsumables = playerProfile.currentBattle.skillUsageCount.Keys
            .Where(skillName => consumables.Any(c => c.actionName == skillName))
            .ToList();
        
        bool allUsed = usedConsumables.Count == consumables.Count;
        bool anyExhausted = consumables.Any(c => c.currentUses == 0);
        
        if (allUsed && anyExhausted)
        {
            var observation = new BehaviorObservation(BehaviorTriggerType.RanOutOfConsumables, mapName);
            observation.SetData("consumablesUsed", usedConsumables);
            observation.SetData("totalConsumables", consumables.Count);
            
            playerProfile.AddObservation(observation);
        }
    }

    private void CheckConsumableDependency(string mapName)
    {
        var sessionData = playerProfile.session;
        
        if (sessionData.totalVictories < 3) return;
        
        float dependencyRate = (float)sessionData.victoriesWithConsumables / sessionData.totalVictories;
        
        if (dependencyRate > 0.5f)
        {
            var observation = new BehaviorObservation(BehaviorTriggerType.ConsumableDependency, mapName);
            observation.SetData("dependencyRate", dependencyRate);
            observation.SetData("victoriesWithConsumables", sessionData.victoriesWithConsumables);
            observation.SetData("totalVictories", sessionData.totalVictories);
            
            playerProfile.AddObservation(observation);
        }
    }

    #endregion

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
        }
    }

    #endregion

    #region Map Analysis

    private void AnalyzeMapBehavior()
    {
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
            }
            else
            {
                playerProfile = new PlayerBehaviorProfile();
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
    
    public void AddObservationDirectly(BehaviorObservation observation)
    {
        if (observation == null) return;
    
        playerProfile.AddObservation(observation);
        SavePlayerProfile();
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
    }

    public void ClearAllData()
    {
        playerProfile = new PlayerBehaviorProfile();
        SavePlayerProfile();
    }

    public string GetSummaryStats()
    {
        var stats = new System.Text.StringBuilder();
        stats.AppendLine("=== ESTATISTICAS DO JOGADOR ===");
        stats.AppendLine($"Total de observacoes: {playerProfile.observations.Count}");
        
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