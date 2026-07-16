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

    private int activeBiomeIndex = 0;        // 0: Mutfak, 1: Bahçe
    private GameObject currentObstaclePrefab;
    
    // Şerit sistemimiz (Sol, Orta, Sağ)
    private float[] lanes = new float[] { -2.0f, 0.0f, 2.0f };
    private JerryController jerryController;
    private List<GameObject> activeObjects = new List<GameObject>(); // Temizlik için aktif nesnelerin listesi

    void Start()
    {
        currentObstaclePrefab = obstaclePrefab;

        if (jerryTransform != null)
        {
            jerryController = jerryTransform.GetComponent<JerryController>();
        }
        
        StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        // Jerry'nin gerisinde kalan nesneleri hafızayı yormaması için otomatik temizliyoruz
        if (jerryTransform == null) return;

        // Biyom döngü kontrolü (Mutfak -> Bahçe -> Mutfak -> ...)
        int currentBiomeIndex = Mathf.FloorToInt(jerryTransform.position.y / biomeLength) % 2;

        if (currentBiomeIndex != activeBiomeIndex)
        {
            activeBiomeIndex = currentBiomeIndex;

            if (activeBiomeIndex == 0) // Mutfak Biyomuna Dönüş
            {
                currentObstaclePrefab = obstaclePrefab;
                if (kitchenRoadSprite != null && roadRenderers != null)
                {
                    foreach (SpriteRenderer renderer in roadRenderers)
                    {
                        if (renderer != null) renderer.sprite = kitchenRoadSprite;
                    }
                }
                
                // Yan panelleri mutfak dekorasyonuna çevir
                if (leftSidePanel != null && kitchenLeftSprite != null) leftSidePanel.sprite = kitchenLeftSprite;
                if (rightSidePanel != null && kitchenRightSprite != null) rightSidePanel.sprite = kitchenRightSprite;

                Debug.Log("Mutfak Biyomuna Geri Dönüldü! Kapanlar tekrar aktif.");
            }
            else // Arka Bahçe Biyomuna Geçiş
            {
                currentObstaclePrefab = backyardObstaclePrefab;
                if (backyardRoadSprite != null && roadRenderers != null)
                {
                    foreach (SpriteRenderer renderer in roadRenderers)
                    {
                        if (renderer != null) renderer.sprite = backyardRoadSprite;
                    }
                }

                // Yan panelleri bahçe dekorasyonuna çevir
                if (leftSidePanel != null && backyardLeftSprite != null) leftSidePanel.sprite = backyardLeftSprite;
                if (rightSidePanel != null && backyardRightSprite != null) rightSidePanel.sprite = backyardRightSprite;

                Debug.Log("Arka Bahçe Biyomuna Geçildi! Çukurlar aktif.");
            }
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
}
