using UnityEngine;
using System.Collections.Generic;

public class MapNode : MonoBehaviour
{
    [SerializeField]
    private List<MapNode> connectedNodes = new List<MapNode>();

    [SerializeField]
    private bool isLocked = true;
    private bool isCompleted = false;

    [Header("Event Configuration")]
    public EventTypeSO eventType;
    
    [Header("Audio Configuration")]
    [Tooltip("Música que tocará na cena do evento. Se null, mantém a música atual.")]
    public AudioClip eventMusic;
    
    [Header("Battle Visual Configuration")]
    [Tooltip("Sprite de fundo específico para batalhas deste nó (sobrescreve o do BattleEventSO)")]
    public Sprite battleBackgroundOverride;
    
    private void Start()
    {
        UpdateVisuals();
    }
    
    private void OnMouseDown()
    {
        if (!isLocked && !isCompleted)
        {
            AudioConstants.PlayButtonSelect();
            SetupAudioForEvent();
            FindObjectOfType<MapManager>().OnNodeClicked(this);
        }
        else
        {
            if (isLocked)
            {
                AudioConstants.PlayCannotSelect();
            }
        }
    }
    
    private void SetupAudioForEvent()
    {
        if (AudioManager.Instance != null)
        {
            AudioClip currentMapMusic = AudioManager.Instance.GetCurrentMusic();
            AudioManager.Instance.SetPendingEventMusic(eventMusic, currentMapMusic);
        }
    }

    public void CompleteNode()
    {
        isCompleted = true;
        UpdateVisuals();
    }
    
    public void UnlockNode()
    {
        if (!isCompleted)
        {
            isLocked = false;
            UpdateVisuals();
        }
    }
    
    public void UnlockConnectedNodes()
    {
        foreach (MapNode node in connectedNodes)
        {
            node.UnlockNode();
        }
    }

    private void UpdateVisuals()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            if (isCompleted) 
            {
                spriteRenderer.color = Color.gray;
            }
            else if (isLocked) 
            {
                spriteRenderer.color = Color.red;
            }
            else 
            {
                spriteRenderer.color = Color.white;
            }
        }
    }
    
    public bool IsCompleted() => isCompleted;
    public bool IsLocked() => isLocked;
    public List<MapNode> GetConnectedNodes() => connectedNodes;
    public AudioClip GetEventMusic() => eventMusic;
    
    public void ForceComplete()
    {
        isCompleted = true;
        isLocked = false;
        UpdateVisuals();
    }
}