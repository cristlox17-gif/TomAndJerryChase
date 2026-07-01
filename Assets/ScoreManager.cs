using UnityEngine;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    private const int LeaderboardSize = 5;

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

    // Yeni skoru kaydet ve sırala
    public static void SaveScore(string playerName, int newScore)
    {
        List<ScoreEntry> scores = GetScores();

        // Yeni skoru listeye ekle
        scores.Add(new ScoreEntry(playerName, newScore));

        // Büyükten küçüğe sırala (descending)
        scores.Sort((x, y) => y.score.CompareTo(x.score));

        // İlk 5 skoru sakla, fazlasını sil
        if (scores.Count > LeaderboardSize)
        {
            scores.RemoveRange(LeaderboardSize, scores.Count - LeaderboardSize);
        }

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

        for (int i = 0; i < LeaderboardSize; i++)
        {
            if (PlayerPrefs.HasKey("HighScore_Value_" + i))
            {
                string name = PlayerPrefs.GetString("HighScore_Name_" + i, "Bilinmeyen");
                int score = PlayerPrefs.GetInt("HighScore_Value_" + i, 0);
                scores.Add(new ScoreEntry(name, score));
            }
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

    // Tüm skorları temizle
    public static void ClearScores()
    {
        for (int i = 0; i < LeaderboardSize; i++)
        {
            PlayerPrefs.DeleteKey("HighScore_Name_" + i);
            PlayerPrefs.DeleteKey("HighScore_Value_" + i);
        }
        PlayerPrefs.Save();
    }
}
