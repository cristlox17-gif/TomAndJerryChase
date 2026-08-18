using UnityEngine;

public class MudObstacle : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        JerryController jerry = other.GetComponent<JerryController>();
        if (jerry != null)
        {
            // Çamura çarptığımızda hafif yavaşlık uygula (%30 yavaşlama, 1.2 saniye)
            jerry.ApplyMudSlowdown(1.2f, 0.3f);
            
            // Çamur lekeleri (yarı körlük) efekti uygula
            jerry.ActivateMudSplats(5.0f);
            
            // Çamur ses efekti çal
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMudSplat();
            }

            // Çamur birikintisini toplayınca yok et
            Destroy(gameObject);
        }
    }
}
