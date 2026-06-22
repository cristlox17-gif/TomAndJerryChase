using UnityEngine;

public class Cheese : MonoBehaviour
{
    public int cheeseValue = 1; // Her peynirin kazandıracağı puan/para miktarı

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Çarpan nesnede JerryController var mı kontrol et
        JerryController jerry = other.GetComponent<JerryController>();
        
        if (jerry != null)
        {
            // Jerry'ye peynir ekle
            jerry.CollectCheese(cheeseValue);
            
            // Peynir toplandığı için peyniri sahneden yok et
            Destroy(gameObject);
        }
    }
}
