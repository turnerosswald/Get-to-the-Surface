using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// GTS > Play Surface Level Tour: opens the Surface Level and plays a camera fly-through of all four layers.
public static class SurfaceLevelTour
{
    [MenuItem("GTS/Play Surface Level Tour")]
    public static void PlayTour()
    {
        if (EditorApplication.isPlaying) { EditorApplication.isPlaying = false; return; }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene("Assets/Scenes/SurfaceLevel.unity");
        PlayerPrefs.SetInt(SurfaceCamera.TourKey, 1);
        PlayerPrefs.Save();
        EditorApplication.isPlaying = true;
    }
}
