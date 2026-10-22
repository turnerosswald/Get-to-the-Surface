using UnityEngine;

// Surface Level camera: follows the player upward through the layers.
[RequireComponent(typeof(Camera))]
public class SurfaceCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField, Min(0f)] private float followSpeed = 4f;
    [SerializeField] private float verticalOffset = 1.5f;
    [SerializeField] private float minY = 3f;
    [SerializeField] private float maxY = 56f;

    public void Setup(Transform followTarget, float bottom, float top)
    {
        target = followTarget;
        minY = bottom;
        maxY = top;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        float goalY = Mathf.Clamp(target.position.y + verticalOffset, minY, maxY);
        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(pos.y, goalY, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
        transform.position = pos;
    }
}
