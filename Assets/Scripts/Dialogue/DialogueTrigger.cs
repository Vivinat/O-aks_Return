using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;


// Componente para disparar diálogos em GameObjects
public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueSO dialogueData;
    
    [SerializeField] private List<DialogueEntry> manualDialogue = new List<DialogueEntry>();
    
    [SerializeField] private TriggerType triggerType = TriggerType.OnClick;
    [SerializeField] private bool triggerOnlyOnce = true;
    [SerializeField] private float triggerDelay = 0f;
    
    [SerializeField] private float autoTriggerDelay = 1f;
    
    [SerializeField] private UnityEvent onDialogueStart;
    [SerializeField] private UnityEvent onDialogueComplete;
    
    private bool hasTriggered = false;
    private bool isPlayerInRange = false;

    public enum TriggerType
    {
        OnClick,
        OnTriggerEnter,
        OnStart,
        Manual
    }

    void Start()
    {
        if (triggerType == TriggerType.OnStart)
        {
            if (triggerDelay > 0)
                Invoke(nameof(TriggerDialogue), triggerDelay);
            else
                TriggerDialogue();
        }
    }

    void OnMouseDown()
    {
        if (triggerType == TriggerType.OnClick && CanTrigger())
            TriggerDialogue();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerType == TriggerType.OnTriggerEnter && 
            other.CompareTag("Player") && CanTrigger())
        {
            isPlayerInRange = true;
            
            if (autoTriggerDelay > 0)
                Invoke(nameof(DelayedTrigger), autoTriggerDelay);
            else
                TriggerDialogue();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            CancelInvoke(nameof(DelayedTrigger));
        }
    }

    private void DelayedTrigger()
    {
        if (isPlayerInRange && CanTrigger())
            TriggerDialogue();
    }

    public void TriggerDialogue()
    {
        if (!CanTrigger())
            return;

        List<DialogueEntry> dialogueToPlay = GetDialogueEntries();

        SetupDialogueMusic();

        if (triggerOnlyOnce)
            hasTriggered = true;

        onDialogueStart?.Invoke();
        DialogueManager.Instance.StartDialogue(dialogueToPlay, OnDialogueCompleted);
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
    }

    public bool CanTrigger()
    {
        if (triggerOnlyOnce && hasTriggered)
            return false;
            
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())
            return false;
            
        return true;
    }

    private List<DialogueEntry> GetDialogueEntries()
    {
        if (dialogueData != null && dialogueData.IsValid())
            return dialogueData.GetDialogueEntries();
        else if (manualDialogue != null && manualDialogue.Count > 0)
            return new List<DialogueEntry>(manualDialogue);
        
        return null;
    }

    private void SetupDialogueMusic()
    {
        if (dialogueData != null)
        {
            AudioClip bgMusic = dialogueData.GetBackgroundMusic();
            
            if (bgMusic != null && AudioManager.Instance != null)
            {
                if (dialogueData.ShouldStopCurrentMusic())
                    AudioManager.Instance.PlayMusic(bgMusic, true);
            }
        }
    }

    private void OnDialogueCompleted()
    {
        onDialogueComplete?.Invoke();
    }

    public void AddDialogueEntry(string speakerName, string text)
    {
        if (manualDialogue == null)
            manualDialogue = new List<DialogueEntry>();
            
        manualDialogue.Add(new DialogueEntry(speakerName, text));
    }

    public void ClearManualDialogue()
    {
        if (manualDialogue != null)
            manualDialogue.Clear();
    }

    public void SetDialogueData(DialogueSO newDialogueData)
    {
        dialogueData = newDialogueData;
    }

    void OnDrawGizmosSelected()
    {
        if (triggerType == TriggerType.OnTriggerEnter)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null && col.isTrigger)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(transform.position, col.bounds.size);
            }
        }
    }
}