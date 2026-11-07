using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float scrollSpeed = 3f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    void LateUpdate()
    {
        transform.position += Vector3.up * scrollSpeed * Time.deltaTime;
    }
}
