using UnityEngine;

public class MosquitoSpawner : MonoBehaviour
{
    // Drag your Mosquito Prefab here in the Inspector
    [Header("Mosquito")]
    public GameObject mosquitoPrefab;

    // Spawning frequency
    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    public float spawnRadius = 10f;

    void Start()
    { // ⬅️ ADDED OPENING BRACE HERE
        if (!mosquitoPrefab)
        {
            Debug.LogError("MosquitoSpawner: Mosquito Prefab is not assigned! Spawning disabled.");
            return;
        }

        // Start the repeating spawn function
        InvokeRepeating(nameof(SpawnMosquito), 1f, spawnInterval);
    } // ⬅️ THIS CLOSING BRACE WAS THE ONE THE COMPILER WAS LOOKING FOR

    void SpawnMosquito()
    {
        // 1. Calculate a random position in a sphere around the Spawner
        Vector3 randomDirection = Random.onUnitSphere;
        Vector3 spawnPosition = transform.position + randomDirection * spawnRadius;

        // 2. Instantiate the mosquito
        GameObject newMosquito = Instantiate(mosquitoPrefab, spawnPosition, Quaternion.identity);

        // 3. Initialize the mosquito.
        // We pass the Spawner's Transform as a placeholder. The Mosquito script 
        // will ignore this placeholder and find the actual character target in its own Start() method.
        Mosquito mosquitoScript = newMosquito.GetComponent<Mosquito>();
        if (mosquitoScript != null)
        {
            mosquitoScript.Initialize(transform);
        }
    }
}
