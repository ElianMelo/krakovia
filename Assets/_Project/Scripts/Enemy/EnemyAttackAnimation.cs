using UnityEngine;

public class EnemyAttackAnimation : MonoBehaviour
{
    private EnemyAttack enemyAttack;

    private void Start()
    {
        enemyAttack = GetComponentInParent<EnemyAttack>();
    }

    // Used in animation
    public void AnimationPerformAttack()
    {
        enemyAttack.AnimationPerformAttack();
    }
}
