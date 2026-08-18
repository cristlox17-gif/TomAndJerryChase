using UnityEngine;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    private const int LeaderboardSize = 100; // 5 yerine 100 yapıldı

    [System.Serializable]
    public struct ScoreEntry
    {
        public string name;
        public int score;

        public ScoreEntry(string name, int score)
        {
            this.name = name;
            this.score = score;
        }
    }

    // Yeni skoru kaydet ve sırala (Her oyuncudan sadece 1 adet en yüksek skor kalır)
    public static void SaveScore(string playerName, int newScore)
    {
        List<ScoreEntry> scores = GetScores();

        // Oyuncunun zaten listede olup olmadığını kontrol et (büyük-küçük harf duyarsız)
        int existingIndex = scores.FindIndex(s => s.name.Equals(playerName, System.StringComparison.OrdinalIgnoreCase));

        if (existingIndex >= 0)
        {
            // Eğer oyuncu varsa ve yeni skoru eskisinden yüksekse güncelle
            if (newScore > scores[existingIndex].score)
            {
                scores[existingIndex] = new ScoreEntry(scores[existingIndex].name, newScore);
            }
            else
            {
                // Yeni skor eskisinden yüksek değilse kaydetme, çık
                return;
            }
        }
        else
        {
            // Eğer listede yoksa yeni giriş ekle
            scores.Add(new ScoreEntry(playerName, newScore));
        }

        // Büyükten küçüğe sırala
        scores.Sort((x, y) => y.score.CompareTo(x.score));

        // En fazla 100 skoru sakla, fazlasını sil
        if (scores.Count > LeaderboardSize)
        {
            scores.RemoveRange(LeaderboardSize, scores.Count - LeaderboardSize);
        }

        // Kaydedilen toplam skor sayısını tut
        PlayerPrefs.SetInt("Leaderboard_Count", scores.Count);

        // Skorları yerel hafızaya kaydet
        for (int i = 0; i < LeaderboardSize; i++)
        {
            if (i < scores.Count)
            {
                PlayerPrefs.SetString("HighScore_Name_" + i, scores[i].name);
                PlayerPrefs.SetInt("HighScore_Value_" + i, scores[i].score);
            }
            else
            {
                PlayerPrefs.DeleteKey("HighScore_Name_" + i);
                PlayerPrefs.DeleteKey("HighScore_Value_" + i);
            }
        }
        PlayerPrefs.Save();
    }

    // Mevcut skorları listeden oku
    public static List<ScoreEntry> GetScores()
    {
        List<ScoreEntry> scores = new List<ScoreEntry>();
        int count = PlayerPrefs.GetInt("Leaderboard_Count", 0);

        // Eski kayıtlardan kalma veriler varsa ve count sıfırsa geriye dönük uyumluluk için ilk 100'ü tara
        if (count == 0)
        {
            for (int i = 0; i < LeaderboardSize; i++)
            {
                if (PlayerPrefs.HasKey("HighScore_Value_" + i))
                {
                    string name = PlayerPrefs.GetString("HighScore_Name_" + i, "Bilinmeyen");
                    int score = PlayerPrefs.GetInt("HighScore_Value_" + i, 0);
                    scores.Add(new ScoreEntry(name, score));
                }
                else
                {
                    break;
                }
            }
            return scores;
        }

        for (int i = 0; i < count; i++)
        {
            string name = PlayerPrefs.GetString("HighScore_Name_" + i, "Bilinmeyen");
            int score = PlayerPrefs.GetInt("HighScore_Value_" + i, 0);
            scores.Add(new ScoreEntry(name, score));
        }
        return scores;
    }

    // Skor Tablosu için Metin Formatı Oluştur
    public static string GetLeaderboardText()
    {
        List<ScoreEntry> scores = GetScores();
        string text = "--- EN YÜKSEK SKORLAR ---\n\n";

        if (scores.Count == 0)
        {
            return text + "Henüz skor kaydedilmedi.\nİlk oynayan sen ol!";
        }

        for (int i = 0; i < scores.Count; i++)
        {
            text += (i + 1) + ". " + scores[i].name + " - " + scores[i].score + " Peynir\n";
        }
        return text;
    }

    // Oyuncunun ismini skor tablosunda günceller (eski ismi yenisiyle değiştirir, skoru korur)
    public static void RenamePlayer(string oldName, string newName)
    {
        if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName) || oldName.Equals(newName, System.StringComparison.OrdinalIgnoreCase)) return;

        List<ScoreEntry> scores = GetScores();

        // Eski isimdeki oyuncuyu ara
        int index = scores.FindIndex(s => s.name.Equals(oldName, System.StringComparison.OrdinalIgnoreCase));

        if (index >= 0)
        {
            // İsmi güncelle, skoru aynı bırak
            scores[index] = new ScoreEntry(newName, scores[index].score);

            // Skorları yerel hafızaya tekrar kaydet
            PlayerPrefs.SetInt("Leaderboard_Count", scores.Count);
            for (int i = 0; i < LeaderboardSize; i++)
            {
                if (i < scores.Count)
                {
                    PlayerPrefs.SetString("HighScore_Name_" + i, scores[i].name);
                    PlayerPrefs.SetInt("HighScore_Value_" + i, scores[i].score);
                }
                else
                {
                    PlayerPrefs.DeleteKey("HighScore_Name_" + i);
                    PlayerPrefs.DeleteKey("HighScore_Value_" + i);
                }
            }
            PlayerPrefs.Save();
        }
    }

    // Tüm skorları temizle
    public static void ClearScores()
    {
        int count = PlayerPrefs.GetInt("Leaderboard_Count", LeaderboardSize);
        for (int i = 0; i < count; i++)
        {
            PlayerPrefs.DeleteKey("HighScore_Name_" + i);
            PlayerPrefs.DeleteKey("HighScore_Value_" + i);
        }
        PlayerPrefs.DeleteKey("Leaderboard_Count");
        PlayerPrefs.Save();
    }
}
