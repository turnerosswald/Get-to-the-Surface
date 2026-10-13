using UnityEngine;

public class Pitfall : MonoBehaviour
{

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Handle player falling into the pitfall
            Debug.Log("Player has fallen into the pitfall!");
            Destroy(other.gameObject); // Destroy the player object (or you can implement a respawn mechanism)
            // You can add additional logic here, such as reducing health or respawning the player
        }
    }
}
