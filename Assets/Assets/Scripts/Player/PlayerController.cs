using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private PlayerMovementController playerMovementController;
    private PlayerFollower playerFollower;

    private int playerHP;
    private int maxHP = 10;

    public void ReceiveDamage()
    {
        ReceiveDamageRpc(OwnerClientId);
    }

    [Rpc(SendTo.Server)]
    private void ReceiveDamageRpc(ulong targetOwnerClientId)
    {
        var rpcParams = new RpcParams
        {
            Send = new RpcSendParams
            {
                Target = NetworkManager.Singleton.RpcTarget.Single(targetOwnerClientId, RpcTargetUse.Persistent)
            }
        };
        SendDamageClientRpc(rpcParams);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SendDamageClientRpc(RpcParams rpcParams = default)
    {
        Debug.Log("Damage Received!");
        playerHP -= 1;
        InterfaceManager.Instance.UpdatePlayerHP(playerHP, maxHP);
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
