using UnityEngine;

public class MosquitoLanding : MonoBehaviour
{
    // === DRAG IN YOUR MOSQUITO GAMEOBJECT HERE ===
    public Transform mosquitoTransform;

    // === Target to land on (Assign in Inspector) ===
    public Transform landingTarget;

    [Header("Movement Settings")]
    public float flySpeed = 5f;
    public float landingSpeed = 1.5f;
    public float rotationSpeed = 10f;
    public float landingDistance = 1.0f; // Distance to start landing sequence
    public float attachDistance = 0.1f; // Distance to stop and attach

    private enum MosquitoState { Flying, Landing, Landed }
    private MosquitoState currentState = MosquitoState.Flying;

    private float landTime = 3f; // How long to stay landed
    private float currentLandTimer;

    // --- Start ---
    void Start()
    {
        if (mosquitoTransform == null)
        {
            Debug.LogError("Mosquito Transform is not assigned! Please drag the character in the Inspector.");
            // Disable the script if the required transform is missing.
            enabled = false;
        }
        else if (landingTarget == null)
        {
            Debug.LogWarning("Landing Target is not assigned. Mosquito will not move.");
            // Optional: Find a default target or set a random point.
        }
    }

    // --- Update Loop ---
    void Update()
    {
        if (mosquitoTransform == null || landingTarget == null) return;

        switch (currentState)
        {
            case MosquitoState.Flying:
                FlyToTarget();
                break;
            case MosquitoState.Landing:
                ExecuteLanding();
                break;
            case MosquitoState.Landed:
                MaintainLandedState();
                break;
        }
    }

    // --- Core Movement Function ---
    void FlyToTarget()
    {
        // 1. Calculate the Direction
        // The 'direction' vector points from the mosquito to the target.
        Vector3 direction = landingTarget.position - mosquitoTransform.position;
        float distance = direction.magnitude;

        // 2. Check for State Change
        if (distance <= landingDistance)
        {
            currentState = MosquitoState.Landing;
            return;
        }

        // 3. Normalize the direction (make it a unit vector)
        Vector3 normalizedDirection = direction.normalized;

        // 4. Apply movement using Transform.Translate
        // DeltaTime ensures frame rate independence.
        mosquitoTransform.Translate(normalizedDirection * flySpeed * Time.deltaTime, Space.World);

        // 5. Rotate to face the direction of travel (for visual realism)
        Quaternion lookRotation = Quaternion.LookRotation(normalizedDirection);
        mosquitoTransform.rotation = Quaternion.Slerp(mosquitoTransform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
    }

    // --- Landing Sequence Function ---
    void ExecuteLanding()
    {
        Vector3 direction = landingTarget.position - mosquitoTransform.position;
        float distance = direction.magnitude;

        // 1. Check for Attachment
        if (distance <= attachDistance)
        {
            currentState = MosquitoState.Landed;
            mosquitoTransform.position = landingTarget.position; // Snap to the exact point
            mosquitoTransform.SetParent(landingTarget); // OPTIONAL: Make it stick to the target
            currentLandTimer = landTime;
            return;
        }

        // 2. Slow Movement
        Vector3 normalizedDirection = direction.normalized;
        mosquitoTransform.Translate(normalizedDirection * landingSpeed * Time.deltaTime, Space.World);

        // 3. Rotation remains the same as Flying for a smooth transition
        Quaternion lookRotation = Quaternion.LookRotation(normalizedDirection);
        mosquitoTransform.rotation = Quaternion.Slerp(mosquitoTransform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
    }

    // --- Landed State Function ---
    void MaintainLandedState()
    {
        // For Landed state, the mosquito is locked in place.
        currentLandTimer -= Time.deltaTime;

        if (currentLandTimer <= 0)
        {
            // Flee or return to Flying state after timer expires
            Debug.Log("Mosquito has finished landing and will now fly away.");
            mosquitoTransform.SetParent(null); // Unparent from the target
            currentState = MosquitoState.Flying; // Or change to a Fleeing state
        }
    }
}