using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger?");
        EnemyController enemyController = other.gameObject.GetComponent<EnemyController>();
        if(enemyController != null)
        {
            enemyController.ReceiveDamage();
        }
    }
}
