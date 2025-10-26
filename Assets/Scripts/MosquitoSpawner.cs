using UnityEngine;

public class MosquitoSpawner : MonoBehaviour
{
    [Header("Mosquito")]
    public GameObject mosquitoPrefab;

    [Header("Target (drag your humanoid root)")]
    public Transform characterRoot;   // <-- set this to Worried_Mouse... in the scene

    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    public float spawnRadius = 3f;    // smaller so they appear near you

    void Start()
    {
        if (!mosquitoPrefab)
        {
            Debug.LogError("MosquitoSpawner: Mosquito Prefab is not assigned! Spawning disabled.");
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

        Vector3 spawnPosition = transform.position + dir * spawnRadius;

        GameObject go = Instantiate(mosquitoPrefab, spawnPosition, Quaternion.identity);

        // Hand off the character reference so Mosquito.Start() can find bones
        var m = go.GetComponent<Mosquito>();
        if (m != null && characterRoot != null)
        {
            m.characterRoot = characterRoot;
        }

        // Debug line so you know it’s spawning
        Debug.DrawLine(transform.position, spawnPosition, Color.yellow, 2f);
        Debug.Log($"Spawned mosquito @ {spawnPosition}");
    }
}
