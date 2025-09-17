// Assets/Scripts/Managers/BattleManager.cs (Atualizado para Itens)

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BattleState
{
    START,
    RUNNING,
    ACTION_PENDING,
    PERFORMING_ACTION,
    WON,
    LOST
}

public class BattleManager : MonoBehaviour
{
    [Header("Battle State")]
    public BattleState currentState;
    
    public List<BattleEntity> playerTeam;
    public List<BattleEntity> enemyTeam;
    private List<BattleEntity> allCharacters;
    private BattleEntity activeCharacter;
    public BattleHUD battleHUD;

    [Header("Battle Rewards")]
    public int coinsRewardMin = 10;
    public int coinsRewardMax = 25;

    void Start()
    {
        playerTeam = new List<BattleEntity>();
        enemyTeam = new List<BattleEntity>();

        List<GameObject> enemySlots = new List<GameObject>
        {
            GameObject.FindWithTag("EnemySlot1"),
            GameObject.FindWithTag("EnemySlot2"),
            GameObject.FindWithTag("EnemySlot3"),
            GameObject.FindWithTag("EnemySlot4")
        };
        
        List<Character> enemiesToSpawn = GameManager.enemiesToBattle;

        // Desativa todos os slots para começar
        foreach (var slot in enemySlots)
        {
            if(slot != null) slot.SetActive(false);
        }
        
        if (enemiesToSpawn != null)
        {
            for (int i = 0; i < enemiesToSpawn.Count; i++)
            {
                if (i < enemySlots.Count && enemySlots[i] != null)
                {
                    GameObject currentSlot = enemySlots[i];
                    currentSlot.SetActive(true);
                    
                    BattleEntity entity = currentSlot.GetComponent<BattleEntity>();
                    entity.characterData = enemiesToSpawn[i];
                    
                    enemyTeam.Add(entity);
                }
            }
        }

        playerTeam = FindObjectsOfType<BattleEntity>().Where(e => e.characterData.team == Team.Player).ToList();
        enemyTeam = FindObjectsOfType<BattleEntity>().Where(e => e.characterData.team == Team.Enemy).ToList();
        allCharacters = playerTeam.Concat(enemyTeam).ToList();
        
        currentState = BattleState.RUNNING;
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

        bool actionExecuted = false;

        // NOVO: Verifica se é consumível
        if (action.isConsumable)
        {
            if (action.CanUse())
            {
                Debug.Log($"{caster.characterData.characterName} usa o consumível {action.actionName}!");
                yield return new WaitForSeconds(1.5f);

                // Executa o efeito do consumível
                foreach (var target in targets)
                {
                    if (target.isDead) continue;

                    switch (action.type)
                    {
                        case ActionType.Attack:
                            target.TakeDamage(action.power);
                            break;
                        case ActionType.Heal:
                            target.Heal(action.power);
                            break;
                        // Adicione outros tipos conforme necessário
                    }
                }

                // USA o consumível
                bool stillHasUses = action.UseAction();
                
                // Se não tem mais usos, remove do inventário
                if (!stillHasUses)
                {
                    Debug.Log($"Consumível {action.actionName} esgotou seus usos e será removido");
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.RemoveItemFromInventory(action);
                    }
                    
                    // Se o consumível for do jogador ativo, atualiza o menu de ações
                    if (caster.characterData.team == Team.Player)
                    {
                        // Força atualização do menu após a ação
                        StartCoroutine(UpdatePlayerMenuAfterDelay());
                    }
                }

                actionExecuted = true;
            }
            else
            {
                Debug.Log($"Consumível {action.actionName} não tem mais usos!");
            }
        }
        else
        {
            // Ação normal (não é consumível) - usa MP
            if (!caster.ConsumeMana(action.manaCost))
            {
                Debug.Log("Ação falhou por falta de MP!");
            }
            else
            {
                Debug.Log($"{caster.characterData.characterName} usa {action.actionName}!");
                yield return new WaitForSeconds(1.5f);

                foreach (var target in targets)
                {
                    if (target.isDead) continue;

                    switch (action.type)
                    {
                        case ActionType.Attack:
                            target.TakeDamage(action.power);
                            break;
                        case ActionType.Heal:
                            target.Heal(action.power);
                            break;
                        // Adicione buff/debuff conforme necessário
                    }
                }

                actionExecuted = true;
            }
        }

        if (actionExecuted)
        {
            yield return new WaitForSeconds(1.0f);
        }

        caster.ResetATB();
        CheckBattleEnd();
    }

    /// <summary>
    /// Atualiza o menu do jogador com um delay (para dar tempo da ação terminar)
    /// </summary>
    private IEnumerator UpdatePlayerMenuAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        
        // Atualiza as ações do jogador removendo o item usado
        if (activeCharacter != null && activeCharacter.characterData.team == Team.Player)
        {
            // Sincroniza as ações do character com o GameManager
            if (GameManager.Instance != null && GameManager.Instance.PlayerCharacterInfo != null)
            {
                activeCharacter.characterData.battleActions = new List<BattleAction>(GameManager.Instance.PlayerBattleActions);
            }
        }
    }
    
    private IEnumerator PerformEnemyAction()
    {
        yield return new WaitForSeconds(1.0f);
    
        // Inimigos só usam ações normais (não consumíveis)
        BattleAction chosenAction = activeCharacter.characterData.battleActions
            .Where(a => !a.isConsumable && activeCharacter.currentMp >= a.manaCost)
            .OrderByDescending(a => a.manaCost)
            .FirstOrDefault();

        if (chosenAction == null)
        {
            Debug.Log($"{activeCharacter.characterData.characterName} não tem mana para nenhuma ação e perdeu o turno!");
            yield return new WaitForSeconds(1.0f);
            activeCharacter.ResetATB();
            currentState = BattleState.RUNNING;
            yield break;
        }

        List<BattleEntity> targets = new List<BattleEntity>();

        switch (chosenAction.targetType)
        {
            case TargetType.SingleEnemy:
                List<BattleEntity> alivePlayers = playerTeam.Where(p => !p.isDead).ToList();
                if (alivePlayers.Count > 0)
                    targets.Add(alivePlayers[Random.Range(0, alivePlayers.Count)]);
                break;
            case TargetType.AllEnemies:
                targets = playerTeam.Where(p => !p.isDead).ToList();
                break;
            case TargetType.Self:
                targets.Add(activeCharacter);
                break;
        }

        if (targets.Count > 0)
        {
            ExecuteAction(chosenAction, targets);
        }
        else
        {
            activeCharacter.ResetATB();
            currentState = BattleState.RUNNING;
        }
    }
    
    private void CheckBattleEnd()
    {
        bool allEnemiesDead = enemyTeam.All(e => e.isDead);
        bool allPlayersDead = playerTeam.All(p => p.isDead);

        if (allEnemiesDead)
        {
            currentState = BattleState.WON;
            Debug.Log("VITÓRIA!");
            
            // NOVO: Recompensa em moedas
            int coinsReward = Random.Range(coinsRewardMin, coinsRewardMax + 1);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddBattleReward(coinsReward);
            }
            
            FindObjectOfType<GameManager>().ReturnToMap();
        }
        else if (allPlayersDead)
        {
            currentState = BattleState.LOST;
            Debug.Log("DERROTA!");
        }
        else
        {
            currentState = BattleState.RUNNING;
        }
    }
}