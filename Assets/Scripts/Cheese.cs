using UnityEngine;

public class Cheese : MonoBehaviour
{
    public int cheeseValue = 1; // Her peynirin kazandıracağı puan/para miktarı

    private void Update()
    {
        // Eğer Jerry'nin mıknatısı aktifse ve peynir aynı dikey hizaya yaklaştıysa peyniri çek
        if (JerryController.Instance != null && JerryController.Instance.IsMagnetActive())
        {
            float yDist = transform.position.y - JerryController.Instance.transform.position.y;
            // Peynir Jerry'nin hizasına geldiyse veya arkasındaysa çek (2.5 birim önünden itibaren çekmeye başlar)
            if (yDist < 2.5f && yDist > -2.0f)
            {
                // Jerry'ye doğru hızla çek
                transform.position = Vector3.MoveTowards(transform.position, JerryController.Instance.transform.position, 15f * Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Çarpan nesnede JerryController var mı kontrol et
        JerryController jerry = other.GetComponent<JerryController>();
        
        if (jerry != null)
        {
            // Jerry'ye peynir ekle
            jerry.CollectCheese(cheeseValue);
            
            // Ses Efekti Çal
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCheeseCollect();
            }
            
            // Peynir toplandığı için peyniri sahneden yok et
            Destroy(gameObject);
        }
    }
}
