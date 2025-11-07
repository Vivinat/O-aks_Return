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
        if (lineContainer == null)
        {
            lineContainer = new GameObject("LineContainer").transform;
            lineContainer.SetParent(this.transform);
        }
        
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<MapCameraManager>();
        }
        
        LoadMapState();
        DrawConnections();
        SetupCameraBounds();
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
    }

    public void OnNodeClicked(MapNode clickedNode)
    {
        if (cameraController != null && cameraController.IsFocusing())
        {
            return;
        }
        
        BossNode bossNode = clickedNode.GetComponent<BossNode>();
        bool isBossNode = bossNode != null;
        
        if (isBossNode)
        {
            PlayerPrefs.SetString("CompletedBossNode", clickedNode.gameObject.name);
            PlayerPrefs.SetString("NextSceneAfterBoss", bossNode.GetNextScene());
        }
        else
        {
            lastCompletedNodeName = clickedNode.gameObject.name;
            PlayerPrefs.SetString("LastCompletedNode", lastCompletedNodeName);
        }

        clickedNode.CompleteNode();
        clickedNode.UnlockConnectedNodes();
        SaveMapState();
        GameManager.Instance.StartEvent(clickedNode.eventType, clickedNode);
    }
    
    private void DrawConnections()
    {
        foreach (Transform child in lineContainer) 
            Destroy(child.gameObject);
            
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

        string completedBossNode = PlayerPrefs.GetString("CompletedBossNode", "");
        if (!string.IsNullOrEmpty(completedBossNode))
        {
            string nextScene = PlayerPrefs.GetString("NextSceneAfterBoss", "");
            
            PlayerPrefs.DeleteKey("CompletedBossNode");
            PlayerPrefs.DeleteKey("NextSceneAfterBoss");
            
            GameManager.Instance.ProgressToNextMap(nextScene);
            return;
        }

        if (loadedData == null) 
        {
            return;
        }

        lastCompletedNodeName = PlayerPrefs.GetString("LastCompletedNode", "");

        foreach (var node in allNodes)
        {
            if (loadedData.nodeStates.TryGetValue(node.gameObject.name, out bool isCompleted) && isCompleted)
            {
                node.ForceComplete();
            }
        }
        
        if (!string.IsNullOrEmpty(lastCompletedNodeName))
        {
            MapNode lastCompletedNode = allNodes.Find(n => n.gameObject.name == lastCompletedNodeName);
            if (lastCompletedNode != null && lastCompletedNode.IsCompleted())
            {
                lastCompletedNode.UnlockConnectedNodes();
                
                if (cameraController != null)
                {
                    StartCoroutine(FocusOnCompletedNodeAfterDelay(lastCompletedNode, 0.5f));
                }
            }
            
            PlayerPrefs.DeleteKey("LastCompletedNode");
        }
        else
        {
            foreach (var node in allNodes)
            {
                if (node.IsCompleted())
                {
                    node.UnlockConnectedNodes();
                }
            }
        }
    }
    
    private System.Collections.IEnumerator FocusOnCompletedNodeAfterDelay(MapNode node, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (cameraController != null && node != null)
        {
            cameraController.FocusOnNode(node.transform);
        }
    }
}