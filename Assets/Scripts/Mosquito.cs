using UnityEngine;
using System.Collections;

public class Mosquito : MonoBehaviour
{
    [Header("Target Character Setup")]
    public Transform characterRoot;
    public bool isLander = false; // set by spawner

    [Header("Movement Settings")]
    public float buzzRadius = 0.5f;
    public float speed = 2f;

    [Header("Landing Settings")]
    public float landDuration = 2f;
    public float landChance = 1f;

    // internal
    private bool isLanded;
    private Transform bodyTarget;
    private Vector3 targetOffset;
    private float buzzSpeedMultiplier = 1f;

    private readonly HumanBodyBones[] targetBones = new HumanBodyBones[]
    {
        HumanBodyBones.Head,
        HumanBodyBones.Neck,
        HumanBodyBones.LeftUpperArm,
        HumanBodyBones.RightUpperArm,
        HumanBodyBones.LeftUpperLeg,
        HumanBodyBones.RightUpperLeg
    };

    // Mosquito.cs (Start function)

    void Start()
    {
        // ... (unchanged code above)

        // --- Find a bone, NO FALLBACK ---
        // The playerTarget should have an Animator component
        Animator anim = characterRoot ? characterRoot.GetComponent<Animator>() : null;

        if (anim && anim.isHuman)
        {
            // Try to target the specific bones you want
            HumanBodyBones bone = targetBones[Random.Range(0, targetBones.Length)];
            bodyTarget = anim.GetBoneTransform(bone);
        }

        if (bodyTarget == null)
        {
            // CRITICAL FIX: Abort if no bone is found. 
            // This prevents the mosquito from using the player's center position, 
            // which causes the straight-line movement without hovering/landing.
            Debug.LogError("Mosquito: Cannot find a valid Humanoid bone target. Disabling mosquito.");
            Destroy(gameObject); // Safely remove the mosquito instance
            return;
        }

        // ... (unchanged code below)
    }

    void Update()
    {
        if (!bodyTarget) return;

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
        if (Random.value < landChance && isLander)
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

    IEnumerator FlyAwayAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        SetLanded(false);
        targetOffset = Random.onUnitSphere * 3f;
        buzzSpeedMultiplier = 3f;
    }

    IEnumerator LandSoon(float delay)
    {
        yield return new WaitForSeconds(delay);
        PickNewAction();
    }

    // Mosquito.cs (SetLanded function)

    void SetLanded(bool landed)
    {
        isLanded = landed;
        buzzSpeedMultiplier = landed ? 0f : 1f;

        // We change the scale multiplier to a large value (15f) for guaranteed visibility.
        transform.localScale = landed ? Vector3.one * 10f : Vector3.one * 15f;
    }

    // --- Added so slapper can check landing state ---
    public bool IsLanded() => isLanded;
}
