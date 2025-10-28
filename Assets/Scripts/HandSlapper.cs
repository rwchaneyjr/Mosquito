using UnityEngine;

// Empty marker: if this component exists, the mosquito was already scored.
public class AlreadyHit : MonoBehaviour { }

[DisallowMultipleComponent]
public class HandSlapper : MonoBehaviour
{
    public PlayerIKSlapper playerSlapper;   // drag your PlayerIKSlapper object here
    public AudioClip slapSound;
    public float triggerRadius = 0.3f;

    [HideInInspector] public bool isSlapping = false; // set true only while slapping
    private AudioSource audioSrc;

    // NEW: cap scoring to once per slap
    private bool scoredThisSwing = false;
    private bool wasSlappingLastFrame = false;

    void Start()
    {
        // Ensure trigger collider
        var col = GetComponent<SphereCollider>() ?? gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = triggerRadius;

        // Ensure kinematic rigidbody (required for trigger events)
        var rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // Ensure audio
        audioSrc = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // Rising edge of a slap → allow scoring again
        if (isSlapping && !wasSlappingLastFrame)
            scoredThisSwing = false;

        wasSlappingLastFrame = isSlapping;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isSlapping) return;                 // only during slap
        if (scoredThisSwing) return;             // cap: one score per swing
        if (!other.CompareTag("Mosquito")) return;

        // Use root so child colliders don't double-count
        Transform root = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform.root;

        // If this mosquito was already scored before, ignore
        if (root.GetComponent<AlreadyHit>()) return;

        // First time → mark it
        root.gameObject.AddComponent<AlreadyHit>();

        // Score once and lock for this swing
        if (playerSlapper != null) playerSlapper.AddScore(10);
        scoredThisSwing = true;

        if (slapSound && audioSrc) audioSrc.PlayOneShot(slapSound);

        // Remove the mosquito
        Destroy(root.gameObject, 0.05f);
    }
}
