using Unity.Netcode;
using UnityEngine;

public class BossProjectile : NetworkBehaviour
{
    public BossProjectileData CurrentBossProjectileData;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        PlayerAttributeController playerAttributeController = other.gameObject.GetComponent<PlayerAttributeController>();
        if (playerAttributeController != null)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), other);
            playerAttributeController.ReceiveDamageEnemy((int)CurrentBossProjectileData.damage, false);
            BossProjectilePool.Instance.DespawnProjectile(NetworkObject);
        }
    }

    public void SetupData(BossProjectileData bossProjectileData)
    {
        CurrentBossProjectileData = bossProjectileData;
    }

    public void ResetProjectileState()
    {

    }
}
