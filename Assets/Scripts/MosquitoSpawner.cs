using UnityEngine;

public class MosquitoSpawner : MonoBehaviour
{
    [Header("Mosquito")]
    public GameObject mosquitoPrefab;

    [Header("Target (drag your humanoid root or spawn point)")]
    public Transform characterRoot;    // <-- set this in the Inspector

    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    public float spawnRadius = 0.5f;   // Lowered default for closer spawns

    void Start()
    {
        if (!mosquitoPrefab)
        {
            Debug.LogError("MosquitoSpawner: Mosquito Prefab is not assigned! Spawning disabled.");
            return;
        }
        if (characterRoot == null)
        {
            Debug.LogError("MosquitoSpawner: Character Root is not assigned! Cannot determine spawn point.");
            return;
        }
        InvokeRepeating(nameof(SpawnMosquito), 1f, spawnInterval);
    }

    void SpawnMosquito()
    {
        // Bias spawns to be in front & slightly above (more visible than full sphere)
        Vector3 dir = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(0.0f, 1f),
            Random.Range(0.3f, 1f)
        ).normalized;

        // Use the assigned characterRoot position for spawning
        Vector3 spawnPosition = characterRoot.position + dir * spawnRadius;

        GameObject go = Instantiate(mosquitoPrefab, spawnPosition, Quaternion.identity);

        // Hand off the character reference so Mosquito.Start() can find bones
        // We SKIP passing the reference if the user has assigned the Main Camera
        // to prevent the mosquito from flying to an invalid bone target.
        if (characterRoot != null && !characterRoot.CompareTag("MainCamera"))
        {
            var m = go.GetComponent<Mosquito>();
            if (m != null)
            {
                m.characterRoot = characterRoot;
            }
        }

        // Debug line to visually confirm spawn location
        Debug.DrawLine(characterRoot.position, spawnPosition, Color.yellow, 2f);
        Debug.Log($"Spawned mosquito @ {spawnPosition}");
    }
}
