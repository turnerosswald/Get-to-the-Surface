using UnityEngine;

// Surface Level: sends the player back to the last checkpoint after a fall,
// counts falls, and shows the controls on screen.
public class SurfaceRespawn : MonoBehaviour
{
    public static SurfaceRespawn Instance { get; private set; }

    [SerializeField] private Rigidbody2D player;
    [SerializeField, Min(1f)] private float fallLimit = 8f;
    [SerializeField] private bool showControls = true;

    private Vector2 checkpoint;
    private int falls;

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
        // Only move the checkpoint upward.
        if (point.y > checkpoint.y) checkpoint = point;
    }

    private void Update()
    {
        if (player != null && player.position.y < checkpoint.y - fallLimit)
            Respawn();
    }

    public void Respawn()
    {
        falls++;
        player.linearVelocity = Vector2.zero;
        player.angularVelocity = 0f;
        player.position = checkpoint;
        player.transform.position = checkpoint;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(16, 12, 300, 24), $"Falls: {falls}");
        if (showControls)
        {
            GUI.Label(new Rect(16, 36, 640, 24),
                "Mouse = aim   1 = throw arms   2 = throw all four limbs   Space = throw again   Right-click = launch");
            GUI.Label(new Rect(16, 60, 640, 24),
                "The ground is crumbling. Climb to the surface!");
        }
    }
}
