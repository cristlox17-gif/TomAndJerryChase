using UnityEngine;

public class TomController : MonoBehaviour
{
    public Transform jerryTransform;     // Jerry'nin konumu (Hierarchy'den Jerry nesnesini sürükleyeceğiz)
    public float followDistance = 4.0f;    // Tom'un Jerry'nin ne kadar arkasından koşacağı (normal takip mesafesi)
    public float catchUpSpeed = 6.0f;     // Jerry yavaşladığında Tom'un ona yetişme hızı
    public float sideSmoothSpeed = 5.0f;   // Tom'un sağa sola yumuşak takip etme hızı
    
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

        // Işınlanma odasındayken kedi kovalamasın ve gizlensin
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (jerryController.isInBonusRoom)
        {
            if (sr != null) sr.enabled = false;
            return;
        }
        
        // Eğer kedi gizliyse ve tekrar görünür olduysa (odadan çıkış anı), konumunu anında sıfırla (kayarak geçiş yapmasın)
        if (sr != null && !sr.enabled)
        {
            sr.enabled = true;
            transform.position = new Vector3(jerryTransform.position.x, jerryTransform.position.y - followDistance, transform.position.z);
        }

        // 1. Sağa-Sola Takip (X ekseni)
        // Tom, Jerry'nin X pozisyonunu yumuşakça (Lerp ile) takip eder
        float targetX = Mathf.Lerp(transform.position.x, jerryTransform.position.x, sideSmoothSpeed * Time.deltaTime);

        // 2. İleri Doğru Takip (Y ekseni)
        // Normalde Tom, Jerry'nin Y pozisyonundan "followDistance" kadar geride durur
        float targetY = jerryTransform.position.y - followDistance;

        // Eğer Jerry engele çarptıysa veya yavaşladıysa, Tom hızla ona yaklaşır
        // Jerry hızlandıkça Tom'un da ona yetişme hızını (catchUpSpeed) ölçeklendiriyoruz
        float currentCatchUpSpeed = catchUpSpeed + (jerryController.forwardSpeed - 5.0f);
        float currentY = Mathf.MoveTowards(transform.position.y, jerryTransform.position.y - followDistance, currentCatchUpSpeed * Time.deltaTime);

        // Pozisyonu güncelle
        transform.position = new Vector3(targetX, currentY, transform.position.z);
    }
}
