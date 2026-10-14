using UnityEngine;
using UnityEngine.UI;

public class WarmthScript : MonoBehaviour
{

    public float hp = 15f;
    public Slider warmthSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        hp -= Time.deltaTime;
        warmthSlider.value = hp;
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Warmth: " + hp);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("CheckPoint"))
        {
            hp = 15f;
        }
    }
}
