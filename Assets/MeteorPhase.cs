using UnityEngine;
using System.Collections.Generic;

// Lets a fast meteor pass through the normal asteroid field the FIRST time it hits it.
// It stays "ghosted" for at least minPhaseTime, and keeps going until it is in open space,
// then it becomes solid again. Later asteroid hits are normal collisions.
// The ship and other meteors are never passed through.
[RequireComponent(typeof(Rigidbody2D))]
public class MeteorPhase : MonoBehaviour
{
    public float minPhaseTime = 1f;   // pass through asteroids for at least this long after the first hit
    public float maxPhaseTime = 3f;   // safety limit if it's still inside asteroids

    Rigidbody2D rb;
    Collider2D[] myCols;
    Vector2 lastVel;
    float lastAngVel;
    bool used, phasing;
    float phaseStart;

    readonly List<Collider2D> ignored = new List<Collider2D>();
    readonly List<Collider2D> overlapBuf = new List<Collider2D>();
    ContactFilter2D filter;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myCols = GetComponentsInChildren<Collider2D>();
        filter = new ContactFilter2D().NoFilter();
        filter.useTriggers = false;
    }

    void FixedUpdate()
    {
        // remember the velocity before this physics step, so a bump can be undone
        lastVel = rb.linearVelocity;
        lastAngVel = rb.angularVelocity;

        if (phasing)
        {
            float t = Time.time - phaseStart;
            if (t >= minPhaseTime && (!TouchingAnyAsteroid() || t >= maxPhaseTime))
                EndPhase();
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!IsAsteroid(col.collider)) return;   // ship / other meteors: normal hit
        if (used && !phasing) return;            // free pass already spent: normal hit

        if (!phasing)
        {
            phasing = true;
            used = true;
            phaseStart = Time.time;
        }

        // ignore this asteroid and undo the bounce
        foreach (var mine in myCols)
            Physics2D.IgnoreCollision(mine, col.collider, true);
        if (!ignored.Contains(col.collider)) ignored.Add(col.collider);

        rb.linearVelocity = lastVel;
        rb.angularVelocity = lastAngVel;
    }

    void EndPhase()
    {
        phasing = false;
        foreach (var c in ignored)
        {
            if (c == null) continue;
            if (Overlaps(c)) continue;   // still inside this one: keep ignoring it so it doesn't explode apart
            foreach (var mine in myCols)
                Physics2D.IgnoreCollision(mine, c, false);
        }
        ignored.Clear();
    }

    bool IsAsteroid(Collider2D c)
    {
        if (c == null) return false;
        return c.GetComponentInParent<AsteroidCleanup2D>() != null
            && c.GetComponentInParent<MeteorPhase>() == null;
    }

    bool TouchingAnyAsteroid()
    {
        foreach (var mine in myCols)
        {
            overlapBuf.Clear();
            mine.Overlap(filter, overlapBuf);
            foreach (var c in overlapBuf)
                if (IsAsteroid(c)) return true;
        }
        return false;
    }

    bool Overlaps(Collider2D other)
    {
        foreach (var mine in myCols)
            if (mine.Distance(other).isOverlapped) return true;
        return false;
    }
}
