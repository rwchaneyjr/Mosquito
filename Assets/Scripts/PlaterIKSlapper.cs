using UnityEngine;
using TMPro;

// Put this on your character
public class PlayerIKSlapper : MonoBehaviour
{
    public Camera cam;
    public Transform rightHandTarget;
    public float handDistance = 2.0f;      // How far from camera
    public float slapDistance = 0.5f;      // How far forward to slap
    public float slapSpeed = 15f;          // How fast to slap
    public TMP_Text scoreText;  // Drag your UI text here in Inspector

    private Animator anim;
    private int score = 0;
    private bool isSlapping = false;

    void Start()
    {
        Debug.Log("========================================");
        Debug.Log("PLAYERIKSLAPPER STARTING");
        Debug.Log($"GameObject name: {gameObject.name}");

        anim = GetComponent<Animator>();

        if (cam == null)
        {
            cam = Camera.main;
        }

        // Create the hand target if it doesn't exist
        if (rightHandTarget == null)
        {
            Debug.Log("Creating RightHandTarget...");
            rightHandTarget = new GameObject("RightHandTarget").transform;
        }

        Debug.Log($"RightHandTarget: {rightHandTarget.name}");

        // Check if scoreText is assigned
        if (scoreText == null)
        {
            Debug.LogError("❌❌❌ SCORE TEXT IS NULL! You must drag your UI Text into the 'Score Text' field!");
        }
        else
        {
            Debug.Log($"✓ Score Text assigned: {scoreText.name}");
        }

        UpdateScoreUI();
        Debug.Log("========================================");
    }

    void Update()
    {
        // Make hand follow mouse
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        rightHandTarget.position = ray.GetPoint(handDistance);
        rightHandTarget.rotation = transform.rotation;

        // Click to slap
        if (Input.GetMouseButtonDown(0) && !isSlapping)
        {
            StartCoroutine(SlapMotion());
        }
    }

    // Slapping animation
    System.Collections.IEnumerator SlapMotion()
    {
        isSlapping = true;

        Vector3 startPos = rightHandTarget.position;
        Vector3 forwardPos = startPos + cam.transform.forward * slapDistance;

        // Slap forward
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * slapSpeed;
            rightHandTarget.position = Vector3.Lerp(startPos, forwardPos, t);
            yield return null;
        }

        // Return back
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * slapSpeed;
            rightHandTarget.position = Vector3.Lerp(forwardPos, startPos, t);
            yield return null;
        }

        isSlapping = false;
    }

    void OnAnimatorIK(int layerIndex)
    {
        // Make hand reach toward target
        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
        anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
        anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
    }

    // Call this to add points
    public void AddScore(int points)
    {
        Debug.Log($"========================================");
        Debug.Log($"AddScore({points}) called!");
        Debug.Log($"Score BEFORE: {score}");

        score += points;

        Debug.Log($"Score AFTER: {score}");

        UpdateScoreUI();

        Debug.Log($"========================================");
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
            Debug.Log($"UI updated to: Score: {score}");
        }
        else
        {
            Debug.LogError("❌ Cannot update UI - scoreText is NULL!");
        }
    }
}
























