using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyController : NetworkBehaviour
{
    [SerializeField] private int maxHP = 5;
    private NetworkVariable<int> currentHP = new NetworkVariable<int>();

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

        SendDamageClientRpc(enemyNetworkObjectId);
    }

    [Rpc(SendTo.Everyone)]
    private void SendDamageClientRpc(ulong enemyNetworkObjectId)
    {
        Debug.Log($"Enemy ({enemyNetworkObjectId}) took damage!");
        // Could trigger hit flash, particles, etc.
    }

    private bool ApplyDamage()
    {
        if (!IsServer) return false;

        currentHP.Value -= 1;

        if (currentHP.Value <= 0)
        {
            NetworkObject.Despawn();
            return true;
        }
        return false;
    }
}
