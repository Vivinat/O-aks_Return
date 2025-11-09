using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioClip defaultMapMusic;

    [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.7f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private float musicFadeTime = 1f;

    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    private AudioClip currentMusic;
    private bool isFading = false;
    private Coroutine fadeCoroutine;

    private AudioClip pendingEventMusic;
    private AudioClip savedMapMusic;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAudioSettings();
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
        if (pendingEventMusic != null)
        {
            PlayMusic(pendingEventMusic, true);
            pendingEventMusic = null;
            return;
        }

        MapMusicSetup sceneMusicSetup = FindObjectOfType<MapMusicSetup>();

        if (sceneMusicSetup != null)
        {
            AudioClip sceneMusic = sceneMusicSetup.GetMapMusic();
            PlayMusic(sceneMusic, sceneMusicSetup.useFade);
        }
        else
        {
            if (savedMapMusic != null)
            {
                PlayMusic(savedMapMusic, true);
            }
            else if (defaultMapMusic != null)
            {
                PlayMusic(defaultMapMusic, true);
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

        ApplyVolumeSettings();
    }

    public void PlayMusic(AudioClip musicClip, bool useFade = true)
    {
        if (musicClip == null)
        {
            StopMusic(useFade);
            return;
        }

        if (currentMusic == musicClip && musicSource.isPlaying && !isFading)
        {
            return;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

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

        float startVolume = musicSource.volume;
        for (float t = 0; t < musicFadeTime; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / musicFadeTime);
            yield return null;
        }

        musicSource.volume = 0;

        PlayMusicDirectly(newMusic);

        for (float t = 0; t < musicFadeTime; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(0, musicVolume, t / musicFadeTime);
            yield return null;
        }

        musicSource.volume = musicVolume;

        isFading = false;
        fadeCoroutine = null;
    }

    private void LoadAudioSettings()
    {
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.7f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
    }

    private void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save();
    }

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
        for (float t = 0; t < musicFadeTime; t += Time.unscaledDeltaTime)
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
        sfxSource.pitch = 1f;
    }

    public void PlaySFX(AudioClip sfxClip, float volume, float pitch = 1f)
    {
        if (sfxClip == null) return;
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(sfxClip, volume);
        sfxSource.pitch = 1f;
    }

    // Define a música do próximo evento e salva a música atual do mapa
    public void SetPendingEventMusic(AudioClip eventMusic, AudioClip mapMusic)
    {
        pendingEventMusic = eventMusic;
        savedMapMusic = mapMusic;
    }

    // Volta à música do mapa salva
    public void ReturnToMapMusic()
    {
        if (savedMapMusic != null)
        {
            PlayMusic(savedMapMusic, true);
        }
        else if (defaultMapMusic != null)
        {
            PlayMusic(defaultMapMusic, true);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (!isFading && musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
        SaveAudioSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
        SaveAudioSettings();
    }

    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;

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

    public void ReloadSettings()
    {
        LoadAudioSettings();
        ApplyVolumeSettings();
    }
}