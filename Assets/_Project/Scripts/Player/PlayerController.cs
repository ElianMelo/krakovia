using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    private PlayerMovementController playerMovementController;
    private PlayerAttackController playerAttackController;
    private PlayerAttributeController playerAttributeController;
    private PlayerClassController playerClassController;
    private PlayerFollower playerFollower;
    private ClientNetworkAnimator clientNetworkAnimator;

    public TMP_Text playerNameText;
    public Image playerIsPvP;

    private NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>();
    public NetworkVariable<bool> isPvPActive = new NetworkVariable<bool>();

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

        isPvPActive.OnValueChanged += OnPvPActiveChanged;

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

        playerAttributeController.SetupClassLevelUp(InterfaceManager.Instance.GetSelectedClass());

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
            // Dog Position
            // transform.position = new Vector3(325.59f, 3.07f, 27.87f);
            transform.position = new Vector3(7.2f, -0.24f, 68.9f);
            count++;
        }
    }

    // todo: Remove this
    private void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.E) && currentSavePoint != null)
        {
            currentSavePoint.SelectThisSavePoint();
        }
        if(Input.GetKeyDown(KeyCode.P) && isPvPActive.Value == false)
        {
            RequestEnablePvPRpc(NetworkObjectId);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            RequestExperienceRpc(playerAttributeController.NetworkObjectId);
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

    private void OnPvPActiveChanged(bool previous, bool currentValue)
    {
        if(IsOwner)
        {
            playerIsPvP.enabled = false;
            if (currentValue)
            {
                InterfaceManager.Instance.PvPInterfaceController.EnablePvPInterface();
            }
            else
            {
                InterfaceManager.Instance.PvPInterfaceController.DisablePvPInterface();
            }
        } else
        {
            playerIsPvP.enabled = currentValue;
        }
        
    }

    private void OnNetworkNameChanged(FixedString64Bytes previous, FixedString64Bytes next)
    {
        playerNameText.text = next.ToString();
    }

    [Rpc(SendTo.Server)]
    public void RequestEnablePvPRpc(ulong networkObjectIdSearch)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectIdSearch, out var classObj))
        {
            var player = classObj.GetComponent<PlayerController>();
            if (player != null)
            {
                player.isPvPActive.Value = true;
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void RequestExperienceRpc(ulong networkObjectIdSearch)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectIdSearch, out var classObj))
        {
            var player = classObj.GetComponent<PlayerAttributeController>();
            if (player != null)
            {
                player.ReceiveExp(player.OwnerClientId, 1000);
            }
        }
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
