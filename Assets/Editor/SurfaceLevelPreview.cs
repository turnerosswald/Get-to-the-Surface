using System.IO;
using UnityEditor;
using UnityEngine;

// GTS > Capture Surface Level Preview: saves Logs/SurfacePreview.png (whole level)
// and Logs/SurfaceLayers.png (one in-game-sized shot per layer).
public static class SurfaceLevelPreview
{
    [MenuItem("GTS/Capture Surface Level Preview")]
    public static void Capture()
    {
        var full = Render(new Vector2(0f, 28.75f), 34f, 540, 1224);
        File.WriteAllBytes("Logs/SurfacePreview.png", full.EncodeToPNG());

        float[] centers = { 5f, 19.5f, 37f, 51f };
        int w = 640, h = 360;
        var grid = new Texture2D(w * 2, h * 2, TextureFormat.RGB24, false);
        for (int i = 0; i < 4; i++)
        {
            var shot = Render(new Vector2(0f, centers[i]), 7.5f, w, h);
            int gx = (i % 2) * w, gy = (1 - i / 2) * h;
            grid.SetPixels(gx, gy, w, h, shot.GetPixels());
            Object.DestroyImmediate(shot);
        }
        grid.Apply();
        File.WriteAllBytes("Logs/SurfaceLayers.png", grid.EncodeToPNG());
        Object.DestroyImmediate(full);
        Object.DestroyImmediate(grid);
        Debug.Log("Saved Logs/SurfacePreview.png and Logs/SurfaceLayers.png");
    }

    private static Texture2D Render(Vector2 center, float size, int w, int h)
    {
        var go = new GameObject("Preview Camera");
        var cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = size;
        cam.transform.position = new Vector3(center.x, center.y, -10f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        var rt = new RenderTexture(w, h, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        cam.targetTexture = null;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(go);
        return tex;
    }
}
