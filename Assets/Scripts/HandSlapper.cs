using UnityEngine;

// Put this on the hand sphere (RightHandTarget)
public class HandSlapper : MonoBehaviour
{
    public PlayerIKSlapper playerScript;  // Drag your character here in Inspector

    void Start()
    {
        Debug.Log("========================================");
        Debug.Log("HANDSLAPPER STARTING");
        Debug.Log($"GameObject name: {gameObject.name}");

        // Make sure we have a collider
        SphereCollider collider = GetComponent<SphereCollider>();
        if (collider == null)
        {
            Debug.Log("No collider found, adding one...");
            collider = gameObject.AddComponent<SphereCollider>();
        }
        collider.isTrigger = true;  // MUST be checked!
        collider.radius = 3.0f;     // HUGE sphere = can't miss!

        Debug.Log($"Collider setup: radius={collider.radius}, isTrigger={collider.isTrigger}");

        // Check if playerScript is assigned
        if (playerScript == null)
        {
            Debug.LogError("❌❌❌ PLAYER SCRIPT IS NULL! You must drag your character into the 'Player Script' field!");
        }
        else
        {
            Debug.Log($"✓ Player Script assigned: {playerScript.name}");
        }

        Debug.Log("========================================");
    }

    void Update()
    {
        // Log every 2 seconds to confirm script is running
        if (Time.frameCount % 120 == 0)
        {
            Debug.Log($"HandSlapper UPDATE - sphere at: {transform.position}");
        }
    }

    // This runs when hand touches something
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("========================================");
        Debug.Log("🔴🔴🔴 COLLISION DETECTED!");
        Debug.Log($"Hit object: {other.gameObject.name}");
        Debug.Log($"Object tag: {other.tag}");

        // Is it a mosquito?
        if (other.CompareTag("Mosquito"))
        {
            Debug.Log("✓ Object is tagged 'Mosquito'");

            // Get the mosquito script
            MosquitoLanding mosquito = other.GetComponent<MosquitoLanding>();

            if (mosquito == null)
            {
                Debug.LogError("❌ Object has Mosquito tag but NO MosquitoLanding script!");
                Debug.Log("========================================");
                return;
            }

            Debug.Log("✓ MosquitoLanding script found");

            // Is it landed? (can only kill landed mosquitoes)
            bool isLanded = mosquito.IsLanded();
            Debug.Log($"IsLanded() returned: {isLanded}");

            if (isLanded)
            {
                Debug.Log("💥💥💥 MOSQUITO IS LANDED - KILLING IT!");

                // Add score
                if (playerScript != null)
                {
                    Debug.Log("Calling AddScore(10)...");
                    playerScript.AddScore(10);
                    Debug.Log("✓ AddScore(10) completed!");
                }
                else
                {
                    Debug.LogError("❌ playerScript is NULL - cannot add score!");
                }

                // Delete mosquito
                Debug.Log("Destroying mosquito GameObject...");
                Destroy(other.gameObject);
                Debug.Log("✓ Mosquito destroyed!");

                Debug.Log("Mosquito killed! +10 points");
            }
            else
            {
                Debug.Log("❌ Mosquito is FLYING - cannot kill it");
            }
        }
        else
        {
            Debug.Log($"⚠️ Not a mosquito. Tag is: '{other.tag}'");
        }

        Debug.Log("========================================");
    }

    // Draw the collider in Scene view so you can see it
    void OnDrawGizmos()
    {
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, col.radius);
        }
    }
}
























