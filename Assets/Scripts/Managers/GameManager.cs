// GameManager.cs (Versão com Sistema de Estados Corrigido)

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
    
    // CORREÇÃO: Dicionário melhorado para guardar o estado de MÚLTIPLOS mapas
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
            
            // NOVO: Carrega os estados salvos ao iniciar
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
    /// CORRIGIDO: Salva o estado de um mapa com validação
    /// </summary>
    public void SaveMapState(MapStateData mapData, string mapName)
    {
        if (string.IsNullOrEmpty(mapName) || mapData == null)
        {
            Debug.LogError("GameManager: Dados ou nome do mapa inválidos para salvar!");
            return;
        }

        currentMapSceneName = mapName;
    
        // CORREÇÃO: A clonagem agora é feita dentro do MapStateData para ser mais segura.
        // Ou podemos criar um novo método de clone se preferir.
        // Por simplicidade, vamos criar uma cópia aqui.
        MapStateData clonedData = new MapStateData(mapName);
        var originalDict = mapData.GetNodeStatesAsDictionary(); // Pega os dados do original
        foreach(var kvp in originalDict)
        {
            clonedData.SetNodeState(kvp.Key, kvp.Value); // Adiciona na cópia
        }
    
        savedMapStates[mapName] = clonedData;
    
        SaveMapStatesToFile(); // Isso também precisará de ajuste se quiser que funcione
    
        Debug.Log($"GameManager: Estado do mapa '{mapName}' salvo.");
    }
    
// Dentro do método GetSavedMapState em GameManager.cs

    public MapStateData GetSavedMapState(string mapName)
    {
        if (string.IsNullOrEmpty(mapName))
        {
            Debug.LogError("GameManager: Nome do mapa não pode ser vazio!");
            return null;
        }

        if (savedMapStates.TryGetValue(mapName, out MapStateData mapData))
        {
            // 1. Chamamos o novo método para obter os dados como um dicionário
            var nodeStatesDict = mapData.GetNodeStatesAsDictionary();
        
            // 2. Usamos o dicionário temporário para os logs
            Debug.Log($"GameManager: Estado do mapa '{mapName}' recuperado com {nodeStatesDict.Count} nós");
        
            // 3. O loop continua igual, mas usando o dicionário que acabamos de criar
            foreach (var kvp in nodeStatesDict)
            {
                Debug.Log($"  {kvp.Key}: {(kvp.Value ? "COMPLETADO" : "não completado")}");
            }
        
            return mapData;
        }
    
        Debug.Log($"GameManager: Nenhum estado salvo encontrado para o mapa '{mapName}'");
        return null;
    }

    /// <summary>
    /// NOVO: Salva todos os estados de mapas em arquivo
    /// </summary>
    private void SaveMapStatesToFile()
    {
        try
        {
            string dataPath = Application.persistentDataPath + "/map_states.json";
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

    /// <summary>
    /// NOVO: Carrega todos os estados de mapas do arquivo
    /// </summary>
    private void LoadAllMapStates()
    {
        try
        {
            string dataPath = Application.persistentDataPath + "/map_states.json";
            
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

    /// <summary>
    /// Limpa os dados de um mapa específico
    /// </summary>
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

        // Captura o background do MapNode
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
        // Volta à música do mapa quando retorna
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
        
        // Para a música atual antes de mudar de mapa
        if (AudioManager.Instance != null)
        {
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

    [ContextMenu("Clear All Map States")]
    public void ClearAllMapStates()
    {
        savedMapStates.Clear();
        SaveMapStatesToFile();
        Debug.Log("Todos os estados de mapas foram limpos");
    }

    [ContextMenu("Force Save Map States")]
    public void ForceSaveMapStates()
    {
        SaveMapStatesToFile();
        Debug.Log("Estados de mapas salvos manualmente");
    }
}

/// <summary>
/// NOVO: Container para serialização dos estados de mapas
/// </summary>
[System.Serializable]
public class MapStatesContainer
{
    public Dictionary<string, MapStateData> savedStates = new Dictionary<string, MapStateData>();
}