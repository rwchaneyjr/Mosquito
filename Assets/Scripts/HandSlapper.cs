using UnityEngine;

[DisallowMultipleComponent]
public class HandSlapper : MonoBehaviour
{
    [Header("Score + Sound")]
    public PlayerIKSlapper playerSlapper; // Drag your player (with PlayerIKSlapper) here
    public AudioClip slapSound;

    [Header("Collider Settings")]
    [Tooltip("Trigger radius for the hand hit area.")]
    public float triggerRadius = 0.35f;

    [HideInInspector] public bool isSlapping = false;

    private AudioSource audioSrc;

    void Start()
    {
        // Ensure a trigger collider exists
        var col = GetComponent<SphereCollider>() ?? gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = triggerRadius;

        // IMPORTANT: moving trigger needs a kinematic Rigidbody for trigger callbacks
        var rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        audioSrc = gameObject.AddComponent<AudioSource>();

        Debug.Log($"✅ HandSlapper ready: trigger r={col.radius}, hasRB={rb != null}, kinematic={rb.isKinematic}");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🟢 Hand collider triggered with {other.name} | tag={other.tag} | isSlapping={isSlapping}");

        if (!isSlapping)
        {
            Debug.Log("🟡 Ignored — not currently slapping");
            return;
        }

        if (other.CompareTag("Mosquito"))
        {
            Debug.Log($"💥 SWAT DETECTED! playerSlapper={(playerSlapper != null ? "✅ assigned" : "❌ null")}");

            if (slapSound && audioSrc)
            {
                Debug.Log("🔊 Playing slap sound...");
                audioSrc.PlayOneShot(slapSound);
            }

            if (playerSlapper != null)
            {
                Debug.Log("🧮 Adding 10 points to score...");
                playerSlapper.AddScore(10);
            }
            else
            {
                Debug.LogWarning("⚠️ HandSlapper.playerSlapper is NULL! Score cannot increase.");
            }

            Debug.Log("💀 Destroying mosquito...");
            Destroy(other.gameObject);
        }
        else
        {
            Debug.Log($"🟣 Collided with non-mosquito object: {other.name}");
        }
    }

}
