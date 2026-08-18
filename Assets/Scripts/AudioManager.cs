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
    public AudioClip heartCollectSound;
    public AudioClip mudSplatSound;

    [Header("Yeni Eklenen Özelliklerin Sesleri")]
    public AudioClip nauseaSound;             // Küf peyniri yendiğindeki bulantı sesi
    public AudioClip teleportInSound;          // Peynir odasına ışınlanma sesi
    public AudioClip teleportOutSound;         // Peynir odasından dönüş sesi
    public AudioClip biomeTransitionSound;     // Biyom geçiş sesi
    public AudioClip magnetSound;              // Mıknatıs alma sesi
    public AudioClip shieldSound;              // Kalkan alma sesi
    public AudioClip potionSound;              // Ters kontrol iksiri alma sesi

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private float musicVolume = 0.5f;
    private float sfxVolume = 0.5f;

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

        // Kaydedilmiş ses seviyelerini yükle (Varsayılan %50)
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;

        // Resources/Audio klasöründen otomatik yükleme fallback'i
        LoadClipsFromResources();
        
        // Müzik tanımlanmışsa oynat
        if (backgroundMusic != null)
        {
            PlayMusic(backgroundMusic);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();

        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();

        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;

    private void LoadClipsFromResources()
    {
        // Eğer inspector'dan atanmamışlarsa, Resources/Audio/ klasöründen yüklemeyi dene
        if (backgroundMusic == null) backgroundMusic = Resources.Load<AudioClip>("Audio/Music");
        if (cheeseCollectSound == null) cheeseCollectSound = Resources.Load<AudioClip>("Audio/Cheese");
        if (obstacleHitSound == null) obstacleHitSound = Resources.Load<AudioClip>("Audio/Hit");
        if (tomAttackSound == null) tomAttackSound = Resources.Load<AudioClip>("Audio/Attack");
        if (gameOverSound == null) gameOverSound = Resources.Load<AudioClip>("Audio/GameOver");
        if (buttonClickSound == null) buttonClickSound = Resources.Load<AudioClip>("Audio/Click");
        if (heartCollectSound == null) heartCollectSound = Resources.Load<AudioClip>("Audio/Heart");
        if (mudSplatSound == null) mudSplatSound = Resources.Load<AudioClip>("Audio/Mud");
        if (nauseaSound == null) nauseaSound = Resources.Load<AudioClip>("Audio/Nausea");
        if (teleportInSound == null) teleportInSound = Resources.Load<AudioClip>("Audio/TeleportIn");
        if (teleportOutSound == null) teleportOutSound = Resources.Load<AudioClip>("Audio/TeleportOut");
        if (biomeTransitionSound == null) biomeTransitionSound = Resources.Load<AudioClip>("Audio/Transition");
        if (magnetSound == null) magnetSound = Resources.Load<AudioClip>("Audio/Magnet");
        if (shieldSound == null) shieldSound = Resources.Load<AudioClip>("Audio/Shield");
        if (potionSound == null) potionSound = Resources.Load<AudioClip>("Audio/Potion");
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
    public void PlayHeartCollect() => PlaySFX(heartCollectSound);
    public void PlayMudSplat() => PlaySFX(mudSplatSound);
    public void PlayNausea() => PlaySFX(nauseaSound);
    public void PlayTeleportIn() => PlaySFX(teleportInSound);
    public void PlayTeleportOut() => PlaySFX(teleportOutSound);
    public void PlayBiomeTransition() => PlaySFX(biomeTransitionSound);
    public void PlayMagnet() => PlaySFX(magnetSound);
    public void PlayShield() => PlaySFX(shieldSound);
    public void PlayPotion() => PlaySFX(potionSound);
}
