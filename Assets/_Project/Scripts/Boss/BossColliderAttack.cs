using Unity.Netcode;
using UnityEngine;

public class BossColliderAttack : NetworkBehaviour
{
    private float damage = 20;

    public void SetupDamage(float damage)
    {
        this.damage = damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        PlayerAttributeController playerAttributeController = other.gameObject.GetComponent<PlayerAttributeController>();
        if (playerAttributeController != null)
        {            
            Physics.IgnoreCollision(GetComponent<Collider>(), other);
            playerAttributeController.ReceiveDamageEnemy((int) damage, false);
        }
    }
}
