using UnityEngine;

public class ObstacleWarning : MonoBehaviour
{
    private Transform targetObstacle;
    private SpriteRenderer spriteRenderer;
    private float warningYOffset = 1.2f; // Ekranın üst kenarından ne kadar aşağıda dursun
    private float flashSpeed = 12f;      // Flaşörün yanıp sönme hızı
    private float pulseSpeed = 6f;       // Boyut büyüme/küçülme hızı
    private bool isFadingOut = false;
    private float fadeOutTimer = 0.3f;    // Yok olurken fade-out süresi
    private float currentAlpha = 1f;

    public void Setup(Transform target)
    {
        targetObstacle = target;
        
        // SpriteRenderer ekle
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateWarningSprite();
        spriteRenderer.sortingOrder = 100; // En üstte gözüksün
        
        // Başlangıç konumu
        UpdatePosition();
    }

    void Update()
    {
        if (targetObstacle == null)
        {
            Destroy(gameObject);
            return;
        }

        // Engel ekrana girdiyse (engelin Y pozisyonu uyarı Y pozisyonunu geçtiyse) yok etme sürecini başlat
        float warningY = GetWarningY();
        if (targetObstacle.position.y <= warningY)
        {
            isFadingOut = true;
        }

        if (isFadingOut)
        {
            fadeOutTimer -= Time.deltaTime;
            currentAlpha = Mathf.Max(0f, fadeOutTimer / 0.3f);
            spriteRenderer.color = new Color(1f, 1f, 1f, currentAlpha);
            
            // Yavaşça yukarı kayarak kaybolsun
            transform.Translate(Vector3.up * 2f * Time.deltaTime);

            if (fadeOutTimer <= 0)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            UpdatePosition();
            
            // Yanıp Sönme (Flaşör) Efekti
            float alpha = 0.4f + Mathf.PingPong(Time.time * flashSpeed, 0.6f);
            spriteRenderer.color = new Color(1f, 1f, 1f, alpha);

            // Pulsing (Büyüyüp Küçülme) Efekti
            float scale = 0.85f + Mathf.PingPong(Time.time * pulseSpeed, 0.3f);
            transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    private void UpdatePosition()
    {
        if (targetObstacle == null) return;

        float targetX = targetObstacle.position.x;
        float warningY = GetWarningY();

        transform.position = new Vector3(targetX, warningY, 0f);
    }

    private float GetWarningY()
    {
        if (Camera.main != null)
        {
            float camY = Camera.main.transform.position.y;
            float camHeight = Camera.main.orthographicSize;
            return camY + camHeight - warningYOffset;
        }
        
        // Kameraya erişilemezse varsayılan bir Y değeri
        return transform.position.y;
    }

    // Dinamik olarak kırmızı renkli ve siyah çerçeveli ünlem işareti sprite'ı üretir
    private Sprite CreateWarningSprite()
    {
        int width = 32;
        int height = 64;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        
        Color red = new Color(1f, 0.1f, 0.1f, 1f);
        Color trans = Color.clear;
        Color black = Color.black;
        
        // 1. Şablonu Temizle
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, trans);
            }
        }
        
        // 2. Kırmızı Ünlem İşaretini Çiz (Y=0 bottom, Y=63 top)
        // Üst Kısım (Bar): y=20'den y=55'e. y=55 (geniş) -> y=20 (dar)
        for (int y = 20; y <= 55; y++)
        {
            int inset = (55 - y) / 10;
            for (int x = 12 + inset; x <= 19 - inset; x++)
            {
                texture.SetPixel(x, y, red);
            }
        }
        
        // Alt Nokta (Dot): y=5'ten y=12'ye
        for (int y = 5; y <= 12; y++)
        {
            for (int x = 12; x <= 19; x++)
            {
                texture.SetPixel(x, y, red);
            }
        }
        
        // 3. Kenarlık (Outline) Ekle: Çevresindeki boş pikselleri siyah yap
        Color[] pixels = texture.GetPixels();
        Color[] outlinedPixels = new Color[pixels.Length];
        System.Array.Copy(pixels, outlinedPixels, pixels.Length);

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                int idx = y * width + x;
                if (pixels[idx].a == 0) // Eğer piksel şeffaf ise
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
        
        // Sprite oluştur (pivot noktasını merkeze yerleştiriyoruz)
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100.0f);
    }
}
