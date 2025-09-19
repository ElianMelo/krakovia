using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    void Start()
    {
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        PlayerMovementController playerMovementController = GetComponent<PlayerMovementController>();
        PlayerFollower playerFollower = FindFirstObjectByType<PlayerFollower>();

        playerFollower.player = transform;
        playerMovementController.SetupFollower(playerFollower.transform);
    }

    void Update()
    {
    }
}
