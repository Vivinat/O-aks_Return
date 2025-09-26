// MapManager.cs (Versão Final - Bug Corrigido)

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

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
        
        // Garante que a lista de nós está preenchida
        PopulateNodesList();
        
        LoadMapState();
        DrawConnections();
        SetupCameraBounds();
    }

    private void PopulateNodesList()
    {
        // Se a lista já foi preenchida no Inspector, não faz nada
        if (allNodes != null && allNodes.Count > 0 && allNodes.All(node => node != null))
        {
            Debug.Log($"MapManager: {allNodes.Count} nós já configurados no Inspector");
            return;
        }

        // Encontra todos os MapNodes na cena automaticamente
        MapNode[] foundNodes = FindObjectsOfType<MapNode>();
        allNodes = new List<MapNode>(foundNodes);
        
        Debug.Log($"MapManager: {allNodes.Count} nós encontrados automaticamente na cena");
        
        if (allNodes.Count == 0)
        {
            Debug.LogError("MapManager: Nenhum MapNode encontrado na cena!");
        }
    }

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
    /// CORREÇÃO CRÍTICA: Evita clique duplo e salvamento duplo
    /// </summary>
    public void OnNodeClicked(MapNode clickedNode)
    {
        // Validação adicional
        if (clickedNode == null)
        {
            Debug.LogError("MapManager: Nó clicado é null!");
            return;
        }

        if (allNodes == null || allNodes.Count == 0)
        {
            Debug.LogError("MapManager: Lista de nós não foi inicializada!");
            PopulateNodesList();
            if (allNodes.Count == 0) return;
        }

        // Verifica se a câmera está focando - se sim, ignora cliques
        if (cameraController != null && cameraController.IsFocusing())
        {
            Debug.Log("Câmera está focando - clique ignorado");
            return;
        }

        // CORREÇÃO PRINCIPAL: Verifica se o nó pode ser clicado antes de processar
        if (clickedNode.IsLocked())
        {
            Debug.Log($"Nó {clickedNode.gameObject.name} está bloqueado - clique ignorado");
            return;
        }

        if (clickedNode.IsCompleted())
        {
            Debug.Log($"Nó {clickedNode.gameObject.name} já foi completado - clique ignorado");
            return;
        }
        
        Debug.Log($"=== PROCESSANDO CLIQUE NO NÓ {clickedNode.gameObject.name} ===");
        
        // Verifica se é um nó de boss
        BossNode bossNode = clickedNode.GetComponent<BossNode>();
        bool isBossNode = bossNode != null;
        
        if (isBossNode)
        {
            Debug.Log($"Boss node clicado: {clickedNode.gameObject.name}");
            PlayerPrefs.SetString("CompletedBossNode", clickedNode.gameObject.name);
            PlayerPrefs.SetString("NextSceneAfterBoss", bossNode.GetNextScene());
        }
        else
        {
            // Salva qual nó normal está sendo completado
            lastCompletedNodeName = clickedNode.gameObject.name;
            PlayerPrefs.SetString("LastCompletedNode", lastCompletedNodeName);
            Debug.Log($"Salvando nó completado: {lastCompletedNodeName}");
        }

        // CORREÇÃO CRÍTICA: Sequência correta de operações
        // 1. Marca o nó como completado
        clickedNode.CompleteNode();
        
        // 2. IMEDIATAMENTE salva o estado para garantir persistência
        SaveMapState();
        
        // 3. Inicia o evento (que mudará de cena)
        Debug.Log($"Iniciando evento para o nó: {clickedNode.gameObject.name}");
        GameManager.Instance.StartEvent(clickedNode.eventType, clickedNode);
    }
    
    private void DrawConnections()
    {
        if (lineContainer == null || linePrefab == null) return;
        
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
        if (allNodes == null || allNodes.Count == 0) return;

        // Usando o construtor correto
        MapStateData mapData = new MapStateData(SceneManager.GetActiveScene().name);
    
        Debug.Log($"=== SALVANDO ESTADO DO MAPA ===");
    
        foreach (var node in allNodes)
        {
            if (node != null)
            {
                // Usando o novo método SetNodeState
                mapData.SetNodeState(node.gameObject.name, node.IsCompleted());
            
                if (node.IsCompleted())
                {
                    Debug.Log($"✓ Nó {node.gameObject.name}: COMPLETADO");
                }
                else
                {
                    Debug.Log($"✗ Nó {node.gameObject.name}: não completado");
                }
            }
        }
    
        GameManager.Instance.SaveMapState(mapData, mapData.mapName);
        Debug.Log($"Estado do mapa '{mapData.mapName}' salvo.");
        Debug.Log($"=== FIM DO SALVAMENTO ===");
    }

    private void LoadMapState()
    {
        string mapName = SceneManager.GetActiveScene().name;
        Debug.Log($"=== CARREGANDO ESTADO DO MAPA '{mapName}' ===");
        
        // Verifica primeiro se estamos retornando de um boss
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
            return;
        }

        MapStateData loadedData = GameManager.Instance.GetSavedMapState(mapName);
        lastCompletedNodeName = PlayerPrefs.GetString("LastCompletedNode", "");
        
        if (loadedData == null) 
        {
            Debug.Log("Nenhum estado salvo encontrado - começando do zero.");
            
            // No primeiro carregamento, desbloqueia o primeiro nó
            if (allNodes.Count > 0)
            {
                allNodes[0].UnlockNode();
                Debug.Log($"Primeiro nó '{allNodes[0].gameObject.name}' desbloqueado");
            }
            
            // CRÍTICO: Limpa qualquer flag remanescente
            PlayerPrefs.DeleteKey("LastCompletedNode");
            return;
        }

        Debug.Log($"Estado encontrado! Último nó completado: '{lastCompletedNodeName}'");

        // PARTE 1: Restaura TODOS os estados primeiro
        int restoredCount = 0; // Contador inicializado
        foreach (var node in allNodes)
        {
            if (loadedData.TryGetNodeState(node.gameObject.name, out bool wasCompleted))
            {
                if (wasCompleted)
                {
                    node.ForceComplete();
                    restoredCount++; // << CORREÇÃO: Incrementa o contador
                    // O log de "restaurado como COMPLETADO" já está dentro do ForceComplete
                }
            }
        }
        
        Debug.Log($"Total de {restoredCount} nós restaurados como completados");
        
        foreach (var node in allNodes)
        {
            if (node.IsCompleted())
            {
                // Este comando irá desbloquear os nós seguintes
                node.UnlockConnectedNodes();
            }
        }
        
        // Se estamos voltando de um evento, foca na câmera E limpa a flag
        if (!string.IsNullOrEmpty(lastCompletedNodeName))
        {
            MapNode lastCompletedNode = allNodes.Find(n => n.gameObject.name == lastCompletedNodeName);
            if (lastCompletedNode != null)
            {
                Debug.Log($"Focando câmera no nó recém-completado: {lastCompletedNodeName}");
                
                if (cameraController != null)
                {
                    StartCoroutine(FocusOnCompletedNodeAfterDelay(lastCompletedNode, 0.5f));
                }
            }
            
            // CRÍTICO: SEMPRE limpa o registro após usar
            PlayerPrefs.DeleteKey("LastCompletedNode");
            Debug.Log("Flag 'LastCompletedNode' limpa após uso");
        }
        
        Debug.Log($"=== FIM DO CARREGAMENTO ===");
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

    // Método para forçar refresh da lista de nós
    [ContextMenu("Refresh Nodes List")]
    public void RefreshNodesList()
    {
        allNodes.Clear();
        PopulateNodesList();
        Debug.Log("Lista de nós atualizada manualmente");
    }

    // Método para forçar salvamento
    [ContextMenu("Force Save Map State")]
    public void ForceSaveMapState()
    {
        SaveMapState();
    }

    // Método para limpar PlayerPrefs
    [ContextMenu("Clear PlayerPrefs")]
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteKey("LastCompletedNode");
        PlayerPrefs.DeleteKey("CompletedBossNode");
        PlayerPrefs.DeleteKey("NextSceneAfterBoss");
        Debug.Log("PlayerPrefs limpos");
    }

    // NOVO: Método para forçar reset completo
    [ContextMenu("Reset All Nodes")]
    public void ResetAllNodes()
    {
        foreach (var node in allNodes)
        {
            if (node != null)
            {
                node.ResetNode();
            }
        }
        
        // Desbloqueia o primeiro nó
        if (allNodes.Count > 0)
        {
            allNodes[0].UnlockNode();
        }
        
        // Limpa PlayerPrefs
        ClearPlayerPrefs();
        
        // Limpa estado salvo
        string mapName = SceneManager.GetActiveScene().name;
        GameManager.Instance.ClearMapData(mapName);
        
        Debug.Log("RESET COMPLETO: Todos os nós resetados, primeiro nó desbloqueado");
    }
}