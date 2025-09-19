// MapNode.cs (Versão Atualizada com AudioClip)

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
    public AudioClip eventMusic; // NOVO: Música para a cena do evento
    
    private void Start()
    {
        UpdateVisuals();
    }
    
    private void OnMouseDown()
    {
        // A única condição para o clique é o nó estar acessível.
        if (!isLocked && !isCompleted)
        {
            Debug.Log($"Nó {gameObject.name} clicado");
            
            // NOVO: Configura a música antes de iniciar o evento
            SetupAudioForEvent();
            
            // Apenas notifica o MapManager. Toda a lógica acontecerá lá.
            FindObjectOfType<MapManager>().OnNodeClicked(this);
        }
        else
        {
            if (isLocked)
                Debug.Log($"Nó {gameObject.name} está bloqueado");
            if (isCompleted)
                Debug.Log($"Nó {gameObject.name} já foi completado");
        }
    }
    
    /// <summary>
    /// Configura o áudio que deve tocar na próxima cena de evento.
    /// </summary>
    private void SetupAudioForEvent()
    {
        if (AudioManager.Instance != null)
        {
            // 1. Pega a música que está tocando atualmente no mapa.
            AudioClip currentMapMusic = AudioManager.Instance.GetCurrentMusic();

            // 2. Avisa ao AudioManager qual música tocar no evento 
            //    e qual música salvar para voltar ao mapa depois.
            //    O próprio AudioManager cuidará de tocar a música quando a cena carregar.
            Debug.Log($"MapNode: Agendando a música '{eventMusic?.name ?? "nenhuma"}' para o próximo evento.");
            AudioManager.Instance.SetPendingEventMusic(eventMusic, currentMapMusic);
        }
        else
        {
            Debug.LogWarning("MapNode: AudioManager não encontrado!");
        }
    }

    // Tornamos esta função PÚBLICA para que o MapManager possa chamá-la.
    public void CompleteNode()
    {
        isCompleted = true;
        UpdateVisuals();
        Debug.Log($"Nó {gameObject.name} completado");
    }
    
    public void UnlockNode()
    {
        if (!isCompleted)
        {
            isLocked = false;
            UpdateVisuals();
            Debug.Log($"Nó {gameObject.name} desbloqueado");
        }
    }
    
    public void UnlockConnectedNodes()
    {
        Debug.Log($"Desbloqueando nós conectados do {gameObject.name}:");
        foreach (MapNode node in connectedNodes)
        {
            Debug.Log($"  -> Desbloqueando: {node.gameObject.name}");
            node.UnlockNode();
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
            }
            else if (isLocked) 
            {
                spriteRenderer.color = Color.red;
            }
            else 
            {
                spriteRenderer.color = Color.white;
            }
        }
    }
    
    // Métodos públicos para verificação de estado
    public bool IsCompleted() => isCompleted;
    public bool IsLocked() => isLocked;
    public List<MapNode> GetConnectedNodes() => connectedNodes;
    
    /// <summary>
    /// NOVO: Retorna a música configurada para este nó
    /// </summary>
    public AudioClip GetEventMusic() => eventMusic;
    
    public void ForceComplete()
    {
        isCompleted = true;
        isLocked = false;
        UpdateVisuals();
        Debug.Log($"Nó {gameObject.name} forçado a completar");
    }
}