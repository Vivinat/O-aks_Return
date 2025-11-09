using UnityEngine;


//Estrutura para representar uma entrada de diálogo
[System.Serializable]
public class DialogueEntry
{
    [Header("Speaker")]
    public string speakerName = "";
    
    [Header("Dialogue Text")]
    [TextArea(3, 8)]
    public string text = "";

    [Header("Optional Settings")]
    [Tooltip("Tempo extra para esta fala (0 = padrão)")]
    public float customDelay = 0f;
    
    [Tooltip("Som específico para esta fala")]
    public AudioClip customSound;

    public DialogueEntry()
    {
        speakerName = "";
        text = "";
    }

    public DialogueEntry(string speaker, string dialogue)
    {
        speakerName = speaker ?? "";
        text = dialogue ?? "";
        customDelay = 0f;
        customSound = null;
    }

    public DialogueEntry(string speaker, string dialogue, float delay, AudioClip sound = null)
    {
        speakerName = speaker ?? "";
        text = dialogue ?? "";
        customDelay = delay;
        customSound = sound;
    }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(text);
    }

    public bool IsNarration()
    {
        return string.IsNullOrEmpty(speakerName);
    }

    public DialogueEntry Clone()
    {
        return new DialogueEntry(speakerName, text, customDelay, customSound);
    }

    public override string ToString()
    {
        string speaker = string.IsNullOrEmpty(speakerName) ? "[Narração]" : speakerName;
        string preview = text.Length > 30 ? text.Substring(0, 30) + "..." : text;
        return $"{speaker}: {preview}";
    }
}