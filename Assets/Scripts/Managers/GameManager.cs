// GameManager.cs (Versão Atualizada com Suporte a Eventos de Diálogo)

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
    
    // NOVO: Sistema para eventos de diálogo
    private MapManager currentMapManager;
    private MapNode pendingNodeToComplete;

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

        // NOVO: Verifica se é um evento de diálogo
        if (eventData is DialogueEventSO dialogueEvent)
        {
            StartDialogueEvent(dialogueEvent, sourceNode);
            return; // Não carrega nova cena para eventos de diálogo
        }

        // LINHA-CHAVE: AQUI NÓS CAPTURAMOS O BACKGROUND DO MAPNODE (para outros eventos)
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

        // O resto do código para outros tipos de eventos...
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
    
    /// <summary>
    /// NOVO: Inicia um evento de diálogo diretamente no mapa atual
    /// </summary>
    private void StartDialogueEvent(DialogueEventSO dialogueEvent, MapNode sourceNode)
    {
        Debug.Log($"Iniciando evento de diálogo: '{dialogueEvent.name}'");
        
        // Salva referências para completar o nó após o diálogo
        currentMapManager = FindObjectOfType<MapManager>();
        pendingNodeToComplete = sourceNode;
        
        if (!dialogueEvent.HasValidDialogue())
        {
            Debug.LogError($"Evento de diálogo '{dialogueEvent.name}' não tem conteúdo válido!");
            CompleteDialogueEvent(); // Completa mesmo assim para não travar o jogo
            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager não encontrado! Não é possível executar o evento de diálogo.");
            CompleteDialogueEvent();
            return;
        }

        // Toca som específico se configurado
        if (dialogueEvent.dialogueSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(dialogueEvent.dialogueSound);
        }

        // Decide como iniciar o diálogo baseado na configuração
        if (dialogueEvent.ShouldUseDialogueSO())
        {
            // Usa o DialogueSO complexo
            Debug.Log("Usando DialogueSO para o evento de diálogo");
            DialogueUtils.ShowDialogue(dialogueEvent.dialogueData, OnDialogueEventComplete);
        }
        else
        {
            // Usa o texto simples
            string speaker = dialogueEvent.GetSpeakerName();
            string text = dialogueEvent.GetDialogueText();
            
            Debug.Log($"Usando texto simples para o evento de diálogo: Speaker='{speaker}', Text='{text.Substring(0, Mathf.Min(50, text.Length))}...'");
            
            if (string.IsNullOrEmpty(speaker))
            {
                DialogueUtils.ShowNarration(text, OnDialogueEventComplete);
            }
            else
            {
                DialogueUtils.ShowSimpleDialogue(speaker, text, OnDialogueEventComplete);
            }
        }
    }
    
    /// <summary>
    /// NOVO: Callback chamado quando o evento de diálogo termina
    /// </summary>
    private void OnDialogueEventComplete()
    {
        Debug.Log("Evento de diálogo completado");
        CompleteDialogueEvent();
    }
    
    /// <summary>
    /// NOVO: Completa o evento de diálogo e marca o nó como terminado
    /// </summary>
    private void CompleteDialogueEvent()
    {
        if (pendingNodeToComplete != null)
        {
            Debug.Log($"Completando nó de diálogo: {pendingNodeToComplete.gameObject.name}");
            
            // Marca o nó como completado
            pendingNodeToComplete.CompleteNode();
            pendingNodeToComplete.UnlockConnectedNodes();
            
            // Salva o estado do mapa
            if (currentMapManager != null)
            {
                // Força o MapManager a salvar o estado atualizado
                var saveMethod = typeof(MapManager).GetMethod("SaveMapState", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                
                if (saveMethod != null)
                {
                    saveMethod.Invoke(currentMapManager, null);
                    Debug.Log("Estado do mapa salvo após completar evento de diálogo");
                }
            }
        }
        
        // Limpa as referências
        pendingNodeToComplete = null;
        currentMapManager = null;
        CurrentEvent = null;
        
        Debug.Log("Evento de diálogo finalizado com sucesso");
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