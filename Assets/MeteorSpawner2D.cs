using UnityEngine;
using System.Collections.Generic;

// Fast diagonal asteroids that streak in from the top corners, with an edge warning first.
public class MeteorSpawner2D : MonoBehaviour
{
    [Header("What to spawn")]
    public GameObject[] meteorPrefabs;   // can reuse the same asteroid prefabs

    [Header("References")]
    public Transform ship;
    public Camera cam;                     // leave empty to use Camera.main

    [Header("Timing")]
    public float firstDelay = 4f;          // seconds before the first one (lets the banner finish)
    public float minInterval = 1.5f;       // time between incoming asteroids (random in this range)
    public float maxInterval = 3.5f;
    public float warningTime = 0.7f;       // how long the "!" shows before it appears

    [Header("Motion")]
    public float minSpeed = 12f;
    public float maxSpeed = 18f;
    public float aimSpread = 1.5f;         // smaller = more accurate at the ship
    public float spawnMargin = 2f;         // how far off-screen they start
    public float maxSpin = 360f;           // degrees/sec
    public float minScale = 0.6f;
    public float maxScale = 1.1f;

    [Header("Pass through asteroids")]
    public float minPhaseTime = 1f;        // after its first asteroid hit, passes through for at least this long
    public float maxPhaseTime = 3f;        // safety limit if it's still inside asteroids

    [Header("Look")]
    public bool addTrail = true;
    public Color trailColor = new Color(1f, 0.6f, 0.2f, 0.8f);
    public float trailTime = 0.25f;        // longer = longer trail
    [Range(0.05f, 1f)] public float trailWidth = 0.3f;   // trail width as a fraction of the meteor's width
    public Color warningColor = new Color(1f, 0.25f, 0.2f);
    public int warningSize = 48;

    class Pending { public Vector2 offset; public float spawnAt; }
    readonly List<Pending> pending = new List<Pending>();

    float nextWave;
    Material trailMat;
    GUIStyle warnStyle;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        nextWave = Time.time + firstDelay;

        Shader sh = Shader.Find("Sprites/Default");
        if (sh == null) sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        trailMat = new Material(sh);
    }

    Vector2 HalfExtents()
    {
        float halfH = cam.orthographicSize;
        return new Vector2(halfH * cam.aspect, halfH);
    }

    void Update()
    {
        if (ship == null || cam == null || meteorPrefabs.Length == 0) return;

        // schedule the next incoming asteroid (with a warning first)
        if (Time.time >= nextWave)
        {
            Vector2 half = HalfExtents();
            float side = Random.value < 0.5f ? -1f : 1f;           // left or right corner
            Vector2 offset = new Vector2(
                side * (half.x + spawnMargin),
                Random.Range(0.2f * half.y, half.y + spawnMargin));  // upper part of the side

            pending.Add(new Pending { offset = offset, spawnAt = Time.time + warningTime });
            nextWave = Time.time + Random.Range(minInterval, maxInterval);
        }

        // spawn the ones whose warning is over
        for (int i = pending.Count - 1; i >= 0; i--)
        {
            if (Time.time >= pending[i].spawnAt)
            {
                Spawn(pending[i].offset);
                pending.RemoveAt(i);
            }
        }
    }

    void Spawn(Vector2 offset)
    {
        Vector2 half = HalfExtents();
        Vector2 pos = (Vector2)cam.transform.position + offset;

        GameObject prefab = meteorPrefabs[Random.Range(0, meteorPrefabs.Length)];
        GameObject a = Instantiate(prefab, pos, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
        a.transform.localScale *= Random.Range(minScale, maxScale);

        // aim at (roughly) where the ship is now
        Vector2 target = (Vector2)ship.position + Random.insideUnitCircle * aimSpread;
        Vector2 dir = (target - pos).normalized;

        Rigidbody2D rb = a.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;  // fast, so don't tunnel
            rb.linearVelocity = dir * Random.Range(minSpeed, maxSpeed);       // rb.velocity on older Unity
            rb.angularVelocity = Random.Range(-maxSpin, maxSpin);
        }

        if (addTrail) AddTrail(a);

        var phase = a.AddComponent<MeteorPhase>();   // one free pass through the asteroid field
        phase.minPhaseTime = minPhaseTime;
        phase.maxPhaseTime = maxPhaseTime;

        // ShipHealth recognises asteroids by this component, so these do damage too
        var cleanup = a.AddComponent<AsteroidCleanup2D>();
        cleanup.cam = cam;
        cleanup.despawnRadius = half.magnitude + spawnMargin + 15f;
        cleanup.spawner = null;   // not counted against the normal spawner's limit
    }

    void AddTrail(GameObject a)
    {
        var sr = a.GetComponentInChildren<SpriteRenderer>();
        var tr = a.AddComponent<TrailRenderer>();
        tr.time = trailTime;
        float w = (sr != null ? sr.bounds.size.x : 1f) * trailWidth;
        tr.widthCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.3f, 0.7f),    // narrows quickly behind the meteor
            new Keyframe(1f, 0f));       // tapers to a point
        tr.widthMultiplier = w;
        tr.numCapVertices = 4;           // rounded end instead of a flat edge
        tr.numCornerVertices = 2;
        tr.material = trailMat;
        tr.startColor = trailColor;
        tr.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
        if (sr != null)
        {
            tr.sortingLayerID = sr.sortingLayerID;
            tr.sortingOrder = sr.sortingOrder - 1;   // trail behind the rock
        }
    }

    // blinking "!" at the screen edge where the asteroid will come from
    void OnGUI()
    {
        if (cam == null || pending.Count == 0) return;
        if (Mathf.Repeat(Time.time * 8f, 1f) > 0.6f) return;   // blink

        if (warnStyle == null)
            warnStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        warnStyle.fontSize = warningSize;

        Vector2 half = HalfExtents();
        float inset = warningSize;
        foreach (var p in pending)
        {
            float x = (p.offset.x / (2f * half.x) + 0.5f) * Screen.width;
            float y = (1f - (p.offset.y / (2f * half.y) + 0.5f)) * Screen.height;
            x = Mathf.Clamp(x, inset, Screen.width - inset);
            y = Mathf.Clamp(y, Screen.height * 0.15f, Screen.height - inset);   // stay below the fuel bar

            Rect r = new Rect(x - 40, y - 40, 80, 80);
            warnStyle.normal.textColor = new Color(0.1f, 0.05f, 0.15f, 0.9f);
            GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), "!", warnStyle);
            warnStyle.normal.textColor = warningColor;
            GUI.Label(r, "!", warnStyle);
        }
    }
}