using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float drainTime = 30f;          // seconds to drain from full with no damage

    public float asteroidDamage = 15f;
    public float hitCooldown = 0.5f;       // stops one collision from damaging every frame

    public Slider healthBar;

    float health;
    float lastHitTime = -999f;
    bool dead;

    void Start()
    {
        health = maxHealth;
        if (healthBar != null)
        {
            healthBar.minValue = 0;
            healthBar.maxValue = maxHealth;
            healthBar.value = health;
        }
    }

    void Update()
    {
        if (dead) return;

        // constant drain
        health -= (maxHealth / drainTime) * Time.deltaTime;
        UpdateBar();

        if (health <= 0f) Die();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // asteroids get AsteroidCleanup2D added by the spawner, so we can detect them with that
        if (col.gameObject.GetComponent<AsteroidCleanup2D>() == null) return;
        if (Time.time - lastHitTime < hitCooldown) return;

        lastHitTime = Time.time;
        TakeDamage(asteroidDamage);
    }

    public void TakeDamage(float amount)
    {
        if (dead) return;
        health = Mathf.Max(0f, health - amount);
        UpdateBar();
        if (health <= 0f) Die();
    }

    public void Heal(float amount)
    {
        if (dead) return;
        health = Mathf.Min(maxHealth, health + amount);
        UpdateBar();
    }

    void UpdateBar()
    {
        if (healthBar != null) healthBar.value = health;
    }

    void Die()
    {
        dead = true;
        Debug.Log("Ship destroyed!");
        // restart the scene, show a game over screen, etc.
        // UnityEngine.SceneManagement.SceneManager.LoadScene(
        //     UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}