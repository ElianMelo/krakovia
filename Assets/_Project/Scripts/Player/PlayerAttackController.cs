using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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

    private float dashSkillCooldown = 3f;

    private PlayerClassController playerClassController;
    private PlayerAttributeController playerAttributeController;

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
        playerAttributeController = GetComponent<PlayerAttributeController>();
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
            float mouseLeftCooldown = playerAttributeController.GetSkillCalculatedCooldown(playerClassController.ActivePlayerClass, SkillCommand.MouseLeft);
            InterfaceManager.Instance.UpdatePlayerSkillFirstCooldown(mouseLeftCooldown);
            //if (playerClassController.PlayerClass == PlayerClass.Skeleton)
            //{
            //    if (handleSkeletonBasicAttackCoroutine != null) StopCoroutine(handleSkeletonBasicAttackCoroutine);
            //    handleSkeletonBasicAttackCoroutine = StartCoroutine(HandleSkeletonBasicAttack(skeletonMouseLeftSkillFrames));
            //}
            animator.SetTrigger("Attack1");
            if (playerClassController.ActivePlayerClass == PlayerClass.Skeleton)
                SkeletonColliderHandler(skeletonMouseLeftSkillFrames);
            if (playerClassController.ActivePlayerClass == PlayerClass.Fairy)
                SpawnAttackVFX(SkillCommand.MouseLeft, mouseLeftSkillForward, 0.3f);
            canUseMouseLeftSkill = false;
            StartCoroutine(EnableMouseLeftSkill(mouseLeftCooldown));
            // CastSearchTarget();
        }
    }

    private void MouseRightSkill()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) && canUseMouseRightSkill)
        {
            float mouseRightCooldown = playerAttributeController.GetSkillCalculatedCooldown(playerClassController.ActivePlayerClass, SkillCommand.MouseRight);
            InterfaceManager.Instance.UpdatePlayerSkillSecondCooldown(mouseRightCooldown);
            animator.SetTrigger("Attack2");
            if (playerClassController.ActivePlayerClass == PlayerClass.Skeleton)
                SkeletonColliderHandler(skeletonMouseRightSkillFrames);
            if (playerClassController.ActivePlayerClass == PlayerClass.Fairy)
                SpawnAttackVFX(SkillCommand.MouseRight, mouseRightSkillForward);
            canUseMouseRightSkill = false;
            StartCoroutine(EnableMouseRightSkill(mouseRightCooldown));
            // CastSearchTarget();
        }
    }

    private void QSkill()
    {
        if (Input.GetKeyDown(KeyCode.Q) && canUseQSkill)
        {
            float qSkillCooldown = playerAttributeController.GetSkillCalculatedCooldown(playerClassController.ActivePlayerClass, SkillCommand.Q);
            InterfaceManager.Instance.UpdatePlayerSkillThirdCooldown(qSkillCooldown);
            animator.SetTrigger("Attack3");
            if (playerClassController.ActivePlayerClass == PlayerClass.Skeleton)
                SkeletonColliderHandler(skeletonQSkillFrames);
            if (playerClassController.ActivePlayerClass == PlayerClass.Fairy)
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
            float fSkillCooldown = playerAttributeController.GetSkillCalculatedCooldown(playerClassController.ActivePlayerClass, SkillCommand.F);
            InterfaceManager.Instance.UpdatePlayerSkillForthCooldown(fSkillCooldown);
            animator.SetTrigger("Attack4");
            if (playerClassController.ActivePlayerClass == PlayerClass.Skeleton)
                SkeletonColliderHandler(skeletonFSkillFrames);
            if (playerClassController.ActivePlayerClass == PlayerClass.Fairy)
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
            PlayerAttributeController playerAttributeController = GetComponent<PlayerAttributeController>();
            PlayerAttributeController otherPlayerAttributeController = other.GetComponentInParent<PlayerAttributeController>();
            if (otherPlayerAttributeController == playerAttributeController)
            {
                Physics.IgnoreCollision(other, GetComponent<Collider>());
                return;
            }
            float damage = otherPlayerAttributeController.Damage;
            bool isCritical = Random.Range(0f, 1f) <= otherPlayerAttributeController.CriticalChance;
            if (isCritical) damage *= 2;
            playerAttributeController.ReceivePlayerDamage((int) damage, isCritical);
        }
    }

    private void CastSearchTarget()
    {
        var playerAttributeControllers = SphereCastFor<PlayerAttributeController>();
        if (playerAttributeControllers != null && playerAttributeControllers.Count > 0)
        {
            foreach (var playerAttributeController in playerAttributeControllers)
            {
                PlayerAttributeController thisPlayerAttributeController = GetComponent<PlayerAttributeController>();
                float damage = thisPlayerAttributeController.Damage;
                bool isCritical = Random.Range(0f, 1f) <= thisPlayerAttributeController.CriticalChance;
                if (isCritical) damage *= 2;
                playerAttributeController.ReceivePlayerDamage((int) damage, isCritical);
            }
        }

        var enemyControllers = SphereCastFor<EnemyController>();
        if (enemyControllers != null && enemyControllers.Count > 0)
        {
            foreach (var enemyController in enemyControllers)
            {
                Vector3 contactPoint = enemyController.GetComponent<Collider>().ClosestPoint(transform.position);

                var playerAttributeController = GetComponent<PlayerAttributeController>();

                bool isCritical = Random.Range(0f, 1f) <= playerAttributeController.CriticalChance;
                float damage = playerAttributeController.Damage;
                if (isCritical) damage *= 2;
                enemyController.ReceiveDamage(0, contactPoint, damage, isCritical);
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
