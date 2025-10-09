using UnityEngine;

public class BossWeakPoint : MonoBehaviour
{
    private BossController bossController;

    private void Awake()
    {
        bossController = GetComponentInParent<BossController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("AttackCollider")
            && other.gameObject.GetComponent<PlayerWeaponTrigger>() != null)
        {
            bossController.WeakpointTakeDamage(other);
        }
    }
}
