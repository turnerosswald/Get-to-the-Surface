using UnityEngine;

// Surface Level camera: follows the player upward through the layers.
// Tour mode (GTS > Play Surface Level Tour) pans from the river to the light on its own.
[RequireComponent(typeof(Camera))]
public class SurfaceCamera : MonoBehaviour
{
    public const string TourKey = "GTS_SurfaceTour";

    [SerializeField] private Transform target;
    [SerializeField, Min(0f)] private float followSpeed = 4f;
    [SerializeField] private float verticalOffset = 1.5f;
    [SerializeField] private float minY = 3f;
    [SerializeField] private float maxY = 56f;

    [Header("Tour")]
    [SerializeField, Min(1f)] private float tourSpeed = 2.2f;

    private static readonly (float y, string title, string subtitle)[] TourStops =
    {
        (3f,  "Layer 1 · The Riverbank", "Crumbling logs, thorn vines, the river below"),
        (18f, "Layer 2 · The Peaks",     "Snowy ledges under falling boulders"),
        (34f, "Layer 3 · The Clouds",    "Drifting clouds, wind gusts and birds"),
        (50f, "Layer 4 · The Light",     "Blinking crystals up to the golden beam"),
    };

    private bool touring;
    private float tourY;
    private float pauseUntil;
    private int nextStop;

    public void Setup(Transform followTarget, float bottom, float top)
    {
        target = followTarget;
        minY = bottom;
        maxY = top;
    }

    private void Start()
    {
        if (PlayerPrefs.GetInt(TourKey, 0) == 1)
        {
            PlayerPrefs.SetInt(TourKey, 0);
            touring = true;
            tourY = minY;
        }
    }

    private void LateUpdate()
    {
        Vector3 pos = transform.position;

        if (touring)
        {
            if (nextStop < TourStops.Length && tourY >= TourStops[nextStop].y)
            {
                var stop = TourStops[nextStop++];
                if (SurfaceRespawn.Instance != null) SurfaceRespawn.Instance.ShowBanner(stop.title, stop.subtitle);
                pauseUntil = Time.time + 3.5f;
            }
            if (Time.time >= pauseUntil && tourY < maxY) tourY += tourSpeed * Time.deltaTime;
            pos.y = tourY;
            transform.position = pos;
            return;
        }

        if (target == null) return;
        float goalY = Mathf.Clamp(target.position.y + verticalOffset, minY, maxY);
        pos.y = Mathf.Lerp(pos.y, goalY, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
        transform.position = pos;
    }
}
