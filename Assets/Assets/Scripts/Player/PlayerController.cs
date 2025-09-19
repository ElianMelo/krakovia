using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private PlayerMovementController playerMovementController;
    private PlayerFollower playerFollower;

    public void ReceiveDamage()
    {
        ReceiveDamageRpc(NetworkObjectId);
    }

    [Rpc(SendTo.Server)]
    private void ReceiveDamageRpc(ulong sourceNetworkObjectId)
    {
        SendDamageRpc(sourceNetworkObjectId);
        Debug.Log("Receive Damage?");
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SendDamageRpc(ulong sourceNetworkObjectId)
    {
        if (sourceNetworkObjectId != NetworkObjectId) return;
        Debug.Log("Receive Damage Client?");
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        playerMovementController = GetComponent<PlayerMovementController>();
        playerFollower = FindFirstObjectByType<PlayerFollower>();

        playerFollower.player = transform;
        playerMovementController.SetupFollower(playerFollower.transform);
    }
}
