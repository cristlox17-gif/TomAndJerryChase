using UnityEngine;
using UnityEngine.UI;

public class OrientationReminder : MonoBehaviour
{
    private GameObject canvasObj;
    private Canvas reminderCanvas;
    private bool wasPaused = false;

    private static OrientationReminder instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitializeOnLoad()
    {
        GameObject go = new GameObject("OrientationReminderManager");
        go.AddComponent<OrientationReminder>();
    }

    void Awake()
    {
        // Sahneler arası geçişte yok olmasın (Tüm oyunu korusun)
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        CreateReminderCanvas();
    }

    private void CreateReminderCanvas()
    {
        // 1. Canvas Oluştur
        canvasObj = new GameObject("OrientationReminderCanvas");
        canvasObj.transform.SetParent(this.transform);
        
        reminderCanvas = canvasObj.AddComponent<Canvas>();
        reminderCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        reminderCanvas.sortingOrder = 9999; // En üstte çizilmesini garantile

        // Canvas Scaler ekle (Çözünürlük bağımsız ölçekleme için)
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // Graphic Raycaster ekle (Arka plana tıklanmasını engellesin)
        canvasObj.AddComponent<GraphicRaycaster>();

        // 2. Arka Plan Paneli (Yarı saydam koyu zemin)
        GameObject panelObj = new GameObject("BackgroundPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image bgImage = panelObj.AddComponent<Image>();
        bgImage.color = new Color(0.08f, 0.08f, 0.1f, 0.95f); // Koyu lacivert/siyah, çok az şeffaf

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // 3. Uyarı Yazısı
        GameObject textObj = new GameObject("ReminderText");
        textObj.transform.SetParent(panelObj.transform, false);
        
        Text reminderText = textObj.AddComponent<Text>();
        reminderText.text = "LÜTFEN CİHAZINIZI YAN ÇEVİRİN\n\n🔄";
        
        // Unity yerleşik fontunu yükle (Unity 6 için LegacyRuntime.ttf, eski sürümler için Arial.ttf)
        Font font = null;
        try
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch (System.Exception)
        {
            // Unity 6 öncesi bir sürüm olabilir, Arial.ttf dene
        }

        if (font == null)
        {
            try
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            catch (System.Exception)
            {
                // Her iki font da bulunamazsa varsayılanı kullan
            }
        }

        reminderText.font = font;
        reminderText.fontSize = 42;
        reminderText.fontStyle = FontStyle.Bold;
        reminderText.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        reminderText.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        // Başlangıçta gizli
        canvasObj.SetActive(false);
    }

    void Update()
    {
        // Ekran genişliği yüksekliğinden küçükse dikey (Portrait) konumdadır
        bool isPortrait = Screen.width < Screen.height;

        if (isPortrait)
        {
            if (canvasObj != null && !canvasObj.activeSelf)
            {
                canvasObj.SetActive(true);
                
                // Eğer oyun sahnesindeysek oyunu duraklat (Kullanıcı cihazı çevirirken ölmesin)
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "SampleScene")
                {
                    Time.timeScale = 0f;
                    wasPaused = true;
                }
            }
        }
        else
        {
            if (canvasObj != null && canvasObj.activeSelf)
            {
                canvasObj.SetActive(false);
                
                // Duraklatılmışsa oyunu kaldığı yerden devam ettir
                if (wasPaused)
                {
                    Time.timeScale = 1f;
                    wasPaused = false;
                }
            }
        }
    }

    void OnDestroy()
    {
        if (wasPaused)
        {
            Time.timeScale = 1f;
        }
    }
}
