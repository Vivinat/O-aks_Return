// GameManager.cs (Versão com Sistema de Moedas)

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Player Configuration")]
    public Character PlayerCharacterInfo;
    public EventTypeSO CurrentEvent { get; private set; }
    
    [Header("Currency System")]
    [SerializeField]
    private CurrencySystem currencySystem = new CurrencySystem();
    public CurrencySystem CurrencySystem => currencySystem;
    
    public static List<Character> enemiesToBattle;
    public static TreasurePoolSO battleActionsPool; 
    
    public List<BattleAction> PlayerBattleActions { get; set; } = new List<BattleAction>();
    
    // Agora, usamos um dicionário para guardar o estado de MÚLTIPLOS mapas.
    // A chave é o nome da cena do mapa, e o valor é o pacote de dados daquele mapa.
    private Dictionary<string, MapStateData> savedMapStates = new Dictionary<string, MapStateData>();
    private string currentMapSceneName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePlayerActions();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializePlayerActions()
    {
        PlayerBattleActions.Clear();
        if (PlayerCharacterInfo != null && PlayerCharacterInfo.battleActions != null)
        {
            PlayerBattleActions.AddRange(PlayerCharacterInfo.battleActions);
        }
    }

    /// <summary>
    /// Salva o "pacote de dados" de um mapa.
    /// </summary>
    public void SaveMapState(MapStateData mapData, string mapName)
    {
        currentMapSceneName = mapName;
        savedMapStates[mapName] = mapData;
        Debug.Log($"Estado do mapa '{mapName}' salvo no GameManager.");
    }

    /// <summary>
    /// Tenta recuperar o "pacote de dados" de um mapa.
    /// </summary>
    public MapStateData GetSavedMapState(string mapName)
    {
        if (savedMapStates.TryGetValue(mapName, out MapStateData mapData))
        {
            return mapData;
        }
        return null;
    }

    public void StartEvent(EventTypeSO eventData)
    {
        CurrentEvent = eventData;
        
        // Verifica se o evento é do tipo Batalha
        if (eventData is BattleEventSO battleEvent)
        {
            enemiesToBattle = battleEvent.enemies;
            Debug.Log($"Iniciando batalha com {enemiesToBattle.Count} inimigos.");
        }
        else if (eventData is TreasureEventSO treasureEventSo)
        {
            battleActionsPool = treasureEventSo.poolForTheMap;
            Debug.Log($"Iniciando skill selection scene");
        }
        else if (eventData is ShopEventSO shopEvent)
        {
            Debug.Log($"Iniciando loja com {shopEvent.actionsForSale?.Count ?? 0} itens");
        }
        else
        {
            enemiesToBattle = null;
        }

        SceneManager.LoadScene(eventData.sceneToLoad);
    }
    
    public void ReturnToMap()
    {
        if (!string.IsNullOrEmpty(currentMapSceneName))
        {
            SceneManager.LoadScene(currentMapSceneName);
        }
        else
        {
            Debug.LogError("Nome da cena do mapa não foi salvo! Não é possível retornar.");
        }
    }

    /// <summary>
    /// Remove um item do inventário quando seus usos se esgotam
    /// </summary>
    public void RemoveItemFromInventory(BattleAction itemToRemove)
    {
        if (PlayerBattleActions.Contains(itemToRemove))
        {
            PlayerBattleActions.Remove(itemToRemove);
            
            // Atualiza também o Character data
            if (PlayerCharacterInfo != null && PlayerCharacterInfo.battleActions != null)
            {
                PlayerCharacterInfo.battleActions.Remove(itemToRemove);
            }
            
            Debug.Log($"Item '{itemToRemove.actionName}' removido do inventário (usos esgotados)");
        }
    }

    /// <summary>
    /// Adiciona moedas ao jogador (útil para recompensas de batalha)
    /// </summary>
    public void AddBattleReward(int coins)
    {
        currencySystem.AddCoins(coins);
        Debug.Log($"Recompensa de batalha: {coins} moedas");
    }
}