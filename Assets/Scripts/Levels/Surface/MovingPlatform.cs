using UnityEngine;

// Surface Level, Layer 3 (The Clouds): a cloud that drifts back and forth.
[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Vector2 travel = new Vector2(2f, 0f);
    [SerializeField, Min(0.05f)] private float speed = 1.2f;
    [SerializeField] private float phase;

    private Rigidbody2D rb;
    private Vector2 center;

    public void Setup(Vector2 travelDistance, float moveSpeed, float startPhase)
    {
        travel = travelDistance; speed = moveSpeed; phase = startPhase;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        center = rb.position;
    }

    private void FixedUpdate()
    {
        float s = Mathf.Sin(Time.time * speed + phase);
        rb.MovePosition(center + travel * s);
    }
}
