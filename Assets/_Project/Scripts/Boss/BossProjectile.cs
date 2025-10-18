using Unity.Netcode;
using UnityEngine;

public class BossProjectile : NetworkBehaviour
{
    public BossProjectileData CurrentBossProjectileData;
    public Rigidbody Rigidbody;
    public GameObject OnHitEffect;
    public float hitEffectDestroyTime;
    public float projectileForce = 100f;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return;

        PlayerAttributeController playerAttributeController = collision.gameObject.GetComponent<PlayerAttributeController>();
        if (playerAttributeController != null)
        {
            // Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
            playerAttributeController.ReceiveDamageEnemy((int)CurrentBossProjectileData.damage, false);
            OnHitEffectSpawnParticleRpc();
            BossProjectilePool.Instance.DespawnProjectile(NetworkObject);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void OnHitEffectSpawnParticleRpc()
    {
        GameObject instance = Instantiate(OnHitEffect, transform);
        Destroy(instance, hitEffectDestroyTime);
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
