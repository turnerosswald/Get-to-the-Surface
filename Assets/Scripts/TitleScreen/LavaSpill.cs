using System.Collections.Generic;
using UnityEngine;

// Lava keeps spilling out of the crater rim, runs a fixed distance across the surface,
// cools from yellow to dark red, then disappears. New blobs are spawned continuously.
public class LavaSpill : MonoBehaviour
{
    [SerializeField] private Sprite pixel;
    [SerializeField] private Vector2 craterCenterPx = new Vector2(172, 118);
    [SerializeField] private Vector2 spawnEvery = new Vector2(0.05f, 0.1f);
    [SerializeField] private int sortingOrder = 2;

    private const int TrailLength = 11;
    private static readonly Color[] Heat =
    {
        new Color32(255, 236, 140, 255), new Color32(255, 170, 40, 255),
        new Color32(220, 80, 24, 255), new Color32(120, 32, 20, 255)
    };

    private class Blob
    {
        public float angle, startRadius, speed, range, wobble, born;
        public int size;
        public SpriteRenderer[] parts;
    }

    private readonly List<Blob> blobs = new List<Blob>();
    private float nextSpawn;

    public void Setup(Sprite pixelSprite, Vector2 centerPx)
    {
        pixel = pixelSprite;
        craterCenterPx = centerPx;
    }

    // Same jagged outline as the crater art.
    private static float CraterRadius(float a) =>
        38f + 7f * Mathf.Sin(a * 3f + 1f) + 5f * Mathf.Sin(a * 5f + 2f) + 3f * Mathf.Sin(a * 9f);

    private void Update()
    {
        while (Time.time >= nextSpawn)
        {
            Spawn();
            nextSpawn = Time.time + Random.Range(spawnEvery.x, spawnEvery.y);
        }

        for (int i = blobs.Count - 1; i >= 0; i--)
        {
            Blob b = blobs[i];
            float d = (Time.time - b.born) * b.speed;
            if (d > b.range)
            {
                Destroy(b.parts[0].transform.parent.gameObject);
                blobs.RemoveAt(i);
                continue;
            }

            float frac = d / b.range;
            for (int k = 0; k < TrailLength; k++)
            {
                SpriteRenderer part = b.parts[k];
                float dd = Mathf.Max(0f, d - k * 1.2f);
                float a = b.angle + 0.08f * Mathf.Sin(b.wobble + dd * 0.3f);
                float r = b.startRadius + dd;
                float px = Mathf.Floor(craterCenterPx.x + Mathf.Cos(a) * r);
                float py = Mathf.Floor(craterCenterPx.y + Mathf.Sin(a) * r);
                part.transform.position = TitlePixels.ToWorld(px, py);

                float heat = frac + k * 0.08f;
                if (heat >= 1f) { part.enabled = false; continue; }
                part.enabled = true;
                part.color = heat < 0.3f ? Heat[0] : heat < 0.55f ? Heat[1] : heat < 0.8f ? Heat[2] : Heat[3];
                float s = k < 4 ? b.size : 1;
                part.transform.localScale = new Vector3(s, s, 1f);
            }
        }
    }

    private void Spawn()
    {
        var b = new Blob
        {
            angle = Random.Range(-Mathf.PI, Mathf.PI),
            speed = Random.Range(14f, 24f),
            range = Random.Range(22f, 38f),
            wobble = Random.Range(0f, 6.28f),
            size = Random.value < 0.67f ? 2 : 3,
            born = Time.time,
            parts = new SpriteRenderer[TrailLength]
        };
        b.startRadius = CraterRadius(b.angle) - 2f;

        var root = new GameObject("Lava Blob");
        root.transform.SetParent(transform, false);
        for (int k = 0; k < TrailLength; k++)
        {
            var go = new GameObject("p");
            go.transform.SetParent(root.transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = pixel;
            sr.sortingOrder = sortingOrder;
            sr.enabled = false;
            b.parts[k] = sr;
        }
        blobs.Add(b);
    }
}
