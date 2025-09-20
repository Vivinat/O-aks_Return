// Assets/Scripts/Dialogue/DialogueSO.cs

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

    /// <summary>
    /// Retorna todas as entradas de diálogo
    /// </summary>
    public List<DialogueEntry> GetDialogueEntries()
    {
        return new List<DialogueEntry>(dialogueEntries);
    }
    
    /// <summary>
    /// Retorna o título do diálogo
    /// </summary>
    public string GetTitle()
    {
        return dialogueTitle;
    }
    
    /// <summary>
    /// Música de fundo específica para este diálogo (opcional)
    /// </summary>
    public AudioClip GetBackgroundMusic()
    {
        return backgroundMusic;
    }
    
    /// <summary>
    /// Se deve parar a música atual ao tocar este diálogo
    /// </summary>
    public bool ShouldStopCurrentMusic()
    {
        return stopCurrentMusicWhenPlaying;
    }
    
    /// <summary>
    /// Adiciona uma nova entrada de diálogo (útil para scripts)
    /// </summary>
    public void AddDialogueEntry(string speakerName, string text)
    {
        if (dialogueEntries == null)
            dialogueEntries = new List<DialogueEntry>();
            
        dialogueEntries.Add(new DialogueEntry(speakerName, text));
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    /// <summary>
    /// Adiciona uma entrada de diálogo completa
    /// </summary>
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
    
    /// <summary>
    /// Remove uma entrada específica
    /// </summary>
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
    
    /// <summary>
    /// Limpa todas as entradas (útil para scripts)
    /// </summary>
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
    
    /// <summary>
    /// Verifica se o diálogo tem entradas válidas
    /// </summary>
    public bool IsValid()
    {
        return dialogueEntries != null && dialogueEntries.Count > 0 && 
               dialogueEntries.Exists(entry => entry != null && entry.IsValid());
    }
    
    /// <summary>
    /// Retorna o número de entradas no diálogo
    /// </summary>
    public int GetEntryCount()
    {
        return dialogueEntries?.Count ?? 0;
    }

    /// <summary>
    /// Inicializa a lista se for nula
    /// </summary>
    private void OnEnable()
    {
        if (dialogueEntries == null)
        {
            dialogueEntries = new List<DialogueEntry>();
        }
    }

    /// <summary>
    /// Validação e limpeza automática no Editor
    /// </summary>
    void OnValidate()
    {
        // Garante que a lista existe
        if (dialogueEntries == null)
        {
            dialogueEntries = new List<DialogueEntry>();
        }

        // Validação do título
        if (string.IsNullOrEmpty(dialogueTitle))
        {
            dialogueTitle = "New Dialogue";
        }
        
        // Remove entradas completamente vazias, mas preserva as que estão sendo editadas
        for (int i = dialogueEntries.Count - 1; i >= 0; i--)
        {
            if (dialogueEntries[i] == null)
            {
                dialogueEntries.RemoveAt(i);
            }
            else if (string.IsNullOrEmpty(dialogueEntries[i].speakerName) && 
                     string.IsNullOrEmpty(dialogueEntries[i].text))
            {
                // Remove apenas se ambos estiverem vazios
                dialogueEntries.RemoveAt(i);
            }
        }

        // Sempre garante pelo menos uma entrada vazia para facilitar edição
        if (dialogueEntries.Count == 0)
        {
            dialogueEntries.Add(new DialogueEntry());
        }
    }

    #if UNITY_EDITOR
    /// <summary>
    /// Métodos para facilitar uso no Editor
    /// </summary>
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
        AddDialogueEntry("Jogador", "Estou procurando informações sobre esta área.");
        AddDialogueEntry("NPC", "Ah, você deve ter cuidado! Há monstros perigosos por aqui.");
        AddDialogueEntry("", "O vento sopra suavemente pelas árvores...");
        
        Debug.Log("✅ Diálogo de exemplo adicionado ao " + name);
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

        Debug.Log($"📊 Validação do {name}:\n" +
                  $"• Entradas válidas: {validEntries}\n" +
                  $"• Entradas inválidas: {invalidEntries}\n" +
                  $"• Total: {dialogueEntries.Count}");
    }
    #endif
}