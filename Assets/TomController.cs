using UnityEngine;

public class TomController : MonoBehaviour
{
    public Transform jerryTransform;     // Jerry'nin konumu (Hierarchy'den Jerry nesnesini sürükleyeceğiz)
    public float followDistance = 4.0f;    // Tom'un Jerry'nin ne kadar arkasından koşacağı (normal takip mesafesi)
    public float catchUpSpeed = 6.0f;     // Jerry yavaşladığında Tom'un ona yetişme hızı
    public float sideSmoothSpeed = 5.0f;   // Tom'un sağa sola yumuşak takip etme hızı
    
    [Header("Saldırı Ayarları")]
    public float attackRange = 1.0f;       // Tom'un Jerry'ye vurabileceği mesafe
    public float pushBackDistance = 3.5f;  // Vurduktan sonra Tom'un ne kadar geriye çekileceği
    public float attackCooldown = 2.0f;    // İki vuruş arasındaki süre (saniye)

    private float lastAttackTime = 0f;
    private JerryController jerryController;

    void Start()
    {
        if (jerryTransform != null)
        {
            jerryController = jerryTransform.GetComponent<JerryController>();
        }
    }

    void Update()
    {
        if (jerryTransform == null || jerryController == null || jerryController.isDead) return;

        // 1. Sağa-Sola Takip (X ekseni)
        // Tom, Jerry'nin X pozisyonunu yumuşakça (Lerp ile) takip eder
        float targetX = Mathf.Lerp(transform.position.x, jerryTransform.position.x, sideSmoothSpeed * Time.deltaTime);

        // 2. İleri Doğru Takip (Y ekseni)
        // Normalde Tom, Jerry'nin Y pozisyonundan "followDistance" kadar geride durur
        float targetY = jerryTransform.position.y - followDistance;

        // Eğer Jerry engele çarptıysa veya yavaşladıysa, Tom hızla ona yaklaşır
        // Tom'un Y pozisyonu, targetY ile Jerry'nin Y pozisyonu arasında yumuşakça ilerler
        float currentY = Mathf.MoveTowards(transform.position.y, jerryTransform.position.y - followDistance, catchUpSpeed * Time.deltaTime);

        // Pozisyonu güncelle
        transform.position = new Vector3(targetX, currentY, transform.position.z);

        // 3. Vurma / Saldırı Mekaniği
        float distanceToJerry = Vector2.Distance(transform.position, jerryTransform.position);
        
        if (distanceToJerry <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            AttackJerry();
        }
    }

    void AttackJerry()
    {
        lastAttackTime = Time.time;
        
        // Jerry'ye hasar ver (1 can düşür)
        jerryController.TakeDamage(1);

        // Görsel efekt ve hisiyat için Tom'u biraz geriye fırlatıyoruz (Jerry kaçabilsin diye)
        transform.position = new Vector3(transform.position.x, transform.position.y - pushBackDistance, transform.position.z);
        
        Debug.Log("Tom Jerry'yi yakaladı ve vurdu! Jerry kaçıyor.");
    }
}
