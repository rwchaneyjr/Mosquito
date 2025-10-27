using UnityEngine;

public class HandSlapper : MonoBehaviour
{
    public PlayerIKSlapper playerSlapper; // Reference to main player script for score
    public AudioClip slapSound;
    private AudioSource audioSrc;

    void Start()
    {
        // Make sure this sphere is a trigger
        SphereCollider col = GetComponent<SphereCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<SphereCollider>();
        }
        col.isTrigger = true;
        col.radius = 0.3f; // Adjust size of slap zone
        
        Debug.Log($"✓ HandSlapper collider: radius={col.radius}, isTrigger={col.isTrigger}");

        // Setup audio
        audioSrc = gameObject.AddComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"👋 Hand hit something: {other.name} (Tag: {other.tag})");

        if (other.CompareTag("Mosquito"))
        {
            MosquitoLanding mosq = other.GetComponent<MosquitoLanding>();

            if (mosq != null && mosq.IsLanded())
            {
                Debug.Log("💥 SWATTED LANDED MOSQUITO!");
                
                // Play sound
                if (slapSound != null && audioSrc != null)
                {
                    audioSrc.PlayOneShot(slapSound);
                }

                // Add score
                if (playerSlapper != null)
                {
                    playerSlapper.AddScore(10);
                }

                // Destroy mosquito
                Destroy(other.gameObject);
            }
            else if (mosq != null && !mosq.IsLanded())
            {
                Debug.Log("❌ Missed - mosquito is flying!");
            }
            else
            {
                Debug.LogWarning("⚠️ Mosquito has no MosquitoLanding component!");
            }
        }
    }
}
