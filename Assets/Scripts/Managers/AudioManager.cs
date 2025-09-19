// Assets/Scripts/Managers/AudioManager.cs

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")] [SerializeField]
    private AudioSource musicSource;

    [SerializeField] private AudioSource sfxSource;

    [Header("Default Music")] [SerializeField]
    private AudioClip defaultMapMusic;

    [Header("Audio Settings")] [Range(0f, 1f)] [SerializeField]
    private float musicVolume = 0.7f;

    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private float musicFadeTime = 1f;

    // Keys para PlayerPrefs
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    // Estado interno
    private AudioClip currentMusic;
    private bool isFading = false;
    private Coroutine fadeCoroutine;

    // Sistema de música pendente para eventos
    private AudioClip pendingEventMusic; // Música que deve tocar no próximo evento
    private AudioClip savedMapMusic; // Música do mapa para retornar

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAudioSettings(); // NOVO: Carrega configurações salvas
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // PRIMEIRO: Verifica se há música pendente de evento (do nó)
        if (pendingEventMusic != null)
        {
            Debug.Log($"AudioManager: Tocando música do evento '{pendingEventMusic.name}' (configurada no nó)");
            PlayMusic(pendingEventMusic, true);
            pendingEventMusic = null;
            return;
        }

        // SEGUNDO: Procura por um MapMusicSetup na cena (apenas para mapas)
        MapMusicSetup sceneMusicSetup = FindObjectOfType<MapMusicSetup>();

        if (sceneMusicSetup != null)
        {
            // Se encontrou um setup (cena de mapa), usa a música do setup
            AudioClip sceneMusic = sceneMusicSetup.GetMapMusic();
            Debug.Log($"AudioManager: Cena de mapa detectada - tocando música '{sceneMusic?.name ?? "nenhuma"}'");
            PlayMusic(sceneMusic, sceneMusicSetup.useFade);
        }
        else
        {
            // Se não encontrou setup E não tem música pendente, volta para música padrão
            if (savedMapMusic != null)
            {
                Debug.Log($"AudioManager: Voltando à música do mapa '{savedMapMusic.name}'");
                PlayMusic(savedMapMusic, true);
            }
            else if (defaultMapMusic != null)
            {
                Debug.Log($"AudioManager: Usando música padrão '{defaultMapMusic.name}'");
                PlayMusic(defaultMapMusic, true);
            }
            else
            {
                Debug.Log("AudioManager: Nenhuma música configurada - mantendo atual");
            }
        }
    }

    private void InitializeAudioSources()
    {
        if (musicSource == null)
        {
            GameObject musicGO = new GameObject("MusicSource");
            musicGO.transform.SetParent(transform);
            musicSource = musicGO.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            GameObject sfxGO = new GameObject("SFXSource");
            sfxGO.transform.SetParent(transform);
            sfxSource = sfxGO.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        ApplyVolumeSettings(); // NOVO: Aplica configurações carregadas
    }

    public void PlayMusic(AudioClip musicClip, bool useFade = true)
    {
        if (musicClip == null)
        {
            StopMusic(useFade);
            return;
        }

        // Se já estiver tocando a mesma música e não estiver em fade, não faz nada
        if (currentMusic == musicClip && musicSource.isPlaying && !isFading)
        {
            Debug.Log($"AudioManager: Música '{musicClip.name}' já está tocando - ignorando");
            return;
        }

        // Se uma corrotina de fade já estiver em execução, para ela
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        Debug.Log($"AudioManager: Mudando música para '{musicClip.name}'");

        if (useFade && musicSource.isPlaying)
        {
            fadeCoroutine = StartCoroutine(FadeToNewMusic(musicClip));
        }
        else
        {
            PlayMusicDirectly(musicClip);
        }
    }

    private void PlayMusicDirectly(AudioClip musicClip)
    {
        currentMusic = musicClip;
        musicSource.clip = musicClip;
        musicSource.volume = musicVolume;
        musicSource.Play();
        isFading = false;
    }

    private IEnumerator FadeToNewMusic(AudioClip newMusic)
    {
        isFading = true;

        // Fade out
        float startVolume = musicSource.volume;
        for (float t = 0;
             t < musicFadeTime;
             t += Time.unscaledDeltaTime) // NOVO: unscaledDeltaTime para funcionar quando pausado
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / musicFadeTime);
            yield return null;
        }

        musicSource.volume = 0;

        // Troca de música
        PlayMusicDirectly(newMusic);

        // Fade in
        for (float t = 0;
             t < musicFadeTime;
             t += Time.unscaledDeltaTime) // NOVO: unscaledDeltaTime para funcionar quando pausado
        {
            musicSource.volume = Mathf.Lerp(0, musicVolume, t / musicFadeTime);
            yield return null;
        }

        musicSource.volume = musicVolume;

        isFading = false;
        fadeCoroutine = null;
    }


    /// <summary>
    /// NOVO: Carrega as configurações de áudio salvas
    /// </summary>
    private void LoadAudioSettings()
    {
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.7f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

        Debug.Log($"AudioManager: Configurações carregadas - Música: {musicVolume:F2}, SFX: {sfxVolume:F2}");
    }

    /// <summary>
    /// NOVO: Salva as configurações de áudio
    /// </summary>
    private void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save();

        Debug.Log($"AudioManager: Configurações salvas - Música: {musicVolume:F2}, SFX: {sfxVolume:F2}");
    }

    /// <summary>
    /// NOVO: Aplica as configurações de volume aos AudioSources
    /// </summary>
    private void ApplyVolumeSettings()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume;
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    public void StopMusic(bool useFade = true)
    {
        if (!musicSource.isPlaying) return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        if (useFade)
        {
            fadeCoroutine = StartCoroutine(FadeOutMusic());
        }
        else
        {
            musicSource.Stop();
            currentMusic = null;
        }
    }

    private IEnumerator FadeOutMusic()
    {
        isFading = true;

        float startVolume = musicSource.volume;
        for (float t = 0;
             t < musicFadeTime;
             t += Time.unscaledDeltaTime) // NOVO: unscaledDeltaTime para funcionar quando pausado
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / musicFadeTime);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = musicVolume;
        currentMusic = null;
        isFading = false;
        fadeCoroutine = null;
    }

    public void PlaySFX(AudioClip sfxClip, float pitch = 1f)
    {
        if (sfxClip == null) return;
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(sfxClip, sfxVolume);
        sfxSource.pitch = 1f; // Reseta o pitch
    }

    public void PlaySFX(AudioClip sfxClip, float volume, float pitch = 1f)
    {
        if (sfxClip == null) return;
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(sfxClip, volume);
        sfxSource.pitch = 1f;
    }

    // --- SISTEMA DE EVENTOS ---

    /// <summary>
    /// Define a música que deve tocar no próximo evento e salva a música atual do mapa
    /// </summary>
    public void SetPendingEventMusic(AudioClip eventMusic, AudioClip mapMusic)
    {
        pendingEventMusic = eventMusic;
        savedMapMusic = mapMusic;

        if (eventMusic != null)
        {
            Debug.Log($"AudioManager: Música '{eventMusic.name}' agendada para próximo evento");
        }
        else
        {
            Debug.Log("AudioManager: Nenhuma música específica agendada para evento");
        }

        if (mapMusic != null)
        {
            Debug.Log($"AudioManager: Música do mapa '{mapMusic.name}' salva para retorno");
        }
    }

    /// <summary>
    /// Volta à música do mapa salva (chamado quando sai de um evento)
    /// </summary>
    public void ReturnToMapMusic()
    {
        if (savedMapMusic != null)
        {
            Debug.Log($"AudioManager: Retornando à música do mapa '{savedMapMusic.name}'");
            PlayMusic(savedMapMusic, true);
        }
        else if (defaultMapMusic != null)
        {
            Debug.Log($"AudioManager: Voltando à música padrão '{defaultMapMusic.name}'");
            PlayMusic(defaultMapMusic, true);
        }
        else
        {
            Debug.Log("AudioManager: Nenhuma música de mapa salva - mantendo atual");
        }
    }

    // --- CONTROLES DE VOLUME (ATUALIZADOS) ---

    /// <summary>
    /// Define o volume da música e salva a configuração
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (!isFading && musicSource != null)
        {
            musicSource.volume = musicVolume;
        }

        SaveAudioSettings(); // NOVO: Salva automaticamente
    }

    /// <summary>
    /// Define o volume dos efeitos e salva a configuração
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }

        SaveAudioSettings(); // NOVO: Salva automaticamente
    }

    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;

    // --- INFORMAÇÕES ---

    public bool IsMusicPlaying() => musicSource.isPlaying;
    public AudioClip GetCurrentMusic() => currentMusic;
    public bool IsFading() => isFading;

    void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyVolumeSettings();
        }
    }

    /// <summary>
    /// NOVO: Método para recarregar configurações (útil para o menu de opções)
    /// </summary>
    public void ReloadSettings()
    {
        LoadAudioSettings();
        ApplyVolumeSettings();
    }
}