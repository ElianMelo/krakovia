using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public enum ProjectileType
{
    TreeProjectile,
    WormProjectile,
    IceProjectile,
    WaterProjectile
}

[System.Serializable]
public class PooledProjectileData
{
    public ProjectileType type;
    public GameObject prefab;
    public int initialPoolSize = 10;
}

public class BossProjectilePool : NetworkBehaviour
{
    public List<PooledProjectileData> projectileTypes = new List<PooledProjectileData>();

    public static BossProjectilePool Instance;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        foreach (var type in projectileTypes)
        {
            var handler = new BossProjectilePoolHandler(type.prefab, type.initialPoolSize);
            NetworkManager.Singleton.PrefabHandler.AddHandler(type.prefab, handler);
        }
    }

    public NetworkObject SpawnProjectile(ProjectileType type, Vector3 position, Quaternion rotation)
    {
        var typeData = projectileTypes.Find(t => t.type == type);
        if (typeData == null)
        {
            Debug.LogError($"Projectile type {type} not found!");
            return null;
        }

        var obj = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(
            typeData.prefab.GetComponent<NetworkObject>(),
            // Server owns it
            ownerClientId: 0,
            position: position,
            rotation: rotation);

        obj.GetComponent<BossProjectile>().ResetProjectileState();

        return obj;
    }

    public void DespawnProjectile(NetworkObject projectile)
    {
        if (!projectile.IsSpawned) return;
        projectile.Despawn();
    }
}
