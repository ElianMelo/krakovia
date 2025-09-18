using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private Animator animator;
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        PlayerMovementController playerMovementController = GetComponent<PlayerMovementController>();
        PlayerFollower playerFollower = FindFirstObjectByType<PlayerFollower>();

        playerFollower.player = transform;
        playerMovementController.SetupFollower(playerFollower.transform);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            animator.SetTrigger("Attack1");
        }
    }
}
