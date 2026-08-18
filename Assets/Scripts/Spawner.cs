using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    public Transform jerryTransform;         // Jerry'nin konumu
    public GameObject obstaclePrefab;        // Engel prefabı (Mutfak - Kapan)
    public GameObject cheesePrefab;          // Peynir prefabı
    public GameObject heartPrefab;           // Kalp prefabı (Can veren nadir nesne)
    [Range(0f, 1f)]
    public float heartSpawnChance = 0.10f;   // Kalp çıkma olasılığı (Örn: 0.1 = %10)

    [Header("Güçlendirici Prefabları")]
    public GameObject magnetPrefab;
    public GameObject shieldPrefab;
    public GameObject potionPrefab;
    public GameObject moldyCheesePrefab;
    public GameObject teleporterPrefab;

    [Header("Güçlendirici Olasılıkları")]
    [Range(0f, 1f)] public float magnetSpawnChance = 0.05f;
    [Range(0f, 1f)] public float shieldSpawnChance = 0.04f;
    [Range(0f, 1f)] public float potionSpawnChance = 0.05f;
    [Range(0f, 1f)] public float moldyCheeseSpawnChance = 0.06f;
    [Range(0f, 0.1f)] public float teleporterSpawnChance = 0.005f; // Varsayılan %0.5

    [Header("Çamur Birikintisi Ayarları")]
    public GameObject mudObstaclePrefab;
    [Range(0f, 1f)] public float mudObstacleSpawnChance = 0.12f; // Varsayılan %12 şans

    [Header("Spawn Ayarları")]
    public float spawnAheadDistance = 15.0f; // Ekranın ne kadar önünde spawn edilsin
    public float spawnInterval = 1.8f;       // Başlangıç spawn sıklığı (saniye)

    [Header("Biyom Ayarları")]
    public float biomeLength = 80.0f;        // Her bir biyomun uzunluğu (Y ekseninde)
    public float transitionSafetyZone = 15.0f; // Sınıra bu kadar mesafe kala ve geçtikten sonra engel üretilmez (Çamur, çukur, kapan dahil)
    public GameObject backyardObstaclePrefab; // Bahçe engeli prefabı (Çukur)
    public Sprite kitchenRoadSprite;         // Mutfak zemin sprite'ı (Ahşap)
    public Sprite backyardRoadSprite;         // Çimen zemin sprite'ı
    public SpriteRenderer[] roadRenderers;    // Zemin sprite renderers (dönüşüm için)

    [Header("Peynir Odası (Bonus) Ayarları")]
    public float bonusRoomDuration = 8.0f;    // Peynir odasında kalınacak süre (böylece boyutunu/uzunluğunu süresini artırarak ayarlayabilirsiniz)
    public float bonusSpawnInterval = 0.4f;   // Peynir odasındaki peynir sıklığı (ne kadar hızlı akacağı)
    [Range(0f, 1f)]
    public float bonusMagnetSpawnChance = 0.02f; // Peynir odasındaki mıknatıs çıkma olasılığı (%2)
    public Sprite cheeseRoadSprite;           // Peynir zemin sprite'ı
    public Sprite cheeseLeftPanelSprite;      // Sol peynir havuzu sprite'ı
    public Sprite cheeseRightPanelSprite;     // Sağ peynir havuzu sprite'ı
    public SpriteRenderer[] bonusRoadRenderers; // Peynir odası zemin renderers (Editörden sürüklenebilir)

    [Header("Peynir Odası Ölçek ve Koordinat Ayarları")]
    public Vector3 cheeseRoadScale = Vector3.one;         // Peynir zemin ölçeği (Scale)
    public Vector3 cheeseLeftPanelScale = Vector3.one;    // Sol peynir havuzu ölçeği
    public Vector3 cheeseRightPanelScale = Vector3.one;   // Sağ peynir havuzu ölçeği
    public float cheesePanelXOffset = 3.8f;                // Sol/Sağ panellerin merkezden (50f) ne kadar uzaklıkta duracağı

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
    
    // Bonus Odası Desen Değişkenleri
    private bool wasBonusActiveLastFrame = false;
    private int bonusPatternStep = 0;
    private int bonusPatternType = 0;        // 0: Alt alta satırlar, 1: Zig-zag takip yolu
    private int zigZagDirection = 1;         // 1: Sağa, -1: Sola
    private int currentZigZagLane = 1;       // Başlangıç orta şerit (0, 1, 2)
    private float originalLeftPanelWorldX;
    private float originalRightPanelWorldX;
    private Vector3 originalRoadScale = Vector3.one;
    
    // Şerit sistemimiz (Sol, Orta, Sağ)
    private float[] lanes = new float[] { -2.0f, 0.0f, 2.0f };
    private JerryController jerryController;
    private List<GameObject> activeObjects = new List<GameObject>(); // Temizlik için aktif nesnelerin listesi

    void Start()
    {
        currentObstaclePrefab = obstaclePrefab;

        if (leftSidePanel != null) originalLeftPanelWorldX = leftSidePanel.transform.position.x;
        if (rightSidePanel != null) originalRightPanelWorldX = rightSidePanel.transform.position.x;
        if (roadRenderers != null && roadRenderers.Length > 0 && roadRenderers[0] != null)
        {
            originalRoadScale = roadRenderers[0].transform.localScale;
        }

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
                if (renderer != null && renderer.transform.position.x < 40f) renderer.sprite = kitchenRoadSprite;
            }
        }

        if (jerryTransform != null)
        {
            jerryController = jerryTransform.GetComponent<JerryController>();
        }
        
        SetupBonusRoomSprites();
        StartCoroutine(SpawnLoop());
    }

    // Sahnedeki X > 40f olan tüm nesneleri bulup otomatik olarak peynir odası tarzına bürür
    void SetupBonusRoomSprites()
    {
        // 1. Doğrudan atanmış bonus zeminleri varsa onları hemen peynir zemin sprite'ına çevir
        if (cheeseRoadSprite != null && bonusRoadRenderers != null)
        {
            foreach (SpriteRenderer renderer in bonusRoadRenderers)
            {
                if (renderer != null) renderer.sprite = cheeseRoadSprite;
            }
        }

        // 2. Sahne taraması yaparak X > 40f olan tüm nesneleri (atama yapılmadıysa) otomatik bul
        SpriteRenderer[] allRenderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        foreach (SpriteRenderer sr in allRenderers)
        {
            if (sr == null) continue;
            
            // Eğer nesne X = 50f (bonus odası) yakınlarındaysa
            if (sr.transform.position.x > 40f)
            {
                string nameLower = sr.gameObject.name.ToLower();
                
                // Yol parçaları (InfiniteRoad scriptli nesneler veya ismi "road"/"zemin"/"background" içerenler)
                if (sr.GetComponent<InfiniteRoad>() != null || nameLower.Contains("road") || nameLower.Contains("zemin") || nameLower.Contains("background"))
                {
                    if (cheeseRoadSprite != null)
                    {
                        sr.sprite = cheeseRoadSprite;
                    }
                }
                // Sol panel / zemin dekorasyonu
                else if (nameLower.Contains("left"))
                {
                    if (cheeseLeftPanelSprite != null)
                    {
                        sr.sprite = cheeseLeftPanelSprite;
                    }
                }
                // Sağ panel / zemin dekorasyonu
                else if (nameLower.Contains("right"))
                {
                    if (cheeseRightPanelSprite != null)
                    {
                        sr.sprite = cheeseRightPanelSprite;
                    }
                }
            }
        }
    }

    private Coroutine transitionCoroutine;

    void Update()
    {
        // Jerry'nin gerisinde kalan nesneleri hafızayı yormaması için otomatik temizliyoruz
        if (jerryTransform == null) return;

        // Bonus odasına giriş algılama ve desen belirleme
        bool isBonusActive = jerryController != null && jerryController.isInBonusRoom;
        if (isBonusActive && !wasBonusActiveLastFrame)
        {
            bonusPatternType = Random.Range(0, 2); // 0: Alt alta satırlar, 1: Zig-zag
            currentZigZagLane = 1;
            zigZagDirection = (Random.value < 0.5f) ? 1 : -1;
            bonusPatternStep = 0;
            Debug.Log("Bonus Odası Deseni Belirlendi: " + (bonusPatternType == 0 ? "Alt Alta Satırlar" : "Zig-Zag Takip"));
        }
        wasBonusActiveLastFrame = isBonusActive;

        // Yan panellerin (dekorasyonların) görsellerini, konumlarını ve ölçeklerini bonus odasına göre güncelle (Mutfak/Bahçe kalıntılarını temizler, konumlandırır ve ölçeklendirir)
        if (isBonusActive)
        {
            // 1. Görselleri Peynir Yap
            if (leftSidePanel != null && cheeseLeftPanelSprite != null && leftSidePanel.sprite != cheeseLeftPanelSprite) leftSidePanel.sprite = cheeseLeftPanelSprite;
            if (rightSidePanel != null && cheeseRightPanelSprite != null && rightSidePanel.sprite != cheeseRightPanelSprite) rightSidePanel.sprite = cheeseRightPanelSprite;
            if (roadRenderers != null && cheeseRoadSprite != null)
            {
                foreach (SpriteRenderer road in roadRenderers)
                {
                    if (road != null && road.sprite != cheeseRoadSprite) road.sprite = cheeseRoadSprite;
                }
            }

            // 2. Konumları Sabitle ve Ölçeklendir (Özel koordinat/ölçek sistemi ile X = 50f civarına yerleştirir)
            if (leftSidePanel != null)
            {
                leftSidePanel.transform.position = new Vector3(50.0f - cheesePanelXOffset, leftSidePanel.transform.position.y, leftSidePanel.transform.position.z);
                leftSidePanel.transform.localScale = cheeseLeftPanelScale;
            }
            if (rightSidePanel != null)
            {
                rightSidePanel.transform.position = new Vector3(50.0f + cheesePanelXOffset, rightSidePanel.transform.position.y, rightSidePanel.transform.position.z);
                rightSidePanel.transform.localScale = cheeseRightPanelScale;
            }
            // Zeminleri otomatik olarak 50f koordinatına ışınla ve ölçeklendir
            if (roadRenderers != null)
            {
                foreach (SpriteRenderer road in roadRenderers)
                {
                    if (road != null)
                    {
                        road.transform.position = new Vector3(50.0f, road.transform.position.y, road.transform.position.z);
                        road.transform.localScale = cheeseRoadScale;
                    }
                }
            }
        }
        else
        {
            // Normal odaya dönüldüğünde aktif biyom görsellerini, konumlarını (X = 0) ve ölçeklerini geri yükle
            if (leftSidePanel != null)
            {
                leftSidePanel.transform.position = new Vector3(originalLeftPanelWorldX, leftSidePanel.transform.position.y, leftSidePanel.transform.position.z);
                leftSidePanel.transform.localScale = (activeBiomeIndex == 0) ? kitchenLeftScale : backyardLeftScale;
            }
            if (rightSidePanel != null)
            {
                rightSidePanel.transform.position = new Vector3(originalRightPanelWorldX, rightSidePanel.transform.position.y, rightSidePanel.transform.position.z);
                rightSidePanel.transform.localScale = (activeBiomeIndex == 0) ? kitchenRightScale : backyardRightScale;
            }
            if (roadRenderers != null)
            {
                foreach (SpriteRenderer road in roadRenderers)
                {
                    if (road != null)
                    {
                        road.transform.position = new Vector3(0.0f, road.transform.position.y, road.transform.position.z);
                        road.transform.localScale = originalRoadScale;
                    }
                }
            }

            if (activeBiomeIndex == 0)
            {
                if (leftSidePanel != null && kitchenLeftSprite != null && leftSidePanel.sprite != kitchenLeftSprite) leftSidePanel.sprite = kitchenLeftSprite;
                if (rightSidePanel != null && kitchenRightSprite != null && rightSidePanel.sprite != kitchenRightSprite) rightSidePanel.sprite = kitchenRightSprite;
                if (roadRenderers != null && kitchenRoadSprite != null)
                {
                    foreach (SpriteRenderer road in roadRenderers)
                    {
                        if (road != null && road.sprite != kitchenRoadSprite) road.sprite = kitchenRoadSprite;
                    }
                }
            }
            else
            {
                if (leftSidePanel != null && backyardLeftSprite != null && leftSidePanel.sprite != backyardLeftSprite) leftSidePanel.sprite = backyardLeftSprite;
                if (rightSidePanel != null && backyardRightSprite != null && rightSidePanel.sprite != backyardRightSprite) rightSidePanel.sprite = backyardRightSprite;
                if (roadRenderers != null && backyardRoadSprite != null)
                {
                    foreach (SpriteRenderer road in roadRenderers)
                    {
                        if (road != null && road.sprite != backyardRoadSprite) road.sprite = backyardRoadSprite;
                    }
                }
            }
        }

        // Biyom geçiş kontrolü (Peynir odasındayken biyom geçişini engeller ve etkilenmesini önler)
        if (!isBonusActive)
        {
            float spawnY = jerryTransform.position.y + spawnAheadDistance;
            int currentBiomeIndex = 0;
            if (spawnY >= 0f)
            {
                currentBiomeIndex = Mathf.FloorToInt(spawnY / biomeLength) % 2;
            }

            if (currentBiomeIndex != activeBiomeIndex)
            {
                activeBiomeIndex = currentBiomeIndex;

                // Eğer çalışan bir geçiş varsa önce onu durdur
                if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
                transitionCoroutine = StartCoroutine(TransitionBiomeRoutine(activeBiomeIndex));
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
            // Eğer peynir (bonus) odasındaysak çok sık (0.4 saniyede bir) yoğun peynir spawn etsin!
            float currentSpawnInterval = Mathf.Max(0.7f, spawnInterval - (jerryController.forwardSpeed - 5.0f) * 0.15f);
            if (jerryController != null && jerryController.isInBonusRoom)
            {
                currentSpawnInterval = bonusSpawnInterval;
            }
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
            int playerBiomeIndex = 0;
            if (jerryTransform.position.y >= 0f)
            {
                playerBiomeIndex = Mathf.FloorToInt(jerryTransform.position.y / biomeLength) % 2;
            }

            // Biyom geçiş sınırlarına yakın yerlerde (Çamur, çukur, kapan dahil) engel üretmiyoruz
            float relativeY = spawnY % biomeLength;
            bool isNearTransition = (relativeY < transitionSafetyZone || relativeY > (biomeLength - transitionSafetyZone));

            bool isBonusActive = jerryController != null && jerryController.isInBonusRoom;

            // Şerit X pozisyonlarını ayarla (Normalde lanes, bonus odasındayken +50)
            float laneOffset = isBonusActive ? 50.0f : 0.0f;

            // 1. Engele Karar Ver (Bonus odasında veya geçiş sınırında değilse, %70 ihtimalle)
            if (!isBonusActive && !isNearTransition && Random.value < 0.7f)
            {
                GameObject obstacleToInstantiate = (playerBiomeIndex == 0) ? obstaclePrefab : backyardObstaclePrefab;
                if (obstacleToInstantiate != null)
                {
                    Vector3 obstaclePos = new Vector3(lanes[obstacleLane] + laneOffset, spawnY, 0);
                    GameObject newObstacle = Instantiate(obstacleToInstantiate, obstaclePos, Quaternion.identity);
                    activeObjects.Add(newObstacle);

                    // Geliş yönünü gösteren uyarı göstergesini oluştur
                    GameObject warningObj = new GameObject("ObstacleWarning_" + obstacleLane);
                    ObstacleWarning warning = warningObj.AddComponent<ObstacleWarning>();
                    warning.Setup(newObstacle.transform);
                }
            }

            // 2. Peynir ve Güçlendirici Kararları
            if (isBonusActive)
            {
                // --- BONUS ODASI (PEYNİR ODASI) SPAWN KURALLARI ---
                
                // A) Peynirler: %50 yoğunlukta desenler (Alt alta veya Subway Surfers tarzı zig-zag)
                if (cheesePrefab != null)
                {
                    if (bonusPatternType == 0)
                    {
                        // Desen 0: Bir satır dolu, bir satır boş (Alt alta satırlar)
                        if (bonusPatternStep % 2 == 0)
                        {
                            for (int i = 0; i < lanes.Length; i++)
                            {
                                Vector3 cheesePos = new Vector3(lanes[i] + laneOffset, spawnY, 0);
                                GameObject newCheese = Instantiate(cheesePrefab, cheesePos, Quaternion.identity);
                                activeObjects.Add(newCheese);
                            }
                        }
                    }
                    else
                    {
                        // Desen 1: Subway Surfers tarzı havada/yerde arka arkaya dizilmiş takip yolu
                        Vector3 cheesePos = new Vector3(lanes[currentZigZagLane] + laneOffset, spawnY, 0);
                        GameObject newCheese = Instantiate(cheesePrefab, cheesePos, Quaternion.identity);
                        activeObjects.Add(newCheese);

                        // Şeridi zig-zag çizmesi için güncelle (1 -> 2 -> 1 -> 0 -> 1 ...)
                        if (currentZigZagLane == 1)
                        {
                            currentZigZagLane += zigZagDirection;
                        }
                        else if (currentZigZagLane == 0)
                        {
                            currentZigZagLane = 1;
                            zigZagDirection = 1;
                        }
                        else if (currentZigZagLane == 2)
                        {
                            currentZigZagLane = 1;
                            zigZagDirection = -1;
                        }
                    }
                    bonusPatternStep++;
                }

                // B) Güçlendiriciler: Sadece ve sadece çok nadir mıknatıs çıkabilir. Tuzak, kalp veya başka özellik çıkmaz!
                if (Random.value < bonusMagnetSpawnChance && magnetPrefab != null)
                {
                    int magnetLane = Random.Range(0, lanes.Length);
                    Vector3 magnetPos = new Vector3(lanes[magnetLane] + laneOffset, spawnY, 0);
                    SpawnSpecialItem(magnetPrefab, PowerUp.PowerUpType.Magnet, 10f, magnetPos);
                }
            }
            else
            {
                // --- NORMAL ODA SPAWN KURALLARI ---

                // A) Normal Peynir Kararı (%80 şansla tek şeritte)
                if (cheesePrefab != null && Random.value < 0.8f)
                {
                    Vector3 cheesePos = new Vector3(lanes[cheeseLane] + laneOffset, spawnY, 0);
                    GameObject newCheese = Instantiate(cheesePrefab, cheesePos, Quaternion.identity);
                    activeObjects.Add(newCheese);
                }

                // B) Normal Güçlendirici ve Kalp Kararı (Boş kalan şeritte)
                int specialLane = -1;
                for (int i = 0; i < lanes.Length; i++)
                {
                    if (i != obstacleLane && i != cheeseLane)
                    {
                        specialLane = i;
                        break;
                    }
                }

                if (specialLane != -1)
                {
                    Vector3 specialPos = new Vector3(lanes[specialLane] + laneOffset, spawnY, 0);
                    
                    if (Random.value < teleporterSpawnChance)
                    {
                        SpawnSpecialItem(teleporterPrefab, PowerUp.PowerUpType.Teleporter, bonusRoomDuration, specialPos);
                    }
                    else if (Random.value < shieldSpawnChance)
                    {
                        SpawnSpecialItem(shieldPrefab, PowerUp.PowerUpType.Shield, 10f, specialPos);
                    }
                    else if (Random.value < magnetSpawnChance)
                    {
                        SpawnSpecialItem(magnetPrefab, PowerUp.PowerUpType.Magnet, 10f, specialPos);
                    }
                    else if (Random.value < potionSpawnChance)
                    {
                        SpawnSpecialItem(potionPrefab, PowerUp.PowerUpType.Potion, 8f, specialPos);
                    }
                    else if (Random.value < moldyCheeseSpawnChance)
                    {
                        SpawnSpecialItem(moldyCheesePrefab, PowerUp.PowerUpType.MoldyCheese, 5f, specialPos);
                    }
                    else if (playerBiomeIndex == 1 && Random.value < mudObstacleSpawnChance)
                    {
                        SpawnMudObstacle(specialPos);
                    }
                    else if (Random.value < heartSpawnChance)
                    {
                        GameObject heartObj;
                        if (heartPrefab != null)
                        {
                            heartObj = Instantiate(heartPrefab, specialPos, Quaternion.identity);
                        }
                        else
                        {
                            heartObj = new GameObject("Heart");
                            heartObj.transform.position = specialPos;
                            SpriteRenderer sr = heartObj.AddComponent<SpriteRenderer>();
                            sr.sprite = Heart.CreateHeartSprite();
                            sr.sortingOrder = 10;
                            BoxCollider2D bc = heartObj.AddComponent<BoxCollider2D>();
                            bc.isTrigger = true;
                            heartObj.AddComponent<Heart>();
                        }
                        activeObjects.Add(heartObj);
                    }
                }
            }
        }
    }

    // Güçlendirici nesnesini üretir (prefab yoksa otomatik renkli daire çizer)
    private void SpawnSpecialItem(GameObject prefab, PowerUp.PowerUpType type, float duration, Vector3 position)
    {
        GameObject itemObj;
        if (prefab != null)
        {
            itemObj = Instantiate(prefab, position, Quaternion.identity);
        }
        else
        {
            itemObj = new GameObject(type.ToString());
            itemObj.transform.position = position;
            
            SpriteRenderer sr = itemObj.AddComponent<SpriteRenderer>();
            sr.sprite = PowerUp.CreatePowerUpSprite(type);
            sr.sortingOrder = 10;
            
            CircleCollider2D cc = itemObj.AddComponent<CircleCollider2D>();
            cc.isTrigger = true;
            
            PowerUp pu = itemObj.AddComponent<PowerUp>();
            pu.type = type;
            pu.duration = duration;
        }
        activeObjects.Add(itemObj);
    }

    // Çamur birikintisini üretir (prefab yoksa otomatik kahverengi elips çizer)
    private void SpawnMudObstacle(Vector3 position)
    {
        GameObject mudObj;
        if (mudObstaclePrefab != null)
        {
            mudObj = Instantiate(mudObstaclePrefab, position, Quaternion.identity);
        }
        else
        {
            mudObj = new GameObject("MudPuddle");
            mudObj.transform.position = position;
            
            SpriteRenderer sr = mudObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateMudPuddleSprite();
            sr.sortingOrder = 9; // Jerry'nin altında görünsün
            
            BoxCollider2D bc = mudObj.AddComponent<BoxCollider2D>();
            bc.isTrigger = true;
            
            mudObj.AddComponent<MudObstacle>();
        }
        activeObjects.Add(mudObj);
    }

    // Dinamik olarak yatay elips şeklinde kahverengi çamur birikintisi dokusu oluşturur (Fallback için)
    public static Sprite CreateMudPuddleSprite()
    {
        int width = 256;
        int height = 256;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        
        Color brown = new Color(0.08f, 0.04f, 0.01f, 1f); // Neredeyse siyahımsı çok koyu çamur rengi
        Color trans = Color.clear;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = x - 128f;
                float dy = y - 128f;
                // a=115, b=60 (Devasa yatay elips çamur birikintisi)
                if ((dx * dx) / 13225f + (dy * dy) / 3600f <= 1f)
                {
                    texture.SetPixel(x, y, brown);
                }
                else
                {
                    texture.SetPixel(x, y, trans);
                }
            }
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    // Yumuşak Biyom Geçiş Animasyonu (Tam ekran kararma overlay'i ile pürüzsüz ve estetik geçiş)
    IEnumerator TransitionBiomeRoutine(int newBiomeIndex)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBiomeTransition();
        }

        float duration = 0.3f; // Kararma süresi (0.3 saniye kararma, 0.3 saniye aydınlanma)
        
        // 1. Ekran Karartma için Geçici UI Canvas ve Image Oluştur
        GameObject canvasObj = new GameObject("TransitionCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Her şeyin en önünde görünmesini garanti et
        
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        GameObject fadeObj = new GameObject("FadeImage");
        fadeObj.transform.SetParent(canvasObj.transform, false);
        UnityEngine.UI.Image fadeImage = fadeObj.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = new Color(0.08f, 0.05f, 0.03f, 0f); // Sıcak bir koyu çikolata kahvesi geçiş rengi
        
        // Ekranı kaplamasını sağla
        UnityEngine.RectTransform rt = fadeImage.GetComponent<UnityEngine.RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        // 2. Karartma (Fade Out) - Overlay rengini yavaşça opak yap
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            fadeImage.color = new Color(0.08f, 0.05f, 0.03f, alpha);
            yield return null;
        }
        fadeImage.color = new Color(0.08f, 0.05f, 0.03f, 1f);

        // 3. Değişim (Swap) - Görselleri ve Ölçekleri güncelle (Ekran kapalıyken arka planda değişir)
        if (newBiomeIndex == 0) // Mutfak Biyomu
        {
            currentObstaclePrefab = obstaclePrefab;

            if (kitchenRoadSprite != null && roadRenderers != null)
            {
                foreach (SpriteRenderer renderer in roadRenderers)
                    if (renderer != null && renderer.transform.position.x < 40f) renderer.sprite = kitchenRoadSprite;
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
            Debug.Log("Biyom Değişti: Mutfak (Arka planda güncellendi)");
        }
        else // Arka Bahçe Biyomu
        {
            currentObstaclePrefab = backyardObstaclePrefab;

            if (backyardRoadSprite != null && roadRenderers != null)
            {
                foreach (SpriteRenderer renderer in roadRenderers)
                    if (renderer != null && renderer.transform.position.x < 40f) renderer.sprite = backyardRoadSprite;
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
            Debug.Log("Biyom Değişti: Arka Bahçe (Arka planda güncellendi)");
        }

        // 4. Aydınlatma (Fade In) - Overlay'i yavaşça şeffaf yap
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            fadeImage.color = new Color(0.08f, 0.05f, 0.03f, alpha);
            yield return null;
        }

        // 5. Temizlik - Canvas'ı yok et
        Destroy(canvasObj);
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
