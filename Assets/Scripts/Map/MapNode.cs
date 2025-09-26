// MapNode.cs (Versão Corrigida)

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
    }
    
    private void OnMouseDown()
    {
        // CORREÇÃO 1: Validações mais robustas
        MapManager mapManager = FindObjectOfType<MapManager>();
        if (mapManager == null)
        {
            Debug.LogError("MapManager não encontrado na cena!");
            return;
        }

        // CORREÇÃO 2: Só permite clique se o nó estiver acessível E não completado
        if (!isLocked && !isCompleted)
        {
            Debug.Log($"Nó {gameObject.name} clicado - Estado: Bloqueado={isLocked}, Completado={isCompleted}");
            
            // Configura a música antes de iniciar o evento
            SetupAudioForEvent();
            
            // Notifica o MapManager
            mapManager.OnNodeClicked(this);
        }
        else
        {
            // CORREÇÃO 3: Logs mais informativos
            if (isLocked)
            {
                Debug.Log($"Nó {gameObject.name} está BLOQUEADO - clique ignorado");
            }
            if (isCompleted)
            {
                Debug.Log($"Nó {gameObject.name} já foi COMPLETADO - clique ignorado");
            }
        }
    }
    
    /// <summary>
    /// Configura o áudio que deve tocar na próxima cena de evento.
    /// </summary>
    private void SetupAudioForEvent()
    {
        if (AudioManager.Instance != null)
        {
            AudioClip currentMapMusic = AudioManager.Instance.GetCurrentMusic();
            Debug.Log($"MapNode: Agendando a música '{eventMusic?.name ?? "nenhuma"}' para o próximo evento.");
            AudioManager.Instance.SetPendingEventMusic(eventMusic, currentMapMusic);
        }
        else
        {
            Debug.LogWarning("MapNode: AudioManager não encontrado!");
        }
    }

    // CORREÇÃO 4: Método CompleteNode mais robusto
    public void CompleteNode()
    {
        if (isCompleted)
        {
            Debug.LogWarning($"Nó {gameObject.name} já estava completado!");
            return;
        }

        isCompleted = true;
        UpdateVisuals();
        Debug.Log($"Nó {gameObject.name} COMPLETADO");
        
        // CORREÇÃO 5: Desbloqueia os nós conectados IMEDIATAMENTE após completar
        UnlockConnectedNodes();
    }
    
    public void UnlockNode()
    {
        // CORREÇÃO 6: Só desbloqueia se não estiver completado
        if (!isCompleted && isLocked)
        {
            isLocked = false;
            UpdateVisuals();
            Debug.Log($"Nó {gameObject.name} DESBLOQUEADO");
        }
    }
    
    public void UnlockConnectedNodes()
    {
        Debug.Log($"Desbloqueando {connectedNodes.Count} nós conectados do {gameObject.name}:");
        
        foreach (MapNode node in connectedNodes)
        {
            if (node != null) // CORREÇÃO 7: Verificação de null
            {
                Debug.Log($"  -> Desbloqueando: {node.gameObject.name}");
                node.UnlockNode();
            }
            else
            {
                Debug.LogWarning($"Nó conectado NULL encontrado em {gameObject.name}!");
            }
        }
    }

    private void UpdateVisuals()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            if (isCompleted) 
            {
                spriteRenderer.color = Color.gray;
                Debug.Log($"Visual do nó {gameObject.name}: CINZA (completado)");
            }
            else if (isLocked) 
            {
                spriteRenderer.color = Color.red;
                Debug.Log($"Visual do nó {gameObject.name}: VERMELHO (bloqueado)");
            }
            else 
            {
                spriteRenderer.color = Color.white;
                Debug.Log($"Visual do nó {gameObject.name}: BRANCO (disponível)");
            }
        }
        else
        {
            Debug.LogWarning($"Nó {gameObject.name} não tem SpriteRenderer!");
        }
    }
    
    // Métodos públicos para verificação de estado
    public bool IsCompleted() => isCompleted;
    public bool IsLocked() => isLocked;
    public List<MapNode> GetConnectedNodes() => connectedNodes;
    
    /// <summary>
    /// Retorna a música configurada para este nó
    /// </summary>
    public AudioClip GetEventMusic() => eventMusic;
    
    // CORREÇÃO 8: ForceComplete mais robusto
    public void ForceComplete()
    {
        bool wasCompleted = isCompleted;
        isCompleted = true;
        isLocked = false;
        UpdateVisuals();
        
        if (!wasCompleted)
        {
            Debug.Log($"Nó {gameObject.name} FORÇADO a completar");
        }
    }

    // CORREÇÃO 9: Método para debug
    [ContextMenu("Debug Node State")]
    public void DebugNodeState()
    {
        Debug.Log($"=== DEBUG NÓ {gameObject.name} ===");
        Debug.Log($"Bloqueado: {isLocked}");
        Debug.Log($"Completado: {isCompleted}");
        Debug.Log($"Nós conectados: {connectedNodes.Count}");
        
        for (int i = 0; i < connectedNodes.Count; i++)
        {
            if (connectedNodes[i] != null)
            {
                Debug.Log($"  [{i}] {connectedNodes[i].gameObject.name}");
            }
            else
            {
                Debug.Log($"  [{i}] NULL!");
            }
        }
    }

    // CORREÇÃO 10: Método para forçar reset (útil para testes)
    [ContextMenu("Reset Node")]
    public void ResetNode()
    {
        isCompleted = false;
        isLocked = true;
        UpdateVisuals();
        Debug.Log($"Nó {gameObject.name} foi RESETADO");
    }

    // CORREÇÃO 11: Validação no Editor
    private void OnValidate()
    {
        // Remove referências null da lista de nós conectados
        if (connectedNodes != null)
        {
            for (int i = connectedNodes.Count - 1; i >= 0; i--)
            {
                if (connectedNodes[i] == null)
                {
                    connectedNodes.RemoveAt(i);
                }
            }
        }

        // Verifica se tem EventType
        if (eventType == null)
        {
            Debug.LogWarning($"Nó {gameObject.name} não tem EventTypeSO configurado!");
        }
    }
}