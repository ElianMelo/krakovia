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
            ReceiveDamage();
        }
    }

    public void ReceiveDamage()
    {
        ReceiveDamageRpc(NetworkObjectId);
    }

    [Rpc(SendTo.Server)]
    private void ReceiveDamageRpc(ulong enemyNetworkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(enemyNetworkObjectId, out var enemyObj))
        {
            var enemy = enemyObj.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.ApplyDamage();
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

    private void ApplyDamage()
    {
        if (!IsServer) return;

        currentHP.Value -= 1;

        if (currentHP.Value <= 0)
        {
            NetworkObject.Despawn();
        }
    }
}
