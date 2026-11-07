using UnityEngine;

// Surface Level: the finish line. Reaching it ends the game.
[RequireComponent(typeof(Collider2D))]
public class SurfaceGoal : MonoBehaviour
{
    [SerializeField] private string message = "You reached the Surface!";
    private bool reached;
    private float finishTime;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (reached || other.attachedRigidbody == null) return;
        if (other.attachedRigidbody.GetComponent<Movement>() == null) return;

        reached = true;
        finishTime = Time.timeSinceLevelLoad;
    }

    private void OnGUI()
    {
        if (!reached) return;

        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 40,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        style.normal.textColor = Color.white;

        int falls = SurfaceRespawn.Instance != null ? SurfaceRespawn.Instance.Falls : 0;
        string text = $"{message}\nTime: {finishTime:0.0}s   Falls: {falls}";
        GUI.Label(new Rect(0, Screen.height * 0.3f, Screen.width, 140), text, style);
    }
}
