using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { Magnet, Shield, Potion, MoldyCheese, Teleporter }
    public PowerUpType type;
    public float duration = 10f; // Etki süresi

    private void OnTriggerEnter2D(Collider2D other)
    {
        JerryController jerry = other.GetComponent<JerryController>();
        if (jerry != null)
        {
            switch (type)
            {
                case PowerUpType.Magnet:
                    jerry.ActivateMagnet(duration);
                    break;
                case PowerUpType.Shield:
                    jerry.ActivateShield(duration);
                    break;
                case PowerUpType.Potion:
                    jerry.ActivateReverseControls(duration);
                    break;
                case PowerUpType.MoldyCheese:
                    jerry.ActivateNausea(duration);
                    break;
                case PowerUpType.Teleporter:
                    jerry.ActivateTeleport(duration);
                    break;
            }

            // Ses Çal
            if (AudioManager.Instance != null)
            {
                switch (type)
                {
                    case PowerUpType.Magnet:
                        AudioManager.Instance.PlayMagnet();
                        break;
                    case PowerUpType.Shield:
                        AudioManager.Instance.PlayShield();
                        break;
                    case PowerUpType.Potion:
                        AudioManager.Instance.PlayPotion();
                        break;
                    case PowerUpType.MoldyCheese:
                        AudioManager.Instance.PlayNausea();
                        break;
                    case PowerUpType.Teleporter:
                        AudioManager.Instance.PlayTeleportIn();
                        break;
                    default:
                        AudioManager.Instance.PlayCheeseCollect();
                        break;
                }
            }

            Destroy(gameObject);
        }
    }

    // Dinamik olarak renkli ve siyah çerçeveli güçlendirici sprite'ları üretir (Fallback için)
    public static Sprite CreatePowerUpSprite(PowerUpType pType)
    {
        Color color = Color.white;
        switch (pType)
        {
            case PowerUpType.Magnet:
                color = Color.yellow;
                break;
            case PowerUpType.Shield:
                color = new Color(0.2f, 0.6f, 1f, 1f); // Açık Mavi
                break;
            case PowerUpType.Potion:
                color = new Color(0.7f, 0f, 1f, 1f); // Mor
                break;
            case PowerUpType.MoldyCheese:
                color = new Color(0f, 0.8f, 0.2f, 1f); // Yeşil (Küf Peyniri)
                break;
            case PowerUpType.Teleporter:
                color = Color.magenta;
                break;
        }

        int width = 32;
        int height = 32;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        
        Color trans = Color.clear;
        Color black = Color.black;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, trans);
            }
        }
        
        // Daire Çiz
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = x - 16f;
                float dy = y - 16f;
                if (dx*dx + dy*dy <= 12f*12f)
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
        
        // Çerçeve (Outline) Ekle
        Color[] pixels = texture.GetPixels();
        Color[] outlinedPixels = new Color[pixels.Length];
        System.Array.Copy(pixels, outlinedPixels, pixels.Length);

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                int idx = y * width + x;
                if (pixels[idx].a == 0)
                {
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
