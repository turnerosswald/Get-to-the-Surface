using UnityEngine;

// Scatters little stars that slowly twinkle.
public class TwinkleStars : MonoBehaviour
{
    [SerializeField] private Sprite pixel;
    [SerializeField] private int count = 60;
    [SerializeField] private int sortingOrder = -90;

    private SpriteRenderer[] stars;
    private float[] phase;

    public void Setup(Sprite pixelSprite) => pixel = pixelSprite;

    private void Start()
    {
        stars = new SpriteRenderer[count];
        phase = new float[count];
        var rng = new System.Random(2);
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("Star");
            go.transform.SetParent(transform, false);
            go.transform.position = TitlePixels.ToWorld(rng.Next(0, 480), rng.Next(0, 270));
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = pixel;
            sr.sortingOrder = sortingOrder;
            stars[i] = sr;
            phase[i] = i * 1.7f;
        }
    }

    private void Update()
    {
        for (int i = 0; i < stars.Length; i++)
        {
            float s = 0.5f + 0.5f * Mathf.Sin(Time.time * 3f + phase[i]);
            float v = (60f + 195f * s) / 255f;
            stars[i].color = new Color(v, v, Mathf.Min(1f, v + 0.08f));
        }
    }
}
