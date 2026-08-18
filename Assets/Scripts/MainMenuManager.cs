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
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        // Ekran döndürmeyi yatay konumlarla sınırla (Dikey konumları engelle)
        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        // Ses ayarlarını yükle ve Slider'lara ata
        if (musicSlider != null)
        {
            float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            musicSlider.value = savedMusicVolume;
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        if (sfxSlider != null)
        {
            float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
            sfxSlider.value = savedSFXVolume;
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        // Oyun başında sadece ana paneli göster, diğerlerini gizle
        if (mainPanel != null) mainPanel.SetActive(true);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Daha önce kaydedilmiş oyuncu adı varsa kutucuğa yaz (varsayılan "Jerry" adını temizle)
        if (nameInputField != null)
        {
            string savedName = PlayerPrefs.GetString("PlayerName", "");
            if (savedName.Equals("Jerry", System.StringComparison.OrdinalIgnoreCase))
            {
                savedName = "";
                PlayerPrefs.DeleteKey("PlayerName");
            }
            nameInputField.text = savedName;
            nameInputField.onValueChanged.AddListener(OnNameInputChanged);
        }

        // Skor tablosunu güncelle
        UpdateLeaderboardUI();
    }

    // Oyunu Başlat
    public void StartGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

        if (nameInputField != null)
        {
            string playerName = nameInputField.text.Trim();
            
            // Eğer isim boş bırakıldıysa oyunu başlatma ve kutucuğu kırmızı yap
            if (string.IsNullOrEmpty(playerName))
            {
                nameInputField.image.color = new Color(1f, 0.6f, 0.6f, 1f); // Açık kırmızı hata rengi
                return; // Sahneyi yükleme, metodu sonlandır
            }

            // Daha önce kaydedilmiş olan ismi al
            string savedName = PlayerPrefs.GetString("PlayerName", "");

            // Eğer daha önce kaydedilmiş bir isim varsa ve yeni isim eskisinden farklıysa,
            // skor tablosundaki eski ismi yeni isimle güncelle
            if (!string.IsNullOrEmpty(savedName) && !savedName.Equals(playerName, System.StringComparison.OrdinalIgnoreCase))
            {
                ScoreManager.RenamePlayer(savedName, playerName);
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
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
        if (mainPanel != null) mainPanel.SetActive(false); // Ana menüyü gizle
        UpdateLeaderboardUI();
    }

    // Skor Tablosunu Kapat
    public void CloseLeaderboard()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true); // Ana menüyü tekrar göster
    }

    // Ayarları Aç
    public void OpenSettings()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (mainPanel != null) mainPanel.SetActive(false); // Ana menüyü gizle
    }

    // Ayarları Kapat
    public void CloseSettings()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true); // Ana menüyü tekrar göster
    }

    // Skorları Sıfırla (Ayarlar içinden çağrılacak)
    public void ResetAllData()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

        ScoreManager.ClearScores();
        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.DeleteKey("MusicVolume"); // Müzik ses ayarını temizle
        PlayerPrefs.DeleteKey("SFXVolume");   // SFX ses ayarını temizle

        if (nameInputField != null) nameInputField.text = "";
        
        if (musicSlider != null) musicSlider.value = 0.5f;
        if (sfxSlider != null) sfxSlider.value = 0.5f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(0.5f);
            AudioManager.Instance.SetSFXVolume(0.5f);
        }

        UpdateLeaderboardUI();
        Debug.Log("Tüm oyun verileri ve skorlar sıfırlandı!");
    }

    // Müzik seviyesi değiştiğinde çağrılır (Slider üzerinden tetiklenir)
    public void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }

    // SFX seviyesi değiştiğinde çağrılır (Slider üzerinden tetiklenir)
    public void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }

    // İsim kutucuğundaki metin her değiştiğinde çağrılır
    public void OnNameInputChanged(string text)
    {
        if (nameInputField != null)
        {
            // Eğer isim boş değilse hata rengini (kırmızıyı) temizle, beyaza döndür
            if (!string.IsNullOrEmpty(text.Trim()))
            {
                nameInputField.image.color = Color.white;
            }
        }
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
