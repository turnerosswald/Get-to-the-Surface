using UnityEngine;

public class ShipShake : MonoBehaviour
{
    [Header("Shake")]
    public float duration = 0.3f;          // seconds
    public float magnitude = 0.15f;        // how far the sprite jitters (world units)
    public float rotationShake = 6f;       // degrees of random tilt

    [Header("Damage particles")]
    public int smokePuffs = 10;            // dark smoke per hit
    public int firePuffs = 6;              // orange combustion flashes per hit
    public int sparks = 12;                // tiny sparks per hit
    public float smokeLingerTime = 1.5f;   // keeps smoking for a bit after a hit
    public float lingerRate = 14f;         // smoke particles per second while lingering
    public float particleScale = 1f;       // make everything bigger or smaller

    SpriteRenderer sr;
    Transform visual;
    float timer;
    float lingerTimer;

    ParticleSystem smoke, fire, sparkFx;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null) { enabled = false; return; }

        // If the sprite sits on the same object as the Rigidbody2D, move it onto a child
        // so shaking the picture never disturbs physics or the camera follow.
        if (sr.transform == transform)
        {
            var go = new GameObject("ShipVisual");
            go.transform.SetParent(transform, false);

            var newSr = go.AddComponent<SpriteRenderer>();
            newSr.sprite = sr.sprite;
            newSr.color = sr.color;
            newSr.flipX = sr.flipX;
            newSr.flipY = sr.flipY;
            newSr.sharedMaterial = sr.sharedMaterial;
            newSr.sortingLayerID = sr.sortingLayerID;
            newSr.sortingOrder = sr.sortingOrder;

            sr.enabled = false;
            sr = newSr;
        }

        visual = sr.transform;
        BuildParticles();
    }

    public void Shake()
    {
        timer = duration;
        lingerTimer = smokeLingerTime;

        if (smoke != null)   smoke.Emit(smokePuffs);
        if (fire != null)    fire.Emit(firePuffs);
        if (sparkFx != null) sparkFx.Emit(sparks);
    }

    void LateUpdate()
    {
        if (visual == null) return;

        // shake the sprite
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);   // 1 -> 0, so the shake fades out

            visual.localPosition = (Vector3)(Random.insideUnitCircle * magnitude * t);
            visual.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-1f, 1f) * rotationShake * t);
        }
        else if (visual.localPosition != Vector3.zero)
        {
            visual.localPosition = Vector3.zero;
            visual.localRotation = Quaternion.identity;
        }

        // keep smoking for a moment after a hit
        if (smoke != null)
        {
            lingerTimer -= Time.deltaTime;
            var em = smoke.emission;
            em.rateOverTime = lingerTimer > 0f ? lingerRate : 0f;
        }
    }

    // ---------- particle building ----------

    void BuildParticles()
    {
        Shader sh = Shader.Find("Sprites/Default");
        if (sh == null) sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        Material mat = new Material(sh);

        float s = particleScale;

        // dark smoke: slow, big, fades out
        smoke = MakeSystem("DamageSmoke", mat, sr.sortingOrder + 2,
            0.8f, 1.5f,  0.4f, 1.2f,  0.25f * s, 0.5f * s, 0.25f * s,
            new Color(0.12f, 0.12f, 0.14f, 0.9f), new Color(0.38f, 0.38f, 0.42f, 0.9f),
            FadeGradient(Color.white, Color.white, 0.85f), 1.9f);

        // combustion: short, bright flashes that cool from yellow to dark red
        fire = MakeSystem("DamageFire", mat, sr.sortingOrder + 3,
            0.2f, 0.45f,  0.5f, 1.8f,  0.18f * s, 0.34f * s, 0.2f * s,
            new Color(1f, 0.85f, 0.25f, 1f), new Color(1f, 0.45f, 0.1f, 1f),
            FireGradient(), 0.4f);

        // sparks: tiny, fast, short lived
        sparkFx = MakeSystem("DamageSparks", mat, sr.sortingOrder + 4,
            0.2f, 0.5f,  3f, 6.5f,  0.05f * s, 0.1f * s, 0.12f * s,
            new Color(1f, 0.9f, 0.4f, 1f), new Color(1f, 0.55f, 0.15f, 1f),
            FadeGradient(Color.white, Color.white, 1f), 1f);
    }

    ParticleSystem MakeSystem(string name, Material mat, int order,
        float lifeMin, float lifeMax, float speedMin, float speedMax,
        float sizeMin, float sizeMax, float radius,
        Color colA, Color colB, Gradient overLife, float endSizeMultiplier)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);

        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;  // smoke stays behind as the ship moves
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
        main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = new ParticleSystem.MinMaxGradient(colA, colB);
        main.gravityModifier = 0f;
        main.maxParticles = 300;

        var em = ps.emission;
        em.rateOverTime = 0f;                       // only emits when we call Emit()

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = radius;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(overLife);

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f,
            AnimationCurve.Linear(0f, 1f, 1f, endSizeMultiplier));

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.sharedMaterial = mat;
        rend.sortingLayerID = sr.sortingLayerID;
        rend.sortingOrder = order;

        ps.Play();
        return ps;
    }

    Gradient FadeGradient(Color a, Color b, float startAlpha)
    {
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(a, 0f), new GradientColorKey(b, 1f) },
            new[] { new GradientAlphaKey(startAlpha, 0f), new GradientAlphaKey(0f, 1f) });
        return g;
    }

    Gradient FireGradient()
    {
        var g = new Gradient();
        g.SetKeys(
            new[] {
                new GradientColorKey(new Color(1f, 0.95f, 0.5f), 0f),   // hot yellow
                new GradientColorKey(new Color(1f, 0.5f, 0.1f), 0.4f),  // orange
                new GradientColorKey(new Color(0.45f, 0.12f, 0.08f), 1f) // dark red
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.9f, 0.6f), new GradientAlphaKey(0f, 1f) });
        return g;
    }
}