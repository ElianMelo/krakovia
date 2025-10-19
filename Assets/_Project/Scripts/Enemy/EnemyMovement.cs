using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class EnemyMovement : NetworkBehaviour
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
    public float aggroMoveSpeed = 4f;
    public float minAggroTime = 3f;
    public float aggroLoseDistance = 25f;

    [Header("Obstacle & Stuck Handling")]
    public float obstacleDetectDistance = 0.6f;
    public float obstacleSphereRadius = 0.3f;
    public float stuckTimeToReset = 1.0f;

    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f; // tempo mínimo entre ataques
    public string WalkAnim = "Walking";
    public string AttackAnim = "Attacking";

    private Rigidbody rb;
    private Vector3 targetPosition;
    private bool hasTarget = false;
    private float waitTimer = 0f;

    private Transform aggroTarget;
    private float aggroTimer = 0f;
    private float attackTimer = 0f;
    private bool isAttacking = false;

    private Animator animator;

    private Vector3 lastPos;
    private float stuckTimer = 0f;

    private void Awake()
    {
        if (!IsServer) return;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        animator = GetComponentInChildren<Animator>();
        lastPos = transform.position;
        StartCoroutine(CheckDistanceAgroRoutine());
    }

    private IEnumerator CheckDistanceAgroRoutine()
    {
        while (true)
        {
            if (aggroTarget != null)
            {
                float d = Vector3.Distance(transform.position, aggroTarget.position);
                if (d > aggroLoseDistance)
                    LoseAggro();
            }
            yield return new WaitForSeconds(0.5f + 1.5f * Random.Range(0f, 1f));
        }
    }

    private void Update()
    {
        if (!IsServer) return;
        attackTimer -= Time.deltaTime;

        if (aggroTarget != null)
            HandleAggro();
        else
            HandleWander();

        TrackStuck();
    }

    private void HandleWander()
    {
        if (!hasTarget)
        {
            waitTimer -= Time.deltaTime;
            if (animator) animator.SetBool(WalkAnim, false);
            if (waitTimer <= 0f)
                PickNewTarget();
        }
        else
        {
            // if (animator) animator.SetBool(AttackAnim, false);
            if (animator) animator.SetBool(WalkAnim, true);
            MoveTowardsTarget(moveSpeed);
        }
    }

    private void HandleAggro()
    {
        if (aggroTarget == null) return;

        aggroTimer -= Time.deltaTime;

        Vector3 targetPos = aggroTarget.position;

        // Ajuste da altura no terreno
        Vector3 rayOrigin = new Vector3(targetPos.x, targetPos.y + raycastHeight, targetPos.z);
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundCheckDistance + raycastHeight, groundMask))
            targetPos = hit.point;

        Vector3 dir = (targetPos - transform.position);
        Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);

        if (flatDir.sqrMagnitude > 0.001f)
            targetPos -= flatDir.normalized * 0.9f;

        targetPosition = targetPos;
        hasTarget = true;

        float distanceToTarget = flatDir.magnitude;

        // Controle de comportamento e animações
        if (animator != null)
        {
            if (distanceToTarget <= attackRange)
            {
                // Se pode atacar e não está no meio de um ataque
                if (!isAttacking && attackTimer <= 0f)
                {
                    StartCoroutine(PerformAttack());
                }
            }
            else
            {
                // Fora do alcance → mover até o player
                if (!isAttacking)
                {
                    animator.SetBool(WalkAnim, true);
                    MoveTowardsTarget(aggroMoveSpeed);
                }
            }
        }
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        animator.SetBool(WalkAnim, false);
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(0.5f); // pequeno delay antes de liberar movimento (ajuste ao seu anim)
        yield return new WaitForSeconds(attackCooldown * 0.8f); // tempo do ataque antes de permitir novo movimento

        isAttacking = false;
    }

    private void PickNewTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        Vector3 candidate = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        Vector3 rayOrigin = candidate + Vector3.up * raycastHeight;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundCheckDistance + raycastHeight, groundMask))
        {
            targetPosition = hit.point;
            hasTarget = true;
        }
        else
        {
            hasTarget = false;
            waitTimer = 1f;
        }
    }

    private void MoveTowardsTarget(float speed)
    {
        if (isAttacking) return; // não mover durante ataque

        Vector3 flatCurrent = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatTarget = new Vector3(targetPosition.x, 0, targetPosition.z);
        Vector3 toTarget = flatTarget - flatCurrent;

        if (toTarget.sqrMagnitude < 0.3f * 0.3f)
        {
            if (aggroTarget == null)
            {
                hasTarget = false;
                waitTimer = Random.Range(minWaitTime, maxWaitTime);
                if (animator) animator.SetBool(WalkAnim, false);
            }
            return;
        }

        Vector3 moveDir = toTarget.normalized;

        // Detecção de obstáculo
        Vector3 detectOrigin = transform.position + Vector3.up * 0.5f;
        if (Physics.SphereCast(detectOrigin, obstacleSphereRadius, moveDir, out RaycastHit obsHit, obstacleDetectDistance))
        {
            if (((1 << obsHit.collider.gameObject.layer) & groundMask) == 0)
            {
                hasTarget = false;
                waitTimer = Random.Range(0.5f, 1.5f);
                return;
            }
        }

        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            Quaternion smoothRot = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            rb.MoveRotation(smoothRot);
        }

        Vector3 moveStep = moveDir * speed * Time.deltaTime;
        Vector3 projectedNewPosXZ = new Vector3(rb.position.x + moveStep.x, rb.position.y, rb.position.z + moveStep.z);

        Vector3 groundCheckOrigin = new Vector3(projectedNewPosXZ.x, transform.position.y + raycastHeight, projectedNewPosXZ.z);
        if (Physics.Raycast(groundCheckOrigin, Vector3.down, out RaycastHit groundHit, groundCheckDistance + raycastHeight, groundMask))
        {
            float desiredY = groundHit.point.y + 0.01f;
            Vector3 newPos = new Vector3(projectedNewPosXZ.x, desiredY, projectedNewPosXZ.z);
            rb.MovePosition(newPos);
        }
        else
        {
            Vector3 fallbackPos = rb.position + new Vector3(moveStep.x, 0f, moveStep.z);
            rb.MovePosition(fallbackPos);
        }
    }

    public void OnAttacked(Transform attacker)
    {
        aggroTarget = attacker;
        aggroTimer = minAggroTime;
        hasTarget = true;
    }

    private void LoseAggro()
    {
        aggroTarget = null;
        hasTarget = false;
        aggroTimer = 0f;
        isAttacking = false;
        if (animator)
        {
            animator.SetBool(AttackAnim, false);
            animator.SetBool(WalkAnim, false);
        }
    }

    public void OnDeath()
    {
        LoseAggro();
    }

    private void TrackStuck()
    {
        float moved = (transform.position - lastPos).sqrMagnitude;
        if (hasTarget && moved < 0.0001f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= stuckTimeToReset)
            {
                hasTarget = false;
                waitTimer = Random.Range(0.5f, 1.5f);
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPos = transform.position;
    }
}
