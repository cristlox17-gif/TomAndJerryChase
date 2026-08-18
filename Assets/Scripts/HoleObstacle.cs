using UnityEngine;

public class HoleObstacle : MonoBehaviour
{
    public int damageAmount = 1; // Çukura düşüldüğünde gidecek can miktarı

    private void OnTriggerEnter2D(Collider2D other)
    {
        JerryController jerry = other.GetComponent<JerryController>();
        
        if (jerry != null)
        {
            // Jerry'ye hasar ver
            jerry.TakeDamage(damageAmount);

            // Çukura çarptığında etraf hafif kahverengimsi olsun (vignette efekti)
            jerry.ActivateDustyVignette(3.0f);

            // Klasik engel çarpma sesini oynat
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayObstacleHit();
            }
            
            Debug.Log("Jerry çukura düştü! Vignette aktif.");
            
            // Çukuru yok etmiyoruz ancak çift hasar almamak için collider'ı kapatıyoruz
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
            }
        }
    }
}
