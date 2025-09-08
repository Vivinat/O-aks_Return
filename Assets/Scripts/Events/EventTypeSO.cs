using System.Collections;
using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;

public enum EventType { Battle, Treasure, Shop, NoEvent, DifficultAdjust}

[CreateAssetMenu(menuName = "Events/Event SO")]
public class EventTypeSO : ScriptableObject
{
    public EventType eventType;
    public string sceneToLoad; 
}
