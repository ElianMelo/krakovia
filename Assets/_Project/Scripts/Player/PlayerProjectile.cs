using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerProjectile : NetworkBehaviour
{
    public float playbackTime;
    public Collider projectileCollider;

    private float damage;
    private bool isCritical;
    private ulong playerSource;

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            StartCoroutine(playbackTimeToActivate());
            StartCoroutine(destroySelf());
        }
    }

    public void SetupProjectile(float damage, bool isCritical, ulong playerSource)
    {
        this.damage = damage;
        this.isCritical = isCritical;
        this.playerSource = playerSource;
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
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            enemyController.ReceiveDamage(playerSource, contactPoint, damage, isCritical);
        }
    }
}
