using UnityEngine;
using UnityEditor;

public class ResetPanels
{
    public static void Execute()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (var go in allObjects)
        {
            if (go.scene.name == "MainMenuScene")
            {
                if (go.name == "SettingsPanel") go.SetActive(false);
                if (go.name == "MainPanel") go.SetActive(true);
                if (go.name == "LeaderboardPanel") go.SetActive(false);
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        
        Debug.Log("Paneller sıfırlandı. MainPanel aktif, diğerleri kapalı.");
    }
}
