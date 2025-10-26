using UnityEngine;
using System.Collections;

public class Mosquito : MonoBehaviour
{
    // ➡️ CHANGED: Now serializing the character's Transform (easier to drag)
    [Header("Target Character Setup")]
    public Transform characterRoot; // Drag the main character GameObject/Transform here

    [Header("Movement Settings")]
    public float buzzRadius = 0.5f;
    public float speed = 2f;
    private Vector3 targetOffset;

    [Header("Landing Settings")]
    public float landDuration = 2.0f;
    public float landChance = 0.3f;

    // Internal state
    private bool isLanded = false;
    private Transform bodyTarget;
    private float buzzSpeedMultiplier = 1f;

    // Head, Neck, Arms (Shoulders), and Legs
    private readonly HumanBodyBones[] targetBones = new HumanBodyBones[]
    {
        HumanBodyBones.Head,
        HumanBodyBones.Neck,
        HumanBodyBones.LeftUpperArm,
        HumanBodyBones.RightUpperArm,
        HumanBodyBones.LeftUpperLeg,
        HumanBodyBones.RightUpperLeg
    };

    void Start()
    {
        Animator characterAnimator = null;

        // Try to get the Animator from the serialized Transform's GameObject
        if (characterRoot != null)
        {
            characterAnimator = characterRoot.GetComponent<Animator>();
        }

        // 1. Find and set a random target bone
        if (characterAnimator != null && characterAnimator.isHuman)
        {
            HumanBodyBones randomBone = targetBones[Random.Range(0, targetBones.Length)];
            bodyTarget = characterAnimator.GetBoneTransform(randomBone);
        }

        if (bodyTarget == null)
        {
            Debug.LogError("Mosquito could not find a valid landing target. Check if Character Root is assigned and has a Humanoid Animator.");
            Destroy(gameObject);
            return;
        }

        // 2. Initial position
        PickNewOffset();
        transform.position = bodyTarget.position + targetOffset;
        SetLanded(false);
    }

    void Update()
    {
        if (isLanded)
        {
            transform.position = bodyTarget.position;
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            bodyTarget.position + targetOffset,
            speed * buzzSpeedMultiplier * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, bodyTarget.position + targetOffset) < 0.05f)
            PickNewAction();
    }

    void PickNewAction()
    {
        if (Random.value < landChance)
        {
            SetLanded(true);
            transform.position = bodyTarget.position;
            StartCoroutine(FlyAwayAfterTime(landDuration));
        }
        else
        {
            PickNewOffset();
            SetLanded(false);
        }
    }

    void PickNewOffset()
    {
        targetOffset = new Vector3(
            Random.Range(-buzzRadius, buzzRadius),
            Random.Range(-buzzRadius, buzzRadius),
            Random.Range(-buzzRadius, buzzRadius)
        );
    }

    public void Initialize(Transform target)
    {
        // Still ignored, but kept for Spawner compatibility
    }

    IEnumerator FlyAwayAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        SetLanded(false);
        targetOffset = Random.onUnitSphere * 5f;
        buzzSpeedMultiplier = 3f;
    }

    public void SetLanded(bool landed)
    {
        isLanded = landed;
        buzzSpeedMultiplier = landed ? 0f : 1f;
    }
}
