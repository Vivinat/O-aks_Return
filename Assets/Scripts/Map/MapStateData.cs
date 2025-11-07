using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapStateData
{
    public Dictionary<string, bool> nodeStates;

    public MapStateData()
    {
        nodeStates = new Dictionary<string, bool>();
    }
}
