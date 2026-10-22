using UnityEngine;

// Surface Level, Layer 4 (The Light): crystal platforms that flicker and vanish on a rhythm.
public class BlinkPlatform : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float onTime = 2.2f;
    [SerializeField, Min(0.1f)] private float offTime = 1.2f;
    [SerializeField, Min(0f)] private float offset;
    [SerializeField, Min(0f)] private float warning = 0.6f;

    private SpriteRenderer sr;
    private Collider2D col;

    public void Setup(float on, float off, float startOffset)
    {
        onTime = on; offTime = off; offset = startOffset;
    }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void Update()
    {
        float cycle = onTime + offTime;
        float t = (Time.time + offset) % cycle;
        bool solid = t < onTime;
        col.enabled = solid;

        Color c = sr.color;
        if (!solid) c.a = 0.15f;
        else if (t > onTime - warning) c.a = (Mathf.Sin(Time.time * 40f) > 0f) ? 1f : 0.35f;
        else c.a = 1f;
        sr.color = c;
    }
}
