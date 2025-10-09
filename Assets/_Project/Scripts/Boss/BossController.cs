using FIMSpace.FProceduralAnimation;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class BossController : NetworkBehaviour
{
    [SerializeField] private float maxHP = 5;
    [SerializeField] private int xpAmount = 5;
    private NetworkVariable<float> currentHP = new NetworkVariable<float>();

    public GameObject explosionEffect;
    public GameObject bloodEffect;
    public LayerMask playerMask;

    private bool isDead = false;

    private Collider enemyCollider;
    private Animator animator;
    private BossAttack bossAttack;
    private RagdollAnimator2 ragdoll;

    private const string HurtAnim = "Hurt";
    private const string DeathAnim = "Death";

    public int XpAmount => xpAmount;
    public float MaxHP => maxHP;
    public bool IsDead => isDead;
    public NetworkVariable<float> CurrentHP => currentHP;

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
        bossAttack = GetComponent<BossAttack>();
        animator = GetComponentInChildren<Animator>();
        ragdoll = GetComponentInChildren<RagdollAnimator2>();
    }

    public void WeakpointTakeDamage(Collider other)
    {
        Debug.Log("Receive Damage!");
        Vector3 contactPoint = other.ClosestPoint(transform.position);
        PlayerController sourceDamage = other.gameObject.GetComponentInParent<PlayerController>();
        PlayerAttributeController playerAttributeController = other.gameObject.GetComponentInParent<PlayerAttributeController>();
        float damage = playerAttributeController.Damage;
        bool isCritical = Random.Range(0f, 1f) <= playerAttributeController.CriticalChance;
        if (isCritical) damage *= 2;
        bossAttack.StartAttackRoutine();
        if (isDead) return;
        ReceiveDamage(sourceDamage.NetworkObjectId,
            contactPoint, damage, isCritical);
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
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyNetworkObjectId, out var enemyObj))
        {
            var enemy = enemyObj.GetComponent<BossController>();
            if (enemy != null)
            {
                isEnemyDead = enemy.ApplyDamage(damage);
            }
        }

        isDead = isEnemyDead;

        if (isEnemyDead && NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(sourceDamage, out var playerObj))
        {
            var player = playerObj.GetComponent<PlayerController>();
            if (player != null)
            {
                player.PlayerAttributeController.ReceiveExp(player.PlayerAttributeController.OwnerClientId, xpAmount);
            }
        }

        SendDamageClientRpc(enemyNetworkObjectId, isEnemyDead, contactPoint, damage, isCritical);
    }

    [Rpc(SendTo.Everyone)]
    private void SendDamageClientRpc(ulong enemyNetworkObjectId, bool isEnemyDead, Vector3 contactPoint, float damage, bool isCritical)
    {
        NumberWorldSpacePooler.Instance.ShowNumberInWorld((int)damage, transform.position + new Vector3(0f,1f,0f), isCritical);

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
        bossAttack.StopAttackRoutine();
        GetComponent<Collider>().excludeLayers = playerMask;
        StartCoroutine(DelayedDespawn());
    }

    private IEnumerator DelayedDespawn()
    {
        yield return new WaitForSeconds(6f);
        NetworkObject.Despawn();
    }
}
