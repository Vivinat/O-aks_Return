using UnityEngine;

public class MapMusicSetup : MonoBehaviour
{
    public AudioClip mapMusic;
    
    public bool useFade = true;

    public AudioClip GetMapMusic()
    {
        return mapMusic;
    }
}