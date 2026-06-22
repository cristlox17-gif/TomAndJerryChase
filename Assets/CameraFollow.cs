using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;            // Takip edilecek karakter (Jerry)
    public float smoothSpeed = 0.125f;  // Kameranın takip yumuşaklığı (0 ile 1 arası)
    public Vector3 offset = new Vector3(0, 0, -10); // Kameranın karakterle arasındaki mesafe (2D'de Z ekseni -10 olmalıdır)

    // LateUpdate kameranın titremesini önlemek için fizik ve hareketlerden sonra çalışır
    void LateUpdate()
    {
        if (target == null) return;

        // Kameranın gitmesi gereken hedef pozisyon
        Vector3 desiredPosition = target.position + offset;
        
        // Kamerayı mevcut konumundan hedef konuma yumuşakça kaydır
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // Kameranın pozisyonunu güncelle
        transform.position = smoothedPosition;
    }
}
