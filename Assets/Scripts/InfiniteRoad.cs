using UnityEngine;

public class InfiniteRoad : MonoBehaviour
{
    public Transform playerTransform; // Takip edilecek Jerry (Oyuncu) nesnesi
    public float roadHeight = 13.8f;    // Tek bir yol görselinin boyutu (Y ekseninde)
    public int totalRoads = 4;         // Sahnedeki toplam yol parçası sayısı (Varsayılan 4)

    void Start()
    {
        // Eski haline getirildi: Başlangıçta ekstra hiçbir işlem yapılmıyor.
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Jerry bu yol parçasını tamamen geçtiğinde ve kamera görüş alanının dışına çıktığında (12 birim tampon alan)
        // Yol parçasını en üste (Jerry'nin ilerisine) taşıyoruz.
        if (playerTransform.position.y > transform.position.y + roadHeight + 12.0f)
        {
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y + (roadHeight * totalRoads),
                transform.position.z
            );
        }
    }
}
