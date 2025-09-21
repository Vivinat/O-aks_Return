// Assets/Scripts/Managers/BattleManager.cs

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

    void Start()
    {
        InitializeEnemyTeam();
        InitializePlayerTeam();

        // Junta todos os personagens
        allCharacters = playerTeam.Concat(enemyTeam).ToList();

        currentState = BattleState.RUNNING;
    }

    private void InitializeEnemyTeam()
    {
        enemyTeam = new List<BattleEntity>();
        List<GameObject> enemySlots = GameObject.FindGameObjectsWithTag("EnemySlot").ToList();
        List<Character> enemiesToSpawn = GameManager.enemiesToBattle;

        // Desativa todos os slots
        enemySlots.ForEach(slot => slot.SetActive(false));

        for (int i = 0; i < enemiesToSpawn.Count; i++)
        {
            if (i < enemySlots.Count)
            {
                GameObject currentSlot = enemySlots[i];
                currentSlot.SetActive(true);

                BattleEntity entity = currentSlot.GetComponent<BattleEntity>();
                entity.characterData = enemiesToSpawn[i];
                
                SpriteRenderer sr = currentSlot.GetComponentInChildren<SpriteRenderer>();
                if(sr != null) sr.sprite = enemiesToSpawn[i].characterSprite;

                enemyTeam.Add(entity);
            }
        }
    }

    private void InitializePlayerTeam()
    {
        playerTeam = FindObjectsOfType<BattleEntity>().Where(e => e.characterData.team == Team.Player).ToList();
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

        // 1. Toca o efeito de flash do atacante
        caster.OnExecuteAction();
        Debug.Log($"{caster.characterData.characterName} usa {action.actionName}!");
        
        // Espera um pouco para o efeito visual acontecer
        yield return new WaitForSeconds(actionDelay);

        // 2. Aplica os efeitos da ação (dano, cura, etc.)
        if (caster.ConsumeMana(action.manaCost))
        {
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
                }
                yield return new WaitForSeconds(0.2f); // Pequeno delay entre alvos
            }
        }
        else
        {
            Debug.Log("Ação falhou por falta de MP!");
        }

        // 3. Espera um pouco antes de continuar
        yield return new WaitForSeconds(postActionDelay);
        
        // 4. Reseta o ATB e verifica o fim da batalha
        caster.ResetATB();
        CheckBattleEnd();
    }

    private IEnumerator PerformEnemyAction()
    {
        yield return new WaitForSeconds(1.0f);
    
        BattleAction chosenAction = activeCharacter.characterData.battleActions
            .Where(a => activeCharacter.currentMp >= a.manaCost)
            .OrderBy(a => Random.value) // Escolhe uma ação aleatória que ele possa pagar
            .FirstOrDefault();

        if (chosenAction == null)
        {
            activeCharacter.ResetATB();
            currentState = BattleState.RUNNING;
            yield break;
        }

        List<BattleEntity> targets = new List<BattleEntity>();
        List<BattleEntity> alivePlayers = playerTeam.Where(p => !p.isDead).ToList();

        if(alivePlayers.Any())
            targets.Add(alivePlayers[Random.Range(0, alivePlayers.Count)]);

        if (targets.Any())
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
        if (enemyTeam.All(e => e.isDead))
        {
            currentState = BattleState.WON;
            Debug.Log("VITÓRIA!");
            // ... sua lógica de recompensa ...
        }
        else if (playerTeam.All(p => p.isDead))
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