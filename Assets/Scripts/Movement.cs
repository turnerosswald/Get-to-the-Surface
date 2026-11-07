using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// GTTS controls:
// 1 = throw both arms
// 2 = throw all four limbs
// R = repeat selected throw after recall finishes
// Mouse = aim
// Space = ONE velocity boost toward the limbs' CURRENT positions, then recall
// No walking or normal jumping.

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    [Header("Front-view limbs")]
    [SerializeField] private Transform leftArm;
    [SerializeField] private Transform rightArm;
    [SerializeField] private Transform leftLeg;
    [SerializeField] private Transform rightLeg;

    [Header("Player reference")]
    [Tooltip("Optional empty child at the rock body's center. Otherwise use the body's sprite bounds / Rigidbody2D.")]
    [SerializeField] private Transform playerCenterPoint;

    [Header("Throw")]
    [SerializeField, Min(0f)] private float throwSpeed = 6f;
    [SerializeField, Min(0f)] private float limbGravityScale = 1f;
    [Tooltip("Start each thrown limb just outside the body center so the attachment offsets do not bias aim.")]
    [SerializeField] private bool spawnFromBodyCenter = true;
    [SerializeField, Min(0f)] private float throwSpawnDistance = 0.25f;

    [Header("Magnetic propulsion")]
    [Tooltip("Added velocity per unit of AVERAGE limb distance from the body.")]
    [SerializeField, Min(0f)] private float magneticStrengthPerUnit = 6f;
    [Tooltip("Multiplier for four limbs compared with two arms.")]
    [SerializeField, Min(1f)] private float fourLimbMultiplier = 1.5f;
    [SerializeField, Min(0f)] private float maxAddedVelocity = 10f;
    [Tooltip("If the average limb location is too close, recall without a boost.")]
    [SerializeField, Min(0f)] private float minimumPullDistance = 0.3f;

    [Header("Recall")]
    [SerializeField, Min(0.1f)] private float recallSpeed = 35f;
    [SerializeField, Min(0.01f)] private float reattachDistance = 0.06f;

    [Header("Diagnostics")]
    [SerializeField] private bool showDebugDirections = true;

    private enum ThrowGroup { Arms, AllFour }
    private enum LimbState { Attached, Thrown, Returning }

    private readonly Transform[] limbs = new Transform[4];
    private readonly Rigidbody2D[] limbBodies = new Rigidbody2D[4];
    private readonly SpriteRenderer[] limbSprites = new SpriteRenderer[4];
    private readonly Transform[] homeParents = new Transform[4];
    private readonly Vector3[] homeLocalPositions = new Vector3[4];
    private readonly Quaternion[] homeLocalRotations = new Quaternion[4];
    private readonly Vector3[] homeLocalScales = new Vector3[4];
    private readonly LimbState[] states = new LimbState[4];
    private readonly Collider2D[][] limbColliders = new Collider2D[4][];

    private Rigidbody2D body;
    private SpriteRenderer[] bodySprites;
    private Collider2D[] bodyColliders;
    private ThrowGroup selectedGroup = ThrowGroup.Arms;
    private bool throwQueued;
    private bool recallQueued;
    private Vector2 queuedThrowDirection;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.freezeRotation = true;

        limbs[0] = leftArm;
        limbs[1] = rightArm;
        limbs[2] = leftLeg;
        limbs[3] = rightLeg;

        for (int i = 0; i < limbs.Length; i++)
        {
            if (limbs[i] == null || !limbs[i].TryGetComponent(out Rigidbody2D rb))
            {
                Debug.LogError($"Missing limb or Rigidbody2D in slot {i} on '{name}'.", this);
                enabled = false;
                return;
            }
            if (!limbs[i].IsChildOf(transform))
                Debug.LogWarning($"Limb '{limbs[i].name}' does not start under '{name}' in the hierarchy.", this);

            limbBodies[i] = rb;
            limbSprites[i] = limbs[i].GetComponentInChildren<SpriteRenderer>(true);
            homeParents[i] = limbs[i].parent;
            homeLocalPositions[i] = limbs[i].localPosition;
            homeLocalRotations[i] = limbs[i].localRotation;
            homeLocalScales[i] = limbs[i].localScale;
            states[i] = LimbState.Attached;
            rb.simulated = false;
        }

        CacheCollidersAndBodySprites();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            selectedGroup = ThrowGroup.Arms;
            QueueThrow();
        }
        else if (keyboard.digit2Key.wasPressedThisFrame)
        {
            selectedGroup = ThrowGroup.AllFour;
            QueueThrow();
        }
        else if (keyboard.rKey.wasPressedThisFrame)
        {
            QueueThrow();
        }

        // Don't launch in the same frame as the original throw.
        if (keyboard.spaceKey.wasPressedThisFrame && !throwQueued)
            recallQueued = true;
    }

    private void FixedUpdate()
    {
        if (throwQueued)
        {
            ThrowSelectedGroup(queuedThrowDirection);
            throwQueued = false;
        }

        if (recallQueued)
        {
            MagneticLaunchAndRecall();
            recallQueued = false;
        }

        UpdateReturningLimbs();
    }

    private void QueueThrow()
    {
        if (throwQueued) return;
        for (int i = 0; i < states.Length; i++)
            if (states[i] != LimbState.Attached) return;

        if (!TryGetAimDirection(out Vector2 direction)) return;
        queuedThrowDirection = direction;
        throwQueued = true;

        if (showDebugDirections)
            Debug.Log($"THROW AIM: direction={direction}, player={GetPlayerCenter()}, object='{name}'", this);
    }

    private bool TryGetAimDirection(out Vector2 direction)
    {
        direction = Vector2.zero;
        Camera cam = Camera.main;
        Mouse mouse = Mouse.current;
        if (cam == null || mouse == null)
        {
            Debug.LogWarning("GTTS needs a camera tagged MainCamera and a mouse.", this);
            return false;
        }

        Vector2 screen = mouse.position.ReadValue();
        Vector2 origin = GetPlayerCenter();
        float depth = Vector3.Dot((Vector3)origin - cam.transform.position, cam.transform.forward);
        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
        direction = (Vector2)mouseWorld - origin;
        if (direction.sqrMagnitude < 0.0001f) return false;
        direction.Normalize();
        return true;
    }

    private void ThrowSelectedGroup(Vector2 direction)
    {
        ThrowLimb(0, direction);
        ThrowLimb(1, direction);
        if (selectedGroup == ThrowGroup.AllFour)
        {
            ThrowLimb(2, direction);
            ThrowLimb(3, direction);
        }
    }

    private void ThrowLimb(int i, Vector2 direction)
    {
        if (states[i] != LimbState.Attached) return;

        // Work out visual sprite center *before* removing the parent.
        Vector2 pivotToVisualCenter = GetLimbCenter(i) - (Vector2)limbs[i].position;
        Vector2 playerCenter = GetPlayerCenter();
        limbs[i].SetParent(null, true);

        if (spawnFromBodyCenter)
        {
            Vector2 desiredVisualCenter = playerCenter + direction * throwSpawnDistance;
            Vector2 newPivot = desiredVisualCenter - pivotToVisualCenter;
            Vector3 oldPivot = limbs[i].position;
            limbs[i].position = new Vector3(newPivot.x, newPivot.y, oldPivot.z);
        }

        Rigidbody2D rb = limbBodies[i];
        rb.simulated = true;
        // Sync the Rigidbody2D to the newly chosen visual spawn position.
        // This avoids a stale physics position restoring the old socket offset.
        rb.position = limbs[i].position;
        rb.gravityScale = limbGravityScale;
        rb.angularVelocity = 0f;
        rb.linearVelocity = direction * throwSpeed; // No inherited player drift.
        states[i] = LimbState.Thrown;

        IgnoreLimbCollisions(i);
    }

    private void MagneticLaunchAndRecall()
    {
        // All points below are world-space positions at the moment Space is pressed.
        Vector2 playerCenter = GetPlayerCenter();
        Vector2 sumOfOffsets = Vector2.zero;
        int count = 0;

        for (int i = 0; i < limbs.Length; i++)
        {
            if (states[i] != LimbState.Thrown) continue;
            Vector2 limbCenter = GetLimbCenter(i);
            Vector2 offset = limbCenter - playerCenter; // FROM body TO limb.
            sumOfOffsets += offset;
            count++;

            if (showDebugDirections)
            {
                Debug.DrawLine(playerCenter, limbCenter, Color.yellow, 2f);
                Debug.Log($"PULL ARM '{limbs[i].name}': body={playerCenter}, limb={limbCenter}, offset={offset}", this);
            }
        }
        if (count == 0) return;

        // Using an average instead of a sum prevents 4 clustered limbs
        // from creating a huge boost merely because we counted 4 offsets.
        Vector2 averageOffset = sumOfOffsets / count;
        Vector2 addedVelocity = Vector2.zero;
        Vector2 velocityBefore = body.linearVelocity;

        if (averageOffset.magnitude >= minimumPullDistance)
        {
            float limbMultiplier = (count == 4) ? fourLimbMultiplier : 1f;
            addedVelocity = Vector2.ClampMagnitude(
                averageOffset * magneticStrengthPerUnit * limbMultiplier,
                maxAddedVelocity
            );

            // CRITICAL: add to current velocity, never replace it.
            body.linearVelocity = velocityBefore + addedVelocity;
        }

        if (showDebugDirections)
        {
            Debug.DrawRay(playerCenter, addedVelocity, Color.cyan, 2f);
            Debug.Log($"MAGNETIC LAUNCH: player={playerCenter}, averageOffset={averageOffset}, " +
                      $"count={count}, previousVelocity={velocityBefore}, addedVelocity={addedVelocity}, " +
                      $"resultVelocity={body.linearVelocity}", this);
        }

        // Start recall *after* sampling the current positions and adding velocity.
        for (int i = 0; i < limbs.Length; i++)
        {
            if (states[i] != LimbState.Thrown) continue;
            Rigidbody2D rb = limbBodies[i];
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
            states[i] = LimbState.Returning;
        }
    }

    private void UpdateReturningLimbs()
    {
        for (int i = 0; i < limbs.Length; i++)
        {
            if (states[i] != LimbState.Returning) continue;
            Transform parent = homeParents[i];
            Vector3 target = parent != null
                ? parent.TransformPoint(homeLocalPositions[i])
                : transform.TransformPoint(homeLocalPositions[i]);

            limbs[i].position = Vector3.MoveTowards(
                limbs[i].position, target, recallSpeed * Time.fixedDeltaTime);

            if (Vector3.Distance(limbs[i].position, target) <= reattachDistance)
            {
                limbs[i].SetParent(parent, false);
                limbs[i].localPosition = homeLocalPositions[i];
                limbs[i].localRotation = homeLocalRotations[i];
                limbs[i].localScale = homeLocalScales[i];
                states[i] = LimbState.Attached;
            }
        }
    }

    private Vector2 GetPlayerCenter()
    {
        if (playerCenterPoint != null) return playerCenterPoint.position;
        bool found = false;
        Bounds b = new Bounds();
        if (bodySprites != null)
        {
            foreach (SpriteRenderer sprite in bodySprites)
            {
                if (sprite == null || !sprite.enabled) continue;
                if (!found) { b = sprite.bounds; found = true; }
                else b.Encapsulate(sprite.bounds);
            }
        }
        return found ? (Vector2)b.center : body.position;
    }

    private Vector2 GetLimbCenter(int i)
    {
        SpriteRenderer sprite = limbSprites[i];
        return (sprite != null && sprite.enabled)
            ? (Vector2)sprite.bounds.center
            : (Vector2)limbs[i].position;
    }

    private bool IsLimbChild(Transform t)
    {
        foreach (Transform limb in limbs)
            if (t == limb || t.IsChildOf(limb)) return true;
        return false;
    }

    private void CacheCollidersAndBodySprites()
    {
        var sprites = new List<SpriteRenderer>();
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>(true))
            if (!IsLimbChild(sr.transform)) sprites.Add(sr);
        bodySprites = sprites.ToArray();

        var colliders = new List<Collider2D>();
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>(true))
            if (!IsLimbChild(col.transform)) colliders.Add(col);
        bodyColliders = colliders.ToArray();

        for (int i = 0; i < limbs.Length; i++)
            limbColliders[i] = limbs[i].GetComponentsInChildren<Collider2D>(true);
    }

    private void IgnoreLimbCollisions(int index)
    {
        foreach (Collider2D limbCollider in limbColliders[index])
        {
            if (limbCollider == null) continue;
            foreach (Collider2D bodyCollider in bodyColliders)
                if (bodyCollider != null)
                    Physics2D.IgnoreCollision(limbCollider, bodyCollider, true);
            for (int j = 0; j < limbs.Length; j++)
            {
                if (j == index) continue;
                foreach (Collider2D other in limbColliders[j])
                    if (other != null)
                        Physics2D.IgnoreCollision(limbCollider, other, true);
            }
        }
    }
}
