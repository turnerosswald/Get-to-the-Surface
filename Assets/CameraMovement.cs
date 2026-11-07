using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Target")]
    public Transform ship;                 // drag the spaceship here

    [Header("Follow")]
    public float yOffset = 0f;             // positive = ship sits lower on screen, negative = higher
    public float smoothTime = 0.15f;       // 0 = locked to the ship, higher = softer, laggier follow
    public bool followX = false;           // true = camera also follows the ship left/right

    float velY;
    float velX;

    void LateUpdate()
    {
        if (ship == null) return;

        Vector3 pos = transform.position;

        float targetY = ship.position.y + yOffset;
        pos.y = smoothTime > 0f
            ? Mathf.SmoothDamp(pos.y, targetY, ref velY, smoothTime)
            : targetY;

        if (followX)
        {
            pos.x = smoothTime > 0f
                ? Mathf.SmoothDamp(pos.x, ship.position.x, ref velX, smoothTime)
                : ship.position.x;
        }

        transform.position = pos;
    }
}