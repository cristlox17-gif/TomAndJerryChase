using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public int damageAmount = 1; // Engele çarpınca gidecek can miktarı

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Çarpan nesnede JerryController var mı kontrol et
        JerryController jerry = other.GetComponent<JerryController>();
        
        if (jerry != null)
        {
            // Jerry'ye hasar ver
            jerry.TakeDamage(damageAmount);
            
            // Ses Efekti Çal
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayObstacleHit();
            }

            // Ekran Sarsıntısını Tetikle
            if (Camera.main != null)
            {
                CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
                if (camFollow != null)
                {
                    camFollow.TriggerShake(0.18f, 0.22f); // Hafif/orta şiddette sarsıntı
                }
            }
            
            // Engele çarptığımız için engeli sahneden yok et
            Destroy(gameObject);
        }
    }
}
