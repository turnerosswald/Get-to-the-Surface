using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class AccelMovement : MonoBehaviour
{

    public Rigidbody2D rb;
    public float acceleration = 100f;
    public float jumpHeight = 5f;
    private float gravity = -9.81f;
    public Vector2 movement;
    public float maxSpeed = 20f;
    public float speedDisplay;
    private CharacterController controller;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private bool isGrounded = true;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
        Debug.Log("Movement: " + movement);

    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpHeight, ForceMode2D.Impulse);
            isGrounded = false;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.AddForce(movement * acceleration * Time.fixedDeltaTime, ForceMode2D.Impulse);
        if (rb.linearVelocity.x > maxSpeed)
        {
             rb.linearVelocity = new Vector2(maxSpeed, rb.linearVelocity.y);
        }
        else if (rb.linearVelocity.x < -maxSpeed)
        {
            rb.linearVelocity = new Vector2(-maxSpeed, rb.linearVelocity.y);
        }
        speedDisplay = rb.linearVelocity.x;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("DeathCond"))
        {
            Destroy(gameObject);
        }
    }
}
