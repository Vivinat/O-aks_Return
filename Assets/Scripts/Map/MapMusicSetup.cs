// Assets/Scripts/Map/MapMusicSetup.cs

using UnityEngine;

/// <summary>
/// Componente para configurar a música de fundo de uma cena.
/// O AudioManager irá procurar por este componente ao carregar uma nova cena.
/// </summary>
public class MapMusicSetup : MonoBehaviour
{
    [Header("Scene Music Configuration")]
    [Tooltip("Música de fundo para esta cena")]
    public AudioClip mapMusic;
    
    [Tooltip("Se deve usar fade quando a música desta cena começar a tocar")]
    public bool useFade = true;

    /// <summary>
    /// Retorna o clip de música configurado para este mapa.
    /// </summary>
    public AudioClip GetMapMusic()
    {
        return mapMusic;
    }
}