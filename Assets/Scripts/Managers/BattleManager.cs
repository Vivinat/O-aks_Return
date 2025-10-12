// Assets/Scripts/Managers/BattleManager.cs (Enhanced for new Action System)

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BattleState { START, RUNNING, ACTION_PENDING, PERFORMING_ACTION, WON, LOST }

public class BattleManager : MonoBehaviour
{
    [Header("Battle State")]
    public BattleState currentState;
    
    public List<BattleEntity> playerTeam;
    public List<BattleEntity> enemyTeam;
    private List<BattleEntity> allCharacters;
    private BattleEntity activeCharacter;
    public BattleHUD battleHUD;

    [Header("Timings")]
    [SerializeField] private float actionDelay = 0.5f;
    [SerializeField] private float postActionDelay = 1.0f;
    [SerializeField] private float enemyActionDisplayTime = 2.0f;
    
    private int currentTurnNumber = 0;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.InitializePlayerStats();
        }
    
        InitializeEnemyTeam();
        InitializePlayerTeam();

        allCharacters = playerTeam.Concat(enemyTeam).ToList();
        currentState = BattleState.RUNNING;
        currentTurnNumber = 0;
    }
    
    private void InitializeEnemyTeam()
    {
        enemyTeam = new List<BattleEntity>();
        List<GameObject> enemySlots = GameObject.FindGameObjectsWithTag("EnemySlot").ToList();
        List<Character> enemiesToSpawn = GameManager.enemiesToBattle;

        enemySlots.ForEach(slot => slot.SetActive(false));

        for (int i = 0; i < enemiesToSpawn.Count; i++)
        {
            if (i < enemySlots.Count)
            {
                GameObject currentSlot = enemySlots[i];
                currentSlot.SetActive(true);

                BattleEntity entity = currentSlot.GetComponent<BattleEntity>();
            
                // NOVO: Aplica modificadores de dificuldade ao inimigo ANTES de atribuir
                if (DifficultySystem.Instance != null)
                {
                    DifficultySystem.Instance.ApplyToEnemy(enemiesToSpawn[i]);
                }
            
                entity.characterData = enemiesToSpawn[i];
            
                SpriteRenderer sr = currentSlot.GetComponentInChildren<SpriteRenderer>();
                if(sr != null) sr.sprite = enemiesToSpawn[i].characterSprite;

                enemyTeam.Add(entity);
            }
        }
    }

    private void InitializePlayerTeam()
    {
        playerTeam = FindObjectsOfType<BattleEntity>()
            .Where(e => e.characterData.team == Team.Player)
            .ToList();
    
        // NOVO: Carrega HP/MP atuais do GameManager
        foreach (BattleEntity player in playerTeam)
        {
            if (GameManager.Instance != null)
            {
                // Usa os valores salvos no GameManager
                typeof(BattleEntity)
                    .GetField("currentHp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(player, GameManager.Instance.GetPlayerCurrentHP());
                
                player.currentMp = GameManager.Instance.GetPlayerCurrentMP();
            
                // Força update das barras
                player.ForceUpdateValueTexts();
            }
        }
    }
    
    void Update()
    {
        if (currentState != BattleState.RUNNING) return;

        // Atualiza ATB de todos
        foreach (var character in allCharacters.Where(c => !c.isDead))
        {
            character.UpdateATB(Time.deltaTime);
        }

        // Verifica quem está pronto para agir
        var readyCharacter = allCharacters
            .Where(c => c.isReady && !c.isDead)
            .OrderByDescending(c => c.characterData.speed)
            .FirstOrDefault();

        if (readyCharacter != null)
        {
            activeCharacter = readyCharacter;
            currentState = BattleState.ACTION_PENDING;

            // Process status effects at the start of turn
            activeCharacter.ProcessStatusEffectsTurn();

            if (activeCharacter.characterData.team == Team.Player)
            {
                battleHUD.ShowActionMenu(activeCharacter);
            }
            else
            {
                StartCoroutine(PerformEnemyAction());
            }
        }
    }
    
    public void ExecuteAction(BattleAction action, List<BattleEntity> targets)
    {
        StartCoroutine(ProcessAction(action, activeCharacter, targets));
    }

    private IEnumerator ProcessAction(BattleAction action, BattleEntity caster, List<BattleEntity> targets)
    {
        currentState = BattleState.PERFORMING_ACTION;
        currentProcessingAction = action; // Set current action for special processing
        
        // NOVO: Registra ação na ordem de turnos
        BehaviorAnalysisIntegration.OnTurnAction(caster);
    
        currentTurnNumber++; // NOVO: Incrementa contador de turno

        // 1. Toca o efeito de flash do atacante
        caster.OnExecuteAction();
        Debug.Log($"{caster.characterData.characterName} uses {action.actionName}!");
    
        // Espera um pouco para o efeito visual acontecer
        yield return new WaitForSeconds(actionDelay);

        // 2. Check mana cost and apply effects
        if (caster.ConsumeMana(action.manaCost))
        {
            BehaviorAnalysisIntegration.OnPlayerSkillUsed(action, caster);
        
            // Handle consumable usage
            if (action.isConsumable)
            {
                bool canStillUse = action.UseAction();
                if (!canStillUse)
                {
                    if (caster.characterData.team == Team.Player)
                    {
                        GameManager.Instance.RemoveItemFromInventory(action);
                    }
                    Debug.Log($"{action.actionName} was removed - uses exhausted!");
                }
            }
            
            // NOVO: Rastreia dano total causado pela skill
            int totalDamageThisAction = 0;

            foreach (ActionEffect effect in action.effects)
            {
                // MODIFICADO: Captura HP antes do efeito
                Dictionary<BattleEntity, int> hpBefore = new Dictionary<BattleEntity, int>();
                foreach (var target in targets)
                {
                    if (!target.isDead)
                    {
                        hpBefore[target] = target.GetCurrentHP();
                    }
                }
            
                yield return StartCoroutine(ApplyActionEffect(effect, caster, targets));
            
                // NOVO: Calcula dano causado
                foreach (var target in targets)
                {
                    if (hpBefore.ContainsKey(target))
                    {
                        int hpAfter = target.isDead ? 0 : target.GetCurrentHP();
                        int damageCaused = hpBefore[target] - hpAfter;
                    
                        if (damageCaused > 0)
                        {
                            totalDamageThisAction += damageCaused;
                        }
                    }
                }
            }
        
            // NOVO: Registra dano causado pela skill (apenas para jogador)
            if (totalDamageThisAction > 0 && caster.characterData.team == Team.Player)
            {
                BehaviorAnalysisIntegration.OnPlayerSkillDamage(action, totalDamageThisAction);
            }
        }
        else
        {
            Debug.Log("Action failed due to insufficient MP!");
        }

        yield return new WaitForSeconds(postActionDelay);
    
        caster.ResetATB();
        currentProcessingAction = null;
        CheckBattleEnd();
    }
    
    /// <summary>
    /// NOVO: Retorna o número do turno atual
    /// </summary>
    public int GetCurrentTurn()
    {
        return currentTurnNumber;
    }

    private IEnumerator ApplyActionEffect(ActionEffect effect, BattleEntity caster, List<BattleEntity> targets)
    {
        // Check for special effects first
        if (ActionEffectProcessor.RequiresSpecialProcessing(GetCurrentAction()))
        {
            foreach (var target in targets)
            {
                if (target.isDead) continue;
                ActionEffectProcessor.ProcessSpecialEffect(GetCurrentAction(), caster, target);
                yield return new WaitForSeconds(0.2f);
            }
            yield break; // Use yield break instead of return in coroutines
        }

        // Apply primary effect to targets
        foreach (var target in targets)
        {
            if (target.isDead) continue;

            switch (effect.effectType)
            {
                case ActionType.Attack:
                    int attackPower = caster.GetModifiedAttackPower(effect.power);
                    target.TakeDamage(attackPower, caster);
                    break;
                    
                case ActionType.Heal:
                    target.Heal(effect.power);
                    break;
                    
                case ActionType.Buff:
                case ActionType.Debuff:
                    // Status effects are handled below
                    break;
            }

            // Apply status effect if present
            if (effect.statusEffect != StatusEffectType.None)
            {
                target.ApplyStatusEffect(effect.statusEffect, effect.statusPower, effect.statusDuration);
            }

            yield return new WaitForSeconds(0.2f);
        }

        // Apply self effect if present
        if (effect.hasSelfEffect && !caster.isDead)
        {
            switch (effect.selfEffectType)
            {
                case ActionType.Attack:
                    caster.TakeDamage(effect.selfEffectPower);
                    Debug.Log($"{caster.characterData.characterName} takes {effect.selfEffectPower} recoil damage!");
                    break;
                    
                case ActionType.Heal:
                    caster.Heal(effect.selfEffectPower);
                    break;
                    
                case ActionType.Buff:
                case ActionType.Debuff:
                    // Self status effect handled below
                    break;
            }

            // Apply self status effect if present
            if (effect.selfStatusEffect != StatusEffectType.None)
            {
                caster.ApplyStatusEffect(effect.selfStatusEffect, effect.selfStatusPower, effect.selfStatusDuration);
            }
        }
    }

    // Helper to get current action being processed
    private BattleAction currentProcessingAction;
    
    private BattleAction GetCurrentAction()
    {
        return currentProcessingAction;
    }

    private IEnumerator PerformEnemyAction()
    {
        yield return new WaitForSeconds(1.0f);
    
        BattleAction chosenAction = activeCharacter.characterData.battleActions
            .Where(a => activeCharacter.currentMp >= a.manaCost && (!a.isConsumable || a.CanUse()))
            .OrderBy(a => Random.value)
            .FirstOrDefault();

        if (chosenAction == null)
        {
            Debug.Log($"{activeCharacter.characterData.characterName} has no available actions!");
            activeCharacter.ResetATB();
            currentState = BattleState.RUNNING;
            yield break;
        }

        // Show enemy action
        string enemyActionText = $"{activeCharacter.characterData.characterName} uses {chosenAction.actionName}!";
        battleHUD.ShowEnemyAction(enemyActionText);
        
        yield return new WaitForSeconds(enemyActionDisplayTime);
        battleHUD.HideEnemyAction();

        // Choose targets based on action
        List<BattleEntity> targets = GetTargetsForAction(chosenAction, activeCharacter);

        if (targets.Any())
        {
            StartCoroutine(ProcessAction(chosenAction, activeCharacter, targets));
        }
        else
        {
            Debug.Log($"{activeCharacter.characterData.characterName} found no valid targets!");
            activeCharacter.ResetATB();
            currentState = BattleState.RUNNING;
        }
    }

    private List<BattleEntity> GetTargetsForAction(BattleAction action, BattleEntity caster)
    {
        List<BattleEntity> targets = new List<BattleEntity>();
        
        switch (action.targetType)
        {
            case TargetType.SingleEnemy:
                if (caster.characterData.team == Team.Player)
                {
                    var aliveEnemies = enemyTeam.Where(e => !e.isDead).ToList();
                    if (aliveEnemies.Any())
                        targets.Add(aliveEnemies[Random.Range(0, aliveEnemies.Count)]);
                }
                else
                {
                    var alivePlayers = playerTeam.Where(p => !p.isDead).ToList();
                    if (alivePlayers.Any())
                        targets.Add(alivePlayers[Random.Range(0, alivePlayers.Count)]);
                }
                break;
                
            case TargetType.SingleAlly:
                if (caster.characterData.team == Team.Player)
                {
                    var alivePlayers = playerTeam.Where(p => !p.isDead).ToList();
                    if (alivePlayers.Any())
                        targets.Add(alivePlayers[Random.Range(0, alivePlayers.Count)]);
                }
                else
                {
                    var aliveEnemies = enemyTeam.Where(e => !e.isDead).ToList();
                    if (aliveEnemies.Any())
                        targets.Add(aliveEnemies[Random.Range(0, aliveEnemies.Count)]);
                }
                break;
                
            case TargetType.Self:
                targets.Add(caster);
                break;
                
            case TargetType.AllEnemies:
                if (caster.characterData.team == Team.Player)
                {
                    targets.AddRange(enemyTeam.Where(e => !e.isDead));
                }
                else
                {
                    targets.AddRange(playerTeam.Where(p => !p.isDead));
                }
                break;
                
            case TargetType.AllAllies:
                if (caster.characterData.team == Team.Player)
                {
                    targets.AddRange(playerTeam.Where(p => !p.isDead));
                }
                else
                {
                    targets.AddRange(enemyTeam.Where(e => !e.isDead));
                }
                break;
                
            case TargetType.Everyone:
                targets.AddRange(allCharacters.Where(c => !c.isDead));
                break;
        }

        return targets;
    }
    
    private void CheckBattleEnd()
    {
        if (PlayerBehaviorAnalyzer.Instance != null)
        {
            PlayerBehaviorAnalyzer.Instance.RecordBattleEnd();
        }
        
        if (enemyTeam.All(e => e.isDead))
        {
            currentState = BattleState.WON;
            Debug.Log("VICTORY!");
            
            int rewardCoins = Random.Range(10, 31);
            GameManager.Instance.AddBattleReward(rewardCoins);
            
            StartCoroutine(HandleBattleVictory(rewardCoins));
        }
        else if (playerTeam.All(p => p.isDead))
        {
            currentState = BattleState.LOST;
            Debug.Log("DEFEAT!");
        }
        else
        {
            currentState = BattleState.RUNNING;
        }
    }

    private IEnumerator HandleBattleVictory(int rewardCoins)
    {
        yield return new WaitForSeconds(2f);
    
        if (DifficultySystem.Instance != null)
        {
            rewardCoins = DifficultySystem.Instance.GetModifiedCoins(rewardCoins);
        }
    
        string victoryMessage = $"Vitória! Recebido {rewardCoins} moedas!";
        battleHUD.ShowEnemyAction(victoryMessage);
    
        yield return new WaitForSeconds(3f);
    
        // NOVO: Salva stats antes de sair
        SavePlayerStatsToGameManager();
    
        GameManager.Instance.ReturnToMap();
    }
    
    public void OnPlayerTurnTimeout(BattleEntity character)
    {
        if (character == null) return;

        Debug.Log($"⏱️ {character.characterData.characterName} perdeu o turno por timeout!");

        // Reseta a ATB do personagem
        character.ResetATB();
    
        // Mostra uma mensagem temporária
        if (battleHUD != null)
        {
            battleHUD.ShowTemporaryMessage(
                $"{character.characterData.characterName} perdeu o turno!",
                1.5f
            );
        }
        
        currentState = BattleState.RUNNING;
    }
    
    /// <summary>
    /// Salva o HP/MP atual do jogador no GameManager quando a batalha termina
    /// </summary>
    private void SavePlayerStatsToGameManager()
    {
        if (GameManager.Instance == null) return;
    
        // Pega o primeiro jogador vivo (ou o primeiro se todos estão mortos)
        BattleEntity player = playerTeam.FirstOrDefault();
    
        if (player != null)
        {
            GameManager.Instance.SetPlayerCurrentHP(player.GetCurrentHP());
            GameManager.Instance.SetPlayerCurrentMP(player.GetCurrentMP());
        
            Debug.Log($"Stats do jogador salvos - HP: {player.GetCurrentHP()}/{player.GetMaxHP()}, MP: {player.GetCurrentMP()}/{player.GetMaxMP()}");
        }
    }
}