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
            animator.SetTrigger("Attack1");
            GameObject enemy = SphereCastEnemy();
            if(enemy != null)
            {
                Debug.Log("Enemy Found!");
            } else
            {
                Debug.Log("Enemy NOT Found!");
            }
        }
    }

    public GameObject SphereCastEnemy()
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
            GameObject enemy = item.collider?.gameObject;
            if (enemy != null)
            {
                return enemy;
            }
        }

        return null;
    }
}
