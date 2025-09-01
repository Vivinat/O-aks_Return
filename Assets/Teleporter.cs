using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Teleporter : MonoBehaviour
{
    private void OnMouseDown()
    {
        GameManager.Instance.ReturnToMap();
    }
}
