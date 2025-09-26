// GameManager.cs (VERSÃO FINAL CORRIGIDA)

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
    
    private Dictionary<string, MapStateData> savedMapStates = new Dictionary<string, MapStateData>();
    private string currentMapSceneName;
    
    public static Sprite pendingBattleBackground;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePlayerActions();
            LoadAllMapStates();
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
    /// CORRIGIDO: Salva o estado de um mapa com lógica simplificada
    /// </summary>
    public void SaveMapState(MapStateData mapData, string mapName)
    {
        if (string.IsNullOrEmpty(mapName) || mapData == null)
        {
            Debug.LogError("GameManager: Dados ou nome do mapa inválidos para salvar!");
            return;
        }

        currentMapSceneName = mapName;
    
        // Lógica simplificada: Apenas armazena o objeto de dados recebido.
        savedMapStates[mapName] = mapData;
    
        SaveMapStatesToFile();
    
        Debug.Log($"GameManager: Estado do mapa '{mapName}' salvo.");
    }

    /// <summary>
    /// CORRIGIDO: Obtém o estado salvo de um mapa
    /// </summary>
    public MapStateData GetSavedMapState(string mapName)
    {
        if (string.IsNullOrEmpty(mapName))
        {
            Debug.LogError("GameManager: Nome do mapa não pode ser vazio!");
            return null;
        }

        if (savedMapStates.TryGetValue(mapName, out MapStateData mapData))
        {
            // CORREÇÃO: Acessa o dicionário diretamente pela variável 'nodeStates'.
            var nodeStatesDict = mapData.nodeStates;
        
            Debug.Log($"GameManager: Estado do mapa '{mapName}' recuperado com {nodeStatesDict.Count} nós");
        
            foreach (var kvp in nodeStatesDict)
            {
                Debug.Log($"  {kvp.Key}: {(kvp.Value ? "COMPLETADO" : "não completado")}");
            }
        
            return mapData;
        }
    
        Debug.Log($"GameManager: Nenhum estado salvo encontrado para o mapa '{mapName}'");
        return null;
    }

    private void SaveMapStatesToFile()
    {
        try
        {
            string dataPath = System.IO.Path.Combine(Application.persistentDataPath, "map_states.json");
            MapStatesContainer container = new MapStatesContainer();
            container.savedStates = savedMapStates;
            
            string jsonData = JsonUtility.ToJson(container, true);
            System.IO.File.WriteAllText(dataPath, jsonData);
            
            Debug.Log($"GameManager: Estados de mapas salvos em {dataPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"GameManager: Erro ao salvar estados: {e.Message}");
        }
    }

    private void LoadAllMapStates()
    {
        try
        {
            string dataPath = System.IO.Path.Combine(Application.persistentDataPath, "map_states.json");
            
            if (System.IO.File.Exists(dataPath))
            {
                string jsonData = System.IO.File.ReadAllText(dataPath);
                MapStatesContainer container = JsonUtility.FromJson<MapStatesContainer>(jsonData);
                
                if (container != null && container.savedStates != null)
                {
                    savedMapStates = container.savedStates;
                    Debug.Log($"GameManager: {savedMapStates.Count} estados de mapas carregados do arquivo");
                }
                else
                {
                    Debug.Log("GameManager: Arquivo de estados existe mas está vazio ou corrompido");
                    savedMapStates = new Dictionary<string, MapStateData>();
                }
            }
            else
            {
                Debug.Log("GameManager: Nenhum arquivo de estados encontrado - iniciando novo");
                savedMapStates = new Dictionary<string, MapStateData>();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"GameManager: Erro ao carregar estados: {e.Message}");
            savedMapStates = new Dictionary<string, MapStateData>();
        }
    }

    public void ClearMapData(string mapName)
    {
        if (savedMapStates.ContainsKey(mapName))
        {
            savedMapStates.Remove(mapName);
            SaveMapStatesToFile();
            Debug.Log($"Dados do mapa '{mapName}' foram limpos.");
        }
    }

    public void StartEvent(EventTypeSO eventData, MapNode sourceNode = null)
    {
        CurrentEvent = eventData;

        if (sourceNode != null)
        {
            pendingBattleBackground = sourceNode.battleBackgroundOverride;
            Debug.Log($"Background pendente '{pendingBattleBackground?.name ?? "NULL"}' foi registrado a partir do nó '{sourceNode.name}'");
        }
        else
        {
            pendingBattleBackground = null;
        }

        if (eventData is BattleEventSO battleEvent)
        {
            enemiesToBattle = battleEvent.enemies;
        }
        else if (eventData is TreasureEventSO treasureEventSo)
        {
            battleActionsPool = treasureEventSo.poolForTheMap;
        }
        else if (eventData is ShopEventSO shopEvent)
        {
            // Lógica para loja
        }
        else
        {
            enemiesToBattle = null;
        }

        SceneManager.LoadScene(eventData.sceneToLoad);
    }
    
    public void ReturnToMap()
    {
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

    public void ProgressToNextMap(string nextSceneName)
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("Nome da próxima cena é inválido!");
            return;
        }

        if (!string.IsNullOrEmpty(currentMapSceneName))
        {
            ClearMapData(currentMapSceneName);
            Debug.Log($"Dados do mapa '{currentMapSceneName}' limpos para progressão.");
        }

        PlayerPrefs.DeleteKey("LastCompletedNode");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic(true);
        }
        
        SceneManager.LoadScene(nextSceneName);
    }

    public void RemoveItemFromInventory(BattleAction itemToRemove)
    {
        if (PlayerBattleActions.Contains(itemToRemove))
        {
            PlayerBattleActions.Remove(itemToRemove);
            
            if (PlayerCharacterInfo?.battleActions != null)
            {
                PlayerCharacterInfo.battleActions.Remove(itemToRemove);
            }
            
            Debug.Log($"Item '{itemToRemove.actionName}' removido do inventário.");
        }
    }

    public void AddBattleReward(int coins)
    {
        currencySystem.AddCoins(coins);
    }

    [ContextMenu("Clear All Map States")]
    public void ClearAllMapStates()
    {
        savedMapStates.Clear();
        SaveMapStatesToFile();
        Debug.Log("Todos os estados de mapas foram limpos.");
    }

    [ContextMenu("Force Save Map States")]
    public void ForceSaveMapStates()
    {
        SaveMapStatesToFile();
    }
}


[System.Serializable]
public class MapStatesContainer : ISerializationCallbackReceiver
{
    public Dictionary<string, MapStateData> savedStates = new Dictionary<string, MapStateData>();

    [SerializeField] private List<string> mapNames = new List<string>();
    [SerializeField] private List<MapStateData> mapDataList = new List<MapStateData>();

    public void OnBeforeSerialize()
    {
        mapNames.Clear();
        mapDataList.Clear();
        foreach (var kvp in savedStates)
        {
            mapNames.Add(kvp.Key);
            mapDataList.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        savedStates = new Dictionary<string, MapStateData>();
        for (int i = 0; i < mapNames.Count && i < mapDataList.Count; i++)
        {
            savedStates[mapNames[i]] = mapDataList[i];
        }
    }
}