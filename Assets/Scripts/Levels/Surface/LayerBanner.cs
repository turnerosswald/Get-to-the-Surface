using UnityEngine;

// Surface Level: shows the layer name when the player climbs into it.
[RequireComponent(typeof(Collider2D))]
public class LayerBanner : MonoBehaviour
{
    [SerializeField] private string title = "Layer";
    [SerializeField] private string subtitle = "";
    private bool shown;

    public void Setup(string layerTitle, string layerSubtitle)
    {
        title = layerTitle; subtitle = layerSubtitle;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (shown || other.attachedRigidbody == null) return;
        if (other.attachedRigidbody.GetComponent<Movement>() == null) return;
        shown = true;
        if (SurfaceRespawn.Instance != null) SurfaceRespawn.Instance.ShowBanner(title, subtitle);
    }
}
