using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    public Transform jerryTransform;         // Jerry'nin konumu
    public GameObject obstaclePrefab;        // Engel prefabı (Mutfak - Kapan)
    public GameObject cheesePrefab;          // Peynir prefabı

    [Header("Spawn Ayarları")]
    public float spawnAheadDistance = 15.0f; // Ekranın ne kadar önünde spawn edilsin
    public float spawnInterval = 1.8f;       // Başlangıç spawn sıklığı (saniye)

    [Header("Biyom Ayarları")]
    public float biomeLength = 80.0f;        // Her bir biyomun uzunluğu (Y ekseninde)
    public GameObject backyardObstaclePrefab; // Bahçe engeli prefabı (Çukur)
    public Sprite kitchenRoadSprite;         // Mutfak zemin sprite'ı (Ahşap)
    public Sprite backyardRoadSprite;         // Çimen zemin sprite'ı
    public SpriteRenderer[] roadRenderers;    // Zemin sprite renderers (dönüşüm için)

    [Header("Yan Panel Ayarları")]
    public SpriteRenderer leftSidePanel;      // Sol dekorasyon paneli
    public SpriteRenderer rightSidePanel;     // Sağ dekorasyon paneli
    public Sprite kitchenLeftSprite;         // Mutfak sol dekorasyon sprite'ı
    public Sprite kitchenRightSprite;        // Mutfak sağ dekorasyon sprite'ı
    public Sprite backyardLeftSprite;        // Bahçe sol dekorasyon sprite'ı
    public Sprite backyardRightSprite;       // Bahçe sağ dekorasyon sprite'ı

    [Header("Yan Panel Ölçekleri (Scale)")]
    public Vector3 kitchenLeftScale = Vector3.one;
    public Vector3 kitchenRightScale = Vector3.one;
    public Vector3 backyardLeftScale = Vector3.one;
    public Vector3 backyardRightScale = Vector3.one;

    private int activeBiomeIndex = 0;        // 0: Mutfak, 1: Bahçe
    private GameObject currentObstaclePrefab;
    
    // Şerit sistemimiz (Sol, Orta, Sağ)
    private float[] lanes = new float[] { -2.0f, 0.0f, 2.0f };
    private JerryController jerryController;
    private List<GameObject> activeObjects = new List<GameObject>(); // Temizlik için aktif nesnelerin listesi

    void Start()
    {
        currentObstaclePrefab = obstaclePrefab;

        // Oyun başında yan panelleri mutfak görsellerine, ölçeklerine ve yolları mutfak zeminine ayarla
        if (leftSidePanel != null)
        {
            if (kitchenLeftSprite != null) leftSidePanel.sprite = kitchenLeftSprite;
            leftSidePanel.transform.localScale = kitchenLeftScale;
        }
        if (rightSidePanel != null)
        {
            if (kitchenRightSprite != null) rightSidePanel.sprite = kitchenRightSprite;
            rightSidePanel.transform.localScale = kitchenRightScale;
        }
        if (kitchenRoadSprite != null && roadRenderers != null)
        {
            foreach (SpriteRenderer renderer in roadRenderers)
            {
                if (renderer != null) renderer.sprite = kitchenRoadSprite;
            }
        }

        if (jerryTransform != null)
        {
            jerryController = jerryTransform.GetComponent<JerryController>();
        }
        
        StartCoroutine(SpawnLoop());
    }

    private Coroutine transitionCoroutine;

    void Update()
    {
        // Jerry'nin gerisinde kalan nesneleri hafızayı yormaması için otomatik temizliyoruz
        if (jerryTransform == null) return;

        // Biyom döngü kontrolü (Mutfak -> Bahçe -> Mutfak -> ...)
        int currentBiomeIndex = Mathf.FloorToInt(jerryTransform.position.y / biomeLength) % 2;

        if (currentBiomeIndex != activeBiomeIndex)
        {
            activeBiomeIndex = currentBiomeIndex;

            // Eğer çalışan bir geçiş varsa önce onu durdur
            if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(TransitionBiomeRoutine(activeBiomeIndex));
        }

        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = activeObjects[i];
            if (obj == null)
            {
                activeObjects.RemoveAt(i);
                continue;
            }

            // Eğer nesne Jerry'nin 8 birim gerisinde kaldıysa sil
            if (obj.transform.position.y < jerryTransform.position.y - 8f)
            {
                activeObjects.RemoveAt(i);
                Destroy(obj);
            }
        }
    }

    IEnumerator SpawnLoop()
    {
        // Jerry hayatta olduğu sürece döngü çalışsın
        while (jerryController != null && !jerryController.isDead)
        {
            // Jerry hızlandıkça engellerin spawn aralığını düşür (Zorluk seviyesi)
            float currentSpawnInterval = Mathf.Max(0.7f, spawnInterval - (jerryController.forwardSpeed - 5.0f) * 0.15f);
            yield return new WaitForSeconds(currentSpawnInterval);

            // Şeritlerden rastgele birini seç
            int obstacleLane = Random.Range(0, lanes.Length);
            int cheeseLane = Random.Range(0, lanes.Length);

            // Aynı şeritte hem peynir hem engel olmasın diye düzenliyoruz
            while (cheeseLane == obstacleLane)
            {
                cheeseLane = Random.Range(0, lanes.Length);
            }

            float spawnY = jerryTransform.position.y + spawnAheadDistance;

            // 1. Engele Karar Ver (%70 ihtimalle engel spawn et)
            if (Random.value < 0.7f && currentObstaclePrefab != null)
            {
                Vector3 obstaclePos = new Vector3(lanes[obstacleLane], spawnY, 0);
                GameObject newObstacle = Instantiate(currentObstaclePrefab, obstaclePos, Quaternion.identity);
                activeObjects.Add(newObstacle);
            }

            // 2. Peynire Karar Ver (%80 ihtimalle peynir spawn et)
            if (Random.value < 0.8f && cheesePrefab != null)
            {
                Vector3 cheesePos = new Vector3(lanes[cheeseLane], spawnY, 0);
                GameObject newCheese = Instantiate(cheesePrefab, cheesePos, Quaternion.identity);
                activeObjects.Add(newCheese);
            }
        }
    }

    // Yumuşak Biyom Geçiş Animasyonu (Fade Out -> Swap -> Fade In)
    IEnumerator TransitionBiomeRoutine(int newBiomeIndex)
    {
        float duration = 0.4f; // Kararma ve aydınlanma süresi (toplam 0.8 saniye)
        float elapsed = 0f;

        // 1. Karartma (Fade Out) - Görselleri yavaşça şeffaf yap
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            SetSpritesAlpha(alpha);
            yield return null;
        }

        // 2. Değişim (Swap) - Görselleri ve Ölçekleri güncelle
        if (newBiomeIndex == 0) // Mutfak Biyomu
        {
            currentObstaclePrefab = obstaclePrefab;
            
            if (kitchenRoadSprite != null && roadRenderers != null)
            {
                foreach (SpriteRenderer renderer in roadRenderers)
                    if (renderer != null) renderer.sprite = kitchenRoadSprite;
            }

            if (leftSidePanel != null)
            {
                leftSidePanel.sprite = kitchenLeftSprite;
                leftSidePanel.transform.localScale = kitchenLeftScale;
            }
            if (rightSidePanel != null)
            {
                rightSidePanel.sprite = kitchenRightSprite;
                rightSidePanel.transform.localScale = kitchenRightScale;
            }
            Debug.Log("Biyom Değişti: Mutfak (Ölçekler Güncellendi)");
        }
        else // Arka Bahçe Biyomu
        {
            currentObstaclePrefab = backyardObstaclePrefab;
            
            if (backyardRoadSprite != null && roadRenderers != null)
            {
                foreach (SpriteRenderer renderer in roadRenderers)
                    if (renderer != null) renderer.sprite = backyardRoadSprite;
            }

            if (leftSidePanel != null)
            {
                leftSidePanel.sprite = backyardLeftSprite;
                leftSidePanel.transform.localScale = backyardLeftScale;
            }
            if (rightSidePanel != null)
            {
                rightSidePanel.sprite = backyardRightSprite;
                rightSidePanel.transform.localScale = backyardRightScale;
            }
            Debug.Log("Biyom Değişti: Arka Bahçe (Ölçekler Güncellendi)");
        }

        // 3. Aydınlatma (Fade In) - Görselleri yavaşça görünür yap
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            SetSpritesAlpha(alpha);
            yield return null;
        }
        
        SetSpritesAlpha(1f); // Tam opaklığı garanti et
    }

    // Tüm yol ve yan panel renderers'ların şeffaflık ayarı
    void SetSpritesAlpha(float alpha)
    {
        if (roadRenderers != null)
        {
            foreach (SpriteRenderer renderer in roadRenderers)
            {
                if (renderer != null)
                {
                    Color col = renderer.color;
                    renderer.color = new Color(col.r, col.g, col.b, alpha);
                }
            }
        }

        if (leftSidePanel != null)
        {
            Color col = leftSidePanel.color;
            leftSidePanel.color = new Color(col.r, col.g, col.b, alpha);
        }
        if (rightSidePanel != null)
        {
            Color col = rightSidePanel.color;
            rightSidePanel.color = new Color(col.r, col.g, col.b, alpha);
        }
    }
}
