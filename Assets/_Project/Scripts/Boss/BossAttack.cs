using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AttackConfig
{
    public GameObject attackBox;
    public GameObject attackFill;
    public float frames;
    public float damage;
    public float finalScale;
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
    public float frameSeconds;
    public float stayAttackActive;
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
        bool isAttacking = true;
        float currentScale = 0f;
        float currentDuration = 0;
        AttackConfig attackConfig = attackConfigs[UnityEngine.Random.Range(0, attackConfigs.Count)];
        Collider attackFillCollider = attackConfig.attackFill.GetComponent<Collider>();
        MeshRenderer attackFillMesh = attackConfig.attackFill.GetComponent<MeshRenderer>();
        MeshRenderer attackBoxMesh = attackConfig.attackBox.GetComponent<MeshRenderer>();
        attackFillCollider.enabled = false;
        attackFillMesh.enabled = true;
        attackBoxMesh.enabled = true;
        float maxDuration = attackConfig.frames * frameSeconds;
        // animator.SetTrigger(AttackAnim);
        while (!bossController.IsDead)
        {
            if(currentDuration == 0)
            {
                animator.SetTrigger(attackConfig.attackAnim.ToString());
            }
            if(isAttacking && currentDuration < maxDuration)
            {
                currentDuration += Time.deltaTime;
                currentScale = attackConfig.finalScale * (currentDuration / maxDuration);
                attackConfig.attackFill.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
                yield return null;
            } else
            {
                currentScale = 0f;
                currentDuration = 0f;
                attackConfig.attackFill.GetComponent<BossColliderAttack>().SetupDamage(attackConfig.damage);
                attackFillCollider.enabled = true;
                yield return new WaitForSeconds(stayAttackActive);
                attackFillCollider.enabled = false;
                attackFillMesh.enabled = false;
                attackBoxMesh.enabled = false;
                yield return new WaitForSeconds(attackDelay);

                // Reset
                attackConfig = attackConfigs[UnityEngine.Random.Range(0, attackConfigs.Count)];
                attackFillCollider = attackConfig.attackFill.GetComponent<Collider>();
                attackFillMesh = attackConfig.attackFill.GetComponent<MeshRenderer>();
                attackBoxMesh = attackConfig.attackBox.GetComponent<MeshRenderer>();
                attackFillCollider.enabled = false;
                attackFillMesh.enabled = true;
                attackBoxMesh.enabled = true;
                maxDuration = attackConfig.frames * frameSeconds;
            }
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
