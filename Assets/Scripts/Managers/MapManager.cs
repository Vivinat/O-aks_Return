// MapManager.cs (Versão com Bug Corrigido)

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
    
    // Variável para rastrear qual nó foi completado mais recentemente
    private string lastCompletedNodeName;
    
    void Start()
    {
        if (lineContainer == null)
        {
            lineContainer = new GameObject("LineContainer").transform;
            lineContainer.SetParent(this.transform);
        }
        
        // Pega o nome do último nó completado (se houver)
        lastCompletedNodeName = PlayerPrefs.GetString("LastCompletedNode", "");
        
        LoadMapState();
        DrawConnections();
    }

    /// <summary>
    /// Esta função agora controla toda a sequência de eventos após um clique.
    /// </summary>
    public void OnNodeClicked(MapNode clickedNode)
    {
        // --- ORDEM CORRETA ---

        // 1. Salva qual nó está sendo completado
        lastCompletedNodeName = clickedNode.gameObject.name;
        PlayerPrefs.SetString("LastCompletedNode", lastCompletedNodeName);

        // 2. Atualiza o estado LÓGICO do mapa.
        clickedNode.CompleteNode();
        clickedNode.UnlockConnectedNodes();
        
        // 3. AGORA, com o estado atualizado, salva tudo.
        SaveMapState();

        // 4. Por último, inicia o evento e a transição de cena.
        GameManager.Instance.StartEvent(clickedNode.eventType);
    }
    
    private void DrawConnections()
    {
        // Limpa linhas antigas
        foreach (Transform child in lineContainer) 
            Destroy(child.gameObject);
            
        // Desenha novas linhas
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
        
        Debug.Log($"Estado do mapa salvo. Nós completados: {mapData.nodeStates.Count}");
    }

    private void LoadMapState()
    {
        string mapName = SceneManager.GetActiveScene().name;
        MapStateData loadedData = GameManager.Instance.GetSavedMapState(mapName);

        if (loadedData == null) 
        {
            Debug.Log("Nenhum estado salvo encontrado para este mapa.");
            return;
        }

        Debug.Log("Carregando estado do mapa...");

        // *** CORREÇÃO DO BUG ***
        // Primeiro, restaura o estado de completado de todos os nós
        foreach (var node in allNodes)
        {
            if (loadedData.nodeStates.TryGetValue(node.gameObject.name, out bool isCompleted) && isCompleted)
            {
                node.ForceComplete();
                Debug.Log($"Nó {node.gameObject.name} marcado como completado");
            }
        }
        
        // *** NOVA LÓGICA ***
        // Se estamos voltando de um evento (há um lastCompletedNodeName), 
        // apenas desbloqueamos os nós conectados DESSE nó específico
        if (!string.IsNullOrEmpty(lastCompletedNodeName))
        {
            MapNode lastCompletedNode = allNodes.Find(n => n.gameObject.name == lastCompletedNodeName);
            if (lastCompletedNode != null && lastCompletedNode.IsCompleted())
            {
                Debug.Log($"Desbloqueando nós conectados apenas do nó recém-completado: {lastCompletedNodeName}");
                lastCompletedNode.UnlockConnectedNodes();
                
                // Lista os nós que foram desbloqueados para debug
                foreach (var connectedNode in lastCompletedNode.GetConnectedNodes())
                {
                    Debug.Log($"  -> Desbloqueado: {connectedNode.gameObject.name}");
                }
            }
            
            // Limpa o registro do último nó completado
            lastCompletedNodeName = "";
            PlayerPrefs.DeleteKey("LastCompletedNode");
        }
        else
        {
            // *** LÓGICA ORIGINAL PARA CARREGAMENTO INICIAL ***
            // Se não há um nó recém-completado (ex: primeira vez carregando o mapa),
            // desbloqueamos os conectados de todos os nós completados
            Debug.Log("Carregamento inicial - desbloqueando todos os nós conectados");
            foreach (var node in allNodes)
            {
                if (node.IsCompleted())
                {
                    node.UnlockConnectedNodes();
                }
            }
        }
        
        // Debug final - mostra quais nós estão acessíveis
        var accessibleNodes = allNodes.FindAll(n => !n.IsCompleted() && !n.GetComponent<MapNode>().GetType().GetField("isLocked", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(n).Equals(true) == true);
        Debug.Log($"Nós acessíveis após carregamento: {accessibleNodes.Count}");
    }
}