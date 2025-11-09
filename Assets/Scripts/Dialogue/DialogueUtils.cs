using System.Collections.Generic;
using UnityEngine;

// Classe utilitária para facilitar o uso do sistema de diálogo
public static class DialogueUtils
{
    public static void ShowSimpleDialogue(string speakerName, string text, System.Action onComplete = null)
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(speakerName, text, onComplete);
        else
            Debug.LogError("DialogueSystem não encontrado!");
    }

    public static void ShowDialogue(List<DialogueEntry> entries, System.Action onComplete = null)
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(entries, onComplete);
        else
            Debug.LogError("DialogueSystem não encontrado!");
    }

    public static void ShowDialogue(DialogueSO dialogueData, System.Action onComplete = null)
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(dialogueData.GetDialogueEntries(), onComplete);
        else
            Debug.LogError("DialogueSystem não encontrado!");
    }

    public static void ShowNarration(string text, System.Action onComplete = null)
    {
        ShowSimpleDialogue("", text, onComplete);
    }
    
    public class DialogueBuilder
    {
        private List<DialogueEntry> entries = new List<DialogueEntry>();

        public DialogueBuilder AddLine(string speaker, string text)
        {
            entries.Add(new DialogueEntry(speaker, text));
            return this;
        }

        public DialogueBuilder AddNarration(string text)
        {
            entries.Add(new DialogueEntry("", text));
            return this;
        }

        public DialogueBuilder AddConversation(string speaker1, string text1, string speaker2, string text2)
        {
            entries.Add(new DialogueEntry(speaker1, text1));
            entries.Add(new DialogueEntry(speaker2, text2));
            return this;
        }

        public void Show(System.Action onComplete = null)
        {
            if (entries.Count > 0)
                ShowDialogue(entries, onComplete);
            else
                Debug.LogWarning("DialogueBuilder: Nenhuma entrada adicionada!");
        }

        public List<DialogueEntry> Build()
        {
            return new List<DialogueEntry>(entries);
        }

        public void Clear()
        {
            entries.Clear();
        }
    }

    public static DialogueBuilder CreateBuilder()
    {
        return new DialogueBuilder();
    }
}