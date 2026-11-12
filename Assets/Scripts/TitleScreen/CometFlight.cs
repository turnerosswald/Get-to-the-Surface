using UnityEngine;

// Moves one comet along its lane, then removes it.
public class CometFlight : MonoBehaviour
{
    private Vector2 start, end;
    private float duration, born;

    public void Setup(Vector2 startPx, Vector2 endPx, float seconds)
    {
        start = startPx; end = endPx; duration = seconds; born = Time.time;
        Vector2 dir = end - start;
        float angle = Mathf.Atan2(-dir.y, dir.x) * Mathf.Rad2Deg;   // pixel y points down
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        Update();
    }

    private void Update()
    {
        float u = (Time.time - born) / duration;
        if (u > 1f) { Destroy(gameObject); return; }
        float f = -0.15f + 1.3f * u;
        Vector2 p = Vector2.Lerp(start, end, f);
        transform.position = TitlePixels.ToWorld(p.x, p.y);
    }
}
