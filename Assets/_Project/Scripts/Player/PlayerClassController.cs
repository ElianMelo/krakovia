using FIMSpace.FProceduralAnimation;
using MoreMountains.Tools;
using Unity.Netcode;
using UnityEngine;

public enum PlayerClass
{
    Fairy,
    Skeleton,
    Horse
}

public class PlayerClassController : NetworkBehaviour
{
    [Header("Fairy")]
    public SkinnedMeshRenderer fairyVisuals;
    public RagdollAnimator2 fairyRagdoll;
    public Animator fairyAnimator;

    [Header("Skeleton")]
    public SkinnedMeshRenderer skeletonVisuals;
    public RagdollAnimator2 skeletonRagdoll;
    public Animator skeletonAnimator;
    public Collider skeletonSword;

    [Header("Horse")]
    public SkinnedMeshRenderer horseVisuals;
    public RagdollAnimator2 horseRagdoll;
    public Animator horseAnimator;
    public Collider horseLeftHand;
    public Collider horseRightHand;

    private PlayerClass _playerClass;
    public PlayerClass ActivePlayerClass => _playerClass;

    private NetworkVariable<PlayerClass> _playerNetworkClass = new NetworkVariable<PlayerClass>();

    private PlayerController playerController;

    public void DisableColliders()
    {
        skeletonSword.enabled = false;
        horseLeftHand.enabled = false;
        horseRightHand.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        playerController = GetComponent<PlayerController>();
        if(!IsOwner)
        {
            playerController.SwapPlayerTo(_playerNetworkClass.Value);
        }
        _playerNetworkClass.OnValueChanged += OnNetworkClassChanged;
    }

    private void OnNetworkClassChanged(PlayerClass previous, PlayerClass next)
    {
        playerController.SwapPlayerTo(next);
    }

    public Animator ChangeClassTo(PlayerClass playerClass)
    {
        DisableAll();
        _playerClass = playerClass;
        switch (playerClass)
        {
            case PlayerClass.Fairy: SwitchFairy(true); return fairyAnimator;
            case PlayerClass.Skeleton: SwitchSkeleton(true); return skeletonAnimator;
            case PlayerClass.Horse: SwitchHorse(true); return horseAnimator;
            default: return fairyAnimator;
        }
    }

    [Rpc(SendTo.Server)]
    public void RequestChangePlayerClassRpc(ulong networkObjectIdSearch, PlayerClass playerClass)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectIdSearch, out var classObj))
        {
            var player = classObj.GetComponent<PlayerClassController>();
            if (player != null)
            {
                player._playerNetworkClass.Value = playerClass;
            }
        }
    }

    private void DisableAll()
    {
        SwitchFairy(false);
        SwitchSkeleton(false);
        SwitchHorse(false);
    }

    private void SwitchFairy(bool target)
    {
        fairyAnimator.enabled = target;
        fairyRagdoll.enabled = target;
        fairyVisuals.enabled = target;
    }

    private void SwitchSkeleton(bool target)
    {
        skeletonAnimator.enabled = target;
        skeletonRagdoll.enabled = target;
        skeletonVisuals.enabled = target;
        skeletonSword.enabled = target;
    }

    private void SwitchHorse(bool target)
    {
        horseAnimator.enabled = target;
        horseRagdoll.enabled = target;
        horseVisuals.enabled = target;
        horseLeftHand.enabled = target;
        horseRightHand.enabled = target;
    }
}
