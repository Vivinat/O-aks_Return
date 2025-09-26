// MapNode.cs (VERSÃO CORRIGIDA)

using UnityEngine;
using System.Collections.Generic;

public class MapNode : MonoBehaviour
{
    [SerializeField]
    private List<MapNode> connectedNodes = new List<MapNode>();

    [SerializeField]
    private bool isLocked = true;
    private bool isCompleted = false;

    [Header("Event Configuration")]
    public EventTypeSO eventType;
    
    [Header("Audio Configuration")]
    [Tooltip("Música que tocará na cena do evento. Se null, mantém a música atual.")]
    public AudioClip eventMusic;
    
    [Header("Battle Visual Configuration")]
    [Tooltip("Sprite de fundo específico para batalhas deste nó (sobrescreve o do BattleEventSO)")]
    public Sprite battleBackgroundOverride;
    
    private void Start()
    {
        UpdateVisuals();
        
        // CORREÇÃO: Verifica se deve estar desbloqueado no início
        // Se não há nós conectados PARA ESTE nó, ele deve começar desbloqueado
        if (IsInitialNode())
        {
            Debug.Log($"Nó {gameObject.name} detectado como inicial - desbloqueando");
            isLocked = false;
            UpdateVisuals();
        }
    }
    
    /// <summary>
    /// CORREÇÃO: Determina se este é um nó inicial (não tem predecessores)
    /// </summary>
    private bool IsInitialNode()
    {
        // Se este nó não é referenciado por nenhum outro nó, é inicial
        MapNode[] allNodes = FindObjectsOfType<MapNode>();
        
        foreach (MapNode node in allNodes)
        {
            if (node != this && node.connectedNodes.Contains(this))
            {
                return false; // Este nó é conectado por outro, não é inicial
            }
        }
        
        // CORREÇÃO ADICIONAL: Também verifica se não está completado E tem eventType
        return eventType != null;
    }
    
    private void OnMouseDown()
    {
        Debug.Log($"[MapNode] Clique detectado no nó: {gameObject.name}");
        Debug.Log($"[MapNode] Estado - Locked: {isLocked}, Completed: {isCompleted}");
        
        // CORREÇÃO: Permite clicar se não está travado E não está completado
        if (!isLocked && !isCompleted)
        {
            Debug.Log($"[MapNode] Nó {gameObject.name} clicado - iniciando evento");
            
            SetupAudioForEvent();
            
            // CORREÇÃO: Verifica se existe MapManager antes de chamar
            MapManager mapManager = FindObjectOfType<MapManager>();
            if (mapManager != null)
            {
                mapManager.OnNodeClicked(this);
            }
            else
            {
                Debug.LogError($"[MapNode] MapManager não encontrado! Não é possível processar clique do nó {gameObject.name}");
            }
        }
        else
        {
            if (isLocked)
                Debug.Log($"[MapNode] Nó {gameObject.name} está bloqueado");
            if (isCompleted)
                Debug.Log($"[MapNode] Nó {gameObject.name} já foi completado");
        }
    }
    
    private void SetupAudioForEvent()
    {
        if (AudioManager.Instance != null)
        {
            AudioClip currentMapMusic = AudioManager.Instance.GetCurrentMusic();
            Debug.Log($"[MapNode] Agendando música '{eventMusic?.name ?? "nenhuma"}' para o próximo evento.");
            AudioManager.Instance.SetPendingEventMusic(eventMusic, currentMapMusic);
        }
        else
        {
            Debug.LogWarning("[MapNode] AudioManager não encontrado!");
        }
    }

    // CORREÇÃO: Método público para completar nó com mais logs
    public void CompleteNode()
    {
        Debug.Log($"[MapNode] Completando nó: {gameObject.name}");
        isCompleted = true;
        UpdateVisuals();
        
        // CORREÇÃO: Força o salvamento do estado após completar
        MapManager mapManager = FindObjectOfType<MapManager>();
        if (mapManager != null)
        {
            // Chama o método público de salvamento se existir
            mapManager.SendMessage("SaveMapState", SendMessageOptions.DontRequireReceiver);
        }
    }
    
