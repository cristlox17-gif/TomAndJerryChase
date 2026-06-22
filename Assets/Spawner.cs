using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    public Transform jerryTransform;         // Jerry'nin konumu
    public GameObject obstaclePrefab;        // Engel prefabı (Inspector'dan atayacağız)
    public GameObject cheesePrefab;          // Peynir prefabı (Inspector'dan atayacağız)

    [Header("Spawn Ayarları")]
    public float spawnAheadDistance = 15.0f; // Ekranın ne kadar önünde (yukarısında) spawn edilsin
    public float spawnInterval = 1.8f;       // Ne sıklıkla spawn edilsin (saniye)
    
    // Şerit sistemimiz (Sol, Orta, Sağ)
    private float[] lanes = new float[] { -2.0f, 0.0f, 2.0f };
    private JerryController jerryController;
    private List<GameObject> activeObjects = new List<GameObject>(); // Temizlik için aktif nesnelerin listesi

    void Start()
    {
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
            yield return new WaitForSeconds(spawnInterval);

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
            if (Random.value < 0.7f && obstaclePrefab != null)
            {
                Vector3 obstaclePos = new Vector3(lanes[obstacleLane], spawnY, 0);
                GameObject newObstacle = Instantiate(obstaclePrefab, obstaclePos, Quaternion.identity);
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
