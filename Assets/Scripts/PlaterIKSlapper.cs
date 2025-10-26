using UnityEngine;
using TMPro;

[RequireComponent(typeof(Animator))]
public class PlayerIKSlapper : MonoBehaviour
{
    [Header("Camera & IK Target")]
    public Camera cam;
    public Transform rightHandTarget;
    public float handDistance = 2.0f;

    [Header("Slap Settings")]
    public float slapReach = 0.35f;
    public float slapSpeed = 10f;
    public AudioClip slapSound;
    public LayerMask hitMask;
    public TMP_Text scoreText;

    private Animator anim;
    private AudioSource audioSrc;
    private int score;
    private bool isSlapping;

    void Start()
    {
        anim = GetComponent<Animator>();
        audioSrc = gameObject.AddComponent<AudioSource>();
        if (!cam) cam = Camera.main;
        if (!rightHandTarget)
        {
            rightHandTarget = new GameObject("RightHandTarget").transform;
            rightHandTarget.position = transform.position + transform.forward * 1.5f;
        }
        UpdateScoreUI();
    }

    void Update()
    {
        // Move IK target to mouse point in front of camera
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        rightHandTarget.position = ray.GetPoint(handDistance);
        rightHandTarget.rotation = transform.rotation;

        if (Input.GetMouseButtonDown(0) && !isSlapping)
            StartCoroutine(SlapMotion(ray));
    }

    System.Collections.IEnumerator SlapMotion(Ray ray)
    {
        isSlapping = true;
        Vector3 start = rightHandTarget.position;
        Vector3 forward = ray.direction * slapReach;
        Vector3 end = start + forward;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * slapSpeed;
            rightHandTarget.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        audioSrc.PlayOneShot(slapSound);

        // Raycast for mosquitoes or body parts hit
        if (Physics.Raycast(ray, out RaycastHit hit, 3f, hitMask))
        {
            if (hit.collider.CompareTag("Mosquito"))
            {
                Destroy(hit.collider.gameObject);
                score += 10;
                UpdateScoreUI();
            }
        }

        // Return hand
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * slapSpeed;
            rightHandTarget.position = Vector3.Lerp(end, start, t);
            yield return null;
        }
        isSlapping = false;
    }

    
    void OnAnimatorIK(int layerIndex)
    {
        // These must be 1 to make the IK take full control of the hand
        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);

        // These apply the position and rotation from your Update()
        anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
        anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
    }
    void UpdateScoreUI() => scoreText.text = "Score: " + score;
}
