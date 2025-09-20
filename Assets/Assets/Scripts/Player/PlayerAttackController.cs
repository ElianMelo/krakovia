using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerAttackController : NetworkBehaviour
{
    public LayerMask mask;
    public float range;

    private Animator animator;
    void Start()
    {
        if (!IsOwner) return;
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            InterfaceManager.Instance.UpdatePlayerSkillFirstCooldown(3f);
            animator.SetTrigger("Attack1");
            PlayerController enemy = SphereCastEnemy();
            if(enemy != null)
            {
                Debug.Log("Enemy Found!");
                enemy.ReceiveDamage();
            } else
            {
                Debug.Log("Enemy NOT Found!");
            }
        }
    }

    public PlayerController SphereCastEnemy()
    {
        List<RaycastHit> raycastHits = Physics.SphereCastAll(transform.position, range, transform.up, range, mask)
            .ToList();

        foreach (var item in raycastHits)
        {
            if (item.collider?.gameObject == gameObject)
            {
                raycastHits.Remove(item);
                break;
            }
        }

        foreach (var item in raycastHits)
        {
            PlayerController enemy = item.collider?.gameObject?.GetComponent<PlayerController>();
            if (enemy != null)
            {
                return enemy;
            }
        }

        return null;
    }
}
