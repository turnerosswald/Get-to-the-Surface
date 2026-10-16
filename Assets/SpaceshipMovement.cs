using UnityEngine;
using UnityEngine.InputSystem;


public class SpaceshipMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private float move;
    public float upSpeed = 3f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        move = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            move -= 1f;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            move += 1f;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(move * moveSpeed, upSpeed);
    }
}
