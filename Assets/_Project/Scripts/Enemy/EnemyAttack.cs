using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public LayerMask mask;
    public int damage = 10;
    public float range = 1.5f;
    public float attackDelay = 1.5f;

    private Animator animator;
    private EnemyController enemyController;
    private Transform target;
    private Coroutine attackRoutine;

    private bool isAttacking;
    private const string AttackAnim = "Attack";

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        enemyController = GetComponent<EnemyController>();

        // Opcional: encontre automaticamente o player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            target = player.transform;
    }

    public void StartAttackRoutine(Transform newTarget = null)
    {
        if (newTarget != null)
            target = newTarget;

        if (attackRoutine == null)
            attackRoutine = StartCoroutine(AttackRoutine());
    }

    public void StopAttackRoutine()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
        isAttacking = false;
    }

    private IEnumerator AttackRoutine()
    {
        while (!enemyController.IsDead && target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            if (distance <= range)
            {
                if (!isAttacking)
                {
                    isAttacking = true;
                    animator.SetTrigger(AttackAnim);
                    yield return new WaitForSeconds(attackDelay);
                    isAttacking = false;
                }
            }
            else
            {
                // fora do alcance — espera um pouco antes de tentar de novo
                yield return new WaitForSeconds(0.2f);
            }
        }

        attackRoutine = null;
    }

    // Chamado via evento da animação
    public void AnimationPerformAttack()
    {
        if (enemyController.IsDead) return;

        var playerAttributeControllers = SphereCastFor<PlayerAttributeController>();
        if (playerAttributeControllers != null && playerAttributeControllers.Count > 0)
        {
            foreach (var playerAttributeController in playerAttributeControllers)
            {
                playerAttributeController.ReceiveDamageEnemy(damage, false);
            }
        }
    }

    private List<T> SphereCastFor<T>() where T : Component
    {
        List<T> components = new List<T>();

        var raycastHits = Physics.SphereCastAll(
            transform.position,
            range,
            transform.up,
            range,
            mask
        );

        foreach (var hit in raycastHits)
        {
            if (hit.collider?.gameObject == gameObject)
                continue;

            var component = hit.collider?.GetComponent<T>();
            if (component != null && !components.Contains(component))
                components.Add(component);
        }

        return components;
    }
}
