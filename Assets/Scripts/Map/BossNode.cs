using UnityEngine;

[RequireComponent(typeof(MapNode))]
public class BossNode : MonoBehaviour
{
    [Header("Boss Configuration")]
    [Tooltip("Nome da cena do próximo mapa para carregar após completar o boss")]
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