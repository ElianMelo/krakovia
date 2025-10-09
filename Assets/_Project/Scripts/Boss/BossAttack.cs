using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttack : MonoBehaviour
{
    public LayerMask mask;
    public int damage;
    public float range;
    public float attackDelay;

    private Animator animator;
    private BossController bossController;
    private const string AttackAnim = "Attack";

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        bossController = GetComponent<BossController>();
    }

    public void StartAttackRoutine()
    {
        StartCoroutine(AttackRoutine());
    }

    public void StopAttackRoutine()
    {
        // Swap this if the enemy attack get more complex
        StopAllCoroutines();
    }

    private IEnumerator AttackRoutine()
    {
        while (!bossController.IsDead)
        {
            animator.SetTrigger(AttackAnim);
            yield return new WaitForSeconds(attackDelay);
        }
    }

    // Used in animation
    public void AnimationPerformAttack()
    {
        if (bossController.IsDead) return;
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
            {
                components.Add(component);
            }
        }

        return components;
    }
}
