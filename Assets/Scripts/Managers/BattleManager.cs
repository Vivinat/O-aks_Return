using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum BattleState { START, RUNNING, ACTION_PENDING, PERFORMING_ACTION, WON, LOST }

public class BattleManager : MonoBehaviour
{
    public BattleState currentState;
    
    public List<BattleEntity> playerTeam;
    public List<BattleEntity> enemyTeam;
    private List<BattleEntity> allCharacters;
    private BattleEntity activeCharacter;
    public BattleHUD battleHUD;

    [SerializeField] private float actionDelay = 0.5f;
    [SerializeField] private float postActionDelay = 1.0f;
    [SerializeField] private float enemyActionDisplayTime = 2.0f;
    
    public DialogueSO boss1Dialogue;
    public DialogueSO boss2Dialogue;
    public DialogueSO boss3Dialogue;
    
    private int currentTurnNumber = 0;
    private BattleActionBackup actionBackup = new BattleActionBackup();
    private BattleAction currentProcessingAction;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.InitializePlayerStats();
        }
        
        if (DeathNegotiationManager.Instance != null)
        {
            DeathNegotiationManager.Instance.ResetForNewBattle();
        }
    
        InitializeEnemyTeam();
        InitializePlayerTeam();

        allCharacters = playerTeam.Concat(enemyTeam).ToList();
        currentState = BattleState.RUNNING;
        currentTurnNumber = 0;
        StartCoroutine(CheckForBossDialogue());
    }
    
    // Instancia o SO do inimigo, aplica stats na instância e atribui ao BattleEntity
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
                
                Character originalEnemySO = enemiesToSpawn[i];
                Character enemyInstance = Instantiate(originalEnemySO);
                enemyInstance.name = originalEnemySO.name + " (Runtime Instance)";
            
                if (DifficultySystem.Instance != null)
                {
                    DifficultySystem.Instance.ApplyToEnemy_Stats(enemyInstance);
                }
            
                entity.characterData = enemyInstance;
            
                SpriteRenderer sr = currentSlot.GetComponentInChildren<SpriteRenderer>();
                if(sr != null) sr.sprite = enemyInstance.characterSprite;

                enemyTeam.Add(entity);
            }
        }
    }

    private void InitializePlayerTeam()
    {
        playerTeam = FindObjectsOfType<BattleEntity>()
            .Where(e => e.characterData.team == Team.Player)
            .ToList();
    
        foreach (BattleEntity player in playerTeam)
        {
            if (GameManager.Instance != null)
            {
                typeof(BattleEntity)
                    .GetField("currentHp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(player, GameManager.Instance.GetPlayerCurrentHP());
                
                player.currentMp = GameManager.Instance.GetPlayerCurrentMP();
                player.ForceUpdateValueTexts();
            }
        }
    }
    
    void Update()
    {
        if (currentState != BattleState.RUNNING) return;

        foreach (var character in allCharacters.Where(c => !c.isDead))
        {
            character.UpdateATB(Time.deltaTime);
        }

        var readyCharacter = allCharacters
            .Where(c => c.isReady && !c.isDead)
            .OrderByDescending(c => c.characterData.speed)
            .FirstOrDefault();

        if (readyCharacter != null)
        {
            activeCharacter = readyCharacter;
            currentState = BattleState.ACTION_PENDING;

            activeCharacter.ProcessStatusEffectsTurn();

            if (activeCharacter.isDead)
            {
                activeCharacter.ResetATB();
                CheckBattleEnd();
                currentState = BattleState.RUNNING;
                return;
            }

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
        currentProcessingAction = action;
        
        BehaviorAnalysisIntegration.OnTurnAction(caster);
        currentTurnNumber++;

        caster.OnExecuteAction();
        yield return new WaitForSeconds(actionDelay);

        if (caster.ConsumeMana(action.manaCost))
        {
            BehaviorAnalysisIntegration.OnPlayerSkillUsed(action, caster);
        
            if (action.isConsumable)
            {
                bool canStillUse = action.UseAction();
                if (!canStillUse)
                {
                    if (caster.characterData.team == Team.Player)
                    {
                        GameManager.Instance.RemoveItemFromInventory(action);
                    }
                }
            }
            
            int totalDamageThisAction = 0;

            foreach (ActionEffect effect in action.effects)
            {
                Dictionary<BattleEntity, int> hpBefore = new Dictionary<BattleEntity, int>();
                foreach (var target in targets)
                {
                    if (!target.isDead)
                    {
                        hpBefore[target] = target.GetCurrentHP();
                    }
                }
            
                yield return StartCoroutine(ApplyActionEffect(effect, caster, targets));
            
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
        
            if (totalDamageThisAction > 0 && caster.characterData.team == Team.Player)
            {
                BehaviorAnalysisIntegration.OnPlayerSkillDamage(action, totalDamageThisAction);
            }
        }

        yield return new WaitForSeconds(postActionDelay);
        caster.ResetATB();
        currentProcessingAction = null;
        CheckBattleEnd();
    }
    
    public int GetCurrentTurn()
    {
        return currentTurnNumber;
    }

    private IEnumerator ApplyActionEffect(ActionEffect effect, BattleEntity caster, List<BattleEntity> targets)
    {
        if (ActionEffectProcessor.RequiresSpecialProcessing(GetCurrentAction()))
        {
            foreach (var target in targets)
            {
                if (target.isDead) continue;
                ActionEffectProcessor.ProcessSpecialEffect(GetCurrentAction(), caster, target);
                yield return new WaitForSeconds(0.2f);
            }
            yield break;
        }

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
                
                case ActionType.RestoreMana:
                    target.RestoreMana(effect.power);
                    break;
                    
                case ActionType.Buff:
                case ActionType.Debuff:
                    break;
            }

            if (effect.statusEffect != StatusEffectType.None)
            {
                target.ApplyStatusEffect(effect.statusEffect, effect.statusPower, effect.statusDuration);
            }

            yield return new WaitForSeconds(0.2f);
        }

        if (effect.hasSelfEffect && !caster.isDead)
        {
            switch (effect.selfEffectType)
            {
                case ActionType.Attack:
                    caster.TakeDamage(effect.selfEffectPower);
                    break;
                    
                case ActionType.Heal:
                    caster.Heal(effect.selfEffectPower);
                    break;
                    
                case ActionType.Buff:
                case ActionType.Debuff:
                    break;
            }

            if (effect.selfStatusEffect != StatusEffectType.None)
            {
                caster.ApplyStatusEffect(effect.selfStatusEffect, effect.selfStatusPower, effect.selfStatusDuration);
            }
        }
    }
    
    private BattleAction GetCurrentAction()
    {
        return currentProcessingAction;
    }

    // Evita contaminação cruzada entre modificadores de jogador/inimigo em suas battleactions
    private IEnumerator PerformEnemyAction()
    {
        yield return new WaitForSeconds(1.0f);
        BattleEntity player = playerTeam.FirstOrDefault(p => !p.isDead);
    
        if (player == null)
        {
            activeCharacter.ResetATB();
            currentState = BattleState.RUNNING;
            yield break;
        }
        
        actionBackup.SaveActions(activeCharacter.characterData.battleActions);
        BattleActionRestorer.RestoreSingleCharacterActions(activeCharacter.characterData);
        
        if (DifficultySystem.Instance != null)
        {
            DifficultySystem.Instance.ApplyToEnemy_Actions(activeCharacter.characterData);
        }
        
        BattleAction chosenAction = EnemyAI.ChooseBestAction(activeCharacter, player, enemyTeam);

        if (chosenAction == null)
        {
            actionBackup.RestoreActions();
            activeCharacter.ResetATB();
            currentState = BattleState.RUNNING;
            yield break;
        }

        string enemyActionText = $"{activeCharacter.characterData.characterName} usa {chosenAction.actionName}!";
        battleHUD.ShowEnemyAction(enemyActionText);
        yield return new WaitForSeconds(enemyActionDisplayTime);
        battleHUD.HideEnemyAction();

        List<BattleEntity> targets = EnemyAI.ChooseBestTargets(chosenAction, activeCharacter, player, enemyTeam);

        if (targets.Any())
        {
            yield return StartCoroutine(ProcessAction(chosenAction, activeCharacter, targets));
        }
        else
        {
            activeCharacter.ResetATB();
            currentState = BattleState.RUNNING;
        }
        
        actionBackup.RestoreActions();
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
            int baseReward = Random.Range(20, 51);
            int totalEnemies = enemyTeam.Count;
            int rewardCoins = Mathf.Min(baseReward * totalEnemies, 150);
            StartCoroutine(HandleBattleVictory(rewardCoins));
        }
        else if (playerTeam.All(p => p.isDead))
        {
            currentState = BattleState.LOST;
            StartCoroutine(HandleBattleDefeat());
        }
        else
        {
            currentState = BattleState.RUNNING;
        }
    }
    
    private IEnumerator HandleBattleDefeat()
    {
        yield return new WaitForSeconds(2f);
        string defeatMessage = "Você foi derrotado...";
        battleHUD.ShowEnemyAction(defeatMessage);
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("Defeat_Scene");
    }

    private IEnumerator HandleBattleVictory(int rewardCoins)
    {
        yield return new WaitForSeconds(2f);
    
        if (DifficultySystem.Instance != null)
        {
            rewardCoins = DifficultySystem.Instance.GetModifiedCoins(rewardCoins);
        }
    
        string victoryMessage = $"Vitória! Recebido {rewardCoins} moedas!";
        GameManager.Instance.AddBattleReward(rewardCoins);
        battleHUD.ShowEnemyAction(victoryMessage);
        yield return new WaitForSeconds(3f);
        SavePlayerStatsToGameManager();
        GameManager.Instance.ReturnToMap();
    }
    
    public void OnPlayerTurnTimeout(BattleEntity character)
    {
        if (character == null) return;

        character.ResetATB();
    
        if (battleHUD != null)
        {
            battleHUD.ShowTemporaryMessage(
                $"{character.characterData.characterName} perdeu o turno!",
                1.5f
            );
        }
        
        currentState = BattleState.RUNNING;
    }
    
    private void SavePlayerStatsToGameManager()
    {
        if (GameManager.Instance == null) return;
        BattleEntity player = playerTeam.FirstOrDefault();
    
        if (player != null)
        {
            GameManager.Instance.SetPlayerCurrentHP(player.GetCurrentHP());
            GameManager.Instance.SetPlayerCurrentMP(player.GetCurrentMP());
        }
    }
    
    private IEnumerator CheckForBossDialogue()
    {
        yield return new WaitForSeconds(0.5f);

        if (GameManager.enemiesToBattle == null || GameManager.enemiesToBattle.Count == 0)
            yield break;

        var enemies = GameManager.enemiesToBattle.Select(e => e.characterName.ToLower()).ToList();

        string bossFound = null;
        DialogueSO dialogueToPlay = null;

        if (enemies.Any(n => n.Contains("mawron")))
        {
            bossFound = "Mawron";
            dialogueToPlay = boss1Dialogue;
        }
        else if (enemies.Any(n => n.Contains("valdemor")))
        {
            bossFound = "Valdemor";
            dialogueToPlay = boss2Dialogue;
        }
        else if (enemies.Any(n => n.Contains("fentho")))
        {
            bossFound = "Fentho";
            dialogueToPlay = boss3Dialogue;
        }

        if (bossFound == null)
            yield break;

        currentState = BattleState.START;
        bool finished = false;

        if (dialogueToPlay != null && DialogueManager.Instance != null)
        {
            DialogueUtils.ShowDialogue(dialogueToPlay, () => finished = true);
        }

        while (!finished)
            yield return null;

        currentState = BattleState.RUNNING;
    }
    
    public BattleEntity GetActiveCharacter()
    {
        return activeCharacter;
    }
}