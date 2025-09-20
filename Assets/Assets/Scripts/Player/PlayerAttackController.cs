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

    private void MouseLeftSkill()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && canUseMouseLeftSkill)
        {
            InterfaceManager.Instance.UpdatePlayerSkillFirstCooldown(mouseLeftSkillCooldown);
            animator.SetTrigger("Attack1");
            canUseMouseLeftSkill = false;
            StartCoroutine(EnableMouseLeftSkill(mouseLeftSkillCooldown));
            CastSearchPlayer();
        }
    }

    private void MouseRightSkill()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) && canUseMouseRightSkill)
        {
            InterfaceManager.Instance.UpdatePlayerSkillSecondCooldown(mouseRightSkillCooldown);
            animator.SetTrigger("Attack2");
            canUseMouseRightSkill = false;
            StartCoroutine(EnableMouseRightSkill(mouseRightSkillCooldown));
            CastSearchPlayer();
        }
    }

    private void QSkill()
    {
        if (Input.GetKeyDown(KeyCode.Q) && canUseQSkill)
        {
            InterfaceManager.Instance.UpdatePlayerSkillThirdCooldown(qSkillCooldown);
            animator.SetTrigger("Attack3");
            canUseQSkill = false;
            StartCoroutine(EnableQSkill(qSkillCooldown));
            CastSearchPlayer();
        }
    }

    private void FSkill()
    {
        if (Input.GetKeyDown(KeyCode.F) && canUseFSkill)
        {
            InterfaceManager.Instance.UpdatePlayerSkillForthCooldown(fSkillCooldown);
            animator.SetTrigger("Attack4");
            canUseFSkill = false;
            StartCoroutine(EnableFSkill(fSkillCooldown));
            CastSearchPlayer();
        }
    }

    private void CastSearchPlayer()
    {
        PlayerController playerController = SphereCastPlayerController();
        if (playerController != null)
        {
            Debug.Log("Enemy Found!");
            playerController.ReceiveDamage();
        }
        else
        {
            Debug.Log("Enemy NOT Found!");
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

    public PlayerController SphereCastPlayerController()
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
            PlayerController playerController = item.collider?.gameObject?.GetComponent<PlayerController>();
            if (playerController != null)
            {
                return playerController;
            }
        }

        return null;
    }
}
