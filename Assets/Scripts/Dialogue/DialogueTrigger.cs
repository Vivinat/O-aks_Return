// Assets/Scripts/Dialogue/DialogueTrigger.cs

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Componente que pode ser adicionado a GameObjects para disparar diálogos
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Configuration")]
    [SerializeField] private DialogueSO dialogueData;
    
    [Header("Manual Dialogue (if no DialogueSO)")]
    [SerializeField] private List<DialogueEntry> manualDialogue = new List<DialogueEntry>();
    
    [Header("Trigger Settings")]
    [SerializeField] private TriggerType triggerType = TriggerType.OnClick;
    [SerializeField] private bool triggerOnlyOnce = true;
    [SerializeField] private float triggerDelay = 0f;
    
    [Header("Auto Trigger Settings")]
    [SerializeField] private float autoTriggerDelay = 1f;
    
    [Header("Events")]
    [SerializeField] private UnityEvent onDialogueStart;
    [SerializeField] private UnityEvent onDialogueComplete;
    
    // Estado interno
    private bool hasTriggered = false;
    private bool isPlayerInRange = false;

    public enum TriggerType
    {
        OnClick,           // Clique no objeto
        OnTriggerEnter,    // Jogador entra no trigger
        OnStart,           // Automaticamente no Start
        Manual             // Apenas por script
    }

    void Start()
    {
        if (triggerType == TriggerType.OnStart)
        {
            if (triggerDelay > 0)
            {
                Invoke(nameof(TriggerDialogue), triggerDelay);
            }
            else
            {
                TriggerDialogue();
            }
        }
    }

    void OnMouseDown()
    {
        if (triggerType == TriggerType.OnClick && CanTrigger())
        {
            TriggerDialogue();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerType == TriggerType.OnTriggerEnter && 
            other.CompareTag("Player") && CanTrigger())
        {
            isPlayerInRange = true;
            
            if (autoTriggerDelay > 0)
            {
                Invoke(nameof(DelayedTrigger), autoTriggerDelay);
            }
            else
            {
                TriggerDialogue();
            }
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
        {
            TriggerDialogue();
        }
    }

    /// <summary>
    /// Dispara o diálogo manualmente (pode ser chamado por outros scripts)
    /// </summary>
    public void TriggerDialogue()
    {
        if (!CanTrigger())
        {
            Debug.Log($"DialogueTrigger '{name}': Não pode disparar o diálogo no momento");
            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueTrigger: DialogueSystem não encontrado na cena!");
            return;
        }

        List<DialogueEntry> dialogueToPlay = GetDialogueEntries();
        
        if (dialogueToPlay == null || dialogueToPlay.Count == 0)
        {
            Debug.LogWarning($"DialogueTrigger '{name}': Nenhum diálogo configurado!");
            return;
        }

        Debug.Log($"DialogueTrigger '{name}': Disparando diálogo com {dialogueToPlay.Count} entradas");

        // Configura música se especificada
        SetupDialogueMusic();

        // Marca como disparado se for única vez
        if (triggerOnlyOnce)
        {
            hasTriggered = true;
        }

        // Dispara eventos
        onDialogueStart?.Invoke();

        // Inicia o diálogo
        DialogueManager.Instance.StartDialogue(dialogueToPlay, OnDialogueCompleted);
    }

    /// <summary>
    /// Força o trigger novamente (útil para debug ou mecânicas especiais)
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
        Debug.Log($"DialogueTrigger '{name}': Reset - pode ser disparado novamente");
    }

    /// <summary>
    /// Verifica se o diálogo pode ser disparado
    /// </summary>
    public bool CanTrigger()
    {
        if (triggerOnlyOnce && hasTriggered)
            return false;
            
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())
            return false;
            
        return true;
    }

    /// <summary>
    /// Retorna as entradas de diálogo configuradas
    /// </summary>
    private List<DialogueEntry> GetDialogueEntries()
    {
        if (dialogueData != null && dialogueData.IsValid())
        {
            return dialogueData.GetDialogueEntries();
        }
        else if (manualDialogue != null && manualDialogue.Count > 0)
        {
            return new List<DialogueEntry>(manualDialogue);
        }
        
        return null;
    }

    /// <summary>
    /// Configura música específica para o diálogo se necessário
    /// </summary>
    private void SetupDialogueMusic()
    {
        if (dialogueData != null)
        {
            AudioClip bgMusic = dialogueData.GetBackgroundMusic();
            
            if (bgMusic != null && AudioManager.Instance != null)
            {
                if (dialogueData.ShouldStopCurrentMusic())
                {
                    AudioManager.Instance.PlayMusic(bgMusic, true);
                }
                else
                {
                    // Talvez tocar em volume baixo por cima da música atual
                    // (implementação dependeria das suas necessidades específicas)
                }
            }
        }
    }

    /// <summary>
    /// Callback chamado quando o diálogo termina
    /// </summary>
    private void OnDialogueCompleted()
    {
        Debug.Log($"DialogueTrigger '{name}': Diálogo completado");
        onDialogueComplete?.Invoke();
    }

    /// <summary>
    /// Adiciona uma entrada de diálogo manualmente (útil para scripts)
    /// </summary>
    public void AddDialogueEntry(string speakerName, string text)
    {
        if (manualDialogue == null)
            manualDialogue = new List<DialogueEntry>();
            
        manualDialogue.Add(new DialogueEntry(speakerName, text));
    }

    /// <summary>
    /// Limpa o diálogo manual
    /// </summary>
    public void ClearManualDialogue()
    {
        if (manualDialogue != null)
            manualDialogue.Clear();
    }

    /// <summary>
    /// Define um DialogueSO por script
    /// </summary>
    public void SetDialogueData(DialogueSO newDialogueData)
    {
        dialogueData = newDialogueData;
    }

    void OnValidate()
    {
        // Validação no Editor
        if (dialogueData == null && (manualDialogue == null || manualDialogue.Count == 0))
        {
            Debug.LogWarning($"DialogueTrigger '{name}': Nenhum diálogo configurado! " +
                           "Atribua um DialogueSO ou configure o diálogo manual.");
        }

        if (triggerType == TriggerType.OnTriggerEnter)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col == null)
            {
                Debug.LogWarning($"DialogueTrigger '{name}': Tipo OnTriggerEnter requer um Collider2D com isTrigger = true");
            }
            else if (!col.isTrigger)
            {
                Debug.LogWarning($"DialogueTrigger '{name}': Collider2D deve ter isTrigger = true para OnTriggerEnter");
            }
        }

        if (triggerType == TriggerType.OnClick)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col == null)
            {
                Debug.LogWarning($"DialogueTrigger '{name}': Tipo OnClick requer um Collider2D para detectar cliques");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Desenha área do trigger se for OnTriggerEnter
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