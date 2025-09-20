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

    //[Header("Horse")]
    //public GameObject horseVisuals;
    //public RagdollAnimator2 horseRagdoll;
    //public Animator horseAnimator;
    
    public Animator ChangeClassTo(PlayerClass playerClass)
    {
        DisableAll();
        switch (playerClass)
        {
            case PlayerClass.Fairy: SwitchFairy(true); return fairyAnimator;
            case PlayerClass.Skeleton: SwitchSkeleton(true); return skeletonAnimator;
            case PlayerClass.Horse: return fairyAnimator;
            default: return fairyAnimator;
        }
    }

    private void DisableAll()
    {
        SwitchFairy(false);
        SwitchSkeleton(false);
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
    }
}
