// Assets/Scripts/UI/BattleHUD.cs

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Use TextMeshPro para textos mais nítidos

public class BattleHUD : MonoBehaviour
{
    public BattleManager battleManager;

    [Header("UI Panels")]
    public GameObject actionPanel;      // Painel que contém os botões de ação
    public GameObject targetSelectionPanel; // Painel que diz "Escolha um Alvo"
    
    [Header("Prefabs")]
    public GameObject actionButtonPrefab; // Prefab de um botão que vamos criar

    private BattleEntity activeCharacter;
    private BattleAction selectedAction;

    void Start()
    {
        // Garante que os painéis comecem desativados
        actionPanel.SetActive(false);
        targetSelectionPanel.SetActive(false);
    }

    /// <summary>
    /// Mostra as ações disponíveis para o personagem ativo. Chamado pelo BattleManager.
    /// </summary>
    public void ShowActionMenu(BattleEntity character)
    {
        activeCharacter = character;
        actionPanel.SetActive(true);

        // Limpa quaisquer botões antigos
        foreach (Transform child in actionPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Cria um botão para cada ação que o personagem possui
        for (int i = 0; i < character.characterData.battleActions.Count; i++)
        {
            BattleAction action = character.characterData.battleActions[i];
            GameObject buttonObj = Instantiate(actionButtonPrefab, actionPanel.transform);
            
            // Configura o texto do botão
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = action.actionName;
            
            // Adiciona o listener para o clique, passando a ação correspondente
            int actionIndex = i; // Variável local para evitar problemas de escopo em lambdas
            buttonObj.GetComponent<Button>().onClick.AddListener(() => OnActionSelected(actionIndex));
        }
    }
    
    /// <summary>
    /// Chamado quando um botão de ação é clicado.
    /// </summary>
    private void OnActionSelected(int actionIndex)
    {
        actionPanel.SetActive(false);
        selectedAction = activeCharacter.characterData.battleActions[actionIndex];
        
        // Ativa o modo de seleção de alvo
        targetSelectionPanel.SetActive(true);
        Debug.Log($"Ação '{selectedAction.actionName}' selecionada. Escolha um alvo.");
    }
    
    /// <summary>
    /// Chamado por um BattleEntity quando ele é clicado como alvo.
    /// </summary>
    public void OnTargetSelected(BattleEntity target)
    {
        // Só executa se estivermos no modo de seleção de alvo
        if (targetSelectionPanel.activeSelf)
        {
            targetSelectionPanel.SetActive(false);
            
            List<BattleEntity> targets = new List<BattleEntity>();

            // Determina os alvos reais com base no TargetType da ação
            switch (selectedAction.targetType)
            {
                case TargetType.SingleEnemy:
                case TargetType.SingleAlly:
                    targets.Add(target);
                    break;
                case TargetType.AllEnemies:
                    targets.AddRange(battleManager.enemyTeam.Where(e => !e.isDead));
                    break;
                case TargetType.AllAllies:
                    targets.AddRange(battleManager.playerTeam.Where(p => !p.isDead));
                    break;
                case TargetType.Self:
                    targets.Add(activeCharacter);
                    break;
            }

            // Manda o BattleManager executar a ação com os alvos definidos
            if (targets.Count > 0)
            {
                battleManager.ExecuteAction(selectedAction, targets);
            }
        }
    }
}