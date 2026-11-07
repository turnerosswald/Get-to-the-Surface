using System.Collections;
using UnityEngine;

// Surface Level: a platform that shakes, then crumbles a moment after the player lands on it.
// It rebuilds itself after a few seconds so the player can try again.
[RequireComponent(typeof(Collider2D))]
public class CollapsingPlatform : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField, Min(0f)] private float collapseDelay = 0.8f;
    [SerializeField, Min(0f)] private float fallTime = 0.6f;
    [SerializeField, Min(0f)] private float fallDistance = 3f;

    [Header("Rebuild")]
    [SerializeField] private bool respawn = true;
    [SerializeField, Min(0f)] private float respawnDelay = 3f;

    [Header("Look")]
    [SerializeField, Min(0f)] private float shakeAmount = 0.06f;
    [SerializeField] private Color warningColor = new Color(1f, 0.45f, 0.3f);

    private Collider2D col;
    private SpriteRenderer sr;
    private Vector3 startPos;
    private Color startColor = Color.white;
    private bool collapsing;

    public void Configure(float delay)
    {
        collapseDelay = delay;
    }

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        startPos = transform.position;
        if (sr != null) startColor = sr.color;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Only the player's main body (the object with Movement) starts the collapse,
        // not thrown limbs.
        if (collapsing || collision.rigidbody == null) return;
        if (collision.rigidbody.GetComponent<Movement>() == null) return;
        StartCoroutine(Collapse());
    }

    private IEnumerator Collapse()
    {
        collapsing = true;

        // 1) Shake and turn red as a warning.
        float t = 0f;
        while (t < collapseDelay)
        {
            t += Time.deltaTime;
            float k = collapseDelay > 0f ? t / collapseDelay : 1f;
            transform.position = startPos + (Vector3)(Random.insideUnitCircle * shakeAmount * k);
            if (sr != null) sr.color = Color.Lerp(startColor, warningColor, k);
            yield return null;
        }

        // 2) Fall away and fade out.
        col.enabled = false;
        t = 0f;
        while (t < fallTime)
        {
            t += Time.deltaTime;
            float k = fallTime > 0f ? t / fallTime : 1f;
            transform.position = startPos + Vector3.down * fallDistance * k * k;
            if (sr != null)
            {
                Color c = warningColor;
                c.a = 1f - k;
                sr.color = c;
            }
            yield return null;
        }
        if (sr != null) sr.enabled = false;

        if (!respawn) yield break;

        // 3) Rebuild after a short wait.
        yield return new WaitForSeconds(respawnDelay);
        transform.position = startPos;
        if (sr != null)
        {
            sr.enabled = true;
            sr.color = startColor;
        }
        col.enabled = true;
        collapsing = false;
    }
}
