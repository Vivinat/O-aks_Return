using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventType { Battle, Treasure, Shop, NoEvent, DifficultAdjust, Dialogue }

[CreateAssetMenu(menuName = "Events/Event SO")]
public class EventTypeSO : ScriptableObject
{
    public EventType eventType;
    public string sceneToLoad; 
    
    /// <summary>
    /// NOVO: Verifica se este evento requer mudança de cena
    /// Eventos de diálogo não mudam de cena
    /// </summary>
    public bool RequiresSceneChange()
    {
        return eventType != EventType.Dialogue;
    }
    
    /// <summary>
    /// NOVO: Verifica se este é um evento que acontece no próprio mapa
    /// </summary>
    public bool IsInMapEvent()
    {
        return eventType == EventType.Dialogue;
    }
    
    void OnValidate()
    {
        // Validação específica para eventos de diálogo
        if (eventType == EventType.Dialogue)
        {
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.LogWarning($"EventTypeSO '{name}': Evento de diálogo não deve ter sceneToLoad definido. Este campo será ignorado.");
            }
        }
        else
        {
            // Para outros tipos de evento, sceneToLoad é obrigatório
            if (string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.LogWarning($"EventTypeSO '{name}': sceneToLoad não foi definido para evento do tipo {eventType}.");
            }
        }
    }
}