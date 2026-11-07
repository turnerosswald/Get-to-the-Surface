
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("Scrolling Settings")]
    [SerializeField, Min(0f)] private float scrollSpeed = 2f;
    [SerializeField] private bool isScrolling = false;

    private void Update()
    {
        if (!isScrolling)
            return;

        // Move the background downward at a constant speed.
        transform.position += Vector3.down * scrollSpeed * Time.deltaTime;
    }

    // Called when the UI button is pressed.
    public void ToggleScroll()
    {
        isScrolling = !isScrolling;
    }

    public void StartScroll()
    {
        isScrolling = true;
    }

    public void StopScroll()
    {
        isScrolling = false;
    }
}
