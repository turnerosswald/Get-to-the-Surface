using UnityEngine;

public class AsteroidSpawner2D : MonoBehaviour
{
    [Header("What to spawn")]
    public GameObject[] asteroidPrefabs;   // drag all asteroid prefabs here

    [Header("References")]
    public Transform ship;
    public Camera cam;                     // leave empty to use Camera.main

    [Header("Spawning")]
    public float spawnInterval = 0.8f;
    public float spawnMargin = 3f;         // how far outside the screen they appear
    public int maxAsteroids = 40;

    [Header("Motion")]
    public float minSpeed = 2f;
    public float maxSpeed = 6f;
    public float aimSpread = 4f;           // randomness around the ship so they don't all hit it
    public float maxSpin = 90f;            // degrees/sec
    public float minScale = 0.7f;
    public float maxScale = 1.6f;

    float timer;
    int activeCount;

    void Start()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (ship == null || asteroidPrefabs.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval && activeCount < maxAsteroids)
        {
            timer = 0f;
            Spawn();
        }
    }

    float SpawnRadius()
    {
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        // distance from screen center to a corner, plus margin = always off-screen
        return new Vector2(halfW, halfH).magnitude + spawnMargin;
    }

    void Spawn()
    {
        GameObject prefab = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)];

        // random point on a ring just outside the camera view
        Vector2 center = cam.transform.position;
        Vector2 pos = center + Random.insideUnitCircle.normalized * SpawnRadius();

        GameObject a = Instantiate(prefab, pos, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
        a.transform.localScale *= Random.Range(minScale, maxScale);

        // aim at somewhere near the ship
        Vector2 target = (Vector2)ship.position + Random.insideUnitCircle * aimSpread;
        Vector2 dir = (target - pos).normalized;

        Rigidbody2D rb = a.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = dir * Random.Range(minSpeed, maxSpeed); // rb.velocity on older Unity
            rb.angularVelocity = Random.Range(-maxSpin, maxSpin);
        }

        var cleanup = a.AddComponent<AsteroidCleanup2D>();
        cleanup.cam = cam;
        cleanup.despawnRadius = SpawnRadius() + 10f;
        cleanup.spawner = this;
        activeCount++;
    }

    public void OnAsteroidDestroyed() => activeCount--;
}