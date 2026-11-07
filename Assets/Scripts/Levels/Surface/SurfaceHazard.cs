using UnityEngine;

// Surface Level: anything that sends the player back to the last checkpoint
// (thorns, falling boulders, birds, the river).
public class SurfaceHazard : MonoBehaviour
{
    private static bool IsPlayer(Rigidbody2D rb) => rb != null && rb.GetComponent<Movement>() != null;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsPlayer(collision.rigidbody)) Hit();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other.attachedRigidbody)) Hit();
    }

    private void Hit()
    {
        if (SurfaceRespawn.Instance != null) SurfaceRespawn.Instance.Respawn();
    }
}
