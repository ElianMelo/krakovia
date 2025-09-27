using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public LayerMask mask;
    public float range;
    public float attackDelay;

    private Animator animator;
    private const string AttackAnim = "Attack";

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
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
        while (true)
        {
            animator.SetTrigger(AttackAnim);
            var playerAttributeControllers = SphereCastFor<PlayerAttributeController>();
            if (playerAttributeControllers != null && playerAttributeControllers.Count > 0)
            {
                foreach (var playerAttributeController in playerAttributeControllers)
                {
                    Debug.Log("Player Found!");
                    playerAttributeController.ReceiveDamage();
                }
            }
            yield return new WaitForSeconds(attackDelay);
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
            if (component != null)
            {
                components.Add(component);
            }
        }

        return components;
    }
}
