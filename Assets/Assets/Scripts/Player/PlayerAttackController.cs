using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerAttackController : NetworkBehaviour
{
    public LayerMask mask;
    public float range;

    private Animator animator;

    private bool canUseMouseLeftSkill = true;
    private bool canUseMouseRightSkill = true;
    private bool canUseQSkill = true;
    private bool canUseFSkill = true;

    private float mouseLeftSkillCooldown = 0.5f;
    private float mouseRightSkillCooldown = 2f;
    private float qSkillCooldown = 3f;
    private float fSkillCooldown = 5f;

    public GameObject mouseLeftSkillPrefab;
    public GameObject mouseRightSkillPrefab;
    public GameObject qSkillPrefab;
    public GameObject fSkillPrefab;

    void Start()
    {
        if (!IsOwner) return;
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (!IsOwner) return;
        MouseLeftSkill();
        MouseRightSkill();
        QSkill();
        FSkill();
    }

    private void SpawnAttackVFX(GameObject vfxPrefab)
    {
        Instantiate(vfxPrefab, transform.forward * 1.2f, transform.rotation);
    }

    private void MouseLeftSkill()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && canUseMouseLeftSkill)
        {
            InterfaceManager.Instance.UpdatePlayerSkillFirstCooldown(mouseLeftSkillCooldown);
            animator.SetTrigger("Attack1");
            SpawnAttackVFX(mouseLeftSkillPrefab);
            canUseMouseLeftSkill = false;
            StartCoroutine(EnableMouseLeftSkill(mouseLeftSkillCooldown));
            CastSearchTarget();
        }
    }

    private void MouseRightSkill()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) && canUseMouseRightSkill)
        {
            InterfaceManager.Instance.UpdatePlayerSkillSecondCooldown(mouseRightSkillCooldown);
            animator.SetTrigger("Attack2");
            SpawnAttackVFX(mouseRightSkillPrefab);
            canUseMouseRightSkill = false;
            StartCoroutine(EnableMouseRightSkill(mouseRightSkillCooldown));
            CastSearchTarget();
        }
    }

    private void QSkill()
    {
        if (Input.GetKeyDown(KeyCode.Q) && canUseQSkill)
        {
            InterfaceManager.Instance.UpdatePlayerSkillThirdCooldown(qSkillCooldown);
            animator.SetTrigger("Attack3");
            SpawnAttackVFX(qSkillPrefab);
            canUseQSkill = false;
            StartCoroutine(EnableQSkill(qSkillCooldown));
            CastSearchTarget();
        }
    }

    private void FSkill()
    {
        if (Input.GetKeyDown(KeyCode.F) && canUseFSkill)
        {
            InterfaceManager.Instance.UpdatePlayerSkillForthCooldown(fSkillCooldown);
            animator.SetTrigger("Attack4");
            SpawnAttackVFX(fSkillPrefab);
            canUseFSkill = false;
            StartCoroutine(EnableFSkill(fSkillCooldown));
            CastSearchTarget();
        }
    }

    private void CastSearchTarget()
    {
        var playerController = SphereCastFor<PlayerController>();
        if (playerController != null)
        {
            Debug.Log("Player Found!");
            playerController.ReceiveDamage();
        }

        var enemyController = SphereCastFor<EnemyController>();
        if (enemyController != null)
        {
            Debug.Log("Enemy Found!");
            enemyController.ReceiveDamage();
        }
    }

    private IEnumerator EnableMouseLeftSkill(float duration)
    {
        yield return new WaitForSeconds(duration);
        canUseMouseLeftSkill = true;
    }

    private IEnumerator EnableMouseRightSkill(float duration)
    {
        yield return new WaitForSeconds(duration);
        canUseMouseRightSkill = true;
    }

    private IEnumerator EnableQSkill(float duration)
    {
        yield return new WaitForSeconds(duration);
        canUseQSkill = true;
    }

    private IEnumerator EnableFSkill(float duration)
    {
        yield return new WaitForSeconds(duration);
        canUseFSkill = true;
    }

    private T SphereCastFor<T>() where T : Component
    {
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
                return component;
            }
        }

        return null;
    }
}
