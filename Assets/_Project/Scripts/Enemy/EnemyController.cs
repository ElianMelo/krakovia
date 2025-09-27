using FIMSpace.FProceduralAnimation;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class EnemyController : NetworkBehaviour
{
    [SerializeField] private int maxHP = 5;
    private NetworkVariable<int> currentHP = new NetworkVariable<int>();

    public GameObject explosionEffect;
    public GameObject bloodEffect;
    public LayerMask playerMask;

    private bool isDead = false;

    private Collider enemyCollider;
    private Animator animator;
    private EnemyMovement enemyMovement;
    private EnemyAttack enemyAttack;
    private RagdollAnimator2 ragdoll;

    private const string HurtAnim = "Hurt";
    private const string DeathAnim = "Death";

    public int MaxHP => maxHP;
    public bool IsDead => isDead;
    public NetworkVariable<int> CurrentHP => currentHP;

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
        if (other.gameObject.CompareTag("AttackCollider"))
        {
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            PlayerController sourceDamage = other.gameObject.GetComponentInParent<PlayerController>();
            enemyMovement.OnAttacked(sourceDamage.transform);
            enemyAttack.StartAttackRoutine();
            if (isDead) return;
            ReceiveDamage(sourceDamage.NetworkObjectId,
                contactPoint);
        }
    }

    public void ReceiveDamage(ulong sourceDamage, Vector3 contactPoint)
    {
        ReceiveDamageRpc(NetworkObjectId, sourceDamage, contactPoint);
    }

    [Rpc(SendTo.Server)]
    private void ReceiveDamageRpc(ulong enemyNetworkObjectId, ulong sourceDamage, Vector3 contactPoint)
    {
        if (isDead) return;
        bool isEnemyDead = false;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyNetworkObjectId, out var enemyObj))
        {
            var enemy = enemyObj.GetComponent<EnemyController>();
            if (enemy != null)
            {
                isEnemyDead = enemy.ApplyDamage();
            }
        }

        if (isEnemyDead && NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(sourceDamage, out var playerObj))
        {
            var player = playerObj.GetComponent<PlayerController>();
            if (player != null)
            {
                player.PlayerAttributeController.ReceiveExp(30);
            }
        }

        SendDamageClientRpc(enemyNetworkObjectId, isEnemyDead, contactPoint);
    }

    [Rpc(SendTo.Everyone)]
    private void SendDamageClientRpc(ulong enemyNetworkObjectId, bool isEnemyDead, Vector3 contactPoint)
    {
        Debug.Log($"Enemy ({enemyNetworkObjectId}) took damage!");

        if(!isEnemyDead)
            animator.SetTrigger(HurtAnim);

        var bloodEffectInstance = Instantiate(bloodEffect, contactPoint, Quaternion.identity);
        Destroy(bloodEffectInstance, 2f);

        if (isEnemyDead)
        {
            var effectInstance = Instantiate(explosionEffect, transform.position + new Vector3(0f,1f,0f), Quaternion.identity);
            Destroy(effectInstance, 2f);
            enemyCollider.excludeLayers = playerMask;
            animator.SetTrigger(DeathAnim);
            ragdoll.Settings.AnimatingMode = RagdollHandler.EAnimatingMode.Sleep;
        }
        // Could trigger hit flash, particles, etc.
    }

    private bool ApplyDamage()
    {
        if (!IsServer) return false;

        currentHP.Value -= 1;

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
        GetComponent<Collider>().excludeLayers = playerMask;
        StartCoroutine(DelayedDespawn());
    }

    private IEnumerator DelayedDespawn()
    {
        yield return new WaitForSeconds(6f);
        NetworkObject.Despawn();
    }
}
