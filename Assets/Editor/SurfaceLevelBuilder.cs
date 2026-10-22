using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Builds the whole Surface Level scene in one click: GTS > Build Surface Level.
// The level sits on top of the team's GTTS Background art and is split into four layers:
//   1 The Riverbank - crumbling logs + thorn vines
//   2 The Peaks     - snowy ledges + falling boulders
//   3 The Clouds    - drifting clouds + wind gusts + birds
//   4 The Light     - blinking crystals, then the golden beam (finish)
// Re-running the menu item rebuilds the scene from scratch, so edit the numbers below and run it again.
public static class SurfaceLevelBuilder
{
    private const string ScenePath = "Assets/Scenes/SurfaceLevel.unity";
    private const string SpriteFolder = "Assets/Sprites/Surface/";
    private const string PlayerPrefabPath = "Assets/rock man/Rock Elemental.prefab";

    private const float HalfWidth = 8f;

    [MenuItem("GTS/Build Surface Level")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        ConfigureSprites();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
        camGo.transform.position = new Vector3(0f, 3f, -10f);
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 7.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.33f, 0.62f, 0.85f);
        camGo.AddComponent<AudioListener>();
        var follow = camGo.AddComponent<SurfaceCamera>();

        // Backdrop: top band of the team's GTTS Background (river, jungle, peaks, clouds, sky)
        var backdrop = Sprite("SurfaceBackdrop");
        var bg = new GameObject("Backdrop (GTTS Background)");
        var bgSr = bg.AddComponent<SpriteRenderer>();
        bgSr.sprite = backdrop;
        bgSr.sortingOrder = -100;
        bg.transform.position = new Vector3(0f, -5f + backdrop.bounds.size.y / 2f, 0f);

        // Player (the team's shared Rock Elemental)
        GameObject player = null;
        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (playerPrefab != null)
        {
            player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.transform.position = new Vector3(0f, 1.6f, 0f);
            follow.Setup(player.transform, 3f, 56f);
        }

        var manager = new GameObject("Surface Level Manager");
        var respawn = manager.AddComponent<SurfaceRespawn>();
        if (player != null) respawn.Setup(player.GetComponent<Rigidbody2D>());

        // Invisible side walls + river
        var bounds = new GameObject("Bounds");
        Invisible("Left Wall", new Vector2(-HalfWidth - 0.5f, 30f), new Vector2(1f, 90f), bounds.transform);
        Invisible("Right Wall", new Vector2(HalfWidth + 0.5f, 30f), new Vector2(1f, 90f), bounds.transform);
        var river = Invisible("River (hazard)", new Vector2(0f, -3.5f), new Vector2(2 * HalfWidth, 2f), bounds.transform);
        river.GetComponent<BoxCollider2D>().isTrigger = true;
        river.AddComponent<SurfaceHazard>();

