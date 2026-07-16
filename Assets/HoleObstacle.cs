using UnityEngine;

public class HoleObstacle : MonoBehaviour
{
    public int damageAmount = 1; // Çukura düşüldüğünde gidecek can miktarı

    private void OnTriggerEnter2D(Collider2D other)
    {
        JerryController jerry = other.GetComponent<JerryController>();
        
        if (jerry != null)
        {
            // Jerry'ye hasar ver ve yavaşlat
            jerry.TakeDamage(damageAmount);
            
            // Görsel hisiyat için Jerry çukura girince küçük bir log yazdırıyoruz
            Debug.Log("Jerry çukura düştü! Dikkat et!");
            
            // Çukuru yok etmiyoruz (çukurlar sabit kalır, içinden geçilir)
            // Ama çift hasar almamak için collider'ı kapatıyoruz
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
            }
        }
    }
}
