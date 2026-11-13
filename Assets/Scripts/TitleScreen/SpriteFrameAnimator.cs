using UnityEngine;

// Plays a list of sprites in a loop (used for the flowing lava in the crater).
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFrameAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField, Min(1f)] private float framesPerSecond = 12f;

    private SpriteRenderer sr;

    public void Setup(Sprite[] sprites, float fps)
    {
        frames = sprites;
        framesPerSecond = fps;
    }

    private void Awake() => sr = GetComponent<SpriteRenderer>();

    private void Update()
    {
        if (frames == null || frames.Length == 0) return;
        int i = (int)(Time.time * framesPerSecond) % frames.Length;
        sr.sprite = frames[i];
    }
}
