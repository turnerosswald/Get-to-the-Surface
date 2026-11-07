using UnityEngine;

public class Pitfall : MonoBehaviour
{
    public GameManager gameManager;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Handle player falling into the pitfall
            Debug.Log("Player has fallen into the pitfall!");
           
            // You can add additional logic here, such as reducing health or respawning the player
            gameManager.GameOver(); // Call the GameOver method in the GameManager
        }
    }
}
