using UnityEngine;
using System.Collections;

public class MosquitoLanding : MonoBehaviour
{
    [Header("Target Character Setup")]
    public Transform characterRoot;

    [Header("Movement Settings")]
    public float buzzRadius = 0.8f;
    public float speed = 2f;
    public float initialApproachSpeed = 4f;

    [Header("Landing Settings")]
    public float landDuration = 3f;
    public float landCheckRadius = 0.5f;

    // Internal state
    private bool isLanded = false;
    private bool hasReachedBuzzZone = false;
    private Transform bodyTarget;
    private Vector3 targetOffset;
    private float currentSpeed;
    private Vector3 landedPosition;

    // Standard Humanoid bone names
    private readonly HumanBodyBones[] humanoidBones = new HumanBodyBones[]
 {
        HumanBodyBones.Head,
        HumanBodyBones.Neck,
        HumanBodyBones.Chest,
        HumanBodyBones.UpperChest,
        HumanBodyBones.Spine,
        HumanBodyBones.LeftUpperArm,
        HumanBodyBones.RightUpperArm,
        HumanBodyBones.LeftLowerArm,
        HumanBodyBones.RightLowerArm,
        HumanBodyBones.LeftUpperLeg,
        HumanBodyBones.RightUpperLeg,
        HumanBodyBones.LeftLowerLeg,
        HumanBodyBones.RightLowerLeg
 };

    // Mixamo bone names - Face, throat, chest, arms, legs
    private readonly string[] mixamoBones = new string[]
    {
        "mixamorig:Head",          // Face/head area
        "mixamorig:HeadTop_End",   // Top of head
        "mixamorig:Neck",          // Throat/neck
        "mixamorig:Spine2",        // Upper chest
        "mixamorig:Spine1",        // Mid chest
        "mixamorig:Spine",         // Lower chest
        "mixamorig:LeftArm",
        "mixamorig:RightArm",
        "mixamorig:LeftForeArm",
        "mixamorig:RightForeArm",
        "mixamorig:LeftUpLeg",
        "mixamorig:RightUpLeg",
        "mixamorig:LeftLeg",
        "mixamorig:RightLeg"
    };

    void Start()
    {
        Debug.Log("🦟 MOSQUITO SPAWNED");

        if (characterRoot == null)
        {
            Debug.LogError("❌ characterRoot is NULL!");
            Destroy(gameObject);
            return;
        }

        // Try Humanoid first (preferred method)
        Animator anim = characterRoot.GetComponent<Animator>();
        if (anim != null && anim.isHuman && anim.avatar != null && anim.avatar.isValid)
        {
            Debug.Log("✓ Using Humanoid Avatar");
            bodyTarget = GetRandomHumanoidBone(anim);
        }

        // Fallback: Search for Mixamo bones by name
        if (bodyTarget == null)
        {
            Debug.Log("⚠️ No Humanoid Avatar, searching for Mixamo bones...");
            bodyTarget = GetRandomMixamoBone(characterRoot);
        }

        // Last resort: Use character center
        if (bodyTarget == null)
        {
            Debug.LogWarning("⚠️ No bones found! Using character root position");
            // Create a dummy target at character's center
            GameObject dummyTarget = new GameObject("DummyTarget");
            dummyTarget.transform.SetParent(characterRoot);
            dummyTarget.transform.localPosition = Vector3.up * 1.5f; // Approximate head height
            bodyTarget = dummyTarget.transform;
        }

        Debug.Log($"✓ Targeting: {bodyTarget.name} at {bodyTarget.position}");

        // Setup collider
        SphereCollider col = GetComponent<SphereCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<SphereCollider>();
        }
        col.isTrigger = true;
        col.radius = 0.5f;

        // Ensure tag
        if (!gameObject.CompareTag("Mosquito"))
        {
            gameObject.tag = "Mosquito";
        }

        // Initial state
        currentSpeed = initialApproachSpeed;
        transform.localScale = Vector3.one * 0.3f;
        PickNewOffset();

