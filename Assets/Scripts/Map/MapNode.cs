// MapNode.cs (Versão com Método IsLocked Público)

using UnityEngine;
using System.Collections.Generic;

public class MapNode : MonoBehaviour
{
    [SerializeField]
    private List<MapNode> connectedNodes = new List<MapNode>();

    [SerializeField]
    private bool isLocked = true;
    private bool isCompleted = false;

    public EventTypeSO eventType;
    
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
    public bool IsLocked() => isLocked; // NOVO método público
    public List<MapNode> GetConnectedNodes() => connectedNodes;
    
    public void ForceComplete()
    {
        isCompleted = true;
        isLocked = false;
        UpdateVisuals();
        Debug.Log($"Nó {gameObject.name} forçado a completar");
    }
}