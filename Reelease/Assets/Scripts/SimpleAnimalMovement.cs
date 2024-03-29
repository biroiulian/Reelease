using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


public class SimpleAnimalMovement : MonoBehaviour
{
    public float baseJumpCooldown = 5;
    public float jumpDuration = 2;
    private float lastJumpTime;
    private float varJumpCooldown;
    private bool isJumping=false;
    private Animator animator;
    public Vector3 jumpingForce;
    private float lastJumpForce = 0;
    public float jumpForceCooldown = 0.3f;

    private Vector3 direction;

    private void Awake()
    {
        lastJumpTime = Time.time;
        animator = GetComponent<Animator>();
        animator.SetBool("isJumping", isJumping);
        varJumpCooldown = (int)Random.Range(baseJumpCooldown - baseJumpCooldown / 2, baseJumpCooldown + baseJumpCooldown / 2);
        direction = GetRandomDirectionVector();
    }

    void Update()
    {
        if (lastJumpTime + varJumpCooldown < Time.time)
        {
            isJumping = true;
            animator.SetBool("isJumping", isJumping);
            lastJumpTime = Time.time;
            varJumpCooldown = Random.Range(baseJumpCooldown - baseJumpCooldown / 2, baseJumpCooldown + baseJumpCooldown / 2);
        }
        if (isJumping)
        {
            if (lastJumpTime + jumpDuration < Time.time)
            {
                isJumping = false;
                animator.SetBool("isJumping", isJumping);
                lastJumpTime = Time.time;
                varJumpCooldown = Random.Range(baseJumpCooldown - baseJumpCooldown / 2, baseJumpCooldown + baseJumpCooldown / 2);
            }
            if (isJumping)
            {
                CheckSlope();
                RotateTowardsMovementDirection();
                Jump();
            }
        }
    }

    private void MoveInDirection()
    {
        transform.position += 3f * Time.deltaTime * direction;
    }

    private void Jump()
    {
        if (lastJumpForce + jumpForceCooldown < Time.time)
        {
            direction = GetCoherentNextDirectionVector();
            var jump = new Vector3(jumpingForce.x * direction.x, jumpingForce.y, jumpingForce.z * direction.z);
            gameObject.GetComponent<Rigidbody>().AddForce(jump, ForceMode.Force);
            lastJumpForce = Time.time;
        }
    }

    private void RotateTowardsMovementDirection()
    {
        var newRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation.normalized, newRotation, 4);
    }

    private Vector3 GetRandomDirectionVector()
    {
        var newDir = Vector3.Normalize(new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)));
        if (newDir.x == 0) newDir.x = 1;
        if (newDir.z == 0) newDir.z = -1;
        return newDir;
    }

    private Vector3 GetOppositeDirectionVector()
    {
        float nextX, nextZ;
        if (Random.Range(1, 3) % 2 == 0)
        {
            nextX = -direction.x / 2;
            nextZ = -direction.z;
        }
        else
        {
            nextX = -direction.x;
            nextZ = -direction.z / 2;
        }

        return Vector3.Normalize(new Vector3(nextX, 0, nextZ));
    }

    private void CheckSlope()
    {
        Ray ray = new Ray(transform.position + direction / 4 + Vector3.up / 2, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 20))
        {
            if (Mathf.Abs(hitInfo.normal.x) > 0.15f)
            {
                direction = GetOppositeDirectionVector();
            }
        }
    }

    private Vector3 GetCoherentNextDirectionVector()
    {
        var nextX = direction.x + Mathf.Pow(-1, Random.Range(1, 3)) * 0.5f;
        var nextZ = direction.z + Mathf.Pow(-1, Random.Range(1, 3)) * 0.5f;

        return Vector3.Normalize(new Vector3(nextX, 0, nextZ));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Natural Obstacle")
        {
            direction = GetOppositeDirectionVector();
            isJumping = false;
            animator.SetBool("isJumping", isJumping);
        }
    }

}
