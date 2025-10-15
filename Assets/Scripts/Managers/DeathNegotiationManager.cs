// Assets/Scripts/Difficulty_System/DeathNegotiationManager.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gerencia o sistema de segunda chance quando o jogador morre
/// </summary>
public class DeathNegotiationManager : MonoBehaviour
{
    public static DeathNegotiationManager Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject negotiationPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button rejectButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private TextMeshProUGUI refreshCountText;
    
    [Header("Voice Lines")]
    [SerializeField] private List<string> introLines = new List<string>
    {
        "Então... este é o seu fim?",
        "Não tão rápido, mortal.",
        "Você realmente pensou que seria tão fácil?",
        "A morte não é o fim... não necessariamente."
    };
    
    [SerializeField] private List<string> offerLines = new List<string>
    {
        "Posso lhe oferecer outra chance... por um preço.",
        "Eu posso trazê-lo de volta, mas isso custará caro.",
        "Vida por vida... ou melhor, vida por poder.",
        "Que tal um acordo? Sua vida continua, mas algo deve ser sacrificado."
    };
    
    [Header("Settings")]
    [SerializeField] private int maxRefreshes = 2;
    [SerializeField] private AudioClip voiceSound;
    
    // Estado interno
    private bool hasUsedSecondChance = false;
    private int refreshesUsed = 0;
    private DeathNegotiationOffer currentOffer;
    private BattleEntity targetPlayer;
    private BattleManager battleManager;
    private System.Action<bool> onNegotiationComplete;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        SetupUI();
    }
    
    void Update()
    {
        // Permite abrir menu de status com E durante negociação
        if (negotiationPanel != null && negotiationPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                // TODO: Abrir menu de status
                Debug.Log("Menu de status (implemente StatusMenuManager)");
            }
        }
    }
    
    private void SetupUI()
    {
        if (negotiationPanel != null)
        {
            negotiationPanel.SetActive(false);
        }
        
        if (acceptButton != null)
        {
            acceptButton.onClick.AddListener(OnAcceptClicked);
        }
        
        if (rejectButton != null)
        {
            rejectButton.onClick.AddListener(OnRejectClicked);
        }
        
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(OnRefreshClicked);
        }
    }
    
    /// <summary>
    /// Inicia o processo de negociação
    /// </summary>
    public void StartNegotiation(BattleEntity player, BattleManager manager, System.Action<bool> callback)
    {
        // Verifica se já usou a segunda chance nesta batalha
        if (hasUsedSecondChance)
        {
            Debug.Log("Segunda chance já foi usada nesta batalha!");
            callback?.Invoke(false);
            return;
        }
        
        targetPlayer = player;
        battleManager = manager;
        onNegotiationComplete = callback;
        refreshesUsed = 0;
        
        // Pausa o jogo
        Time.timeScale = 0f;
        
        // Inicia sequência de diálogo
        StartCoroutine(NegotiationSequence());
    }
    
    private IEnumerator NegotiationSequence()
    {
        // Fase 1: Introdução com voz misteriosa
        List<DialogueEntry> introDialogue = new List<DialogueEntry>
        {
            new DialogueEntry("???", GetRandomLine(introLines)),
            new DialogueEntry("???", GetRandomLine(offerLines))
        };
        
        bool dialogueDone = false;
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(introDialogue, () => dialogueDone = true);
        }
        else
        {
            dialogueDone = true;
        }
        
        // Espera diálogo terminar (usando unscaled time)
        while (!dialogueDone)
        {
            yield return null;
        }
        
        yield return new WaitForSecondsRealtime(0.5f);
        
        // Fase 2: Mostra UI de negociação
        GenerateNewOffer();
        ShowNegotiationUI();
    }
    
    private void GenerateNewOffer()
    {
        currentOffer = DeathOfferGenerator.GenerateOffer(targetPlayer);
        UpdateOfferDisplay();
    }
    
    private void UpdateOfferDisplay()
    {
        if (currentOffer == null) return;
        
        if (titleText != null)
        {
            titleText.text = currentOffer.title;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = currentOffer.GetFormattedDescription();
        }
        
        UpdateRefreshButton();
    }
    
    private void UpdateRefreshButton()
    {
        if (refreshButton != null)
        {
            bool canRefresh = refreshesUsed < maxRefreshes;
            refreshButton.interactable = canRefresh;
            
            // Muda cor do botão se desabilitado
            var colors = refreshButton.colors;
            if (!canRefresh)
            {
                colors.normalColor = Color.gray;
                colors.highlightedColor = Color.gray;
                colors.pressedColor = Color.gray;
            }
            else
            {
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
                colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            }
            refreshButton.colors = colors;
        }
        
        if (refreshCountText != null)
        {
            int remaining = maxRefreshes - refreshesUsed;
            refreshCountText.text = $"Recarregar ({remaining}/{maxRefreshes})";
        }
    }
    
    private void ShowNegotiationUI()
    {
        if (negotiationPanel != null)
        {
            negotiationPanel.SetActive(true);
        }
        
        PlayVoiceSound();
    }
    
    private void HideNegotiationUI()
    {
        if (negotiationPanel != null)
        {
            negotiationPanel.SetActive(false);
        }
    }
    
    #region Button Callbacks
    
    private void OnAcceptClicked()
    {
        Debug.Log("=== SEGUNDA CHANCE ACEITA ===");
        
        if (currentOffer == null || targetPlayer == null)
        {
            Debug.LogError("Oferta ou jogador inválido!");
            return;
        }
        
        // Aplica penalidade
        currentOffer.ApplyPenalty(targetPlayer);
        
        // Revive o jogador
        RevivePlayer();
        
        // Marca que usou a segunda chance
        hasUsedSecondChance = true;
        
        // Mostra feedback
        StartCoroutine(ShowAcceptFeedback());
    }
    
    private IEnumerator ShowAcceptFeedback()
    {
        // Mostra diálogo de confirmação
        List<DialogueEntry> acceptDialogue = new List<DialogueEntry>
        {
            new DialogueEntry("???", "Ave, Logrif! Acordo fechado."),
            new DialogueEntry("???", "Nos veremos em breve...")
        };
        
        bool dialogueDone = false;
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(acceptDialogue, () => dialogueDone = true);
        }
        else
        {
            dialogueDone = true;
        }
        
        while (!dialogueDone)
        {
            yield return null;
        }
        
        // Fecha UI e resume batalha
        HideNegotiationUI();
        Time.timeScale = 1f;
        
        onNegotiationComplete?.Invoke(true);
    }
    
    private void OnRejectClicked()
    {
        Debug.Log("=== SEGUNDA CHANCE RECUSADA ===");
        
        // Mostra diálogo de rejeição
        StartCoroutine(ShowRejectFeedback());
    }
    
    private IEnumerator ShowRejectFeedback()
    {
        List<DialogueEntry> rejectDialogue = new List<DialogueEntry>
        {
            new DialogueEntry("???", "Orgulho... ou estupidez?"),
            new DialogueEntry("???", "Descanse em paz... ou não.")
        };
        
        bool dialogueDone = false;
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(rejectDialogue, () => dialogueDone = true);
        }
        else
        {
            dialogueDone = true;
        }
        
        while (!dialogueDone)
        {
            yield return null;
        }
        
        // Fecha UI e confirma morte
        HideNegotiationUI();
        Time.timeScale = 1f;
        
        onNegotiationComplete?.Invoke(false);
    }
    
    private void OnRefreshClicked()
    {
        if (refreshesUsed >= maxRefreshes)
        {
            Debug.Log("Máximo de recarregamentos atingido!");
            return;
        }
        
        refreshesUsed++;
        Debug.Log($"Recarregamento {refreshesUsed}/{maxRefreshes}");
        
        // Gera nova oferta
        GenerateNewOffer();
        
        PlayVoiceSound();
    }
    
    #endregion
    
    #region Helper Methods
    
    private void RevivePlayer()
    {
        if (targetPlayer == null) return;
        
        // Cura 100 HP e 100 MP
        var hpField = typeof(BattleEntity).GetField("currentHp", 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        if (hpField != null)
        {
            hpField.SetValue(targetPlayer, 100);
        }
        
        targetPlayer.currentMp = 100;
        
        // Marca como vivo
        var deadField = typeof(BattleEntity).GetField("isDead", 
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.Instance);
        
        if (deadField != null)
        {
            deadField.SetValue(targetPlayer, false);
        }
        
        // Reativa HUD
        targetPlayer.EnableHUDElements();
        
        // Reativa sprite
        SpriteRenderer sr = targetPlayer.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
        }
        
        // Força update das barras
        targetPlayer.ForceUpdateValueTexts();
        
        Debug.Log($"Jogador revivido: HP=100, MP=100");
    }
    
    private string GetRandomLine(List<string> lines)
    {
        if (lines == null || lines.Count == 0)
        {
            return "...";
        }
        
        return lines[Random.Range(0, lines.Count)];
    }
    
    private void PlayVoiceSound()
    {
        if (voiceSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(voiceSound);
        }
    }
    
    #endregion
    
    /// <summary>
    /// Reseta o estado para nova batalha
    /// </summary>
    public void ResetForNewBattle()
    {
        hasUsedSecondChance = false;
        refreshesUsed = 0;
        currentOffer = null;
        targetPlayer = null;
        battleManager = null;
        
        HideNegotiationUI();
        
        Debug.Log("Sistema de segunda chance resetado para nova batalha");
    }
    
    /// <summary>
    /// Verifica se o sistema já foi usado nesta batalha
    /// </summary>
    public bool HasUsedSecondChance()
    {
        return hasUsedSecondChance;
    }
}