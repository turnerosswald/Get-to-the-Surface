using UnityEngine;

// Surface Level camera: follows the player upward and fades the background
// from cave darkness to daylight as the player gets closer to the surface.
[RequireComponent(typeof(Camera))]
public class SurfaceCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField, Min(0f)] private float followSpeed = 4f;
    [SerializeField] private float verticalOffset = 1.5f;
    [SerializeField] private float minY = 3f;

    [Header("Darkness to daylight")]
    [SerializeField] private float caveY = 0f;
    [SerializeField] private float surfaceY = 35f;
    [SerializeField] private Color caveColor = new Color(0.06f, 0.05f, 0.08f);
    [SerializeField] private Color skyColor = new Color(0.55f, 0.8f, 1f);

    private Camera cam;

    public void Setup(Transform followTarget, float bottom, float top)
    {
        target = followTarget;
        caveY = bottom;
        surfaceY = top;
    }

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float goalY = Mathf.Max(minY, target.position.y + verticalOffset);
        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(pos.y, goalY, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
        transform.position = pos;

        float k = Mathf.InverseLerp(caveY, surfaceY, target.position.y);
        cam.backgroundColor = Color.Lerp(caveColor, skyColor, k * k);
    }
}
