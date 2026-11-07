using UnityEngine;

// Surface Level, Layer 3: drifting white streaks that show which way the wind blows.
public class WindStreaks : MonoBehaviour
{
    [SerializeField] private Sprite streak;
    [SerializeField] private Vector2 area = new Vector2(16f, 4f);
    [SerializeField] private float speed = 8f;
    [SerializeField] private int count = 14;

    private Transform[] streaks;

    public void Setup(Sprite sprite, Vector2 size, float windSpeed)
    {
        streak = sprite; area = size; speed = windSpeed;
    }

    private void Start()
    {
        streaks = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("Streak");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(Random.Range(-area.x / 2, area.x / 2), Random.Range(-area.y / 2, area.y / 2), 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = streak;
            sr.sortingOrder = 3;
            sr.flipX = speed < 0;
            streaks[i] = go.transform;
        }
    }

    private void Update()
    {
        foreach (var s in streaks)
        {
            Vector3 p = s.localPosition;
            p.x += speed * Time.deltaTime;
            if (p.x > area.x / 2) p.x = -area.x / 2;
            if (p.x < -area.x / 2) p.x = area.x / 2;
            s.localPosition = p;
        }
    }
}
