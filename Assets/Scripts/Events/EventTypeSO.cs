using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventType { Battle, Treasure, Shop, NoEvent, DifficultAdjust, Dialogue }

[CreateAssetMenu(menuName = "Events/Event SO")]
public class EventTypeSO : ScriptableObject
{
    public EventType eventType;
    public string sceneToLoad; 
    
    public bool RequiresSceneChange()
    {
        return eventType != EventType.Dialogue;
    }
    
    public bool IsInMapEvent()
    {
        return eventType == EventType.Dialogue;
    }
    
    void OnValidate()
    {
        if (eventType == EventType.Dialogue)
        {
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.LogWarning($"EventTypeSO '{name}': Evento de diálogo não deve ter sceneToLoad definido.");
            }
        }
        else
        {
            if (string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.LogWarning($"EventTypeSO '{name}': sceneToLoad não definido para evento tipo {eventType}.");
            }
        }
    }
}