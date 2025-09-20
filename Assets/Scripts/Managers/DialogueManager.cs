// Assets/Scripts/Dialogue/DialogueSystem.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Sistema de diálogo centralizado que pode ser invocado de qualquer lugar do jogo.
/// Gerencia pausa automática do jogo e efeito de digitação.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button continueButton; // Botão invisível para capturar cliques
    [SerializeField] private GameObject skipIndicator; // Indicador visual para continuar

    [Header("Animation Settings")]
    [SerializeField] private float typewriterSpeed = 0.03f;
    [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float panelFadeTime = 0.3f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip typewriterSound;
    [SerializeField] private AudioClip dialogueOpenSound;
    [SerializeField] private AudioClip dialogueCloseSound;

    // Estado interno
    private Queue<DialogueEntry> dialogueQueue = new Queue<DialogueEntry>();
    private Coroutine typewriterCoroutine;
    private bool isTyping = false;
    private bool isDialogueActive = false;
    private float originalTimeScale = 1f;
    private bool wasBattleRunning = false;
    private int clickCount = 0;
    private float lastClickTime = 0f;
    private const float doubleClickTime = 0.3f;
    private string currentFullText = ""; // NOVO: Armazena o texto completo atual
    private bool waitingForInput = false; // NOVO: Flag para controlar input
    private bool skipTyping = false; // <<< ADICIONE ESTA LINHA


    // Callbacks
    private System.Action onDialogueComplete;

    // Referências para controle de pausa
    private BattleManager battleManager;
    private CanvasGroup dialogueCanvasGroup;

    void Awake()
    {
        // Singleton pattern
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
        // Garante que o diálogo comece fechado
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    void Update()
    {
        // Permite ESC para pular diálogo rapidamente (modo debug)
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Escape))
        {
            SkipAllDialogue();
        }
    }

    private void SetupComponents()
    {
        // Setup do CanvasGroup para fade
        dialogueCanvasGroup = dialoguePanel?.GetComponent<CanvasGroup>();
        if (dialogueCanvasGroup == null && dialoguePanel != null)
        {
            dialogueCanvasGroup = dialoguePanel.AddComponent<CanvasGroup>();
        }

        // Setup do botão invisível para capturar cliques
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnDialogueClick);
            
            Image buttonImage = continueButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                // Torna o botão invisível mas funcional
                Color transparent = buttonImage.color;
                transparent.a = 0f;
                buttonImage.color = transparent;
                buttonImage.raycastTarget = true; 
            }
        }

        // Setup inicial do indicador
        if (skipIndicator != null)
            skipIndicator.SetActive(false);
    }

    #region Public API

    /// <summary>
    /// Inicia um diálogo com uma única entrada
    /// </summary>
    public void StartDialogue(string speakerName, string text, System.Action onComplete = null)
    {
        List<DialogueEntry> entries = new List<DialogueEntry>
        {
            new DialogueEntry(speakerName, text)
        };
        StartDialogue(entries, onComplete);
    }

    /// <summary>
    /// Inicia um diálogo com múltiplas entradas
    /// </summary>
    public void StartDialogue(List<DialogueEntry> entries, System.Action onComplete = null)
    {
        if (entries == null || entries.Count == 0)
        {
            Debug.LogWarning("DialogueSystem: Lista de diálogos vazia!");
            return;
        }

        // Armazena callback
        onDialogueComplete = onComplete;

        // Prepara a fila de diálogos
        dialogueQueue.Clear();
        foreach (var entry in entries)
        {
            dialogueQueue.Enqueue(entry);
        }

        // Inicia o sistema
        StartCoroutine(BeginDialogueSequence());
    }

    /// <summary>
    /// Para todo o diálogo imediatamente
    /// </summary>
    public void SkipAllDialogue()
    {
        if (!isDialogueActive) return;

        StopAllCoroutines();
        dialogueQueue.Clear();
        EndDialogue();
    }

    /// <summary>
    /// Verifica se há diálogo ativo
    /// </summary>
    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    #endregion

    #region Dialogue Flow

    private IEnumerator BeginDialogueSequence()
    {
        Debug.Log("DialogueSystem: Iniciando sequência de diálogo");
        
        PauseGame();
        if (speakerNameText != null) speakerNameText.text = "";
        if (dialogueText != null) dialogueText.text = "";
        yield return StartCoroutine(ShowDialoguePanel());
        
        isDialogueActive = true;
        PlayAudio(dialogueOpenSound);
        
        // Processa todos os diálogos na fila
        while (dialogueQueue.Count > 0)
        {
            DialogueEntry currentEntry = dialogueQueue.Dequeue();
            yield return StartCoroutine(DisplayDialogue(currentEntry));
            
            // Espera o jogador clicar para continuar (exceto no último)
            if (dialogueQueue.Count > 0)
            {
                yield return StartCoroutine(WaitForPlayerInput());
            }
        }
        
        // Espera input final para fechar
        yield return StartCoroutine(WaitForPlayerInput());
        
        EndDialogue();
    }

    private IEnumerator DisplayDialogue(DialogueEntry entry)
    {
        skipTyping = false; // <<< ADICIONE ESTA LINHA PARA RESETAR O ESTADO

        if (speakerNameText != null)
        {
            speakerNameText.text = entry.speakerName;
        }

        // Limpa o texto anterior
        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        // NOVO: Armazena o texto completo
        currentFullText = entry.text;

        // Esconde o indicador durante a digitação
        if (skipIndicator != null)
            skipIndicator.SetActive(false);

        // Inicia o efeito de digitação
        isTyping = true;
        waitingForInput = false;
        typewriterCoroutine = StartCoroutine(TypewriterEffect(entry.text));
        
        yield return typewriterCoroutine;
        
        isTyping = false;
        
        // Mostra o indicador quando terminar de digitar
        if (skipIndicator != null)
            skipIndicator.SetActive(true);
    }

    private IEnumerator TypewriterEffect(string fullText)
    {
        string currentText = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            // VERIFICA A CADA LETRA SE O JOGADOR PEDIU PARA PULAR
            if (skipTyping)
            {
                break; // Sai do loop de digitação imediatamente
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
            
            // Verifica input por teclado (além do clique do botão)
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                waitingForInput = false;
            }
        }
    }

