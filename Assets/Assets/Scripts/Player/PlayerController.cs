using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private PlayerMovementController playerMovementController;
    private PlayerAttackController playerAttackController;
    private PlayerClassController playerClassController;
    private PlayerFollower playerFollower;
    private ClientNetworkAnimator clientNetworkAnimator;

    public override void OnNetworkSpawn()
    {
        // Visual logic for all players
        clientNetworkAnimator = GetComponent<ClientNetworkAnimator>();
        playerClassController = GetComponent<PlayerClassController>();
        playerMovementController = GetComponent<PlayerMovementController>();
        playerAttackController = GetComponent<PlayerAttackController>();

        if (OwnerClientId == 1)
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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F5))
        {
            SwapPlayerTo(PlayerClass.Fairy);
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            SwapPlayerTo(PlayerClass.Skeleton);
        }
    }

    private void SwapPlayerTo(PlayerClass playerClass)
    {
        clientNetworkAnimator.Animator = playerClassController.ChangeClassTo(playerClass);
        playerMovementController.SetupPlayerAnimator(clientNetworkAnimator.Animator);
        playerAttackController.SetupPlayerAnimator(clientNetworkAnimator.Animator);
    }
}
