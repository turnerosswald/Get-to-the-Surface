using UnityEngine;

// Removes falling objects once they leave the play area.
public class DestroyBelow : MonoBehaviour
{
    private float limitY;
    private float lifetime;

    public void Setup(float y, float seconds) { limitY = y; lifetime = seconds; }

    private void Update()
    {
        lifetime -= Time.deltaTime;
        if (transform.position.y < limitY || lifetime <= 0f) Destroy(gameObject);
    }
}
