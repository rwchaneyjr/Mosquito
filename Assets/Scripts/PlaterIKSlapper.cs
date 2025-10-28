using UnityEngine;
using TMPro;

[RequireComponent(typeof(Animator))]
public class PlayerIKSlapper : MonoBehaviour
{
    [Header("Camera & IK Target")]
    public Camera cam;
    public Transform rightHandTarget;
    [Tooltip("How far from the camera the hand target sits while aiming with the mouse.")]
    public float handDistance = 0.9f;

    [Header("Slap Settings")]
    public float slapReach = 0.6f;
    public float slapSpeed = 10f;
    public AudioClip slapSound;
    public TMP_Text scoreText;

    [Header("References")]
    [Tooltip("Drag the HandSlapper (on your hand object) here.")]
    public HandSlapper handSlapper;

    private Animator anim;
    private AudioSource audioSrc;
    private int score;
    private bool isSlapping;

    private Transform chestBone;

    void Start()
    {
        anim = GetComponent<Animator>();
        audioSrc = gameObject.AddComponent<AudioSource>();
        if (!cam) cam = Camera.main;

        // ✅ Find chest or spine bone for consistent slap direction
        chestBone = anim.GetBoneTransform(HumanBodyBones.Chest);
        if (!chestBone)
            chestBone = anim.GetBoneTransform(HumanBodyBones.Spine);

        // Auto-create a hand target if not set
        if (!rightHandTarget)
        {
            rightHandTarget = new GameObject("RightHandTarget").transform;
            var centerRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            rightHandTarget.position = centerRay.GetPoint(handDistance);
        }

        // Make target face same direction as chest
        if (chestBone)
            rightHandTarget.rotation = chestBone.rotation;
        else
            rightHandTarget.rotation = transform.rotation;

        // Auto-link HandSlapper if not assigned
        if (!handSlapper)
        {
            handSlapper = FindObjectOfType<HandSlapper>();
            if (handSlapper)
                Debug.Log($"🔗 Auto-linked HandSlapper: {handSlapper.name}");
            else
                Debug.LogWarning("⚠️ HandSlapper not found! Drag it manually in Inspector.");
        }

        UpdateScoreUI();
    }

    void Update()
    {
        // Move the target with mouse pointer
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        rightHandTarget.position = ray.GetPoint(handDistance);

        // Rotate to match body orientation
        if (chestBone)
            rightHandTarget.rotation = chestBone.rotation;
        else
            rightHandTarget.rotation = transform.rotation;

        // Start slap on click
        if (Input.GetMouseButtonDown(0) && !isSlapping)
        {
            StartCoroutine(SlapMotion());
        }

        // Optional debug line to see slap direction in Scene view
        if (chestBone)
        {
            Vector3 worldDir = chestBone.TransformDirection(new Vector3(0f, 0f, -1f)); // away from chest
            Debug.DrawRay(rightHandTarget.position, worldDir * slapReach, Color.cyan);
        }
    }

    System.Collections.IEnumerator SlapMotion()
    {
        isSlapping = true;
        if (handSlapper) handSlapper.isSlapping = true;

        Vector3 start = rightHandTarget.position;

        // ✅ Always slap AWAY from the chest, regardless of rotation
        Vector3 localDir = new Vector3(0f, 0f, -1f); // negative Z = away from body
        Transform refBone = (chestBone ? chestBone : transform);
        Vector3 worldDir = refBone.TransformDirection(localDir).normalized;
        Vector3 end = start + worldDir * slapReach;

        Debug.Log($"🖐 Slapping direction: {worldDir}");
        Debug.DrawRay(start, worldDir * slapReach, Color.green, 1f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * slapSpeed;
            rightHandTarget.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.15f); // small overlap window

        if (slapSound) audioSrc.PlayOneShot(slapSound);

        // Return hand to original position
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * slapSpeed;
            rightHandTarget.position = Vector3.Lerp(end, start, t);
            yield return null;
        }

        isSlapping = false;
        if (handSlapper) handSlapper.isSlapping = false;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (anim == null || rightHandTarget == null) return;

        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
        anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
        anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
    }

    // --- Score System ---
    public void AddScore(int points)
    {
        int before = score;
        score += points;
        Debug.Log($"📈 AddScore called! Before={before}, After={score}, +{points}");
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText)
        {
            scoreText.text = "Score: " + score;
        }
        else
        {
            Debug.LogWarning("⚠️ ScoreText is NULL! Drag a TMP text object into PlayerIKSlapper.");
        }
    }
}
