using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe utilitária com métodos estáticos para facilitar o uso do sistema de diálogo
/// </summary>
public static class DialogueUtils
{
    /// <summary>
    /// Mostra um diálogo simples com uma única fala
    /// </summary>
    public static void ShowSimpleDialogue(string speakerName, string text, System.Action onComplete = null)
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(speakerName, text, onComplete);
        }
        else
        {
            Debug.LogError("DialogueUtils: DialogueSystem não encontrado!");
        }
    }

    /// <summary>
    /// Mostra um diálogo de múltiplas falas
    /// </summary>
    public static void ShowDialogue(List<DialogueEntry> entries, System.Action onComplete = null)
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(entries, onComplete);
        }
        else
        {
            Debug.LogError("DialogueUtils: DialogueSystem não encontrado!");
        }
    }

    /// <summary>
    /// Mostra um diálogo de um ScriptableObject
    /// </summary>
    public static void ShowDialogue(DialogueSO dialogueData, System.Action onComplete = null)
    {
        if (dialogueData == null || !dialogueData.IsValid())
        {
            Debug.LogError("DialogueUtils: DialogueSO inválido!");
            return;
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueData.GetDialogueEntries(), onComplete);
        }
        else
        {
            Debug.LogError("DialogueUtils: DialogueSystem não encontrado!");
        }
    }

    /// <summary>
    /// Cria rapidamente uma conversa entre dois personagens
    /// </summary>
    public static void ShowConversation(string speaker1, string text1, string speaker2, string text2, System.Action onComplete = null)
    {
        List<DialogueEntry> conversation = new List<DialogueEntry>
        {
            new DialogueEntry(speaker1, text1),
            new DialogueEntry(speaker2, text2)
        };
        
        ShowDialogue(conversation, onComplete);
    }

    /// <summary>
    /// Mostra um diálogo de narração (sem nome de speaker)
    /// </summary>
    public static void ShowNarration(string text, System.Action onComplete = null)
    {
        ShowSimpleDialogue("", text, onComplete);
    }

    /// <summary>
    /// Cria um diálogo de introdução para batalhas
    /// </summary>
    public static void ShowBattleIntro(string enemyName, System.Action onComplete = null)
    {
        string introText = $"Um {enemyName} selvagem apareceu!";
        ShowNarration(introText, onComplete);
    }

    /// <summary>
    /// Mostra diálogo de vitória em batalha
    /// </summary>
    public static void ShowBattleVictory(int coinsEarned, System.Action onComplete = null)
    {
        string victoryText = $"Vitória! Você ganhou {coinsEarned} moedas.";
        ShowNarration(victoryText, onComplete);
    }

    /// <summary>
    /// Mostra diálogo quando encontra um tesouro
    /// </summary>
    public static void ShowTreasureFound(string itemName, System.Action onComplete = null)
    {
        string treasureText = $"Você encontrou: {itemName}!";
        ShowNarration(treasureText, onComplete);
    }

    /// <summary>
    /// Mostra diálogo de confirmação genérico
    /// </summary>
    public static void ShowConfirmation(string message, System.Action onComplete = null)
    {
        ShowNarration(message, onComplete);
    }

    /// <summary>
    /// Para todo diálogo ativo imediatamente
    /// </summary>
    public static void StopAllDialogue()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.SkipAllDialogue();
        }
    }

    /// <summary>
    /// Verifica se há diálogo ativo
    /// </summary>
    public static bool IsDialogueActive()
    {
        return DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive();
    }

    /// <summary>
    /// Builder pattern para criar diálogos complexos
    /// </summary>
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
            {
                ShowDialogue(entries, onComplete);
            }
            else
            {
                Debug.LogWarning("DialogueBuilder: Nenhuma entrada de diálogo adicionada!");
            }
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

    /// <summary>
    /// Cria um novo DialogueBuilder
    /// </summary>
    public static DialogueBuilder CreateBuilder()
    {
        return new DialogueBuilder();
    }

    /// <summary>
    /// Métodos de exemplo para diferentes situações do jogo
    /// </summary>
    public static class Examples
    {
        /// <summary>
        /// Exemplo: Diálogo de introdução do jogo
        /// </summary>
        public static void ShowGameIntro(System.Action onComplete = null)
        {
            var dialogue = CreateBuilder()
                .AddNarration("Bem-vindo ao mundo da aventura!")
                .AddLine("Guia", "Olá, jovem aventureiro! Pronto para sua jornada?")
                .AddLine("Jogador", "Sim! Estou pronto para qualquer desafio!")
                .AddLine("Guia", "Ótimo! Lembre-se: coragem e sabedoria são suas melhores armas.")
                .AddNarration("Sua aventura começou...");
                
            dialogue.Show(onComplete);
        }

        /// <summary>
        /// Exemplo: Diálogo quando completa um nó do mapa
        /// </summary>
        public static void ShowNodeComplete(string nodeName, System.Action onComplete = null)
        {
            string message = $"Você completou o desafio de {nodeName}! Novos caminhos se abrem...";
            ShowNarration(message, onComplete);
        }

        /// <summary>
        /// Exemplo: Diálogo de boss
        /// </summary>
        public static void ShowBossEncounter(string bossName, System.Action onComplete = null)
        {
            var dialogue = CreateBuilder()
                .AddNarration($"O ar fica pesado... {bossName} aparece!")
                .AddLine(bossName, "Então... outro aventureiro ousa me desafiar?")
                .AddLine(bossName, "Muito bem! Mostre-me sua força!")
                .AddNarration("A batalha final começou!");
                
            dialogue.Show(onComplete);
        }

        /// <summary>
        /// Exemplo: Diálogo de loja
        /// </summary>
        public static void ShowShopWelcome(System.Action onComplete = null)
        {
            var dialogue = CreateBuilder()
                .AddLine("Comerciante", "Bem-vindo à minha loja, aventureiro!")
                .AddLine("Comerciante", "Tenho os melhores itens da região. Dê uma olhada!")
                .AddNarration("Você pode comprar itens com suas moedas.");
                
            dialogue.Show(onComplete);
        }
    }
}