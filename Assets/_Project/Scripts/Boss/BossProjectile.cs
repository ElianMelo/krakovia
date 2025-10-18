using Unity.Netcode;
using UnityEngine;

public class BossProjectile : NetworkBehaviour
{
    public BossProjectileData CurrentBossProjectileData;
    public Rigidbody Rigidbody;
    public float projectileForce = 100f;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return;

        PlayerAttributeController playerAttributeController = collision.gameObject.GetComponent<PlayerAttributeController>();
        if (playerAttributeController != null)
        {
            // Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
            playerAttributeController.ReceiveDamageEnemy((int)CurrentBossProjectileData.damage, false);
            BossProjectilePool.Instance.DespawnProjectile(NetworkObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }

    public void SetupData(BossProjectileData bossProjectileData)
    {
        CurrentBossProjectileData = bossProjectileData;
    }

    public void ResetProjectileState()
    {
        Rigidbody.AddForce(transform.forward * projectileForce, ForceMode.Impulse);
    }
}
