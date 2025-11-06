using UnityEngine;

[RequireComponent(typeof(BattleEntity))]
public class TargetSelector : MonoBehaviour
{
    private BattleEntity battleEntity;
    private BattleHUD battleHUD;
    private EnemyHighlight enemyHighlight;

    void Awake()
    {
        battleEntity = GetComponent<BattleEntity>();
        enemyHighlight = GetComponent<EnemyHighlight>();
    }

    void Start()
    {
        battleHUD = FindObjectOfType<BattleHUD>();
    }

    private void OnMouseEnter()
    {
        // Ativa highlight apenas se estiver em modo de seleção de alvo
        if (battleHUD != null && battleHUD.targetSelectionPanel != null && 
            battleHUD.targetSelectionPanel.activeSelf && !battleEntity.isDead)
        {
            if (enemyHighlight != null)
            {
                enemyHighlight.StartHighlight();
            }
            
            Debug.Log($"Mouse sobre {battleEntity.characterData.characterName}");
        }
    }

    private void OnMouseExit()
    {
        if (enemyHighlight != null)
        {
            enemyHighlight.StopHighlight();
        }
    }

    private void OnMouseDown()
    {
        if (!battleEntity.isDead && battleHUD != null)
        {
            if (enemyHighlight != null)
            {
                enemyHighlight.StopHighlight();
            }
            
            battleHUD.OnTargetSelected(battleEntity);
        }
    }

    // Força parar o highlight quando a seleção é cancelada
    public void ForceStopHighlight()
    {
        if (enemyHighlight != null)
        {
            enemyHighlight.StopHighlight();
        }
    }
}