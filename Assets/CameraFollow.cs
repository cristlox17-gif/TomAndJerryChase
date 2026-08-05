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

        // Eğer ekran sarsıntısı aktifse pozisyona rastgele kayma ekle
        if (shakeDuration > 0)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            shakeOffset.z = 0f; // 2D oyunda Z ekseni sarsılmamalı
            finalPosition += shakeOffset;
            shakeDuration -= Time.deltaTime;
        }

        transform.position = finalPosition;
    }
}
