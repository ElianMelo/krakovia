using FIMSpace.FProceduralAnimation;
using NUnit.Framework.Internal.Commands;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyController : NetworkBehaviour
{
    [SerializeField] private float maxHP = 5;
    [SerializeField] private int xpAmount = 5;
    private NetworkVariable<float> currentHP = new NetworkVariable<float>();

    public LayerMask playerMask;

    private bool isDead = false;

    private Collider enemyCollider;
    private Animator animator;
    private EnemyMovement enemyMovement;
    private EnemyAttack enemyAttack;
    private RagdollAnimator2 ragdoll;
    public EnemySpawnerZone enemySpawnerZone;

    private const string HurtAnim = "Hurt";
    private const string DeathAnim = "Death";

    public int XpAmount => xpAmount;
    public float MaxHP => maxHP;
    public bool IsDead => isDead;
    public NetworkVariable<float> CurrentHP => currentHP;

    private HashSet<ulong> playersToReceiveExperience = new();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHP.Value = maxHP;
        }
    }

    private void Awake()
    {
        enemyCollider = GetComponent<Collider>();
        enemyMovement = GetComponent<EnemyMovement>();
        enemyAttack = GetComponent<EnemyAttack>();
        animator = GetComponentInChildren<Animator>();
        ragdoll = GetComponentInChildren<RagdollAnimator2>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("AttackCollider")
            && other.gameObject.GetComponent<PlayerWeaponTrigger>() != null)
        {
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            PlayerController sourceDamage = other.gameObject.GetComponentInParent<PlayerController>();
            PlayerAttributeController playerAttributeController = sourceDamage.GetComponent<PlayerAttributeController>();
            PlayerClassController playerClassController = sourceDamage.GetComponent<PlayerClassController>();
            PlayerAttackController playerAttackController = sourceDamage.GetComponent<PlayerAttackController>();
            float skillDamageMultiplier = playerAttributeController.GetMultiplierBasedOnSkill(playerAttackController.CurrentActiveSkillCommand,
                    playerClassController.ActivePlayerClass);
            float damage = playerAttributeController.Damage * (skillDamageMultiplier / 100);
            bool isCritical = Random.Range(0f, 1f) <= playerAttributeController.CriticalChance;
            if(isCritical) damage *= 2;
            enemyMovement.OnAttacked(sourceDamage.transform);
            enemyAttack.StartAttackRoutine();
            if (isDead) return;
            ReceiveDamage(sourceDamage.NetworkObjectId,
                contactPoint, damage, isCritical);
        }
    }

    public void ReceiveDamage(ulong sourceDamage, Vector3 contactPoint, float damage, bool isCritical)
    {
        ReceiveDamageRpc(NetworkObjectId, sourceDamage, contactPoint, damage, isCritical);
    }

    [Rpc(SendTo.Server)]
    private void ReceiveDamageRpc(ulong enemyNetworkObjectId, ulong sourceDamage, Vector3 contactPoint, float damage, bool isCritical)
    {
        if (isDead) return;
        bool isEnemyDead = false;

        playersToReceiveExperience.Add(sourceDamage);

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyNetworkObjectId, out var enemyObj))
        {
            var enemy = enemyObj.GetComponent<EnemyController>();
            if (enemy != null)
            {
                isEnemyDead = enemy.ApplyDamage(damage);
            }
        }

        isDead = isEnemyDead;

        if(isEnemyDead)
        {
            ApplyExperienceListPlays();
            playersToReceiveExperience.Clear();
        }

        SendDamageClientRpc(enemyNetworkObjectId, isEnemyDead, contactPoint, damage, isCritical);
    }

    private void ApplyExperienceListPlays()
    {
        foreach (var playerReceiver in playersToReceiveExperience)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerReceiver, out var playerObj))
            {
                var player = playerObj.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.PlayerAttributeController.ReceiveExp(player.PlayerAttributeController.OwnerClientId, xpAmount);
                }
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SendDamageClientRpc(ulong enemyNetworkObjectId, bool isEnemyDead, Vector3 contactPoint, float damage, bool isCritical)
    {
        NumberWorldSpacePooler.Instance.ShowNumberInWorld((int)damage, transform.position + new Vector3(0f,1f,0f), isCritical);

        if(!isEnemyDead)
            animator.SetTrigger(HurtAnim);

        EnemyEffectPooler.Instance.ShowHitEffect(contactPoint, Quaternion.identity, 2f);

        if (isEnemyDead)
        {
            EnemyEffectPooler.Instance.ShowDeathEffect(transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity, 2f);

            // enemyCollider.excludeLayers = playerMask;
            animator.SetTrigger(DeathAnim);
            ragdoll.Settings.AnimatingMode = RagdollHandler.EAnimatingMode.Sleep;
        }
        // Could trigger hit flash, particles, etc.
    }

    private bool ApplyDamage(float damage)
    {
        if (!IsServer) return false;

        currentHP.Value -= damage;

        if (currentHP.Value <= 0 && !isDead)
        {
            Death();
            return true;
        }
        return false;
    }

    private void Death()
    {
        isDead = true;
        enemyAttack.StopAttackRoutine();
        enemyMovement.OnDeath();
        //GetComponent<Collider>().excludeLayers = playerMask;
        StartCoroutine(DelayedDespawn());
    }

    public void ResetEnemyState()
    {
        ragdoll.Settings.AnimatingMode = RagdollHandler.EAnimatingMode.Standing;
        isDead = false;
        currentHP.Value = maxHP;
        //GetComponent<Collider>().excludeLayers = LayerMask.non;
    }

    private IEnumerator DelayedDespawn()
    {
        yield return new WaitForSeconds(5f);
        ragdoll.Settings.AnimatingMode = RagdollHandler.EAnimatingMode.Off;
        enemySpawnerZone.DespawnEnemy(NetworkObject);
        // NetworkObject.Despawn();
    }
}
