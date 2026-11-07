using UnityEngine;

// Surface Level: sends the player back to the last checkpoint after a fall or a hit,
// counts falls, and draws the on-screen text (controls + layer banners).
public class SurfaceRespawn : MonoBehaviour
{
    public static SurfaceRespawn Instance { get; private set; }

    [SerializeField] private Rigidbody2D player;
    [SerializeField, Min(1f)] private float fallLimit = 8f;
    [SerializeField] private bool showControls = true;

    private Vector2 checkpoint;
    private int falls;
    private float lastRespawn = -10f;

    private string bannerTitle;
    private string bannerSubtitle;
    private float bannerUntil;

    public int Falls => falls;

    public void Setup(Rigidbody2D playerBody)
    {
        player = playerBody;
    }

    private void Awake()
    {
        Instance = this;
        if (player != null) checkpoint = player.position;
    }

    public void SetCheckpoint(Vector2 point)
    {
        if (point.y > checkpoint.y) checkpoint = point;
    }

    private void Update()
    {
        if (player != null && player.position.y < checkpoint.y - fallLimit)
            Respawn();
    }

    public void Respawn()
    {
        if (player == null || Time.time - lastRespawn < 0.3f) return;
        lastRespawn = Time.time;
        falls++;
        player.linearVelocity = Vector2.zero;
        player.angularVelocity = 0f;
        player.position = checkpoint;
        player.transform.position = checkpoint;
    }

    public void ShowBanner(string title, string subtitle)
    {
        bannerTitle = title;
        bannerSubtitle = subtitle;
        bannerUntil = Time.time + 3f;
    }

    private static void Outlined(Rect r, string text, GUIStyle style, Color color)
    {
        var old = style.normal.textColor;
        style.normal.textColor = new Color(0.1f, 0.05f, 0.15f, 0.9f);
        GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), text, style);
        style.normal.textColor = color;
        GUI.Label(r, text, style);
        style.normal.textColor = old;
    }

    private void OnGUI()
    {
        var small = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
        Outlined(new Rect(16, 12, 300, 26), $"Falls: {falls}", small, Color.white);
        if (showControls)
            Outlined(new Rect(16, 38, 900, 26),
                "Mouse = aim   1 = throw arms   2 = throw all four   Space = throw again   Right-click = launch", small, Color.white);

        if (Time.time < bannerUntil && !string.IsNullOrEmpty(bannerTitle))
        {
            var big = new GUIStyle(GUI.skin.label) { fontSize = 44, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            var sub = new GUIStyle(big) { fontSize = 22 };
            Outlined(new Rect(0, Screen.height * 0.18f, Screen.width, 60), bannerTitle, big, new Color(1f, 0.9f, 0.4f));
            Outlined(new Rect(0, Screen.height * 0.18f + 56, Screen.width, 34), bannerSubtitle, sub, Color.white);
        }
    }
}
