// Assets/Scripts/Managers/BattleManager.cs (FIXED - NO CROSS-CONTAMINATION)

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    
    [Header("Boss Dialogues")]
    public DialogueSO boss1Dialogue;
    public DialogueSO boss2Dialogue;
    public DialogueSO boss3Dialogue;
    
    private int currentTurnNumber = 0;
    
    // Sistema de backup para evitar contaminação entre jogador/inimigo
    private BattleActionBackup actionBackup = new BattleActionBackup();

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
    
    /// <summary>
    /// CORRIGIDO: Instancia o SO do inimigo, aplica stats na instância,
    /// e atribui a INSTÂNCIA ao BattleEntity.
    /// Isso corrige os tooltips SEM contaminar as ações.
    /// </summary>
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
                
                // --- INÍCIO DA CORREÇÃO ---
                
                // 1. Pega o ScriptableObject original
                Character originalEnemySO = enemiesToSpawn[i];

                // 2. Cria uma INSTÂNCIA (cópia) dele
                Character enemyInstance = Instantiate(originalEnemySO);
                enemyInstance.name = originalEnemySO.name + " (Runtime Instance)";
            
                // 3. Aplica os STATS (HP, Def, etc) nessa INSTÂNCIA
                if (DifficultySystem.Instance != null)
                {
                    // Agora estamos modificando 'enemyInstance', não o SO original
                    DifficultySystem.Instance.ApplyToEnemy_Stats(enemyInstance);
                }
            
                // 4. Atribui a INSTÂNCIA modificada ao BattleEntity
                entity.characterData = enemyInstance;
                
                // --- FIM DA CORREÇÃO ---
            
                SpriteRenderer sr = currentSlot.GetComponentInChildren<SpriteRenderer>();
                // Usa o sprite da instância (que é o mesmo do original)
                if(sr != null) sr.sprite = enemyInstance.characterSprite;

                enemyTeam.Add(entity);
            }
        }
    }

    /// <summary>
    /// Inicializa jogador SEM aplicar modificadores
    /// (modificações já foram aplicadas nos SOs pela negociação)
    /// </summary>
    private void InitializePlayerTeam()
    {
        playerTeam = FindObjectsOfType<BattleEntity>()
            .Where(e => e.characterData.team == Team.Player)
            .ToList();
    
        // Carrega HP/MP salvos
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

            // ✅ ADICIONE ESTA VERIFICAÇÃO
            if (activeCharacter.isDead)
            {
                Debug.Log($"{activeCharacter.characterData.characterName} morreu por efeitos de status antes de agir!");
                activeCharacter.ResetATB();
                CheckBattleEnd(); // Verifica se a batalha terminou
                currentState = BattleState.RUNNING;
                return; // Importante: sai do Update sem processar a ação
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
        Debug.Log($"{caster.characterData.characterName} uses {action.actionName}!");
    
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
                    Debug.Log($"{action.actionName} was removed - uses exhausted!");
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
        else
        {
            Debug.Log("Action failed due to insufficient MP!");
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
                    Debug.Log($"{caster.characterData.characterName} takes {effect.selfEffectPower} recoil damage!");
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

    private BattleAction currentProcessingAction;
    
    private BattleAction GetCurrentAction()
    {
        return currentProcessingAction;
    }

    /// <summary>
    /// CORRIGIDO: Evita contaminação cruzada entre modificadores de jogador/inimigo
    /// </summary>
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
        
        // === PASSO 1: SALVA valores atuais das ações DESTE inimigo ===
        actionBackup.SaveActions(activeCharacter.characterData.battleActions);
        
        // === PASSO 2: RESETA para valores base ===
        BattleActionRestorer.RestoreSingleCharacterActions(activeCharacter.characterData);
        
        // === PASSO 3: APLICA modificadores de inimigo (se houver) ===
        if (DifficultySystem.Instance != null)
        {
            DifficultySystem.Instance.ApplyToEnemy_Actions(activeCharacter.characterData);
        }
        
        // === PASSO 4: DEIXA ESCOLHER e AGIR ===
        BattleAction chosenAction = EnemyAI.ChooseBestAction(activeCharacter, player, enemyTeam);

        if (chosenAction == null)
        {
            Debug.Log($"{activeCharacter.characterData.characterName} não tem ações disponíveis!");
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
            Debug.Log($"{activeCharacter.characterData.characterName} não encontrou alvos válidos!");
            activeCharacter.ResetATB();
            currentState = BattleState.RUNNING;
        }
        
        // === PASSO 5: VOLTA o que era antes ===
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
            Debug.Log("VICTORY!");
            
            int baseReward = Random.Range(20, 51);
            int totalEnemies = enemyTeam.Count;
            int rewardCoins = baseReward * totalEnemies;
            rewardCoins = Mathf.Min(rewardCoins, 150);
            Debug.Log("Acrescentado " + rewardCoins + " ao jogador. Fórmula usada:" + baseReward + " + " + totalEnemies );
            
            StartCoroutine(HandleBattleVictory(rewardCoins));
        }
        else if (playerTeam.All(p => p.isDead))
        {
            currentState = BattleState.LOST;
            Debug.Log("DEFEAT!");
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
    
        Debug.Log("=== JOGADOR MORREU - INDO PARA TELA DE MORTE ===");
    
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

        Debug.Log($"⏱️ {character.characterData.characterName} perdeu o turno por timeout!");

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
        
            Debug.Log($"Stats do jogador salvos - HP: {player.GetCurrentHP()}/{player.GetMaxHP()}, MP: {player.GetCurrentMP()}/{player.GetMaxMP()}");
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

        Debug.Log($"[BattleManager] Boss detectado: {bossFound}");

        currentState = BattleState.START;

        bool finished = false;

        if (dialogueToPlay != null && DialogueManager.Instance != null)
        {
            DialogueUtils.ShowDialogue(dialogueToPlay, () => finished = true);
        }

        while (!finished)
            yield return null;

        currentState = BattleState.RUNNING;
        Debug.Log($"[BattleManager] Diálogo do boss '{bossFound}' concluído. Batalha retomada.");
    }
    
    public BattleEntity GetActiveCharacter()
    {
        return activeCharacter;
    }
}