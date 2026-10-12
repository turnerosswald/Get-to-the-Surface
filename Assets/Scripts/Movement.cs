using UnityEngine;
using UnityEngine.InputSystem;

// GTTS prototype controls:
// 1 = select + throw both arms
// 2 = select + throw all four limbs
// Space = throw the currently selected group again
// Mouse = aim throws (no mouse click needed)
// Right mouse button = magnetic launch toward thrown limbs, then recall them
// No walking / ordinary jumping.
[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    [Header("Drag the FRONT-view limb GameObjects here")]
    [SerializeField] private Transform leftArm;
    [SerializeField] private Transform rightArm;
    [SerializeField] private Transform leftLeg;
    [SerializeField] private Transform rightLeg;

    [Header("Movement tuning")]
    [SerializeField, Min(0f)] private float throwSpeed = 9f;
    [SerializeField, Min(0f)] private float magneticImpulsePerLimb = 3f;
    [SerializeField, Min(0.1f)] private float recallSpeed = 35f;
    [SerializeField, Min(0.01f)] private float reattachDistance = 0.06f;

    private enum ThrowGroup { Arms, AllFour }
    private enum LimbState { Attached, Thrown, Returning }

    private readonly Transform[] limbs = new Transform[4];
    private readonly Rigidbody2D[] limbBodies = new Rigidbody2D[4];
    private readonly Vector3[] homeLocalPositions = new Vector3[4];
    private readonly Quaternion[] homeLocalRotations = new Quaternion[4];
    private readonly LimbState[] states = new LimbState[4];

    private Rigidbody2D body;
    private ThrowGroup selectedGroup = ThrowGroup.Arms;

    
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.constraints = RigidbodyConstraints2D.FreezeRotation;

        Debug.Log(
            $"MOVEMENT STARTED ON: {gameObject.name}\n" +
            $"Scene: {gameObject.scene.name}\n" +
            $"Root: {transform.root.name}\n" +
            $"Left Arm: {(leftArm != null ? leftArm.name : "NULL")}\n" +
            $"Right Arm: {(rightArm != null ? rightArm.name : "NULL")}\n" +
            $"Left Leg: {(leftLeg != null ? leftLeg.name : "NULL")}\n" +
            $"Right Leg: {(rightLeg != null ? rightLeg.name : "NULL")}",
            this
        );

        limbs[0] = leftArm;
        limbs[1] = rightArm;
        limbs[2] = leftLeg;
        limbs[3] = rightLeg;

        for (int i = 0; i < limbs.Length; i++)
        {
            if (limbs[i] == null)
            {
                Debug.LogError(
                    $"Limb {i} is NOT assigned!\n" +
                    $"GameObject: {gameObject.name}\n" +
                    $"Scene: {gameObject.scene.name}\n" +
                    $"Entity ID: {GetEntityId()}",
                    this
                );

                enabled = false;
                return;
            }

            if (!limbs[i].TryGetComponent(out Rigidbody2D limbRb))
            {
                Debug.LogError(
                    $"Rigidbody2D missing on {limbs[i].name}!", this);
                enabled = false;
                return;
            }

            limbBodies[i] = limbRb;
            homeLocalPositions[i] = limbs[i].localPosition;
            homeLocalRotations[i] = limbs[i].localRotation;
            states[i] = LimbState.Attached;

            // Disable independent limb physics while attached
            limbRb.simulated = false;
        }
    }


    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            selectedGroup = ThrowGroup.Arms;
            ThrowSelectedGroup();
        }
        else if (keyboard.digit2Key.wasPressedThisFrame)
        {
            selectedGroup = ThrowGroup.AllFour;
            ThrowSelectedGroup();
        }
        else if (keyboard.spaceKey.wasPressedThisFrame)
        {
            ThrowSelectedGroup();
        }

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.rightButton.wasPressedThisFrame)
            MagneticRecall();
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < limbs.Length; i++)
        {
            if (states[i] != LimbState.Returning)
                continue;

            Vector3 target = transform.TransformPoint(homeLocalPositions[i]);
            limbs[i].position = Vector3.MoveTowards(
                limbs[i].position,
                target,
                recallSpeed * Time.fixedDeltaTime
            );

            if (Vector3.Distance(limbs[i].position, target) <= reattachDistance)
            {
                limbs[i].SetParent(transform, true);
                limbs[i].localPosition = homeLocalPositions[i];
                limbs[i].localRotation = homeLocalRotations[i];
                states[i] = LimbState.Attached;
            }
        }
    }

    private void ThrowSelectedGroup()
    {
        if (!TryGetAimDirection(out Vector2 direction))
            return;

        ThrowLimb(0, direction); // Left arm
        ThrowLimb(1, direction); // Right arm

        if (selectedGroup == ThrowGroup.AllFour)
        {
            ThrowLimb(2, direction); // Left leg
            ThrowLimb(3, direction); // Right leg
        }
    }

    private bool TryGetAimDirection(out Vector2 direction)
    {
        direction = Vector2.zero;
        Camera cam = Camera.main;
        Mouse mouse = Mouse.current;
        if (cam == null || mouse == null)
        {
            Debug.LogWarning("Need a MainCamera and mouse to aim a throw.", this);
            return false;
        }

        Vector2 screenPosition = mouse.position.ReadValue();
        float cameraDistance = Mathf.Abs(transform.position.z - cam.transform.position.z);
        Vector3 worldPosition = cam.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, cameraDistance)
        );

        direction = (Vector2)(worldPosition - transform.position);
        if (direction.sqrMagnitude < 0.0001f)
            return false;

        direction.Normalize();
        return true;
    }

    private void ThrowLimb(int i, Vector2 direction)
    {
        // A limb cannot be thrown twice until it has reattached.
        if (states[i] != LimbState.Attached)
            return;

        limbs[i].SetParent(null, true);

        Rigidbody2D limbRb = limbBodies[i];
        limbRb.simulated = true;
        limbRb.gravityScale = 0f; // Easy-to-test projectile arc; change later.
        limbRb.angularVelocity = 0f;
        limbRb.linearVelocity = body.linearVelocity + direction * throwSpeed;
        states[i] = LimbState.Thrown;
    }

    private void MagneticRecall()
    {
        Vector2 totalPull = Vector2.zero;

        for (int i = 0; i < limbs.Length; i++)
        {
            if (states[i] != LimbState.Thrown)
                continue;

            // Pull the main body TOWARD each thrown limb, not opposite to it.
            Vector2 offset = (Vector2)limbs[i].position - body.position;
            if (offset.sqrMagnitude > 0.01f)
                totalPull += offset.normalized;

            Rigidbody2D limbRb = limbBodies[i];
            limbRb.linearVelocity = Vector2.zero;
            limbRb.angularVelocity = 0f;
            limbRb.simulated = false;
            states[i] = LimbState.Returning;
        }

        // More limbs in the same direction = stronger magnetic launch.
        // One instant impulse, NOT continuous dragging.
        if (totalPull.sqrMagnitude > 0.0001f)
            body.AddForce(totalPull * magneticImpulsePerLimb, ForceMode2D.Impulse);
    }
}
