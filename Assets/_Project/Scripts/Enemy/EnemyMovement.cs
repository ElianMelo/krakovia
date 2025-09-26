using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Unity.Netcode.Components;

[RequireComponent(typeof(NetworkTransform))]
public class EnemyMovement : NetworkBehaviour
{
    public float moveSpeed = 2f;
    public float wanderRadius = 10f;
    public float idleTime = 2f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    //private bool isMoving = false;

    private Animator animator;

    private const string WalkAnim = "Walking";

    private void Start()
    {
        startPosition = transform.position;
        animator = GetComponentInChildren<Animator>();

        if (IsServer)
            StartCoroutine(WanderRoutine());
    }

    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            animator.SetBool(WalkAnim, true);

            // Pick random target
            Vector3 randomOffset = new Vector3(
                Random.Range(-wanderRadius, wanderRadius),
                0f,
                Random.Range(-wanderRadius, wanderRadius)
            );
            targetPosition = startPosition + randomOffset;

            float stopThreshold = 0.2f;
            float stopThresholdSqr = stopThreshold * stopThreshold;

            // Safety timers
            float maxWalkTime = 5f; // max seconds trying to reach a point
            float stuckCheckInterval = 1f; // check every second
            float stuckThreshold = 0.05f; // movement below this is considered "stuck"

            float elapsedTime = 0f;
            float stuckTimer = 0f;
            Vector3 lastPosition = transform.position;

            while ((transform.position - targetPosition).sqrMagnitude > stopThresholdSqr)
            {
                elapsedTime += Time.deltaTime;
                stuckTimer += Time.deltaTime;

                // Direction to target
                Vector3 toTarget = targetPosition - transform.position;
                Vector3 flatDirection = new Vector3(toTarget.x, 0f, toTarget.z).normalized;

                // --- Handle slopes ---
                Vector3 move = flatDirection * moveSpeed * Time.deltaTime;
                if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 2f))
                {
                    // Project move onto ground plane
                    move = Vector3.ProjectOnPlane(move, hit.normal);
                }

                transform.position += move;

                // Smooth rotation towards movement direction
                if (flatDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(flatDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
                }

                // --- Stuck detection ---
                if (stuckTimer >= stuckCheckInterval)
                {
                    float movedDistance = (transform.position - lastPosition).sqrMagnitude;
                    if (movedDistance < stuckThreshold * stuckThreshold)
                    {
                        // Consider stuck → break out early
                        break;
                    }
                    lastPosition = transform.position;
                    stuckTimer = 0f;
                }

                // --- Timeout check ---
                if (elapsedTime >= maxWalkTime)
                {
                    break;
                }

                yield return null;
            }

            animator.SetBool(WalkAnim, false);

            yield return new WaitForSeconds(idleTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, wanderRadius);
    }
}
