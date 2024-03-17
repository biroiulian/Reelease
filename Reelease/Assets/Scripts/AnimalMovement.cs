using UnityEngine;
using UnityEngine.UIElements;

public class AnimalMovement : MonoBehaviour
{
    public Camera gameCamera;
    public float speed;
    public float rotationSpeed;
    public float baseRestingTime;
    public float baseWalkingTime;
    public float maxSlope;
    public float directioRefreshCooldown = 2;

    private float variableRestingTime;
    private float variableWalkingTime;
    private float startRestingTime = 0;
    private float startWalkingTime = 0;
    private float lastDirectionRefreshTime = 0;
    private bool isWalking;
    private Vector3 direction;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("IsWalking", Random.Range(0, 10)%2==0);
    }

    void Update()
    {

        if (lastDirectionRefreshTime + directioRefreshCooldown < Time.time)
        {
            direction = GetRandomDirectionVector();
            lastDirectionRefreshTime = Time.time;
        }

        if (isWalking)
        {
            MoveInDirection();
            RotateTowardsMovementDirection();
            if(startWalkingTime + variableWalkingTime < Time.time)
            {
                startRestingTime = Time.time;
                variableRestingTime = Random.Range(baseRestingTime - baseRestingTime / 2, baseRestingTime + baseRestingTime / 2);
                isWalking = false;
                animator.SetBool("IsWalking", isWalking);
            }
        }
        else if (!isWalking)
        {
            if (startRestingTime + variableRestingTime < Time.time)
            {
                startWalkingTime = Time.time;
                variableWalkingTime = Random.Range(baseWalkingTime- baseWalkingTime/2, baseWalkingTime + baseWalkingTime / 2);
                isWalking = true;
                animator.SetBool("IsWalking", isWalking);
            }
        } 

    }

    private Vector3 GetRandomDirectionVector()
    {
        return Vector3.Normalize(new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)));
    }

    private void MoveInDirection()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void RotateTowardsMovementDirection()
    {
        var newRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation.normalized, newRotation, rotationSpeed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Natural Obstacle")
        {
            Debug.Log("Collided with something");
            direction = (transform.position - collision.transform.position).normalized; // I guess
            direction.y = 0;
        }
    }


}