    public void UnlockNode()
    {
        // CORREÇÃO: Só desbloqueia se não está completado
        if (!isCompleted)
        {
            Debug.Log($"[MapNode] Desbloqueando nó: {gameObject.name}");
            isLocked = false;
            UpdateVisuals();
        }
        else
        {
            Debug.Log($"[MapNode] Tentativa de desbloquear nó já completado: {gameObject.name}");
        }
    }
    
    // CORREÇÃO: Método melhorado para desbloquear nós conectados
    public void UnlockConnectedNodes()
    {
        Debug.Log($"[MapNode] Desbloqueando {connectedNodes.Count} nós conectados do {gameObject.name}:");
        
        foreach (MapNode node in connectedNodes)
        {
            if (node != null)
            {
                Debug.Log($"[MapNode]   -> Desbloqueando: {node.gameObject.name}");
                node.UnlockNode();
            }
            else
            {
                Debug.LogWarning($"[MapNode] Nó conectado nulo encontrado em {gameObject.name}!");
            }
        }
        
        // CORREÇÃO: Força atualização visual de todos os nós após desbloqueio
        StartCoroutine(RefreshAllNodesAfterDelay());
    }
    
    /// <summary>
    /// CORREÇÃO: Força atualização visual de todos os nós após um pequeno delay
    /// </summary>
    private System.Collections.IEnumerator RefreshAllNodesAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        
        MapNode[] allNodes = FindObjectsOfType<MapNode>();
        foreach (MapNode node in allNodes)
        {
            if (node != null)
            {
                node.UpdateVisuals();
            }
        }
        
        Debug.Log($"[MapNode] Todos os nós visuais atualizados após desbloqueio de {gameObject.name}");
    }

    private void UpdateVisuals()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            if (isCompleted) 
            {
                spriteRenderer.color = Color.gray;
                Debug.Log($"[MapNode] {gameObject.name} visual: COMPLETADO (cinza)");
            }
            else if (isLocked) 
            {
                spriteRenderer.color = Color.red;
                Debug.Log($"[MapNode] {gameObject.name} visual: BLOQUEADO (vermelho)");
            }
            else 
            {
                spriteRenderer.color = Color.white;
                Debug.Log($"[MapNode] {gameObject.name} visual: DISPONÍVEL (branco)");
            }
        }
    }
    
    // Métodos públicos para verificação de estado
    public bool IsCompleted() => isCompleted;
    public bool IsLocked() => isLocked;
    public List<MapNode> GetConnectedNodes() => connectedNodes;
    public AudioClip GetEventMusic() => eventMusic;
    
    // CORREÇÃO: Método para forçar completar com logs detalhados
    public void ForceComplete()
    {
        Debug.Log($"[MapNode] Forçando completar nó: {gameObject.name}");
        isCompleted = true;
        isLocked = false; // CORREÇÃO: Garante que não fica travado
        UpdateVisuals();
    }
    
    // CORREÇÃO: Método para resetar nó (útil para debug)
    [ContextMenu("Reset Node")]
    public void ResetNode()
    {
        isCompleted = false;
        isLocked = !IsInitialNode(); // Só trava se não for inicial
        UpdateVisuals();
        Debug.Log($"[MapNode] Nó {gameObject.name} resetado");
    }
    
    // CORREÇÃO: Método de debug para verificar estado
    [ContextMenu("Debug Node State")]
    public void DebugNodeState()
    {
        Debug.Log($"=== DEBUG DO NÓ {gameObject.name} ===");
        Debug.Log($"Locked: {isLocked}");
        Debug.Log($"Completed: {isCompleted}");
        Debug.Log($"EventType: {(eventType != null ? eventType.name : "NULL")}");
        Debug.Log($"Nós conectados: {connectedNodes.Count}");
        
        for (int i = 0; i < connectedNodes.Count; i++)
        {
            if (connectedNodes[i] != null)
            {
                Debug.Log($"  [{i}] {connectedNodes[i].gameObject.name} - Locked: {connectedNodes[i].isLocked}, Completed: {connectedNodes[i].isCompleted}");
            }
            else
            {
                Debug.Log($"  [{i}] NULL");
            }
        }
        Debug.Log($"===================================");
    }
}