using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Wander Settings")]
    public float wanderRadius = 8f;
    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;

    [Header("Ground Settings")]
    public float raycastHeight = 2f;
    public float groundCheckDistance = 4f;
    public LayerMask groundMask;

    [Header("Aggro Settings")]
    public float aggroMoveSpeed = 4f;        // Faster when chasing
    public float minAggroTime = 3f;          // Must stay aggroed for at least this long

    private Rigidbody rb;
    private Vector3 targetPosition;
    private bool hasTarget = false;
    private float waitTimer = 0f;

    private Transform aggroTarget;
    private float aggroTimer = 0f;

    private Animator animator;

    private const string WalkAnim = "Walking";

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Avoid physics tipping the enemy over
        animator = GetComponentInChildren<Animator>();
        StartCoroutine(CheckDistanceAgroRoutine());
    }

    private IEnumerator CheckDistanceAgroRoutine()
    {
        if(aggroTarget != null && Vector3.Distance(transform.position, aggroTarget.position) > 10f)
        {
            OnDeath();
        }
        yield return new WaitForSeconds(5f * Random.Range(0f, 1f));
    }

    private void Update()
    {
        if (aggroTarget != null)
        {
            HandleAggro();
        }
        else
        {
            HandleWander();
        }
    }

    private void HandleWander()
    {
        if (!hasTarget)
        {
            waitTimer -= Time.deltaTime;
            animator.SetBool(WalkAnim, false);
            if (waitTimer <= 0f)
            {
                PickNewTarget();
            }
        }
        else
        {
            animator.SetBool(WalkAnim, true);
            MoveTowardsTarget(moveSpeed);
        }
    }

    private void HandleAggro()
    {
        if (aggroTarget == null) return;

        aggroTimer -= Time.deltaTime;

        // Keep chasing the aggro target
        Vector3 targetPos = aggroTarget.position;

        // Raycast ground below target so enemy doesn’t float
        Vector3 rayOrigin = targetPos + Vector3.up * raycastHeight;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundCheckDistance, groundMask))
        {
            targetPos = hit.point;
        }

        // Apply offset so enemy stops a bit before reaching the target
        Vector3 dir = (targetPos - transform.position).normalized;
        float offset = 1.0f; // adjust this value as needed
        targetPos -= dir * offset;

        targetPosition = targetPos;
        hasTarget = true;

        animator.SetBool(WalkAnim, true);
        MoveTowardsTarget(aggroMoveSpeed);
    }

    private void PickNewTarget()
    {
        // Pick a random point in a circle around the enemy
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        Vector3 candidate = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        // Raycast down to find the ground
        Vector3 rayOrigin = candidate + Vector3.up * raycastHeight;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundCheckDistance, groundMask))
        {
            targetPosition = hit.point;
            hasTarget = true;
        }
        else
        {
            waitTimer = 1f; // retry later
        }
    }

    private void MoveTowardsTarget(float speed)
    {
        Vector3 flatCurrent = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatTarget = new Vector3(targetPosition.x, 0, targetPosition.z);
        Vector3 toTarget = flatTarget - flatCurrent;

        if (toTarget.sqrMagnitude < 0.3f * 0.3f)
        {
            if (aggroTarget == null) // Only stop wandering if not chasing
            {
                hasTarget = false;
                waitTimer = Random.Range(minWaitTime, maxWaitTime);
            }
            return;
        }

        Vector3 moveDir = toTarget.normalized;

        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            Quaternion smoothRot = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            rb.MoveRotation(smoothRot);
        }

        Vector3 moveStep = moveDir * speed * Time.deltaTime;
        Vector3 newPos = rb.position + moveStep;
        rb.MovePosition(newPos);
    }

    // Call this when enemy is attacked
    public void OnAttacked(Transform attacker)
    {
        if (aggroTarget == null || aggroTimer <= 0f)
        {
            aggroTarget = attacker;
            aggroTimer = minAggroTime;
            hasTarget = true;
        }
    }

    // Call this when enemy dies
    public void OnDeath()
    {
        aggroTarget = null;
        hasTarget = false;
    }

    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(transform.position, wanderRadius);

    //    if (hasTarget)
    //    {
    //        Gizmos.color = (aggroTarget != null) ? Color.red : Color.cyan;
    //        Gizmos.DrawSphere(targetPosition, 0.2f);
    //    }
    //}
}
