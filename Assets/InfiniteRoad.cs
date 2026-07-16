using UnityEngine;

public class InfiniteRoad : MonoBehaviour
{
    public Transform playerTransform; // Takip edilecek Jerry (Oyuncu) nesnesi
    public float roadHeight = 12.0f;    // Tek bir yol görselinin boyutu (Y ekseninde)
    public int totalRoads = 3;         // Döngüdeki toplam yol parçası sayısı

    void Update()
    {
        if (playerTransform == null) return;

        // Jerry bu yol parçasını tamamen geçtiğinde (ekranın altında kaldığında)
        // Yol parçasını en üste (Jerry'nin ilerisine) taşıyoruz.
        if (playerTransform.position.y > transform.position.y + roadHeight)
        {
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y + (roadHeight * totalRoads),
                transform.position.z
            );
        }
    }
}
