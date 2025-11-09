using UnityEngine;

[CreateAssetMenu(menuName = "Events/Dialogue Event SO")]
public class DialogueEventSO : EventTypeSO
{
    
    public string dialogueText = "";
    public string speakerName = "";
    public DialogueSO dialogueData;
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
    
}