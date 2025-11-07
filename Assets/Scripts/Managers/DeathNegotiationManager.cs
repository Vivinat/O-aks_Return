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
        "Não tão rápido...",
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
        if (negotiationPanel != null && negotiationPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (StatusPanel.Instance != null)
                {
                    StatusPanel.Instance.TogglePanel();
                }
            }
        }
    }
    
    public bool IsNegotiationActive()
    {
        return negotiationPanel != null && negotiationPanel.activeSelf;
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
    
    public void StartNegotiation(BattleEntity player, BattleManager manager, System.Action<bool> callback)
    {
        if (hasUsedSecondChance)
        {
            callback?.Invoke(false);
            return;
        }
        
        targetPlayer = player;
        battleManager = manager;
        onNegotiationComplete = callback;
        refreshesUsed = 0;
        
        Time.timeScale = 0f;
        StartCoroutine(NegotiationSequence());
    }
    
    private IEnumerator NegotiationSequence()
    {
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
        
        while (!dialogueDone)
        {
            yield return null;
        }
        
        yield return new WaitForSecondsRealtime(0.5f);
        
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
    
    private void OnAcceptClicked()
    {
        if (currentOffer == null || targetPlayer == null) return;
        
        currentOffer.ApplyPenalty(targetPlayer);
        RevivePlayer();
        hasUsedSecondChance = true;
        
        StartCoroutine(ShowAcceptFeedback());
    }
    
    private IEnumerator ShowAcceptFeedback()
    {
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
        
        HideNegotiationUI();
        Time.timeScale = 1f;
        
        onNegotiationComplete?.Invoke(true);
    }
    
    private void OnRejectClicked()
    {
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
        
        HideNegotiationUI();
        Time.timeScale = 1f;
        
        onNegotiationComplete?.Invoke(false);
    }
    
    private void OnRefreshClicked()
    {
        if (refreshesUsed >= maxRefreshes) return;
        
        refreshesUsed++;
        GenerateNewOffer();
        PlayVoiceSound();
    }
    
    private void RevivePlayer()
    {
        if (targetPlayer == null) return;
        
        var hpField = typeof(BattleEntity).GetField("currentHp", 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);

        if (hpField != null)
        {
            hpField.SetValue(targetPlayer, 100);
        }
        
        targetPlayer.currentMp = 100;
        
        var deadField = typeof(BattleEntity).GetField("isDead", 
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.Instance);
        
        if (deadField != null)
        {
            deadField.SetValue(targetPlayer, false);
        }
        
        targetPlayer.EnableHUDElements();
        
        SpriteRenderer sr = targetPlayer.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
        }
        
        targetPlayer.ForceUpdateValueTexts();
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
    
    public void ResetForNewBattle()
    {
        hasUsedSecondChance = false;
        refreshesUsed = 0;
        currentOffer = null;
        targetPlayer = null;
        battleManager = null;
        
        HideNegotiationUI();
    }
    
    public bool HasUsedSecondChance()
    {
        return hasUsedSecondChance;
    }
}