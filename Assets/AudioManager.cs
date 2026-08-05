using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Müzik (Yerleşik veya Resources klasöründen otomatik yüklenir)")]
    public AudioClip backgroundMusic;

    [Header("Ses Efektleri")]
    public AudioClip cheeseCollectSound;
    public AudioClip obstacleHitSound;
    public AudioClip tomAttackSound;
    public AudioClip gameOverSound;
    public AudioClip buttonClickSound;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitializeOnLoad()
    {
        // Eğer sahnede halihazırda bir AudioManager yoksa dinamik olarak oluştur
#if UNITY_2023_1_OR_NEWER
        if (FindAnyObjectByType<AudioManager>() == null)
#else
        if (FindObjectOfType<AudioManager>() == null)
#endif
        {
            GameObject go = new GameObject("AudioManagerManager");
            go.AddComponent<AudioManager>();
        }
    }

    void Awake()
    {
        // Singleton Yapısı
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // AudioSource bileşenlerini ekle
        musicSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.playOnAwake = false;

        // Resources/Audio klasöründen otomatik yükleme fallback'i
        LoadClipsFromResources();
        
        // Müzik tanımlanmışsa oynat
        if (backgroundMusic != null)
        {
            PlayMusic(backgroundMusic);
        }
    }

    private void LoadClipsFromResources()
    {
        // Eğer inspector'dan atanmamışlarsa, Resources/Audio/ klasöründen yüklemeyi dene
        if (backgroundMusic == null) backgroundMusic = Resources.Load<AudioClip>("Audio/Music");
        if (cheeseCollectSound == null) cheeseCollectSound = Resources.Load<AudioClip>("Audio/Cheese");
        if (obstacleHitSound == null) obstacleHitSound = Resources.Load<AudioClip>("Audio/Hit");
        if (tomAttackSound == null) tomAttackSound = Resources.Load<AudioClip>("Audio/Attack");
        if (gameOverSound == null) gameOverSound = Resources.Load<AudioClip>("Audio/GameOver");
        if (buttonClickSound == null) buttonClickSound = Resources.Load<AudioClip>("Audio/Click");
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // Harici scriptlerin ses çalması için yardımcı fonksiyonlar
    public void PlayCheeseCollect() => PlaySFX(cheeseCollectSound);
    public void PlayObstacleHit() => PlaySFX(obstacleHitSound);
    public void PlayTomAttack() => PlaySFX(tomAttackSound);
    public void PlayGameOver() => PlaySFX(gameOverSound);
    public void PlayButtonClick() => PlaySFX(buttonClickSound);
}
