using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private PlayerMovementController playerMovementController;
    private PlayerAttackController playerAttackController;
    private PlayerClassController playerClassController;
    private PlayerFollower playerFollower;
    private ClientNetworkAnimator clientNetworkAnimator;

    private int playerHP = 10;
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
        // Visual logic for all players
        clientNetworkAnimator = GetComponent<ClientNetworkAnimator>();
        playerClassController = GetComponent<PlayerClassController>();
        playerMovementController = GetComponent<PlayerMovementController>();
        playerAttackController = GetComponent<PlayerAttackController>();

        if (OwnerClientId == 0)
        {
            clientNetworkAnimator.Animator = playerClassController.ChangeClassTo(PlayerClass.Fairy);
        }
        else
        {
            clientNetworkAnimator.Animator = playerClassController.ChangeClassTo(PlayerClass.Skeleton);
        }

        playerMovementController.SetupPlayerAnimator(clientNetworkAnimator.Animator);
        playerAttackController.SetupPlayerAnimator(clientNetworkAnimator.Animator);

        // Logic strict to the owner
        if (!IsOwner) return;
               
        playerFollower = FindFirstObjectByType<PlayerFollower>();

        playerFollower.player = transform;
        playerMovementController.SetupFollower(playerFollower.transform);
    }
}
