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
    public float landChance = 0.4f;

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

    void Start()
    {
        // --- Ensure we have a target ---
        if (characterRoot == null)
        {
            // fallback: try to find the Player tag
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) characterRoot = p.transform;
        }

        Animator anim = characterRoot ? characterRoot.GetComponent<Animator>() : null;

        // --- Find a bone or fallback ---
        if (anim && anim.isHuman)
        {
            HumanBodyBones bone = targetBones[Random.Range(0, targetBones.Length)];
            bodyTarget = anim.GetBoneTransform(bone);
        }

        if (bodyTarget == null && characterRoot != null)
        {
            Debug.LogWarning("Mosquito: using characterRoot as fallback (no bone found).");
            bodyTarget = characterRoot;
        }

        // If still nothing, abort gracefully
        if (bodyTarget == null)
        {
            Debug.LogError("Mosquito: no valid target found. Disabling mosquito.");
            enabled = false;
            return;
        }

        PickNewOffset();
        transform.position = bodyTarget.position + targetOffset;
        SetLanded(false);

        if (isLander)
            StartCoroutine(LandSoon(Random.Range(1f, 3f)));
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
