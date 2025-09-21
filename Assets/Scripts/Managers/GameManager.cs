// GameManager.cs (Versão Atualizada com AudioManager)

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
    
    // Dicionário para guardar o estado de MÚLTIPLOS mapas
    private Dictionary<string, MapStateData> savedMapStates = new Dictionary<string, MapStateData>();
    private string currentMapSceneName;
    
    public static Sprite pendingBattleBackground; // Sprite que será usado na próxima batalha

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

    /// <summary>
    /// Limpa os dados de um mapa específico
    /// </summary>
    public void ClearMapData(string mapName)
    {
        if (savedMapStates.ContainsKey(mapName))
        {
            savedMapStates.Remove(mapName);
            Debug.Log($"Dados do mapa '{mapName}' foram limpos.");
        }
    }

    public void StartEvent(EventTypeSO eventData, MapNode sourceNode = null)
    {
        CurrentEvent = eventData;

        // LINHA-CHAVE: AQUI NÓS CAPTURAMOS O BACKGROUND DO MAPNODE
        if (sourceNode != null)
        {
            pendingBattleBackground = sourceNode.battleBackgroundOverride;
            Debug.Log($"Background pendente '{pendingBattleBackground?.name ?? "NULL"}' foi registrado a partir do nó '{sourceNode.name}'");
        }
        else
        {
            // Garante que não usemos um background antigo de um evento anterior
            pendingBattleBackground = null;
        }

        // O resto do seu código continua igual...
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
        // NOVO: Volta à música do mapa quando retorna
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ReturnToMapMusic();
        }
        
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
    /// Progride para o próximo mapa (usado quando completa um boss)
    /// </summary>
    public void ProgressToNextMap(string nextSceneName)
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("Nome da próxima cena é inválido!");
            return;
        }

        // Limpa dados do mapa atual
        if (!string.IsNullOrEmpty(currentMapSceneName))
        {
            ClearMapData(currentMapSceneName);
            Debug.Log($"Dados do mapa '{currentMapSceneName}' limpos para progressão.");
        }

        // Limpa referências de nós completados
        PlayerPrefs.DeleteKey("LastCompletedNode");
        
        Debug.Log($"Progredindo para o próximo mapa: {nextSceneName}");
        
        // NOVO: Para a música atual antes de mudar de mapa
        if (AudioManager.Instance != null)
        {
            // Não precisa configurar música específica - o novo mapa terá sua própria música
            AudioManager.Instance.StopMusic(true);
        }
        
        // Carrega a próxima cena
        SceneManager.LoadScene(nextSceneName);
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