using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;
    [Range(0f, 1f)]
    public float spawnChance = 1f;
}

public class EnemySpawnerZone : NetworkBehaviour
{
    public float spawnRadius = 20f;
    public int maxEnemies = 10;
    public float spawnInterval = 5f;
    public List<EnemySpawnData> enemyTypes = new List<EnemySpawnData>();

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private float timer;

    void Update()
    {
        if (!IsServer) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval && spawnedEnemies.Count < maxEnemies)
        {
            timer = 0f;
            SpawnEnemy();
        }

        spawnedEnemies.RemoveAll(e => e == null);
    }

    private void SpawnEnemy()
    {
        if (enemyTypes.Count == 0) return;

        GameObject prefabToSpawn = ChooseEnemyType();
        if (prefabToSpawn == null) return;

        Vector3 spawnPos = transform.position + Random.insideUnitSphere * spawnRadius;
        spawnPos.y = TerrainHeight(spawnPos);

        GameObject enemyInstance = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        enemyInstance.GetComponent<NetworkObject>().Spawn(true);

        spawnedEnemies.Add(enemyInstance);
    }

    private GameObject ChooseEnemyType()
    {
        float totalWeight = 0f;
        foreach (var enemy in enemyTypes)
            totalWeight += enemy.spawnChance;

        float roll = Random.value * totalWeight;
        float cumulative = 0f;

        foreach (var enemy in enemyTypes)
        {
            cumulative += enemy.spawnChance;
            if (roll <= cumulative)
                return enemy.enemyPrefab;
        }

        return null;
    }

    private float TerrainHeight(Vector3 pos)
    {
        if (Terrain.activeTerrain != null)
        {
            pos.y = Terrain.activeTerrain.SampleHeight(pos);
        }
        return pos.y;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
