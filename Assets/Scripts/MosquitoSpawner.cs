using UnityEngine;

public class MosquitoSpawner : MonoBehaviour
{
    [Header("Mosquito")]
    public GameObject mosquitoPrefab;

    [Header("Spawn Origin (Set to Main Camera)")]
    public Transform spawnOrigin;    // <-- NEW FIELD for camera position

    [Header("Movement Target (Set to Player/Mos)")]
    public Transform playerTarget;   // <-- NEW FIELD for player root

    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    public float spawnRadius = 4f;  // Set this for distance from camera

    void Start()
    {
        if (!mosquitoPrefab || !spawnOrigin || !playerTarget)
        {
            Debug.LogError("MosquitoSpawner: Prefab, Spawn Origin, or Player Target is not assigned!");
            enabled = false;
            return;
        }
        InvokeRepeating(nameof(SpawnMosquito), 1f, spawnInterval);
    }

    // MosquitoSpawner.cs (The SpawnMosquito function)

    void SpawnMosquito()
    {
        // PURE POSITIONING: Spawns in a random sphere around the origin point.
        // The visual direction (from camera to player) is controlled entirely
        // by where you place the Spawn Origin object relative to the player.
        Vector3 randomOffset = Random.onUnitSphere * spawnRadius;

        // Use the assigned spawnOrigin position (your Cube or the Camera) as the center
        Vector3 spawnPosition = spawnOrigin.position + randomOffset;

        GameObject go = Instantiate(mosquitoPrefab, spawnPosition, Quaternion.identity);

        // Hand off the actual PLAYER reference to the Mosquito script
        var m = go.GetComponent<Mosquito>();
        if (m != null)
        {
            m.characterRoot = playerTarget;
        }

        // Debug line to visually confirm spawn location
        Debug.DrawLine(spawnOrigin.position, spawnPosition, Color.yellow, 2f);
        Debug.Log($"Spawned mosquito @ {spawnPosition}");
    }
}