using UnityEngine;

// Comets fly in one at a time at a steady rhythm, cycling through five lanes of different sizes.
// They are drawn between the space background and the Earth, so they pass BEHIND the planet.
public class CometShower : MonoBehaviour
{
    [System.Serializable]
    public struct Lane
    {
        public Sprite sprite;
        public Vector2 startPx;
        public Vector2 endPx;
    }

    [SerializeField] private Lane[] lanes;
    [SerializeField, Min(0.1f)] private float every = 0.9f;
    [SerializeField, Min(0.1f)] private float flightTime = 3.6f;
    [SerializeField] private int sortingOrder = -50;

    private float nextLaunch;
    private int launched;

    public void Setup(Lane[] laneData, float interval, float flight)
    {
        lanes = laneData;
        every = interval;
        flightTime = flight;
    }

    private void Update()
    {
        if (lanes == null || lanes.Length == 0) return;
        while (Time.time >= nextLaunch)
        {
            Launch(lanes[launched % lanes.Length]);
            launched++;
            nextLaunch += every;
        }
    }

    private void Launch(Lane lane)
    {
        var go = new GameObject("Comet");
        go.transform.SetParent(transform, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = lane.sprite;
        sr.sortingOrder = sortingOrder;
        go.AddComponent<CometFlight>().Setup(lane.startPx, lane.endPx, flightTime);
    }
}
