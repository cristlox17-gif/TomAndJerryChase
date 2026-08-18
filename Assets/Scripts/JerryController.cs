using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class JerryController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float forwardSpeed = 5.0f;     // Otomatik yukarı koşma hızı
    public float sidewaySpeed = 7.0f;     // Sağa/sola hareket hızı
    public float minX = -3.0f;            // Ekranın en sol sınırı
    public float maxX = 3.0f;             // Ekranın en sağ sınırı

    [Header("Zorluk Ayarları (Zamanla Hızlanma)")]
    public float speedIncreaseRate = 0.04f; // Saniyede hızın ne kadar artacağı
    public float maxForwardSpeed = 12.0f;   // Ulaşılabilecek maksimum hız

    [Header("Oyun Durumu")]
    public int maxLives = 3;
    public int currentLives;
    public int cheeseCount = 0;
    public bool isDead = false;

    [Header("Arayüz (UI) Elemanları")]
    public GameObject gameOverPanel;      // Kaybettiğimizde açılacak panel
    public Text livesText;                // Canı gösterecek UI yazısı
    public Text cheeseText;               // Peynir miktarını gösterecek UI yazısı

    public static JerryController Instance;

    [Header("Güçlendirici Durumları")]
    public bool isInBonusRoom = false;
    private float bonusRoomTimer = 0f;
    private float originalX = 0f;

    private float magnetTimer = 0f;
    private float shieldTimer = 0f;
    private float reverseControlsTimer = 0f;
    private float nauseaTimer = 0f;
    private float vignetteTimer = 0f;
    private float mudSplatsTimer = 0f;

    private GameObject vignetteOverlayInstance;
    private GameObject mudSplatsOverlayInstance;

    private float currentForwardSpeed;
    private float mudSlowTimer = 0f;
    private float mudSlowMultiplier = 1f;

    public bool IsMagnetActive() => magnetTimer > 0f;
    public bool IsShieldActive() => shieldTimer > 0f;
    public bool IsControlsReversed() => reverseControlsTimer > 0f;

     void Start()
    {
        Instance = this;
        // Ekran döndürmeyi yatay konumlarla sınırla (Dikey konumları engelle)
        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        currentLives = maxLives;
        currentForwardSpeed = forwardSpeed;

        // Metinleri kalınlaştır
        MakeTextBold(livesText);
        MakeTextBold(cheeseText);

        UpdateUI();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false); // Oyun başında paneli gizle
        }
    }

    // UI metinlerini kalınlaştırarak okunabilirliğini artıran yardımcı fonksiyon
    private void MakeTextBold(Text textComponent)
    {
        if (textComponent == null) return;

        // Metni kalın (Bold) yap
        textComponent.fontStyle = FontStyle.Bold;
    }

    void Update()
    {
        if (isDead) return;

        // --- GÜÇLENDİRİCİ ZAMANLAYICILARI ---
        
        // 1. Mıknatıs Süresi
        if (magnetTimer > 0f) magnetTimer -= Time.deltaTime;

        // 2. Kalkan Süresi (Aktifken karakter rengini mavi yap)
        if (shieldTimer > 0f)
        {
            shieldTimer -= Time.deltaTime;
            GetComponent<SpriteRenderer>().color = new Color(0.6f, 0.8f, 1f, 0.8f);
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.white;
        }

        // 3. Ters Kontroller Süresi
        if (reverseControlsTimer > 0f) reverseControlsTimer -= Time.deltaTime;

        // 4. Mide Bulantısı (Minecraft Tarzı Dalgalanma/Wobble Efekti)
        if (nauseaTimer > 0f)
        {
            nauseaTimer -= Time.deltaTime;
            if (Camera.main != null)
            {
                CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
                if (camFollow != null)
                {
                    camFollow.nauseaTimer = nauseaTimer;
                }
            }
        }

        // 5. Tozlu Vignette Süresi (Kademeli yok olma)
        if (vignetteTimer > 0f)
        {
            vignetteTimer -= Time.deltaTime;
            if (vignetteTimer <= 0f)
            {
                if (vignetteOverlayInstance != null)
                {
                    Destroy(vignetteOverlayInstance);
                    vignetteOverlayInstance = null;
                }
            }
            else if (vignetteOverlayInstance != null)
            {
                Image img = vignetteOverlayInstance.GetComponentInChildren<Image>();
                if (img != null)
                {
                    float alpha = Mathf.Min(0.35f, vignetteTimer / 1.5f * 0.35f);
                    img.color = new Color(0.18f, 0.12f, 0.08f, alpha);
                }
            }
        }

        // 6. Çamur Lekeleri Süresi (Kademeli şeffaflaşarak yok olma)
        if (mudSplatsTimer > 0f)
        {
            mudSplatsTimer -= Time.deltaTime;
            if (mudSplatsTimer <= 0f)
            {
                if (mudSplatsOverlayInstance != null)
                {
                    Destroy(mudSplatsOverlayInstance);
                    mudSplatsOverlayInstance = null;
                }
            }
            else if (mudSplatsOverlayInstance != null)
            {
                // Lekeleri kademeli soluklaştır
                Image[] splatImages = mudSplatsOverlayInstance.GetComponentsInChildren<Image>();
                foreach (Image img in splatImages)
                {
                    if (img != null)
                    {
                        float alpha = Mathf.Min(0.9f, mudSplatsTimer / 1.5f * 0.9f);
                        Color c = img.color;
                        img.color = new Color(c.r, c.g, c.b, alpha);
                    }
                }
            }
        }

        // 6. Işınlanma (Bonus Odası) Süresi
        if (isInBonusRoom)
        {
            bonusRoomTimer -= Time.deltaTime;
            if (bonusRoomTimer <= 0f)
            {
                isInBonusRoom = false;
                transform.position = new Vector3(originalX, transform.position.y, transform.position.z);
                
                if (Camera.main != null)
                {
                    CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
                    if (camFollow != null) camFollow.lockX = true;
                }

                minX = -3.0f;
                maxX = 3.0f;

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayTeleportOut();
                }
            }
        }

        // --- HAREKET VE FİZİK ---

        // Zorlaşma mekaniği (Mide bulantısı veya çamur gibi durumlarda da hız artmaya devam eder)
        forwardSpeed += speedIncreaseRate * Time.deltaTime;
        forwardSpeed = Mathf.Min(forwardSpeed, maxForwardSpeed);

        // Çamur yavaşlaması kontrolü
        if (mudSlowTimer > 0f)
        {
            mudSlowTimer -= Time.deltaTime;
            if (mudSlowTimer <= 0f)
            {
                mudSlowMultiplier = 1f;
            }
        }

        currentForwardSpeed = forwardSpeed * mudSlowMultiplier;

        // 1. Düz İlerleme
        transform.Translate(Vector3.up * currentForwardSpeed * Time.deltaTime);

        // 2. Sağa-Sola Kaçış (Klavye veya Dokunmatik)
        float moveX = Input.GetAxisRaw("Horizontal");

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
            {
                if (touch.position.x < Screen.width / 2.0f)
                {
                    moveX = -1.0f;
                }
                else
                {
                    moveX = 1.0f;
                }
            }
        }

        // Yönleri Tersine Çevirme İksiri Aktif mi?
        if (IsControlsReversed())
        {
            moveX = -moveX;
        }

        float newX = transform.position.x + moveX * sidewaySpeed * Time.deltaTime;
        newX = Mathf.Clamp(newX, minX, maxX);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    // Can Kaybetme Fonksiyonu
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // Eğer kalkan aktifse hasar alma!
        if (IsShieldActive()) return;

        currentLives -= damage;
        currentLives = Mathf.Clamp(currentLives, 0, maxLives);
        UpdateUI();

        // Hasar alındığında (kapana veya çukura çarpıldığında) kamera hafifçe sarsılsın (siyah kenarlıklar görünmeyecek limitlerde)
        if (Camera.main != null)
        {
            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.TriggerShake(0.30f, 0.08f); // 0.30 saniye boyunca 0.08 şiddetinde güvenli sarsıntı
            }
        }

        if (currentLives <= 0)
        {
            Die();
        }
    }

    // Can Kazanma Fonksiyonu
    public void Heal(int amount)
    {
        if (isDead) return;

        currentLives += amount;
        
        // Eğer yeni can miktarı maksimum can sınırını aşarsa, maksimum can sınırını da genişlet
        if (currentLives > maxLives)
        {
            maxLives = currentLives;
        }

        UpdateUI();
    }

    // Peynir Toplama Fonksiyonu
    public void CollectCheese(int amount)
    {
        if (isDead) return;

        cheeseCount += amount;
        UpdateUI();
    }


    // Çamur yavaşlaması uygular (duration: saniye, slowPercent: örn. 0.3f = %30 yavaşlama)
    public void ApplyMudSlowdown(float duration, float slowPercent)
    {
        if (isDead) return;

        mudSlowTimer = duration;
        mudSlowMultiplier = 1f - Mathf.Clamp01(slowPercent);
    }

    public void ActivateMagnet(float duration) => magnetTimer = duration;

    public void ActivateShield(float duration) => shieldTimer = duration;

    public void ActivateReverseControls(float duration) => reverseControlsTimer = duration;

    public void ActivateNausea(float duration) => nauseaTimer = duration;

    public void ActivateDustyVignette(float duration)
    {
        vignetteTimer = duration;
        if (vignetteOverlayInstance == null)
        {
            GameObject canvasObj = new GameObject("VignetteCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 998; // Çamur lekelerinin arkasında kalmalı
            canvasObj.AddComponent<CanvasScaler>();
            
            GameObject panelObj = new GameObject("VignettePanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            Image img = panelObj.AddComponent<Image>();
            img.color = new Color(0.18f, 0.12f, 0.08f, 0.35f); // Düşük başlangıç opaklığı
            img.raycastTarget = false; // Tıklamayı engellemez
            
            RectTransform rt = panelObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            
            vignetteOverlayInstance = canvasObj;
        }
    }

    public void ActivateMudSplats(float duration)
    {
        mudSplatsTimer = duration;
        if (mudSplatsOverlayInstance == null)
        {
            GameObject canvasObj = new GameObject("MudSplatsCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Her şeyin önünde
            canvasObj.AddComponent<CanvasScaler>();
            
            mudSplatsOverlayInstance = canvasObj;

            // 4 adet daha belirgin çamur lekesi oluştur
            int splatCount = 4;
            for (int i = 0; i < splatCount; i++)
            {
                GameObject splatObj = new GameObject("MudSplat_" + i);
                splatObj.transform.SetParent(canvasObj.transform, false);
                
                Image img = splatObj.AddComponent<Image>();
                img.sprite = PowerUp.CreatePowerUpSprite(PowerUp.PowerUpType.MoldyCheese); // Daire dokusunu kullan
                img.color = new Color(0.05f, 0.02f, 0.0f, 0.99f); // Neredeyse zift gibi kapatan çok koyu çamur rengi
                img.raycastTarget = false; // Tıklamayı kesinlikle engellemez
                
                RectTransform rt = splatObj.GetComponent<RectTransform>();
                
                // Dev boyutlar (380 ile 600 piksel arası)
                float size = Random.Range(380f, 600f);
                rt.sizeDelta = new Vector2(size, size);
                
                // Ekran ortasına göre rastgele saç (X: -400..400, Y: -250..250)
                rt.anchoredPosition = new Vector2(Random.Range(-400f, 400f), Random.Range(-250f, 250f));
            }
        }
    }

    public void ActivateTeleport(float duration)
    {
        if (isInBonusRoom) return;

        isInBonusRoom = true;
        bonusRoomTimer = duration;
        originalX = transform.position.x;

        // Jerry'yi bonus odasının ortasına ışınla (X = 50f)
        transform.position = new Vector3(50f, transform.position.y, transform.position.z);

        // Kamera yatay kilidini aç ki bonus odasını takip edebilsin
        if (Camera.main != null)
        {
            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null) camFollow.lockX = false;
        }

        // Hareket sınırlarını bonus odasına göre güncelle (X = 50f civarı)
        minX = 47.0f;
        maxX = 53.0f;

        // Ekran parlaması (Sarsıntı ve ses)
        if (Camera.main != null)
        {
            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null) camFollow.TriggerShake(0.4f, 0.3f);
        }
        // Ses çalma işlemini PowerUp.cs tetikler (mükerrerliği önlemek için buradaki çalma kodunu kaldırıyoruz)
    }

    // Arayüzü Güncelleme
    void UpdateUI()
    {
        if (livesText != null) livesText.text = "CAN: " + currentLives;
        if (cheeseText != null) cheeseText.text = "PEYNİR: " + cheeseCount;
    }

    // Kaybetme / Ölme
    void Die()
    {
        isDead = true;
        currentForwardSpeed = 0f;

        // Çamur veya toz lekeleri varsa hemen temizle (buton tıklanabilirliğini garanti eder)
        if (vignetteOverlayInstance != null)
        {
            Destroy(vignetteOverlayInstance);
            vignetteOverlayInstance = null;
        }
        if (mudSplatsOverlayInstance != null)
        {
            Destroy(mudSplatsOverlayInstance);
            mudSplatsOverlayInstance = null;
        }
        vignetteTimer = 0f;
        mudSplatsTimer = 0f;

        // Ses Efekti Çal
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOver();
        }

        // İsmi yerel hafızadan çekip skoru kaydet
        string playerName = PlayerPrefs.GetString("PlayerName", "Oyuncu");
        ScoreManager.SaveScore(playerName, cheeseCount);
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // "Oyun Bitti" panelini aktif yap
        }
        else
        {
            Debug.Log("Oyun Bitti! Canınız tükendi. Skor: " + cheeseCount);
        }
    }

    // Ana menüye dönmek için buton fonksiyonu
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
