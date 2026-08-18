using UnityEngine;

public class Heart : MonoBehaviour
{
    public int healAmount = 1; // Kazanılacak can miktarı

    private void OnTriggerEnter2D(Collider2D other)
    {
        JerryController jerry = other.GetComponent<JerryController>();
        
        if (jerry != null)
        {
            // Can ekle
            jerry.Heal(healAmount);
            
            // Ses efekti oynat
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayHeartCollect();
            }
            
            Destroy(gameObject);
        }
    }

    // Dinamik olarak kırmızı renkli ve siyah çerçeveli kalp sprite'ı üretir
    public static Sprite CreateHeartSprite()
    {
        int width = 32;
        int height = 32;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        
        Color red = new Color(1f, 0.1f, 0.2f, 1f); // Tatlı bir kalp kırmızısı
        Color trans = Color.clear;
        Color black = Color.black;
        
        // 1. Doku Temizliği
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, trans);
            }
        }
        
        // 2. Kalp Denklemiyle Kırmızı Şekil Çiz: (x^2 + y^2 - 1)^3 - x^2 * y^3 < 0
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Koordinatları merkez (16, 14) olacak şekilde normalize et ve ölçekle
                float nx = (x - 16f) / 10f;
                float ny = (y - 14f) / 10f;
                
                float term = nx * nx + ny * ny - 1f;
                if (term * term * term - nx * nx * ny * ny * ny < 0f)
                {
                    texture.SetPixel(x, y, red);
                }
            }
        }
        
        // 3. Çerçeve (Outline) Ekle
        Color[] pixels = texture.GetPixels();
        Color[] outlinedPixels = new Color[pixels.Length];
        System.Array.Copy(pixels, outlinedPixels, pixels.Length);

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                int idx = y * width + x;
                if (pixels[idx].a == 0) // Eğer piksel boş ise
                {
                    // Çevresindeki komşularda kırmızı renk var mı kontrol et
                    if (pixels[idx + 1].a > 0 || pixels[idx - 1].a > 0 ||
                        pixels[idx + width].a > 0 || pixels[idx - width].a > 0 ||
                        pixels[idx + width + 1].a > 0 || pixels[idx + width - 1].a > 0 ||
                        pixels[idx - width + 1].a > 0 || pixels[idx - width - 1].a > 0)
                    {
                        outlinedPixels[idx] = black;
                    }
                }
            }
        }
        
        texture.SetPixels(outlinedPixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100.0f);
    }
}
