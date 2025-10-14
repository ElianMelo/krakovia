using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public enum EnemyType
{
    Dog,
    Mummy,
    Yeti,
    IceDoggo,
    EnemyA,
    EnemyB,
    EnemyC,
}

[System.Serializable]
public class PooledEnemyData
{
    public EnemyType type;
    public GameObject prefab;
    public int initialPoolSize = 10;
}

public class EnemyPool : NetworkBehaviour
{
    public List<PooledEnemyData> enemyTypes = new List<PooledEnemyData>();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        foreach (var type in enemyTypes)
        {
            var handler = new EnemyPoolHandler(type.prefab, type.initialPoolSize);
            NetworkManager.Singleton.PrefabHandler.AddHandler(type.prefab, handler);
        }
    }

    public NetworkObject SpawnEnemy(EnemyType type, Vector3 position, Quaternion rotation)
    {
        var typeData = enemyTypes.Find(t => t.type == type);
        if (typeData == null)
        {
            Debug.LogError($"Enemy type {type} not found!");
            return null;
        }

        var obj = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(
            typeData.prefab.GetComponent<NetworkObject>(),
            // Server owns it
            ownerClientId: 0,
            position: position,
            rotation: rotation);

        obj.GetComponent<EnemyController>().ResetEnemyState();

        return obj;
    }

    public void DespawnEnemy(NetworkObject enemy)
    {
        enemy.Despawn();
    }
}
