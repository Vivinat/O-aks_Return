using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapStateData
{
    // O dicionário que guarda o nome do nó e seu estado de 'completado'.
    public Dictionary<string, bool> nodeStates;

    // Um construtor que facilita a criação deste objeto.
    public MapStateData()
    {
        nodeStates = new Dictionary<string, bool>();
    }
}
