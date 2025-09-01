// MapNode.cs (Versão Corrigida e Simplificada)

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
            // Apenas notifica o MapManager. Toda a lógica acontecerá lá.
            FindObjectOfType<MapManager>().OnNodeClicked(this);
        }
    }

    // Tornamos esta função PÚBLICA para que o MapManager possa chamá-la.
    public void CompleteNode()
    {
        isCompleted = true;
        UpdateVisuals();
    }
    
    public void UnlockNode()
    {
        if (!isCompleted)
        {
            isLocked = false;
            UpdateVisuals();
        }
    }
    
    public void UnlockConnectedNodes()
    {
        foreach (MapNode node in connectedNodes)
        {
            node.UnlockNode();
        }
    }

    private void UpdateVisuals()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (isCompleted) spriteRenderer.color = Color.gray;
        else if (isLocked) spriteRenderer.color = Color.red;
        else spriteRenderer.color = Color.white;
    }
    
    public bool IsCompleted() => isCompleted;
    public List<MapNode> GetConnectedNodes() => connectedNodes;
    public void ForceComplete()
    {
        isCompleted = true;
        isLocked = false;
        UpdateVisuals();
    }
}