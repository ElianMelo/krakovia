using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[System.Serializable]
public class EnemySpawnData
{
    public EnemyType type;
    [Range(0f, 1f)]
    public float spawnChance = 1f;
}

public class EnemySpawnerZone : NetworkBehaviour
{
    public float spawnRadius = 20f;
    public int maxEnemies = 10;
    public float spawnInterval = 5f;
    public List<EnemySpawnData> enemyTypes = new List<EnemySpawnData>();
    public EnemyPool globalPool;

    private List<(NetworkObject enemy, EnemyType type)> spawnedEnemies = new List<(NetworkObject, EnemyType)>();
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

        spawnedEnemies.RemoveAll(e => e.enemy == null || !e.enemy.gameObject.activeSelf);
    }

    private void SpawnEnemy()
    {
        if (enemyTypes.Count == 0 || globalPool == null) return;

        EnemySpawnData selectedType = ChooseEnemyType();
        if (selectedType == null) return;

        Vector3 spawnPos = transform.position + Random.insideUnitSphere * spawnRadius;
        spawnPos.y = TerrainHeight(spawnPos);

        NetworkObject enemyInstance = globalPool.GetFromPool(selectedType.type);
        if (enemyInstance == null) return;

        enemyInstance.transform.position = spawnPos;
        enemyInstance.transform.rotation = Quaternion.identity;
        enemyInstance.Spawn(true);

        spawnedEnemies.Add((enemyInstance, selectedType.type));
    }

    private EnemySpawnData ChooseEnemyType()
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
                return enemy;
        }

        return null;
    }

    public void DespawnEnemy(NetworkObject enemy)
    {
        var entry = spawnedEnemies.Find(e => e.enemy == enemy);
        if (entry.enemy == null) return;

        enemy.Despawn(true);
        globalPool.ReturnToPool(enemy, entry.type);
        spawnedEnemies.Remove(entry);
    }

    private float TerrainHeight(Vector3 pos)
    {
        if (Terrain.activeTerrain != null)
            pos.y = Terrain.activeTerrain.SampleHeight(pos);
        return pos.y;
    }

    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, spawnRadius);
    //}
}
