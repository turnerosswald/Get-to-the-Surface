using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Builds the whole Surface Level scene in one click: GTS > Build Surface Level.
// Re-running it rebuilds the scene from scratch, so tweak the PLAN below and run it again.
public static class SurfaceLevelBuilder
{
    private const string ScenePath = "Assets/Scenes/SurfaceLevel.unity";
    private const string SpriteFolder = "Assets/Sprites/Surface";
    private const string SpritePath = SpriteFolder + "/SurfaceBlock.png";
    private const string PlayerPrefabPath = "Assets/rock man/Rock Elemental.prefab";

    private const float SurfaceHeight = 35f;

    // S = safe stone (checkpoint), C = crumbling, F = fast-crumbling
    private static readonly (float x, float y, float width, char type)[] Plan =
    {
        (0f,    0f,    8f,   'S'),
        (-3f,   2.5f,  3f,   'C'),
        (2f,    5f,    3f,   'C'),
        (-2f,   7.5f,  2.5f, 'C'),
        (2.5f, 10f,    3f,   'S'),
        (-1f,  12.5f,  2.5f, 'C'),
        (-4f,  15f,    2.5f, 'C'),
        (0f,   17.5f,  2f,   'F'),
        (3.5f, 20f,    3f,   'S'),
        (0.5f, 22.5f,  2f,   'F'),
        (-3f,  25f,    2f,   'C'),
        (0f,   27.5f,  1.8f, 'F'),
        (3f,   30f,    1.8f, 'F'),
        (-0.5f,32.5f,  3f,   'C'),
    };

    private static readonly Color StoneColor = new Color(0.45f, 0.42f, 0.4f);
    private static readonly Color CrumbleColor = new Color(0.78f, 0.62f, 0.42f);
    private static readonly Color FastColor = new Color(0.85f, 0.45f, 0.32f);
    private static readonly Color WallColor = new Color(0.18f, 0.15f, 0.14f);
    private static readonly Color GrassColor = new Color(0.35f, 0.75f, 0.35f);

    [MenuItem("GTS/Build Surface Level")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Sprite block = GetOrCreateBlockSprite();
        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (playerPrefab == null)
        {
            EditorUtility.DisplayDialog("Surface Level", "Couldn't find the player prefab at " + PlayerPrefabPath, "OK");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
        camGo.transform.position = new Vector3(0f, 3f, -10f);
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 7f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.05f, 0.08f);
        camGo.AddComponent<AudioListener>();
        var follow = camGo.AddComponent<SurfaceCamera>();

        // Player
        var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        player.transform.position = new Vector3(0f, 1.5f, 0f);
        var body = player.GetComponent<Rigidbody2D>();
        follow.Setup(player.transform, 0f, SurfaceHeight);

        // Respawn manager
        var manager = new GameObject("Surface Level Manager");
        var respawn = manager.AddComponent<SurfaceRespawn>();
        respawn.Setup(body);

        // Platforms
        var platforms = new GameObject("Platforms");
        for (int i = 0; i < Plan.Length; i++)
        {
            var p = Plan[i];
            Color color = p.type == 'S' ? StoneColor : p.type == 'F' ? FastColor : CrumbleColor;
            var go = MakeBlock($"{TypeName(p.type)} {i}", block, color, new Vector2(p.x, p.y), new Vector2(p.width, 0.5f), platforms.transform);

            if (p.type == 'C' || p.type == 'F')
            {
                var cp = go.AddComponent<CollapsingPlatform>();
                cp.Configure(p.type == 'F' ? 0.35f : 0.8f);
            }
            else if (i > 0)
            {
                var lantern = MakeBlock("Checkpoint", block, new Color(1f, 0.85f, 0.3f),
                    new Vector2(p.x, p.y + 0.75f), new Vector2(0.4f, 0.6f), go.transform.parent);
                lantern.GetComponent<BoxCollider2D>().isTrigger = true;
                lantern.AddComponent<SurfaceCheckpoint>();
            }
        }

        // Side walls
        var walls = new GameObject("Walls");
        MakeBlock("Left Wall", block, WallColor, new Vector2(-7.5f, SurfaceHeight / 2f), new Vector2(1f, SurfaceHeight + 20f), walls.transform);
        MakeBlock("Right Wall", block, WallColor, new Vector2(7.5f, SurfaceHeight / 2f), new Vector2(1f, SurfaceHeight + 20f), walls.transform);

        // The surface: grass, sun, and the finish trigger
        var surface = new GameObject("Surface");
        MakeBlock("Grass", block, GrassColor, new Vector2(0f, SurfaceHeight), new Vector2(16f, 1f), surface.transform);
        var sun = MakeBlock("Sun", block, new Color(1f, 0.93f, 0.5f), new Vector2(4.5f, SurfaceHeight + 5f), new Vector2(2f, 2f), surface.transform);
        Object.DestroyImmediate(sun.GetComponent<BoxCollider2D>());
        var goal = MakeBlock("Finish", block, new Color(1f, 1f, 1f, 0f), new Vector2(0f, SurfaceHeight + 1.5f), new Vector2(16f, 2f), surface.transform);
        goal.GetComponent<BoxCollider2D>().isTrigger = true;
        goal.AddComponent<SurfaceGoal>();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings(ScenePath);
        Selection.activeGameObject = player;
        Debug.Log("Surface Level built and saved to " + ScenePath + ". Press Play to test!");
    }

    private static string TypeName(char t) => t == 'S' ? "Stone" : t == 'F' ? "Fast Crumble" : "Crumble";

    private static GameObject MakeBlock(string name, Sprite sprite, Color color, Vector2 pos, Vector2 size, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        go.AddComponent<BoxCollider2D>();
        return go;
    }

    private static Sprite GetOrCreateBlockSprite()
    {
        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        if (existing != null) return existing;

        Directory.CreateDirectory(SpriteFolder);
        var tex = new Texture2D(16, 16);
        var pixels = Enumerable.Repeat(Color.white, 16 * 16).ToArray();
        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(SpritePath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(SpritePath);

        var importer = (TextureImporter)AssetImporter.GetAtPath(SpritePath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 16f;
        importer.filterMode = FilterMode.Point;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
    }

    private static void AddToBuildSettings(string path)
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(s => s.path == path)) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
