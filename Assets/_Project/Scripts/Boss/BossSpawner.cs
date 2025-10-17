using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class BossSpawner : NetworkBehaviour
{
    public GameObject bossPrefab;
    public float spawnDelay;

    private BossController currentBossController;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        SpawnBoss();
    }

    public void Death()
    {
        Destroy(currentBossController.gameObject);
        StartCoroutine(SpawnNewBoss());
        IEnumerator SpawnNewBoss()
        {
            yield return new WaitForSeconds(spawnDelay);
            SpawnBoss();
        }
    }

    private void SpawnBoss()
    {
        var instance = Instantiate(bossPrefab, transform.position, transform.rotation);
        currentBossController = instance.GetComponent<BossController>();
        currentBossController.SetupBossSpawner(this);
        currentBossController.transform.position = transform.position;
        currentBossController.transform.rotation = transform.rotation;
        currentBossController.GetComponent<NetworkObject>().Spawn();
    }
}
