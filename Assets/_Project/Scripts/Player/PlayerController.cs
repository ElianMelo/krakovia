using System.Collections;
using TMPro;
using Unity.Collections;
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

    public TMP_Text playerNameText;
    private NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>();

    private SavePoint currentSavePoint;

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

        


        // clientNetworkAnimator.Animator = playerClassController.ChangeClassTo(PlayerClass.Horse);

        // Working with classes
        if (IsOwner)
        {
            playerNameText.enabled = false;
            playerNameText.text = InterfaceManager.Instance.GetPlayerName();
            RequestChangePlayerNameRpc(
                NetworkObjectId, InterfaceManager.Instance.GetPlayerName());

            SwapPlayerTo(InterfaceManager.Instance.GetSelectedClass());
            playerClassController.RequestChangePlayerClassRpc(
                NetworkObjectId, InterfaceManager.Instance.GetSelectedClass());
        }

        if(!IsOwner)
        {
            playerName.OnValueChanged += OnNetworkNameChanged;
            playerNameText.text = playerName.Value.ToString();
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
        StartCoroutine(ForceInitialPosition());
    }

    private IEnumerator ForceInitialPosition()
    {
        var count = 0;
        while(count <= 1)
        {
            yield return new WaitForSeconds(1f);
            transform.position = new Vector3(325.59f, 3.07f, 27.87f);
            count++;
        }
    }

    // todo: Remove this
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentSavePoint != null)
        {
            currentSavePoint.SelectThisSavePoint();
        }
        //if (Input.GetKeyDown(KeyCode.F5))
        //{
        //    SwapPlayerTo(PlayerClass.Fairy);
        //    playerClassController.RequestChangePlayerClassRpc(
        //        NetworkObjectId, PlayerClass.Fairy);
        //}
        //if (Input.GetKeyDown(KeyCode.F6))
        //{
        //    SwapPlayerTo(PlayerClass.Skeleton);
        //    playerClassController.RequestChangePlayerClassRpc(
        //        NetworkObjectId, PlayerClass.Skeleton);
        //}
        //if (Input.GetKeyDown(KeyCode.F7))
        //{
        //    SwapPlayerTo(PlayerClass.Horse);
        //    playerClassController.RequestChangePlayerClassRpc(
        //        NetworkObjectId, PlayerClass.Horse);
        //}
    }

    private void OnNetworkNameChanged(FixedString64Bytes previous, FixedString64Bytes next)
    {
        playerNameText.text = next.ToString();
    }

    [Rpc(SendTo.Server)]
    public void RequestChangePlayerNameRpc(ulong networkObjectIdSearch, string name)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectIdSearch, out var classObj))
        {
            var player = classObj.GetComponent<PlayerController>();
            if (player != null)
            {
                player.playerName.Value = new FixedString64Bytes(name);
            }
        }
    }

    public void SwapPlayerTo(PlayerClass playerClass)
    {
        clientNetworkAnimator.Animator = playerClassController.ChangeClassTo(playerClass);
        playerMovementController.SetupPlayerAnimator(clientNetworkAnimator.Animator);
        playerAttackController.SetupPlayerAnimator(clientNetworkAnimator.Animator);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;
        if (!other.CompareTag("SavePoint")) return;
        currentSavePoint = other.GetComponent<SavePoint>();
        if (currentSavePoint == null) return;
        currentSavePoint.ShowButton();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsOwner) return;
        if (!other.CompareTag("SavePoint")) return;
        currentSavePoint = other.GetComponent<SavePoint>();
        if (currentSavePoint == null) return;
        currentSavePoint.HideButton();
        currentSavePoint = null;
    }
}
