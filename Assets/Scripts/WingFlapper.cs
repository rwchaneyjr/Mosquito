using UnityEngine;

/// Flaps two wings around a hinge axis using a sine wave.
/// Assign leftWing and rightWing in the Inspector to the wing root transforms (their pivots should be at the hinge).
[DisallowMultipleComponent]
public class WingFlapper : MonoBehaviour
{
    [Header("Wing Transforms (roots at hinge)")]
    public Transform leftWing;
    public Transform rightWing;

    [Header("Hinge & Motion")]
    [Tooltip("Local axis on each wing that acts like the hinge (e.g., Vector3.forward).")]
    public Vector3 localHingeAxis = Vector3.forward;
    [Tooltip("Peak rotation from rest (degrees). 25–45 looks good.")]
    public float amplitudeDeg = 35f;
    [Tooltip("Flaps per second. 20–60 sells a tiny, frantic insect.")]
    public float flapHz = 40f;
    [Tooltip("Optional extra phase offset between left/right (degrees). 180 gives opposite motion.")]
    public float leftRightPhaseDeg = 180f;

    [Header("Behavior")]
    [Tooltip("Randomizes the starting phase so a swarm doesn't flap in perfect sync.")]
    public bool randomizeStartPhase = true;
    [Tooltip("Multiply flap speed based on movement speed (0 = off).")]
    public float speedToHzFactor = 0f;

    // --- internals ---
    private Quaternion _leftRestLocal;
    private Quaternion _rightRestLocal;
    private float _basePhaseRad;

    void Awake()
    {
        if (leftWing) _leftRestLocal = leftWing.localRotation;
        if (rightWing) _rightRestLocal = rightWing.localRotation;
        _basePhaseRad = randomizeStartPhase ? Random.value * Mathf.PI * 2f : 0f;

        // Normalize hinge axis just in case
        if (localHingeAxis.sqrMagnitude < 0.0001f) localHingeAxis = Vector3.forward;
        localHingeAxis.Normalize();
    }

    void LateUpdate()
    {
        if (!leftWing && !rightWing) return;

        // Optional speed → Hz boost (if you have a movement script, read its velocity magnitude)
        float effectiveHz = flapHz;
        if (speedToHzFactor > 0f)
        {
            // Simple example: sample parent velocity if available
            var rb = GetComponentInParent<Rigidbody>();
            if (rb) effectiveHz += rb.velocity.magnitude * speedToHzFactor;
        }

        float t = Time.time;
        float leftAngle = Mathf.Sin(_basePhaseRad + t * effectiveHz * Mathf.PI * 2f) * amplitudeDeg;
        float rightAngle = Mathf.Sin(_basePhaseRad + Deg2Rad(leftRightPhaseDeg) + t * effectiveHz * Mathf.PI * 2f) * amplitudeDeg;

        // Apply around each wing's local hinge axis
        if (leftWing)
            leftWing.localRotation = _leftRestLocal * Quaternion.AngleAxis(leftAngle, localHingeAxis);

        if (rightWing)
            rightWing.localRotation = _rightRestLocal * Quaternion.AngleAxis(rightAngle, localHingeAxis);
    }

    public void SetFlapActive(bool active)
    {
        enabled = active;
        if (!active)
        {
            if (leftWing) leftWing.localRotation = _leftRestLocal;
            if (rightWing) rightWing.localRotation = _rightRestLocal;
        }
    }

    private static float Deg2Rad(float d) => d * Mathf.Deg2Rad;
}
