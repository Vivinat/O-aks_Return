using UnityEngine;
using System.Collections;

public class MapCameraManager : MonoBehaviour
{
    public float panSpeed = 2f;
    public float smoothTime = 0.1f;
    
    public Vector2 minBounds = new Vector2(-10f, -10f);
    public Vector2 maxBounds = new Vector2(10f, 10f);
    
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
        CheckForNodeFocus();
    }
    
    void Update()
    {
        if (isFocusing) return;
        
        HandleMouseInput();
        UpdateCameraPosition();
    }
    
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isMousePressed = true;
            lastMouseWorldPos = GetMouseWorldPosition();
        }
        
        if (Input.GetMouseButton(0) && isMousePressed)
        {
            Vector3 currentMouseWorldPos = GetMouseWorldPosition();
            Vector3 deltaMovement = lastMouseWorldPos - currentMouseWorldPos;
            
            targetPosition += deltaMovement * panSpeed;
            
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
            
            lastMouseWorldPos = GetMouseWorldPosition();
        }
        
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
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
    
    void CheckForNodeFocus()
    {
        string lastCompletedNode = PlayerPrefs.GetString("LastCompletedNode", "");
        
        if (!string.IsNullOrEmpty(lastCompletedNode))
        {
            GameObject nodeObject = GameObject.Find(lastCompletedNode);
            if (nodeObject != null)
            {
                MapNode node = nodeObject.GetComponent<MapNode>();
                if (node != null && node.IsCompleted())
                {
                    FocusOnNode(nodeObject.transform);
                }
            }
        }
    }
    
    public void FocusOnNode(Transform nodeTransform)
    {
        if (nodeTransform != null && !isFocusing)
        {
            StartCoroutine(FocusOnNodeCoroutine(nodeTransform));
        }
    }
    
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
    }
    
    public void SetCameraBounds(Vector2 newMinBounds, Vector2 newMaxBounds)
    {
        minBounds = newMinBounds;
        maxBounds = newMaxBounds;
        
        targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
    }
    
    public bool IsFocusing()
    {
        return isFocusing;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            new Vector3((minBounds.x + maxBounds.x) / 2, (minBounds.y + maxBounds.y) / 2, 0), 
            new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0)
        );
    }
}