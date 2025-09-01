// GameManager.cs (Versão Refatorada)

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public EventTypeSO CurrentEvent { get; private set; }
    
    // Agora, usamos um dicionário para guardar o estado de MÚLTIPLOS mapas.
    // A chave é o nome da cena do mapa, e o valor é o pacote de dados daquele mapa.
    private Dictionary<string, MapStateData> savedMapStates = new Dictionary<string, MapStateData>();
    private string currentMapSceneName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Salva o "pacote de dados" de um mapa.
    /// </summary>
    public void SaveMapState(MapStateData mapData, string mapName)
    {
        currentMapSceneName = mapName;
        savedMapStates[mapName] = mapData; // Salva ou atualiza os dados para o mapa especificado.
        Debug.Log($"Estado do mapa '{mapName}' salvo no GameManager.");
    }

    /// <summary>
    /// Tenta recuperar o "pacote de dados" de um mapa.
    /// </summary>
    public MapStateData GetSavedMapState(string mapName)
    {
        if (savedMapStates.TryGetValue(mapName, out MapStateData mapData))
        {
            return mapData;
        }
        return null; // Retorna nulo se não houver dados salvos para este mapa.
    }

    public void StartEvent(EventTypeSO eventData)
    {
        CurrentEvent = eventData;
        SceneManager.LoadScene(eventData.sceneToLoad);
    }
    
    public void ReturnToMap()
    {
        if (!string.IsNullOrEmpty(currentMapSceneName))
        {
            SceneManager.LoadScene(currentMapSceneName);
        }
        else
        {
            Debug.LogError("Nome da cena do mapa não foi salvo! Não é possível retornar.");
        }
    }
}