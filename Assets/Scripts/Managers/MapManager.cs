// MapManager.cs (Versão Simplificada com Boss)

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
    
    // Referência para o controlador de câmera
    [SerializeField]
    private MapCameraManager cameraController;
    
    // Variável para rastrear qual nó foi completado mais recentemente
    private string lastCompletedNodeName;
    
    void Start()
    {
        if (lineContainer == null)
        {
            lineContainer = new GameObject("LineContainer").transform;
            lineContainer.SetParent(this.transform);
        }
        
        // Encontra o controlador de câmera se não foi atribuído
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<MapCameraManager>();
        }
        
        LoadMapState();
        DrawConnections();
        SetupCameraBounds();
    }

    /// <summary>
    /// Configura automaticamente os limites da câmera baseado nas posições dos nós
    /// </summary>
    private void SetupCameraBounds()
    {
        if (cameraController == null || allNodes.Count == 0) return;
        
        Vector2 minBounds = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 maxBounds = new Vector2(float.MinValue, float.MinValue);
        
        // Encontra os limites baseado nas posições dos nós
        foreach (MapNode node in allNodes)
        {
            Vector3 nodePos = node.transform.position;
            minBounds.x = Mathf.Min(minBounds.x, nodePos.x);
            minBounds.y = Mathf.Min(minBounds.y, nodePos.y);
            maxBounds.x = Mathf.Max(maxBounds.x, nodePos.x);
            maxBounds.y = Mathf.Max(maxBounds.y, nodePos.y);
        }
        
        // Adiciona uma margem
        float margin = 3f;
        minBounds -= Vector2.one * margin;
        maxBounds += Vector2.one * margin;
        
        cameraController.SetCameraBounds(minBounds, maxBounds);
        Debug.Log($"Limites da câmera configurados: Min{minBounds}, Max{maxBounds}");
    }

    /// <summary>
    /// Função principal chamada quando um nó é clicado
    /// NOVO: Verifica se é um nó de boss
    /// </summary>
    public void OnNodeClicked(MapNode clickedNode)
    {
        // Verifica se a câmera está focando - se sim, ignora cliques
        if (cameraController != null && cameraController.IsFocusing())
        {
            Debug.Log("Câmera está focando - clique ignorado");
            return;
        }
        
        // NOVO: Verifica se é um nó de boss
        BossNode bossNode = clickedNode.GetComponent<BossNode>();
        bool isBossNode = bossNode != null;
        
        if (isBossNode)
        {
            Debug.Log($"Boss node clicado: {clickedNode.gameObject.name}");
            // Salva informação de que é um boss para usar após o evento
            PlayerPrefs.SetString("CompletedBossNode", clickedNode.gameObject.name);
            PlayerPrefs.SetString("NextSceneAfterBoss", bossNode.GetNextScene());
        }
        else
        {
            // Salva qual nó normal está sendo completado
            lastCompletedNodeName = clickedNode.gameObject.name;
            PlayerPrefs.SetString("LastCompletedNode", lastCompletedNodeName);
        }

        // Atualiza o estado LÓGICO do mapa
        clickedNode.CompleteNode();
        clickedNode.UnlockConnectedNodes();
        
        // Salva o estado atualizado
        SaveMapState();

        // Inicia o evento
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

        // NOVO: Verifica primeiro se estamos retornando de um boss
        string completedBossNode = PlayerPrefs.GetString("CompletedBossNode", "");
        if (!string.IsNullOrEmpty(completedBossNode))
        {
            string nextScene = PlayerPrefs.GetString("NextSceneAfterBoss", "");
            
            // Limpa as flags
            PlayerPrefs.DeleteKey("CompletedBossNode");
            PlayerPrefs.DeleteKey("NextSceneAfterBoss");
            
            Debug.Log($"Boss '{completedBossNode}' foi completado! Progredindo para: {nextScene}");
            
            // Progride para o próximo mapa
            GameManager.Instance.ProgressToNextMap(nextScene);
            return; // Sai aqui pois estamos mudando de cena
        }

        if (loadedData == null) 
        {
            Debug.Log("Nenhum estado salvo encontrado para este mapa - começando do zero.");
            return;
        }

        Debug.Log("Carregando estado do mapa...");

        // Pega o nome do último nó completado (se houver)
        lastCompletedNodeName = PlayerPrefs.GetString("LastCompletedNode", "");

        // Restaura o estado de completado de todos os nós
        foreach (var node in allNodes)
        {
            if (loadedData.nodeStates.TryGetValue(node.gameObject.name, out bool isCompleted) && isCompleted)
            {
                node.ForceComplete();
                Debug.Log($"Nó {node.gameObject.name} marcado como completado");
            }
        }
        
        // Se estamos voltando de um evento normal
        if (!string.IsNullOrEmpty(lastCompletedNodeName))
        {
            MapNode lastCompletedNode = allNodes.Find(n => n.gameObject.name == lastCompletedNodeName);
            if (lastCompletedNode != null && lastCompletedNode.IsCompleted())
            {
                Debug.Log($"Desbloqueando nós conectados do nó recém-completado: {lastCompletedNodeName}");
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
        else
        {
            // Carregamento inicial - desbloqueamos os conectados de todos os nós completados
            Debug.Log("Carregamento inicial - desbloqueando todos os nós conectados");
            foreach (var node in allNodes)
            {
                if (node.IsCompleted())
                {
                    node.UnlockConnectedNodes();
                }
            }
        }
    }
    
    /// <summary>
    /// Foca a câmera no nó completado após um delay
    /// </summary>
    private System.Collections.IEnumerator FocusOnCompletedNodeAfterDelay(MapNode node, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (cameraController != null && node != null)
        {
            cameraController.FocusOnNode(node.transform);
        }
    }
}