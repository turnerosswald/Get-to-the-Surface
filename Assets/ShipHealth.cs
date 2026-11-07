using UnityEngine;
using UnityEngine.UI;

public class ShipHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float drainTime = 30f;          // seconds to drain from full with no damage

    [Header("Asteroid damage")]
    public float asteroidDamage = 15f;
    public float hitCooldown = 0.5f;       // stops one collision from damaging every frame

    [Header("UI")]
    public Image fillImage;                // the liquid image (Image Type: Filled, Horizontal, Left)
    public int pixelSteps = 0;             // 0 = smooth drain. Set to 376 for a pixel-by-pixel drain

    float health;
    float lastHitTime = -999f;
    bool dead;

    ShipShake shake;                       // NEW (1): goes with the other fields, above Start()

    void Start()
    {
        health = maxHealth;
        shake = GetComponent<ShipShake>(); // NEW (2): inside Start()
        UpdateBar();
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
        if (shake != null) shake.Shake();  // NEW (3): inside TakeDamage(), after health goes down
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
        if (fillImage == null) return;

        float t = Mathf.Clamp01(health / maxHealth);
        if (pixelSteps > 0)
            t = Mathf.Ceil(t * pixelSteps) / pixelSteps;   // snap to whole pixels

        fillImage.fillAmount = t;
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