// Assets/Scripts/Dialogue/DialogueEntry.cs
// ARQUIVO SEPARADO para resolver problemas de serialização

using UnityEngine;

/// <summary>
/// Estrutura para representar uma entrada de diálogo
/// Arquivo separado para garantir serialização correta no Unity
/// </summary>
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

    /// <summary>
    /// Construtor padrão (necessário para serialização)
    /// </summary>
    public DialogueEntry()
    {
        speakerName = "";
        text = "";
    }

    /// <summary>
    /// Construtor com parâmetros
    /// </summary>
    public DialogueEntry(string speaker, string dialogue)
    {
        speakerName = speaker ?? "";
        text = dialogue ?? "";
        customDelay = 0f;
        customSound = null;
    }

    /// <summary>
    /// Construtor completo
    /// </summary>
    public DialogueEntry(string speaker, string dialogue, float delay, AudioClip sound = null)
    {
        speakerName = speaker ?? "";
        text = dialogue ?? "";
        customDelay = delay;
        customSound = sound;
    }

    /// <summary>
    /// Verifica se esta entrada é válida
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(text);
    }

    /// <summary>
    /// Verifica se é narração (sem speaker)
    /// </summary>
    public bool IsNarration()
    {
        return string.IsNullOrEmpty(speakerName);
    }

    /// <summary>
    /// Cria uma cópia desta entrada
    /// </summary>
    public DialogueEntry Clone()
    {
        return new DialogueEntry(speakerName, text, customDelay, customSound);
    }

    /// <summary>
    /// Representação em string para debug
    /// </summary>
    public override string ToString()
    {
        string speaker = string.IsNullOrEmpty(speakerName) ? "[Narração]" : speakerName;
        string preview = text.Length > 30 ? text.Substring(0, 30) + "..." : text;
        return $"{speaker}: {preview}";
    }
}