        Debug.Log("=== MOSQUITO READY ===");
    }

    Transform GetRandomHumanoidBone(Animator anim)
    {
        HumanBodyBones randomBone = humanoidBones[Random.Range(0, humanoidBones.Length)];
        Transform bone = anim.GetBoneTransform(randomBone);

        if (bone != null)
        {
            Debug.Log($"✓ Found Humanoid bone: {randomBone}");
            return bone;
        }

        // Try other bones if first fails
        foreach (var boneType in humanoidBones)
        {
            bone = anim.GetBoneTransform(boneType);
            if (bone != null)
            {
                Debug.Log($"✓ Found Humanoid bone: {boneType}");
                return bone;
            }
        }

        return null;
    }

    Transform GetRandomMixamoBone(Transform root)
    {
        string randomBoneName = mixamoBones[Random.Range(0, mixamoBones.Length)];
        Transform bone = FindDeepChild(root, randomBoneName);

        if (bone != null)
        {
            Debug.Log($"✓ Found Mixamo bone: {randomBoneName}");
            return bone;
        }

        // Try all bones
        foreach (string boneName in mixamoBones)
        {
            bone = FindDeepChild(root, boneName);
            if (bone != null)
            {
                Debug.Log($"✓ Found Mixamo bone: {boneName}");
                return bone;
            }
        }

        return null;
    }

    // Recursively search for a child by name
    Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform result = FindDeepChild(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }

    void Update()
    {
        if (!bodyTarget || isLanded)
        {
            if (isLanded)
            {
                transform.position = landedPosition;
            }
            return;
        }

        Vector3 targetPosition = bodyTarget.position + targetOffset;
        float distanceToBone = Vector3.Distance(transform.position, bodyTarget.position);

        // Move towards target
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            currentSpeed * Time.deltaTime
        );

        // Rotate to face direction
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                Time.deltaTime * 5f
            );
        }

        // Check if reached buzz zone
        if (!hasReachedBuzzZone && distanceToBone < buzzRadius * 2f)
        {
            hasReachedBuzzZone = true;
            currentSpeed = speed;
            Debug.Log("✓ Entered buzz zone");
        }

        // Check if close enough to land
        if (hasReachedBuzzZone && distanceToBone < landCheckRadius && !isLanded)
        {
            float landChance = 0.3f;
            if (Random.value < landChance * Time.deltaTime)
            {
                LandOnTarget();
            }
        }

        // Pick new buzz position
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        if (hasReachedBuzzZone && distanceToTarget < 0.1f)
        {
            PickNewOffset();
        }
    }

    void LandOnTarget()
    {
        if (isLanded) return;

        Debug.Log($"🦟 LANDING ON: {bodyTarget.name}");

        isLanded = true;
        landedPosition = bodyTarget.position;
        transform.position = landedPosition;

        // Make bigger when landed
        transform.localScale = Vector3.one * 25.0f;  // Much bigger!
        StartCoroutine(FlyAwayAfterTime(landDuration));
    }

    void PickNewOffset()
    {
        targetOffset = new Vector3(
            Random.Range(-buzzRadius, buzzRadius),
            Random.Range(-buzzRadius, buzzRadius),
            Random.Range(-buzzRadius, buzzRadius)
        );
    }

    IEnumerator FlyAwayAfterTime(float time)
    {
        Debug.Log($"⏱️ Mosquito will fly away in {time}s");
        yield return new WaitForSeconds(time);

        if (!isLanded) yield break;

        Debug.Log("🦟 Flying away!");
        isLanded = false;
        hasReachedBuzzZone = false;

        targetOffset = Random.onUnitSphere * 5f;
        currentSpeed = initialApproachSpeed * 2f;
        transform.localScale = Vector3.one * 0.3f;

        yield return new WaitForSeconds(3f);

        Debug.Log("💀 Destroying escaped mosquito");
        Destroy(gameObject);
    }

    public bool IsLanded() => isLanded;

    void OnDestroy()
    {
        Debug.Log($"💀 Mosquito destroyed. Was landed: {isLanded}");
    }

    void OnDrawGizmos()
    {
        if (bodyTarget != null)
        {
            Gizmos.color = isLanded ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(bodyTarget.position, buzzRadius);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(bodyTarget.position, landCheckRadius);
        }
    }
}