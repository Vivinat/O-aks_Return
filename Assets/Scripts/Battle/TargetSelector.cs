// Assets/Scripts/Battle/TargetSelector.cs

using UnityEngine;

[RequireComponent(typeof(BattleEntity))]
public class TargetSelector : MonoBehaviour
{
    private BattleEntity battleEntity;
    private BattleHUD battleHUD;
    private EnemyHighlight enemyHighlight; // NOVO: Referência para o sistema de highlight

    void Awake()
    {
        battleEntity = GetComponent<BattleEntity>();
        enemyHighlight = GetComponent<EnemyHighlight>(); // NOVO: Pega o componente de highlight
    }

    void Start()
    {
        // Encontra a referência do HUD na cena
        battleHUD = FindObjectOfType<BattleHUD>();
    }

    // NOVO: Detecta quando o mouse entra na área do objeto
    private void OnMouseEnter()
    {
        // Só mostra highlight se estivermos em modo de seleção de alvo
        if (battleHUD != null && battleHUD.targetSelectionPanel != null && 
            battleHUD.targetSelectionPanel.activeSelf && !battleEntity.isDead)
        {
            // Ativa o highlight se o componente existir
            if (enemyHighlight != null)
            {
                enemyHighlight.StartHighlight();
            }
            
            Debug.Log($"Mouse sobre {battleEntity.characterData.characterName}");
        }
    }

    // NOVO: Detecta quando o mouse sai da área do objeto
    private void OnMouseExit()
    {
        // Para o highlight se o componente existir
        if (enemyHighlight != null)
        {
            enemyHighlight.StopHighlight();
        }
    }

    // Este evento é chamado quando o Collider2D do objeto é clicado
    private void OnMouseDown()
    {
        // Se o personagem não estiver morto, avisa o HUD que ele foi selecionado
        if (!battleEntity.isDead && battleHUD != null)
        {
            // NOVO: Para o highlight quando clicado
            if (enemyHighlight != null)
            {
                enemyHighlight.StopHighlight();
            }
            
            battleHUD.OnTargetSelected(battleEntity);
        }
    }

    // NOVO: Método para forçar parar o highlight (útil quando a seleção é cancelada)
    public void ForceStopHighlight()
    {
        if (enemyHighlight != null)
        {
            enemyHighlight.StopHighlight();
        }
    }
}