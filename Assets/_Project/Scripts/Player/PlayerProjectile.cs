using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerProjectile : NetworkBehaviour
{
    public float playbackTime;
    public Collider projectileCollider;

    private float damage;
    private bool isCritical;
    private ulong playerSource;
    private Transform playerSourceTransform;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(playbackTimeToActivate());
            StartCoroutine(destroySelf());
        }
    }

    public void SetupProjectile(float damage, bool isCritical, ulong playerSource, Transform playerSourceTransform)
    {
        this.damage = damage;
        this.isCritical = isCritical;
        this.playerSource = playerSource;
        this.playerSourceTransform = playerSourceTransform;
    }

    private IEnumerator playbackTimeToActivate()
    {
        yield return new WaitForSeconds(playbackTime);
        projectileCollider.enabled = true;
    }

    private IEnumerator destroySelf()
    {
        yield return new WaitForSeconds(3f);
        NetworkObject.DontDestroyWithOwner = true;
        NetworkObject.Despawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;
        EnemyController enemyController = other.gameObject.GetComponent<EnemyController>();
        if(enemyController != null)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), other);
            var enemyMovement = enemyController.GetComponent<EnemyMovement>();
            var enemyAttack = enemyController.GetComponent<EnemyAttack>();
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            enemyMovement.OnAttacked(playerSourceTransform);
            enemyAttack.StartAttackRoutine();
            enemyController.ReceiveDamage(playerSource, contactPoint, damage, isCritical);
        }

        BossWeakPoint bossWeakPoint = other.gameObject.GetComponent<BossWeakPoint>();
        if (bossWeakPoint != null)
        {
            BossController bossController = other.gameObject.GetComponentInParent<BossController>();
            Physics.IgnoreCollision(GetComponent<Collider>(), other);
            //var enemyAttack = bossController.GetComponent<EnemyAttack>();
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            //enemyMovement.OnAttacked(playerSourceTransform);
            //enemyAttack.StartAttackRoutine();
            bossController.ReceiveDamage(playerSource, contactPoint, damage, isCritical);
        }

        // Source dont take damage
        if (playerSourceTransform == other.transform) return;

        PlayerAttributeController playerAttributeController = other.gameObject.GetComponent<PlayerAttributeController>();
        if (playerAttributeController != null)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), other);
            playerAttributeController.ReceivePlayerDamageFairy((int)damage, isCritical);
        }
    }
}
