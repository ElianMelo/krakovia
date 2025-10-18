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
    [Header("Fairy Skin")]
    public Material fairySkinA;
    public Material fairySkinB;
    public Material fairySkinC;
    public Material fairySkinD;

    [Header("Skeleton")]
    public SkinnedMeshRenderer skeletonVisuals;
    public RagdollAnimator2 skeletonRagdoll;
    public Animator skeletonAnimator;
    public Collider skeletonSword;
    [Header("Skeleton Skin")]
    public Material skeletonSkinA;
    public Material skeletonSkinB;
    public Material skeletonSkinC;
    public Material skeletonSkinD;

    [Header("Horse")]
    public SkinnedMeshRenderer horseVisuals;
    public RagdollAnimator2 horseRagdoll;
    public Animator horseAnimator;
    public Collider horseLeftHand;
    public Collider horseRightHand;
    [Header("Horse Skin")]
    public Material horseSkinA;
    public Material horseSkinB;
    public Material horseSkinC;
    public Material horseSkinD;

    private PlayerClass _playerClass;
    public PlayerClass ActivePlayerClass => _playerClass;

    private NetworkVariable<PlayerClass> _playerNetworkClass = new NetworkVariable<PlayerClass>();

    private PlayerController playerController;

    public NetworkVariable<PlayerClassSkin> _playerNetworkClassSkin = new NetworkVariable<PlayerClassSkin>();

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
            playerController.SwapClassSkinPlayerTo(_playerNetworkClassSkin.Value);
            playerController.SwapPlayerTo(_playerNetworkClass.Value);
        }
        _playerNetworkClass.OnValueChanged += OnNetworkClassChanged;
        _playerNetworkClassSkin.OnValueChanged += OnNetworkClassSkinChanged;
    }

    private void OnNetworkClassSkinChanged(PlayerClassSkin previous, PlayerClassSkin next)
    {
        playerController.SwapClassSkinPlayerTo(next);
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

    [Rpc(SendTo.Server)]
    public void RequestChangePlayerClassSkinRpc(ulong networkObjectIdSearch, PlayerClassSkin playerClassSkin)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectIdSearch, out var classObj))
        {
            var player = classObj.GetComponent<PlayerClassController>();
            if (player != null)
            {
                player._playerNetworkClassSkin.Value = playerClassSkin;
            }
        }
    }

    private void DisableAll()
    {
        SwitchFairy(false);
        SwitchSkeleton(false);
        SwitchHorse(false);
    }

    public RagdollAnimator2 GetRagdollByClass()
    {
        switch (_playerNetworkClass.Value)
        {   
            case PlayerClass.Fairy: return fairyRagdoll;
            case PlayerClass.Skeleton: return skeletonRagdoll;
            case PlayerClass.Horse: return horseRagdoll;
            default: return skeletonRagdoll;
        }
    }

    private void SwitchFairy(bool target)
    {
        fairyAnimator.enabled = target;
        fairyRagdoll.enabled = target;
        fairyVisuals.enabled = target;
        fairyVisuals.material = GetMaterialBasedOnPlayerSkin(_playerNetworkClass.Value, _playerNetworkClassSkin.Value);
    }

    private void SwitchSkeleton(bool target)
    {
        skeletonAnimator.enabled = target;
        skeletonRagdoll.enabled = target;
        skeletonVisuals.enabled = target;
        skeletonSword.enabled = target;
        skeletonVisuals.material = GetMaterialBasedOnPlayerSkin(_playerNetworkClass.Value, _playerNetworkClassSkin.Value);
    }

    private void SwitchHorse(bool target)
    {
        horseAnimator.enabled = target;
        horseRagdoll.enabled = target;
        horseVisuals.enabled = target;
        horseLeftHand.enabled = target;
        horseRightHand.enabled = target;
        horseVisuals.material = GetMaterialBasedOnPlayerSkin(_playerNetworkClass.Value, _playerNetworkClassSkin.Value);
    }

    public void SwitchMaterial(PlayerClassSkin playerClassSkin)
    {
        switch (_playerNetworkClass.Value)
        {
            case PlayerClass.Fairy:
                fairyVisuals.material = GetMaterialBasedOnPlayerSkin(_playerNetworkClass.Value, playerClassSkin);
                break;
            case PlayerClass.Skeleton:
                skeletonVisuals.material = GetMaterialBasedOnPlayerSkin(_playerNetworkClass.Value, playerClassSkin);
                break;
            case PlayerClass.Horse:
                horseVisuals.material = GetMaterialBasedOnPlayerSkin(_playerNetworkClass.Value, playerClassSkin);
                break;
            default:
                break;
        }
    }

    private Material GetMaterialBasedOnPlayerSkin(PlayerClass playerClass, PlayerClassSkin playerClassSkin)
    {
        switch (playerClass)
        {
            case PlayerClass.Fairy:
                switch (playerClassSkin)
                {
                    case PlayerClassSkin.VariationA: return fairySkinA;
                    case PlayerClassSkin.VariationB: return fairySkinB;
                    case PlayerClassSkin.VariationC: return fairySkinC;
                    case PlayerClassSkin.VariationD: return fairySkinD;
                    default: return fairySkinA;
                }
            case PlayerClass.Skeleton:
                switch (playerClassSkin)
                {
                    case PlayerClassSkin.VariationA: return skeletonSkinA;
                    case PlayerClassSkin.VariationB: return skeletonSkinB;
                    case PlayerClassSkin.VariationC: return skeletonSkinC;
                    case PlayerClassSkin.VariationD: return skeletonSkinD;
                    default: return skeletonSkinA;
                }
            case PlayerClass.Horse:
                switch (playerClassSkin)
                {
                    case PlayerClassSkin.VariationA: return horseSkinA;
                    case PlayerClassSkin.VariationB: return horseSkinB;
                    case PlayerClassSkin.VariationC: return horseSkinC;
                    case PlayerClassSkin.VariationD: return horseSkinD;
                    default: return horseSkinA;
                } 
            default:
                return fairySkinA;
        }
    }
}
