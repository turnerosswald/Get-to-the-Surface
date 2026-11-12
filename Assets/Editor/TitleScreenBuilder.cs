using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// GTS > Build Title Screen: creates Assets/Scenes/TitleScreen.unity from the pixel art in
// Assets/Sprites/TitleScreen and puts it first in Build Settings so the game opens on it.
public static class TitleScreenBuilder
{
    private const string Folder = "Assets/Sprites/TitleScreen/";
    private const string ScenePath = "Assets/Scenes/TitleScreen.unity";

    // comet sprite size and head position (pixels), plus the lane it flies along
    private static readonly (int w, int h, int hx, int hy, Vector2 from, Vector2 to)[] Comets =
    {
        (96, 15, 92, 7,  new Vector2(470, -25), new Vector2(20, 250)),
        (112, 17, 107, 8, new Vector2(300, -30), new Vector2(-40, 190)),
        (87, 14, 83, 7,  new Vector2(-30, 20),  new Vector2(500, 95)),
        (67, 12, 64, 6,  new Vector2(510, 170), new Vector2(250, 300)),
        (53, 10, 50, 5,  new Vector2(200, -20), new Vector2(330, 300)),
    };
    private static readonly Vector2 CraterPx = new Vector2(172, 118);

    [MenuItem("GTS/Build Title Screen")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        ConfigureSprites();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
        camGo.transform.position = new Vector3(0, 0, -10);
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = TitlePixels.Height / TitlePixels.PPU / 2f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        camGo.AddComponent<AudioListener>();

        Sprite pixel = S("TS_Pixel");

        Make("Space", S("TS_Space"), Vector3.zero, -100);
        var stars = new GameObject("Twinkling Stars"); stars.AddComponent<TwinkleStars>().Setup(pixel);

        var lanes = new CometShower.Lane[Comets.Length];
        for (int i = 0; i < Comets.Length; i++)
            lanes[i] = new CometShower.Lane { sprite = S("TS_Comet_" + i), startPx = Comets[i].from, endPx = Comets[i].to };
        new GameObject("Comets (behind Earth)").AddComponent<CometShower>().Setup(lanes, 0.9f, 3.6f);

        Make("Earth, Moon & Title", S("TS_Foreground"), Vector3.zero, 0);

        var frames = AssetDatabase.FindAssets("TS_Lava_ t:Sprite", new[] { Folder.TrimEnd('/') })
            .Select(g => AssetDatabase.GUIDToAssetPath(g)).OrderBy(p => p)
            .Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p)).ToArray();
        var lava = Make("Flowing Lava", frames[0], TitlePixels.ToWorld(CraterPx.x, CraterPx.y), 1);
        lava.AddComponent<SpriteFrameAnimator>().Setup(frames, 12f);
        new GameObject("Lava Spilling Out").AddComponent<LavaSpill>().Setup(pixel, CraterPx);

        var press = Make("Press Space To Start", S("TS_PressStart"), TitlePixels.ToWorld(307, 171), 3);
        var hero = Make("Rock Elemental (start animation)", S("TS_Hero"), Vector3.zero, 10);
        var fade = Make("Fade", pixel, Vector3.zero, 20);
        fade.transform.localScale = new Vector3(TitlePixels.Width + 2, TitlePixels.Height + 2, 1);
        fade.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
        var loading = Make("Loading Level", S("TS_Loading"), Vector3.zero, 21);

        new GameObject("Title Screen Controller").AddComponent<TitleScreenController>().Setup(
            press.GetComponent<SpriteRenderer>(), S("TS_PressStart"), S("TS_PressStart_Hot"),
            hero.GetComponent<SpriteRenderer>(), fade.GetComponent<SpriteRenderer>(),
            loading.GetComponent<SpriteRenderer>(), CraterPx);

        EditorSceneManager.SaveScene(scene, ScenePath);
        var list = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
        list.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log("Title screen built: " + ScenePath + " (first scene in Build Settings)");
    }

    private static GameObject Make(string name, Sprite sprite, Vector3 pos, int order)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        return go;
    }

    private static Sprite S(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Folder + name + ".png");

    private static void ConfigureSprites()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Folder.TrimEnd('/') }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string file = System.IO.Path.GetFileNameWithoutExtension(path);
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = TitlePixels.PPU;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.maxTextureSize = 2048;

            var st = new TextureImporterSettings();
            ti.ReadTextureSettings(st);
            st.spriteMeshType = SpriteMeshType.FullRect;
            st.spriteAlignment = (int)SpriteAlignment.Center;
            if (file.StartsWith("TS_Comet_"))
            {
                var c = Comets[int.Parse(file.Substring(9))];
                st.spriteAlignment = (int)SpriteAlignment.Custom;
                st.spritePivot = new Vector2((c.hx + 0.5f) / c.w, 1f - (c.hy + 0.5f) / c.h);
            }
            else if (file.StartsWith("TS_PressStart"))
            {
                st.spriteAlignment = (int)SpriteAlignment.TopLeft;
            }
            ti.SetTextureSettings(st);
            ti.SaveAndReimport();
        }
    }
}
