// Assets/Scripts/Managers/BattleManager.cs

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
    // Não é mais um singleton. Referências podem ser obtidas pela UI ou outros scripts se necessário.

    [Header("Battle State")]
    public BattleState currentState;
    
    // As listas agora são privadas, gerenciadas internamente
    public List<BattleEntity> playerTeam;
    public List<BattleEntity> enemyTeam;
    private List<BattleEntity> allCharacters;
    private BattleEntity activeCharacter;
    public BattleHUD battleHUD;

    void Start()
    {
        // Inicializa as listas
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
                // Garante que não tentemos acessar um slot que não existe
                if (i < enemySlots.Count && enemySlots[i] != null)
                {
                    GameObject currentSlot = enemySlots[i];
                    currentSlot.SetActive(true); // Ativa o slot
                    
                    BattleEntity entity = currentSlot.GetComponent<BattleEntity>();
                    entity.characterData = enemiesToSpawn[i]; // Injeta os dados do inimigo!
                    
                    enemyTeam.Add(entity); // Adiciona à lista de combate
                }
            }
        }

        // O ideal é que os personagens já estejam na cena ou sejam instanciados
        // com base em dados de um GameManager.
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

        // Pega o personagem pronto com a maior velocidade como critério de desempate
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

        if (!caster.ConsumeMana(action.manaCost))
        {
            Debug.Log("Ação falhou por falta de MP!");
        }
        else
        {
            Debug.Log($"{caster.characterData.characterName} usa {action.actionName}!");
            yield return new WaitForSeconds(1.5f);

            // *** MUDANÇA PRINCIPAL: Itera sobre todos os alvos ***
            foreach (var target in targets)
            {
                if (target.isDead) continue; // Pula alvos já mortos

                // Aplica o efeito da ação em cada alvo
                switch (action.type)
                {
                    case ActionType.Attack:
                        target.TakeDamage(action.power);
                        break;
                    case ActionType.Heal:
                        target.Heal(action.power);
                        break;
                }
            }
        }

        yield return new WaitForSeconds(1.0f);
        caster.ResetATB();
        CheckBattleEnd();
    }
    
    private IEnumerator PerformEnemyAction()
    {
        yield return new WaitForSeconds(1.0f);
        
        BattleAction chosenAction = activeCharacter.characterData.battleActions[0];
        List<BattleEntity> targets = new List<BattleEntity>();

        // *** MUDANÇA PRINCIPAL: IA agora entende TargetType ***
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
            // Adicione casos para SingleAlly e AllAllies se os inimigos puderem curar
        }

        if (targets.Count > 0)
        {
            ExecuteAction(chosenAction, targets);
        }
        else
        {
            // Se não encontrou alvos válidos, apenas reseta o turno
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
            // Exemplo de como interagir com o GameManager sem ser singleton
            FindObjectOfType<GameManager>().ReturnToMap();
        }
        else if (allPlayersDead)
        {
            currentState = BattleState.LOST;
            Debug.Log("DERROTA!");
            // FindObjectOfType<GameManager>().LoadScene("GameOver");
        }
        else
        {
            currentState = BattleState.RUNNING;
        }
    }
}