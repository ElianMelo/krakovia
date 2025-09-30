using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public enum SkillCommand
{
    MouseLeft,
    MouseRight,
    Q,
    F
}

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
    private float dashSkillCooldown = 3f;

    private PlayerClassController playerClassController;

    [Header("Fairy")]
    public Transform spellPosition;
    public GameObject mouseLeftSkillPrefab;
    public float mouseLeftSkillForward;
    public GameObject mouseRightSkillPrefab;
    public float mouseRightSkillForward;
    public GameObject qSkillPrefab;
    public float qSkillForward;
    public GameObject fSkillPrefab;
    public float fSkillForward;

    [Header("Skeleton")]
    public Collider skeletonSword;
    public int skeletonMouseLeftSkillFrames;
    public int skeletonMouseRightSkillFrames;
    public int skeletonQSkillFrames;
    public int skeletonFSkillFrames;

    private Coroutine waitingTimerDisableColliderCoroutine;
    private Coroutine handleSkeletonBasicAttackCoroutine;

    void Start()
    {
        if (!IsOwner) return;
        // animator = GetComponentInChildren<Animator>();
        playerClassController = GetComponent<PlayerClassController>();
    }

    void Update()
    {
        if (!IsOwner) return;
        MouseLeftSkill();
        MouseRightSkill();
        QSkill();
        FSkill();
    }

    public void SetupPlayerAnimator(Animator animator)
    {
        this.animator = animator;
    }

    private void SpawnAttackVFX(SkillCommand skillCommand, float forwardIntensity, float upAjust = 0f)
    {
        SpawnAttackProjectileRpc(skillCommand, spellPosition.position + transform.forward * forwardIntensity + new Vector3(0f, upAjust, 0f), spellPosition.rotation);
    }

    [Rpc(SendTo.Server)]
    private void SpawnAttackProjectileRpc(SkillCommand skillCommand, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = GetPrefabBasedOnSkill(skillCommand);
        var instance = Instantiate(prefab, position, rotation);
        var instanceNetworkObject = instance.GetComponent<NetworkObject>();
        instanceNetworkObject.Spawn();
    }

    private GameObject GetPrefabBasedOnSkill(SkillCommand skillCommand)
    {
        switch (skillCommand)
        {
            case SkillCommand.MouseLeft: return mouseLeftSkillPrefab;
            case SkillCommand.MouseRight: return mouseRightSkillPrefab;
            case SkillCommand.Q: return qSkillPrefab;
            case SkillCommand.F: return fSkillPrefab;
            default: return fSkillPrefab;
        }
    }

    private IEnumerator WaitingTimerDisableCollider(int frames)
    {
        skeletonSword.enabled = true;
        yield return new WaitForSeconds(frames * ConstantsManager.FramesToSeconds);
        skeletonSword.enabled = false;
    }

    private void SkeletonColliderHandler(int frames)
    {
        if (waitingTimerDisableColliderCoroutine != null) StopCoroutine(waitingTimerDisableColliderCoroutine);
        waitingTimerDisableColliderCoroutine = StartCoroutine(WaitingTimerDisableCollider(frames));
    }

    private IEnumerator HandleSkeletonBasicAttack(int frames)
    {
        animator.SetLayerWeight(animator.GetLayerIndex("UpperBody"), 1f);
        yield return new WaitForSeconds(frames * ConstantsManager.FramesToSeconds);
        animator.SetLayerWeight(animator.GetLayerIndex("UpperBody"), 0f);
    }

    private void MouseLeftSkill()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && canUseMouseLeftSkill)
        {
            InterfaceManager.Instance.UpdatePlayerSkillFirstCooldown(mouseLeftSkillCooldown);
            //if (playerClassController.PlayerClass == PlayerClass.Skeleton)
            //{
            //    if (handleSkeletonBasicAttackCoroutine != null) StopCoroutine(handleSkeletonBasicAttackCoroutine);
            //    handleSkeletonBasicAttackCoroutine = StartCoroutine(HandleSkeletonBasicAttack(skeletonMouseLeftSkillFrames));
            //}
            animator.SetTrigger("Attack1");
            if (playerClassController.PlayerClass == PlayerClass.Skeleton)
                SkeletonColliderHandler(skeletonMouseLeftSkillFrames);
            if (playerClassController.PlayerClass == PlayerClass.Fairy)
                SpawnAttackVFX(SkillCommand.MouseLeft, mouseLeftSkillForward, 0.3f);
            canUseMouseLeftSkill = false;
            StartCoroutine(EnableMouseLeftSkill(mouseLeftSkillCooldown));
            // CastSearchTarget();
        }
    }

    private void MouseRightSkill()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) && canUseMouseRightSkill)
        {
            InterfaceManager.Instance.UpdatePlayerSkillSecondCooldown(mouseRightSkillCooldown);
            animator.SetTrigger("Attack2");
            if (playerClassController.PlayerClass == PlayerClass.Skeleton)
                SkeletonColliderHandler(skeletonMouseRightSkillFrames);
            if (playerClassController.PlayerClass == PlayerClass.Fairy)
                SpawnAttackVFX(SkillCommand.MouseRight, mouseRightSkillForward);
            canUseMouseRightSkill = false;
            StartCoroutine(EnableMouseRightSkill(mouseRightSkillCooldown));
            // CastSearchTarget();
        }
    }

    private void QSkill()
    {
        if (Input.GetKeyDown(KeyCode.Q) && canUseQSkill)
        {
            InterfaceManager.Instance.UpdatePlayerSkillThirdCooldown(qSkillCooldown);
            animator.SetTrigger("Attack3");
            if (playerClassController.PlayerClass == PlayerClass.Skeleton)
                SkeletonColliderHandler(skeletonQSkillFrames);
            if (playerClassController.PlayerClass == PlayerClass.Fairy)
                SpawnAttackVFX(SkillCommand.Q, qSkillForward);
            canUseQSkill = false;
            StartCoroutine(EnableQSkill(qSkillCooldown));
            // CastSearchTarget();
        }
    }

    private void FSkill()
    {
        if (Input.GetKeyDown(KeyCode.F) && canUseFSkill)
        {
            InterfaceManager.Instance.UpdatePlayerSkillForthCooldown(fSkillCooldown);
            animator.SetTrigger("Attack4");
            if (playerClassController.PlayerClass == PlayerClass.Skeleton)
                SkeletonColliderHandler(skeletonFSkillFrames);
            if (playerClassController.PlayerClass == PlayerClass.Fairy)
                SpawnAttackVFX(SkillCommand.F, fSkillForward);
            canUseFSkill = false;
            StartCoroutine(EnableFSkill(fSkillCooldown));
            // CastSearchTarget();
        }
    }

    public void DashSKill()
    {
        InterfaceManager.Instance.UpdateDashSkillCooldown(dashSkillCooldown);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("AttackCollider")
            && other.gameObject.GetComponent<PlayerWeaponTrigger>() != null)
        {
            var playerAttributeController = GetComponent<PlayerAttributeController>();
            if (other.GetComponentInParent<PlayerAttributeController>() == playerAttributeController)
            {
                Physics.IgnoreCollision(other, GetComponent<Collider>());
                return;
            }
            playerAttributeController.ReceiveDamage(1);
        }
    }

    private void CastSearchTarget()
    {
        var playerAttributeControllers = SphereCastFor<PlayerAttributeController>();
        if (playerAttributeControllers != null && playerAttributeControllers.Count > 0)
        {
            foreach (var playerAttributeController in playerAttributeControllers)
            {
                playerAttributeController.ReceiveDamage(1);
            }
        }

        var enemyControllers = SphereCastFor<EnemyController>();
        if (enemyControllers != null && enemyControllers.Count > 0)
        {
            foreach (var enemyController in enemyControllers)
            {
                Vector3 contactPoint = enemyController.GetComponent<Collider>().ClosestPoint(transform.position);

                enemyController.ReceiveDamage(0, contactPoint);
            }
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
