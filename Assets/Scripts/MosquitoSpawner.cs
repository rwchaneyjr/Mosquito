using UnityEngine;
using System.Reflection;

public class MosquitoSpawner : MonoBehaviour
{
    [Header("Mosquito")]
    public GameObject mosquitoPrefab;

    [Header("Spawn Origin (Camera or spawn point)")]
    public Transform spawnOrigin;

    [Header("Movement Target (Player Root)")]
    public Transform playerTarget;

    [Header("Spawn Settings")]
    public float spawnInterval = 3f;
    public float spawnRadius = 4f;
    public int maxMosquitoes = 5;

    private int currentMosquitoCount = 0;

    void Start()
    {
        if (!mosquitoPrefab)
        {
            Debug.LogError("❌ MosquitoSpawner: Mosquito Prefab not assigned!");
            enabled = false;
            return;
        }

        if (!spawnOrigin)
        {
            Debug.LogError("❌ MosquitoSpawner: Spawn Origin not assigned!");
            enabled = false;
            return;
        }

        if (!playerTarget)
        {
            Debug.LogError("❌ MosquitoSpawner: Player Target not assigned!");
            enabled = false;
            return;
        }

        Debug.Log($"✓ MosquitoSpawner ready. Will spawn around {spawnOrigin.name}, targeting {playerTarget.name}");

        InvokeRepeating(nameof(SpawnMosquito), 1f, spawnInterval);
    }

    void SpawnMosquito()
    {
        if (currentMosquitoCount >= maxMosquitoes)
        {
            Debug.Log($"⚠️ Max mosquitoes reached ({maxMosquitoes})");
            return;
        }

        // Spawn in random position around spawn origin
        Vector3 randomOffset = Random.onUnitSphere * spawnRadius;
        Vector3 spawnPosition = spawnOrigin.position + randomOffset;

        GameObject go = Instantiate(mosquitoPrefab, spawnPosition, Quaternion.identity);
        currentMosquitoCount++;

        Debug.Log($"🦟 Spawned mosquito #{currentMosquitoCount} at {spawnPosition}");

        // Try to assign the player target - works with any mosquito script
        Component mosquito = go.GetComponent("MosquitoLanding");

        if (mosquito == null)
        {
            mosquito = go.GetComponent("Mosquito");
        }

        if (mosquito != null)
        {
            // Try multiple possible field names
            bool success = TrySetField(mosquito, "characterRoot", playerTarget) ||
                          TrySetField(mosquito, "playerTarget", playerTarget) ||
                          TrySetField(mosquito, "target", playerTarget);

            if (success)
            {
                Debug.Log($"✓ Assigned target to mosquito: {playerTarget.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ Could not assign target - check mosquito script has public Transform characterRoot");
            }
        }
        else
        {
            Debug.LogError("❌ Spawned prefab has no Mosquito script!");
            Destroy(go);
            currentMosquitoCount--;
            return;
        }

        // Track when mosquito is destroyed
        StartCoroutine(TrackMosquito(go));

        // Debug line
        Debug.DrawLine(spawnOrigin.position, spawnPosition, Color.yellow, 2f);
    }

    bool TrySetField(Component component, string fieldName, object value)
    {
        System.Type type = component.GetType();
        FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);

        if (field != null)
        {
            field.SetValue(component, value);
            Debug.Log($"✓ Set field '{fieldName}' via reflection");
            return true;
        }
        return false;
    }

    System.Collections.IEnumerator TrackMosquito(GameObject mosquito)
    {
        yield return new WaitUntil(() => mosquito == null);
        currentMosquitoCount--;
        Debug.Log($"🦟 Mosquito destroyed. Count: {currentMosquitoCount}");
    }
}