// GameManager.cs (Versão Completamente Corrigida)

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
    
    // Sistema para eventos de diálogo
    private MapManager currentMapManager;
    private MapNode pendingNodeToComplete;
    
    [Header("Player Current Stats")]
    [SerializeField]
    private int playerCurrentHP = -1; // -1 significa "não inicializado"
    [SerializeField]
    private int playerCurrentMP = -1; // -1 significa "não inicializado"
    
    [Header("Initial Setup")]
    [Tooltip("Skill inicial que o jogador sempre começa (Ataque_Sombrio)")]
    [SerializeField] private BattleAction initialPlayerSkill;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Aplica modificadores do DifficultySystem ao personagem
            if (DifficultySystem.Instance != null && PlayerCharacterInfo != null)
            {
                DifficultySystem.Instance.ApplyToPlayer(PlayerCharacterInfo);
            }
            
            // Certifica que SpecificSkillModifier existe
            if (GetComponent<SpecificSkillModifier>() == null)
            {
                gameObject.AddComponent<SpecificSkillModifier>();
            }
            
            // Inicializa ações do jogador
            InitializePlayerActions();
            
            // NOVO: Inicializa cópias runtime para tooltips
            InitializePlayerActionCopies();
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
    /// NOVO: Inicializa as cópias runtime das ações do jogador
    /// Estas cópias são APENAS PARA CONSULTA pelos tooltips
    /// </summary>
    private void InitializePlayerActionCopies()
    {
        if (PlayerCharacterInfo == null || PlayerCharacterInfo.battleActions == null)
        {
            Debug.LogWarning("Não foi possível inicializar cópias - dados do jogador não encontrados");
            return;
        }
        
        // Certifica que o BattleActionRuntimeCopies existe
        if (BattleActionRuntimeCopies.Instance == null)
        {
            GameObject runtimeCopiesObj = new GameObject("BattleActionRuntimeCopies");
            runtimeCopiesObj.AddComponent<BattleActionRuntimeCopies>();
            Debug.Log("BattleActionRuntimeCopies criado automaticamente");
        }
        
        // Inicializa as cópias com as ações do jogador
        BattleActionRuntimeCopies.Instance.InitializePlayerActions(PlayerCharacterInfo.battleActions);
        
        Debug.Log($"✓ Cópias runtime inicializadas com {PlayerCharacterInfo.battleActions.Count} ações");
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

        // Para eventos de diálogo
        if (eventData is DialogueEventSO dialogueEvent)
        {
            StartDialogueEvent(dialogueEvent, sourceNode);
            return;
        }
    
        // Para eventos de negociação
        if (eventData is NegotiationEventSO negotiationEvent)
        {
            // Salva referências para completar o nó após a negociação
            currentMapManager = FindObjectOfType<MapManager>();
            pendingNodeToComplete = sourceNode;
        
            Debug.Log("Iniciando evento de negociação");
        }

        // Captura background do MapNode
        if (sourceNode != null)
        {
            pendingBattleBackground = sourceNode.battleBackgroundOverride;
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
            Debug.Log($"Iniciando loja com {shopEvent.actionsForSale?.Count ?? 0} itens");
        }
        else
        {
            enemiesToBattle = null;
        }

        SceneManager.LoadScene(eventData.sceneToLoad);
    }
    
    /// <summary>
    /// Inicia um evento de diálogo diretamente no mapa atual
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
            CompleteDialogueEvent();
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
            Debug.Log("Usando DialogueSO para o evento de diálogo");
            DialogueUtils.ShowDialogue(dialogueEvent.dialogueData, OnDialogueEventComplete);
        }
        else
        {
            string speaker = dialogueEvent.GetSpeakerName();
            string text = dialogueEvent.GetDialogueText();
            
            Debug.Log($"Usando texto simples para o evento de diálogo");
            
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
    /// Callback chamado quando o evento de diálogo termina
    /// </summary>
    private void OnDialogueEventComplete()
    {
        Debug.Log("Evento de diálogo completado");
        CompleteDialogueEvent();
    }
    
    /// <summary>
    /// Completa o evento de diálogo e marca o nó como terminado
    /// </summary>
    private void CompleteDialogueEvent()
    {
        if (pendingNodeToComplete != null)
        {
            Debug.Log($"Completando nó de diálogo: {pendingNodeToComplete.gameObject.name}");
            
            pendingNodeToComplete.CompleteNode();
            pendingNodeToComplete.UnlockConnectedNodes();
            
            if (currentMapManager != null)
            {
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
        
        pendingNodeToComplete = null;
        currentMapManager = null;
        CurrentEvent = null;
        
        Debug.Log("Evento de diálogo finalizado com sucesso");
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

        if (!string.IsNullOrEmpty(currentMapSceneName))
        {
            ClearMapData(currentMapSceneName);
            Debug.Log($"Dados do mapa '{currentMapSceneName}' limpos para progressão.");
        }

        PlayerPrefs.DeleteKey("LastCompletedNode");
        
        Debug.Log($"Progredindo para o próximo mapa: {nextSceneName}");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic(true);
        }
        
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
            
            if (PlayerCharacterInfo != null && PlayerCharacterInfo.battleActions != null)
            {
                PlayerCharacterInfo.battleActions.Remove(itemToRemove);
            }
            
            Debug.Log($"Item '{itemToRemove.actionName}' removido do inventário (usos esgotados)");
        }
    }
    
    /// <summary>
    /// Completa evento de negociação (chamado pelo NegotiationManager)
    /// </summary>
    public void CompleteNegotiationEvent()
    {
        Debug.Log("=== COMPLETANDO EVENTO DE NEGOCIAÇÃO ===");
        
        // Marca o nó como completado
        if (currentMapManager != null && pendingNodeToComplete != null)
        {
            Debug.Log($"✅ Completando nó: {pendingNodeToComplete.gameObject.name}");
        
            pendingNodeToComplete.CompleteNode();
            pendingNodeToComplete.UnlockConnectedNodes();
        
            // Salva estado do mapa
            var saveMethod = typeof(MapManager).GetMethod("SaveMapState", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
        
            if (saveMethod != null)
            {
                saveMethod.Invoke(currentMapManager, null);
            }
        }
    
        // Limpa referências
        pendingNodeToComplete = null;
        currentMapManager = null;
        CurrentEvent = null;
    }

    /// <summary>
    /// Adiciona moedas ao jogador (útil para recompensas de batalha)
    /// </summary>
    public void AddBattleReward(int coins)
    {
        currencySystem.AddCoins(coins);
        Debug.Log($"Recompensa de batalha: {coins} moedas");
    }
    
    /// <summary>
    /// Retorna o HP atual do jogador (ou HP máximo se não inicializado)
    /// </summary>
    public int GetPlayerCurrentHP()
    {
        if (playerCurrentHP < 0 && PlayerCharacterInfo != null)
        {
            playerCurrentHP = PlayerCharacterInfo.maxHp;
        }
        return playerCurrentHP;
    }

    /// <summary>
    /// Define o HP atual do jogador
    /// </summary>
    public void SetPlayerCurrentHP(int value)
    {
        if (PlayerCharacterInfo != null)
        {
            playerCurrentHP = Mathf.Clamp(value, 0, PlayerCharacterInfo.maxHp);
            Debug.Log($"HP do jogador atualizado: {playerCurrentHP}/{PlayerCharacterInfo.maxHp}");
        }
    }

    /// <summary>
    /// Retorna o MP atual do jogador (ou MP máximo se não inicializado)
    /// </summary>
    public int GetPlayerCurrentMP()
    {
        if (playerCurrentMP < 0 && PlayerCharacterInfo != null)
        {
            playerCurrentMP = PlayerCharacterInfo.maxMp;
        }
        return playerCurrentMP;
    }

    /// <summary>
    /// Define o MP atual do jogador
    /// </summary>
    public void SetPlayerCurrentMP(int value)
    {
        if (PlayerCharacterInfo != null)
        {
            playerCurrentMP = Mathf.Clamp(value, 0, PlayerCharacterInfo.maxMp);
            Debug.Log($"MP do jogador atualizado: {playerCurrentMP}/{PlayerCharacterInfo.maxMp}");
        }
    }

    /// <summary>
    /// Inicializa HP/MP atuais com base nos valores máximos
    /// </summary>
    public void InitializePlayerStats()
    {
        if (PlayerCharacterInfo != null)
        {
            if (playerCurrentHP < 0)
            {
                playerCurrentHP = PlayerCharacterInfo.maxHp;
            }
            if (playerCurrentMP < 0)
            {
                playerCurrentMP = PlayerCharacterInfo.maxMp;
            }
        }
    }

    /// <summary>
    /// Reseta HP/MP para valores máximos
    /// </summary>
    public void ResetPlayerStats()
    {
        if (PlayerCharacterInfo != null)
        {
            playerCurrentHP = PlayerCharacterInfo.maxHp;
            playerCurrentMP = PlayerCharacterInfo.maxMp;
            Debug.Log("Stats do jogador resetados para máximos");
        }
    }

    public BattleAction GetInitialPlayerSkill()
    {
        return initialPlayerSkill;
    }
}