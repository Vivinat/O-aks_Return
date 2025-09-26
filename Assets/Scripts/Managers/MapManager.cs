// MapManager.cs (VERSÃO CORRIGIDA)

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
    
    [SerializeField]
    private MapCameraManager cameraController;
    
    private string lastCompletedNodeName;
    
    void Start()
    {
        Debug.Log("[MapManager] Iniciando MapManager...");
        
        if (lineContainer == null)
        {
            lineContainer = new GameObject("LineContainer").transform;
            lineContainer.SetParent(this.transform);
        }
        
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<MapCameraManager>();
        }
        
        // CORREÇÃO: Mudança na ordem - carrega estado ANTES de desenhar conexões
        LoadMapState();
        DrawConnections();
        SetupCameraBounds();
        
        // CORREÇÃO: Verifica estado de todos os nós após carregar
        DebugAllNodesState();
    }

    private void SetupCameraBounds()
    {
        if (cameraController == null || allNodes.Count == 0) return;
        
        Vector2 minBounds = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 maxBounds = new Vector2(float.MinValue, float.MinValue);
        
        foreach (MapNode node in allNodes)
        {
            Vector3 nodePos = node.transform.position;
            minBounds.x = Mathf.Min(minBounds.x, nodePos.x);
            minBounds.y = Mathf.Min(minBounds.y, nodePos.y);
            maxBounds.x = Mathf.Max(maxBounds.x, nodePos.x);
            maxBounds.y = Mathf.Max(maxBounds.y, nodePos.y);
        }
        
        float margin = 3f;
        minBounds -= Vector2.one * margin;
        maxBounds += Vector2.one * margin;
        
        cameraController.SetCameraBounds(minBounds, maxBounds);
        Debug.Log($"[MapManager] Limites da câmera configurados: Min{minBounds}, Max{maxBounds}");
    }

    public void OnNodeClicked(MapNode clickedNode)
    {
        Debug.Log($"[MapManager] Processando clique no nó: {clickedNode.gameObject.name}");
    
        if (cameraController != null && cameraController.IsFocusing())
        {
            Debug.Log("[MapManager] Câmera está focando - clique ignorado");
            return;
        }
    
        // Salva o nome do nó que estamos visitando
        lastCompletedNodeName = clickedNode.gameObject.name;
        PlayerPrefs.SetString("LastCompletedNode", lastCompletedNodeName);

        // PASSO 1: Atualiza o estado lógico dos nós na cena
        clickedNode.CompleteNode();
        clickedNode.UnlockConnectedNodes();
    
        // PASSO 2: Salva o novo estado do mapa (APENAS UMA VEZ)
        SaveMapState();

        // PASSO 3: Inicia o evento
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartEvent(clickedNode.eventType, clickedNode);
        }
        else
        {
            Debug.LogError("[MapManager] GameManager.Instance é null!");
        }
    }
    
    private void DrawConnections()
    {
        // Limpa linhas antigas
        foreach (Transform child in lineContainer) 
        {
            if (child != null)
                Destroy(child.gameObject);
        }
            
        // Desenha novas linhas
        foreach (var node in allNodes)
        {
            if (node == null) continue;
            
            foreach (var connectedNode in node.GetConnectedNodes())
            {
                if (connectedNode == null) continue;
                
                GameObject lineObj = Instantiate(linePrefab, lineContainer);
                LineRenderer lr = lineObj.GetComponent<LineRenderer>();
                
                if (lr != null)
                {
                    lr.SetPosition(0, node.transform.position);
                    lr.SetPosition(1, connectedNode.transform.position);
                }
            }
        }
        
        Debug.Log($"[MapManager] Conexões desenhadas para {allNodes.Count} nós");
    }

    // CORREÇÃO: Método público para salvar estado
    public void SaveMapState()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[MapManager] GameManager.Instance é null - não é possível salvar estado!");
            return;
        }
        
        MapStateData mapData = new MapStateData();
        
        foreach (var node in allNodes)
        {
            if (node != null)
            {
                mapData.nodeStates[node.gameObject.name] = node.IsCompleted();
                Debug.Log($"[MapManager] Salvando nó {node.gameObject.name}: {node.IsCompleted()}");
            }
        }
        
        string mapName = SceneManager.GetActiveScene().name;
        GameManager.Instance.SaveMapState(mapData, mapName);
        
        Debug.Log($"[MapManager] Estado do mapa '{mapName}' salvo com {mapData.nodeStates.Count} nós");
    }

    private void LoadMapState()
    {
        Debug.Log("[MapManager] Carregando estado do mapa...");
        
        string mapName = SceneManager.GetActiveScene().name;
        
        // CORREÇÃO: Verificação de GameManager
        if (GameManager.Instance == null)
        {
            Debug.LogError("[MapManager] GameManager.Instance é null - não é possível carregar estado!");
            return;
        }
        
        // Verifica primeiro se estamos retornando de um boss
        string completedBossNode = PlayerPrefs.GetString("CompletedBossNode", "");
        if (!string.IsNullOrEmpty(completedBossNode))
        {
            string nextScene = PlayerPrefs.GetString("NextSceneAfterBoss", "");
            
            PlayerPrefs.DeleteKey("CompletedBossNode");
            PlayerPrefs.DeleteKey("NextSceneAfterBoss");
            
            Debug.Log($"[MapManager] Boss '{completedBossNode}' completado! Progredindo para: {nextScene}");
            
            GameManager.Instance.ProgressToNextMap(nextScene);
            return;
        }

        MapStateData loadedData = GameManager.Instance.GetSavedMapState(mapName);

        if (loadedData == null) 
        {
            Debug.Log("[MapManager] Nenhum estado salvo - inicializando mapa do zero");
            InitializeDefaultMapState();
            return;
        }

        Debug.Log($"[MapManager] Estado encontrado com {loadedData.nodeStates.Count} nós salvos");

        lastCompletedNodeName = PlayerPrefs.GetString("LastCompletedNode", "");

        // CORREÇÃO: Restaura o estado ANTES de processar lógica de desbloqueio
        foreach (var node in allNodes)
        {
            if (node != null && loadedData.nodeStates.TryGetValue(node.gameObject.name, out bool isCompleted))
            {
                if (isCompleted)
                {
                    Debug.Log($"[MapManager] Restaurando nó completado: {node.gameObject.name}");
                    node.ForceComplete();
                }
            }
        }
        
        // CORREÇÃO: Processa desbloqueio após restaurar estados
        if (!string.IsNullOrEmpty(lastCompletedNodeName))
        {
            ProcessLastCompletedNode();
        }
        else
        {
            ProcessInitialMapLoad();
        }
    }
    
    /// <summary>
    /// CORREÇÃO: Inicializa estado padrão do mapa (primeiro carregamento)
    /// </summary>
    private void InitializeDefaultMapState()
    {
        Debug.Log("[MapManager] Inicializando estado padrão do mapa...");
        
        // Encontra e desbloqueia nós iniciais
        foreach (var node in allNodes)
        {
            if (node != null && IsInitialNode(node))
            {
                Debug.Log($"[MapManager] Desbloqueando nó inicial: {node.gameObject.name}");
                node.UnlockNode();
            }
        }
        
        // Salva o estado inicial
        SaveMapState();
    }
    
    /// <summary>
    /// CORREÇÃO: Verifica se um nó é inicial (não tem predecessores)
    /// </summary>
    private bool IsInitialNode(MapNode node)
    {
        foreach (MapNode otherNode in allNodes)
        {
            if (otherNode != null && otherNode != node && 
                otherNode.GetConnectedNodes().Contains(node))
            {
                return false;
            }
        }
        return node.eventType != null;
    }
    
    /// <summary>
    /// CORREÇÃO: Processa o último nó completado
    /// </summary>
    private void ProcessLastCompletedNode()
    {
        MapNode lastCompletedNode = allNodes.Find(n => n != null && n.gameObject.name == lastCompletedNodeName);
        if (lastCompletedNode != null && lastCompletedNode.IsCompleted())
        {
            Debug.Log($"[MapManager] Processando último nó completado: {lastCompletedNodeName}");
            
            // CORREÇÃO: Força desbloqueio dos nós conectados
            lastCompletedNode.UnlockConnectedNodes();
            
            // Foca a câmera no nó completado
            if (cameraController != null)
            {
                StartCoroutine(FocusOnCompletedNodeAfterDelay(lastCompletedNode, 0.5f));
            }
        }
        
        // Limpa o registro
        PlayerPrefs.DeleteKey("LastCompletedNode");
    }
    
    /// <summary>
    /// CORREÇÃO: Processa carregamento inicial do mapa
    /// </summary>
    private void ProcessInitialMapLoad()
    {
        Debug.Log("[MapManager] Processando carregamento inicial - desbloqueando nós conectados");
        
        foreach (var node in allNodes)
        {
            if (node != null && node.IsCompleted())
            {
                Debug.Log($"[MapManager] Desbloqueando conectados do nó completado: {node.gameObject.name}");
                node.UnlockConnectedNodes();
            }
        }
        
        // CORREÇÃO: Salva estado após desbloqueio inicial
        SaveMapState();
    }
    
    private System.Collections.IEnumerator FocusOnCompletedNodeAfterDelay(MapNode node, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (cameraController != null && node != null)
        {
            cameraController.FocusOnNode(node.transform);
        }
    }
    
    /// <summary>
    /// CORREÇÃO: Método de debug para verificar estado de todos os nós
    /// </summary>
    [ContextMenu("Debug All Nodes State")]
    private void DebugAllNodesState()
    {
        Debug.Log("=== DEBUG ESTADO DE TODOS OS NÓS ===");
        
        foreach (var node in allNodes)
        {
            if (node != null)
            {
                Debug.Log($"{node.gameObject.name}: Locked={node.IsLocked()}, Completed={node.IsCompleted()}, Connections={node.GetConnectedNodes().Count}");
            }
            else
            {
                Debug.Log("Nó NULL encontrado na lista!");
            }
        }
        
        Debug.Log("=====================================");
    }
    
    /// <summary>
    /// CORREÇÃO: Força reset de todos os nós (para debug)
    /// </summary>
    [ContextMenu("Reset All Nodes")]
    private void ResetAllNodes()
    {
        foreach (var node in allNodes)
        {
            if (node != null)
            {
                node.SendMessage("ResetNode", SendMessageOptions.DontRequireReceiver);
            }
        }
        
        InitializeDefaultMapState();
        Debug.Log("[MapManager] Todos os nós foram resetados");
    }
}