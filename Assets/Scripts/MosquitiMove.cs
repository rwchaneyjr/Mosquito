using UnityEngine;

public class MosquitoMove : MonoBehaviour
{
    public float buzzRadius = 0.5f;
    public float speed = 2f;
    private Vector3 targetOffset;

    void Start() => PickNewOffset();

    void Update()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetOffset, speed * Time.deltaTime);
        if (Vector3.Distance(transform.localPosition, targetOffset) < 0.05f)
            PickNewOffset();
    }

    void PickNewOffset()
    {
        targetOffset = new Vector3(
            Random.Range(-buzzRadius, buzzRadius),
            Random.Range(-buzzRadius, buzzRadius),
            Random.Range(-buzzRadius, buzzRadius)
        );
    }
    public void SetLanded(bool landed)
    {
        // Example: slow the flap and reduce amplitude when perched
        var flapper = GetComponentInChildren<WingFlapper>();
        if (!flapper) return;

        if (landed)
        {
            flapper.flapHz = 12f;
            flapper.amplitudeDeg = 10f;
        }
        else
        {
            flapper.flapHz = 45f;
            flapper.amplitudeDeg = 35f;
        }
    }

}
