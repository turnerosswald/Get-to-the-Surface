using UnityEngine;

// Surface Level: a lantern on a safe platform. Touching it saves your progress.
[RequireComponent(typeof(Collider2D))]
public class SurfaceCheckpoint : MonoBehaviour
{
    [SerializeField] private Color activeColor = new Color(0.4f, 1f, 0.5f);
    private bool activated;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated || other.attachedRigidbody == null) return;
        if (other.attachedRigidbody.GetComponent<Movement>() == null) return;

        activated = true;
        if (SurfaceRespawn.Instance != null)
            SurfaceRespawn.Instance.SetCheckpoint(transform.position);

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = activeColor;
    }
}
