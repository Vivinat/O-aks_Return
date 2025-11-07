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
    
    private MapManager currentMapManager;
    private MapNode pendingNodeToComplete;
    
    [Header("Player Current Stats")]
    [SerializeField]
    private int playerCurrentHP = -1;
    [SerializeField]
    private int playerCurrentMP = -1;
    
    [Header("Initial Setup")]
    [Tooltip("Skill inicial que o jogador sempre começa (Ataque_Sombrio)")]
    [SerializeField] private BattleAction initialPlayerSkill;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (GetComponent<SpecificSkillModifier>() == null)
            {
                gameObject.AddComponent<SpecificSkillModifier>();
            }
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

    public void SaveMapState(MapStateData mapData, string mapName)
    {
        currentMapSceneName = mapName;
        savedMapStates[mapName] = mapData;
    }

    public MapStateData GetSavedMapState(string mapName)
    {
        if (savedMapStates.TryGetValue(mapName, out MapStateData mapData))
        {
            return mapData;
        }
        return null;
    }

    public void ClearMapData(string mapName)
    {
        if (savedMapStates.ContainsKey(mapName))
        {
            savedMapStates.Remove(mapName);
        }
    }

    public void StartEvent(EventTypeSO eventData, MapNode sourceNode = null)
    {
        CurrentEvent = eventData;

        if (eventData is DialogueEventSO dialogueEvent)
        {
            StartDialogueEvent(dialogueEvent, sourceNode);
            return;
        }
    
        if (eventData is NegotiationEventSO negotiationEvent)
        {
            currentMapManager = FindObjectOfType<MapManager>();
            pendingNodeToComplete = sourceNode;
        }

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
        else
        {
            enemiesToBattle = null;
        }

        SceneManager.LoadScene(eventData.sceneToLoad);
    }
    
    private void StartDialogueEvent(DialogueEventSO dialogueEvent, MapNode sourceNode)
    {
        currentMapManager = FindObjectOfType<MapManager>();
        pendingNodeToComplete = sourceNode;
        
        if (!dialogueEvent.HasValidDialogue())
        {
            CompleteDialogueEvent();
            return;
        }

        if (DialogueManager.Instance == null)
        {
            CompleteDialogueEvent();
            return;
        }

        if (dialogueEvent.dialogueSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(dialogueEvent.dialogueSound);
        }

        if (dialogueEvent.ShouldUseDialogueSO())
        {
            DialogueUtils.ShowDialogue(dialogueEvent.dialogueData, OnDialogueEventComplete);
        }
        else
        {
            string speaker = dialogueEvent.GetSpeakerName();
            string text = dialogueEvent.GetDialogueText();
            
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
    
    private void OnDialogueEventComplete()
    {
        CompleteDialogueEvent();
    }
    
    private void CompleteDialogueEvent()
    {
        if (pendingNodeToComplete != null)
        {
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
                }
            }
        }
        
        pendingNodeToComplete = null;
        currentMapManager = null;
        CurrentEvent = null;
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
    }

    public void ProgressToNextMap(string nextSceneName)
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            return;
        }

        if (!string.IsNullOrEmpty(currentMapSceneName))
        {
            ClearMapData(currentMapSceneName);
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
            
            if (PlayerCharacterInfo != null && PlayerCharacterInfo.battleActions != null)
            {
                PlayerCharacterInfo.battleActions.Remove(itemToRemove);
            }
        }
    }
    
    public void CompleteNegotiationEvent(NegotiationCardSO selectedCard, CardAttribute playerAttr, CardAttribute enemyAttr, int value)
    {
        if (currentMapManager != null && pendingNodeToComplete != null)
        {
            pendingNodeToComplete.CompleteNode();
            pendingNodeToComplete.UnlockConnectedNodes();
        
            var saveMethod = typeof(MapManager).GetMethod("SaveMapState", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
        
            if (saveMethod != null)
            {
                saveMethod.Invoke(currentMapManager, null);
            }
        }
    
        pendingNodeToComplete = null;
        currentMapManager = null;
        CurrentEvent = null;
    }

    public void AddBattleReward(int coins)
    {
        currencySystem.AddCoins(coins);
    }
    
    public int GetPlayerCurrentHP()
    {
        if (playerCurrentHP < 0 && PlayerCharacterInfo != null)
        {
            playerCurrentHP = PlayerCharacterInfo.maxHp;
        }
        return playerCurrentHP;
    }

    public void SetPlayerCurrentHP(int value)
    {
        if (PlayerCharacterInfo != null)
        {
            playerCurrentHP = Mathf.Clamp(value, 0, PlayerCharacterInfo.maxHp);
        }
    }

    public int GetPlayerCurrentMP()
    {
        if (playerCurrentMP < 0 && PlayerCharacterInfo != null)
        {
            playerCurrentMP = PlayerCharacterInfo.maxMp;
        }
        return playerCurrentMP;
    }

    public void SetPlayerCurrentMP(int value)
    {
        if (PlayerCharacterInfo != null)
        {
            playerCurrentMP = Mathf.Clamp(value, 0, PlayerCharacterInfo.maxMp);
        }
    }

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

    public void ResetPlayerStats()
    {
        if (PlayerCharacterInfo != null)
        {
            playerCurrentHP = PlayerCharacterInfo.maxHp;
            playerCurrentMP = PlayerCharacterInfo.maxMp;
        }
    }

    public BattleAction GetInitialPlayerSkill()
    {
        return initialPlayerSkill;
    }
}