// Em DialogueManager.cs

    private void OnDialogueClick()
    {
        Debug.Log($"Clique Recebido! Estado: [isTyping = {isTyping}] --- [waitingForInput = {waitingForInput}]");
        // COMPORTAMENTO 1: Se está digitando, envia o "pedido" para pular.
        if (isTyping)
        {
            skipTyping = true;
        }
        // COMPORTAMENTO 2: Se não está digitando, avança para o próximo diálogo.
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
            
            // Mostra o texto completo imediatamente
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
        Debug.Log("DialogueSystem: Pausando o jogo");
        
        // Salva o estado atual do tempo
        originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        
        // Pausa batalhas específicamente
        battleManager = FindObjectOfType<BattleManager>();
        if (battleManager != null && battleManager.currentState == BattleState.RUNNING)
        {
            wasBattleRunning = true;
            // A batalha naturalmente para de processar quando Time.timeScale = 0
        }
        
        // Desabilita interações com nós do mapa
        DisableMapInteractions(true);
        
        Debug.Log($"Jogo pausado. TimeScale: {Time.timeScale}");
    }

    private void ResumeGame()
    {
        Debug.Log("DialogueSystem: Resumindo o jogo");
        
        // Restaura o tempo
        Time.timeScale = originalTimeScale;
        
        // Restaura batalhas
        if (battleManager != null && wasBattleRunning)
        {
            wasBattleRunning = false;
            // A batalha automaticamente continua quando Time.timeScale volta ao normal
        }
        
        // Reabilita interações
        DisableMapInteractions(false);
        
        Debug.Log($"Jogo resumido. TimeScale: {Time.timeScale}");
    }

    private void DisableMapInteractions(bool disable)
    {
        // Desabilita cliques nos nós do mapa
        MapNode[] mapNodes = FindObjectsOfType<MapNode>();
        foreach (var node in mapNodes)
        {
            Collider2D collider = node.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = !disable;
            }
        }
        
        // Desabilita outros sistemas interativos se necessário
        // (Adicione aqui conforme sua necessidade)
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
                // Fade in suave
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
            // Fade out suave
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
        Debug.Log("DialogueSystem: Finalizando diálogo");
        
        StartCoroutine(EndDialogueSequence());
    }

    private IEnumerator EndDialogueSequence()
    {
        PlayAudio(dialogueCloseSound);
        
        // Esconde o painel
        yield return StartCoroutine(HideDialoguePanel());
        
        // Limpa estado
        isDialogueActive = false;
        isTyping = false;
        waitingForInput = false;
        clickCount = 0;
        currentFullText = "";
        
        if (skipIndicator != null)
            skipIndicator.SetActive(false);
        
        // Resume o jogo
        ResumeGame();
        
        // Chama callback se existir
        onDialogueComplete?.Invoke();
        onDialogueComplete = null;
        
        Debug.Log("DialogueSystem: Diálogo finalizado");
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
        // Validação no Editor
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

// REMOVIDO DialogueEntry daqui - agora está em arquivo separado