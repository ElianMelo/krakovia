using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AttackConfig
{
    public float animationLength;
    public AttackAnim attackAnim;
}

public enum AttackAnim
{
    Attack1, Attack2, Attack3, Attack4,
}

public class BossAttack : MonoBehaviour
{
    public LayerMask mask;
    public int damage;
    public float range;
    public float attackDelay;

    private Animator animator;
    private BossController bossController;
    private const string AttackAnim = "Attack";
    private bool attackRoutineRunning;

    private Coroutine attackRoutine;

    [SerializeField]
    public List<AttackConfig> attackConfigs = new();

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        bossController = GetComponent<BossController>();
    }

    public void StartAttackRoutine()
    {
        if (bossController.IsDead) return;
        if (attackRoutineRunning) return;
        if(attackRoutine != null) StopCoroutine(attackRoutine);
        attackRoutine = StartCoroutine(AttackRoutine());
    }

    public void StopAttackRoutine()
    {
        // Swap this if the enemy attack get more complex
        StopAllCoroutines();
    }

    private IEnumerator AttackRoutine()
    {
        attackRoutineRunning = true;
        AttackConfig attackConfig = attackConfigs[UnityEngine.Random.Range(0, attackConfigs.Count)];
        while (!bossController.IsDead)
        {
            animator.SetTrigger(attackConfig.attackAnim.ToString());
            yield return new WaitForSeconds(attackConfig.animationLength);
            yield return new WaitForSeconds(attackDelay);
            attackConfig = attackConfigs[UnityEngine.Random.Range(0, attackConfigs.Count)];
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
