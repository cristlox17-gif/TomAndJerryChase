using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;            // Takip edilecek karakter (Jerry)
    public float smoothSpeed = 0.125f;  // Kameranın takip yumuşaklığı (0 ile 1 arası)
    public Vector3 offset = new Vector3(0, 0, -10); // Kameranın karakterle arasındaki mesafe (2D'de Z ekseni -10 olmalıdır)

    public bool lockX = true;           // Kameranın yatayda (sağa-sola) takip etmesini engeller (sabit tutar)

    [Header("Ekran Sarsıntısı (Screen Shake)")]
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0.15f;

    [Header("Mide Bulantısı Efekti (Minecraft Tarzı)")]
    [HideInInspector] public float nauseaTimer = 0f;
    private float baseOrthographicSize = 5f;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            baseOrthographicSize = cam.orthographicSize;
        }
    }

    public void TriggerShake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }

    // LateUpdate kameranın titremesini önlemek için fizik ve hareketlerden sonra çalışır
    void LateUpdate()
    {
        if (target == null) return;

        // Kameranın gitmesi gereken hedef pozisyon
        Vector3 desiredPosition = target.position + offset;
        
        // Eğer yatay takip kilitliyse, X konumunu sabit tutuyoruz (0)
        if (lockX)
        {
            desiredPosition.x = offset.x;
        }
        
        // Kamerayı mevcut konumundan hedef konuma yumuşakça kaydır
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // Kameranın pozisyonunu güncelle (Z eksenini koruyoruz)
        Vector3 finalPosition = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);

        // Eğer ekran sarsıntısı aktifse pozisyona hafif 2D kayma ekle (arka taraftaki siyahlık görünmesin diye küçük limitli)
        if (shakeDuration > 0)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            shakeOffset.z = 0f; // Z eksenini koru
            finalPosition += shakeOffset;
            shakeDuration -= Time.deltaTime;
        }

        // --- MİDE BULANTISI (HAFİF BULANTI KONUM SALLANTISI) ---
        if (nauseaTimer > 0f)
        {
            // Çok hafif yatay ve dikey salınım (0.06 ve 0.05 birim) zemin kaymasını önler ve siyahlığı açığa çıkarmaz
            Vector3 nauseaPosOffset = new Vector3(
                Mathf.Sin(Time.time * 5.0f) * 0.06f,
                Mathf.Cos(Time.time * 4.0f) * 0.05f,
                0f
            );
            finalPosition += nauseaPosOffset;
        }
        else
        {
            // Efekt bittiğinde açı ve zoom değerlerinin sıfır olduğundan emin ol
            transform.rotation = Quaternion.identity;
            if (cam != null && cam.orthographicSize != baseOrthographicSize)
            {
                cam.orthographicSize = baseOrthographicSize;
            }
        }

        transform.position = finalPosition;
    }
}
