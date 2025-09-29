// Assets/Scripts/Events/DialogueEventSO.cs

using UnityEngine;

/// <summary>
/// Evento de diálogo que pode ser executado diretamente no mapa sem trocar de cena.
/// Suporta tanto texto direto quanto referência a DialogueSO.
/// </summary>
[CreateAssetMenu(menuName = "Events/Dialogue Event SO")]
public class DialogueEventSO : EventTypeSO
{
    [Header("Dialogue Configuration")]
    [Tooltip("Texto do diálogo a ser exibido. Se preenchido, será usado em vez do DialogueSO.")]
    [TextArea(3, 8)]
    public string dialogueText = "";
    
    [Tooltip("Nome do speaker para o diálogo simples (opcional)")]
    public string speakerName = "";
    
    [Header("Advanced Dialogue")]
    [Tooltip("ScriptableObject de diálogo complexo (opcional). Ignorado se dialogueText estiver preenchido.")]
    public DialogueSO dialogueData;
    
    [Header("Audio Configuration")]
    [Tooltip("Som específico para este diálogo (opcional)")]
    public AudioClip dialogueSound;
    
    /// <summary>
    /// Verifica se este evento tem conteúdo de diálogo válido
    /// </summary>
    public bool HasValidDialogue()
    {
        return !string.IsNullOrEmpty(dialogueText) || 
               (dialogueData != null && dialogueData.IsValid());
    }
    
    /// <summary>
    /// Retorna o texto do diálogo, priorizando o texto direto
    /// </summary>
    public string GetDialogueText()
    {
        if (!string.IsNullOrEmpty(dialogueText))
        {
            return dialogueText;
        }
        
        if (dialogueData != null && dialogueData.GetDialogueEntries().Count > 0)
        {
            // Se for um DialogueSO com uma única entrada, retorna o texto
            var entries = dialogueData.GetDialogueEntries();
            if (entries.Count == 1)
            {
                return entries[0].text;
            }
        }
        
        return "Diálogo não configurado.";
    }
    
    /// <summary>
    /// Retorna o nome do speaker
    /// </summary>
    public string GetSpeakerName()
    {
        if (!string.IsNullOrEmpty(speakerName))
        {
            return speakerName;
        }
        
        if (dialogueData != null && dialogueData.GetDialogueEntries().Count > 0)
        {
            return dialogueData.GetDialogueEntries()[0].speakerName;
        }
        
        return "";
    }
    
    /// <summary>
    /// Verifica se deve usar o DialogueSO em vez do texto simples
    /// </summary>
    public bool ShouldUseDialogueSO()
    {
        return string.IsNullOrEmpty(dialogueText) && 
               dialogueData != null && 
               dialogueData.IsValid();
    }
    
    void OnValidate()
    {
        // Validação no Editor
        if (string.IsNullOrEmpty(dialogueText) && dialogueData == null)
        {
            Debug.LogWarning($"DialogueEventSO '{name}': Nem dialogueText nem dialogueData foram configurados!");
        }
        
        if (!string.IsNullOrEmpty(dialogueText) && dialogueData != null)
        {
            Debug.LogWarning($"DialogueEventSO '{name}': Tanto dialogueText quanto dialogueData estão configurados. dialogueText será usado.");
        }
    }
}