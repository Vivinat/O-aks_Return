using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject para criar e configurar diálogos no Editor
/// </summary>
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue")]
public class DialogueSO : ScriptableObject
{
    [Header("Dialogue Configuration")]
    [SerializeField] private string dialogueTitle = "New Dialogue";
    
    [Header("Dialogue Entries")]
    [Tooltip("Lista de falas que compõem este diálogo")]
    [SerializeField] private List<DialogueEntry> dialogueEntries = new List<DialogueEntry>();
    
    [Header("Audio Settings (Optional)")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private bool stopCurrentMusicWhenPlaying = false;

    public List<DialogueEntry> GetDialogueEntries()
    {
        return new List<DialogueEntry>(dialogueEntries);
    }
    
    public string GetTitle()
    {
        return dialogueTitle;
    }
    
    public AudioClip GetBackgroundMusic()
    {
        return backgroundMusic;
    }
    
    public bool ShouldStopCurrentMusic()
    {
        return stopCurrentMusicWhenPlaying;
    }
    
    public void AddDialogueEntry(string speakerName, string text)
    {
        if (dialogueEntries == null)
            dialogueEntries = new List<DialogueEntry>();
            
        dialogueEntries.Add(new DialogueEntry(speakerName, text));
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    public void AddDialogueEntry(DialogueEntry entry)
    {
        if (dialogueEntries == null)
            dialogueEntries = new List<DialogueEntry>();
            
        if (entry != null)
        {
            dialogueEntries.Add(entry);
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
    }
    
    public void RemoveDialogueEntry(int index)
    {
        if (dialogueEntries != null && index >= 0 && index < dialogueEntries.Count)
        {
            dialogueEntries.RemoveAt(index);
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
    }
    
    public void ClearDialogue()
    {
        if (dialogueEntries == null)
            dialogueEntries = new List<DialogueEntry>();
        else
            dialogueEntries.Clear();
            
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    public bool IsValid()
    {
        return dialogueEntries != null && dialogueEntries.Count > 0 && 
               dialogueEntries.Exists(entry => entry != null && entry.IsValid());
    }
    
    public int GetEntryCount()
    {
        return dialogueEntries?.Count ?? 0;
    }

    private void OnEnable()
    {
        if (dialogueEntries == null)
            dialogueEntries = new List<DialogueEntry>();
    }

    void OnValidate()
    {
        if (dialogueEntries == null)
            dialogueEntries = new List<DialogueEntry>();

        if (string.IsNullOrEmpty(dialogueTitle))
            dialogueTitle = "New Dialogue";
        
        // Remove entradas completamente vazias
        for (int i = dialogueEntries.Count - 1; i >= 0; i--)
        {
            if (dialogueEntries[i] == null)
                dialogueEntries.RemoveAt(i);
            else if (string.IsNullOrEmpty(dialogueEntries[i].speakerName) && 
                     string.IsNullOrEmpty(dialogueEntries[i].text))
                dialogueEntries.RemoveAt(i);
        }

        if (dialogueEntries.Count == 0)
            dialogueEntries.Add(new DialogueEntry());
    }

    #if UNITY_EDITOR
    [ContextMenu("Add Empty Entry")]
    public void AddEmptyEntry()
    {
        if (dialogueEntries == null)
            dialogueEntries = new List<DialogueEntry>();
            
        dialogueEntries.Add(new DialogueEntry());
        UnityEditor.EditorUtility.SetDirty(this);
    }

    [ContextMenu("Add Sample Dialogue")]
    public void AddSampleDialogue()
    {
        ClearDialogue();
        AddDialogueEntry("NPC", "Olá! Como posso ajudá-lo hoje?");
        
        Debug.Log("Diálogo de exemplo adicionado ao " + name);
    }

    [ContextMenu("Validate Dialogue")]
    public void ValidateDialogue()
    {
        int validEntries = 0;
        int invalidEntries = 0;

        foreach (var entry in dialogueEntries)
        {
            if (entry != null && entry.IsValid())
                validEntries++;
            else
                invalidEntries++;
        }

        Debug.Log($"Validação do {name}:\n" +
                  $"• Entradas válidas: {validEntries}\n" +
                  $"• Entradas inválidas: {invalidEntries}\n" +
                  $"• Total: {dialogueEntries.Count}");
    }
    #endif
}