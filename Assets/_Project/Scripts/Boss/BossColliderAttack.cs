using Unity.Netcode;
using UnityEngine;

public class BossColliderAttack : NetworkBehaviour
{
    private float damage = 2000;

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
            playerAttributeController.ReceiveDamageEnemy((int) damage, false);
        }
    }
}
