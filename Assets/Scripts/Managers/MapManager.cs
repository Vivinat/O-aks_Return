// MapManager.cs (Versão com Lógica Centralizada)

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    [SerializeField]
    private List<MapNode> allNodes = new List<MapNode>();
    [SerializeField]
    private GameObject linePrefab;
    [SerializeField]
    private Transform lineContainer;
    
    void Start()
    {
        if (lineContainer == null)
        {
            lineContainer = new GameObject("LineContainer").transform;
            lineContainer.SetParent(this.transform);
        }
        LoadMapState();
        DrawConnections();
    }

    /// <summary>
    /// Esta função agora controla toda a sequência de eventos após um clique.
    /// </summary>
    public void OnNodeClicked(MapNode clickedNode)
    {
        // --- ORDEM CORRETA ---

        // 1. Primeiro, atualiza o estado LÓGICO do mapa.
        clickedNode.CompleteNode();
        clickedNode.UnlockConnectedNodes();
        
        // 2. AGORA, com o estado atualizado, salva tudo.
        SaveMapState();

        // 3. Por último, inicia o evento e a transição de cena.
        GameManager.Instance.StartEvent(clickedNode.eventType);
    }
    
    // O resto do script (DrawConnections, SaveMapState, LoadMapState) permanece exatamente o mesmo.
    // ...
    // (Cole o resto do seu código do MapManager aqui, ele já está correto)
    private void DrawConnections()
    {
        foreach (Transform child in lineContainer) Destroy(child.gameObject);
        foreach (var node in allNodes)
        {
            foreach (var connectedNode in node.GetConnectedNodes())
            {
                GameObject lineObj = Instantiate(linePrefab, lineContainer);
                LineRenderer lr = lineObj.GetComponent<LineRenderer>();
                lr.SetPosition(0, node.transform.position);
                lr.SetPosition(1, connectedNode.transform.position);
            }
        }
    }

    private void SaveMapState()
    {
        MapStateData mapData = new MapStateData();
        foreach (var node in allNodes)
        {
            mapData.nodeStates[node.gameObject.name] = node.IsCompleted();
        }
        string mapName = SceneManager.GetActiveScene().name;
        GameManager.Instance.SaveMapState(mapData, mapName);
    }

    private void LoadMapState()
    {
        string mapName = SceneManager.GetActiveScene().name;
        MapStateData loadedData = GameManager.Instance.GetSavedMapState(mapName);

        if (loadedData == null) return;

        foreach (var node in allNodes)
        {
            if (loadedData.nodeStates.TryGetValue(node.gameObject.name, out bool isCompleted) && isCompleted)
            {
                node.ForceComplete();
            }
        }
        
        foreach (var node in allNodes)
        {
            if (node.IsCompleted())
            {
                node.UnlockConnectedNodes();
            }
        }
    }
}