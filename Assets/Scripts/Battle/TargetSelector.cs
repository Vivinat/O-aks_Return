// Assets/Scripts/Battle/TargetSelector.cs

using UnityEngine;

[RequireComponent(typeof(BattleEntity))]
public class TargetSelector : MonoBehaviour
{
    private BattleEntity battleEntity;
    private BattleHUD battleHUD;

    void Awake()
    {
        battleEntity = GetComponent<BattleEntity>();
    }

    void Start()
    {
        // Encontra a referência do HUD na cena
        battleHUD = FindObjectOfType<BattleHUD>();
    }

    // Este evento é chamado quando o Collider2D do objeto é clicado
    private void OnMouseDown()
    {
        // Se o personagem não estiver morto, avisa o HUD que ele foi selecionado
        if (!battleEntity.isDead && battleHUD != null)
        {
            battleHUD.OnTargetSelected(battleEntity);
        }
    }
}