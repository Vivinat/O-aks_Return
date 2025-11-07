// Assets/Scripts/Dialogue/DialogueSystem.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Sistema de diálogo centralizado com pausa automática e efeito de digitação
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject skipIndicator;

    [Header("Animation Settings")]
    [SerializeField] private float typewriterSpeed = 0.03f;
    [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float panelFadeTime = 0.3f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip typewriterSound;
    [SerializeField] private AudioClip dialogueOpenSound;
    [SerializeField] private AudioClip dialogueCloseSound;

    private Queue<DialogueEntry> dialogueQueue = new Queue<DialogueEntry>();
    private Coroutine typewriterCoroutine;
    private bool isTyping = false;
    private bool isDialogueActive = false;
    private float originalTimeScale = 1f;
    private bool wasBattleRunning = false;
    private string currentFullText = "";
    private bool waitingForInput = false;
    private bool skipTyping = false;
    private System.Action onDialogueComplete;
    private BattleManager battleManager;
    private CanvasGroup dialogueCanvasGroup;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupComponents();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Escape))
        {
            SkipAllDialogue();
        }
    }

    private void SetupComponents()
    {
        dialogueCanvasGroup = dialoguePanel?.GetComponent<CanvasGroup>();
        if (dialogueCanvasGroup == null && dialoguePanel != null)
        {
            dialogueCanvasGroup = dialoguePanel.AddComponent<CanvasGroup>();
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnDialogueClick);
            
            Image buttonImage = continueButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                Color transparent = buttonImage.color;
                transparent.a = 0f;
                buttonImage.color = transparent;
                buttonImage.raycastTarget = true; 
            }
        }

        if (skipIndicator != null)
            skipIndicator.SetActive(false);
    }

    #region Public API

    public void StartDialogue(string speakerName, string text, System.Action onComplete = null)
    {
        List<DialogueEntry> entries = new List<DialogueEntry>
        {
            new DialogueEntry(speakerName, text)
        };
        StartDialogue(entries, onComplete);
    }

    public void StartDialogue(List<DialogueEntry> entries, System.Action onComplete = null)
    {
        if (entries == null || entries.Count == 0)
        {
            return;
        }

        onDialogueComplete = onComplete;

        dialogueQueue.Clear();
        foreach (var entry in entries)
        {
            dialogueQueue.Enqueue(entry);
        }

        StartCoroutine(BeginDialogueSequence());
    }

    public void SkipAllDialogue()
    {
        if (!isDialogueActive) return;

        StopAllCoroutines();
        dialogueQueue.Clear();
        EndDialogue();
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    #endregion

    #region Dialogue Flow

    private IEnumerator BeginDialogueSequence()
    {
        PauseGame();
        if (speakerNameText != null) speakerNameText.text = "";
        if (dialogueText != null) dialogueText.text = "";
        yield return StartCoroutine(ShowDialoguePanel());
        
        isDialogueActive = true;
        PlayAudio(dialogueOpenSound);
        
        while (dialogueQueue.Count > 0)
        {
            DialogueEntry currentEntry = dialogueQueue.Dequeue();
            yield return StartCoroutine(DisplayDialogue(currentEntry));
            
            if (dialogueQueue.Count > 0)
            {
                yield return StartCoroutine(WaitForPlayerInput());
            }
        }
        
        yield return StartCoroutine(WaitForPlayerInput());
        
        EndDialogue();
    }

    private IEnumerator DisplayDialogue(DialogueEntry entry)
    {
        skipTyping = false;

        if (speakerNameText != null)
        {
            speakerNameText.text = entry.speakerName;
        }

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        currentFullText = entry.text;

        if (skipIndicator != null)
            skipIndicator.SetActive(false);

        isTyping = true;
        waitingForInput = false;
        typewriterCoroutine = StartCoroutine(TypewriterEffect(entry.text));
        
        yield return typewriterCoroutine;
        
        isTyping = false;
        
        if (skipIndicator != null)
            skipIndicator.SetActive(true);
    }

    private IEnumerator TypewriterEffect(string fullText)
    {
        string currentText = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            if (skipTyping)
            {
                break;
            }

            currentText += fullText[i];

            if (dialogueText != null)
                dialogueText.text = currentText;

            if (typewriterSound != null && i % 3 == 0)
            {
                PlayAudio(typewriterSound);
            }

            yield return new WaitForSecondsRealtime(typewriterSpeed);
        }
        
        if (dialogueText != null)
            dialogueText.text = fullText;
    }

    private IEnumerator WaitForPlayerInput()
    {
        waitingForInput = true;
        
        while (waitingForInput)
        {
            yield return null;
            
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                waitingForInput = false;
            }
        }
    }

    private void OnDialogueClick()
    {
        if (isTyping)
        {
            skipTyping = true;
        }
        else if (waitingForInput)
        {
            waitingForInput = false;
        }
    }
    
    private void CompleteCurrentText()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            isTyping = false;
            
            if (dialogueText != null && !string.IsNullOrEmpty(currentFullText))
            {
                dialogueText.text = currentFullText;
            }
            
            if (skipIndicator != null)
                skipIndicator.SetActive(true);
        }
    }

    #endregion

    #region Game State Management

    private void PauseGame()
    {
        originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        
        battleManager = FindObjectOfType<BattleManager>();
        if (battleManager != null && battleManager.currentState == BattleState.RUNNING)
        {
            wasBattleRunning = true;
        }
        
        DisableMapInteractions(true);
    }

    private void ResumeGame()
    {
        Time.timeScale = originalTimeScale;
        
        if (battleManager != null && wasBattleRunning)
        {
            wasBattleRunning = false;
        }
        
        DisableMapInteractions(false);
    }

    private void DisableMapInteractions(bool disable)
    {
        MapNode[] mapNodes = FindObjectsOfType<MapNode>();
        foreach (var node in mapNodes)
        {
            Collider2D collider = node.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = !disable;
            }
        }
    }

    #endregion

    #region UI Animations

    private IEnumerator ShowDialoguePanel()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            
            if (dialogueCanvasGroup != null)
            {
                float elapsedTime = 0f;
                
                while (elapsedTime < panelFadeTime)
                {
                    elapsedTime += Time.unscaledDeltaTime;
                    float progress = elapsedTime / panelFadeTime;
                    float alpha = fadeInCurve.Evaluate(progress);
                    
                    dialogueCanvasGroup.alpha = alpha;
                    
                    yield return null;
                }
                
                dialogueCanvasGroup.alpha = 1f;
            }
        }
    }

    private IEnumerator HideDialoguePanel()
    {
        if (dialogueCanvasGroup != null)
        {
            float elapsedTime = 0f;
            float startAlpha = dialogueCanvasGroup.alpha;
            
            while (elapsedTime < panelFadeTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / panelFadeTime;
                float alpha = Mathf.Lerp(startAlpha, 0f, progress);
                
                dialogueCanvasGroup.alpha = alpha;
                
                yield return null;
            }
            
            dialogueCanvasGroup.alpha = 0f;
        }
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    #endregion

    #region End Dialogue

    private void EndDialogue()
    {
        StartCoroutine(EndDialogueSequence());
    }

    private IEnumerator EndDialogueSequence()
    {
        PlayAudio(dialogueCloseSound);
        
        yield return StartCoroutine(HideDialoguePanel());
        
        isDialogueActive = false;
        isTyping = false;
        waitingForInput = false;
        currentFullText = "";
        
        if (skipIndicator != null)
            skipIndicator.SetActive(false);
        
        ResumeGame();
        
        onDialogueComplete?.Invoke();
        onDialogueComplete = null;
    }

    #endregion

    #region Audio

    private void PlayAudio(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip);
        }
    }

    #endregion

    #region Validation

    void OnValidate()
    {
        if (dialoguePanel == null)
            Debug.LogWarning("DialogueSystem: dialoguePanel não foi atribuído!");
            
        if (speakerNameText == null)
            Debug.LogWarning("DialogueSystem: speakerNameText não foi atribuído!");
            
        if (dialogueText == null)
            Debug.LogWarning("DialogueSystem: dialogueText não foi atribuído!");
            
        if (continueButton == null)
            Debug.LogWarning("DialogueSystem: continueButton não foi atribuído!");
    }

    #endregion
}