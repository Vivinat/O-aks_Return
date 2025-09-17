// Assets/Scripts/Map/BossNode.cs

using UnityEngine;

/// <summary>
/// Componente simples que marca um nó como boss e define a próxima cena
/// </summary>
[RequireComponent(typeof(MapNode))]
public class BossNode : MonoBehaviour
{
    [Header("Boss Configuration")]
    [Tooltip("Nome da cena do próximo mapa para carregar após completar o boss")]
    public string nextSceneName = "Map_Level2";

    /// <summary>
    /// Retorna o nome da próxima cena
    /// </summary>
    public string GetNextScene()
    {
        return nextSceneName;
    }

    /// <summary>
    /// Verifica se este é um nó de boss
    /// </summary>
    public bool IsBossNode()
    {
        return true;
    }
}