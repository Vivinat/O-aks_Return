// Assets/Scripts/UI/OptionsMenu.cs (Versão Manual)

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    public static OptionsMenu Instance { get; private set; }

    [Header("UI References - Configure no Inspector")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button exitGameButton;
    [SerializeField] private Button returnToMenuButton;

    [Header("Audio Controls - Configure no Inspector")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;

    [Header("Settings")]
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private bool pauseGameWhenOpen = true;

    // Estado do jogo pausado
    private bool wasGamePaused = false;
    private float originalTimeScale = 1f;

    // Keys para PlayerPrefs
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    void Awake()
    {
        // Singleton - persiste entre cenas
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(transform.root.gameObject);
            return;
        }
    }

    void Start()
    {
        InitializeMenu();
    }

    void Update()
    {
        // Tecla ESC para abrir/fechar o menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleOptionsMenu();
        }
    }

    private void InitializeMenu()
    {
        // Garante que o painel comece fechado
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        SetupButtons();
        SetupAudioSliders();
        LoadAudioSettings();

        Debug.Log("OptionsMenu inicializado com configuração manual");
    }

    private void SetupButtons()
    {
        // Configura os eventos dos botões
        if (optionsButton != null)
        {
            optionsButton.onClick.RemoveAllListeners();
            optionsButton.onClick.AddListener(OpenOptionsMenu);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseOptionsMenu);
        }

        if (exitGameButton != null)
        {
            exitGameButton.onClick.RemoveAllListeners();
            exitGameButton.onClick.AddListener(ExitGame);
        }

        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.RemoveAllListeners();
            returnToMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }

    private void SetupAudioSliders()
    {
        // Configura os eventos dos sliders
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveAllListeners();
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveAllListeners();
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    private void LoadAudioSettings()
    {
        // Carrega configurações salvas ou usa valores padrão
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.7f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

        // Aplica no AudioManager se disponível
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(musicVolume);
            AudioManager.Instance.SetSFXVolume(sfxVolume);
        }

        // Atualiza os sliders (sem disparar eventos)
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(musicVolume);
            UpdateMusicVolumeText(musicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);
            UpdateSFXVolumeText(sfxVolume);
        }

        Debug.Log($"Configurações de áudio carregadas - Música: {musicVolume:F1}, SFX: {sfxVolume:F1}");
    }

    #region Public Methods

    public void ToggleOptionsMenu()
    {
        if (optionsPanel == null)
        {
            Debug.LogWarning("OptionsPanel não foi atribuído no Inspector!");
            return;
        }

        if (optionsPanel.activeSelf)
        {
            CloseOptionsMenu();
        }
        else
        {
            OpenOptionsMenu();
        }
    }

    public void OpenOptionsMenu()
    {
        if (optionsPanel == null)
        {
            Debug.LogWarning("OptionsPanel não foi atribuído no Inspector!");
            return;
        }

        Debug.Log("Abrindo menu de opções");

        // Salva o estado atual do jogo
        if (pauseGameWhenOpen)
        {
            originalTimeScale = Time.timeScale;
            wasGamePaused = (Time.timeScale == 0f);
            Time.timeScale = 0f;
        }

        // Ativa o painel
        optionsPanel.SetActive(true);

        // Atualiza as configurações de áudio
        LoadAudioSettings();
    }

    public void CloseOptionsMenu()
    {
        if (optionsPanel == null) return;

        Debug.Log("Fechando menu de opções");

        // Desativa o painel
        optionsPanel.SetActive(false);

        // Restaura o estado do tempo apenas se o jogo não estava pausado antes
        if (pauseGameWhenOpen && !wasGamePaused)
        {
            Time.timeScale = originalTimeScale;
        }

        // Salva as configurações
        SaveAudioSettings();
    }

    public void ExitGame()
    {
        Debug.Log("Saindo do jogo...");
        SaveAudioSettings();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void ReturnToMainMenu()
    {
        Debug.Log($"Retornando ao menu principal: {menuSceneName}");
        
        SaveAudioSettings();

        // Restaura o tempo antes de mudar de cena
        Time.timeScale = 1f;

        // Limpa dados do mapa atual se aplicável
        if (GameManager.Instance != null)
        {
            PlayerPrefs.DeleteKey("LastCompletedNode");
            PlayerPrefs.DeleteKey("CompletedBossNode");
            PlayerPrefs.DeleteKey("NextSceneAfterBoss");
        }

        SceneManager.LoadScene(menuSceneName);
    }

    public bool IsMenuOpen()
    {
        return optionsPanel != null && optionsPanel.activeSelf;
    }

    public void SetOptionsButtonVisible(bool visible)
    {
        if (optionsButton != null)
        {
            optionsButton.gameObject.SetActive(visible);
        }
    }

    #endregion

    #region Audio Controls

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
        UpdateMusicVolumeText(value);
        
        // Salva imediatamente quando o usuário muda o slider
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
        UpdateSFXVolumeText(value);
        
        // Salva imediatamente quando o usuário muda o slider
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
    }

    private void UpdateMusicVolumeText(float value)
    {
        if (musicVolumeText != null)
        {
            musicVolumeText.text = $"Music:{Mathf.RoundToInt(value * 100)}";
        }
    }

    private void UpdateSFXVolumeText(float value)
    {
        if (sfxVolumeText != null)
        {
            sfxVolumeText.text = $"SFX:{Mathf.RoundToInt(value * 100)}";
        }
    }

    private void SaveAudioSettings()
    {
        if (musicVolumeSlider != null)
        {
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolumeSlider.value);
        }

        if (sfxVolumeSlider != null)
        {
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolumeSlider.value);
        }

        PlayerPrefs.Save();
        Debug.Log("Configurações de áudio salvas");
    }

    #endregion

    #region Scene Management

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Garante que o menu esteja fechado ao carregar nova cena
        if (optionsPanel != null && optionsPanel.activeSelf)
        {
            optionsPanel.SetActive(false);
            Time.timeScale = 1f;
        }

        // Reaplica as configurações de áudio
        LoadAudioSettings();
    }

    #endregion

    #region Validation

    void OnValidate()
    {
        // Validação no Editor para ajudar na configuração
        if (optionsPanel == null)
            Debug.LogWarning("OptionsMenu: optionsPanel não foi atribuído!");
            
        if (optionsButton == null)
            Debug.LogWarning("OptionsMenu: optionsButton não foi atribuído!");
            
        if (musicVolumeSlider == null)
            Debug.LogWarning("OptionsMenu: musicVolumeSlider não foi atribuído!");
            
        if (sfxVolumeSlider == null)
            Debug.LogWarning("OptionsMenu: sfxVolumeSlider não foi atribuído!");
    }

    #endregion
}