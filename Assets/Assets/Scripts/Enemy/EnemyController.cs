using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

public class EnemyController : NetworkBehaviour
{
    [SerializeField] private int maxHP = 5;
    private NetworkVariable<int> currentHP = new NetworkVariable<int>();

    public GameObject effect;

    private bool isDead = false;

    public int MaxHP => maxHP;
    public NetworkVariable<int> CurrentHP => currentHP;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHP.Value = maxHP;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("AttackCollider"))
        {
            ReceiveDamage(other.gameObject.GetComponentInParent<PlayerController>().NetworkObjectId);
        }
    }

    public void ReceiveDamage(ulong sourceDamage)
    {
        ReceiveDamageRpc(NetworkObjectId, sourceDamage);
    }

    [Rpc(SendTo.Server)]
    private void ReceiveDamageRpc(ulong enemyNetworkObjectId, ulong sourceDamage)
    {
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

        SendDamageClientRpc(enemyNetworkObjectId, isEnemyDead);
    }

    [Rpc(SendTo.Everyone)]
    private void SendDamageClientRpc(ulong enemyNetworkObjectId, bool isEnemyDead)
    {
        Debug.Log($"Enemy ({enemyNetworkObjectId}) took damage!");
        if (isEnemyDead)
        {
            var effectInstance = Instantiate(effect, transform.position + new Vector3(0f,1f,0f), Quaternion.identity);
            Destroy(effectInstance, 2f);
        }
        // Could trigger hit flash, particles, etc.
    }

    private bool ApplyDamage()
    {
        if (!IsServer) return false;

        currentHP.Value -= 1;

        if (currentHP.Value <= 0 && !isDead)
        {
            isDead = true;
            StartCoroutine(DelayedDespawn());
            return true;
        }
        return false;
    }

    private IEnumerator DelayedDespawn()
    {
        yield return new WaitForSeconds(1f);
        NetworkObject.Despawn();
    }
}
