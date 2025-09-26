// Assets/Scripts/Map/MapStateData.cs (VERSÃO FINAL CORRIGIDA)

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapStateData : ISerializationCallbackReceiver
{
    // Dicionário que usamos no código. Não será salvo diretamente.
    public Dictionary<string, bool> nodeStates = new Dictionary<string, bool>();

    // Listas que o JsonUtility VAI salvar.
    [SerializeField]
    private List<string> nodeNames = new List<string>();
    [SerializeField]
    private List<bool> completionStates = new List<bool>();

    // Chamado ANTES de salvar o objeto em JSON.
    public void OnBeforeSerialize()
    {
        // Limpa as listas para garantir que não haja dados antigos.
        nodeNames.Clear();
        completionStates.Clear();

        // Converte o dicionário para as listas.
        foreach (var kvp in nodeStates)
        {
            nodeNames.Add(kvp.Key);
            completionStates.Add(kvp.Value);
        }
    }

    // Chamado DEPOIS de carregar o objeto do JSON.
    public void OnAfterDeserialize()
    {
        // Limpa o dicionário para preenchê-lo com os dados carregados.
        nodeStates = new Dictionary<string, bool>();

        // Reconstrói o dicionário a partir das listas.
        for (int i = 0; i < nodeNames.Count && i < completionStates.Count; i++)
        {
            nodeStates[nodeNames[i]] = completionStates[i];
        }
    }
    
    // Métodos de ajuda para interagir com o dicionário
    public void SetNodeState(string nodeName, bool isCompleted)
    {
        nodeStates[nodeName] = isCompleted;
    }
    
    public bool GetNodeState(string nodeName)
    {
        return nodeStates.TryGetValue(nodeName, out bool isCompleted) && isCompleted;
    }

    public int GetNodeCount()
    {
        return nodeStates.Count;
    }
}