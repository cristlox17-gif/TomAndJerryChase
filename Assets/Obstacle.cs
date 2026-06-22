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
            
            // Engele çarptığımız için engeli sahneden yok et
            Destroy(gameObject);
        }
    }
}
