// MapStateData.cs (Versão Corrigida com Listas)

using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class MapStateData
{
    // Dicionário foi substituído por duas listas para garantir a serialização pela Unity
    [SerializeField]
    private List<string> nodeNames = new List<string>();
    [SerializeField]
    private List<bool> completionStates = new List<bool>();

    public string mapName;
    public long lastUpdated;
    public int totalNodes;
    public int completedNodes;

    public MapStateData()
    {
        lastUpdated = System.DateTime.Now.Ticks;
    }
    
    public MapStateData(string mapName) : this()
    {
        this.mapName = mapName;
    }

    /// <summary>
    /// Adiciona ou atualiza o estado de um nó usando as listas.
    /// </summary>
    public void SetNodeState(string nodeName, bool isCompleted)
    {
        if (string.IsNullOrEmpty(nodeName))
        {
            Debug.LogError("MapStateData: Nome do nó não pode ser vazio!");
            return;
        }

        int index = nodeNames.IndexOf(nodeName);
        if (index != -1)
        {
            // O nó já existe, apenas atualiza o estado
            completionStates[index] = isCompleted;
        }
        else
        {
            // Novo nó, adiciona às listas
            nodeNames.Add(nodeName);
            completionStates.Add(isCompleted);
        }
        UpdateMetadata();
    }

    /// <summary>
    /// Tenta obter o estado de um nó. Retorna true se encontrou, false caso contrário.
    /// </summary>
    public bool TryGetNodeState(string nodeName, out bool isCompleted)
    {
        int index = nodeNames.IndexOf(nodeName);
        if (index != -1)
        {
            isCompleted = completionStates[index];
            return true;
        }

        isCompleted = false;
        return false;
    }

    /// <summary>
    /// NOVO: Método para obter todas as entradas como um dicionário (para uso temporário)
    /// </summary>
    public Dictionary<string, bool> GetNodeStatesAsDictionary()
    {
        Dictionary<string, bool> dict = new Dictionary<string, bool>();
        for (int i = 0; i < nodeNames.Count; i++)
        {
            dict[nodeNames[i]] = completionStates[i];
        }
        return dict;
    }

    // Você pode manter os outros métodos (UpdateMetadata, GetCompletedNodes, etc.)
    // mas eles precisarão ser adaptados para usar as listas em vez do dicionário.
    public void UpdateMetadata()
    {
        lastUpdated = System.DateTime.Now.Ticks;
        totalNodes = nodeNames.Count;
        completedNodes = completionStates.Count(completed => completed);
    }
}