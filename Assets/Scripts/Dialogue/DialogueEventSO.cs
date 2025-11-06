using UnityEngine;

/// <summary>
/// Evento de diálogo executado no mapa. Suporta texto direto ou referência a DialogueSO.
/// </summary>
[CreateAssetMenu(menuName = "Events/Dialogue Event SO")]
public class DialogueEventSO : EventTypeSO
{
    [Header("Dialogue Configuration")]
    [Tooltip("Texto do diálogo. Se preenchido, será usado em vez do DialogueSO.")]
    [TextArea(3, 8)]
    public string dialogueText = "";
    
    [Tooltip("Nome do speaker (opcional)")]
    public string speakerName = "";
    
    [Header("Advanced Dialogue")]
    [Tooltip("DialogueSO complexo (opcional). Ignorado se dialogueText estiver preenchido.")]
    public DialogueSO dialogueData;
    
    [Header("Audio Configuration")]
    [Tooltip("Som específico para este diálogo (opcional)")]
    public AudioClip dialogueSound;
    
    public bool HasValidDialogue()
    {
        return !string.IsNullOrEmpty(dialogueText) || 
               (dialogueData != null && dialogueData.IsValid());
    }
    
    public string GetDialogueText()
    {
        if (!string.IsNullOrEmpty(dialogueText))
            return dialogueText;
        
        if (dialogueData != null && dialogueData.GetDialogueEntries().Count > 0)
        {
            var entries = dialogueData.GetDialogueEntries();
            if (entries.Count == 1)
                return entries[0].text;
        }
        
        return "Diálogo não configurado.";
    }
    
    public string GetSpeakerName()
    {
        if (!string.IsNullOrEmpty(speakerName))
            return speakerName;
        
        if (dialogueData != null && dialogueData.GetDialogueEntries().Count > 0)
            return dialogueData.GetDialogueEntries()[0].speakerName;
        
        return "";
    }
    
    public bool ShouldUseDialogueSO()
    {
        return string.IsNullOrEmpty(dialogueText) && 
               dialogueData != null && 
               dialogueData.IsValid();
    }
    
    void OnValidate()
    {
        if (string.IsNullOrEmpty(dialogueText) && dialogueData == null)
            Debug.LogWarning($"DialogueEventSO '{name}': Nem dialogueText nem dialogueData foram configurados!");
        
        if (!string.IsNullOrEmpty(dialogueText) && dialogueData != null)
            Debug.LogWarning($"DialogueEventSO '{name}': Ambos configurados. dialogueText será usado.");
    }
}