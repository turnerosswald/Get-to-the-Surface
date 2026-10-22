using UnityEngine;

// Surface Level, Layer 3 (The Clouds): a bird that flies across its lane.
public class SurfaceBird : MonoBehaviour
{
    [SerializeField] private float leftX = -8f;
    [SerializeField] private float rightX = 8f;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float bob = 0.3f;

    private float baseY;
    private int dir = 1;
    private SpriteRenderer sr;

    public void Setup(float left, float right, float flySpeed, int startDir)
    {
        leftX = left; rightX = right; speed = flySpeed; dir = startDir;
    }

    private void Awake()
    {
        baseY = transform.position.y;
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Vector3 p = transform.position;
        p.x += dir * speed * Time.deltaTime;
        if (p.x > rightX) dir = -1;
        if (p.x < leftX) dir = 1;
        p.y = baseY + Mathf.Sin(Time.time * 4f) * bob;
        transform.position = p;
        if (sr != null) sr.flipX = dir < 0;
    }
}
