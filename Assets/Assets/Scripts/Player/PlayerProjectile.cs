using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerProjectile : NetworkBehaviour
{

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            StartCoroutine(destroySelf());
        }
    }

    private IEnumerator destroySelf()
    {
        yield return new WaitForSeconds(3f);
        NetworkObject.DontDestroyWithOwner = true;
        NetworkObject.Despawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyController enemyController = other.gameObject.GetComponent<EnemyController>();
        if(enemyController != null)
        {
            enemyController.ReceiveDamage();
        }
    }
}
