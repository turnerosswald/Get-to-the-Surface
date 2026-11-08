using UnityEngine;
using System.Collections;

public class ShooterSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SpawnShooter();
    }

    IEnumerator SpawnShooter()
    {
        while (true)
        {
            Instantiate(Resources.Load("Shooter"), new Vector3(10, Random.Range(-4, 4), 0), Quaternion.identity);
            yield return new WaitForSeconds(2f);
        }
    }
}