        BuildRiverbank();
        BuildPeaks();
        BuildClouds();
        BuildLight();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings(ScenePath);
        if (player != null) Selection.activeGameObject = player;
        Debug.Log("Surface Level built and saved to " + ScenePath);
    }

    // ---------- Layer 1: The Riverbank ----------
    private static void BuildRiverbank()
    {
        var layer = new GameObject("Layer 1 - The Riverbank").transform;
        Banner(layer, 0.6f, "Layer 1 · The Riverbank", "The logs won't hold for long...");

        Ground(layer, "Start Bank", new Vector2(0f, 0f), 8f, flowers: true);

        Log(layer, new Vector2(-4f, 2.75f), 3f);
        Log(layer, new Vector2(0.5f, 4.75f), 3f);
        Log(layer, new Vector2(4.5f, 6.75f), 2.5f);
        Log(layer, new Vector2(0f, 8.75f), 3f);

        Thorns(layer, new Vector2(-1.8f, 6.3f), 3f);
        Thorns(layer, new Vector2(2.7f, 10.4f), 2.5f);
        Thorns(layer, new Vector2(-6.5f, 5f), 4f);

        Decor(layer, "Vine", new Vector2(-6.9f, 10f), 2, -5);
        Decor(layer, "Vine", new Vector2(6.8f, 9f), 3, -5);

        var ledge = Ground(layer, "Checkpoint Bank", new Vector2(-3.5f, 11f), 4f, flowers: true);
        Checkpoint(layer, new Vector2(-4.5f, 12.25f));
    }

    // ---------- Layer 2: The Peaks ----------
    private static void BuildPeaks()
    {
        var layer = new GameObject("Layer 2 - The Peaks").transform;
        Banner(layer, 12.6f, "Layer 2 · The Peaks", "Watch out for falling boulders!");

        Stone(layer, new Vector2(1.5f, 13.5f), 3f);
        Stone(layer, new Vector2(5f, 16f), 2.5f);
        Stone(layer, new Vector2(0.5f, 18.5f), 2.5f);
        Stone(layer, new Vector2(-4f, 21f), 3f);
        Stone(layer, new Vector2(0f, 23.5f), 2.5f);
        Stone(layer, new Vector2(4f, 26f), 4f);
        Checkpoint(layer, new Vector2(5f, 27.25f));

        var spawner = new GameObject("Boulder Spawner");
        spawner.transform.SetParent(layer, false);
        spawner.transform.position = new Vector3(0f, 29.5f, 0f);
        spawner.AddComponent<FallingRockSpawner>().Setup(Sprite("Boulder"), -6.5f, 6.5f, 1.6f, 11f);
    }

    // ---------- Layer 3: The Clouds ----------
    private static void BuildClouds()
    {
        var layer = new GameObject("Layer 3 - The Clouds").transform;
        Banner(layer, 27.6f, "Layer 3 · The Clouds", "Ride the clouds, fight the wind!");

        Cloud(layer, new Vector2(-3f, 29.5f), 3f, new Vector2(2f, 0f), 1.1f, 0f);
        Cloud(layer, new Vector2(2.5f, 32f), 3f, new Vector2(2.5f, 0f), 0.9f, 1.5f);
        Cloud(layer, new Vector2(-2f, 34.5f), 3f, Vector2.zero, 0f, 0f);
        Cloud(layer, new Vector2(3f, 37f), 3f, new Vector2(2f, 0f), 1.2f, 3f);
        Cloud(layer, new Vector2(-3f, 39.5f), 3f, Vector2.zero, 0f, 0f);
        Cloud(layer, new Vector2(1f, 42f), 2.5f, new Vector2(2.5f, 0f), 1f, 0.7f);
        Cloud(layer, new Vector2(-3.5f, 44.5f), 4f, Vector2.zero, 0f, 0f);
        Checkpoint(layer, new Vector2(-4.5f, 45.6f));

        Wind(layer, new Vector2(0f, 35.8f), new Vector2(2 * HalfWidth, 3.5f), toRight: false);
        Wind(layer, new Vector2(0f, 41f), new Vector2(2 * HalfWidth, 2.5f), toRight: true);

        Bird(layer, 33.2f, 3f, 1);
        Bird(layer, 38.2f, 4f, -1);
        Bird(layer, 43.2f, 3.5f, 1);
    }

    // ---------- Layer 4: The Light ----------
    private static void BuildLight()
    {
        var layer = new GameObject("Layer 4 - The Light").transform;
        Banner(layer, 45.8f, "Layer 4 · The Light", "Time your jumps - the crystals fade!");

        Crystal(layer, new Vector2(-3f, 47f), 0f);
        Crystal(layer, new Vector2(0.5f, 49f), 0.8f);
        Crystal(layer, new Vector2(-2.5f, 51f), 1.6f);
        Crystal(layer, new Vector2(1.5f, 53f), 0.4f);

        var gold = Tiled("Finish Platform", "Gold", new Vector2(5f, 55f), new Vector2(3.5f, 0.75f), layer, -1);
        gold.AddComponent<BoxCollider2D>().size = new Vector2(3.5f, 0.75f);

        var beam = Tiled("Golden Beam", "Beam", new Vector2(5f, 60f), new Vector2(3f, 10f), layer, -2, sliced: false, tiled: false);
        beam.transform.localScale = new Vector3(3f / 1f, 10f / 4f, 1f);

        var goal = new GameObject("Finish (reach the light)");
        goal.transform.SetParent(layer, false);
        goal.transform.position = new Vector3(5f, 56.5f, 0f);
        var gc = goal.AddComponent<BoxCollider2D>();
        gc.isTrigger = true;
        gc.size = new Vector2(3.5f, 2f);
        goal.AddComponent<SurfaceGoal>();
    }

    // ---------- Building blocks ----------
    private static GameObject Ground(Transform parent, string name, Vector2 pos, float width, bool flowers)
    {
        var go = Tiled(name, "Grass", pos, new Vector2(width, 1f), parent, -1);
        go.AddComponent<BoxCollider2D>().size = new Vector2(width, 1f);
        if (flowers)
        {
            for (float x = -width / 2 + 0.4f; x < width / 2 - 0.3f; x += 0.9f)
                Decor(parent, "Flower" + Random.Range(0, 3), new Vector2(pos.x + x + Random.Range(-0.2f, 0.2f), pos.y + 0.75f), 0, 1);
        }
        return go;
    }

    private static void Log(Transform parent, Vector2 pos, float width)
    {
        var go = Tiled("Crumbling Log", "Log", pos, new Vector2(width, 0.75f), parent, -1);
        go.AddComponent<BoxCollider2D>().size = new Vector2(width, 0.75f);
        go.AddComponent<CollapsingPlatform>().Configure(0.7f);
    }

    private static void Stone(Transform parent, Vector2 pos, float width)
    {
        var go = Tiled("Snowy Ledge", "Stone", pos, new Vector2(width, 1f), parent, -1);
        go.AddComponent<BoxCollider2D>().size = new Vector2(width, 1f);
    }

    private static void Thorns(Transform parent, Vector2 pos, float height)
    {
        var go = Tiled("Thorn Vine (hazard)", "Thorns", pos, new Vector2(1f, height), parent, 1);
        var c = go.AddComponent<BoxCollider2D>();
        c.size = new Vector2(0.8f, height);
        c.isTrigger = true;
        go.AddComponent<SurfaceHazard>();
    }

    private static void Cloud(Transform parent, Vector2 pos, float width, Vector2 travel, float speed, float phase)
    {
        var go = Tiled(travel == Vector2.zero ? "Cloud" : "Drifting Cloud", "Cloud", pos, new Vector2(width, 1.25f), parent, -1, sliced: true);
        var c = go.AddComponent<BoxCollider2D>();
        c.size = new Vector2(width - 0.3f, 0.6f);
        c.offset = new Vector2(0f, 0.15f);
        if (travel != Vector2.zero)
        {
            go.AddComponent<Rigidbody2D>();
            go.AddComponent<MovingPlatform>().Setup(travel, speed, phase);
        }
    }

    private static void Crystal(Transform parent, Vector2 pos, float offset)
    {
        var go = Tiled("Blinking Crystal", "Crystal", pos, new Vector2(2.5f, 0.75f), parent, -1, sliced: true);
        go.AddComponent<BoxCollider2D>().size = new Vector2(2.5f, 0.75f);
        go.AddComponent<BlinkPlatform>().Setup(2.2f, 1.2f, offset);
    }

    private static void Wind(Transform parent, Vector2 pos, Vector2 size, bool toRight)
    {
        var go = new GameObject(toRight ? "Wind Gust (pushes right)" : "Wind Gust (pushes left)");
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        var c = go.AddComponent<BoxCollider2D>();
        c.size = size;
        c.isTrigger = true;
        c.usedByEffector = true;
        var eff = go.AddComponent<AreaEffector2D>();
        eff.forceAngle = toRight ? 0f : 180f;
        eff.forceMagnitude = 7f;
        eff.linearDamping = 0f;
        go.AddComponent<WindStreaks>().Setup(Sprite("Wind"), size, toRight ? 8f : -8f);
    }

    private static void Bird(Transform parent, float y, float speed, int dir)
    {
        var go = new GameObject("Bird (hazard)");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(-dir * 5f, y, 0f);
        go.transform.localScale = Vector3.one * 1.6f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite("Bird");
        sr.sortingOrder = 2;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var c = go.AddComponent<BoxCollider2D>();
        c.isTrigger = true;
        c.size = new Vector2(0.8f, 0.5f);
        go.AddComponent<SurfaceHazard>();
        go.AddComponent<SurfaceBird>().Setup(-HalfWidth + 0.5f, HalfWidth - 0.5f, speed, dir);
    }

    private static void Checkpoint(Transform parent, Vector2 pos)
    {
        var go = new GameObject("Checkpoint Lantern");
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite("Lantern");
        sr.sortingOrder = 1;
        var c = go.AddComponent<BoxCollider2D>();
        c.isTrigger = true;
        c.size = new Vector2(1.2f, 1.6f);
        go.AddComponent<SurfaceCheckpoint>();
    }

    private static void Banner(Transform parent, float y, string title, string subtitle)
    {
        var go = new GameObject("Layer Banner");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(0f, y, 0f);
        var c = go.AddComponent<BoxCollider2D>();
        c.isTrigger = true;
        c.size = new Vector2(2 * HalfWidth, 0.5f);
        go.AddComponent<LayerBanner>().Setup(title, subtitle);
    }

    private static void Decor(Transform parent, string sprite, Vector2 pos, int tilesDown, int order)
    {
        var go = new GameObject("Decor " + sprite);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite(sprite);
        sr.sortingOrder = order;
        if (tilesDown > 0)
        {
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(0.5f, 2f * tilesDown);
        }
    }

    private static GameObject Invisible(string name, Vector2 pos, Vector2 size, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.AddComponent<BoxCollider2D>().size = size;
        return go;
    }

    private static GameObject Tiled(string name, string sprite, Vector2 pos, Vector2 size, Transform parent, int order, bool sliced = false, bool tiled = true)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite(sprite);
        sr.sortingOrder = order;
        if (sliced) { sr.drawMode = SpriteDrawMode.Sliced; sr.size = size; }
        else if (tiled) { sr.drawMode = SpriteDrawMode.Tiled; sr.tileMode = SpriteTileMode.Continuous; sr.size = size; }
        return go;
    }

    private static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + name + ".png");

    // Pixel-art import settings for every sprite in Assets/Sprites/Surface.
    private static void ConfigureSprites()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { SpriteFolder.TrimEnd('/') }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            bool isBackdrop = path.EndsWith("SurfaceBackdrop.png");
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 16f;
            ti.filterMode = isBackdrop ? FilterMode.Bilinear : FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.wrapMode = TextureWrapMode.Clamp;

            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(settings);

            if (path.EndsWith("Cloud.png")) ti.spriteBorder = new Vector4(12, 0, 12, 0);
            else if (path.EndsWith("Crystal.png")) ti.spriteBorder = new Vector4(6, 0, 6, 0);
            else ti.spriteBorder = Vector4.zero;

            ti.SaveAndReimport();
        }
    }

    private static void AddToBuildSettings(string path)
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(s => s.path == path)) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
