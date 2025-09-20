using Unity.Netcode;
using UnityEngine;

public class EnemyController : NetworkBehaviour
{
    [SerializeField] private int maxHP = 5;
    private NetworkVariable<int> currentHP = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHP.Value = maxHP;
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
