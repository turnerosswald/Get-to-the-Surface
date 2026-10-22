using UnityEngine;

// Surface Level, Layer 2 (The Peaks): drops boulders from the cliffs above.
public class FallingRockSpawner : MonoBehaviour
{
    [SerializeField] private Sprite rockSprite;
    [SerializeField] private float minX = -6f;
    [SerializeField] private float maxX = 6f;
    [SerializeField, Min(0.2f)] private float interval = 1.8f;
    [SerializeField] private float destroyBelowY = 10f;
    [SerializeField, Min(0.1f)] private float rockSize = 1.3f;

    private float timer;

    public void Setup(Sprite sprite, float left, float right, float every, float killY)
    {
        rockSprite = sprite; minX = left; maxX = right; interval = every; destroyBelowY = killY;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < interval) return;
        timer = 0f;
        SpawnRock();
    }

    private void SpawnRock()
    {
        var rock = new GameObject("Boulder");
        rock.transform.position = new Vector3(Random.Range(minX, maxX), transform.position.y, 0f);
        rock.transform.localScale = Vector3.one * rockSize * Random.Range(0.8f, 1.2f);

        var sr = rock.AddComponent<SpriteRenderer>();
        sr.sprite = rockSprite;
        sr.sortingOrder = 2;

        var rb = rock.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1.2f;
        rb.angularVelocity = Random.Range(-180f, 180f);

        rock.AddComponent<CircleCollider2D>().radius = 0.6f;
        rock.AddComponent<SurfaceHazard>();
        rock.AddComponent<DestroyBelow>().Setup(destroyBelowY, 8f);
    }
}
