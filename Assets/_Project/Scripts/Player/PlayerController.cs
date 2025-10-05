using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private PlayerMovementController playerMovementController;
    private PlayerAttackController playerAttackController;
    private PlayerAttributeController playerAttributeController;
    private PlayerClassController playerClassController;
    private PlayerFollower playerFollower;
    private ClientNetworkAnimator clientNetworkAnimator;

    public PlayerAttributeController PlayerAttributeController
    {
        get => playerAttributeController;
        set => playerAttributeController = value;
    }

    public override void OnNetworkSpawn()
    {
        // Visual logic for all players
        clientNetworkAnimator = GetComponent<ClientNetworkAnimator>();
        playerClassController = GetComponent<PlayerClassController>();
        playerMovementController = GetComponent<PlayerMovementController>();
        playerAttackController = GetComponent<PlayerAttackController>();
        playerAttributeController = GetComponent<PlayerAttributeController>();

        transform.position = new Vector3(325.59f, 3.07f, 27.87f);

        // clientNetworkAnimator.Animator = playerClassController.ChangeClassTo(PlayerClass.Horse);

        // Working with classes
        if (IsOwner)
        {
            SwapPlayerTo(InterfaceManager.Instance.GetSelectedClass());
            playerClassController.RequestChangePlayerClassRpc(
                NetworkObjectId, InterfaceManager.Instance.GetSelectedClass());
        }

        playerMovementController.SetupPlayerAnimator(clientNetworkAnimator.Animator);
        playerAttackController.SetupPlayerAnimator(clientNetworkAnimator.Animator);

        if(IsServer)
        {
            playerAttributeController.CurrentHP.Value = (int) playerAttributeController.MaxHP.Value;
        }
        
        playerAttributeController.CurrentHP.OnValueChanged += playerAttributeController.OnHealthValueChanged;

        playerAttributeController.SetupClassLevelUp(PlayerClass.Skeleton);

        // Logic strict to the owner
        if (!IsOwner) return;

        playerAttributeController.SwitchHealthBar(false);

        playerFollower = FindFirstObjectByType<PlayerFollower>();

        playerFollower.player = transform;
        playerMovementController.SetupFollower(playerFollower.transform);
    }

    private void Start()
    {
        playerClassController.DisableColliders();
    }

    // todo: Remove this
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SwapPlayerTo(PlayerClass.Fairy);
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            SwapPlayerTo(PlayerClass.Skeleton);
        }
        if (Input.GetKeyDown(KeyCode.F7))
        {
            SwapPlayerTo(PlayerClass.Horse);
        }
    }

    public void SwapPlayerTo(PlayerClass playerClass)
    {
        clientNetworkAnimator.Animator = playerClassController.ChangeClassTo(playerClass);
        playerMovementController.SetupPlayerAnimator(clientNetworkAnimator.Animator);
        playerAttackController.SetupPlayerAnimator(clientNetworkAnimator.Animator);
    }
}
