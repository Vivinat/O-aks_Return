// Assets/Scripts/UI/OptionsMenu.cs (Versão Corrigida - Bug de Pausa Resolvido)

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

    // Keys para PlayerPrefs
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    void Awake()
    {
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
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        SetupButtons();
        SetupAudioSliders();
        LoadAudioSettings();

        Debug.Log("OptionsMenu inicializado com configuração manual");
    }

    private void SetupButtons()
    {
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
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.7f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(musicVolume);
            AudioManager.Instance.SetSFXVolume(sfxVolume);
        }

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
        AudioConstants.PlayMenuOpen();

        if (pauseGameWhenOpen)
        {
            Time.timeScale = 0f;
            Debug.Log($"Jogo pausado. Time.timeScale = {Time.timeScale}");
        }

        // Notifica o BattleHUD se existir
        BattleHUD battleHUD = FindObjectOfType<BattleHUD>();
        if (battleHUD != null)
        {
            battleHUD.OnGamePaused();
        }

        optionsPanel.SetActive(true);
        LoadAudioSettings();
    }

    public void CloseOptionsMenu()
    {
        if (optionsPanel == null) return;

        Debug.Log("Fechando menu de opções");

        optionsPanel.SetActive(false);

        // CORRIGIDO: Sempre restaura o Time.timeScale para 1 quando fecha
        if (pauseGameWhenOpen)
        {
            Time.timeScale = 1f;
            Debug.Log($"Jogo retomado. Time.timeScale = {Time.timeScale}");
        }

        // Notifica o BattleHUD se existir
        BattleHUD battleHUD = FindObjectOfType<BattleHUD>();
        if (battleHUD != null)
        {
            battleHUD.OnGameResumed();
        }

        SaveAudioSettings();
    }

    public void ExitGame()
    {
        Debug.Log("Saindo do jogo...");
        SaveAudioSettings();

        // CORRIGIDO: Restaura o timeScale antes de sair
        Time.timeScale = 1f;

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

        // CORRIGIDO: Restaura o tempo antes de mudar de cena
        Time.timeScale = 1f;

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
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
        UpdateSFXVolumeText(value);
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
        // CORRIGIDO: Garante que o timeScale está correto ao carregar nova cena
        if (optionsPanel != null && optionsPanel.activeSelf)
        {
            optionsPanel.SetActive(false);
        }
        
        Time.timeScale = 1f;
        Debug.Log($"Nova cena carregada: {scene.name}. Time.timeScale resetado para 1");

        LoadAudioSettings();
    }

    #endregion

    #region Validation

    void OnValidate()
    {
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