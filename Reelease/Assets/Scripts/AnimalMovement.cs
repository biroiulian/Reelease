using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public enum State
{
    Idle, //0
    Idle2, //1 
    Eating, //2
    Walking //3
}

public class AnimalMovement : MonoBehaviour
{
    public float speed;
    public float rotationSpeed;
    public float maxSlope;
    public float baseStateCooldown = 4;
    public float directionCooldown = 1;

    private float lastRefreshStateTime;
    private float varStateCooldown;
    private float lastRefreshDirectionTime;
    private State state;
    private Vector3 direction;
    private Animator animator;

    private void Awake()
    {
        state = (State)Random.Range(0, 4);
        if (state == State.Walking)
        {
            varStateCooldown = Random.Range(baseStateCooldown - baseStateCooldown / 2, baseStateCooldown + baseStateCooldown / 2);
        }
        else
        {
            varStateCooldown = Random.Range(baseStateCooldown, baseStateCooldown + baseStateCooldown / 2);
        }

        direction = GetRandomDirectionVector();

        lastRefreshStateTime = Time.time;
        lastRefreshDirectionTime = Time.time;
        animator = GetComponent<Animator>();
        animator.SetInteger("State", (int)state);
    }

    void Update()
    {
        if(direction.y != 0) { Debug.Break(); } // We should never move downwards or upwards

        if(lastRefreshDirectionTime + directionCooldown < Time.time)
        {
            direction = GetCoherentNextDirectionVector();
            Debug.Log("new dir " + direction);
            lastRefreshDirectionTime += directionCooldown;
        }

        if (lastRefreshStateTime + varStateCooldown < Time.time)
        {
            state = (State)Random.Range(0, 4);
            if (state == State.Walking)
            {
                direction = GetCoherentNextDirectionVector();
                varStateCooldown = Random.Range(baseStateCooldown, baseStateCooldown * 3);
            }
            else if (state == State.Eating) 
            {
                varStateCooldown = 1f;
            }
            else
            {
                varStateCooldown = Random.Range(baseStateCooldown - baseStateCooldown/2, baseStateCooldown + baseStateCooldown / 2);
            }

            lastRefreshStateTime = Time.time;
            animator.SetInteger("State", (int)state);
        }

        if (state == State.Walking)
        {
            CheckSlope();
            RotateTowardsMovementDirection();
            MoveInDirection();
        }
    }

    private Vector3 GetRandomDirectionVector()
    {
        var newDir =  Vector3.Normalize(new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)));
        if (newDir.x == 0) newDir.x = 1;
        if (newDir.z == 0) newDir.z = -1;
        return newDir;
    }

    // This should keep the next direction in the same quadran
    private Vector3 GetCoherentNextDirectionVector()
    {
        var nextX = direction.x + Mathf.Pow(-1, Random.Range(1,3)) * 0.5f;
        var nextZ = direction.z + Mathf.Pow(-1, Random.Range(1,3)) * 0.5f;

        return Vector3.Normalize(new Vector3(nextX, 0, nextZ));
    }

    private Vector3 GetOppositeDirectionVector()
    {
        float nextX, nextZ;
        if (Random.Range(1, 3)%2==0)
        {
            nextX = -direction.x/2;
            nextZ = -direction.z;
        }
        else
        {
            nextX = -direction.x;
            nextZ = -direction.z/2;
        }

        return Vector3.Normalize(new Vector3(nextX, 0, nextZ));
    }

    private void MoveInDirection()
    {
        transform.position += speed * Time.deltaTime * direction;
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
            direction = GetOppositeDirectionVector();
            //Debug.DrawLine(transform.position, collision.gameObject.transform.position, Color.red, 3);
        }
    }

    private void CheckSlope()
    {
        Ray ray = new Ray(transform.position + direction/4 + Vector3.up/2, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 20))
        {
            if (Mathf.Abs(hitInfo.normal.x) > maxSlope)
            {
                //Debug.DrawLine(ray.origin, hitInfo.point, Color.red, 3);
                direction = GetOppositeDirectionVector();
            }
            //Debug.DrawLine(ray.origin, ray.origin + ray.direction * 3, Color.green);
        }
    }

}
