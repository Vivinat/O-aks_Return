// Assets/Scripts/Map/SimpleCameraController.cs
// Versão mais simples onde a câmera sempre segue o mouse enquanto segura o botão

using UnityEngine;
using System.Collections;

public class MapCameraManager : MonoBehaviour
{
    [Header("Camera Settings")]
    public float panSpeed = 2f;
    public float smoothTime = 0.1f;
    
    [Header("Camera Limits")]
    public Vector2 minBounds = new Vector2(-10f, -10f);
    public Vector2 maxBounds = new Vector2(10f, 10f);
    
    [Header("Focus Settings")]
    public float focusTime = 1f;
    public AnimationCurve focusCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private Camera mainCamera;
    private Vector3 targetPosition;
    private Vector3 velocity;
    private bool isFocusing = false;
    private bool isMousePressed = false;
    private Vector3 lastMouseWorldPos;
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();
            
        targetPosition = transform.position;
        
        // Verifica se voltou de um evento e deve focar no último nó completado
        CheckForNodeFocus();
    }
    
    void Update()
    {
        if (isFocusing) return; // Não permite interação durante foco automático
        
        HandleMouseInput();
        UpdateCameraPosition();
    }
    
    void HandleMouseInput()
    {
        // Quando pressiona o mouse
        if (Input.GetMouseButtonDown(0))
        {
            isMousePressed = true;
            lastMouseWorldPos = GetMouseWorldPosition();
        }
        
        // Enquanto segura o mouse - move a câmera
        if (Input.GetMouseButton(0) && isMousePressed)
        {
            Vector3 currentMouseWorldPos = GetMouseWorldPosition();
            Vector3 deltaMovement = lastMouseWorldPos - currentMouseWorldPos;
            
            targetPosition += deltaMovement * panSpeed;
            
            // Aplica limites
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
            
            // Atualiza a posição do mouse baseada na nova posição da câmera
            lastMouseWorldPos = GetMouseWorldPosition();
        }
        
        // Quando solta o mouse
        if (Input.GetMouseButtonUp(0))
        {
            isMousePressed = false;
        }
    }
    
    Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z);
        return mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }
    
    void UpdateCameraPosition()
    {
        // Move suavemente para a posição alvo
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
    
    /// <summary>
    /// Verifica se deve focar em um nó específico ao retornar de um evento
    /// </summary>
    void CheckForNodeFocus()
    {
        string lastCompletedNode = PlayerPrefs.GetString("LastCompletedNode", "");
        
        if (!string.IsNullOrEmpty(lastCompletedNode))
        {
            // Procura o nó pelo nome
            GameObject nodeObject = GameObject.Find(lastCompletedNode);
            if (nodeObject != null)
            {
                MapNode node = nodeObject.GetComponent<MapNode>();
                if (node != null && node.IsCompleted())
                {
                    Debug.Log($"Focando na câmera do nó completado: {lastCompletedNode}");
                    FocusOnNode(nodeObject.transform);
                }
            }
        }
    }
    
    /// <summary>
    /// Foca suavemente em um nó específico
    /// </summary>
    public void FocusOnNode(Transform nodeTransform)
    {
        if (nodeTransform != null && !isFocusing)
        {
            StartCoroutine(FocusOnNodeCoroutine(nodeTransform));
        }
    }
    
    /// <summary>
    /// Foca suavemente em uma posição específica
    /// </summary>
    public void FocusOnPosition(Vector3 worldPosition)
    {
        if (!isFocusing)
        {
            StartCoroutine(FocusOnPositionCoroutine(worldPosition));
        }
    }
    
    IEnumerator FocusOnNodeCoroutine(Transform nodeTransform)
    {
        yield return StartCoroutine(FocusOnPositionCoroutine(nodeTransform.position));
    }
    
    IEnumerator FocusOnPositionCoroutine(Vector3 worldPosition)
    {
        isFocusing = true;
        
        Vector3 startPosition = transform.position;
        Vector3 endPosition = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
        
        // Aplica limites à posição final
        endPosition.x = Mathf.Clamp(endPosition.x, minBounds.x, maxBounds.x);
        endPosition.y = Mathf.Clamp(endPosition.y, minBounds.y, maxBounds.y);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < focusTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / focusTime;
            float curveValue = focusCurve.Evaluate(progress);
            
            Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, curveValue);
            transform.position = currentPosition;
            targetPosition = currentPosition;
            
            yield return null;
        }
        
        transform.position = endPosition;
        targetPosition = endPosition;
        isFocusing = false;
        
        Debug.Log($"Câmera focada na posição: {endPosition}");
    }
    
    /// <summary>
    /// Define novos limites para a câmera (útil para diferentes tamanhos de mapa)
    /// </summary>
    public void SetCameraBounds(Vector2 newMinBounds, Vector2 newMaxBounds)
    {
        minBounds = newMinBounds;
        maxBounds = newMaxBounds;
        
        // Reaplica limites à posição atual
        targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
    }
    
    /// <summary>
    /// Retorna se a câmera está atualmente focando em algo
    /// </summary>
    public bool IsFocusing()
    {
        return isFocusing;
    }
    
    void OnDrawGizmosSelected()
    {
        // Desenha os limites da câmera no editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            new Vector3((minBounds.x + maxBounds.x) / 2, (minBounds.y + maxBounds.y) / 2, 0), 
            new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0)
        );
    }
}