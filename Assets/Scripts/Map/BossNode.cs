using UnityEngine;

[RequireComponent(typeof(MapNode))]
public class BossNode : MonoBehaviour
{
    public string nextSceneName = "Map_Level2";

    public string GetNextScene()
    {
        return nextSceneName;
    }

    public bool IsBossNode()
    {
        return true;
    }
}