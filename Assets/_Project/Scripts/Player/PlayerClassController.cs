using FIMSpace.FProceduralAnimation;
using UnityEngine;

public enum PlayerClass
{
    Fairy,
    Skeleton,
    Horse
}

public class PlayerClassController : MonoBehaviour
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

    private PlayerClass _playerClass;
    public PlayerClass PlayerClass => _playerClass;

    public void DisableColliders()
    {
        skeletonSword.enabled = false;
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
    }
}
