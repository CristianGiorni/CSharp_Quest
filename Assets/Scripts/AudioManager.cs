using UnityEngine;

// Sonidos y musica del juego. Se agrega a un objeto vacio "AudioManager" en cada escena.
// Los demas scripts lo llaman solos (AudioManager.Success(), etc.). Si falta un clip, simplemente no suena.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Efectos")]
    public AudioClip success;
    public AudioClip reject;
    public AudioClip door;
    public AudioClip elevator;
    public AudioClip click;
    public AudioClip pickup;
    public AudioClip levelComplete;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    [Header("Musica (en loop)")]
    public AudioClip music;
    [Range(0f, 1f)] public float musicVolume = 0.25f;

    private AudioSource sfxSource;
    private AudioSource musicSource;

    void Awake()
    {
        Instance = this;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = musicVolume;

        if (music != null)
        {
            musicSource.clip = music;
            musicSource.Play();
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        // M silencia o activa todo el sonido
        if (Input.GetKeyDown(KeyCode.M))
        {
            AudioListener.volume = AudioListener.volume > 0f ? 0f : 1f;
        }
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip != null) sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public static void Success() { if (Instance != null) Instance.PlayClip(Instance.success); }
    public static void Reject() { if (Instance != null) Instance.PlayClip(Instance.reject); }
    public static void Door() { if (Instance != null) Instance.PlayClip(Instance.door); }
    public static void Elevator() { if (Instance != null) Instance.PlayClip(Instance.elevator); }
    public static void Click() { if (Instance != null) Instance.PlayClip(Instance.click); }
    public static void Pickup() { if (Instance != null) Instance.PlayClip(Instance.pickup); }
    public static void LevelComplete() { if (Instance != null) Instance.PlayClip(Instance.levelComplete); }
}