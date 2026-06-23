using UnityEngine;
using UnityEngine.UI;

public class JerryController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float forwardSpeed = 5.0f;     // Otomatik yukarı koşma hızı
    public float sidewaySpeed = 7.0f;     // Sağa/sola hareket hızı
    public float minX = -3.0f;            // Ekranın en sol sınırı
    public float maxX = 3.0f;             // Ekranın en sağ sınırı

    [Header("Oyun Durumu")]
    public int maxLives = 3;
    public int currentLives;
    public int cheeseCount = 0;
    public bool isDead = false;

    [Header("Arayüz (UI) Elemanları")]
    public GameObject gameOverPanel;      // Kaybettiğimizde açılacak panel
    public Text livesText;                // Canı gösterecek UI yazısı
    public Text cheeseText;               // Peynir miktarını gösterecek UI yazısı

    private float currentForwardSpeed;
    private float slowDuration = 1.5f;     // Engele çarpınca ne kadar süre yavaşlasın?
    private float slowTimer = 0f;
    private bool isSlowed = false;

    void Start()
    {
        currentLives = maxLives;
        currentForwardSpeed = forwardSpeed;
        UpdateUI();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false); // Oyun başında paneli gizle
        }
    }

    void Update()
    {
        if (isDead) return;

        // Yavaşlama süresi kontrolü
        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0)
            {
                currentForwardSpeed = forwardSpeed;
                isSlowed = false;
            }
        }

        // 1. Düz İlerleme (Yukarı Doğru Otomatik Hareket)
        transform.Translate(Vector3.up * currentForwardSpeed * Time.deltaTime);

        // 2. Sağa-Sola Kaçış (Klavye veya Dokunmatik Ekran)
        float moveX = Input.GetAxisRaw("Horizontal");

        // Eğer ekranda dokunma algılanırsa (Telefon/Tablet için)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            // Dokunma devam ediyorsa veya hareket ediyorsa
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
            {
                // Ekranın sol yarısına dokunulduysa sola git (-1)
                if (touch.position.x < Screen.width / 2.0f)
                {
                    moveX = -1.0f;
                }
                // Ekranın sağ yarısına dokunulduysa sağa git (1)
                else
                {
                    moveX = 1.0f;
                }
            }
        }

        float newX = transform.position.x + moveX * sidewaySpeed * Time.deltaTime;

        // Karakterin ekran dışına çıkmasını engelle (Sınırlandır)
        newX = Mathf.Clamp(newX, minX, maxX);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    // Can Kaybetme Fonksiyonu
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentLives -= damage;
        currentLives = Mathf.Clamp(currentLives, 0, maxLives);
        UpdateUI();

        // Engele çarpınca geçici olarak yavaşla (Tom yaklaşabilsin diye)
        TriggerSlowdown();

        if (currentLives <= 0)
        {
            Die();
        }
    }

    // Peynir Toplama Fonksiyonu
    public void CollectCheese(int amount)
    {
        if (isDead) return;

        cheeseCount += amount;
        UpdateUI();
    }

    // Engele çarpınca yavaşlama tetikleyici
    void TriggerSlowdown()
    {
        isSlowed = true;
        slowTimer = slowDuration;
        currentForwardSpeed = forwardSpeed * 0.4f; // Hızı %60 düşür (Tom yetişebilsin diye)
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
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // "Oyun Bitti" panelini aktif yap
        }
        else
        {
            Debug.Log("Oyun Bitti! Canınız tükendi.");
        }
    }
}
