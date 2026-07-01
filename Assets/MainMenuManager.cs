using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneller")]
    public GameObject mainPanel;
    public GameObject leaderboardPanel;
    public GameObject settingsPanel;

    [Header("UI Elemanları")]
    public InputField nameInputField;
    public Text leaderboardText;

    void Start()
    {
        // Oyun başında sadece ana paneli göster, diğerlerini gizle
        if (mainPanel != null) mainPanel.SetActive(true);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Daha önce kaydedilmiş oyuncu adı varsa kutucuğa yaz
        if (nameInputField != null)
        {
            string savedName = PlayerPrefs.GetString("PlayerName", "");
            nameInputField.text = savedName;
        }

        // Skor tablosunu güncelle
        UpdateLeaderboardUI();
    }

    // Oyunu Başlat
    public void StartGame()
    {
        if (nameInputField != null)
        {
            string playerName = nameInputField.text.Trim();
            
            // Eğer isim boş bırakıldıysa varsayılan bir isim ata
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "Jerry";
            }
            
            // Oyuncu ismini kaydet
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();
        }

        // Oyun sahnesine geç (Sahnenin adının "SampleScene" olduğundan emin ol veya Build Settings'e ekle)
        SceneManager.LoadScene("SampleScene");
    }

    // Skor Tablosunu Aç
    public void OpenLeaderboard()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
        UpdateLeaderboardUI();
    }

    // Skor Tablosunu Kapat
    public void CloseLeaderboard()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }

    // Ayarları Aç
    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    // Ayarları Kapat
    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // Skorları Sıfırla (Ayarlar içinden çağrılacak)
    public void ResetAllData()
    {
        ScoreManager.ClearScores();
        PlayerPrefs.DeleteKey("PlayerName");
        if (nameInputField != null) nameInputField.text = "";
        UpdateLeaderboardUI();
        Debug.Log("Tüm oyun verileri ve skorlar sıfırlandı!");
    }

    // Skor Yazısını Güncelle
    void UpdateLeaderboardUI()
    {
        if (leaderboardText != null)
        {
            leaderboardText.text = ScoreManager.GetLeaderboardText();
        }
    }

    // Oyundan Çıkış
    public void QuitGame()
    {
        Debug.Log("Oyundan çıkılıyor...");
        Application.Quit();
    }
}
