using UnityEngine;

public class Icicle : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Rigidbody2D rb;
    public GameManager gameManager;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Handle player being hit by the icicle
            rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY; // Freeze the X position of the icicle
            // You can add additional logic here, such as reducing health or respawning the player
            rb.WakeUp(); // Wake up the Rigidbody2D to allow it to fall
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Handle icicle hitting the ground
            gameManager.GameOver(); // Call the GameOver method in the GameManager
            // You can add additional logic here, such as playing a sound effect or destroying the icicle after a delay
        }
        if (collision.gameObject.CompareTag("Environment") || collision.gameObject.CompareTag("Obstacles"))
        {
            // Handle icicle hitting the ground
            Destroy(gameObject); // Destroy the icicle object
            // You can add additional logic here, such as playing a sound effect or destroying the icicle after a delay
        }
    }
}
