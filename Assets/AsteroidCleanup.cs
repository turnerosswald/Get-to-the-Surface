using UnityEngine;

public class AsteroidCleanup2D : MonoBehaviour
{
    public Camera cam;
    public float despawnRadius;
    public AsteroidSpawner2D spawner;

    void Update()
    {
        if (cam == null) return;
        if (Vector2.Distance(transform.position, cam.transform.position) > despawnRadius)
            Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (spawner != null) spawner.OnAsteroidDestroyed();
    }
}