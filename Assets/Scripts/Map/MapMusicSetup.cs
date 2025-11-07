using UnityEngine;

public class MapMusicSetup : MonoBehaviour
{
    [Header("Scene Music Configuration")]
    [Tooltip("Música de fundo para esta cena")]
    public AudioClip mapMusic;
    
    [Tooltip("Se deve usar fade quando a música desta cena começar a tocar")]
    public bool useFade = true;

    public AudioClip GetMapMusic()
    {
        return mapMusic;
    }
}